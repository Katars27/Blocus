using Blocus.Views;

namespace Blocus
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("Favorites", typeof(FavoritesView));
            Routing.RegisterRoute("Trash", typeof(TrashView));
        }
    }
}
