using System;

namespace SqliteCompare.Entity
{
    public class UICompareResult : BaseNotifyEmtity
    {
        private string _errorItem;
        private string _errorItemType;
        private string _errorType;
        private string _objectName;
        private string _objectType;

        /// <summary>
        ///     对象名称  【表名或者索引名】
        /// </summary>
        public string ObjectName
        {
            get { return _objectName; }
            set
            {
                _objectName = value;
                OnPropertyChanged("ObjectType");
            }
        }

        /// <summary>
        ///     对象类型
        /// </summary>
        public string ObjectType
        {
            get { return _objectType; }
            set
            {
                _objectType = value;
                OnPropertyChanged("ObjectType");
            }
        }

        /// <summary>
        ///     出现错误的对象名称
        /// </summary>
        public string ErrorItem
        {
            get { return _errorItem; }
            set
            {
                _errorItem = value;
                OnPropertyChanged("ObjectType");
            }
        }

        /// <summary>
        ///     出现错误的对象名称
        /// </summary>
        public string ErrorItemType
        {
            get { return _errorItemType; }
            set
            {
                _errorItemType = value;
                OnPropertyChanged("ObjectType");
            }
        }

        /// <summary>
        ///     错误类型
        /// </summary>
        public string ErrorType
        {
            get { return _errorType; }
            set
            {
                _errorType = value;
                OnPropertyChanged("ObjectType");
            }
        }


        public override string ToString()
        {
            return String.Format("-->【ObjectName:{0} | ObjectType:{1} | ErrorItem:{2} | ErrorItemType:{3} | ErrorType:{4} 】 {5}",
                ObjectName,ObjectType,ErrorItem,ErrorItemType,ErrorType,Environment.NewLine);
        }
    }
}