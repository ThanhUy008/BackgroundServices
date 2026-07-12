using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.DbContexts;
using Shared.Entities;
using Shared.Messages;
using Shared.VersionTrackerEntities;

namespace ChangeTracker.BaseServices;

public abstract class BaseTransformService<TEntity, TMessage>
    where TEntity : ITrackable
    where TMessage : class
{
    private readonly IDbContextFactory<CustomerDbContext> _customerDbContextFactory;

    public BaseTransformService(IDbContextFactory<CustomerDbContext> customerDbContextFactory)
    {
        _customerDbContextFactory = customerDbContextFactory;
    }

    public async Task<List<TMessage>> TransformChangedEntities(
        IEnumerable<TEntity> changedEntities,
        CancellationToken cancellationToken = default
    )
    {
        var messages = new List<TMessage>();

        using var _customerDbContext = await _customerDbContextFactory.CreateDbContextAsync(
            cancellationToken
        );

        foreach (var entity in changedEntities)
        {
            var mappedMessage = await Map(entity, _customerDbContext, cancellationToken);
            if (mappedMessage != null)
            {
                messages.Add(mappedMessage);
            }
        }

        return messages;
    }

    //This to kill items immediatly after fetch
    public abstract Task<TMessage> Map(
        TEntity entity,
        CustomerDbContext customerDbContext,
        CancellationToken cancellationToken = default
    );
}
