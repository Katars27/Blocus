using Blocus.Data;
using Blocus.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Blocus.Services
{
    public class BlockService
    {
        private readonly BlocusContext _context;

        public BlockService(BlocusContext context)
        {
            _context = context;
        }

        // Получить все блоки с детьми
        public async Task<List<Block>> GetAllBlocksAsync()
        {
            return await _context.Blocks
                .Include(b => b.Children)
                .ToListAsync();
        }

        // Получить блок по id
        public async Task<Block?> GetBlockAsync(Guid id)
        {
            return await _context.Blocks
                .Include(b => b.Children)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        // Добавить новый блок
        public async Task AddBlockAsync(Block block)
        {
            _context.Blocks.Add(block);
            await _context.SaveChangesAsync();
        }

        // Обновить блок (только сам блок, без Children)
        public async Task UpdateBlockAsync(Block block)
        {
            var existing = await _context.Blocks
                                         .Include(b => b.Children)
                                         .FirstOrDefaultAsync(b => b.Id == block.Id);

            if (existing == null)
                throw new InvalidOperationException($"Block {block.Id} not found");

            existing.Title = block.Title;
            existing.Content = block.Content;
            existing.Props = block.Props;
            existing.Order = block.Order;
            existing.Type = block.Type;
            existing.UpdatedAt = DateTime.UtcNow;

            SyncChildren(existing, block.Children);

            await _context.SaveChangesAsync();
        }

        private void SyncChildren(Block existingParent, IEnumerable<Block> incomingChildren)
        {
            var toRemove = existingParent.Children
                .Where(ec => !incomingChildren.Any(ic => ic.Id == ec.Id))
                .ToList();

            foreach (var removed in toRemove)
                _context.Blocks.Remove(removed);

            foreach (var incoming in incomingChildren)
            {
                var match = existingParent.Children
                    .FirstOrDefault(ec => ec.Id == incoming.Id);

                if (match == null)
                {
                    var newChild = new Block
                    {
                        Id = incoming.Id != default ? incoming.Id : Guid.NewGuid(),
                        ParentId = existingParent.Id,
                        Type = incoming.Type,
                        Title = incoming.Title,
                        Content = incoming.Content,
                        Props = incoming.Props,
                        Order = incoming.Order,
                        CreatedAt = incoming.CreatedAt != default
                                      ? incoming.CreatedAt
                                      : DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    existingParent.Children.Add(newChild);

                    if (incoming.Children?.Any() == true)
                        SyncChildren(newChild, incoming.Children);
                }
                else
                {
                    match.Type = incoming.Type;
                    match.Title = incoming.Title;
                    match.Content = incoming.Content;
                    match.Props = incoming.Props;
                    match.Order = incoming.Order;
                    match.UpdatedAt = DateTime.UtcNow;

                    if (incoming.Children != null)
                        SyncChildren(match, incoming.Children);
                }
            }
        }

        public async Task DeleteBlockAsync(Guid id)
        {
            var block = await GetBlockAsync(id);
            if (block != null)
            {
                _context.Blocks.Remove(block);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Block>> GetChildrenAsync(Guid parentId)
        {
            return await _context.Blocks
                                 .Where(b => b.ParentId == parentId)
                                 .OrderBy(b => b.Order)
                                 .AsNoTracking()
                                 .ToListAsync();
        }

        public async Task<bool> HasChildrenAsync(Guid parentId)
        {
            return await _context.Blocks
                                 .AnyAsync(b => b.ParentId == parentId);
        }

        public async Task<List<Block>> GetRootBlocksAsync()
        {
            var all = await _context.Blocks
                .Where(b => b.ParentId == null)
                .OrderBy(b => b.Order)
                .AsNoTracking()
                .ToListAsync();

            return all
                .Where(b => !ParseProps(b).ContainsKey("deletedAt"))
                .ToList();
        }

        private Dictionary<string, object> ParseProps(Block b)
        {
            if (string.IsNullOrWhiteSpace(b.Props))
                return new Dictionary<string, object>();
            return JsonSerializer
                .Deserialize<Dictionary<string, object>>(b.Props!)
                ?? new Dictionary<string, object>();
        }

        private void SerializeProps(Block b, Dictionary<string, object> dict)
        {
            b.Props = JsonSerializer.Serialize(dict);
        }

        public async Task ToggleFavoriteAsync(Guid id)
        {
            var block = await _context.Blocks.FindAsync(id);
            if (block == null) return;

            var props = ParseProps(block);
            var current = props.ContainsKey("isFavorite")
                && bool.TryParse(props["isFavorite"].ToString(), out var f) && f;

            props["isFavorite"] = !current;
            SerializeProps(block, props);

            await _context.SaveChangesAsync();
        }

        public async Task<List<Block>> GetFavoriteBlocksAsync()
        {
            var all = await _context.Blocks
                .AsNoTracking()
                .ToListAsync();

            var favs = all
                .Where(b =>
                {
                    var props = ParseProps(b);
                    var isFav = props.TryGetValue("isFavorite", out var v)
                                && bool.TryParse(v?.ToString(), out var flag)
                                && flag;
                    var isDeleted = props.ContainsKey("deletedAt");
                    return isFav && !isDeleted;
                })
                .OrderByDescending(b => b.UpdatedAt)
                .ToList();

            return favs;
        }

        public async Task MoveToTrashAsync(Guid id)
        {
            var block = await _context.Blocks.FindAsync(id);
            if (block == null) return;

            var props = ParseProps(block);
            props["deletedAt"] = DateTime.UtcNow.ToString("o");
            SerializeProps(block, props);

            await _context.SaveChangesAsync();
        }

        public async Task<List<Block>> GetDeletedBlocksAsync()
        {
            var all = await _context.Blocks.ToListAsync();
            return all
                .Where(b => ParseProps(b).ContainsKey("deletedAt"))
                .OrderBy(b =>
                    DateTime.Parse(ParseProps(b)["deletedAt"].ToString()!))
                .ToList();
        }

        public async Task RestoreFromTrashAsync(Guid id)
        {
            var block = await _context.Blocks.FindAsync(id);
            if (block == null) return;

            var props = ParseProps(block);
            if (props.Remove("deletedAt"))
                SerializeProps(block, props);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteBlockPermanentlyAsync(Guid id)
        {
            var block = await _context.Blocks.FindAsync(id);
            if (block != null)
            {
                _context.Blocks.Remove(block);
                await _context.SaveChangesAsync();
            }
        }

        public async Task CleanupTrashAsync()
        {
            var cutoff = DateTime.UtcNow.AddDays(-30); // Удаляем через 30 дней

            var all = await _context.Blocks.ToListAsync();

            var deleted = all
                .Where(b => ParseProps(b).ContainsKey("deletedAt"))
                .ToList();

            foreach (var b in deleted)
            {
                var props = ParseProps(b);
                if (DateTime.Parse(props["deletedAt"].ToString()!) <= cutoff)
                    _context.Blocks.Remove(b);
            }

            await _context.SaveChangesAsync();
        }
    }
}
