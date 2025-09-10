using Microsoft.UI.Xaml;
using QuestPDF.Infrastructure;
using QuestPDF.Infrastructure;

namespace VisorDTE
{
    public partial class App : Application
    {
        public static Window MainWindow { get; private set; }

        public App()
        {
            this.InitializeComponent();
            QuestPDF.Settings.License = LicenseType.Community;
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            MainWindow = new MainWindow();
            MainWindow.Activate();
        }
    }
}