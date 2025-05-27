using Blocus.Services;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;

namespace Blocus.ViewModels
{
    public partial class TrashViewModel : ObservableObject
    {
        private readonly BlockService _svc;
        private readonly MainViewModel _mainVm;

        public ObservableCollection<BlockViewModel> TrashedBlocks { get; } = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string searchText = string.Empty;

        public TrashViewModel(BlockService svc, MainViewModel mainVm)
        {
            _svc = svc;
            _mainVm = mainVm;

            _ = LoadTrashAsync();
        }

        public async Task LoadTrashAsync()
        {
            IsLoading = true;

            TrashedBlocks.Clear();
            var list = await _svc.GetDeletedBlocksAsync();
            foreach (var b in list)
            {
                TrashedBlocks.Add(new BlockViewModel(
                    b,
                    _svc,
                    _mainVm,
                    level: 0,
                    parent: null,
                    trashViewModel: this));
            }

            IsLoading = false;
        }

        partial void OnSearchTextChanged(string value)
        {
            FilterTrashedBlocks();
        }

        private void FilterTrashedBlocks()
        {
            var query = (SearchText ?? "").Trim().ToLower();

            var filtered = TrashedBlocks.Where(b => string.IsNullOrWhiteSpace(query) || (b.Title?.ToLower().Contains(query) ?? false)).ToList();
            TrashedBlocks.Clear();
            foreach (var block in filtered)
            {
                TrashedBlocks.Add(block);
            }
        }

        [RelayCommand]
        public async Task ClearTrashAsync()
        {
            foreach (var block in TrashedBlocks.ToList())
            {
                await block.PermanentlyDeleteAsync();
            }
        }
    }
}
