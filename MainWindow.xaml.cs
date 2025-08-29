// /MainWindow.xaml.cs
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.ComponentModel;
using System.Linq;
using VisorDTE.ViewModels;
using WinRT.Interop;
using System.Diagnostics; // Asegúrate de que esta línea esté presente

namespace VisorDTE
{
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; } = new MainViewModel();

        // Guarda el último ancho en píxeles antes de ocultar el panel.
        // Inicializado a un valor que representaría 0.5* para un inicio.
        private double _lastInspectorColumnWidth = 0; // Se inicializa en 0 para que en el primer "mostrar" use 0.5*

        public MainWindow()
        {
            this.InitializeComponent();
            RootGrid.DataContext = ViewModel;
            this.Title = "Visor de Documentos Tributarios Electrónicos";
            HeaderTitleTextBlock.Text = this.Title;
            this.Activated += MainWindow_Activated;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.IsInspectorVisible))
            {
                if (ViewModel.IsInspectorVisible)
                {
                    // AL MOSTRAR:
                    // Si el ancho actual es 0 (estaba oculto), restauramos el ancho.
                    if (InspectorColumn.Width.Value == 0)
                    {
                        if (_lastInspectorColumnWidth > 0)
                        {
                            // Restaurar al último ancho en píxeles si se había guardado
                            InspectorColumn.Width = new GridLength(_lastInspectorColumnWidth, GridUnitType.Pixel);
                        }
                        else
                        {
                            // Si no hay ancho guardado (primera vez o reinicio), usar el proporcional 0.5*
                            InspectorColumn.Width = new GridLength(0.5, GridUnitType.Star);
                        }
                    }
                }
                else
                {
                    // AL OCULTAR:
                    // 1. Guardar el ancho actual de la columna en píxeles antes de ocultarla,
                    //    siempre que no sea ya 0 (para evitar guardar 0 si ya estaba oculto).
                    if (InspectorColumn.ActualWidth > 0)
                    {
                        _lastInspectorColumnWidth = InspectorColumn.ActualWidth;
                    }
                    // 2. Forzar el ancho a 0 para ocultarla completamente.
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

        private void TreeView_Loaded(object sender, RoutedEventArgs e)
        {
            var treeView = sender as TreeView;
            if (treeView != null && treeView.RootNodes.Any())
            {
                var rootNode = treeView.RootNodes[0];
                rootNode.IsExpanded = true;

                foreach (var childNode in rootNode.Children)
                {
                    childNode.IsExpanded = true;
                }
            }
        }

        private void TreeView_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var delta = e.GetCurrentPoint(sender as UIElement).Properties.MouseWheelDelta;
            InspectorScrollViewer.ChangeView(null, InspectorScrollViewer.VerticalOffset - delta, null);
            e.Handled = true;
        }
    }
}