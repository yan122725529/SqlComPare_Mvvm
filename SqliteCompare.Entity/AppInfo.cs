namespace SqliteCompare.Entity
{
    public class AppInfo : BaseNotifyEmtity
    {
        private bool _needCompareIndex = true;
        private string _sourceDbPath;
        private string _targetDbPath;

        public AppInfo()
        {
            AppSetting appSetting = AppSetting.LoadData();
            SourceDbPath = appSetting.SourceDbPath;
            TargetDbPath = appSetting.TargetDbPath;
            NeedCompareIndex = appSetting.NeedCompareIndex;
        }

        /// <summary>
        ///     源数据库的路径
        /// </summary>
        public string SourceDbPath
        {
            get { return _sourceDbPath; }
            set
            {
                _sourceDbPath = value;
                OnPropertyChanged("SourceDbPath");
            }
        }

        /// <summary>
        ///     目标数据库的路径
        /// </summary>
        public string TargetDbPath
        {
            get { return _targetDbPath; }
            set
            {
                _targetDbPath = value;
                OnPropertyChanged("TargetDbPath");
            }
        }

        /// <summary>
        ///     目标数据库的路径
        /// </summary>
        public bool NeedCompareIndex
        {
            get { return _needCompareIndex; }
            set
            {
                _needCompareIndex = value;
                OnPropertyChanged("NeedCompareIndex");
            }
        }
    }
}