using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VisorDTE.Models; // Asegúrate de que este using esté si mueves el archivo

namespace VisorDTE.Views // O VisorDTE.Converters
{
    public class DteTemplateSelector : DataTemplateSelector
    {
        // Estas propiedades las enlazaremos desde el XAML
        public DataTemplate WideTemplate { get; set; }
        public DataTemplate NarrowTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            // App.MainWindow.Bounds.Width nos da el ancho actual de la ventana
            if (App.MainWindow.Bounds.Width < 720)
            {
                return NarrowTemplate;
            }
            else
            {
                return WideTemplate;
            }
        }
    }
}