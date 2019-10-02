using Links.Localization;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Links.WPF
{
    public class NewViewModel : Conductor<IScreen>
    {
        public NewViewModel()
        {
            DisplayName = Translations.NewWindow;
        }

        public void NavigateToTest1()
        {
            NavigateTo<Test1ViewModel>();
        }

        public void NavigateToTest2()
        {
            NavigateTo<Test2ViewModel>();
        }

        public void NavigateTo<T>() where T:IScreen
        {
            var vm = IoC.Get<T>();
            DisplayName = vm.DisplayName;
            ActivateItem(vm);
        }
    }
}
