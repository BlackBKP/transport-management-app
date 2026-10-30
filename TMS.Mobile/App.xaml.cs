namespace TMS.Mobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            string theme = Preferences.Default.Get("tms-native-theme", "system");
            UserAppTheme = theme switch
            {
                "dark" => AppTheme.Dark,
                "light" => AppTheme.Light,
                _ => AppTheme.Unspecified
            };
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage());
        }
    }
}
