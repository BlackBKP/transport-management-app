namespace TMS.Mobile
{
    public partial class MainPage : ContentPage
    {
        private const string ServerPreferenceKey = "tms-server-url";
        private Uri? _serverAddress;

        public MainPage()
        {
            InitializeComponent();
            ServerEntry.Text = Preferences.Default.Get(ServerPreferenceKey, string.Empty);
            UpdateThemeButton();
        }

        private void OnConnectClicked(object? sender, EventArgs eventArgs)
        {
            string address = ServerEntry.Text?.Trim() ?? string.Empty;

            if (!Uri.TryCreate(address, UriKind.Absolute, out Uri? serverAddress) ||
                !string.IsNullOrEmpty(serverAddress.UserInfo) ||
                (serverAddress.Scheme != Uri.UriSchemeHttps && !IsLocalDevelopmentAddress(serverAddress)))
            {
                SetupStatus.Text = "Enter a valid HTTPS address. HTTP is allowed only for local development in Debug builds.";
                SetupStatus.IsVisible = true;
                return;
            }

            _serverAddress = new Uri(serverAddress.GetLeftPart(UriPartial.Authority));
            Preferences.Default.Set(ServerPreferenceKey, _serverAddress.AbsoluteUri);
            SetupStatus.IsVisible = false;
            SetupPanel.IsVisible = false;
            BrowserToolbar.IsVisible = true;
            WorkspaceView.IsVisible = true;
            ConnectionError.IsVisible = false;
            WorkspaceView.Source = new Uri(_serverAddress, "/Account/Login").AbsoluteUri;
        }

        private static bool IsLocalDevelopmentAddress(Uri address)
        {
#if DEBUG
            if (address.Scheme == Uri.UriSchemeHttp)
            {
                return address.IsLoopback || address.Host == "10.0.2.2";
            }
#endif
            return false;
        }

        private async void OnWorkspaceNavigating(object? sender, WebNavigatingEventArgs eventArgs)
        {
            // This private URL is sent by the theme observer installed on trusted TMS pages.
            if (eventArgs.Url == "tms-theme://dark" || eventArgs.Url == "tms-theme://light")
            {
                eventArgs.Cancel = true;
                SetNativeTheme(eventArgs.Url.EndsWith("dark", StringComparison.Ordinal));
                return;
            }

            if (!Uri.TryCreate(eventArgs.Url, UriKind.Absolute, out Uri? destination))
            {
                eventArgs.Cancel = true;
                return;
            }

            bool sameServer = _serverAddress != null &&
                destination.Scheme == _serverAddress.Scheme &&
                destination.Host == _serverAddress.Host &&
                destination.Port == _serverAddress.Port;

            if (!sameServer)
            {
                eventArgs.Cancel = true;

                if (destination.Scheme == Uri.UriSchemeHttps)
                {
                    await Launcher.Default.OpenAsync(destination);
                }

                return;
            }

            ConnectionError.IsVisible = false;
            SetLoading(true);
        }

        private async void OnWorkspaceNavigated(object? sender, WebNavigatedEventArgs eventArgs)
        {
            SetLoading(false);
            bool succeeded = eventArgs.Result == WebNavigationResult.Success;

            ConnectionError.IsVisible = !succeeded;
            WorkspaceView.IsVisible = succeeded;

            if (!succeeded)
            {
                return;
            }

            try
            {
                // Keep native chrome and the shared web preference synchronized.
                await ApplyWebThemeAsync();
                await WorkspaceView.EvaluateJavaScriptAsync("""
                    (function () {
                        window.tmsThemeObserver?.disconnect();
                        window.tmsThemeObserver = new MutationObserver(function () {
                            const theme = document.documentElement.dataset.theme;
                            if (theme === "light" || theme === "dark") {
                                window.location.href = "tms-theme://" + theme;
                            }
                        });
                        window.tmsThemeObserver.observe(document.documentElement, {
                            attributes: true,
                            attributeFilter: ["data-theme"]
                        });
                    })();
                    """);
            }
            catch (Exception)
            {
                // A page navigating away can dispose the WebView script context.
            }
        }

        private async void OnThemeClicked(object? sender, EventArgs eventArgs)
        {
            SetNativeTheme(Application.Current?.RequestedTheme != AppTheme.Dark);

            if (WorkspaceView.IsVisible)
            {
                try
                {
                    await ApplyWebThemeAsync();
                }
                catch (Exception)
                {
                    // The next successful navigation will apply the selected theme.
                }
            }
        }

        private Task<string> ApplyWebThemeAsync()
        {
            string theme = Application.Current?.RequestedTheme == AppTheme.Dark ? "dark" : "light";

            return WorkspaceView.EvaluateJavaScriptAsync($$"""
                (function () {
                    if (document.documentElement.dataset.theme !== "{{theme}}") {
                        document.getElementById("theme-toggle")?.click();
                    }
                })();
                """);
        }

        private void SetNativeTheme(bool dark)
        {
            if (Application.Current == null)
            {
                return;
            }

            Application.Current.UserAppTheme = dark ? AppTheme.Dark : AppTheme.Light;
            Preferences.Default.Set("tms-native-theme", dark ? "dark" : "light");
            UpdateThemeButton();
        }

        private void UpdateThemeButton()
        {
            ThemeButton.Text = Application.Current?.RequestedTheme == AppTheme.Dark ? "Light" : "Dark";
        }

        private void SetLoading(bool loading)
        {
            LoadingIndicator.IsVisible = loading;
            LoadingIndicator.IsRunning = loading;
        }

        private void OnBackClicked(object? sender, EventArgs eventArgs)
        {
            if (WorkspaceView.CanGoBack)
            {
                WorkspaceView.GoBack();
            }
        }

        protected override bool OnBackButtonPressed()
        {
            if (WorkspaceView.IsVisible && WorkspaceView.CanGoBack)
            {
                WorkspaceView.GoBack();
                return true;
            }

            return base.OnBackButtonPressed();
        }

        private void OnHomeClicked(object? sender, EventArgs eventArgs)
        {
            if (_serverAddress != null)
            {
                WorkspaceView.Source = _serverAddress.AbsoluteUri;
            }
        }

        private void OnReloadClicked(object? sender, EventArgs eventArgs)
        {
            ConnectionError.IsVisible = false;
            WorkspaceView.IsVisible = true;
            WorkspaceView.Reload();
        }

        private void OnServerClicked(object? sender, EventArgs eventArgs)
        {
            WorkspaceView.IsVisible = false;
            BrowserToolbar.IsVisible = false;
            ConnectionError.IsVisible = false;
            SetupPanel.IsVisible = true;
            SetLoading(false);
        }
    }
}
