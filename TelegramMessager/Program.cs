using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;

namespace TelegramMessager
{
    public class Program
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger();
        private static DateTimeNow DateTimeNow;

        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0].ToLower() == "/service")
            {
                // Запуск как служба Windows
                ServiceBase[] ServicesToRun = new ServiceBase[]
                {
                    new TelegramService()
                };
                ServiceBase.Run(ServicesToRun);
            }
            else
            {
                // Запуск как консольное приложение
                RunMainLogic();
            }
        }

        // Основная логика программы, вынесенная в отдельный метод
        public static async void RunMainLogic()
        {
            _logger.Trace("Инциализация программы !");

            DateTimeNow = new DateTimeNow(_logger);

            bool onlyInstance = true;
            string procName = Process.GetCurrentProcess().ProcessName;
            Properties.Settings.Default.procname = procName;
            int c = 0;

            Process[] processes = Process.GetProcesses();

            foreach (Process process in processes)
            {
                if (process.ProcessName.Contains(procName))
                {
                    c++;

                    if (c > 1)
                    {
                        onlyInstance = false;
                        _logger.Trace("Найден такой же процесс");
                    }
                }
            }

            _logger.Trace($"onlyInstance = {onlyInstance}.");

            if (!onlyInstance)
            {
                _logger.Error(new Exception("Ошибка повторного запуска программы"), "Найден такой-же процесс");
                Thread.Sleep(1000);
                return;
            }
            else
            {
                _logger.Trace($"Запуск программы " + procName);

                try
                {
                    _logger.Trace($"Начал проход по try в Program");

                    bool isFurstStart = true;

                    Peoples peoples = new Peoples();
                    EnumDayOrNight enumDateDayOrNight = EnumDayOrNight.Night;
                    Database database = await Database.CreateDbAsync(_logger);
                    TelegramBot telegramBot = new TelegramBot(peoples.GetListPeoples(), _logger);
                    List<Data> datas = new List<Data>();
                    _logger.Trace($"Инициализирует list<Data>");
                    List<DataMount> mounts = new List<DataMount>();
                    _logger.Trace($"Инициализирует list<DataMount>");
                    string text;
                    int countMas;
                    double countM3;

                    while (true)
                    {
                        _logger.Trace($"Начало цикла While(true)");
                        DateTimeNow.ChangeDateTime();
                        DateTime dateTimeNow = DateTimeNow.GetDateTimeNow();
                        TimeSpan startTime = new TimeSpan(dateTimeNow.Hour, dateTimeNow.Minute, dateTimeNow.Second);

                        var sw = new Stopwatch();
                        _logger.Trace($"Инициализирует сикундомер");
                        sw.Start();
                        _logger.Trace($"Старт сикундомера || Время сейчас - " + dateTimeNow);
                        Console.WriteLine("Время сейчас - " + dateTimeNow);
                        text = "";
                        countMas = 0;
                        countM3 = 0;

                        if (isFurstStart)
                        {
                            _logger.Trace($"Входит в isFurstStart = true запуск");
                            DateTime currentTime = dateTimeNow;
                            TimeSpan targetTime;

                            if (currentTime.TimeOfDay >= new TimeSpan(8, 5, 0) && currentTime.TimeOfDay < new TimeSpan(20, 5, 0))
                            {
                                _logger.Trace($"Входит в if(currentTime.TimeOfDay >= new TimeSpan(8, 5, 0) && currentTime.TimeOfDay < new TimeSpan(20, 5, 0))");

                                enumDateDayOrNight = EnumDayOrNight.Day;
                                _logger.Trace($"enumDateDayOrNight = {enumDateDayOrNight}");

                                var hour = currentTime.Hour;
                                var minut = currentTime.Minute;
                                var second = currentTime.Second;

                                targetTime = new TimeSpan(20, 5, 0) - new TimeSpan(hour, minut, second);
                                _logger.Trace($"targetTime = {targetTime}");
                            }
                            else
                            {
                                _logger.Trace($"не удовлетворяет условию if(currentTime.TimeOfDay >= new TimeSpan(8, 5, 0) && currentTime.TimeOfDay < new TimeSpan(20, 5, 0))");

                                enumDateDayOrNight = EnumDayOrNight.Night;
                                _logger.Trace($"enumDateDayOrNight = {enumDateDayOrNight}");

                                var hour = currentTime.Hour;
                                var minut = currentTime.Minute;
                                var second = currentTime.Second;

                                targetTime = new TimeSpan(8, 5, 0) - new TimeSpan(hour, minut, second);

                                hour = targetTime.Hours;
                                minut = targetTime.Minutes;
                                second = targetTime.Minutes;

                                if (hour < 0)
                                {
                                    _logger.Trace($"Hour < 0");
                                    hour *= -1;
                                }

                                if (minut < 0)
                                {
                                    _logger.Trace($"minut < 0");

                                    hour *= -1;
                                }

                                if (second < 0)
                                {
                                    _logger.Trace($"second < 0");
                                    second *= -1;
                                }

                                targetTime = new TimeSpan(hour, minut, second);
                                _logger.Trace($"targetTime = {targetTime}");
                            }

                            isFurstStart = false;
                            _logger.Trace($"isFurstStart = {isFurstStart}");

                            startTime = targetTime;

                            _logger.Trace($"Ожидает до {startTime} время сейчас {dateTimeNow}");
                            Console.WriteLine($"Ожидает до {startTime} время сейчас {dateTimeNow}");
                            Thread.Sleep(startTime);
                            continue;
                        }
                        else
                        {
                            _logger.Trace($"Ожижает 12 часо");
                            startTime = TimeSpan.FromHours(12);
                        }


                        if (enumDateDayOrNight == EnumDayOrNight.Night)
                        {
                            _logger.Trace($"enumDateDayOrNight = EnumDayOrNight.Night");

                            var getDataTask = database.GetDataNight();
                            _logger.Trace("Получил данные с database.GetDataNight");

                            if (getDataTask.Result == null)
                            {
                                _logger.Error(new Exception("Не прошл проверку на null для getDataTask.Result"), "Ошибка получения данных");

                                Console.WriteLine("Ошибка получения данных");
                                continue;
                            }

                            datas.AddRange(getDataTask.Result);

                            text += $"Дата {dateTimeNow.ToString("(yyyy-MM-dd) (HH:mm)")}";
                            text += "\n\n                                 Ночь\n";

                            for (int i = 0; i < datas.Count; i++)
                            {
                                if (datas[i].IsDay == EnumDayOrNight.Night)
                                {
                                    text += $"{datas[i].Text} / {datas[i].Count} массива / {datas[i].LongCount} м3\n";
                                    countMas += datas[i].Count;
                                    countM3 += datas[i].LongCount;
                                }
                            }

                            text += "\n                                 День\n";

                            for (int i = 0; i < datas.Count; i++)
                            {
                                if (datas[i].IsDay == EnumDayOrNight.Day)
                                {
                                    text += $"{datas[i].Text} / {datas[i].Count} массива / {datas[i].LongCount} м3\n";
                                    countMas += datas[i].Count;
                                    countM3 += datas[i].LongCount;
                                }
                            }

                            text += $"\n                                 Итоги\n" +
                                $"{countMas} массива / {countM3} m3";

                            _logger.Trace($"Вывод text для дня = ({text})");

                            double countMountMas = 0;
                            double countMountM3 = 0;

                            var getDataMountTask = database.GetMountData(enumDateDayOrNight);
                            _logger.Trace($"Вызов getDataMountTask = database.GetMountData(enumDateDayOrNight); ");

                            getDataMountTask.GetAwaiter().GetResult();
                            _logger.Trace($"Получение данных для getDataMountTask");

                            if (getDataMountTask.Result == null)
                            {
                                _logger.Error(new Exception("Не прошл проверку на null для getDataMountTask.Result"), "Ошибка получения данных");
                                Console.WriteLine("Ошибка получения данных");
                                continue;
                            }
                            else
                            {
                                mounts.AddRange(getDataMountTask.Result);
                                _logger.Trace($"Данные getDataMountTask.Result добавляются в список mounts");
                            }

                            text += $"\n\n               Информация за период\n" +
                                $"            с {mounts[0].GetFromDate().ToString("yyyy-MM-dd")} по {mounts[0].GetByDate().ToString("yyyy-MM-dd")}\n\n";

                            for (int i = 0; i < mounts.Count; i++)
                            {
                                text += $"{mounts[i].Text} / {mounts[i].Count} массива / {mounts[i].LongCount} м3\n";
                                countMountMas += mounts[i].Count;
                                countMountM3 += mounts[i].LongCount;
                            }

                            text += $"\n               Итоги за этот периуд\n {countMountMas} массива / {countMountM3} m3";

                            _logger.Trace($"Данные Text для месяца = ({text})");

                            telegramBot.SendMessage(text);
                        }
                        else
                        {
                            _logger.Trace($"enumDateDayOrNight = EnumDayOrNight.Day");

                            var getDataTask = database.GetDataDay();
                            _logger.Trace("Получение данных для getDataTask");

                            if (getDataTask.Result == null)
                            {
                                _logger.Error(new Exception("Не прошл проверку на null для getDataTask.Result"), "Ошибка получения данных");

                                Console.WriteLine("Ошибка получения данных");
                                continue;
                            }

                            datas.AddRange(getDataTask.Result);
                            _logger.Trace("Добавляем данные getDataTask в List dates");

                            text += $"Дата {dateTimeNow.ToString("(yyyy-MM-dd) (HH:mm)")}";
                            text += "\n\n                                 День\n";

                            for (int i = 0; i < datas.Count; i++)
                            {
                                if (datas[i].IsDay == EnumDayOrNight.Day)
                                {
                                    text += $"{datas[i].Text} / {datas[i].Count} массива / {datas[i].LongCount} м3\n";
                                    countMas += datas[i].Count;
                                    countM3 += datas[i].LongCount;
                                }
                            }

                            text += $"\n                                 Итого\n" +
                                $"{countMas} массива / {countM3} m3";

                            _logger.Trace($"Готовый text дня ({text})");

                            telegramBot.SendMessage(text);
                        }

                        if (enumDateDayOrNight == EnumDayOrNight.Day)
                            enumDateDayOrNight = EnumDayOrNight.Night;
                        else
                            enumDateDayOrNight = EnumDayOrNight.Day;

                        _logger.Trace($"Сменил enumDateDayOrNight на противоположный Теперь enumDateDayOrNight = {enumDateDayOrNight}");

                        Console.WriteLine(text);
                        datas.Clear();
                        mounts.Clear();

                        _logger.Trace("Очистил datas и mounts");

                        sw.Stop();
                        _logger.Trace("Выключил таймер");
                        startTime -= sw.Elapsed;
                        _logger.Trace($"Вычислил время задержки = {sw.Elapsed}");
                        _logger.Trace("Время окончания - " + dateTimeNow + "\nОжидать до следующего вызова " + startTime);
                        Console.WriteLine("Время окончания - " + dateTimeNow + "\nОжидать до следующего вызова " + startTime + "\n");
                        _logger.Trace($"Уход в ожидание");
                        Thread.Sleep(startTime);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Ошибка программы");
                    Console.WriteLine(ex.Message);
                    return;
                }
            }
        }
    }
}
