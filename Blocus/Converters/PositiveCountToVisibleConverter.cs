using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Blocus.Converters
{
    public class PositiveCountToVisibleConverter : IValueConverter
    {
        // value: Children.Count (int)
        // возвращаем true, если count > 0
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
                return count > 0;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
