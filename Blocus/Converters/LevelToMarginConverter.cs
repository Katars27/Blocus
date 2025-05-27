using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Blocus.Converters
{
    /// <summary>
    /// Конвертер: уровень вложенности → левый отступ (для визуализации иерархии блоков)
    /// </summary>
    public class LevelToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int level)
                return new Thickness(16 * level, 4, 0, 0); // Слева отступ зависит от уровня вложенности
            return new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
