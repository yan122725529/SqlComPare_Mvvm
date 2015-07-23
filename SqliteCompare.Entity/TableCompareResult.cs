using System.Collections;
using System.Collections.Generic;

namespace SqliteCompare.Entity
{
    public class TableCompareResult
    {
        //标准库数据对象信息
        public DbObjectInfo SourceInfo
        {
            set; get;
        }
        /// <summary>
        /// 对比库数据库对象信息
        /// </summary>
        public DbObjectInfo TargetInfo
        {
            set; get;
            
        }
        /// <summary>
        /// 错误类型 1-不一致，2-缺少，3-多余
        /// </summary>
        public int ErrorType { get; set; }

        /// <summary>
        /// 对比库多余的字段
        /// </summary>
        public IList<SqliteColInfo> SourceCol { get; set; }
        /// <summary>
        /// 对比库少的字段
        /// </summary>
        public IList<SqliteColInfo> TargetCol { get; set; }


        /// <summary>
        /// 对比库多余的字段
        /// </summary>
        public IList<SqliteColInfo> MoreCol { get; set; }
        /// <summary>
        /// 对比库少的字段
        /// </summary>
        public IList<SqliteColInfo> LostCol { get; set; }


    }
}