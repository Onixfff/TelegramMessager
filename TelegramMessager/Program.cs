using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace TelegramMessager
{
    public class Program
    {
        private static DateTimeNow DateTimeNow = new DateTimeNow();
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        static void Main(string[] args)
        {
            logger.Trace("Инциализация программы !");

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
                        logger.Trace("Найден такой же процесс");
                    }
                }
            }

            logger.Trace($"onlyInstance = {onlyInstance}.");

            if (!onlyInstance)
            {
                logger.Error(new Exception("Ошибка повторного запуска программы"), "Найден такой-же процесс");
                Thread.Sleep(1000);
                return;
            }
            else
            {
                logger.Trace($"Запуск программы " + procName);

                try
                {
                    logger.Trace($"Начал проход по try в Program");

                    bool isFurstStart = true;
                    List<People> peoples = new List<People>()
                    {
                        new People(787471566),
                        new People(961317657), //Владимир Викторович
                        new People(1973965023), //Татьяна Владимировна
                        new People(805032669) // Артем Данишевский
                    };

                    EnumDayOrNight enumDateDayOrNight = EnumDayOrNight.Night;
                    Database database = new Database();
                    TelegramBot telegramBot = new TelegramBot(peoples);
                    List<Data> datas = new List<Data>();
                    logger.Trace($"Инициализирует list<Data>");
                    List<DataMount> mounts = new List<DataMount>();
                    logger.Trace($"Инициализирует list<DataMount>");
                    string text;
                    int countMas;
                    double countM3;

                    while (true)
                    {
                        logger.Trace($"Начало цикла While(true)");
                        DateTimeNow.ChangeDateTime();
                        DateTime dateTimeNow = DateTimeNow.GetDateTimeNow();
                        TimeSpan startTime = new TimeSpan(dateTimeNow.Hour, dateTimeNow.Minute, dateTimeNow.Second);

                        var sw = new Stopwatch();
                        logger.Trace($"Инициализирует сикундомер");
                        sw.Start();
                        logger.Trace($"Старт сикундомера || Время сейчас - " + dateTimeNow);
                        Console.WriteLine("Время сейчас - " + dateTimeNow);
                        text = "";
                        countMas = 0;
                        countM3 = 0;

                        if (isFurstStart)
                        {
                            logger.Trace($"Входит в isFurstStart = true запуск");
                            DateTime currentTime = dateTimeNow;
                            TimeSpan targetTime;

                            if (currentTime.TimeOfDay >= new TimeSpan(8, 5, 0) && currentTime.TimeOfDay < new TimeSpan(20, 5, 0))
                            {
                                logger.Trace($"Входит в if(currentTime.TimeOfDay >= new TimeSpan(8, 5, 0) && currentTime.TimeOfDay < new TimeSpan(20, 5, 0))");

                                enumDateDayOrNight = EnumDayOrNight.Day;
                                logger.Trace($"enumDateDayOrNight = {enumDateDayOrNight}");

                                var hour = currentTime.Hour;
                                var minut = currentTime.Minute;
                                var second = currentTime.Second;

                                targetTime = new TimeSpan(20, 5, 0) - new TimeSpan(hour, minut, second);
                                logger.Trace($"targetTime = {targetTime}");
                            }
                            else
                            {
                                logger.Trace($"не удовлетворяет условию if(currentTime.TimeOfDay >= new TimeSpan(8, 5, 0) && currentTime.TimeOfDay < new TimeSpan(20, 5, 0))");

                                enumDateDayOrNight = EnumDayOrNight.Night;
                                logger.Trace($"enumDateDayOrNight = {enumDateDayOrNight}");

                                var hour = currentTime.Hour;
                                var minut = currentTime.Minute;
                                var second = currentTime.Second;

                                targetTime = new TimeSpan(8, 5, 0) - new TimeSpan(hour, minut, second);

                                hour = targetTime.Hours;
                                minut = targetTime.Minutes;
                                second = targetTime.Minutes;

                                if (hour < 0)
                                {
                                    logger.Trace($"Hour < 0");
                                    hour *= -1;
                                }

                                if (minut < 0)
                                {
                                    logger.Trace($"minut < 0");

                                    hour *= -1;
                                }

                                if (second < 0)
                                {
                                    logger.Trace($"second < 0");
                                    second *= -1;
                                }

                                targetTime = new TimeSpan(hour, minut, second);
                                logger.Trace($"targetTime = {targetTime}");
                            }

                            isFurstStart = false;
                            logger.Trace($"isFurstStart = {isFurstStart}");

                            startTime = targetTime;

                            logger.Trace($"Ожидает до 8:05:00\nНадо прождать {startTime}");
                            Console.WriteLine($"Ожидает до 8:05:00\nНадо прождать {startTime}");
                            Thread.Sleep(startTime);
                            continue;
                        }
                        else
                        {
                            logger.Trace($"Ожижает 12 часо");
                            startTime = TimeSpan.FromHours(12);
                        }


                        if (enumDateDayOrNight == EnumDayOrNight.Night)
                        {
                            logger.Trace($"enumDateDayOrNight = EnumDayOrNight.Night");

                            var getDataTask = database.GetDataNight();
                            logger.Trace("Получил данные с database.GetDataNight");

                            if (getDataTask.Result == null)
                            {
                                logger.Error(new Exception("Не прошл проверку на null для getDataTask.Result"), "Ошибка получения данных");

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
                            
                            logger.Trace($"Вывод text для дня = ({text})");

                            double countMountMas = 0;
                            double countMountM3 = 0;

                            var getDataMountTask = database.GetMountData(enumDateDayOrNight);
                            logger.Trace($"Вызов getDataMountTask = database.GetMountData(enumDateDayOrNight); ");

                            getDataMountTask.GetAwaiter().GetResult();
                            logger.Trace($"Получение данных для getDataMountTask");

                            if (getDataMountTask.Result == null)
                            {
                                logger.Error(new Exception("Не прошл проверку на null для getDataMountTask.Result"), "Ошибка получения данных");
                                Console.WriteLine("Ошибка получения данных");
                                continue;
                            }
                            else
                            {
                                mounts.AddRange(getDataMountTask.Result);
                                logger.Trace($"Данные getDataMountTask.Result добавляются в список mounts");
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
                            
                            logger.Trace($"Данные Text для месяца = ({text})");

                            telegramBot.SendMessage(text);
                        }
                        else
                        {
                            logger.Trace($"enumDateDayOrNight = EnumDayOrNight.Day");

                            var getDataTask = database.GetDataDay();
                            logger.Trace("Получение данных для getDataTask");

                            if (getDataTask.Result == null)
                            {
                                logger.Error(new Exception("Не прошл проверку на null для getDataTask.Result"), "Ошибка получения данных");

                                Console.WriteLine("Ошибка получения данных");
                                continue;
                            }

                            datas.AddRange(getDataTask.Result);
                            logger.Trace("Добавляем данные getDataTask в List dates");

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

                            logger.Trace($"Готовый text дня ({text})");

                            telegramBot.SendMessage(text);
                        }

                        if (enumDateDayOrNight == EnumDayOrNight.Day)
                            enumDateDayOrNight = EnumDayOrNight.Night;
                        else
                            enumDateDayOrNight = EnumDayOrNight.Day;

                        logger.Trace($"Сменил enumDateDayOrNight на противоположный Теперь enumDateDayOrNight = {enumDateDayOrNight}");

                        Console.WriteLine(text);
                        datas.Clear();
                        mounts.Clear();

                        logger.Trace("Очистил datas и mounts");

                        sw.Stop();
                        logger.Trace("Выключил таймер");
                        startTime -= sw.Elapsed;
                        logger.Trace($"Вычислил время задержки = {sw.Elapsed}");
                        logger.Trace("Время окончания - " + dateTimeNow + "\nОжидать до следующего вызова " + startTime);
                        Console.WriteLine("Время окончания - " + dateTimeNow + "\nОжидать до следующего вызова " + startTime + "\n");
                        logger.Trace($"Уход в ожидание");
                        Thread.Sleep(startTime);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Ошибка программы");
                    Console.WriteLine(ex.Message);
                    return;
                }
            }
        }
    }
}
