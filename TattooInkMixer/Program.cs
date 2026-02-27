using TattooInkMixer.Services;
using MudBlazor.Services;
using TattooInkMixer.Components;
using Microsoft.EntityFrameworkCore;
using TattooInkMixer.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();
builder.Services.AddScoped<IColorTableRepository, EfColorTableRepository>();
builder.Services.AddScoped<ColorTableStore>();
builder.Services.AddDbContext<InkMixerDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<InkMixerDbContext>();
    dbContext.Database.Migrate();

    if (!dbContext.ColorTableRecords.Any())
    {
        dbContext.ColorTableRecords.AddRange(ColorTableStore.CreateDefaultEntries().Select(entry => new ColorTableRecord
        {
            Category = entry.Category,
            Name = entry.Name,
            Brand = entry.Brand,
            Hex = entry.Hex
        }));

        dbContext.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
