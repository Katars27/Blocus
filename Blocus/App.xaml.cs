using System;
using System.Linq;
using System.Diagnostics;
using Blocus.Data;
using Blocus.Models;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Blocus
{
    public partial class App : Application
    {
        // Сервис-провайдер из MauiProgram
        public static IServiceProvider Services { get; private set; }

        public App()
        {
            InitializeComponent();
            // Инициализация БД
            try
            {
                using var db = new BlocusContext();
                db.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EnsureCreated failed: {ex.GetType()}: {ex.Message}");
                if (ex.InnerException != null)
                    Debug.WriteLine($" → Inner: {ex.InnerException.GetType()}: {ex.InnerException.Message}");
                throw;
            }

            // Сидинг
            try
            {
                using var db = new BlocusContext();
                if (!db.Blocks.Any())
                {
                    var root = new Block
                    {
                        Type = "page",
                        Title = "Главная",
                        Content = "Добро Пожаловать",
                        Order = 0
                    };
                    db.Blocks.Add(root);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Seeding failed: {ex.GetType()}: {ex.Message}");
                if (ex.InnerException != null)
                    Debug.WriteLine($" → Inner: {ex.InnerException.GetType()}: {ex.InnerException.Message}");
                throw;
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
            => new Window(new AppShell());

        // Этот метод вызывайте из MauiProgram после Build()
        internal static void SetServices(IServiceProvider services)
            => Services = services;
    }
}
