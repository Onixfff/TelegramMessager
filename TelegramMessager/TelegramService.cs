using System.ServiceProcess;
using NLog;
using System.IO;
using System;

namespace TelegramMessager
{
    public class TelegramService : ServiceBase
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger();
        private TelegramBot _bot;

        public TelegramService()
        {
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BatFiles", "config.ini");
                if (File.Exists(configPath))
                {
                    foreach (string line in File.ReadAllLines(configPath))
                    {
                        if (!line.StartsWith(";") && line.Contains("ServiceName="))
                        {
                            ServiceName = line.Split('=')[1].Trim();
                            _logger.Info($"Имя службы загружено из config.ini: {ServiceName}");
                            break;
                        }
                    }
                }
                else
                {
                    ServiceName = "TelegramMessager";
                    _logger.Warn("config.ini не найден, используется имя службы по умолчанию: TelegramMessager");
                }
            }
            catch (Exception ex)
            {
                ServiceName = "TelegramMessager";
                _logger.Error(ex, "Ошибка при чтении config.ini, используется имя службы по умолчанию: TelegramMessager");
            }
        }

        protected override void OnStart(string[] args)
        {
            _logger.Info("Служба запускается...");
            // Запускаем основную логику в отдельном потоке
            System.Threading.Tasks.Task.Run(() => Program.RunMainLogic());
            _logger.Info("Служба запущена");
        }

        protected override void OnStop()
        {
            _logger.Info("Служба останавливается...");
            // Здесь код для корректного завершения работы
            if (_bot != null)
            {
                _bot.Dispose();
            }
            _logger.Info("Служба остановлена");
        }
    }
}