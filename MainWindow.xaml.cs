using Microsoft.UI.Xaml;
using VisorDTE.ViewModels;

namespace VisorDTE;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }

    public MainWindow()
    {
        this.InitializeComponent();
        ViewModel = new MainViewModel();
        this.RootGrid.DataContext = ViewModel;
        this.Title = "Visor de Documentos Tributarios Electrónicos";
    }
}