// /MainWindow.xaml.cs
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using VisorDTE.Models;
using VisorDTE.ViewModels;
using Windows.ApplicationModel;
using WinRT.Interop;

namespace VisorDTE
{
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; } = new MainViewModel();
        private double _lastInspectorColumnWidth = 0;

        public MainWindow()
        {
            this.InitializeComponent();
            RootGrid.DataContext = ViewModel;
            SetWindowIcon();
            this.Title = "Visor de Documentos Tributarios Electrónicos";
            HeaderTitleTextBlock.Text = this.Title;
            this.Activated += MainWindow_Activated;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }
        private void SetWindowIcon()
        {
            // Obtenemos el AppWindow, que es el que controla las propiedades de la ventana del sistema.
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

            // Construimos la ruta completa al icono dentro del paquete de la aplicación instalada.
            string iconPath = Path.Combine(Package.Current.InstalledLocation.Path, "Assets/appicon.ico");
            appWindow.SetIcon(iconPath);
        }


        private void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            // Asignamos el XamlRoot de forma segura una vez que el Grid está cargado
            if (sender is Grid grid)
            {
                ViewModel.MainXamlRoot = grid.XamlRoot;
            }
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is JsonPropertyNode node)
            {
                ViewModel.CopyToClipboardCommand.Execute(node.Value);
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.IsInspectorVisible))
            {
                if (ViewModel.IsInspectorVisible)
                {
                    if (InspectorColumn.Width.Value == 0)
                    {
                        if (_lastInspectorColumnWidth > 0)
                        {
                            InspectorColumn.Width = new GridLength(_lastInspectorColumnWidth);
                        }
                        else
                        {
                            InspectorColumn.Width = new GridLength(0.5, GridUnitType.Star);
                        }
                    }
                }
                else
                {
                    if (InspectorColumn.ActualWidth > 0)
                    {
                        _lastInspectorColumnWidth = InspectorColumn.ActualWidth;
                    }
                    InspectorColumn.Width = new GridLength(0);
                }
            }
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
                int newWidth = (int)(workArea.Width * 0.5); // 50% del ancho del área de trabajo
                int newHeight = workArea.Height; // Altura completa del área de trabajo

                // --- INICIO DE LA CORRECCIÓN ---
                // Posicionamos la ventana en el borde izquierdo (workArea.X)
                // y en el borde superior (workArea.Y).
                appWindow.MoveAndResize(new Windows.Graphics.RectInt32(
                    workArea.X,
                    workArea.Y,
                    newWidth,
                    newHeight));
                // --- FIN DE LA CORRECCIÓN ---
            }
        }

        private void FlipView_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.FirstOrDefault() is DteViewModel selected)
            {
                ViewModel.SelectedDte = selected;
            }
        }
    }
}