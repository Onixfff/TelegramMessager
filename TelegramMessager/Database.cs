using MySql.Data.MySqlClient;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace TelegramMessager
{

    public class Database
    {
        private MySqlConnection _mCon = new MySqlConnection(ConfigurationManager.ConnectionStrings["server"].ConnectionString);
        private DateTimeNow _dateTimeClass;
        private ILogger _logger;

        List<Data> datas;
        List<DataMount> mounts;

        public Database(ILogger logger)
        {
            _logger = logger;
            _dateTimeClass = new DateTimeNow(_logger);
        }

        public async Task<List<Data>> GetDataNight()
        {
            datas = new List<Data>();
            DateTime thisDay = _dateTimeClass.GetDateTimeMinus1Day();
            string query = $"SELECT if (time(Timestamp) < '08:00:00',date_format(date_sub(Timestamp, INTERVAL 1 DAY), \"%d %M %Y\"), date_format(Timestamp, \"%d %M %Y\")) as df, if (time(Timestamp) <= '20:00:00' and time(Timestamp)>= '08:00:00','день','ночь') as shift, data_52, count(dbid)-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and(if (time(Timestamp) < '08:00:00',date_format(date_sub(Timestamp, INTERVAL 1 DAY), \"%d %M %Y\"),date_format(Timestamp, \"%d %M %Y\")))= ( if (time(ms.data_err) < '08:00:00',date_format(date_sub(ms.data_err, INTERVAL 1 DAY), \"%d %M %Y\"),date_format(ms.data_err, \"%d %M %Y\")))and(if (time(Timestamp) <= '20:00:00' and time(Timestamp)>= '08:00:00','день','ночь'))= (if (time(ms.data_err) <= '20:00:00' and time(ms.data_err)>= '08:00:00','день','ночь'))) as count_1, round(((count(dbid)-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and(if (time(Timestamp) < '08:00:00',date_format(date_sub(Timestamp, INTERVAL 1 DAY), \"%d %M %Y\"),date_format(Timestamp, \"%d %M %Y\")))= ( if (time(ms.data_err) < '08:00:00',date_format(date_sub(ms.data_err, INTERVAL 1 DAY), \"%d %M %Y\"),date_format(ms.data_err, \"%d %M %Y\")))and(if (time(Timestamp) <= '20:00:00' and time(Timestamp)>= '08:00:00','день','ночь'))= (if (time(ms.data_err) <= '20:00:00' and time(ms.data_err)>= '08:00:00','день','ночь')))) * '4.32'), 2) as mas from spslogger.mixreport as mr where Timestamp >= '{thisDay.ToString("yyyy-MM-dd")} 08:00:00' and Timestamp < concat( date_add('{thisDay:yyyy-MM-dd}', interval 1 day), ' 08:00:00')  group by df,shift, data_52";

            try
            {
                try
                {
                    if (_mCon.State == System.Data.ConnectionState.Closed)
                    {
                        _logger.Trace("Подключение бд");
                        await _mCon.OpenAsync();
                    }
                }
                catch (MySqlException)
                {
                    goto Select;
                }

            Select:
                using (MySqlCommand command = new MySqlCommand(query, _mCon))
                {
                    var s = _mCon.State;

                    using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            EnumDayOrNight enumDay = new EnumDayOrNight();
                            var two = reader.GetString(1);

                            if (two == "день")
                                enumDay = EnumDayOrNight.Day;
                            else
                                enumDay = EnumDayOrNight.Night;

                            datas.Add(new Data(
                                reader.GetString(0),
                                enumDay,
                                reader.GetString(2),
                                reader.GetInt32(3),
                                reader.GetDouble(4)));
                        }
                        reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _logger.Error(ex, "Ошибка GetDataNight");

            }
            finally { _mCon.Close(); }

            _logger.Trace($"Возвращаем datas - {datas}");
            return datas;
        }

        public async Task<List<Data>> GetDataDay()
        {
            datas = new List<Data>();
            DateTime thisDay = _dateTimeClass.GetDateTimeNow(); ;
            string query = $"SELECT if (time(Timestamp) < '08:00:00',date_format(date_sub(Timestamp, INTERVAL 1 DAY), \"%d %M %Y\"), date_format(Timestamp, \"%d %M %Y\")) as df, if (time(Timestamp) <= '20:00:00' and time(Timestamp)>= '08:00:00','день','ночь') as shift, data_52, count(dbid)-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and(if (time(Timestamp) < '08:00:00',date_format(date_sub(Timestamp, INTERVAL 1 DAY), \"%d %M %Y\"),date_format(Timestamp, \"%d %M %Y\")))= ( if (time(ms.data_err) < '08:00:00',date_format(date_sub(ms.data_err, INTERVAL 1 DAY), \"%d %M %Y\"),date_format(ms.data_err, \"%d %M %Y\")))and(if (time(Timestamp) <= '20:00:00' and time(Timestamp)>= '08:00:00','день','ночь'))= (if (time(ms.data_err) <= '20:00:00' and time(ms.data_err)>= '08:00:00','день','ночь'))) as count_1, round(((count(dbid)-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and(if (time(Timestamp) < '08:00:00',date_format(date_sub(Timestamp, INTERVAL 1 DAY), \"%d %M %Y\"),date_format(Timestamp, \"%d %M %Y\")))= ( if (time(ms.data_err) < '08:00:00',date_format(date_sub(ms.data_err, INTERVAL 1 DAY), \"%d %M %Y\"),date_format(ms.data_err, \"%d %M %Y\")))and(if (time(Timestamp) <= '20:00:00' and time(Timestamp)>= '08:00:00','день','ночь'))= (if (time(ms.data_err) <= '20:00:00' and time(ms.data_err)>= '08:00:00','день','ночь')))) * '4.32'), 2) as mas from spslogger.mixreport as mr where Timestamp >= '{thisDay:yyyy-MM-dd} 08:00:00' and Timestamp < concat( date_add('{thisDay:yyyy-MM-dd}', interval 1 day), ' 08:00:00')  group by df,shift, data_52";
            try
            {
                try
                {
                    if (_mCon.State == System.Data.ConnectionState.Closed)
                    {
                        _logger.Trace("Подключение бд");
                        await _mCon.OpenAsync();
                    }
                }
                catch (MySqlException)
                {
                    goto Select;
                }

            Select:
                using (MySqlCommand command = new MySqlCommand(query, _mCon))
                {
                    var s = _mCon.State;

                    using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            EnumDayOrNight enumDay = new EnumDayOrNight();
                            var two = reader.GetString(1);

                            if (two == "день")
                                enumDay = EnumDayOrNight.Day;
                            else
                                continue;

                            datas.Add(new Data(
                                reader.GetString(0),
                                enumDay,
                                reader.GetString(2),
                                reader.GetInt32(3),
                                reader.GetDouble(4)));
                        }
                        reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _logger.Error(ex, "Ошибка GetDataDay");

            }
            finally { _mCon.Close(); }

            _logger.Trace($"Возвращаем datas - {datas}");

            return datas;

        }

        public async Task<List<DataMount>> GetMountData(EnumDayOrNight enumDayOrNight)
        {
            mounts = new List<DataMount>();
            string query;

            var _dateTimeFirstMount = _dateTimeClass.GetFirstDateTimeMount();
            DateTime _dateTimeNow = _dateTimeClass.GetDateTimeNow();

            if (enumDayOrNight == EnumDayOrNight.Day)
            {
                _logger.Trace($"enumDayOrNight == EnumDayOrNight.Day");
                query = $"SELECT data_52, (count(dbid)-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and ms.data_err >= '{_dateTimeFirstMount.ToString("yyyy-MM-dd")} 08:00:00' and ms.data_err < concat(date_add('{_dateTimeNow.ToString("yyyy-MM-dd")}', interval 1 day), ' 08:00:00')) ) as count_1, round((count(dbid-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and ms.data_err >= '{_dateTimeFirstMount.ToString("yyyy-MM-dd")} 08:00:00' and ms.data_err < concat(date_add('{_dateTimeNow.ToString("yyyy-MM-dd")}', interval 1 day), ' 08:00:00')) )* '4.32'), 2) as mas,concat(cast(sum(data_23) + sum(data_25) as char(10)),  ' / ', (round((((sum(data_23) + sum(data_25)) / (count(dbid-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and ms.data_err >= '{_dateTimeFirstMount.ToString("yyyy-MM-dd")} 08:00:00' and ms.data_err < concat(date_add('{_dateTimeNow.ToString("yyyy-MM-dd")}', interval 1 day), ' 08:00:00')) ) * '4.32'))), 1))) as Lime_sum,concat(cast(sum(data_27) + sum(data_29) as char(10)), ' / ', (round((((sum(data_27) + sum(data_29)) / (count(dbid-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and ms.data_err >= '{_dateTimeFirstMount.ToString("yyyy-MM-dd")} 08:00:00' and ms.data_err < concat(date_add('{_dateTimeNow.ToString("yyyy-MM-dd")}', interval 1 day), ' 08:00:00')) ) * '4.32'))), 1))) as Cement_sum,  concat(cast(round(sum(data_116), 1) as char(10)), ' / ', (round((sum(data_116) / count(dbid-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and ms.data_err >= '{_dateTimeFirstMount.ToString("yyyy-MM-dd")} 08:00:00' and ms.data_err < concat(date_add('{_dateTimeNow.ToString("yyyy-MM-dd")}', interval 1 day), ' 08:00:00')) ) / '4.32'), 1))) as Gips,concat(cast(round(sum(data_181), 1) as char(10)), ' / ', (round((sum(data_181) / count(dbid-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and ms.data_err >= '{_dateTimeFirstMount.ToString("yyyy-MM-dd")} 08:00:00' and ms.data_err < concat(date_add('{_dateTimeNow.ToString("yyyy-MM-dd")}', interval 1 day), ' 08:00:00')) ) / '4.32'), 1))) as Sand, concat(cast(round(sum(data_162), 3) as char(10)), ' / ', (round((sum(data_162) / count(dbid-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and ms.data_err >= '{_dateTimeFirstMount.ToString("yyyy-MM-dd")} 08:00:00' and ms.data_err < concat(date_add('{_dateTimeNow.ToString("yyyy-MM-dd")}', interval 1 day), ' 08:00:00')) ) / '4.32'), 1))) as Additive, concat(cast(round((sum(data_193) + sum(data_199)), 2) as char(10)), ' / ', (round(((sum(data_193) + sum(data_199)) / count(dbid-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and ms.data_err >= '{_dateTimeFirstMount.ToString("yyyy-MM-dd")} 08:00:00' and ms.data_err < concat(date_add('{_dateTimeNow.ToString("yyyy-MM-dd")}', interval 1 day), ' 08:00:00')) ) / '4.32'), 2))) as alum,concat(cast(round((count(dbid) * '4.32' * '0.8'), 2) as char(10)), ' / ', '0.8') as drob, (select sum(sum_er) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and ms.data_err >= '{_dateTimeFirstMount.ToString("yyyy-MM-dd")} 08:00:00' and ms.data_err < concat(date_add('{_dateTimeNow.ToString("yyyy-MM-dd")}', interval 1 day), ' 08:00:00')) as brak from spslogger.mixreport as mr where  Timestamp >= '{_dateTimeFirstMount.ToString("yyyy-MM-dd")} 08:00:00' and Timestamp < concat( date_add('{_dateTimeNow.ToString("yyyy-MM-dd")}', interval 1 day), ' 08:00:00')   group by data_52";
            }
            else
            {
                _logger.Trace($"enumDayOrNight == EnumDayOrNight.Night");

                var FirstDay = new DateTime(_dateTimeNow.Year, _dateTimeNow.Month, 1);
                if (FirstDay.Day == _dateTimeNow.Day)
                {
                    _logger.Trace($"FirstDay.Day == _dateTimeNow.Day ({FirstDay.Day} == {_dateTimeNow.Day})");

                    _dateTimeFirstMount = _dateTimeFirstMount.AddMonths(-1);
                    query = $"SELECT data_52, (count(dbid)-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and ms.data_err >= '{_dateTimeFirstMount.ToString("yyyy-MM-dd")} 08:00:00' and ms.data_err < concat(date_add('{_dateTimeNow.ToString("yyyy-MM-dd")}', interval 1 day), ' 08:00:00')) ) as count_1, round((count(dbid-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and ms.data_err >= '{_dateTimeFirstMount.ToString("yyyy-MM-dd")} 08:00:00' and ms.data_err < concat(date_add('{_dateTimeNow.ToString("yyyy-MM-dd")}', interval 1 day), ' 08:00:00')) )* '4.32'), 2) as mas,concat(cast(sum(data_23) + sum(data_25) as char(10)),  ' / ', (round((((sum(data_23) + sum(data_25)) / (count(dbid-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and ms.data_err >= '{_dateTimeFirstMount.ToString("yyyy-MM-dd")} 08:00:00' and ms.data_err < concat(date_add('{_dateTimeNow.ToString("yyyy-MM-dd")}', interval 1 day), ' 08:00:00')) ) * '4.32'))), 1))) as Lime_sum,concat(cast(sum(data_27) + sum(data_29) as char(10)), ' / ', (round((((sum(data_27) + sum(data_29)) / (count(dbid-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and ms.data_err >= '{_dateTimeFirstMount.ToString("yyyy-MM-dd")} 08:00:00' and ms.data_err < concat(date_add('{_dateTimeNow.ToString("yyyy-MM-dd")}', interval 1 day), ' 08:00:00')) ) * '4.32'))), 1))) as Cement_sum,  concat(cast(round(sum(data_116), 1) as char(10)), ' / ', (round((sum(data_116) / count(dbid-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and ms.data_err >= '{_dateTimeFirstMount.ToString("yyyy-MM-dd")} 08:00:00' and ms.data_err < concat(date_add('{_dateTimeNow.ToString("yyyy-MM-dd")}', interval 1 day), ' 08:00:00')) ) / '4.32'), 1))) as Gips,concat(cast(round(sum(data_181), 1) as char(10)), ' / ', (round((sum(data_181) / count(dbid-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and ms.data_err >= '{_dateTimeFirstMount.ToString("yyyy-MM-dd")} 08:00:00' and ms.data_err < concat(date_add('{_dateTimeNow.ToString("yyyy-MM-dd")}', interval 1 day), ' 08:00:00')) ) / '4.32'), 1))) as Sand, concat(cast(round(sum(data_162), 3) as char(10)), ' / ', (round((sum(data_162) / count(dbid-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and ms.data_err >= '{_dateTimeFirstMount.ToString("yyyy-MM-dd")} 08:00:00' and ms.data_err < concat(date_add('{_dateTimeNow.ToString("yyyy-MM-dd")}', interval 1 day), ' 08:00:00')) ) / '4.32'), 1))) as Additive, concat(cast(round((sum(data_193) + sum(data_199)), 2) as char(10)), ' / ', (round(((sum(data_193) + sum(data_199)) / count(dbid-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and ms.data_err >= '{_dateTimeFirstMount.ToString("yyyy-MM-dd")} 08:00:00' and ms.data_err < concat(date_add('{_dateTimeNow.ToString("yyyy-MM-dd")}', interval 1 day), ' 08:00:00')) ) / '4.32'), 2))) as alum,concat(cast(round((count(dbid) * '4.32' * '0.8'), 2) as char(10)), ' / ', '0.8') as drob, (select sum(sum_er) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and ms.data_err >= '{_dateTimeFirstMount.ToString("yyyy-MM-dd")} 08:00:00' and ms.data_err < concat(date_add('{_dateTimeNow.ToString("yyyy-MM-dd")}', interval 1 day), ' 08:00:00')) as brak from spslogger.mixreport as mr where  Timestamp >= '{_dateTimeFirstMount.ToString("yyyy-MM-dd")} 08:00:00' and Timestamp < concat( date_add('{_dateTimeNow.ToString("yyyy-MM-dd")}', interval 1 day), ' 08:00:00')   group by data_52";
                }
                else
                {
                    _logger.Trace($"FirstDay.Day = _dateTimeNow.Day ({FirstDay.Day} != {_dateTimeNow.Day})");

                    query = $"SELECT data_52, (count(dbid)-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and ms.data_err >= '{_dateTimeFirstMount.ToString("yyyy-MM-dd")} 08:00:00' and ms.data_err < concat(date_add('{_dateTimeNow.ToString("yyyy-MM-dd")}', interval 1 day), ' 08:00:00')) ) as count_1, round((count(dbid-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and ms.data_err >= '{_dateTimeFirstMount.ToString("yyyy-MM-dd")} 08:00:00' and ms.data_err < concat(date_add('{_dateTimeNow.ToString("yyyy-MM-dd")}', interval 1 day), ' 08:00:00')) )* '4.32'), 2) as mas,concat(cast(sum(data_23) + sum(data_25) as char(10)),  ' / ', (round((((sum(data_23) + sum(data_25)) / (count(dbid-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and ms.data_err >= '{_dateTimeFirstMount.ToString("yyyy-MM-dd")} 08:00:00' and ms.data_err < concat(date_add('{_dateTimeNow.ToString("yyyy-MM-dd")}', interval 1 day), ' 08:00:00')) ) * '4.32'))), 1))) as Lime_sum,concat(cast(sum(data_27) + sum(data_29) as char(10)), ' / ', (round((((sum(data_27) + sum(data_29)) / (count(dbid-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and ms.data_err >= '{_dateTimeFirstMount.ToString("yyyy-MM-dd")} 08:00:00' and ms.data_err < concat(date_add('{_dateTimeNow.ToString("yyyy-MM-dd")}', interval 1 day), ' 08:00:00')) ) * '4.32'))), 1))) as Cement_sum,  concat(cast(round(sum(data_116), 1) as char(10)), ' / ', (round((sum(data_116) / count(dbid-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and ms.data_err >= '{_dateTimeFirstMount.ToString("yyyy-MM-dd")} 08:00:00' and ms.data_err < concat(date_add('{_dateTimeNow.ToString("yyyy-MM-dd")}', interval 1 day), ' 08:00:00')) ) / '4.32'), 1))) as Gips,concat(cast(round(sum(data_181), 1) as char(10)), ' / ', (round((sum(data_181) / count(dbid-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and ms.data_err >= '{_dateTimeFirstMount.ToString("yyyy-MM-dd")} 08:00:00' and ms.data_err < concat(date_add('{_dateTimeNow.ToString("yyyy-MM-dd")}', interval 1 day), ' 08:00:00')) ) / '4.32'), 1))) as Sand, concat(cast(round(sum(data_162), 3) as char(10)), ' / ', (round((sum(data_162) / count(dbid-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and ms.data_err >= '{_dateTimeFirstMount.ToString("yyyy-MM-dd")} 08:00:00' and ms.data_err < concat(date_add('{_dateTimeNow.ToString("yyyy-MM-dd")}', interval 1 day), ' 08:00:00')) ) / '4.32'), 1))) as Additive, concat(cast(round((sum(data_193) + sum(data_199)), 2) as char(10)), ' / ', (round(((sum(data_193) + sum(data_199)) / count(dbid-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and ms.data_err >= '{_dateTimeFirstMount.ToString("yyyy-MM-dd")} 08:00:00' and ms.data_err < concat(date_add('{_dateTimeNow.ToString("yyyy-MM-dd")}', interval 1 day), ' 08:00:00')) ) / '4.32'), 2))) as alum,concat(cast(round((count(dbid) * '4.32' * '0.8'), 2) as char(10)), ' / ', '0.8') as drob, (select sum(sum_er) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and ms.data_err >= '{_dateTimeFirstMount.ToString("yyyy-MM-dd")} 08:00:00' and ms.data_err < concat(date_add('{_dateTimeNow.ToString("yyyy-MM-dd")}', interval 1 day), ' 08:00:00')) as brak from spslogger.mixreport as mr where  Timestamp >= '{_dateTimeFirstMount.ToString("yyyy-MM-dd")} 08:00:00' and Timestamp < concat( date_add('{_dateTimeNow.ToString("yyyy-MM-dd")}', interval 1 day), ' 08:00:00')   group by data_52";
                }

            }

            try
            {
                try
                {
                    if (_mCon.State == System.Data.ConnectionState.Closed)
                    {
                        _logger.Trace("Подключение бд");
                        await _mCon.OpenAsync();
                    }
                }
                catch (MySqlException)
                {
                    goto Select;
                }

            Select:
                using (MySqlCommand command = new MySqlCommand(query, _mCon))
                {
                    var s = _mCon.State;

                    using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            mounts.Add(new DataMount(reader.GetString(0), reader.GetInt32(1), reader.GetDouble(2)));
                            mounts[0].AddFromDate(_dateTimeFirstMount);
                            mounts[0].AddByDate(_dateTimeNow);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _logger.Error(ex, "Ошибка GetMountData");

            }
            finally { _mCon.Close(); }

            return mounts;
        }
    }
}
