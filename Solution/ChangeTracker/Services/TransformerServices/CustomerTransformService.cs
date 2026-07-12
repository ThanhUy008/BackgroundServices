using System;
using System.Collections.Generic;
using System.Linq;
using ChangeTracker.BaseServices;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.DbContexts;
using Shared.Entities;
using Shared.Messages;
using Shared.VersionTrackerEntities;

namespace ChangeTracker.Services.TransformerServices;

public class CustomerTransformService : BaseTransformService<Customer, CustomerMessage>
{
    private IDbContextFactory<CustomerDbContext> _customerDbContextFactory;

    public CustomerTransformService(IDbContextFactory<CustomerDbContext> customerDbContextFactory)
        : base(customerDbContextFactory)
    {
        _customerDbContextFactory = customerDbContextFactory;
    }

    public override async Task<CustomerMessage> Map(
        Customer customer,
        CustomerDbContext customerDbContext,
        CancellationToken cancellationToken = default
    )
    {
        var items = await customerDbContext.Items.Take(20).ToListAsync(cancellationToken);
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
