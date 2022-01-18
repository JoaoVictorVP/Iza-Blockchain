using IzaBlockchain.Final;
using IzaBlockchain.Net;

namespace IzaBlockchain.Daemon
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Run Daemon Host
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            // Listen for network feedbacks
            NetworkFeedback.ListenFeedbacks(feedback =>
            {
                switch (feedback.Type)
                {
                    case NetworkFeedback.FeedbackType.Info:
                        _logger.LogInformation(feedback.Message);
                        break;
                    case NetworkFeedback.FeedbackType.Warning:
                        _logger.LogWarning(feedback.Message);
                        break;
                    case NetworkFeedback.FeedbackType.Error:
                        _logger.LogError(feedback.Message);
                        break;
                }
            });

            // Do Run IZA Blockchain
            MainClass.Run();

            return base.StartAsync(cancellationToken);
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            // Do Stop IZA Blockchain
            MainClass.Stop();

            return base.StopAsync(cancellationToken);
        }
    }
}