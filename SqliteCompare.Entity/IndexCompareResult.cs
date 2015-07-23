using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqliteCompare.Entity
{
   public class IndexCompareResult
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
    }
}
