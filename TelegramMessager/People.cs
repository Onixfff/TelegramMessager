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
