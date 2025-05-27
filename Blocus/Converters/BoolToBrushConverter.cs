using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;        
using Microsoft.Maui.Graphics;

namespace Blocus.Converters
{
    public class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parts = (parameter as string)?.Split(',') ?? new[] { "#FFFFFF", "#000000" };
            var trueColor = Color.FromArgb(parts[0]);
            var falseColor = Color.FromArgb(parts[1]);
            return (value as bool? == true)
                ? new SolidColorBrush(trueColor)
                : new SolidColorBrush(falseColor);
        }

        // Для двустороннего биндинга, нам не нужно обратное преобразование:
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

}
