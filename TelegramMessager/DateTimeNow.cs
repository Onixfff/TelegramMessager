using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramMessager
{
    public class DateTimeNow
    {
       private DateTime _dateTimeNow = DateTime.Now;
        public DateTime GetDateTimeNow()
        {
            return _dateTimeNow;
        }

        public void ChangeDateTime()
        {
            _dateTimeNow = DateTime.Now;
        }

        public DateTime GetFirstDateTimeMount()
        {
            DateTime firstDayOfMonth = new DateTime(_dateTimeNow.Year, _dateTimeNow.Month, 1);
            return firstDayOfMonth;
        }

        public DateTime GetDateTimeMinus1Day()
        {
            return _dateTimeNow.AddDays(-1);
        }
    }
}
