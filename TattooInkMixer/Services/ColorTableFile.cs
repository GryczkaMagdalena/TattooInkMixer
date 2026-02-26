namespace TattooInkMixer.Services;

public sealed class ColorTableEntry
{
    public string Category { get; set; } = ColorCategory.Standard;
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string Hex { get; set; } = string.Empty;
}

public sealed class ColorTableFile
{
    public int Version { get; set; } = 1;
    public List<ColorTableEntry> Entries { get; set; } = [];
}

public static class ColorCategory
{
    public const string Standard = "Standard";
    public const string Company = "Company";
}
