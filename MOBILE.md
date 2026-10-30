# TMS mobile and tablet companion

.NET MAUI 10 application targeting Android phones/tablets and iPhone/iPad. It uses a native C# shell with a WebView that opens the existing responsive MVC application. The same server handles sign-in, booking validation, persistence, dashboard and route map. A running TMS web server and network connection are required; this is not an offline booking app.

## Open the project
Open TMS.Mobile.slnx from the repository root in Visual Studio with the .NET MAUI workload installed.
TMS.slnx remains the web-only solution so normal web builds do not require mobile SDKs.

## Android demo APK
artifacts/android/TMS-demo.apk is a standalone Debug APK signed with the development key.
Install it on an Android 7.0+ phone or tablet for internal demo use. This package is not a store release.

## Connect to your workspace
1. Start the MVC web project or deploy it to an HTTPS host.
2. Open TMS on your device and enter the root server URL.
3. Tap Connect to workspace.
4. Sign in using user / P@ssword or korakod.p / P@ssw0rd.

The URL setting is remembered. Passwords stay in the web login form and are not saved by the native app.
Both accounts share the same demo bookings. Use the web Sign out button to end the session.

The native toolbar provides Back, Home, Reload, Server and Light/Dark controls. Web theme changes are reflected in the native shell. The interface supports portrait and landscape layouts.

## Local development
Run the web app from the repository root:

    dotnet run --project TMS --launch-profile http

For Android emulator, enter http://10.0.2.2:5014.
For a USB-connected Android device, run:

    adb reverse tcp:5014 tcp:5014

Then enter http://127.0.0.1:5014 in the Debug app. HTTP is restricted to emulator/loopback development addresses; hosted/shared demos should use HTTPS. The server must remain running.

## Build Android
    dotnet restore TMS.Mobile/TMS.Mobile.csproj -p:TargetFrameworks=net10.0-android
    dotnet build TMS.Mobile/TMS.Mobile.csproj -f net10.0-android -p:TargetFrameworks=net10.0-android -p:EmbedAssembliesIntoApk=true --no-restore

The signed Debug APK is generated under TMS.Mobile/bin/Debug/net10.0-android.
Copy the new Signed.apk to artifacts/android/TMS-demo.apk after rebuilding.

## iPhone / iPad
The iOS target and device-family configuration are included.
Building, signing and installing iOS requires a Mac with a compatible Xcode installation and Apple signing credentials. Use an HTTPS TMS URL. iOS compilation and device execution were not verified on this Windows computer.

## Verification limits
Android C#/XAML compilation and standalone APK packaging succeeded.
No device or emulator was connected during verification, so Android installation, device WebView behavior and native navigation/theme synchronization still require a device test.
