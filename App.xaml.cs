// App.xaml.cs
using Microsoft.UI.Xaml;

namespace VisorDTE;

public partial class App : Application
{
    // ===== CAMBIO CLAVE AQUÍ =====
    // Creamos una propiedad estática para acceder a la ventana principal.
    // Reemplazamos la variable privada "m_window".
    public static Window MainWindow { get; private set; }

    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        // Asignamos la nueva instancia de MainWindow a nuestra propiedad estática.
        MainWindow = new MainWindow();
        MainWindow.Activate();
    }
}