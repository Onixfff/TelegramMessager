using System;
using System.Collections.Generic;
using System.Threading;
using Telegram.Bot;

namespace TelegramMessager
{
    public class TelegramBot
    {
        private List<People> _peoples = null;

        public TelegramBot(List<People> peoples)
        {
            _peoples = peoples;
        }

        public async void SendMessage( string text)
        {
            TelegramBotClient botClient = new TelegramBotClient("6797439955:AAHA_jPPUvpRdIVdIEt2ZeTPkXketnLEnro");

            for (int i = 0; i < _peoples.Count; i++)
            {
                await botClient.SendTextMessageAsync(_peoples[i].ChatId, text);
                Thread.Sleep(5000);
            }
        }

        public int GetLastDay()
        {
            return DateTime.Now.Day;
        }
    }
}
