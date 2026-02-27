using Microsoft.EntityFrameworkCore;

namespace TattooInkMixer.Data;

public class InkMixerDbContext(DbContextOptions<InkMixerDbContext> options) : DbContext(options)
{
}
