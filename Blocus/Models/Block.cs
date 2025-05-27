using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Text.Json;


namespace Blocus.Models
{
    /// <summary>
    /// Универсальный блок для блоковой архитектуры
    /// </summary>

    public partial class Block : ObservableObject
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? ParentId { get; set; }
        [ForeignKey(nameof(ParentId))]
        public virtual Block? Parent { get; set; }
        [Required, MaxLength(32)]
        public string Type { get; set; } = "page";

        [Required, MaxLength(256)]
        public string? Title { get; set; }

        [MaxLength(4096)]
        public string? Content { get; set; }

        [MaxLength(2048)]
        public string? Props { get; set; }
        public int Order { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Для MVVM/UI
        public virtual ObservableCollection<Block> Children { get; set; } = new();

        public DateTime? GetDeletedAt()
        {
            if (string.IsNullOrEmpty(Props))
                return null;
            try
            {
                var json = JsonSerializer.Deserialize<Dictionary<string, object>>(Props);
                if (json != null && json.TryGetValue("DeletedAt", out var value) && value != null)
                {
                    if (DateTime.TryParse(value.ToString(), out var dt))
                        return dt;
                }
            }
            catch { }
            return null;
        }

        // Установить дату удаления в Props
        public void SetDeletedAt(DateTime? deletedAt)
        {
            var dict = string.IsNullOrEmpty(Props)
                ? new Dictionary<string, object>()
                : JsonSerializer.Deserialize<Dictionary<string, object>>(Props) ?? new Dictionary<string, object>();
            if (deletedAt.HasValue)
                dict["DeletedAt"] = deletedAt.Value.ToString("o"); // ISO 8601
            else
                dict.Remove("DeletedAt");
            Props = JsonSerializer.Serialize(dict);
        }

        // Проверка, удалён ли блок
        public bool IsDeleted => GetDeletedAt().HasValue;
    }

}
