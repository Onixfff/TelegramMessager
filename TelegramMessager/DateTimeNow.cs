using NLog;
using System;

namespace TelegramMessager
{
    public class DateTimeNow
    {
        private static DateTime _dateTimeNow = DateTime.Now;
        private ILogger _logger = LogManager.GetCurrentClassLogger();

        public DateTimeNow(ILogger logger)
        {
            _logger = logger;
        }

        public DateTime GetDateTimeNow()
        {
            _logger.Trace($"Возвращает время сейчас {DateTime.Now}");
            return DateTime.Now;
        }

        public void ChangeDateTime()
        {
            _dateTimeNow = DateTime.Now;
            _logger.Trace($"Меняет время _dateTimeNow на {_dateTimeNow}");
        }

        public DateTime GetFirstDateTimeMount()
        {

            DateTime firstDayOfMonth = new DateTime(_dateTimeNow.Year, _dateTimeNow.Month, 1);
            _logger.Trace($"Возвращает первый день месяца {firstDayOfMonth}");

            return firstDayOfMonth;
        }

        public DateTime GetDateTimeMinus1Day()
        {
            _logger.Trace($"Возвращает _dateTimeNow ( {_dateTimeNow.AddDays(-1)})");

            return _dateTimeNow.AddDays(-1);
        }
    }
}
