using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Blocus.ViewModels
{
    /// <summary>
    /// VM для контекстного меню блока — инкапсулирует команды для работы с выбранным BlockViewModel.
    /// </summary>
    public partial class BlockContextMenuViewModel : ObservableObject
    {
        /// <summary>Текущий блок, для которого открыто контекстное меню.</summary>
        [ObservableProperty]
        private BlockViewModel? currentBlock;

        /// <summary>Можно ли выполнять действие — блок должен быть выбран.</summary>
        public bool CanExecuteBlockAction() => CurrentBlock is not null;

        /// <summary>Переключает флаг «избранного» у текущего блока.</summary>
        [RelayCommand(CanExecute = nameof(CanExecuteBlockAction))]
        private async Task ToggleFavorite()
        {
            if (CurrentBlock is null) return;
            await CurrentBlock.ToggleFavoriteAsync();
        }

        /// <summary>Создаёт копию блока.</summary>
        [RelayCommand(CanExecute = nameof(CanExecuteBlockAction))]
        private void Duplicate() =>
            CurrentBlock?.DuplicateCommand.Execute(null);

        /// <summary>Перемещает блок в корзину.</summary>
        [RelayCommand(CanExecute = nameof(CanExecuteBlockAction))]
        private void MoveToTrash() =>
            CurrentBlock?.MoveToTrashCommand.Execute(null);

        /// <summary>Открывает блок в новой вкладке.</summary>
        [RelayCommand(CanExecute = nameof(CanExecuteBlockAction))]
        private void OpenInNewTab() =>
            CurrentBlock?.OpenInNewTabCommand.Execute(null);

        /// <summary>Открывает блок в новом окне.</summary>
        [RelayCommand(CanExecute = nameof(CanExecuteBlockAction))]
        private void OpenInNewWindow() =>
            CurrentBlock?.OpenInNewWindowCommand.Execute(null);

        /// <summary>
        /// При смене CurrentBlock обновляем CanExecute всех команд.
        /// </summary>
        partial void OnCurrentBlockChanged(BlockViewModel? value)
        {
            ToggleFavoriteCommand.NotifyCanExecuteChanged();
            DuplicateCommand.NotifyCanExecuteChanged();
            MoveToTrashCommand.NotifyCanExecuteChanged();
            OpenInNewTabCommand.NotifyCanExecuteChanged();
            OpenInNewWindowCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Устанавливает блок, для которого нужно показать меню.
        /// </summary>
        public void ShowForBlock(BlockViewModel block)
        {
            CurrentBlock = block;
        }
    }
}
