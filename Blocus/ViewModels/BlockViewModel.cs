using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading.Tasks;
using Blocus.Models;
using Blocus.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.ApplicationModel;

namespace Blocus.ViewModels
{
    public partial class BlockViewModel : ObservableObject
    {
        public string Type => block.Type;
        public static int MaxLevel { get; } = 5;  
        public bool HasParent => Parent != null;


        private bool _isAddingBlock;
        private bool _areChildrenLoaded;

        [ObservableProperty]
        private Block block;

        [ObservableProperty] private bool isExpanded;
        [ObservableProperty] private bool isEditing;
        [ObservableProperty] private bool isSelected;

        private readonly TrashViewModel? _trashViewModel;
        public Guid Id => block.Id;
        public int Level { get; }
        public BlockViewModel? Parent { get; }
        public ObservableCollection<BlockViewModel> Children { get; }

        private readonly BlockService _blockService;
        private readonly MainViewModel _mainViewModel;

        public BlockViewModel(Block block, BlockService blockService, MainViewModel mainViewModel, int level = 0, BlockViewModel? parent = null, TrashViewModel? trashViewModel = null)
        {
            this.block = block;
            _blockService = blockService;
            _mainViewModel = mainViewModel;
            _trashViewModel = trashViewModel;
            Level = level;
            Parent = parent;
            Children = new ObservableCollection<BlockViewModel>();
        }

        public string Title
        {
            get => block.Title ?? string.Empty;
            set
            {
                if (block.Title != value)
                {
                    block.Title = value;
                    OnPropertyChanged();
                    _ = SaveAsync();
                }
            }
        }

        public string Content
        {
            get => block.Content ?? string.Empty;
            set
            {
                if (block.Content != value)
                {
                    block.Content = value;
                    OnPropertyChanged();
                    _ = SaveAsync();
                }
            }
        }

        // Парсит block.Props в словарь
        private Dictionary<string, object> ParseProps()
        {
            if (string.IsNullOrWhiteSpace(block.Props))
                return new();

            try
            {
                return JsonSerializer
                    .Deserialize<Dictionary<string, object>>(block.Props!)
                    ?? new();
            }
            catch
            {
                return new();
            }
        }
        public string? TimeUntilDeletion
        {
            get
            {
                var props = ParseProps();
                if (props.TryGetValue("deletedAt", out var value) && DateTime.TryParse(value?.ToString(), out var deletedAt))
                {
                    var daysLeft = 30 - (DateTime.UtcNow - deletedAt).TotalDays;
                    if (daysLeft <= 0)
                        return "Удаляется скоро";
                    return $"Удалится через {Math.Ceiling(daysLeft)} дн.";
                }
                return null;
            }
        }

        public bool IsFavorite
        {
            get
            {
                var props = ParseProps();
                return props.TryGetValue("isFavorite", out var v)
                       && bool.TryParse(v?.ToString(), out var flag)
                       && flag;
            }
        }

        [RelayCommand]
        public async Task ToggleFavoriteAsync()
        {
            await _blockService.ToggleFavoriteAsync(Id);
            var updated = await _blockService.GetBlockAsync(Id);
            if (updated != null)
                block.Props = updated.Props;

            OnPropertyChanged(nameof(IsFavorite));

            // Перемещаем блок между списками (избранное / не избранное)
            if (_mainViewModel != null)
            {
                if (IsFavorite)
                {
                    // Перемещаем в список избранного
                    MoveBlockToParentList(this, _mainViewModel.FavoriteBlocks);
                }
                else
                {
                    // Возвращаем в исходный список
                    MoveBlockToParentList(this, Parent != null ? Parent.Children : _mainViewModel.RootBlocks);
                }
            }
        }

        private void MoveBlockToParentList(BlockViewModel blockViewModel, ObservableCollection<BlockViewModel> targetList)
        {
            if (blockViewModel.Parent != null)
                blockViewModel.Parent.Children.Remove(blockViewModel);
            else
                _mainViewModel.RootBlocks.Remove(blockViewModel);

            targetList.Add(blockViewModel);
        }

        [RelayCommand]
        public async Task LoadChildrenAsync()
        {
            if (_areChildrenLoaded) return;

            var list = await _blockService.GetChildrenAsync(Id);
            foreach (var child in list)
            {
                Children.Add(new BlockViewModel(child, _blockService, _mainViewModel, Level + 1, this));
            }

            _areChildrenLoaded = true;
        }

        [RelayCommand]
        public async Task ToggleExpandAsync()
        {
            if (!IsExpanded)
                await LoadChildrenAsync();
            IsExpanded = !IsExpanded;
        }

