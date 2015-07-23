using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using AutoFacWrapper;
using Caliburn.Micro;
using Microsoft.Win32;
using SqliteCompare.Entity;
using SqliteCompare.Service.InterFace;

using Threads;

namespace SqliteCompare.Views
{
    public class CompareViewModel : Conductor<IScreen>.Collection.OneActive, IScreen, ICompareViewModel
    {
        public CompareViewModel()
        {
            #region 让Caliburn 知道

            Assembly assembly=Assembly.GetExecutingAssembly();
            if (!AssemblySource.Instance.Contains(assembly))
            {
                AssemblySource.Instance.Add(assembly);
            }

            #endregion


            _service = ClassFactory.GetInstance<ICompareService>();
            _appInfo = new AppInfo();
            DicError.Add(1, "对象不一致");
            DicError.Add(2, "对象丢失");
            DicError.Add(3, "对象冗余");
            ResultsList = new ObservableCollection<UICompareResult>();
        }

        #region 临时代码 待完善

        public string UIShow
        {
            get { return _uiShow; }
            set
            {
                _uiShow = value;
                NotifyOfPropertyChange(() => UIShow);
            }
        }

        #endregion

        public void CompareDB()
        {
            //储存AppSeting
            var builderSetting = AppSetting.LoadData();
            builderSetting.SourceDbPath = appInfo.SourceDbPath;
            builderSetting.TargetDbPath = appInfo.TargetDbPath;
            builderSetting.NeedCompareIndex = appInfo.NeedCompareIndex;
            builderSetting.SaveData();

            WpfTask.FactoryStartNew(() =>
            {
                UIThread.Invoke(() => { IsComparing = true; });
                _service.CompareDBTable();

                if (new AppInfo().NeedCompareIndex)
                    _service.CompareDBIndex();

                UIThread.Invoke(ConvertToCompareResult);
                UIThread.Invoke(() => { IsComparing = false; });
            });
        }

        public void RepairDB()
        {
            if (_service.DifTableList == null && _service.DifIndexList == null)
                return;

            var repareCmdList = new List<string>();

            #region 修复表

            var tableList = _service.DifTableList;
            var IndexList = _service.DifIndexList;

            #region 修复字段

            WpfTask.FactoryStartNew(() =>
            {
                UIThread.Invoke(() => { IsRepairing = true; });
                /*先删除字段 在新增
                //SQLite 没有原生的删除字段的方法 只能变通
                //把除了需要删除的字段意外的字段缓存，删除原表，重命名新表为旧表名*/
                //字段冗余
                foreach (var info in tableList.Where(x => x.MoreCol != null && x.MoreCol.Any()))
                {
                    //需要缓存的字段
                    var copycol = string.Empty;
                    foreach (var colinfo in info.TargetCol)
                    {
                        if (info.MoreCol.Any(x => x.Name == colinfo.Name))
                            continue;
                        copycol = copycol + colinfo.Name + ",";
                    }

                    if (copycol.Length > 0)
                        copycol = copycol.Substring(0, copycol.Length - 1);

                    var cmdBuilder = new StringBuilder();

                    cmdBuilder.AppendLine(string.Format("Create Table {0}_TempNew as Select {1} from {0};",
                        info.TargetInfo.name,
                        copycol));
                    cmdBuilder.AppendLine(string.Format("Drop table if Exists {0};", info.TargetInfo.name));
                    cmdBuilder.AppendLine(string.Format("Alter table {0}_TempNew Rename to {0};", info.TargetInfo.name));
                    repareCmdList.Add(cmdBuilder.ToString());
                }
                //缺少字段
                foreach (var info in tableList.Where(x => x.LostCol != null && x.LostCol.Any()))
                {
                    foreach (var lost in info.LostCol)
                    {
                        var cmdBuilder = new StringBuilder();

                        var notnull = string.Empty;
                        if (lost.NotNull != "0")
                            notnull = "not null";
                        var defaultValue = string.Empty;
                        if (lost.Dflt_Value != null)
                            defaultValue = "default " + "'" + lost.Dflt_Value + "'";
                        cmdBuilder.AppendLine(string.Format("Alter table {0} add column {1} {2} {3} {4};",
                            info.SourceInfo.name,
                            lost.Name, lost.Type, notnull, defaultValue));
                        repareCmdList.Add(cmdBuilder.ToString());
                    }
                }
                //缺少表
                repareCmdList.AddRange(tableList.Where(x => x.ErrorType == 2).Select(info => info.SourceInfo.sql));
                //表冗余
                repareCmdList.AddRange(
                    tableList.Where(x => x.ErrorType == 3)
                        .Select(info => string.Format("Drop table if Exists {0} ", info.TargetInfo.name)));

                #endregion

                #endregion

                #region 修复索引

                //缺少索引
                repareCmdList.AddRange(IndexList.Where(x => x.ErrorType == 2).Select(info => info.SourceInfo.sql));
                //索引 冗余
                repareCmdList.AddRange(
                    IndexList.Where(x => x.ErrorType == 3)
                        .Select(info => string.Format("Drop INDEX if Exists {0} ", info.TargetInfo.name)));

                #endregion

                _service.RepairDb(repareCmdList, tmp =>
                {
#if DEBUG
                    Thread.Sleep(100);
#endif
                    UIThread.Invoke(() => { RepairedNum = tmp; });
                });
                UIThread.Invoke(() => { IsRepairing = false; });
            });
        }

