using System;
using System.Collections.Generic;
using System.Linq;
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
        IDbContextFactory<CustomerDbContext> customerDbContextFactory
    )
        : base(appDbContextFactory, customerDbContextFactory)
    {
        _appDbContextFactory = appDbContextFactory;
        _customerDbContextFactory = customerDbContextFactory;
    }

    public override async Task<List<Customer>> GetChangedEntityFromLast(
        VersionTracker currVer,
        CancellationToken cancellationToken = default
    )
    {
        using var _customerDbContext = await _customerDbContextFactory.CreateDbContextAsync(
            cancellationToken
        );

        return await _customerDbContext
            .Customers.Where(c => c.UpdatedOn > DateTime.MinValue)
            .ToListAsync(cancellationToken);
    }
}
