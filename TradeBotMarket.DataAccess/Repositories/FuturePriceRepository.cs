using Microsoft.EntityFrameworkCore;
using TradeBotMarket.DataAccess.Data;
using TradeBotMarket.Domain.Enums;
using TradeBotMarket.Domain.Extensions;
using TradeBotMarket.Domain.Interfaces;
using TradeBotMarket.Domain.Models;

namespace TradeBotMarket.DataAccess.Repositories;

public class FuturePriceRepository : IFuturePriceRepository
{
    private readonly ApplicationDbContext _context;

    public FuturePriceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FuturePrice>> GetAllPricesAsync(int maxItems)
    {
        return await _context.FuturePrices
            .OrderByDescending(p => p.Timestamp)
            .Take(maxItems)
            .ToListAsync();
    }

    public async Task<IEnumerable<FuturePrice>> GetPricesBySymbolAsync(FutureSymbolType symbol, int maxItems)
    {
        string symbolString = symbol.GetEnumMemberValue();
        return await GetPricesBySymbolAsync(symbolString, maxItems);
    }

    public async Task<IEnumerable<FuturePrice>> GetPricesBySymbolAsync(string symbolString, int maxItems)
    {
        return await _context.FuturePrices
            .Where(p => p.Symbol == symbolString)
            .OrderByDescending(p => p.Timestamp)
            .Take(maxItems)
            .ToListAsync();
    }

    public async Task<FuturePrice?> GetLatestPriceAsync(string symbol)
    {
        return await _context.FuturePrices
            .Where(p => p.Symbol == symbol)
            .OrderByDescending(p => p.Timestamp)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<FuturePrice>> GetPricesByDateRangeAsync(string symbol, DateTime startDate, DateTime endDate)
    {
        return await _context.FuturePrices
            .Where(p => p.Symbol == symbol && p.Timestamp >= startDate && p.Timestamp <= endDate)
            .OrderBy(p => p.Timestamp)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string symbol, DateTime timestamp)
    {
        return await _context.FuturePrices
            .AnyAsync(p => p.Symbol == symbol && p.Timestamp == timestamp);
    }

    public async Task AddPriceAsync(FuturePrice price)
    {
        await _context.FuturePrices.AddAsync(price);
        await SaveChangesAsync();
    }

    public async Task<IEnumerable<FuturePrice>> GetHistoricalPricesAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.FuturePrices
            .Where(p => p.Timestamp >= startDate && p.Timestamp <= endDate)
            .OrderBy(p => p.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<FuturePrice>> GetPricesForPeriodAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        return await _context.FuturePrices
            .Where(p => p.Timestamp >= startDate && p.Timestamp <= endDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(FuturePrice price)
    {
        await _context.FuturePrices.AddAsync(price);
    }

    public async Task UpdateAsync(FuturePrice price)
    {
        _context.FuturePrices.Update(price);
        await SaveChangesAsync();
    }

    public async Task<FuturePrice?> GetPriceBySymbolAndTimestampAsync(string symbol, DateTime timestamp)
    {
        return await _context.FuturePrices
            .FirstOrDefaultAsync(p => p.Symbol == symbol && p.Timestamp == timestamp);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
} 