using Blocus.ViewModels;

namespace Blocus;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _ = viewModel.LoadBlocksAsync();
    }
}

