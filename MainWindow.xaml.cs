// /MainWindow.xaml.cs
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Diagnostics; // <-- Necesario para Debug.WriteLine
using System.Linq;
using VisorDTE.ViewModels;
using WinRT.Interop;

namespace VisorDTE
{
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; } = new MainViewModel();

        public MainWindow()
        {
            this.InitializeComponent();
            RootGrid.DataContext = ViewModel;
            Debug.WriteLine("[DEBUG] DataContext de RootGrid establecido en ViewModel.");

            this.Title = "Visor de Documentos Tributarios Electrónicos";
            HeaderTitleTextBlock.Text = this.Title;
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
    }
}