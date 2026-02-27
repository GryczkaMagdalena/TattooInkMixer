using Microsoft.EntityFrameworkCore;
using TattooInkMixer.Data;

namespace TattooInkMixer.Services;

public sealed class EfColorTableRepository(InkMixerDbContext dbContext) : IColorTableRepository
{
    public IReadOnlyList<ColorTableEntry> LoadEntries()
    {
        dbContext.Database.EnsureCreated();

        return dbContext.ColorTableRecords
            .AsNoTracking()
            .OrderBy(record => record.Id)
            .Select(record => new ColorTableEntry
            {
                Category = record.Category,
                Name = record.Name,
                Brand = record.Brand,
                Hex = record.Hex
            })
            .ToList();
    }

    public void SaveEntries(IEnumerable<ColorTableEntry> entries)
    {
        dbContext.Database.EnsureCreated();

        dbContext.ColorTableRecords.RemoveRange(dbContext.ColorTableRecords);

        dbContext.ColorTableRecords.AddRange(entries.Select(entry => new ColorTableRecord
        {
            Category = entry.Category,
            Name = entry.Name,
            Brand = entry.Brand,
            Hex = entry.Hex
        }));

        dbContext.SaveChanges();
    }
}
