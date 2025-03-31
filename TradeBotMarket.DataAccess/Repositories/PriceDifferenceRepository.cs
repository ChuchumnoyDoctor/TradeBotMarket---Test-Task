using Microsoft.EntityFrameworkCore;
using TradeBotMarket.DataAccess.Data;
using TradeBotMarket.Domain.Interfaces;
using TradeBotMarket.Domain.Models;

namespace TradeBotMarket.DataAccess.Repositories;

public class PriceDifferenceRepository : IPriceDifferenceRepository
{
    private readonly ApplicationDbContext _context;

    public PriceDifferenceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PriceDifference>> GetAllDifferencesAsync(int maxItems)
    {
        return await _context.PriceDifferences
            .OrderByDescending(p => p.FirstPriceTimestamp)
            .Take(maxItems)
            .ToListAsync();
    }

    public async Task<IEnumerable<PriceDifference>> GetDifferencesBySymbolAsync(int maxItems)
    {
        return await _context.PriceDifferences
            .OrderByDescending(p => p.FirstPriceTimestamp)
            .Take(maxItems)
            .ToListAsync();
    }

    public async Task AddDifferenceAsync(PriceDifference difference)
    {
        await _context.PriceDifferences.AddAsync(difference);
        await SaveChangesAsync();
    }

    public async Task<IEnumerable<PriceDifference>> GetDifferencesAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.PriceDifferences
            .Where(d => d.SecondPriceTimestamp >= startDate && d.SecondPriceTimestamp <= endDate)
            .OrderBy(d => d.SecondPriceTimestamp)
            .ToListAsync();
    }

    public async Task AddAsync(PriceDifference difference)
    {
        await _context.PriceDifferences.AddAsync(difference);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<FuturePrice>> GetPricesForPeriodAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        return await _context.FuturePrices
            .Where(p => p.Timestamp >= startDate && p.Timestamp <= endDate)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveDifferencesAsync(IEnumerable<PriceDifference> differences, CancellationToken cancellationToken)
    {
        await _context.PriceDifferences.AddRangeAsync(differences, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
} 