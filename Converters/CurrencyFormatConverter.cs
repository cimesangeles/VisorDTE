using Microsoft.UI.Xaml.Data;
using System;
using System.Globalization;

namespace VisorDTE.Converters
{
    public class CurrencyFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is decimal || value is double || value is int)
            {
                return string.Format(CultureInfo.GetCultureInfo("es-SV"), "{0:C}", value);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}