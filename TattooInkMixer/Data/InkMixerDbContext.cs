using Microsoft.EntityFrameworkCore;

namespace TattooInkMixer.Data;

public class InkMixerDbContext(DbContextOptions<InkMixerDbContext> options) : DbContext(options)
{
    public DbSet<ColorTableRecord> ColorTableRecords => Set<ColorTableRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ColorTableRecord>(entity =>
        {
            entity.ToTable("ColorTableEntries");
            entity.HasKey(record => record.Id);

            entity.Property(record => record.Category)
                .IsRequired();

            entity.Property(record => record.Name)
                .IsRequired();

            entity.Property(record => record.Brand)
                .IsRequired(false);

            entity.Property(record => record.Hex)
                .IsRequired();
        });
    }
}
