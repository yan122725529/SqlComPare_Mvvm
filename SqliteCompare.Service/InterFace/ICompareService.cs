using System;
using System.Collections.Generic;
using SqliteCompare.Entity;

namespace SqliteCompare.Service.InterFace
{
    public interface ICompareService
    {
        List<TableCompareResult> DifTableList { set; get; }
        List<IndexCompareResult> DifIndexList { set; get; }

        List<TableCompareResult> CompareDBTable();
        List<IndexCompareResult> CompareDBIndex();
        void RefreshContext();
        void RepairDb(IEnumerable<string> sqls, Action<int> change);
    }
}