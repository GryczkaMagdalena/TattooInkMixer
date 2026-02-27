namespace TattooInkMixer.Services;

public sealed class ColorTableStore
{
    private readonly object _sync = new();

    public IReadOnlyList<ColorTableEntry> Entries
    {
        get
        {
            lock (_sync)
            {
                return _entries
                    .Select(entry => new ColorTableEntry
                    {
                        Category = entry.Category,
                        Name = entry.Name,
                        Brand = entry.Brand,
                        Hex = entry.Hex
                    })
                    .ToList();
            }
        }
    }

    private List<ColorTableEntry> _entries = [];

    public void EnsureInitialized()
    {
        lock (_sync)
        {
            if (_entries.Count > 0)
                return;

            _entries = CreateDefaultEntries();
        }
    }

    public void ReplaceEntries(IEnumerable<ColorTableEntry> entries)
    {
        lock (_sync)
        {
            _entries = entries.Select(entry => new ColorTableEntry
            {
                Category = entry.Category,
                Name = entry.Name,
                Brand = entry.Brand,
                Hex = entry.Hex
            }).ToList();
        }
    }

    private static List<ColorTableEntry> CreateDefaultEntries() =>
    [
        new() { Category = ColorCategory.Standard, Name = "Red", Hex = "#D81B1B" },
        new() { Category = ColorCategory.Standard, Name = "Green", Hex = "#1BBE4B" },
        new() { Category = ColorCategory.Standard, Name = "Blue", Hex = "#1B4FD8" },
        new() { Category = ColorCategory.Standard, Name = "Black", Hex = "#101010" },
        new() { Category = ColorCategory.Standard, Name = "White", Hex = "#FFFFFF" },
        new() { Category = ColorCategory.Company, Brand = "World Famous Limitless", Name = "Red 1", Hex = "#A40504" },
        new() { Category = ColorCategory.Company, Brand = "Platinum by Dynamic", Name = "Red Grape", Hex = "#D81E86" },
        new() { Category = ColorCategory.Company, Brand = "Kuro Sumi Imperial", Name = "Pine Green", Hex = "#0B5834" }
    ];
}
