using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using Telegram.Bot.Types;

namespace TelegramMessager
{
    public class Program
    { 
        static void Main(string[] args)
        {
            try
            {
                TimeSpan startTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                bool isFurstStart = true;
                List<People> peoples = new List<People>()
                {
                    new People(787471566)
                    //new People(961317657)
                };

                EnumDayOrNight enumDateDayOrNight = EnumDayOrNight.Night;

                Database database = new Database();
                TelegramBot telegramBot = new TelegramBot(peoples);
                List<Data> datas = new List<Data>();
                List<DataMount> mounts = new List<DataMount>();
                string text;
                int countMas;
                double countM3;

                while (true)
                {
                    var sw = new Stopwatch();
                    text = "";
                    countMas = 0;
                    countM3 = 0;

                    if (isFurstStart)
                    {
                        DateTime currentTime = DateTime.Now;
                        DateTime targetTime;

                        targetTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 16, 38, 0);
                        //targetTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 8, 5, 0).AddDays(1);
                        enumDateDayOrNight = EnumDayOrNight.Night;
                        TimeSpan timeUntilTarget = targetTime - currentTime;
                        startTime = timeUntilTarget;
                        isFurstStart = false;
                        Console.WriteLine($"Ожидает до {targetTime}\nНадо прождать {startTime}");
                        Thread.Sleep(startTime);
                        continue;
                    }
                    else
                    {
                        startTime = TimeSpan.FromMinutes(1);
                        //startTime = TimeSpan.FromHours(12);
                        sw.Start();
                    }


                    var getDataTask = database.GetData();
                    getDataTask.GetAwaiter().GetResult();
                    
                    if (getDataTask.Result == null)
                    {
                        Console.WriteLine("Ошибка получения данных");
                        continue;
                    }

                    datas.AddRange(getDataTask.Result);

                    if (enumDateDayOrNight == EnumDayOrNight.Night)
                    {

                        text += "Ночь\n";

                        for (int i = 0; i < datas.Count; i++)
                        {
                            if (datas[i].IsDay == EnumDayOrNight.Night)
                            {
                                text += $"{datas[i].Text} / {datas[i].Count} массива / {datas[i].LongCount} м3";
                                countMas += datas[i].Count;
                                countM3 += datas[i].LongCount;
                            }
                        }

                        text += "\nДень\n";

                        for(int i = 0; i < datas.Count; i++)
                        {
                            if (datas[i].IsDay == EnumDayOrNight.Day)
                            {
                                text += $"{datas[i].Text} / {datas[i].Count} массива / {datas[i].LongCount} м3";
                                countMas += datas[i].Count;
                                countM3 += datas[i].LongCount;
                            }
                        }

                        text += $"\nИтого - {countMas} массива / {countM3} m3";
                        telegramBot.SendMessage(text);
                    }
                    else
                    {
                        text += "\nДень\n";

                        for (int i = 0; i < datas.Count; i++)
                        {
                            if (datas[i].IsDay == EnumDayOrNight.Day)
                            {
                                text += $"{datas[i].Text} / {datas[i].Count} массива / {datas[i].LongCount} м3";
                                countMas += datas[i].Count;
                                countM3 += datas[i].LongCount;
                            }
                        }

                        text += $"\nИтого - {countMas} массива / {countM3} m3";
                        telegramBot.SendMessage(text);

                        if (DateTime.Now.Day == telegramBot.GetLastDay())
                        {
                            var getDataMountTask = database.GetMountData();
                            getDataMountTask.GetAwaiter().GetResult();

                            if (getDataMountTask.Result == null)
                            {
                                Console.WriteLine("Ошибка получения данных");
                                continue;
                            }

                            countMas = 0;
                            countM3 = 0;

                            text = $"Информация за месяц с {mounts[0].GetFromDate()} по {mounts[0].GetByDate()}\n";

                            for (int i = 0; i < mounts.Count; i++)
                            {
                                    text += $"{mounts[i].Text} / {mounts[i].Count} массива / {mounts[i].LongCount} м3";
                                    countMas += mounts[i].Count;
                                    countM3 += mounts[i].LongCount;
                            }

                            text += $"\nИтого - {countMas} массива / {countM3} m3";
                            telegramBot.SendMessage(text);
                        }
                    }

                    if (enumDateDayOrNight == EnumDayOrNight.Day)
                        enumDateDayOrNight = EnumDayOrNight.Night;
                    else
                        enumDateDayOrNight = EnumDayOrNight.Day;

                    Console.WriteLine(text);
                    datas.Clear();
                    mounts.Clear();

                    if (isFurstStart)
                        isFurstStart = false;
                    sw.Stop();
                    startTime -= sw.Elapsed;
                    Console.WriteLine("Ожидать до следующего вызова " + startTime);
                    Thread.Sleep(startTime);
                    sw.Reset();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }
    }
}