        [RelayCommand]
        public async Task AddChildBlockAsync()
        {
            if (_isAddingBlock) return;
            _isAddingBlock = true;

            try
            {
                if (Level >= MaxLevel)
                {
                    await Shell.Current.DisplayAlert("Ограничение", $"Максимальная вложенность — {MaxLevel}", "Ок");
                    return;
                }

                var newBlock = new Block
                {
                    Type = "text",
                    Title = "Новый элемент",
                    Content = string.Empty,
                    ParentId = Id,
                    Order = Children.Count,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _blockService.AddBlockAsync(newBlock);

                var vm = new BlockViewModel(newBlock, _blockService, _mainViewModel, Level + 1, this);
                Children.Add(vm);
                _mainViewModel.OnTreeChanged();
            }
            finally
            {
                _isAddingBlock = false;
            }
        }

        [RelayCommand]
        public async Task SaveAsync()
        {
            var wasExpanded = IsExpanded;
            IsEditing = false;
            block.UpdatedAt = DateTime.UtcNow;
            await _blockService.UpdateBlockAsync(block);
            _areChildrenLoaded = false;

            await ShowSavedSnackbar();

            IsExpanded = wasExpanded;
            if (wasExpanded)
            {
                await LoadChildrenAsync();
            }
        }

        [RelayCommand]
        public async Task DeleteAsync()
        {
            // Помещаем блок в корзину (помечаем как удалённый)
            System.Diagnostics.Debug.WriteLine("DeleteAsync: Start deleting block with ID = " + Id);
            await _blockService.MoveToTrashAsync(Id);
            System.Diagnostics.Debug.WriteLine("DeleteAsync: Block moved to trash with ID = " + Id);

            // Обновляем UI в MainViewModel
            _mainViewModel.OnTreeChanged();  // Обновляем отображение данных в интерфейсе

            // Проверка на перезагрузку блоков
            if (Level == 0)
            {
                System.Diagnostics.Debug.WriteLine("DeleteAsync: Root block, reloading blocks...");
                await _mainViewModel.LoadBlocksAsync();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("DeleteAsync: Child block, removing from parent...");
                Parent?.Children.Remove(this);  // Убираем блок из родительской коллекции
            }

            // Логирование завершения
            System.Diagnostics.Debug.WriteLine("DeleteAsync: Finished processing block with ID = " + Id);
        }


        [RelayCommand]
        public async Task MoveToAsync(BlockViewModel newParent)
        {
            if (newParent.Level >= MaxLevel)
            {
                await Shell.Current.DisplayAlert("Ограничение", $"Нельзя вложить глубже {MaxLevel}", "Ок");
                return;
            }

            Parent?.Children.Remove(this);
            block.ParentId = newParent.Id;
            block.UpdatedAt = DateTime.UtcNow;
            await _blockService.UpdateBlockAsync(block);
            newParent._areChildrenLoaded = false;
            newParent.Children.Add(this);
            _mainViewModel.OnTreeChanged();
        }

        [RelayCommand]
        public async Task OpenAsync()
        {
            await LoadChildrenAsync();
            IsExpanded = true;
            _mainViewModel.OpenInTab(this);
        }

        [RelayCommand]
        public void EnterEditMode() => IsEditing = true;

        [RelayCommand]
        public void GoBack()
        {
            if (Parent != null)
                _mainViewModel.OpenInTab(Parent);
        }

        private async Task ShowSavedSnackbar()
        {
        }


        [RelayCommand]
        public async Task DuplicateAsync()
        {
            var clone = new Block
            {
                Type = block.Type,
                Title = block.Title + " (копия)",
                Content = block.Content,
                ParentId = block.ParentId,
                Order = Parent?.Children.Count ?? _mainViewModel.RootBlocks.Count,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _blockService.AddBlockAsync(clone);

            var vm = new BlockViewModel(clone, _blockService, _mainViewModel, Level, Parent);

            if (Parent != null)
                Parent.Children.Add(vm);
            else
                _mainViewModel.RootBlocks.Add(vm);

            _mainViewModel.OnTreeChanged();
        }

        [RelayCommand]
        public async Task RestoreAsync()
        {
            try
            {
                if (_trashViewModel == null) return;

                await _blockService.RestoreFromTrashAsync(Id);
                _mainViewModel.OnTreeChanged();
                _trashViewModel.TrashedBlocks.Remove(this);

                await Shell.Current.DisplayAlert("Успех", "Блок восстановлен!", "Ок");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", ex.ToString(), "Ок");
            }
        }

        [RelayCommand]
        public async Task PermanentlyDeleteAsync()
        {
            if (_trashViewModel == null) return;

            await _blockService.DeleteBlockPermanentlyAsync(Id);
            _mainViewModel.OnTreeChanged();
            _trashViewModel.TrashedBlocks.Remove(this);

            await Shell.Current.DisplayAlert("Удалено", "Блок удалён навсегда!", "Ок");
        }

        [RelayCommand]
        public async Task MoveToTrashAsync() => await DeleteAsync();

        [RelayCommand]
        public async Task OpenInNewTabAsync() => await OpenAsync();

        [RelayCommand]
        public async Task OpenInNewWindowAsync() => await OpenAsync();

        public void NotifyIsSelectedChanged()
        {
            OnPropertyChanged(nameof(IsSelected));
        }
    }
}
