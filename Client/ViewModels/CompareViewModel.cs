using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AutoFacWrapper;
using Caliburn.Micro;
using SqliteCompare.Entity;
using SqliteCompare.Service.InterFace;

namespace SqliteCompare.Shell
{
    public class CompareViewModel : Conductor<CompareViewModel>.Collection.OneActive
    {
        private readonly ICompareService _service;
        private readonly Dictionary<int, string> DicError = new Dictionary<int, string>();
        private AppInfo _appInfo;
       

        public CompareViewModel()
        {
            _service = ClassFactory.GetInstance<ICompareService>();
            _appInfo = new AppInfo();
            DicError.Add(1, "对象不一致");
            DicError.Add(2, "对象丢失");
            DicError.Add(3, "对象冗余");
        }

        public AppInfo appInfo
        {
            set
            {
                _appInfo = value;
                NotifyOfPropertyChange(() => appInfo);
            }
            get { return _appInfo; }
        }

        public List<UICompareResult> ResultsList { set; get; }

        public void CompareDb()
        {
            _service.CompareDBTable();
            if (new AppInfo().NeedCompareIndex)
                _service.CompareDBIndex();

            ConvertToCompareResult();

            //储存AppSeting
            var builderSetting = AppSetting.LoadData();
            builderSetting.SourceDbPath = appInfo.SourceDbPath;
            builderSetting.TargetDbPath = appInfo.TargetDbPath;
            builderSetting.NeedCompareIndex = appInfo.NeedCompareIndex;
            builderSetting.SaveData();
        }

        /// <summary>
        ///     把查询出的差异转换成UI呈现对象
        /// </summary>
        public void ConvertToCompareResult()
        {
            if (ResultsList == null)
                ResultsList = new List<UICompareResult>();

            ResultsList.Clear();


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
                        UIShow = UIShow + tmp.ToString();
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
                        UIShow = UIShow + tmp.ToString();
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
                    UIShow = UIShow + tmp.ToString();
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
                    UIShow = UIShow + tmp.ToString();
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
                        UIShow = UIShow + tmp.ToString();
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
                        UIShow = UIShow + tmp.ToString();
                    }
                }
            }
        }

        public void ChangeSourceDir()
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            if (string.IsNullOrEmpty(dialog.SelectedPath)) return;
            appInfo.SourceDbPath = dialog.SelectedPath;
        }

        public void ChangeTargetDbPathDir()
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            if (string.IsNullOrEmpty(dialog.SelectedPath)) return;
            appInfo.TargetDbPath = dialog.SelectedPath;
        }

        #region 临时代码 待完善
        private string _uiShow=String.Empty;
        public string UIShow
        {
            get { return _uiShow; }
            set
            {
                _uiShow = value;
                NotifyOfPropertyChange(()=> UIShow);
            }
        }

        #endregion
    }
}