using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace TelegramMessager
{
    public class Program
    {
        private static DateTimeNow DateTimeNow = new DateTimeNow();

        static void Main(string[] args)
        {
            try
            {
                bool isFurstStart = true;
                List<People> peoples = new List<People>()
                {
                    new People(787471566),
                    //new People(961317657), //Владимир Викторович
                    //new People(1973965023) //Татьяна Владимировна
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
                    DateTimeNow.ChangeDateTime();
                    DateTime dateTimeNow = DateTimeNow.GetDateTimeNow();
                    TimeSpan startTime = new TimeSpan(dateTimeNow.Hour, dateTimeNow.Minute, dateTimeNow.Second);

                    var sw = new Stopwatch();
                    sw.Start();
                    Console.WriteLine("Время сейчас - " + dateTimeNow);
                    text = "";
                    countMas = 0;
                    countM3 = 0;

                    if (isFurstStart)
                    {
                        DateTime currentTime = dateTimeNow;
                        TimeSpan targetTime;

                        if (currentTime.TimeOfDay >= new TimeSpan(8,5,0) && currentTime.TimeOfDay < new TimeSpan(20,5,0) ) 
                        {
                            enumDateDayOrNight = EnumDayOrNight.Day;

                            var hour = currentTime.Hour;
                            var minut = currentTime.Minute;
                            var second = currentTime.Second;

                            targetTime =  new TimeSpan(20, 5, 0) - new TimeSpan(hour, minut, second);
                        }
                        else
                        {
                            enumDateDayOrNight = EnumDayOrNight.Night;

                            var hour = currentTime.Hour;
                            var minut = currentTime.Minute;
                            var second = currentTime.Second;

                            targetTime = new TimeSpan(8,5,0) - new TimeSpan(hour, minut, second);

                            hour = targetTime.Hours;
                            minut = targetTime.Minutes;
                            second = targetTime.Minutes;

                            if(hour < 0)
                            {
                                hour *= -1;
                            }

                            if (minut < 0)
                            {
                                hour *= -1;
                            }

                            if(second < 0)
                            {
                                second *= -1;
                            }

                            targetTime = new TimeSpan(hour, minut, second);
                        }

                        isFurstStart = false;

                        startTime = targetTime;
                        Console.WriteLine($"Ожидает до 8:05:00\nНадо прождать {startTime}");
                        Thread.Sleep(startTime);
                        continue;
                    }
                    else
                    {
                        startTime = TimeSpan.FromHours(12);
                    }
                        

                    if (enumDateDayOrNight == EnumDayOrNight.Night)
                    {
                        var getDataTask = database.GetDataNight();

                        if (getDataTask.Result == null)
                        {
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

                        for(int i = 0; i < datas.Count; i++)
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

                        text += $"\n\n               Информация за период\n" +
                            $"            с {mounts[0].GetFromDate().ToString("yyyy-MM-dd")} по {mounts[0].GetByDate().ToString("yyyy-MM-dd")}\n\n";

                        for (int i = 0; i < mounts.Count; i++)
                        {
                            text += $"{mounts[i].Text} / {mounts[i].Count} массива / {mounts[i].LongCount} м3\n";
                            countMountMas += mounts[i].Count;
                            countMountM3 += mounts[i].LongCount;
                        }

                        text += $"\n               Итоги за этот периуд\n {countMountMas} массива / {countMountM3} m3";
                        telegramBot.SendMessage(text);
                    }
                    else
                    {
                        var getDataTask = database.GetDataDay();

                        if (getDataTask.Result == null)
                        {
                            Console.WriteLine("Ошибка получения данных");
                            continue;
                        }

                        datas.AddRange(getDataTask.Result);

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
                    Console.WriteLine("Время окончания - " + dateTimeNow + "\nОжидать до следующего вызова " + startTime + "\n");
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
