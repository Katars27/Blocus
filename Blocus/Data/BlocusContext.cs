using Microsoft.EntityFrameworkCore;
using Blocus.Models;
using System;
using System.IO;

namespace Blocus.Data
{
    public class BlocusContext : DbContext
    {
        // Пустой конструктор для вашего `new BlocusContext()` в App
        public BlocusContext()
        { }

        // Конструктор для DI / AddDbContext
        public BlocusContext(DbContextOptions<BlocusContext> options)
            : base(options)
        { }

        public DbSet<Block> Blocks { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Если уже сконфигурирован через DI — ничего не трогаем
            if (optionsBuilder.IsConfigured)
                return;

            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "blocus.db"
            );
            // включаем общий кэш, чтобы не блокировать файл при параллельных запросах
            optionsBuilder.UseSqlite($"Data Source={dbPath};Cache=Shared;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Каскадное удаление
            modelBuilder.Entity<Block>()
                .HasMany(b => b.Children)
                .WithOne(b => b.Parent)
                .HasForeignKey(b => b.ParentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Индексы на будущее (если понадобится искать по Title/Content)
            modelBuilder.Entity<Block>().HasIndex(b => b.Title);
            modelBuilder.Entity<Block>().HasIndex(b => b.Content);
        }
    }
}
