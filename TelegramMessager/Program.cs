using Google.Protobuf.WellKnownTypes;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TelegramMessager
{
    public class Program
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public static async Task Main(string[] args)
        {
            // Защита от дубликатов процесса
            if (IsAnotherInstanceRunning())
            {
                Console.WriteLine("Обнаружен другой запущенный экземпляр. Завершение.");
                return;
            }

            if (args.Length > 0)
            {
                var arg = args[0];

                if (arg.Equals("/service", StringComparison.OrdinalIgnoreCase))
                {
                    var servicesToRun = new ServiceBase[] { new TelegramService() };
                    ServiceBase.Run(servicesToRun);
                    return;
                }

                // Поддержка: /test:day  или  /test:night
                if (arg.StartsWith("/test:", StringComparison.OrdinalIgnoreCase))
                {
                    string periodPart = arg.Substring("/test:".Length).Trim();
                    if (TryParsePeriod(periodPart, out EnumDayOrNight period))
                    {
                        _logger.Info($"Запуск в ТЕСТОВОМ режиме для периода: {period}");
                        await RunTestMode(period);
                        return;
                    }
                    else
                    {
                        _logger.Error($"Неверный формат тестового аргумента. Используйте: /test:day или /test:night");
                        Console.WriteLine("Ошибка: неверный формат. Используйте /test:day или /test:night");
                        return;
                    }
                }

                // Если просто "/test" — определяем период автоматически
                if (arg.Equals("/test", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.Info("Запуск в ТЕСТОВОМ режиме (автоопределение периода)");
                    var autoPeriod = IsDayTime(DateTime.Now) ? EnumDayOrNight.Day : EnumDayOrNight.Night;
                    await RunTestMode(autoPeriod);
                    return;
                }
            }
            else
            {
                var cts = new CancellationTokenSource();
                try
                {
                    Console.CancelKeyPress += (s, e) =>
                    {
                        _logger.Info("Получен сигнал завершения (Ctrl+C). Завершаем работу...");
                        e.Cancel = true; // предотвращает немедленное завершение
                        cts.Cancel();
                    };

                    await RunMainLogic(cts.Token);
                }
                finally
                {
                    cts.Dispose();
                }
            }
        }

        private static bool TryParsePeriod(string input, out EnumDayOrNight period)
        {
            period = EnumDayOrNight.Night; // значение по умолчанию

            if (string.IsNullOrWhiteSpace(input))
                return false;

            string clean = input.Trim().ToLowerInvariant();

            if (clean == "day" || clean == "день")
            {
                period = EnumDayOrNight.Day;
                return true;
            }
            if (clean == "night" || clean == "ночь")
            {
                period = EnumDayOrNight.Night;
                return true;
            }

            return false;
        }

        public static async Task RunMainLogic(CancellationToken cancellationToken)
        {
            // Защита от дубликатов процесса
            if (IsAnotherInstanceRunning())
            {
                _logger.Warn("Обнаружен другой запущенный экземпляр. Завершение.");
                return;
            }

            _logger.Info("Запуск основной логики Telegram-бота");

            try
            {
                var dateTimeProvider = new DateTimeNow(_logger);
                var peoples = new Peoples();
                var database = await Database.CreateDbAsync(_logger);
                var telegramBot = new TelegramBot(peoples.GetListPeoples(), _logger);

                // Определяем начальный период
                var now = dateTimeProvider.GetDateTimeNow();
                var currentPeriod = IsDayTime(now) ? EnumDayOrNight.Day : EnumDayOrNight.Night;
                _logger.Info($"Начальный период: {currentPeriod}");

                // Ждём до ближайшей границы (08:05 или 20:05)
                var initialDelay = CalculateInitialDelay(now);
                _logger.Info($"Первичная задержка: {initialDelay}");
                await Task.Delay(initialDelay, cancellationToken);
                
                //Инициализируем таймер (он будет гасить разницу во времени)
                Stopwatch stopwatch = new Stopwatch();

                while (!cancellationToken.IsCancellationRequested)
                {
                    stopwatch.Restart();

                    now = dateTimeProvider.GetDateTimeNow();
                    _logger.Info($"Формирование отчёта за период: {currentPeriod} ({now:yyyy-MM-dd HH:mm})");

                    try
                    {
                        string reportText;
                        if (currentPeriod == EnumDayOrNight.Night)
                        {
                            reportText = await BuildNightReportAsync(database, now, _logger, cancellationToken);
                        }
                        else
                        {
                            reportText = await BuildDayReportAsync(database, now, _logger, cancellationToken);
                        }

                        await telegramBot.SendMessageAsync(reportText);
                        Console.WriteLine(reportText);
                        _logger.Info("Отчёт успешно отправлен");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, $"Ошибка при формировании или отправке отчёта за {currentPeriod}");
                    }

                    // Переключаем период
                    currentPeriod = currentPeriod == EnumDayOrNight.Day
                        ? EnumDayOrNight.Night
                        : EnumDayOrNight.Day;

                    stopwatch.Stop();

                    TimeSpan timeSleep = new TimeSpan(12, 0, 0);

                    timeSleep -= stopwatch.Elapsed;

                    // Ждём 12 часов до следующего отчёта
                    await Task.Delay(timeSleep, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Info("Работа программы прервана по запросу отмены.");
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "Критическая ошибка в основном цикле");
            }
        }

        private static async Task RunTestMode(EnumDayOrNight currentPeriod)
        {
            try
            {
                var logger = _logger;
                logger.Info("Инициализация для тестового режима...");

                var peoples = new Peoples();
                var database = await Database.CreateDbAsync(logger);
                var telegramBot = new TelegramBot(peoples.GetListPeoples(), logger);
                var now = DateTime.Now;

                // Определяем текущий период (день/ночь) на основе реального времени
                logger.Info($"Тестовый отчёт за период: {currentPeriod}");

                string reportText;
                if (currentPeriod == EnumDayOrNight.Night)
                {
                    reportText = await BuildNightReportAsync(database, now, logger, CancellationToken.None);
                }
                else
                {
                    reportText = await BuildDayReportAsync(database, now, logger, CancellationToken.None);
                }

                // Выводим в консоль (не отправляем в Telegram, если не хотите)

                string fullReportText = $"" +
                    $"\n=== ТЕСТОВЫЙ ОТЧЁТ ===\n" +
                    $"{reportText}" +
                    $"\n=== КОНЕЦ ОТЧЁТА ===";

                await telegramBot.SendMessageAsync(reportText);
                Console.WriteLine(reportText);

                logger.Info("Тестовый отчёт успешно сформирован и выведен в консоль.");
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "Ошибка в тестовом режиме");
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        private static bool IsAnotherInstanceRunning()
        {
            var current = Process.GetCurrentProcess();
            var processes = Process.GetProcessesByName(current.ProcessName);

            // Считаем, сколько экземпляров уже запущено
            int count = processes.Count(p =>
                p.Id != current.Id &&
                p.MainModule?.FileName == current.MainModule?.FileName);

            foreach (var p in processes.Where(p => p.Id != current.Id))
            {
                p.Dispose();
            }

            return count > 0;
        }

        private static bool IsDayTime(DateTime dt) =>
            dt.TimeOfDay >= new TimeSpan(8, 5, 0) && dt.TimeOfDay < new TimeSpan(20, 5, 0);

        private static TimeSpan CalculateInitialDelay(DateTime now)
        {
            if (IsDayTime(now))
            {
                // Сейчас день → ждём до 20:05
                var target = now.Date.Add(new TimeSpan(20, 5, 0));
                return target > now ? target - now : TimeSpan.Zero;
            }
            else
            {
                // Сейчас ночь → ждём до 08:05 следующего дня
                var target = now.Date.AddDays(1).Add(new TimeSpan(8, 5, 0));
                return target - now;
            }
        }

        private static async Task<string> BuildDayReportAsync(Database db, DateTime now, ILogger logger, CancellationToken ct)
        {
            logger.Trace("Построение дневного отчёта");
            var data = await db.GetDataDay(ct);
            if (data == null)
                throw new InvalidOperationException("GetDataDay вернул null");

            return FormatReport(data, now, includeNight: false);
        }

        private static async Task<string> BuildNightReportAsync(Database db, DateTime now, ILogger logger, CancellationToken ct)
        {
            logger.Trace("Построение ночного отчёта");
            var nightData = await db.GetDataNight(ct);
            var dayData = await db.GetDataDay(ct);

            if (nightData == null || dayData == null)
                throw new InvalidOperationException("GetDataNight или GetDataDay вернули null");

            var allData = nightData.Concat(dayData).ToList();
            var mainReport = FormatReport(allData, now, includeNight: true);

            var mounts = await db.GetMountData(EnumDayOrNight.Night, ct);
            if (mounts == null || !mounts.Any())
                throw new InvalidOperationException("GetMountData вернул null или пустой список");

            var periodInfo = mounts[0];
            var mountReport = $"\n\n               Информация за период\n" +
                              $"            с {periodInfo.GetFromDate():yyyy-MM-dd} по {periodInfo.GetByDate():yyyy-MM-dd}\n\n";

            double totalMas = 0, totalM3 = 0;
            foreach (var m in mounts)
            {
                mountReport += $"{m.Text} / {m.Count} массива / {m.LongCount} м3\n";
                totalMas += m.Count;
                totalM3 += m.LongCount;
            }

            mountReport += $"\n               Итоги за этот период\n {totalMas} массива / {totalM3} m3";

            return mainReport + mountReport;
        }

        private static string FormatReport(List<Data> data, DateTime now, bool includeNight)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Дата ({now:yyyy-MM-dd HH:mm})");

            int totalMas = 0;
            double totalM3 = 0.0;

            if (includeNight)
            {
                var nightEntries = data.Where(d => d.IsDay == EnumDayOrNight.Night).ToList();
                if (nightEntries.Any())
                {
                    sb.AppendLine("\n                                 Ночь");
                    foreach (var d in nightEntries)
                    {
                        sb.AppendLine($"{d.Text} / {d.Count} массива / {d.LongCount} м3");
                        totalMas += d.Count;
                        totalM3 += d.LongCount;
                    }
                }
            }

            var dayEntries = data.Where(d => d.IsDay == EnumDayOrNight.Day).ToList();
            if (dayEntries.Any())
            {
                sb.AppendLine(includeNight ? "\n                                 День" : "\n\n                                 День");
                foreach (var d in dayEntries)
                {
                    sb.AppendLine($"{d.Text} / {d.Count} массива / {d.LongCount} м3");
                    totalMas += d.Count;
                    totalM3 += d.LongCount;
                }
            }

            sb.AppendLine($"\n                                 Итоги");
            sb.AppendLine($"{totalMas} массива / {totalM3:F2} m3");

            return sb.ToString();
        }
    }
}