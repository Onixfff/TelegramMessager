﻿using NLog;
using System.Collections.Generic;
using System.Threading;
using Telegram.Bot;

namespace TelegramMessager
{
    public class TelegramBot
    {
        private List<People> _peoples = null;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public TelegramBot(List<People> peoples)
        {
            logger.Trace($"Инициализация TelegramBot");
            _peoples = peoples;
        }

        public async void SendMessage( string text)
        {
            logger.Trace($"Вход в метод по отправке сообщения из телеграмм бота");
            TelegramBotClient botClient = new TelegramBotClient("6797439955:AAHA_jPPUvpRdIVdIEt2ZeTPkXketnLEnro");

            for (int i = 0; i < _peoples.Count; i++)
            {
                logger.Trace($"Отправка сообщения {_peoples[i].ChatId} и что отправляет {text}");
                await botClient.SendTextMessageAsync(_peoples[i].ChatId, text);
                Thread.Sleep(5000);
            }
        }
    }
}
