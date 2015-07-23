using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace DapperWrapper
{
    public static class SqliteHelper
    {
        public static string DbPwd { get; set; }
        // public static string DocumentPath { get; set; }

        public static string ConnectionString { get; set; }

        /// <summary>
        ///     执行需要的sql操作
        /// </summary>
        /// <param name="sqls">需要执行的sql数组，数组中每个字符串在一个单独的事物中执行</param>
        /// <exception cref="SQLiteException">sql异常</exception>
        public static string GetVersinInDb()
        {
            string SQLiteVersion;
            using (var sqlConnection = new SQLiteConnection(ConnectionString))
            {
                sqlConnection.Open();
                var cmd = sqlConnection.CreateCommand();

                //cmd.CommandType = CommandType.Text;
                //取第一条记录
                cmd.CommandText = "select VersionNumber from Core_Version limit 0,1";
                var versionNumber = cmd.ExecuteScalar();
                SQLiteVersion = versionNumber == null ? "0.0.0.0" : versionNumber.ToString();
            }
            return SQLiteVersion;
        }

        public static void Update(IEnumerable<string> sqls)
        {
            //连接数据库
            using (var sqlConnection = new SQLiteConnection(ConnectionString))
            {
                sqlConnection.Open();
                var cmd = sqlConnection.CreateCommand();
                var tx = sqlConnection.BeginTransaction();
                try
                {
                    foreach (var sql in sqls)
                    {
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
                catch (Exception)
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public static void Update(IEnumerable<string> sqls, Action<int> change)
        {
            //连接数据库
            var value = 0;
            using (var sqlConnection = new SQLiteConnection(ConnectionString))
            {
                sqlConnection.Open();
                var cmd = sqlConnection.CreateCommand();
                var tx = sqlConnection.BeginTransaction();
                try
                {
                    foreach (var sql in sqls)
                    {
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                        value = value + 1;
                        if (change != null)
                            change(value);
                    }
                    tx.Commit();
                }
                catch (Exception)
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public static DataSet Query(string sql)
        {
            var dt = new DataSet();
            using (var sqlConnection = new SQLiteConnection(ConnectionString))
            {
                sqlConnection.Open();
                var sda = new SQLiteDataAdapter(sql, ConnectionString);
                sda.Fill(dt, "ds");
            }

            return dt;
        }
    }
}