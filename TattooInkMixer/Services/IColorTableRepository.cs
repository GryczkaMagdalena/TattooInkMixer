namespace TattooInkMixer.Services;

public interface IColorTableRepository
{
    IReadOnlyList<ColorTableEntry> LoadEntries();
    void SaveEntries(IEnumerable<ColorTableEntry> entries);
}
