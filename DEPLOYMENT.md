# GitHub and Vercel demo deployment

## What belongs in GitHub
Commit the C# projects, Razor views, static web assets, mobile source, tests, documentation, Dockerfile and .dockerignore.
The reviewed .gitignore excludes build/IDE output, local bookings and Data Protection keys, environment files, certificates and signing keys, APKs, dependency caches, Vercel's local project link, logs and generated showcase media.
Capture/export scripts and scenes.json remain eligible for source control.

The Git origin is https://github.com/BlackBKP/transport-management-app.git. Review git status before each commit.
.gitignore does not remove files that have already been committed or staged in another repository.
The two usernames/passwords in DemoAuthenticationService.cs are deliberately public demo credentials, not production accounts.
Use GitHub Releases to distribute an APK rather than committing generated packages.

## Vercel hosting
Vercel's Container Runtime is currently Beta and documented as available on all plans:
https://vercel.com/docs/functions/runtimes/container

This is an ASP.NET Core MVC application. Deploy the root Dockerfile using the Container preset; publishing only TMS/wwwroot would omit C# controllers, authentication and booking behavior.

1. Push this source tree to a GitHub repository.
2. Import that repository into Vercel.
3. Set Root Directory to the repository root, where Dockerfile lives.
4. Select the Container framework preset if it is not automatically detected.
5. Set the project environment variable PORT to 8080.
6. Keep ASPNETCORE_ENVIRONMENT=Production and Tms__DataDirectory=/tmp/tms as specified in Dockerfile. Add the latter setting in Vercel too if you override container environment settings.
7. Let Vercel build the Dockerfile; do not configure an npm build or static output directory.
8. Deploy, then check /Account/Login, both demo accounts, dashboard, booking/cancellation, theme switching, /favicon.ico and /site.webmanifest.

The container listens on the platform PORT variable with 8080 as its fallback. The .NET runtime runs as its built-in non-root app user.
The image contains only the published web app; mobile artifacts, local bookings, encryption keys and showcase media are excluded from the container context.

## Important storage limit
Vercel Functions provide a read-only filesystem with writable /tmp scratch space:
https://vercel.com/docs/functions/runtimes#file-system-support

ApplicationStorage uses Tms:DataDirectory (environment variable Tms__DataDirectory) for booking JSON and the authentication key directory. Local runs keep their existing App_Data location. The Docker demo uses /tmp/tms.

This makes the container start without writing into the read-only application directory. It does NOT provide durable or shared storage.
Bookings can reset and existing login cookies can stop working when an instance restarts, scales down, redeploys or another instance handles the request.
Separate instances can show different bookings. This mode is suitable only for a resettable prototype walkthrough.
For a reliable multi-user demo, use shared database storage and a shared ASP.NET Data Protection key repository, or host the app with durable storage on an ASP.NET-compatible host.

## Local container check
With Docker installed:

    docker build -t tms-demo .
    docker run --rm -p 8080:8080 -e PORT=8080 tms-demo

Open http://localhost:8080.
Docker is not installed in the current verification environment, so the image build and a real Vercel deployment have not been verified here.
The web Release publish, Git ignore rules, icon assets and existing HTTP regression checks can still be verified locally.

## Icons
The web favicon, PNG shortcut icons and Apple touch icon use the same navy-and-white T mark as the MAUI Android/iOS launcher icon.
Both the login and authenticated layouts include the shared _AppIcons partial.
The web manifest provides shortcut metadata; it does not add offline booking support.
