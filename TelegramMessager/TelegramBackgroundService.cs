using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TelegramMessager
{
    public class TelegramBackgroundService : BackgroundService
    {
        private readonly ILogger<TelegramBackgroundService> _logger;

        public TelegramBackgroundService(ILogger<TelegramBackgroundService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Служба TelegramMessager запущена");
                
                // Запускаем основную логику в отдельном потоке
                await Task.Run(() => 
                {
                    Program.RunMainLogic();
                }, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в работе службы TelegramMessager");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Служба TelegramMessager останавливается...");
            await base.StopAsync(cancellationToken);
        }
    }
} 