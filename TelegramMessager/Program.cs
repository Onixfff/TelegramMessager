using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramMessager
{
    public class Program
    {
        static TelegramBotClient botClient = new TelegramBotClient("6797439955:AAHA_jPPUvpRdIVdIEt2ZeTPkXketnLEnro");
        
        static void Main(string[] args)
        {
            while (true)
            {
                Database database = new Database();
                string userText = Console.ReadLine();
                    botClient.SendTextMessageAsync(787471566, userText);
                    Thread.Sleep(10000);
            }
        }
    }
}
