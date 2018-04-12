
using System.ComponentModel;

namespace WPGIS.DataType
{
    /// <summary>
    /// ViewModelbase
    /// </summary>
    public class ViewModelbase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
