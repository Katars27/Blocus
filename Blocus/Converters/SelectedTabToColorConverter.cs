using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Blocus.ViewModels;

namespace Blocus.Converters
{
    public class SelectedTabToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // value — твой BlockViewModel
            // parameter — x:Reference на RootPage (ContentPage), откуда BindingContext — это MainViewModel
            var vm = (parameter as ContentPage)?.BindingContext as MainViewModel;
            var currentTab = value as BlockViewModel;

            // Находим выбранную вкладку (SelectedTab)
            if (vm != null && currentTab != null)
            {
                return vm.SelectedTab == currentTab
                    ? Color.FromArgb("#bfaaff") // Активная вкладка
                    : Color.FromArgb("#23232e"); // Неактивная
            }
            // Фолбек
            return Color.FromArgb("#23232e");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
