using Blocus.Models;
using Blocus.ViewModels;
using System.Diagnostics;

namespace Blocus.Views;

public partial class BlockContentView : ContentView
{
    public BlockContentView()
    {
        InitializeComponent();
    }

    public BlockContentView(BlockViewModel blockViewModel) : this()
    {
        BindingContext = blockViewModel;
        _ = blockViewModel.LoadChildrenAsync();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (BindingContext is BlockViewModel vm)
        {
            _ = vm.LoadChildrenAsync();
        }
    }
}
