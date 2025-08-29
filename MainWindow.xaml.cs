// /MainWindow.xaml.cs
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.ComponentModel;
using System.Linq;
using VisorDTE.Models;
using VisorDTE.ViewModels;
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
            this.Title = "Visor de Documentos Tributarios Electrónicos";
            HeaderTitleTextBlock.Text = this.Title;
            this.Activated += MainWindow_Activated;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is JsonPropertyNode node)
            {
                ViewModel.CopyToClipboardCommand.Execute(node.Value);
            }
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
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
                int newWidth = (int)(workArea.Width * 0.5);
                int newHeight = workArea.Height;

                appWindow.MoveAndResize(new Windows.Graphics.RectInt32(workArea.X, workArea.Y, newWidth, newHeight));
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