        /// <summary>
        ///     把查询出的差异转换成UI呈现对象
        /// </summary>
        public void ConvertToCompareResult()
        {
            ResultsList.Clear();
            RepairedNum = 0;
            UIShow = "";

            foreach (var re in _service.DifTableList)
            {
                if (re.ErrorType == 1)
                {
                    //丢失的列
                    foreach (var col in re.LostCol)
                    {
                        var tmp = new UICompareResult
                        {
                            ObjectName = re.SourceInfo.name,
                            ObjectType = re.SourceInfo.type,
                            ErrorItem = col.Name,
                            ErrorItemType = "col",
                            ErrorType = DicError[2]
                        };
                        ResultsList.Add(tmp);
                        UIShow = UIShow + tmp;
                    }
                    //冗余的列

                    foreach (var col in re.MoreCol)
                    {
                        var tmp = new UICompareResult
                        {
                            ObjectName = re.SourceInfo.name,
                            ObjectType = re.SourceInfo.type,
                            ErrorItem = col.Name,
                            ErrorItemType = "col",
                            ErrorType = DicError[3]
                        };
                        ResultsList.Add(tmp);
                        UIShow = UIShow + tmp;
                    }
                }

                if (re.ErrorType == 2)
                {
                    var tmp = new UICompareResult
                    {
                        ObjectName = re.SourceInfo.name,
                        ObjectType = re.SourceInfo.type,
                        ErrorItem = re.SourceInfo.name,
                        ErrorItemType = re.SourceInfo.type,
                        ErrorType = DicError[re.ErrorType]
                    };
                    ResultsList.Add(tmp);
                    UIShow = UIShow + tmp;
                }

                if (re.ErrorType == 3)
                {
                    var tmp = new UICompareResult
                    {
                        ObjectName = re.TargetInfo.name,
                        ObjectType = re.TargetInfo.type,
                        ErrorItem = re.TargetInfo.name,
                        ErrorItemType = re.TargetInfo.type,
                        ErrorType = DicError[re.ErrorType]
                    };

                    ResultsList.Add(tmp);
                    UIShow = UIShow + tmp;
                }
            }
            if (new AppInfo().NeedCompareIndex)
            {
                foreach (var re in _service.DifIndexList)
                {
                    //如果是丢失 对象信息从源对象取
                    if (re.ErrorType == 2)
                    {
                        var tmp = new UICompareResult
                        {
                            ObjectName = re.SourceInfo.name,
                            ObjectType = re.SourceInfo.type,
                            ErrorItem = re.SourceInfo.name,
                            ErrorItemType = re.SourceInfo.type,
                            ErrorType = DicError[re.ErrorType]
                        };


                        ResultsList.Add(tmp);
                        UIShow = UIShow + tmp;
                    }
                    //如果是冗余 对象信息从源对象取
                    if (re.ErrorType == 3)
                    {
                        var tmp = new UICompareResult
                        {
                            ObjectName = re.TargetInfo.name,
                            ObjectType = re.TargetInfo.type,
                            ErrorItem = re.TargetInfo.name,
                            ErrorItemType = re.TargetInfo.type,
                            ErrorType = DicError[re.ErrorType]
                        };
                        ResultsList.Add(tmp);
                        UIShow = UIShow + tmp;
                    }
                }
            }
            UIThread.Invoke(() =>
            {
                NeedRepairNum = ResultsList.Count;
                NotifyOfPropertyChange(() => CanRepairDB);
            });
        }

