using NLog;
using System;

namespace TelegramMessager
{
    public class DateTimeNow
    {
        private static DateTime _dateTimeNow = DateTime.Now;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public DateTime GetDateTimeNow()
        {
            logger.Trace($"Возвращает время сейчас {DateTime.Now}");
            return _dateTimeNow;
        }

        public void ChangeDateTime()
        {
            _dateTimeNow = DateTime.Now;
            logger.Trace($"Меняет время _dateTimeNow на {_dateTimeNow}");
        }

        public DateTime GetFirstDateTimeMount()
        {

            DateTime firstDayOfMonth = new DateTime(_dateTimeNow.Year, _dateTimeNow.Month, 1);
            logger.Trace($"Возвращает первый день месяца {firstDayOfMonth}");

            return firstDayOfMonth;
        }

        public DateTime GetDateTimeMinus1Day()
        {
            logger.Trace($"Возвращает _dateTimeNow ( {_dateTimeNow.AddDays(-1)})");

            return _dateTimeNow.AddDays(-1);
        }
    }
}
