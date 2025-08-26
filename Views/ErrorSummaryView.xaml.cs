using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Collections.Generic;
using VisorDTE.Models;

namespace VisorDTE.Views
{
    public sealed partial class ErrorSummaryView : UserControl
    {
        public List<FileError> Errors { get; set; }
        public int SuccessCount { get; set; }

        public ErrorSummaryView()
        {
            this.InitializeComponent();
            this.Loaded += (s, e) =>
            {
                ErrorListView.ItemsSource = Errors;
                SuccessCountTextBlock.Text = $"{SuccessCount} archivo(s) procesado(s) correctamente.";
                ErrorCountTextBlock.Text = $"{Errors.Count} archivo(s) con error.";
            };
        }

        private void Item_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is FileError error)
            {
                ErrorDetailTextBlock.Text = error.ErrorMessage;
            }
        }
    }
}