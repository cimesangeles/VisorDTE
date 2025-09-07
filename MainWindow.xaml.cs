using Microsoft.UI.Xaml;
using VisorDTE.ViewModels;

namespace VisorDTE;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }

    public MainWindow()
    {
        this.InitializeComponent();
        ViewModel = new MainViewModel(); // El DataContext se enlaza en el XAML con x:Bind
        this.Title = "Visor de Documentos Tributarios Electrónicos";
    }
}