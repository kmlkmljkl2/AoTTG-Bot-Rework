using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoTTG_Bot_Rework.CustomClasses
{
    public class ObservableBool : INotifyPropertyChanged
    {
        private bool myBooleanProperty = false;

        public bool Data
        {
            get { return myBooleanProperty; }
            set
            {
                if (myBooleanProperty != value)
                {
                    myBooleanProperty = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