        public void ChangeSourceDir()
        {
            var dialog = new OpenFileDialog();
            var result = dialog.ShowDialog();
            if (result.HasValue && !result.Value) return;
            appInfo.SourceDbPath = dialog.FileName;
        }

        public void ChangeTargetDbPathDir()
        {
            var dialog = new OpenFileDialog();
            var result = dialog.ShowDialog();
            if (result.HasValue && !result.Value) return;
            appInfo.TargetDbPath = dialog.FileName;
        }

        #region 字段

        private string _uiShow = string.Empty;
        private int _needRepairNum;
        private int _repairedNum;
        private ObservableCollection<UICompareResult> _resultsList;
        private bool _canRepairDb;
        private bool _CanCompareDB;

        public AppInfo appInfo
        {
            set
            {
                _appInfo = value;
                NotifyOfPropertyChange(() => appInfo);
            }
            get { return _appInfo; }
        }

        private readonly ICompareService _service;
        private readonly Dictionary<int, string> DicError = new Dictionary<int, string>();
        private AppInfo _appInfo;
        private bool _isComparing;
        private bool _isRepairing;


        public ObservableCollection<UICompareResult> ResultsList
        {
            set
            {
                _resultsList = value;
                NotifyOfPropertyChange(() => ResultsList);
            }
            get { return _resultsList; }
        }


        /// <summary>
        ///     缓存对比信息
        /// </summary>
        public List<TableCompareResult> TableResultsList { set; get; }

        public List<IndexCompareResult> IndexResultsList { set; get; }


        public bool CanRepairDB
        {
            get { return (NeedRepairNum - RepairedNum) > 0; }
        }

        public bool CanCompareDB
        {
            get { return (RepairedNum == NeedRepairNum); }
        }

        /// <summary>
        ///     共多少需要修复
        /// </summary>
        public int NeedRepairNum
        {
            set
            {
                _needRepairNum = value;
                NotifyOfPropertyChange(() => NeedRepairNum);
                NotifyOfPropertyChange(() => CanCompareDB);
                NotifyOfPropertyChange(() => CanRepairDB);
            }
            get { return _needRepairNum; }
        }

        /// <summary>
        ///     已修复
        /// </summary>
        public int RepairedNum
        {
            set
            {
                _repairedNum = value;
                NotifyOfPropertyChange(() => RepairedNum);
                NotifyOfPropertyChange(() => CanCompareDB);
                NotifyOfPropertyChange(() => CanRepairDB);
            }
            get { return _repairedNum; }
        }

        /// <summary>
        ///     正在对比数据库标志
        /// </summary>
        public bool IsComparing
        {
            get { return _isComparing; }
            set
            {
                _isComparing = value;
                NotifyOfPropertyChange(() => IsComparing);
            }
        }

        /// <summary>
        ///     正在修复
        /// </summary>
        public bool IsRepairing
        {
            get { return _isRepairing; }
            set
            {
                _isRepairing = value;
                NotifyOfPropertyChange(() => IsRepairing);
            }
        }

        #endregion
    }
}