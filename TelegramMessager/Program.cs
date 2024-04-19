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
                    //new People(961317657) Владимир Викторович
                    //new People(1973965023) Татьяна Владимировна
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
                    sw.Start();
                    Console.WriteLine("Время сейчас - " + DateTime.Now);
                    text = "";
                    countMas = 0;
                    countM3 = 0;

                    if (isFurstStart)
                    {
                        DateTime currentTime = DateTime.Now;
                        DateTime targetTime;

                        targetTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 17, 57, 0);
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
                    }


                    var getDataTask = database.GetData();
                    
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

                        double countMountMas = 0;
                        double countMountM3 = 0;

                        var getDataMountTask = database.GetMountData();
                        getDataMountTask.GetAwaiter().GetResult();

                        if (getDataMountTask.Result == null)
                        {
                            Console.WriteLine("Ошибка получения данных");
                            continue;
                        }
                        else
                        {
                            mounts.AddRange(getDataMountTask.Result);
                        }

                        text += $"\n\nИнформация за месяц с {mounts[0].GetFromDate().ToString("yyyy-MM-dd")} по {mounts[0].GetByDate().ToString("yyyy-MM-dd")}\n";

                        for (int i = 0; i < mounts.Count; i++)
                        {
                            text += $"{mounts[i].Text} / {mounts[i].Count} массива / {mounts[i].LongCount} м3";
                            countMountMas += mounts[i].Count;
                            countMountM3 += mounts[i].LongCount;
                        }

                        text += $"\nИтого за месяц - {countMountMas} массива / {countMountM3} m3";
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
                    }

                    if (enumDateDayOrNight == EnumDayOrNight.Day)
                        enumDateDayOrNight = EnumDayOrNight.Night;
                    else
                        enumDateDayOrNight = EnumDayOrNight.Day;

                    Console.WriteLine(text);
                    datas.Clear();
                    mounts.Clear();
                    sw.Stop();
                    startTime -= sw.Elapsed;
                    Console.WriteLine("Время окончания - " +DateTime.Now+ "\nОжидать до следующего вызова " + startTime + "\n");
                    Thread.Sleep(startTime);
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
