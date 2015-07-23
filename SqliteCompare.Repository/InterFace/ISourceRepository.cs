using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqliteCompare.Entity;

namespace SqliteCompare.Repository
{
    public interface ISourceRepository
    {
        IList<DbObjectInfo> GetDBTables();
        IList<DbObjectInfo> GetDBIndex();
        IList<SqliteColInfo> GetColInfoByTable(string tableName);
        void RefreshContext();
    }
}
