
using DapperExtensions.Mapper;

namespace SqliteCompare.Entity
{
    public class DbObjectInfo
    {
        /// <summary>
        ///     对象名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        ///     对象类型
        /// </summary>
        public string type { get; set; }

        public string sql { get; set; }

        public int rootpage { get; set; }

        public string tbl_name { get; set; }
    }

    public class DbObjectInfoMapper : ClassMapper<DbObjectInfo>
    {
        public DbObjectInfoMapper()
        {
            Table("sqlite_master");
           AutoMap();
        }
    }
}