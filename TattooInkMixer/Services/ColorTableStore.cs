namespace TattooInkMixer.Services;

public sealed class ColorTableStore
{
    private readonly IColorTableRepository _repository;
    private readonly object _sync = new();

    public ColorTableStore(IColorTableRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<ColorTableEntry> Entries
    {
        get
        {
            lock (_sync)
            {
                return _repository.LoadEntries()
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

    public void EnsureInitialized()
    {
        lock (_sync)
        {
            if (_repository.LoadEntries().Count > 0)
                return;

            _repository.SaveEntries(CreateDefaultEntries());
        }
    }

    public void ReplaceEntries(IEnumerable<ColorTableEntry> entries)
    {
        lock (_sync)
        {
            _repository.SaveEntries(entries.Select(entry => new ColorTableEntry
            {
                Category = entry.Category,
                Name = entry.Name,
                Brand = entry.Brand,
                Hex = entry.Hex
            }).ToList());
        }
    }

    public static List<ColorTableEntry> CreateDefaultEntries() =>
    [
        new() { Category = ColorCategory.Company, Brand = "World Famous Limitless", Name = "Straight White", Hex = "#EEEEE9" },
        new() { Category = ColorCategory.Company, Brand = "World Famous Limitless", Name = "Dark Blue 1 v2", Hex = "#003175" },
        new() { Category = ColorCategory.Company, Brand = "World Famous Limitless", Name = "Dark Green 2", Hex = "#0A804A" },
        new() { Category = ColorCategory.Company, Brand = "World Famous Limitless", Name = "Pure Yellow", Hex = "#FFFF0C" },
        new() { Category = ColorCategory.Company, Brand = "World Famous Limitless", Name = "Pure Orange", Hex = "#F4530C" },
        new() { Category = ColorCategory.Company, Brand = "World Famous Limitless", Name = "Medium Red 1", Hex = "#B10D0E" }
    ];
}
