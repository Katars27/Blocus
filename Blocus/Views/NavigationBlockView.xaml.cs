using Blocus.ViewModels;
using Microsoft.Maui.Controls;

namespace Blocus.Views
{
    public partial class NavigationBlockView : ContentView
    {
        public NavigationBlockView()
        {
            InitializeComponent();
            this.HandlerChanged += OnHandlerChanged;
        }

        private void OnHandlerChanged(object sender, EventArgs e)
        {
            if (this.Handler != null)
            {
                var menu = CreateMenu();
                FlyoutBase.SetContextFlyout(this, menu);
            }
        }

        private MenuFlyout CreateMenu()
        {
            var menu = new MenuFlyout();
            var vm = BindingContext as BlockViewModel;

            // Добавить/убрать в избранное
            var favText = vm?.IsFavorite == true
                ? "Убрать из избранного"
                : "Добавить в избранное";
            menu.Add(new MenuFlyoutItem
            {
                Text = favText,
                Command = vm?.ToggleFavoriteCommand
            });

            // Копия
            menu.Add(new MenuFlyoutItem
            {
                Text = "Создать копию",
                Command = vm?.DuplicateCommand
            });

            // В корзину
            menu.Add(new MenuFlyoutItem
            {
                Text = "Переместить в корзину",
                Command = vm?.MoveToTrashCommand
            });

            menu.Add(new MenuFlyoutSeparator());

            // Открыть в новой вкладке
            menu.Add(new MenuFlyoutItem
            {
                Text = "Открыть в новой вкладке",
                Command = vm?.OpenInNewTabCommand
            });

            /*// Открыть в новом окне
            menu.Add(new MenuFlyoutItem
            {
                Text = "Открыть в новом окне",
                Command = vm?.OpenInNewWindowCommand
            });
            */
            return menu;
        }
    }
}
