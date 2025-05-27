using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Blocus.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Blocus.ViewModels
{
    public partial class FavoritesViewModel : ObservableObject
    {
        private readonly BlockService _svc;
        private readonly MainViewModel _mainVm;

        // Список ViewModel-ов избранного
        public ObservableCollection<BlockViewModel> Favorites { get; } = new();

        // Теперь принимаем в конструктор MainViewModel
        public FavoritesViewModel(BlockService svc, MainViewModel mainVm)
        {
            _svc = svc;
            _mainVm = mainVm;

            // стартуем загрузку сразу
            _ = LoadFavoritesAsync();
        }

        // Загрузка избранного — создаём BlockViewModel с передачей mainVm
        private async Task LoadFavoritesAsync()
        {
            Favorites.Clear();
            var list = await _svc.GetFavoriteBlocksAsync();
            foreach (var b in list)
            {
                Favorites.Add(new BlockViewModel(
                    b,
                    _svc,
                    _mainVm,    // ← передаём MainViewModel, чтобы Toggle вернул блок обратно
                    level: 0,
                    parent: null));
            }
        }

        /// <summary>
        /// Снимает флаг «избранного» у выбранного блока.
        /// </summary>
        [RelayCommand]
        public async Task ToggleFavoriteAsync(BlockViewModel vm)
        {
            if (vm == null) return;

            // Доверьтесь логике в BlockViewModel: он сам переключит Props,
            // обновит IsFavorite и переместит себя между списками.
            await vm.ToggleFavoriteAsync();

            // VM сам себя уберёт из Favorites и вернёт в основной список
        }
    }
}
