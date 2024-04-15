using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramMessager
{
    public class People
    {
        public long ChatId { get; private set; }

        public People(long chatId) 
        {
            ChatId = chatId;
        }
    }
}
