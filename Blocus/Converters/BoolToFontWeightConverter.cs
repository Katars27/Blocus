using System;
using Microsoft.Maui.Controls;
using System.Globalization;

namespace Blocus.Converters
{
    public class BoolToFontWeightConverter : IValueConverter
    {
        // true → Bold, false → None
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isSelected = value as bool? == true;
            return isSelected ? FontAttributes.Bold : FontAttributes.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
