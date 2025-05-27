using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Blocus.Models;
using Blocus.Services;
using Blocus.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls; // ← для Shell.Current.GoToAsync

namespace Blocus.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private const int MaxOpenTabs = 7;
        private readonly BlockService _blockService;

        public ObservableCollection<BlockViewModel> AllFavoriteBlocks { get; } = new();
        public ObservableCollection<BlockViewModel> AllRootBlocks { get; } = new();

        public ObservableCollection<BlockViewModel> OpenTabs { get; } = new();
        [ObservableProperty]
        private BlockViewModel? selectedTab;

        public ObservableCollection<BlockViewModel> RootBlocks { get; } = new();

        [ObservableProperty]
        private string searchText = string.Empty;

        public ObservableCollection<BlockViewModel> FlatNavigationBlocks { get; } = new();
        public ObservableCollection<BlockViewModel> FavoriteBlocks { get; } = new();

        public BlockContextMenuViewModel BlockMenuVM { get; } = new();

        [ObservableProperty]
        private double sliderValue = 50;

        private BlockViewModel? selectedBlock;
        public BlockViewModel? SelectedBlock
        {
            get => selectedBlock;
            set
            {
                if (selectedBlock != value)
                {
                    var old = selectedBlock;
                    selectedBlock = value;
                    old?.NotifyIsSelectedChanged();
                    selectedBlock?.NotifyIsSelectedChanged();
                    OnPropertyChanged(nameof(Breadcrumbs));
                }
            }
        }

        public IEnumerable<BlockViewModel> Breadcrumbs
        {
            get
            {
                var list = new List<BlockViewModel>();
                var current = SelectedBlock;
                while (current != null)
                {
                    list.Insert(0, current);
                    current = current.Parent;
                }
                return list;
            }
        }

        public MainViewModel(BlockService blockService)
        {
            _blockService = blockService;
            _ = LoadBlocksAsync();
        }

        [RelayCommand]
        public async Task LoadBlocksAsync()
        {
            var roots = await _blockService.GetRootBlocksAsync();
            RootBlocks.Clear();
            AllRootBlocks.Clear();
            foreach (var block in roots)
            {
                var vm = new BlockViewModel(block, _blockService, this, 0, null);
                RootBlocks.Add(vm);
                AllRootBlocks.Add(vm);
            }

            if (RootBlocks.Count > 0)
                SelectedBlock = RootBlocks[0];

            await LoadFavoritesAsync();
            OnPropertyChanged(nameof(Breadcrumbs));
        }

        [RelayCommand]
        public async Task LoadFavoritesAsync()
        {
            var favs = await _blockService.GetFavoriteBlocksAsync();
            FavoriteBlocks.Clear();
            AllFavoriteBlocks.Clear();
            foreach (var b in favs)
            {
                var vm = new BlockViewModel(b, _blockService, this, 0, null);
                FavoriteBlocks.Add(vm);
                AllFavoriteBlocks.Add(vm);
            }
        }

        [RelayCommand]
        public async Task AddRootBlockAsync()
        {
            var newBlock = new Block
            {
                Type = "page",
                Title = "Новый проект",
                Content = "Поменяйте название)",
                Order = RootBlocks.Count,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _blockService.AddBlockAsync(newBlock);
            var newVM = new BlockViewModel(newBlock, _blockService, this, 0, null);
            RootBlocks.Add(newVM);
            SelectedBlock = newVM;
        }

        [RelayCommand]
        public async Task AddRootBlockByTypeAsync(string type)
        {
            if (string.IsNullOrWhiteSpace(type)) return;

            var newBlock = new Block
            {
                Type = type,
                Content = type == "checkbox" ? "Чекбокс" : "Новый проект",
                Order = RootBlocks.Count,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _blockService.AddBlockAsync(newBlock);
            var newVM = new BlockViewModel(newBlock, _blockService, this, 0, null);
            RootBlocks.Add(newVM);
            SelectedBlock = newVM;
        }

        public void OpenInTab(BlockViewModel vm)
        {
            if (OpenTabs.Contains(vm))
            {
                SelectedTab = vm;
                return;
            }

            if (OpenTabs.Count >= MaxOpenTabs)
            {
                var toClose = OpenTabs[0];
                OpenTabs.RemoveAt(0);
                if (SelectedTab == toClose)
                    SelectedTab = OpenTabs.FirstOrDefault();
            }

            OpenTabs.Add(vm);
            SelectedTab = vm;
        }

        [RelayCommand]
        public void CloseTab(BlockViewModel vm)
        {
            if (!OpenTabs.Contains(vm)) return;

            var idx = OpenTabs.IndexOf(vm);
            OpenTabs.Remove(vm);

            if (SelectedTab == vm)
            {
                if (OpenTabs.Count > 0)
                    SelectedTab = OpenTabs[Math.Clamp(idx, 0, OpenTabs.Count - 1)];
                else
                    SelectedTab = null;
            }
        }

        [RelayCommand]
        public void SelectTab(BlockViewModel vm)
        {
            SelectedTab = vm;
        }

        [RelayCommand]
        public void SelectBlock(BlockViewModel? block)
        {
            SelectedBlock = block;
            OnPropertyChanged(nameof(Breadcrumbs));
        }

        [RelayCommand]
        public async Task OpenBlockAsync(BlockViewModel vm)
        {
            await vm.LoadChildrenAsync();
            OpenInTab(vm);
        }

        public void OnTreeChanged()
        {
            OnPropertyChanged(nameof(RootBlocks));
            OnPropertyChanged(nameof(Breadcrumbs));
        }

        public void UpdateFlatList()
        {
            FlatNavigationBlocks.Clear();
            foreach (var block in RootBlocks)
                AddToFlatList(block, 0);
        }

        private void AddToFlatList(BlockViewModel block, int level)
        {
            FlatNavigationBlocks.Add(block);
            if (block.IsExpanded)
            {
                foreach (var child in block.Children)
                    AddToFlatList(child, level + 1);
            }
        }

        public BlockViewModel? FindParentForBlock(BlockViewModel child)
        {
            foreach (var root in RootBlocks)
            {
                var found = FindParentRecursive(root, child);
                if (found != null)
                    return found;
            }
            return null;
        }

        private BlockViewModel? FindParentRecursive(BlockViewModel parent, BlockViewModel target)
        {
            if (parent.Children.Contains(target))
                return parent;
            foreach (var ch in parent.Children)
            {
                var result = FindParentRecursive(ch, target);
                if (result != null)
                    return result;
            }
            return null;
        }

        public static IEnumerable<BlockViewModel> AllBlocks(BlockViewModel root)
        {
            yield return root;
            foreach (var child in root.Children)
            {
                foreach (var desc in AllBlocks(child))
                    yield return desc;
            }
        }

        [RelayCommand]
        public async Task LoadBlockAsync(BlockViewModel selected)
        {
            await selected.LoadChildrenAsync();
            OpenInTab(selected);
        }

        partial void OnSliderValueChanged(double value)
        {
            // Обработчик изменения слайдера
        }

        [RelayCommand]
        public async Task OpenFavoritesAsync()
        => await Shell.Current.GoToAsync("//Favorites");

        [RelayCommand]
        public async Task OpenTrashAsync()
            => await Shell.Current.GoToAsync("//Trash");

        partial void OnSearchTextChanged(string value)
        {
            FilterBlocks();
        }

        private void FilterBlocks()
        {
            var query = (SearchText ?? "").Trim().ToLower();

            FavoriteBlocks.Clear();
            foreach (var item in AllFavoriteBlocks.Where(b => string.IsNullOrWhiteSpace(query) || (b.Title?.ToLower().Contains(query) ?? false)))
                FavoriteBlocks.Add(item);

            RootBlocks.Clear();
            foreach (var item in AllRootBlocks.Where(b => string.IsNullOrWhiteSpace(query) || (b.Title?.ToLower().Contains(query) ?? false)))
                RootBlocks.Add(item);
        }
    }
}
