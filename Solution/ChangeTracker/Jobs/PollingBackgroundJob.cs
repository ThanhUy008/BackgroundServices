using ChangeTracker.Services;
using Microsoft.Extensions.Logging;

namespace ChangeTracker.Jobs;

public class PollingBackgroundJob : BackgroundService
{
    private readonly ILogger<PollingBackgroundJob> _logger;

    private readonly PollingCustomerServices _pollingService;

    public PollingBackgroundJob(
        ILogger<PollingBackgroundJob> logger,
        PollingCustomerServices pollingService
    )
    {
        _logger = logger;
        _pollingService = pollingService;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Polling start");

        while (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Polling loop start");

            try
            {
                var newlyChangedEntities = await _pollingService.FetchNewlyChangedEntities();

                foreach (var entity in newlyChangedEntities)
                {
                    Console.WriteLine(entity.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while looping PollingBackGroundJob");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
        }

        _logger.LogInformation("PollingBackgroundJob stopped completely.");
    }
}
