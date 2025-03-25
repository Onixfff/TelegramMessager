namespace TelegramMessager
{
    public class Data
    {
        public string Date { get; private set; }
        public EnumDayOrNight IsDay { get; private set; }
        public string Text { get; private set; }
        public int Count { get; private set; }
        public double LongCount { get; private set; }

        public Data(string date, EnumDayOrNight isDay, string text, int count, double longCount)
        {
            Date = date;
            IsDay = isDay;
            Text = text;
            Count = count;
            LongCount = longCount;
        }
    }
}
