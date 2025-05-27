using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Blocus.Converters
{
    /// <summary>
    /// Конвертирует bool (IsExpanded) в иконку-стрелку для UI (▼ / ►).
    /// true = ▼ (развёрнуто), false = ► (свёрнуто)
    /// </summary>
    public class BoolToChevronConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b && b ? "chevron_down.png" : "chevron_right.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException("BoolToChevronConverter used for one-way binding only.");
    }
}
