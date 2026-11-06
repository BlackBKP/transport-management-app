param(
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$projectFile = Join-Path $projectRoot 'TMS\TMS.csproj'
$runRoot = Join-Path $projectRoot '.test-runs'
$testDirectory = Join-Path $runRoot ([Guid]::NewGuid().ToString('N'))

if (!$SkipBuild) {
    dotnet build (Join-Path $projectRoot 'TMS.slnx') --no-restore
    if ($LASTEXITCODE -ne 0) {
        throw 'Web build failed.'
    }
}

New-Item -ItemType Directory -Path $testDirectory -Force | Out-Null
$listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 0)
$listener.Start()
$port = $listener.LocalEndpoint.Port
$listener.Stop()
$baseUrl = "http://127.0.0.1:$port"

$startInfo = [Diagnostics.ProcessStartInfo]::new('dotnet')
$startInfo.UseShellExecute = $false
$startInfo.CreateNoWindow = $true
$startInfo.RedirectStandardOutput = $true
$startInfo.RedirectStandardError = $true
$startInfo.WorkingDirectory = $projectRoot
$startInfo.Environment['ASPNETCORE_ENVIRONMENT'] = 'Development'
$startInfo.Environment['Tms__DataDirectory'] = Join-Path $testDirectory 'App_Data'

foreach ($argument in @(
    'run', '--project', $projectFile, '--no-build', '--no-restore', '--no-launch-profile',
    '--urls', $baseUrl, '--contentRoot', $testDirectory
)) {
    $startInfo.ArgumentList.Add($argument)
}

$server = [Diagnostics.Process]::Start($startInfo)
$serverOutput = $server.StandardOutput.ReadToEndAsync()
$serverErrors = $server.StandardError.ReadToEndAsync()

function Assert-Condition([bool]$Condition, [string]$Message) {
    if (!$Condition) {
        throw $Message
    }
}

