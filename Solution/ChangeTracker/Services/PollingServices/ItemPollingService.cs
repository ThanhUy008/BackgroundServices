using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ChangeTracker.Services.BaseServices;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.DbContexts;
using Shared.Entities;
using Shared.VersionTrackerEntities;

namespace ChangeTracker.Services.PollingServices;

public class ItemPollingService : BasePollingService<Item>
{
    private IDbContextFactory<AppDbContext> _appDbContextFactory;
    private IDbContextFactory<CustomerDbContext> _customerDbContextFactory;

    public ItemPollingService(
        IDbContextFactory<AppDbContext> appDbContextFactory,
        IDbContextFactory<CustomerDbContext> customerDbContextFactory,
        ILogger<BasePollingService<Item>> logger
    )
        : base(appDbContextFactory, customerDbContextFactory, logger)
    {
        _appDbContextFactory = appDbContextFactory;
        _customerDbContextFactory = customerDbContextFactory;
    }

    public override async IAsyncEnumerable<Item> GetChangedEntityFromLast(
        VersionTracker currVer,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        using var _customerDbContext = await _customerDbContextFactory.CreateDbContextAsync(
            cancellationToken
        );

        await foreach (
            var item in _customerDbContext
                .Items.Where(c => c.UpdatedOn > DateTime.MinValue)
                .AsNoTracking()
                .AsAsyncEnumerable()
                .WithCancellation(cancellationToken)
        )
        {
            // _logger.LogWarning($"Item ref: {RuntimeHelpers.GetHashCode(item)}");
            yield return item;
        }
    }
}
