using System;
using System.IO;
using System.Xml.Serialization;

namespace SqliteCompare.Entity
{
    /// <summary>
    ///     App全局的一些信息
    /// </summary>
    public class AppSetting
    {
        private static readonly string _settingpath = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory,
            "AppSetting.xml");

        static AppSetting()
        {
            var file = _settingpath;
            if (!File.Exists(file) || File.ReadAllText(file).Length == 0)
            {
                InitFile();
            }
            else
            {
                RepairFile();
            }
        }

        public string SourceDbPath { get; set; }

        /// <summary>
        ///     目标数据库的路径
        /// </summary>
        public string TargetDbPath { get; set; }

        /// <summary>
        ///     目标数据库的路径
        /// </summary>
        public bool NeedCompareIndex { get; set; }

        /// <summary>
        ///     修复文件节点
        /// </summary>
        private static void RepairFile()
        {
            //检查节点
            var entity = LoadData();

            entity.SaveData();
        }

        /// <summary>
        ///     初始化配置文件文件
        /// </summary>
        private static AppSetting InitFile()
        {
            var entity = new AppSetting
            {
                SourceDbPath = string.Empty,
                TargetDbPath = string.Empty,
                NeedCompareIndex = true
            };


            entity.SaveData();
            return entity;
        }

        public static AppSetting LoadData()
        {
            AppSetting entity;
            try
            {
                using (var sr = new StreamReader(_settingpath))
                {
                    var xs = new XmlSerializer(typeof (AppSetting));
                    entity = xs.Deserialize(sr) as AppSetting;
                }
            }
            catch (Exception)
            {
                return InitFile();
            }
            return entity;
        }

        public void SaveData()
        {
            try
            {
                var xs = new XmlSerializer(typeof (AppSetting));
                var sw = new StreamWriter(_settingpath);
                xs.Serialize(sw, this);
                sw.Close();
            }
            catch (Exception ex)
            {
            }
        }
    }
}