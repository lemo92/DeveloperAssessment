using Microsoft.EntityFrameworkCore;
using TaskTracker.Infrastructure;

namespace TaskTracker.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _services;

    public Worker(ILogger<Worker> logger, IServiceProvider services)
    {
        _logger = logger;
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var now = DateTime.UtcNow;
                var overdue = await db.Tasks
                    .Where(t => t.Status != TaskTracker.Domain.Entities.TaskStatus.Completed && t.DueDate < now)
                    .ToListAsync(stoppingToken);

                foreach (var t in overdue)
                {
                    t.Status = TaskTracker.Domain.Entities.TaskStatus.Overdue;
                }

                var count = await db.SaveChangesAsync(stoppingToken);
                if (count > 0)
                {
                    _logger.LogInformation("Marked {count} tasks as Overdue", count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while marking overdue tasks");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
