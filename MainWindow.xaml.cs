// /MainWindow.xaml.cs
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using System;
using VisorDTE.ViewModels;
using WinRT.Interop;

namespace VisorDTE;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; } = new MainViewModel();

    public MainWindow()
    {
        this.InitializeComponent();
        this.Title = "Visor de Documentos Tributarios Electrónicos";
        this.Activated += MainWindow_Activated;
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        this.Activated -= MainWindow_Activated;
        SetupInitialWindowSize();
    }

    private void SetupInitialWindowSize()
    {
        IntPtr hWnd = WindowNative.GetWindowHandle(this);
        WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

        DisplayArea displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Nearest);
        if (displayArea != null)
        {
            var workArea = displayArea.WorkArea;
            int newWidth = (int)(workArea.Width * 0.5);
            int newHeight = workArea.Height;

            Windows.Graphics.RectInt32 windowRect;
            windowRect.X = workArea.X;
            windowRect.Y = workArea.Y;
            windowRect.Width = newWidth;
            windowRect.Height = newHeight;

            appWindow.MoveAndResize(windowRect);
        }
    }

    // Este evento sigue siendo necesario para evitar que el FlipView cambie de página con la rueda del mouse
    private void FlipView_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        // Esta lógica ahora la maneja el ScrollViewer interno de cada página,
        // pero detenemos el evento aquí para que no interfiera con el FlipView.
        e.Handled = true;
    }
}