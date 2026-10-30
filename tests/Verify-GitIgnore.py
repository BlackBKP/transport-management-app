"""Verify ignore rules in a disposable Git repository, without initializing the project."""
from pathlib import Path
import shutil
import subprocess
import tempfile

root = Path(__file__).resolve().parents[1]
ignored = [
    "TMS/bin/Debug/app.dll", "TMS/obj/project.assets.json", ".vs/TMS/index.db",
    "TMS/TMS.csproj.user", "TMS/App_Data/bookings.json", "TMS/App_Data/keys/key.xml",
    "TMS.Mobile/demo.keystore", "TMS/Properties/PublishProfiles/Demo.pubxml",
    ".env", ".env.production", ".vercel/project.json", ".aws/credentials",
    "artifacts/android/TMS-demo.apk", "demo.aab", "demo.ipa", "certificate.pfx",
    "showcase/dependencies/pkg/file.py", "showcase/screenshots/demo.png",
    "showcase/TMS-showcase.mp4", "showcase/encode.log", "node_modules/pkg/index.js",
    ".test-runs/example/App_Data/bookings.json", ".verification/login.html",
]
included = [
    ".env.example", ".env.production.example", ".editorconfig", "Dockerfile",
    ".dockerignore", "DEPLOYMENT.md", "TMS.slnx", "TMS/TMS.csproj", "TMS/Program.cs",
    "TMS/appsettings.json", "TMS/appsettings.Development.json",
    "TMS/Properties/launchSettings.json", "TMS/wwwroot/favicon.ico",
    "TMS/wwwroot/icons/icon-192.png", "TMS/wwwroot/site.webmanifest",
    "TMS.Mobile/Resources/AppIcon/appicon.svg",
    "TMS.Mobile/Resources/Fonts/OpenSans-Regular.ttf",
    "tests/Verify-Web.ps1", "showcase/capture-screenshots.cjs", "showcase/scenes.json",
]

# The temporary repository is always created and removed within this workspace.
with tempfile.TemporaryDirectory(prefix=".ignore-check-", dir=root) as directory:
    subprocess.run(["git", "init", "--quiet", "--template=", directory], check=True)
    shutil.copyfile(root / ".gitignore", Path(directory) / ".gitignore")
    result = subprocess.run(
        ["git", "-C", directory, "check-ignore", "--no-index", "--stdin", "-z"],
        input=("\0".join(ignored + included) + "\0").encode("utf-8"),
        capture_output=True, check=False,
    )
    if result.returncode not in (0, 1):
        raise RuntimeError(result.stderr)
    actual = set(result.stdout.decode("utf-8").rstrip("\0").split("\0"))
    assert actual == set(ignored), {
        "unexpectedly_included": sorted(set(ignored) - actual),
        "unexpectedly_ignored": sorted(actual - set(ignored)),
    }
    print(f"PASS: {len(ignored)} ignored paths; {len(included)} source/assets remain included.")
