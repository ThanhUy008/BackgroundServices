using ChangeTracker.BaseServices;
using ChangeTracker.Services;
using ChangeTracker.Services.BaseServices;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Entities;
using Shared.Helpers;
using Shared.Messages;

namespace ChangeTracker.Jobs.BaseJobs;

public abstract class BasePollingJob<TEntity, TMessage> : BackgroundService
    where TEntity : ITrackable
    where TMessage : class
{
    private readonly ILogger<BasePollingJob<TEntity, TMessage>> _logger;
    private readonly BasePollingService<TEntity> _pollingService;
    private readonly BaseTransformService<TEntity, TMessage> _transformService;

    public BasePollingJob(
        ILogger<BasePollingJob<TEntity, TMessage>> logger,
        BasePollingService<TEntity> pollingService,
        BaseTransformService<TEntity, TMessage> transformService
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
                var (messages, latestUpdatedOn) = await FetchAndTransform();

                if (messages.Count == 0)
                {
                    _logger.LogWarning("No changed entities found");
                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                    continue;
                }

                foreach (var message in messages)
                {
                    // _logger.LogInformation(
                    //     $"Processing customer: {message.Name} {message.Description} {message.Items.Length} items"
                    // );
                }
                // _logger.LogWarning("Polling loop end");
                _logger.LogWarning("Generation of message: {0} of", GC.GetGeneration(messages));

                GCLogger.Log(_logger);

                await _pollingService.UpdateVersionTracker(latestUpdatedOn, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while looping PollingBackGroundJob");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
        }

        _logger.LogInformation("PollingBackgroundJob stopped completely.");
    }

    public async Task<(List<TMessage>, DateTime)> FetchAndTransform(
        CancellationToken cancellationToken = default
    )
    {
        var newlyChangedEntities = await _pollingService.FetchNewlyChangedEntities();

        if (newlyChangedEntities.Count == 0)
        {
            _logger.LogInformation("No changed entities found");
            return (new List<TMessage>(), DateTime.MinValue);
        }

        var temp = await _transformService.TransformChangedEntities(
            newlyChangedEntities,
            cancellationToken
        );

        _logger.LogWarning("Fetched and transformed {Count} changed entities", temp.Count);
        _logger.LogWarning("Generation of temp: {0}", GC.GetGeneration(temp));
        return (temp, newlyChangedEntities.Max(x => x.UpdatedOn));
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cleaning up resources before closing...");
        await base.StopAsync(cancellationToken);
    }
}
