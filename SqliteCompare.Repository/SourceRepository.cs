using System.Collections.Generic;
using System.Data;
using System.Linq;
using DapperWrapper;
using SqliteCompare.Entity;

namespace SqliteCompare.Repository
{
    public class SourceRepository : Respository<DbObjectInfo>, ISourceRepository
    {
        public SourceRepository()
        {
            RefreshContext();
        }

        public IList<DbObjectInfo> GetDBTables()
        {
            return GetList<DbObjectInfo>(@"select * from sqlite_master where type='table'").ToList();
        }
        public IList<DbObjectInfo> GetDBIndex()
        {
            return GetList<DbObjectInfo>(@"select * from sqlite_master where type='index'").ToList();
        }
        
        public IList<SqliteColInfo> GetColInfoByTable(string tableName)
        {
            IList<SqliteColInfo> re = null;
            SqliteHelper.ConnectionString = _context.GetConnection().ConnectionString;
            var ds = SqliteHelper.Query(string.Format("pragma table_info ('{0}')", tableName));
            if (ds != null && ds.Tables[0] != null)
            {
                var data = ds.Tables[0];
                re = (from DataRow row in data.Rows
                    select new SqliteColInfo
                    {
                        Cid = row["cid"].ToString(),
                        Name = row["name"].ToString(),
                        Type = row["type"].ToString(),
                        NotNull = row["notnull"].ToString(),
                        Dflt_Value = row["dflt_value"].ToString(),
                        Pk = row["pk"].ToString()
                    }).ToList();
            }

            return re;
        }

        public void RefreshContext()
        {
            _context = new SourceContext();
        }
    }
}