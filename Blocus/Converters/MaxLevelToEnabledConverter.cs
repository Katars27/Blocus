using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Blocus.Converters
{
    /// <summary>
    /// Конвертер для ограничения глубины вложенности блоков: 
    /// возвращает true, если level < max (max передаётся через ConverterParameter).
    /// </summary>
    public class MaxLevelToEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int level && parameter is not null && int.TryParse(parameter.ToString(), out int max))
                return level < max;

            return false; // невалидные данные => нельзя добавить вложенность
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
