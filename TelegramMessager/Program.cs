using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using Telegram.Bot;
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
                bool isFurstStart = false;
                List<People> peoples = new List<People>()
                {
                    new People(787471566)
                };

                EnumDayOrNight enumDateDayOrNight = EnumDayOrNight.Night;

                Database database = new Database();
                TelegramBot telegramBot = new TelegramBot(peoples);
                List<Data> datas = new List<Data>();
                string text;
                int countMas;
                double countM3;

                while (true)
                {
                    text = "";
                    countMas = 0;
                    countM3 = 0;

                    if (isFurstStart)
                    {
                        DateTime currentTime = DateTime.Now;
                        DateTime targetTime;

                        targetTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 8, 5, 0).AddDays(1);
                        enumDateDayOrNight = EnumDayOrNight.Night;
                        TimeSpan timeUntilTarget = targetTime - currentTime;
                        startTime = timeUntilTarget;
                        isFurstStart = false;
                        Thread.Sleep(startTime);
                        continue;
                    }
                    else
                    {
                        startTime = TimeSpan.FromHours(12);
                    }

                    if (enumDateDayOrNight == EnumDayOrNight.Night)
                    {
                        var getDataTask = database.GetData();
                        getDataTask.GetAwaiter().GetResult();

                        if (getDataTask.Result == null)
                        {
                            Console.WriteLine("Ошибка получения данных");
                            continue;
                        }

                        datas.AddRange(getDataTask.Result);

                        text += "Ночь\n";

                        for (int i = 0; i < datas.Count; i++)
                        {
                            if (datas[0].IsDay == EnumDayOrNight.Night)
                            {
                                text += $"{datas[0].Text} / {datas[0].Count} массива / {datas[0].LongCount} м3";
                                countMas += datas[0].Count;
                                countM3 += datas[0].LongCount;
                            }
                        }

                        text += "\nДень\n";

                        for(int i = 0; i < datas.Count; i++)
                        {
                            if (datas[0].IsDay == EnumDayOrNight.Day)
                            {
                                text += $"{datas[0].Text} / {datas[0].Count} массива / {datas[0].LongCount} м3";
                                countMas += datas[0].Count;
                                countM3 += datas[0].LongCount;
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
                            if (datas[0].IsDay == EnumDayOrNight.Day)
                            {
                                text += $"{datas[0].Text} / {datas[0].Count} массива / {datas[0].LongCount} м3";
                                countMas += datas[0].Count;
                                countM3 += datas[0].LongCount;
                            }
                        }

                        text += $"\nИтого - {countMas} массива / {countM3} m3";
                        telegramBot.SendMessage(text);
                        datas.Clear();
                    }

                    if (enumDateDayOrNight == EnumDayOrNight.Day)
                        enumDateDayOrNight = EnumDayOrNight.Night;
                    else
                        enumDateDayOrNight = EnumDayOrNight.Day;

                    if(isFurstStart)
                        isFurstStart = false;

                    Thread.Sleep(startTime);
                    //Thread.Sleep(TimeSpan.FromHours(12)); // Поток ждет 12 часов
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }
    }

    public class TelegramBot
    {
        private List<People> _peoples = null;

        public TelegramBot(List<People> peoples)
        {
            _peoples = peoples;
        }

        public void SendMessage( string text)
        {
            TelegramBotClient botClient = new TelegramBotClient("6797439955:AAHA_jPPUvpRdIVdIEt2ZeTPkXketnLEnro");

            for (int i = 0; i < _peoples.Count; i++)
            {
                botClient.SendTextMessageAsync(_peoples[i].ChatId, text);
                Thread.Sleep(5000);
            }
        }
    }
}
