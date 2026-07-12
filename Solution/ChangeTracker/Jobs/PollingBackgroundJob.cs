using ChangeTracker.Services;
using Microsoft.Extensions.Logging;
using Shared.Helpers;
using Shared.Messages;

namespace ChangeTracker.Jobs;

public class PollingBackgroundJob : BackgroundService
{
    private readonly ILogger<PollingBackgroundJob> _logger;
    private readonly PollingCustomerServices _pollingService;
    private readonly TransformService _transformService;

    public PollingBackgroundJob(
        ILogger<PollingBackgroundJob> logger,
        PollingCustomerServices pollingService,
        TransformService transformService
    )
    {
        _logger = logger;
        _pollingService = pollingService;
        _transformService = transformService;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Polling start");

        while (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Polling loop start");

            try
            {
                var messages = await FetchAndTransform();

                foreach (var message in messages)
                {
                    // _logger.LogInformation(
                    //     $"Processing customer: {message.Name} {message.Description} {message.Items.Length} items"
                    // );
                }
                // _logger.LogWarning("Polling loop end");
                _logger.LogWarning("Generation of message: {0} of", GC.GetGeneration(messages));

                GCLogger.Log(_logger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while looping PollingBackGroundJob");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
        }

        _logger.LogInformation("PollingBackgroundJob stopped completely.");
    }

    private async Task<List<CustomerMessage>> FetchAndTransform()
    {
        var newlyChangedEntities = await _pollingService.FetchNewlyChangedEntities();

        var temp = await _transformService.TransformChangedEntities(newlyChangedEntities);

        _logger.LogWarning("Fetched and transformed {Count} changed entities", temp.Count);
        // GCLogger.Log(_logger);
        _logger.LogWarning("Generation of temp: {0}", GC.GetGeneration(temp));
        return temp;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cleaning up resources before closing...");
        await base.StopAsync(cancellationToken);
    }
}
