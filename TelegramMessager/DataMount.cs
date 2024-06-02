using System;

namespace TelegramMessager
{
    public class DataMount
    {
        public string Text { get; private set; }
        public int Count { get; private set; }
        public double LongCount { get; private set; }

        private DateTime _byDate;
        private DateTime _fromDate;

        public DataMount(string text, int count, double longCount)
        {
            Text = text;
            Count = count;
            LongCount = longCount;
        }

        public void AddByDate(DateTime date)
        {
            _byDate = date;
        }

        public void AddFromDate(DateTime date)
        {
            _fromDate = date;
        }

        public DateTime GetFromDate()
        {
            return _fromDate;
        }

        public DateTime GetByDate()
        {
            return _byDate;
        }
    }
}