function Get-CsrfToken([string]$Html) {
    $match = [regex]::Match($Html, 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"')
    Assert-Condition $match.Success 'Missing anti-forgery token.'
    return $match.Groups[1].Value
}

function Get-Response([string]$Path, $Session = $null) {
    return Invoke-WebRequest "$baseUrl$Path" -WebSession $Session -SkipHttpErrorCheck
}

function Send-Form([string]$Path, [hashtable]$Body, $Session) {
    return Invoke-WebRequest "$baseUrl$Path" -Method Post -Body $Body -WebSession $Session -SkipHttpErrorCheck
}

try {
    $ready = $false

    for ($attempt = 0; $attempt -lt 50; $attempt++) {
        try {
            $response = Get-Response '/Account/Login'
            if ($response.StatusCode -eq 200) {
                $ready = $true
                break
            }
        } catch {
            Start-Sleep -Milliseconds 200
        }
    }

    Assert-Condition $ready 'Test server did not start.'

    foreach ($path in @('/', '/Transport/Create', '/Transport/Bookings', '/Transport/Map')) {
        $response = Get-Response $path
        Assert-Condition ($response.Content -match 'id="userName"') "Anonymous access was allowed: $path"
    }

    $anonymousPost = Send-Form '/Transport/Cancel' @{ id = 1 } $null
    Assert-Condition ($anonymousPost.Content -match 'id="userName"') 'Anonymous cancellation was allowed.'
    Write-Output 'PASS: Protected pages and actions require sign-in.'

    foreach ($path in @('/favicon.ico', '/icons/tms.svg', '/icons/icon-192.png', '/icons/icon-512.png', '/icons/apple-touch-icon.png', '/site.webmanifest')) {
        $icon = Get-Response $path
        Assert-Condition ($icon.StatusCode -eq 200) "Anonymous icon asset failed: $path"
        Assert-Condition (($icon.Headers['Content-Type'] -join ';') -notmatch 'text/html') "Icon returned login HTML: $path"
    }

    $manifestContent = (Get-Response '/site.webmanifest').Content
    $manifestJson = if ($manifestContent -is [byte[]]) {
        [Text.Encoding]::UTF8.GetString($manifestContent)
    } else {
        [string]$manifestContent
    }
    $manifest = $manifestJson | ConvertFrom-Json
    Assert-Condition ($manifest.short_name -eq 'TMS' -and $manifest.icons.Count -eq 2) 'App manifest is invalid.'
    Assert-Condition ($response.Content -match '/icons/tms.svg' -and $response.Content -match '/site.webmanifest') 'Login layout is missing shared app icons.'
    Write-Output 'PASS: Anonymous favicon, touch icons, manifest and login icon links.'

    $loginPage = Invoke-WebRequest "$baseUrl/Account/Login" -SessionVariable session
    $token = Get-CsrfToken $loginPage.Content
    $invalidLogin = Send-Form '/Account/Login' @{
        __RequestVerificationToken = $token
        userName = 'user'
        password = 'invalid'
    } $session
    Assert-Condition ($invalidLogin.Content -match 'username or password is incorrect') 'Invalid credentials accepted.'
    Assert-Condition ($invalidLogin.Content -notmatch 'value="invalid"') 'Password echoed into HTML.'

    $unknownUser = Send-Form '/Account/Login' @{
        __RequestVerificationToken = $token
        userName = 'unknown-demo-user'
        password = 'P@ssw0rd'
    } $session
    Assert-Condition ($unknownUser.Content -match 'username or password is incorrect') 'Unknown demo user was accepted.'


    $missingToken = Send-Form '/Account/Login' @{ userName = 'user'; password = 'P@ssword' } $session
    Assert-Condition ($missingToken.StatusCode -eq 200 -and $missingToken.Content -match 'Your sign-in form expired') 'Missing form token did not recover to login.'
    Assert-Condition ($missingToken.Content -match 'id="userName"' -and $missingToken.Content -notmatch '<h1>Dashboard</h1>') 'Rejected login was authenticated.'

    $invalidToken = Send-Form '/Account/Login' @{
        __RequestVerificationToken = 'invalid-token'
        userName = 'korakod.p'
        password = 'P@ssw0rd'
    } $session
    Assert-Condition ($invalidToken.Content -match 'Your sign-in form expired') 'Invalid form token did not recover.'
    Assert-Condition ($invalidToken.Content -notmatch 'value="P@ssw0rd"') 'Rejected password was reflected in the response.'

    $foreignLoginPage = Invoke-WebRequest "$baseUrl/Account/Login" -SessionVariable foreignSession
    $foreignToken = Get-CsrfToken $foreignLoginPage.Content
    $mismatchedToken = Send-Form '/Account/Login' @{
        __RequestVerificationToken = $foreignToken
        userName = 'korakod.p'
        password = 'P@ssw0rd'
    } $session
    Assert-Condition ($mismatchedToken.Content -match 'Your sign-in form expired') 'A token from another session was accepted.'

    # The rejected POST was not replayed. A fresh form must be submitted explicitly.
    $recoveredLogin = Get-Response '/Account/Login' $session
    $token = Get-CsrfToken $recoveredLogin.Content

    $login = Send-Form '/Account/Login' @{
        __RequestVerificationToken = $token
        userName = 'user'
        password = 'P@ssword'
        returnUrl = 'https://example.com'
    } $session
    Assert-Condition ($login.Content -match '<h1>Dashboard</h1>') 'Valid login failed or external redirect followed.'
    Write-Output 'PASS: Valid/invalid login, password handling, anti-forgery and safe return URL.'

    foreach ($path in @(
        '/', '/Transport/Bookings', '/Transport/Create', '/Transport/Vehicles',
        '/Transport/Routes', '/Transport/Shifts', '/Transport/Map', '/Home',
        '/css/site.css', '/css/login.css', '/js/theme.js', '/js/site.js', '/js/login.js', '/js/route-map.js'
    )) {
        $response = Get-Response $path $session
        Assert-Condition ($response.StatusCode -eq 200) "Page or asset failed: $path"
        if ($path -notmatch '^/(css|js)/') {
            Assert-Condition ($response.Content -notmatch 'id="userName"') "Authenticated page returned login: $path"
        }
    }

    $createPage = Get-Response '/Transport/Create' $session
    $token = Get-CsrfToken $createPage.Content
    $date = [DateTime]::UtcNow.AddHours(7).AddDays(30).ToString('yyyy-MM-dd')
    $booking = @{
        __RequestVerificationToken = $token
        employeeId = 'CHECK0001'
        employeeName = 'Verification Employee'
        department = 'Engineering'
        vehicleId = 3
        routeId = 1
        shiftId = 1
        travelDate = $date
        pickupPoint = 'BTS Bang Na'
    }

    $created = Send-Form '/Transport/Create' $booking $session
    Assert-Condition ($created.Content -match 'Booking confirmed') 'Booking creation or camelCase binding failed.'

    $duplicate = Send-Form '/Transport/Create' $booking $session
    Assert-Condition ($duplicate.Content -match 'already has a booking') 'Duplicate reservation accepted.'

    $booking.employeeId = 'CHECK0002'
    $booking.routeId = 2
    $booking.pickupPoint = 'BTS On Nut'
    $conflict = Send-Form '/Transport/Create' $booking $session
    Assert-Condition ($conflict.Content -match 'assigned to another route') 'Vehicle route conflict accepted.'

    $booking.routeId = 1
    $booking.pickupPoint = 'Invalid stop'
    $pickup = Send-Form '/Transport/Create' $booking $session
    Assert-Condition ($pickup.Content -match 'pickup point on the selected route') 'Invalid pickup accepted.'

    $booking.pickupPoint = 'BTS Bang Na'
    $booking.travelDate = [DateTime]::UtcNow.AddHours(7).AddDays(-1).ToString('yyyy-MM-dd')
    $past = Send-Form '/Transport/Create' $booking $session
    Assert-Condition ($past.Content -match 'today or later') 'Past booking accepted.'

    $booking.travelDate = $date
    $booking.employeeName = ''
    $required = Send-Form '/Transport/Create' $booking $session
    Assert-Condition ($required.Content -match 'employeeName field is required') 'Required employee name not checked.'
    $booking.employeeName = 'Verification Employee'

    Write-Output 'PASS: Booking creation, duplicate, route, pickup, date and required-field validation.'

    foreach ($number in 2..12) {
        $booking.employeeId = "CHECK$($number.ToString('0000'))"
        $result = Send-Form '/Transport/Create' $booking $session
        Assert-Condition ($result.Content -match 'Booking confirmed') "Seat $number was not reserved."
    }

    $booking.employeeId = 'CHECK0013'
    $capacity = Send-Form '/Transport/Create' $booking $session
    Assert-Condition ($capacity.Content -match 'fully booked') 'Vehicle capacity exceeded.'

    $bookingFile = Join-Path $testDirectory 'App_Data\bookings.json'
    $savedBookings = Get-Content $bookingFile -Raw | ConvertFrom-Json
    $firstBooking = $savedBookings | Where-Object employeeId -eq 'CHECK0001'
    Assert-Condition ($null -ne $firstBooking) 'camelCase JSON persistence failed.'

    $cancelled = Send-Form '/Transport/Cancel' @{
        __RequestVerificationToken = $token
        id = $firstBooking.id
        date = $date
    } $session
    Assert-Condition ($cancelled.Content -match 'Booking cancelled') 'Cancellation failed.'

    $replacement = Send-Form '/Transport/Create' $booking $session
    Assert-Condition ($replacement.Content -match 'Booking confirmed') 'Cancelled seat was not released.'

    $filtered = Get-Response "/Transport/Bookings?date=$date&search=CHECK0013&shiftId=1" $session
    Assert-Condition ($filtered.Content -match 'CHECK0013' -and $filtered.Content -notmatch 'CHECK0002') 'Employee filtering failed.'

    $unsafeCancel = Send-Form '/Transport/Cancel' @{ id = $firstBooking.id; date = $date } $session
    Assert-Condition ($unsafeCancel.StatusCode -eq 400) 'Cancellation accepted a missing anti-forgery token.'
    Write-Output 'PASS: Capacity, cancellation, seat release, persistence and filtering.'

    $dashboard = Get-Response '/' $session
    $logoutToken = Get-CsrfToken $dashboard.Content
    $signedOut = Send-Form '/Account/Logout' @{ __RequestVerificationToken = $logoutToken } $session
    Assert-Condition ($signedOut.Content -match 'id="userName"') 'Sign-out failed.'
    $protected = Get-Response '/Transport/Bookings' $session
    Assert-Condition ($protected.Content -match 'id="userName"') 'Signed-out session retained access.'

    $secondLoginPage = Get-Response '/Account/Login' $session
    $secondToken = Get-CsrfToken $secondLoginPage.Content
    $secondLogin = Send-Form '/Account/Login' @{
        __RequestVerificationToken = $secondToken
        userName = 'korakod.p'
        password = 'P@ssw0rd'
        returnUrl = '/Transport/Map'
    } $session
    Assert-Condition ($secondLogin.Content -match '<h1>Route map</h1>' -and $secondLogin.Content -match 'korakod.p') 'Second account or local return URL failed.'

    Write-Output 'PASS: Sign-out, access revocation, second account and local return URL.'
    Write-Output 'All web regression checks passed. Existing application data was untouched.'
} catch {
    if ($server.HasExited) {
        Write-Output $serverOutput.GetAwaiter().GetResult()
        Write-Output $serverErrors.GetAwaiter().GetResult()
    }
    throw
} finally {
    if (!$server.HasExited) {
        $server.Kill($true)
        $server.WaitForExit()
    }

    # Delete only this isolated test folder beneath the checked workspace location.
    $resolvedTest = [IO.Path]::GetFullPath($testDirectory)
    $resolvedRoot = [IO.Path]::GetFullPath($runRoot) + [IO.Path]::DirectorySeparatorChar
    if ($resolvedTest.StartsWith($resolvedRoot, [StringComparison]::OrdinalIgnoreCase)) {
        Remove-Item -LiteralPath $resolvedTest -Recurse -Force
    }

    if ((Test-Path $runRoot) -and !(Get-ChildItem -LiteralPath $runRoot -Force)) {
        Remove-Item -LiteralPath $runRoot
    }
}


