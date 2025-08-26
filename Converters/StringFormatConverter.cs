using Microsoft.UI.Xaml.Data;
using System;

namespace VisorDTE.Converters
{
    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return null;
            if (parameter is not string formatString) return value.ToString();

            try
            {
                return string.Format(formatString, value);
            }
            catch (FormatException)
            {
                return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}