using System.Runtime.CompilerServices;
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
    protected readonly ILogger<BasePollingService<TEntity>> _logger;

    public BasePollingService(
        IDbContextFactory<AppDbContext> appDbContextFactory,
        IDbContextFactory<CustomerDbContext> customerDbContextFactory,
        ILogger<BasePollingService<TEntity>> logger
    )
    {
        _appDbContextFactory = appDbContextFactory;
        _customerDbContextFactory = customerDbContextFactory;
        _logger = logger;
    }

    public virtual async IAsyncEnumerable<List<TEntity>> FetchNewlyChangedEntitiesAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        using var _customerDbContext = await _customerDbContextFactory.CreateDbContextAsync(
            cancellationToken
        );

        var tableName = _customerDbContext.GetTableName<TEntity>();

        var currVer = await GetOrCreateVersionTrackerAsync(tableName, cancellationToken);

        var batch = new List<TEntity>(500);

        await foreach (var entity in GetChangedEntityFromLast(currVer, cancellationToken))
        {
            batch.Add(entity);

            // _logger.LogWarning($"Fetched entity ref: {RuntimeHelpers.GetHashCode(entity)}");

            if (batch.Count == 500)
            {
                yield return batch;

                batch = new List<TEntity>(500);
            }
        }
    }

    public virtual async Task<VersionTracker> GetOrCreateVersionTrackerAsync(
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

    public virtual async Task UpdateVersionTrackerAsync(
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

    public abstract IAsyncEnumerable<TEntity> GetChangedEntityFromLast(
        VersionTracker currVer,
        CancellationToken cancellationToken = default
    );
}
