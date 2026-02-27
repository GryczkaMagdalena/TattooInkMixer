namespace TattooInkMixer.Data;

public sealed class ColorTableRecord
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string Hex { get; set; } = string.Empty;
}
