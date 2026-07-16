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

public class CustomerPollingService : BasePollingService<Customer>
{
    private IDbContextFactory<AppDbContext> _appDbContextFactory;
    private IDbContextFactory<CustomerDbContext> _customerDbContextFactory;

    public CustomerPollingService(
        IDbContextFactory<AppDbContext> appDbContextFactory,
        IDbContextFactory<CustomerDbContext> customerDbContextFactory,
        ILogger<BasePollingService<Customer>> logger
    )
        : base(appDbContextFactory, customerDbContextFactory, logger)
    {
        _appDbContextFactory = appDbContextFactory;
        _customerDbContextFactory = customerDbContextFactory;
    }

    public override async IAsyncEnumerable<Customer> GetChangedEntityFromLast(
        VersionTracker currVer,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        using var _customerDbContext = await _customerDbContextFactory.CreateDbContextAsync(
            cancellationToken
        );

        await foreach (
            var customer in _customerDbContext
                .Customers.Where(c => c.UpdatedOn > currVer.LastVersionTimestamp)
                .AsNoTracking()
                .AsAsyncEnumerable()
                .WithCancellation(cancellationToken)
        )
        {
            // _logger.LogWarning($"Customer ref: {RuntimeHelpers.GetHashCode(customer)}");
            yield return customer;
        }
    }
}
