using ChangeTracker.BaseServices;
using ChangeTracker.Jobs.BaseJobs;
using ChangeTracker.Services;
using ChangeTracker.Services.PollingServices;
using ChangeTracker.Services.TransformerServices;
using Microsoft.Extensions.Logging;
using Shared.Entities;
using Shared.Helpers;
using Shared.Messages;

namespace ChangeTracker.Jobs;

public class CustomerPollingBackgroundJob : BasePollingJob<Customer, CustomerMessage>
{
    private readonly ILogger<CustomerPollingBackgroundJob> _logger;
    private readonly CustomerPollingService _pollingService;
    private readonly CustomerTransformService _transformService;

    public CustomerPollingBackgroundJob(
        ILogger<CustomerPollingBackgroundJob> logger,
        CustomerPollingService pollingService,
        CustomerTransformService transformService,
        MessagePubliserService messagePublisherService
    )
        : base(logger, pollingService, transformService, messagePublisherService)
    {
        _logger = logger;
        _pollingService = pollingService;
        _transformService = transformService;
    }
}
