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

public class ItemTransformService : BaseTransformService<Item, ItemMessage>
{
    private IDbContextFactory<CustomerDbContext> _customerDbContextFactory;

    public ItemTransformService(IDbContextFactory<CustomerDbContext> customerDbContextFactory)
        : base(customerDbContextFactory)
    {
        _customerDbContextFactory = customerDbContextFactory;
    }

    public override Task<ItemMessage> Map(
        Item item,
        CustomerDbContext customerDbContext,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(
            new ItemMessage
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                CreatedOn = item.CreatedOn,
            }
        );
    }
}
