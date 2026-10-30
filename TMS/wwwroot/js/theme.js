(function initializeTheme() {
    const storageKey = "tms-theme";
    const systemPreference = window.matchMedia("(prefers-color-scheme: dark)");
    let savedTheme = readSavedTheme();

    function isValidTheme(theme) {
        return theme === "light" || theme === "dark";
    }

    function readSavedTheme() {
        try {
            const theme = localStorage.getItem(storageKey);
            return isValidTheme(theme) ? theme : null;
        } catch {
            // Theme switching still works when browser storage is unavailable.
            return null;
        }
    }

    function applyTheme() {
        const systemTheme = systemPreference.matches ? "dark" : "light";
        document.documentElement.dataset.theme = savedTheme || systemTheme;
    }

    // Apply before the stylesheet loads to avoid flashing the opposite theme.
    applyTheme();

    document.addEventListener("DOMContentLoaded", function bindThemeButton() {
        const themeButton = document.getElementById("theme-toggle");

        if (!themeButton) {
            return;
        }

        function updateThemeButton() {
            const darkThemeActive = document.documentElement.dataset.theme === "dark";

            themeButton.textContent = darkThemeActive ? "☀ Light" : "☾ Dark";
            themeButton.setAttribute(
                "aria-label",
                darkThemeActive ? "Switch to light theme" : "Switch to dark theme"
            );
            themeButton.setAttribute("aria-pressed", String(darkThemeActive));
        }

        themeButton.addEventListener("click", function toggleTheme() {
            savedTheme = document.documentElement.dataset.theme === "dark" ? "light" : "dark";
            applyTheme();

            try {
                localStorage.setItem(storageKey, savedTheme);
            } catch {
                // Keep the selected theme for this page without persistence.
            }

            updateThemeButton();
        });

        systemPreference.addEventListener("change", function updateSystemTheme() {
            if (!savedTheme) {
                applyTheme();
                updateThemeButton();
            }
        });

        window.addEventListener("storage", function synchronizeTheme(event) {
            if (event.key !== storageKey) {
                return;
            }

            savedTheme = isValidTheme(event.newValue) ? event.newValue : null;
            applyTheme();
            updateThemeButton();
        });

        updateThemeButton();
    });
})();
