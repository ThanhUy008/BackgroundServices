using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.DbContexts;
using Shared.VersionTrackerEntities;

namespace ChangeTracker.Services.BaseServices;

public abstract class BasePollingService<TEntity>
    where TEntity : ITrackable
{
    private readonly IDbContextFactory<AppDbContext> _appDbContextFactory;
    private readonly IDbContextFactory<CustomerDbContext> _customerDbContextFactory;

    public BasePollingService(
        IDbContextFactory<AppDbContext> appDbContextFactory,
        IDbContextFactory<CustomerDbContext> customerDbContextFactory
    )
    {
        _appDbContextFactory = appDbContextFactory;
        _customerDbContextFactory = customerDbContextFactory;
    }

    public virtual async Task<List<TEntity>> FetchNewlyChangedEntities(
        CancellationToken cancellationToken = default
    )
    {
        using var _customerDbContext = await _customerDbContextFactory.CreateDbContextAsync(
            cancellationToken
        );

        var currTable = _customerDbContext.GetTableName<TEntity>();

        var currVer = await GetOrCreateVersionTracker(currTable, cancellationToken);

        return await GetChangedEntityFromLast(currVer, cancellationToken);
    }

    public virtual async Task<VersionTracker> GetOrCreateVersionTracker(
        string tableName,
        CancellationToken cancellationToken = default
    )
    {
        using var _appDbContext = await _appDbContextFactory.CreateDbContextAsync(
            cancellationToken
        );

        var currVer = await _appDbContext.VersionTrackers.FirstOrDefaultAsync(
            x => x.TableName == tableName,
            cancellationToken: cancellationToken
        );

        if (currVer == null)
        {
            currVer = new()
            {
                Id = Guid.NewGuid(),
                TableName = tableName,
                LastVersionTimestamp = DateTime.MinValue,
            };

            _appDbContext.VersionTrackers.Add(currVer);

            await _appDbContext.SaveChangesAsync(cancellationToken);

            await _appDbContext.Entry(currVer).ReloadAsync(cancellationToken);
        }

        return currVer;
    }

    public virtual async Task UpdateVersionTracker(
        DateTime lastVersionTimestamp,
        CancellationToken cancellationToken = default
    )
    {
        using var _appDbContext = await _appDbContextFactory.CreateDbContextAsync(
            cancellationToken
        );

        using var _customerDbContext = await _customerDbContextFactory.CreateDbContextAsync(
            cancellationToken
        );

        var currTable = _customerDbContext.GetTableName<TEntity>();

        var currVer =
            await _appDbContext.VersionTrackers.FirstOrDefaultAsync(
                x => x.TableName == currTable,
                cancellationToken: cancellationToken
            )
            ?? throw new InvalidOperationException(
                $"VersionTracker for table {currTable} not found."
            );

        currVer.LastVersionTimestamp = lastVersionTimestamp;
        await _appDbContext.SaveChangesAsync(cancellationToken);
    }

    public abstract Task<List<TEntity>> GetChangedEntityFromLast(
        VersionTracker currVer,
        CancellationToken cancellationToken = default
    );
}
