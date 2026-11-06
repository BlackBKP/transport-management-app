# Verification — 30 October 2026

- Web project: .NET 10 build passed, zero warnings and zero errors.
- Android project: .NET 10 MAUI C#/XAML build and standalone Debug APK packaging passed, zero warnings and zero errors.
- Both requested accounts sign in successfully.
- Invalid/missing credentials fail; password is not echoed into HTML.
- Login and cancellation reject missing anti-forgery tokens.
- Anonymous visitors are redirected to login for transport pages and booking actions.
- External return URLs are ignored; local return URLs work.
- Sign-out removes access to protected pages.
- Dashboard, bookings, create, fleet, routes, shifts and map pages respond successfully after login.
- Booking creation, camelCase form binding, required fields, duplicate employees, vehicle route conflicts, pickup validation and past dates checked.
- A 12-seat van rejects the 13th reservation; cancellation releases a seat.
- Local JSON persistence and employee/date/shift filtering checked.
- Live desktop browser login and dark-mode navigation checked.
- Server-rendered login, dashboard, booking and map layouts checked in isolated Chromium at widths 320, 390, 768, 1024 and 1440; no page-level horizontal overflow.
- Map's offline fallback and route list are available when remote assets cannot load.

Automated HTTP regression checks: tests/Verify-Web.ps1.
Test storage is isolated from the application's existing bookings.
Temporary preview servers are stopped after verification.

## Remaining device checks
No Android device/emulator was connected, so installation and native WebView interactions are unverified.
iOS requires Mac/Xcode/signing and was not built or run on this Windows machine.
The MAUI companion requires a running web server; there is no offline booking synchronization.

## GitHub / hosting / icon checks
- Reviewed .gitignore with Git in an isolated repository: 23 generated/private paths excluded and 20 source/asset paths retained.
- Replaced the ASP.NET template favicon with the TMS mark; SVG, ICO, 192/512 PNG and Apple touch icons are included.
- Both layouts include icon links; icon and manifest endpoints work without login.
- Added configurable runtime storage and a root Dockerfile for Vercel's beta Container Runtime.
- Release publish and the full web regression check passed after these changes.
- Docker is not installed here; the container image and Vercel deployment remain unverified.
- Container storage is temporary and does not provide durable/shared bookings or authentication keys.

## Login form recovery — 6 November 2026
- Three fresh live sign-in attempts using korakod.p succeeded before the change; the screenshot failure was not reproducible with a newly generated form.
- The bare 400 is consistent with an expired/mismatched anti-forgery token, potentially caused by temporary per-instance encryption keys.
- Added login-only recovery to a fresh form with a message; rejected credentials are not replayed.
- Verified missing, invalid and cross-session form tokens cannot authenticate, then verified successful login using a fresh token.
- Booking/cancellation anti-forgery enforcement remains unchanged.
- Web build and the full isolated HTTP regression check passed.
- Shared/durable key storage is still required to prevent token invalidation across container restarts or multiple instances.

## Fixed demo password hashes — 6 November 2026
- DemoAuthenticationService now stores salted PBKDF2 hashes instead of generating hashes from seed passwords at each startup.
- No user database, account registration or external authentication service was added.
- Both requested accounts, incorrect passwords and an unknown username are covered by the HTTP regression checks.
- Browser authentication still uses the existing cookie; this password simplification does not change the container key-storage limitations.
