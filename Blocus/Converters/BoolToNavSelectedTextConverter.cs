using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using System.Globalization;

namespace Blocus.Converters
{
    public class BoolToNavSelectedTextConverter : IValueConverter
    {
        // true → акцентный цвет, false → стандартный
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isSelected = value as bool? == true;
            return isSelected ? Colors.White : Colors.LightGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
