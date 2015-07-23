using System.ComponentModel;
using SqliteCompare.Entity.Annotations;

namespace SqliteCompare.Entity
{
    public class BaseNotifyEmtity : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}