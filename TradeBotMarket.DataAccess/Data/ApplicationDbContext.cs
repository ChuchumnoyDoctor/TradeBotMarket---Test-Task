using Microsoft.EntityFrameworkCore;
using TradeBotMarket.Domain.Models;

namespace TradeBotMarket.DataAccess.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<FuturePrice> FuturePrices { get; set; } = null!;
    public DbSet<PriceDifference> PriceDifferences { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FuturePrice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Symbol).IsRequired();
            entity.HasIndex(e => new { e.Symbol, e.Timestamp }).IsUnique();
        });

        modelBuilder.Entity<PriceDifference>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstSymbol).IsRequired();
            entity.Property(e => e.SecondSymbol).IsRequired();
            entity.HasIndex(e => new { e.FirstSymbol, e.SecondSymbol, e.FirstPriceTimestamp, e.SecondPriceTimestamp });
        });
    }
} 