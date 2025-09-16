// /App.xaml.cs
using Microsoft.UI.Xaml;
using QuestPDF.Infrastructure;
using Windows.Storage;

namespace VisorDTE
{
    public partial class App : Application
    {
        public static Window MainWindow { get; private set; }
        public static FrameworkElement MainRoot { get; private set; }

        public App()
        {
            this.InitializeComponent();
            QuestPDF.Settings.License = LicenseType.Community;
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            MainWindow = new MainWindow();

            MainRoot = MainWindow.Content as FrameworkElement;
            ApplyTheme();

            MainWindow.Activate();
        }

        public static void ApplyTheme()
        {
            var savedTheme = ApplicationData.Current.LocalSettings.Values["appTheme"];
            if (MainRoot != null)
            {
                MainRoot.RequestedTheme = savedTheme switch
                {
                    "Light" => ElementTheme.Light,
                    "Dark" => ElementTheme.Dark,
                    _ => ElementTheme.Default // Usar tema del sistema
                };
            }
        }
    }
}