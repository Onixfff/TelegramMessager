using System.ServiceProcess;
using NLog;
using System.IO;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TelegramMessager
{
    public class TelegramService : ServiceBase
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private CancellationTokenSource _cts;
        private Task _mainTask;

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

            _cts = new CancellationTokenSource();
            _mainTask = Program.RunMainLogic(_cts.Token);

            _logger.Info("Служба успешно запущена");
        }

        protected override void OnStop()
        {
            _logger.Info("Служба останавливается...");

            // Запрашиваем отмену
            _cts?.Cancel();

            // Ждём завершения основной задачи (но не вечно)
            try
            {
                // Даём 30 секунд на graceful shutdown (максимум для службы Windows)
                _mainTask?.Wait(TimeSpan.FromSeconds(30));
            }
            catch (AggregateException ex)
            {
                _logger.Warn(ex, "Исключения при ожидании завершения задачи");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Неожиданная ошибка при остановке");
            }
            finally
            {
                _cts?.Dispose();
            }

            _logger.Info("Служба остановлена");
        }
    }
}