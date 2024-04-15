using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace TelegramMessager
{

    public class Database
    {
        private MySqlConnection _mCon = new MySqlConnection(ConfigurationManager.ConnectionStrings["server"].ConnectionString);

        public Database()
        {
            GetData();
        }

        private async void GetData()
        {

            string query = "SELECT if (time(Timestamp) < '08:00:00',date_format(date_sub(Timestamp, INTERVAL 1 DAY), \"%d %M %Y\"), date_format(Timestamp, \"%d %M %Y\")) as df, if (time(Timestamp) <= '20:00:00' and time(Timestamp)>= '08:00:00','день','ночь') as shift, data_52, count(dbid)-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and(if (time(Timestamp) < '08:00:00',date_format(date_sub(Timestamp, INTERVAL 1 DAY), \"%d %M %Y\"),date_format(Timestamp, \"%d %M %Y\")))= ( if (time(ms.data_err) < '08:00:00',date_format(date_sub(ms.data_err, INTERVAL 1 DAY), \"%d %M %Y\"),date_format(ms.data_err, \"%d %M %Y\")))and(if (time(Timestamp) <= '20:00:00' and time(Timestamp)>= '08:00:00','день','ночь'))= (if (time(ms.data_err) <= '20:00:00' and time(ms.data_err)>= '08:00:00','день','ночь'))) as count_1, round(((count(dbid)-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and(if (time(Timestamp) < '08:00:00',date_format(date_sub(Timestamp, INTERVAL 1 DAY), \"%d %M %Y\"),date_format(Timestamp, \"%d %M %Y\")))= ( if (time(ms.data_err) < '08:00:00',date_format(date_sub(ms.data_err, INTERVAL 1 DAY), \"%d %M %Y\"),date_format(ms.data_err, \"%d %M %Y\")))and(if (time(Timestamp) <= '20:00:00' and time(Timestamp)>= '08:00:00','день','ночь'))= (if (time(ms.data_err) <= '20:00:00' and time(ms.data_err)>= '08:00:00','день','ночь')))) * '4.32'), 2) as mas from spslogger.mixreport as mr where Timestamp >= '2024-04-12 08:00:00' and Timestamp < concat( date_add('2024-04-30', interval 1 day), ' 08:00:00')  group by df,shift, data_52";
            //query = "SELECT if (time(Timestamp) < '08:00:00',date_format(date_sub(Timestamp, INTERVAL 1 DAY), \"%d %M %Y\"),date_format(Timestamp, \"%d %M %Y\")) as df,if (time(Timestamp) <= '20:00:00' and time(Timestamp)>= '08:00:00','день','ночь') as shift, data_52,min(dbid) as min, max(dbid) as max, count(dbid)-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and(if (time(Timestamp) < '08:00:00',date_format(date_sub(Timestamp, INTERVAL 1 DAY), \"%d %M %Y\"),date_format(Timestamp, \"%d %M %Y\")))= ( if (time(ms.data_err) < '08:00:00',date_format(date_sub(ms.data_err, INTERVAL 1 DAY), \"%d %M %Y\"),date_format(ms.data_err, \"%d %M %Y\")))and(if (time(Timestamp) <= '20:00:00' and time(Timestamp)>= '08:00:00','день','ночь'))= (if (time(ms.data_err) <= '20:00:00' and time(ms.data_err)>= '08:00:00','день','ночь'))) as count_1, round(((count(dbid)-(select ifnull(sum(sum_er),0) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and(if (time(Timestamp) < '08:00:00',date_format(date_sub(Timestamp, INTERVAL 1 DAY), \"%d %M %Y\"),date_format(Timestamp, \"%d %M %Y\")))= ( if (time(ms.data_err) < '08:00:00',date_format(date_sub(ms.data_err, INTERVAL 1 DAY), \"%d %M %Y\"),date_format(ms.data_err, \"%d %M %Y\")))and(if (time(Timestamp) <= '20:00:00' and time(Timestamp)>= '08:00:00','день','ночь'))= (if (time(ms.data_err) <= '20:00:00' and time(ms.data_err)>= '08:00:00','день','ночь')))) * '4.32'), 2) as mas, sum(data_23) as Lime_1,sum(data_25) as Lime_2,  (sum(data_23) + sum(data_25)) as Lime_sum,sum(data_27) as Cement_1, sum(data_29) as Cement_2,(sum(data_27) + sum(data_29)) as Cement_sum,sum(data_116) as Gips, round(sum(data_181), 1) as Sand, round(sum(data_162), 1) as Additive,round((sum(data_193) + sum(data_199)), 2) as alum, round((count(dbid) * '4.32' * '0.8'), 2) as drob, (select sum(sum_er) as brak from spslogger.error_mas as ms where mr.data_52 = ms.recepte and(if (time(Timestamp) < '08:00:00',date_format(date_sub(Timestamp, INTERVAL 1 DAY), \"%d %M %Y\"),date_format(Timestamp, \"%d %M %Y\")))= ( if (time(ms.data_err) < '08:00:00',date_format(date_sub(ms.data_err, INTERVAL 1 DAY), \"%d %M %Y\"),date_format(ms.data_err, \"%d %M %Y\")))and(if (time(Timestamp) <= '20:00:00' and time(Timestamp)>= '08:00:00','день','ночь'))= (if (time(ms.data_err) <= '20:00:00' and time(ms.data_err)>= '08:00:00','день','ночь'))) as brak from spslogger.mixreport as mr where Timestamp >= '2024-04-01 08:00:00' and Timestamp < concat( date_add('2024-04-30', interval 1 day), ' 08:00:00')  group by df,shift, data_52";
            try
            {
                await _mCon.OpenAsync();
            }
            catch(MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            using (MySqlCommand command = new MySqlCommand(query, _mCon))
            {
                var s = _mCon.State;

                using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var one = reader.GetString(0);
                        var two = reader.GetString(1);
                        var tre = reader.GetString(2);
                        var fou = reader.GetInt32(3);
                        var fff = reader.GetDouble(4);
                    }
                    reader.Close();
                }
            }
            
            _mCon.Clone();
        }

    }

    public class Data
    {
        private string Date;
        private string DateName;
    }
}
