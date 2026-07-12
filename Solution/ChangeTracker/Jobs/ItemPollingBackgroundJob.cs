using ChangeTracker.Jobs.BaseJobs;
using ChangeTracker.Services;
using ChangeTracker.Services.PollingServices;
using ChangeTracker.Services.TransformerServices;
using Microsoft.Extensions.Logging;
using Shared.Entities;
using Shared.Helpers;
using Shared.Messages;

namespace ChangeTracker.Jobs;

public class ItemPollingBackgroundJob : BasePollingJob<Item, ItemMessage>
{
    private readonly ILogger<ItemPollingBackgroundJob> _logger;
    private readonly ItemPollingService _pollingService;
    private readonly ItemTransformService _transformService;

    public ItemPollingBackgroundJob(
        ILogger<ItemPollingBackgroundJob> logger,
        ItemPollingService pollingService,
        ItemTransformService transformService
    )
        : base(logger, pollingService, transformService)
    {
        _logger = logger;
        _pollingService = pollingService;
        _transformService = transformService;
    }
}
