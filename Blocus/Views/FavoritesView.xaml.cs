using Blocus.ViewModels;

namespace Blocus.Views;

public partial class FavoritesView : ContentPage
{
	public FavoritesView()
	{
		InitializeComponent();
        BindingContext = App.Services.GetRequiredService<FavoritesViewModel>();
    }
}