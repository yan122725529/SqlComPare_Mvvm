using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqliteCompare.Entity
{
    public class SqliteColInfo
    {   
        /// <summary>
        /// 列ID
        /// </summary>
        public string Cid { get; set; }
        /// <summary>
        /// 列名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 列数据类型
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 是否可为空
        /// </summary>
        public string NotNull { get; set; }
        /// <summary>
        /// 默认值
        /// </summary>
        public string Dflt_Value { get; set; }
        /// <summary>
        /// 是否PK
        /// </summary>
        public string Pk { get; set; }

    }
}
