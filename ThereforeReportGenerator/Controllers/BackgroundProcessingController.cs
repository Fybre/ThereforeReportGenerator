using ThereforeReportGenerator.Context;
using ThereforeReportGenerator.Models;

namespace ThereforeReportGenerator.Controllers
{

    public class BackgroundProcessingController : IHostedService
    {

        private readonly int _interval;
        private readonly ILogger _logger;
        private System.Timers.Timer _timer;
        private readonly IServiceScopeFactory _scopeFactory;

        private int interval = 60000;
        public BackgroundProcessingController(ILogger<BackgroundProcessingController> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _interval = interval;
            _logger.LogInformation("Background controller created");
            _timer = new System.Timers.Timer(interval);
            _timer.Elapsed += Timer_Elapsed;
            _timer.AutoReset = true;
            _scopeFactory = serviceScopeFactory;
            StartupEvent();
        }

        /// <summary>
        /// Startup processing
        /// </summary>
        private void StartupEvent()
        {

        }

        private async void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            _logger.LogInformation("Timer elapsed");
            _timer.Stop();
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var _db = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
                    var _reportController = scope.ServiceProvider.GetRequiredService<ReportController>();
                    var mailConfig = _db.MailConfigs.FirstOrDefault()??new SMTPMailConfig();
                    if (!mailConfig.IsValid()) { throw new Exception("Invalid Mail Configuration"); }
                    var reportsToRun = _db.ReportConfigurations.Where(x => x.NextRun < DateTime.UtcNow && x.Enabled).ToList();
                    if (reportsToRun != null)
                    {
                        var reportTasks = new List<Task>();
                        foreach (var report in reportsToRun)
                        {
                            _logger.LogInformation($"Report {report.TenantName} running");

                            reportTasks.Add( _reportController.ProcessReport(report, mailConfig));
                            
                            //_db.ReportConfigurations.Find(report.Id)?.SetNextRun(); _db.SaveChanges();
                            _logger.LogInformation($"Set next run time to {report.NextRun}");
                        }
                        await Task.WhenAll(reportTasks);
                        _db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"EXCEPTION Timer Elapsed: {ex.Message}");
            }
            finally
            {
                _timer.Start();
            }
        }



        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Start called");
            _timer.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stop called");
            return Task.CompletedTask;
        }
    }
}
