using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using SqliteCompare.Entity;

namespace DapperWrapper
{
    public class TargetContext : IDbContext
    {
        public static string _systemDataConnectionString;

        public static string SystemDataConnectionString
        {
            get
            {
                
                return _systemDataConnectionString = string.Format(
                    @"Data Source={0};UTF8Encoding=True;Version=3;Pooling=True",
                   new AppInfo().TargetDbPath);
            }
        }

        public IDbConnection GetConnection()
        {
            DbConnection connection = new SQLiteConnection(SystemDataConnectionString);
            return connection;
        }

        public string ContextName
        {
            get { return "Target"; }
        }
    }
}