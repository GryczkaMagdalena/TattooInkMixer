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
    /// <summary>
    /// Export schema version for compatibility. Increment when file format changes.
    /// Importers should remain backward-compatible with older versions when possible.
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// UTC timestamp for when this file was exported.
    /// </summary>
    public DateTimeOffset ExportedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Optional application name that produced the export.
    /// </summary>
    public string? App { get; set; }

    public List<ColorTableEntry> Entries { get; set; } = [];
}

public static class ColorCategory
{
    public const string Standard = "Standard";
    public const string Company = "Company";
}
