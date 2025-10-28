using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Jellyfin.Plugin.FinampStatsSync.Model;
using Microsoft.EntityFrameworkCore;

namespace Jellyfin.Plugin.FinampStatsSync.Database;

public class AppDbContext : DbContext
{
    public AppDbContext()
    {
        var folder = Path.Combine(Plugin.Instance!.DataFolderPath);
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        DbPath = Path.Combine(folder, "playback_entries.db");
    }

    public DbSet<TrackItem> TrackItems { get; set; }

    public string DbPath { get; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite($"Data Source={DbPath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TrackItem>(builder =>
        {
            builder
                .Property(p => p.ArtistIds)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
                .HasColumnType("TEXT");
        });
    }
}
