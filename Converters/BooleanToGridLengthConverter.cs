using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace VisorDTE.Converters
{
    public class BooleanToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isVisible && isVisible)
            {
                // Si es visible, usa el ancho que pasemos como parámetro (ej: "1*")
                // o "Auto" si no se pasa parámetro.
                string widthStr = parameter as string ?? "Auto";
                if (widthStr.EndsWith('*'))
                {
                    double starValue = double.Parse(widthStr.TrimEnd('*'));
                    return new GridLength(starValue, GridUnitType.Star);
                }
                return new GridLength(1, GridUnitType.Auto);
            }
            else
            {
                // Si está oculto, el ancho es cero.
                return new GridLength(0);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}