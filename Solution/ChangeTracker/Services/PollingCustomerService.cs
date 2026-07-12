using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.DbContexts;
using Shared.Entities;
using Shared.VersionTrackerEntities;

namespace ChangeTracker.Services;

public class PollingCustomerServices
{
    private IDbContextFactory<AppDbContext> _appDbContextFactory;
    private IDbContextFactory<CustomerDbContext> _customerDbContextFactory;

    public PollingCustomerServices(
        IDbContextFactory<AppDbContext> appDbContextFactory,
        IDbContextFactory<CustomerDbContext> customerDbContextFactory
    )
    {
        _appDbContextFactory = appDbContextFactory;
        _customerDbContextFactory = customerDbContextFactory;
    }

    public async Task<List<Customer>> FetchNewlyChangedEntities()
    {
        using var _appDbContext = await _appDbContextFactory.CreateDbContextAsync();
        using var _customerDbContext = await _customerDbContextFactory.CreateDbContextAsync();

        var currTable = _customerDbContext.GetTableName<Customer>();

        var currVer = await GetOrCreateVersionTracker(currTable, _appDbContext);

        return await GetChangedEntityFromLast(currVer, _customerDbContext);
    }

    private async Task<VersionTracker> GetOrCreateVersionTracker(
        string tableName,
        AppDbContext _appDbContext
    )
    {
        var currVer = await _appDbContext.VersionTrackers.FirstOrDefaultAsync(x =>
            x.TableName == tableName
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

            await _appDbContext.SaveChangesAsync();

            await _appDbContext.Entry(currVer).ReloadAsync();
        }

        return currVer;
    }

    private Task<List<Customer>> GetChangedEntityFromLast(
        VersionTracker currVer,
        CustomerDbContext _customerDbContext
    )
    {
        return _customerDbContext
            .Customers.Where(x => x.UpdatedOn > currVer.LastVersionTimestamp)
            .ToListAsync();
    }
}
