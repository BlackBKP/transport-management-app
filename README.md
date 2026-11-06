# TMS - Transportation Management System

ASP.NET Core MVC on .NET 10. No database setup is required. Sign in with one of the demo accounts below. The optional street map uses external Leaflet assets and OpenStreetMap tiles.

## Run
    dotnet run --project TMS --launch-profile http

Open the URL printed by the application. The default page is the dashboard.

## Features
- Date, shift and employee filters with live dashboard totals.
- Employee reservations with route, pickup point, vehicle and shift selection.
- Server validation for duplicate employee reservations, capacity, vehicle route conflicts, valid pickup points and dates.
- Booking cancellation releases a seat.
- Fleet, route and shift directories.
- Responsive interface inspired by the supplied dashboard and booking screenshots.

Bookings use seeded fictional data and persist to TMS/App_Data/bookings.json. Stop the app and delete that file to reset the sample reservations. The initial sample date is the day the store is first initialized (Thailand time).

Model properties use PascalCase. JsonPropertyName, ModelBinder and Display annotations use camelCase for serialized names, form input binding and field metadata. Human-readable form labels are defined in Razor views.

This prototype uses cookie authentication with two fixed demo accounts and has no real supplier integration. All visitors share the demo bookings. Fleet, routes and shifts are read-only mock reference data in TMS/Services/DemoTransportData.cs.


## Map and themes
Use Route map in the sidebar or View route map on the dashboard. Select a route to focus its stops; Show all and Fit routes restore the map extent. Geographic coordinates are illustrative and the connections are straight lines, not road directions. The demo network covers Bang Na and On Nut in Bangkok, plus Pak Nam and Bang Phli in Samut Prakan. The sample Main campus in Bang Phli is fictional; coordinates and distances are illustrative.
The interactive street map uses Leaflet 1.9.4 and OpenStreetMap tiles, requiring internet access. If Leaflet cannot load, a local schematic overview appears; the stop list remains available.
The header theme button switches between light and dark. Preference is stored in this browser; first use follows the system preference.

## Code organization
- Controllers: HTTP requests, form validation and navigation.
- Interfaces: service contracts registered through dependency injection.
- Services/TransportService.cs: booking rules and local JSON persistence.
- Services/DemoTransportData.cs: fictional fleet, routes, shifts and seed bookings.
- Models: one model per file; PascalCase properties with camelCase annotations.
- Views: indented Razor markup, shared layout and partials.
- wwwroot/js: separate site, theme and map scripts with named functions.

See CODING_STANDARDS.md and .editorconfig for conventions. Format C# using:

    dotnet format whitespace TMS.slnx --no-restore

## Sign in
The login screen is /Account/Login. All transport screens and booking actions require authentication.

| Username | Password |
| --- | --- |
| user | P@ssw0rd |
| korakod.p | P@ssw0rd |

Passwords are case-sensitive. Usernames are case-insensitive.
The server verifies these fixed demo accounts against stored salted PBKDF2 hashes. No user database or registration service is required.
The login page supports Show/Hide password, Keep me signed in, validation messages and light/dark mode.
The theme preference is shared across login and workspace pages in the same browser.
Use Sign out in the header to end a session.
These fixed accounts are for demo presentations; both share the same booking data.

## Mobile and tablet
See MOBILE.md for the .NET MAUI 10 companion project, server setup and Android/iOS instructions.
A standalone Android demo package is available at artifacts/android/TMS-demo.apk.
Open TMS.Mobile.slnx for both projects or TMS.slnx for web only.

## Repeatable regression check
Run with PowerShell 7:

    pwsh -NoProfile -File tests/Verify-Web.ps1

The script builds the web project, starts an isolated server on an available port, checks authentication and booking behavior, then stops that server and removes its test data. Your existing App_Data is untouched. In a restricted execution environment, starting the test subprocess may require execution outside that sandbox.

## Publishing source and hosting
See DEPLOYMENT.md for the reviewed Git ignore rules, icon assets and Vercel Container Runtime setup.
The container demo uses temporary storage; durable bookings and stable shared login sessions require external shared storage.
