using Blocus.ViewModels;

namespace Blocus.Views;

public partial class TrashView : ContentPage
{
	public TrashView()
	{
		InitializeComponent();
        BindingContext = App.Services.GetRequiredService<TrashViewModel>();
    }
}