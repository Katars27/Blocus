using System;
using System.IO;
using Blocus.Data;
using Blocus.Services;
using Blocus.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Blocus.Views;

using Microsoft.Maui.LifecycleEvents;
#if WINDOWS
using Microsoft.UI.Windowing;
using Microsoft.UI;
using WinRT.Interop;
#endif

namespace Blocus
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Resources/Fonts/OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("Resources/Fonts/OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // ——————————————————————————————————————————————
            // 1) Подключение SQLite
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "blocus.db");
            var connString = $"Data Source={dbPath};Cache=Shared;";

            builder.Services.AddDbContext<BlocusContext>(options =>
                options.UseSqlite(connString)
                       .EnableSensitiveDataLogging(),
                ServiceLifetime.Scoped);

            // ——————————————————————————————————————————————
            // 2) Сервисы
            builder.Services.AddScoped<BlockService>();

            // ——————————————————————————————————————————————
            // 3) Главная VM и контекстное меню
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<BlockContextMenuViewModel>();

            // ——————————————————————————————————————————————
            // 4) FavoritesViewModel и TrashViewModel
            builder.Services.AddTransient<FavoritesViewModel>(sp =>
                new FavoritesViewModel(
                    sp.GetRequiredService<BlockService>(),
                    sp.GetRequiredService<MainViewModel>()));

            builder.Services.AddTransient<TrashViewModel>(sp =>
                new TrashViewModel(
                    sp.GetRequiredService<BlockService>(),
                    sp.GetRequiredService<MainViewModel>()));

            // ——————————————————————————————————————————————
            // 5) Страницы / Views
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<FavoritesView>();
            builder.Services.AddTransient<TrashView>();
            builder.Services.AddTransient<NavigationView>();
            builder.Services.AddTransient<NavigationBlockView>();

            // ——————————————————————————————————————————————
            // 6) Центрирование окна и размеры (только для Windows)
            builder.ConfigureLifecycleEvents(events =>
            {
#if WINDOWS
                events.AddWindows(w =>
                    w.OnWindowCreated(window =>
                    {
                        var mauiWinUIWindow = (Microsoft.UI.Xaml.Window)window;
                        var hWnd = WindowNative.GetWindowHandle(mauiWinUIWindow);
                        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
                        var appWindow = AppWindow.GetFromWindowId(windowId);

                        // Задать стартовый размер окна
                        int width = 1600;
                        int height = 900;
                        appWindow.Resize(new Windows.Graphics.SizeInt32(width, height));

                        // Получаем рабочую область монитора
                        var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
                        int screenWidth = displayArea.WorkArea.Width;
                        int screenHeight = displayArea.WorkArea.Height;

                        // Центрируем окно
                        int x = (screenWidth - width) / 2;
                        int y = (screenHeight - height) / 2;
                        appWindow.Move(new Windows.Graphics.PointInt32(x, y));

                    })
                );
#endif
            });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // ——————————————————————————————————————————————
            var app = builder.Build();

            // Сохраняем ServiceProvider в App для доступа из View-кода
            App.SetServices(app.Services);

            // Ensure database is created
            using (var scope = app.Services.CreateScope())
            {
                var ctx = scope.ServiceProvider.GetRequiredService<BlocusContext>();
                ctx.Database.EnsureCreated();
            }

            return app;
        }
    }
}
