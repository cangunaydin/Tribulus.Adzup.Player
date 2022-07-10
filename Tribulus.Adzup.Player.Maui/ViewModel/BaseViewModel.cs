using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tribulus.Adzup.Player.Maui.ViewModel
{
    public partial class BaseViewModel:ObservableObject
    {
        [ObservableProperty]
        string _title;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        bool _isBusy;


        public bool IsNotBusy => !IsBusy;
        public BaseViewModel()
        {
            
        }
    }
}
