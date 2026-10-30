(function initializeLogin() {
    const passwordInput = document.getElementById("password");
    const toggleButton = document.getElementById("password-toggle");

    toggleButton?.addEventListener("click", function togglePasswordVisibility() {
        const showPassword = passwordInput.type === "password";
        passwordInput.type = showPassword ? "text" : "password";
        toggleButton.textContent = showPassword ? "Hide" : "Show";
        toggleButton.setAttribute("aria-label", showPassword ? "Hide password" : "Show password");
        toggleButton.setAttribute("aria-pressed", String(showPassword));
    });
})();
