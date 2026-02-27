using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace TattooInkMixer.Data.Migrations;

[DbContext(typeof(InkMixerDbContext))]
partial class InkMixerDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");

        modelBuilder.Entity("TattooInkMixer.Data.ColorTableRecord", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<string>("Brand")
                    .HasColumnType("TEXT");

                b.Property<string>("Category")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<string>("Hex")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.ToTable("ColorTableEntries");
            });
#pragma warning restore 612, 618
    }
}
