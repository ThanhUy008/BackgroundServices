using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.DbContexts;
using Shared.Entities;
using Shared.Messages;
using Shared.VersionTrackerEntities;

namespace ChangeTracker.Services;

public class TransformService
{
    private IDbContextFactory<CustomerDbContext> _customerDbContextFactory;

    public TransformService(IDbContextFactory<CustomerDbContext> customerDbContextFactory)
    {
        _customerDbContextFactory = customerDbContextFactory;
    }

    public async Task<List<CustomerMessage>> TransformChangedEntities(
        IEnumerable<Customer> changedEntities
    )
    {
        var customerMessages = new List<CustomerMessage>();

        using var _customerDbContext = await _customerDbContextFactory.CreateDbContextAsync();

        foreach (var customer in changedEntities)
        {
            customerMessages.Add(await Map(customer, _customerDbContext));
        }

        return customerMessages;
    }

    //This to kill items immediatly after fetch
    private async Task<CustomerMessage> Map(Customer customer, CustomerDbContext customerDbContext)
    {
        var items = await customerDbContext.Items.ToListAsync();
        return new CustomerMessage
        {
            Id = customer.Id,
            Name = customer.Name,
            Description = customer.Phone,
            CreatedOn = customer.CreatedOn,
            Items = items
                .Select(item => new
                {
                    item.Id,
                    item.Name,
                    item.Description,
                })
                .ToArray(),
        };
    }
}
