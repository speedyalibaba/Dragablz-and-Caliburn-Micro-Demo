using Links.Contract;
using Links.Contract.Services;
using Links.Services;
using Caliburn.Micro;
using Dragablz;
using MahApps.Metro.Controls;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Links
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : MetroWindow
    {
        private static bool _isStartupInitiated;

        public ShellView()
        {
            InitializeComponent();
        }

        private void Tabs_Loaded(object sender, RoutedEventArgs e)
        {
            RestoreTabs();
        }

        private void RestoreTabs()
        {
            if (_isStartupInitiated) return;
            _isStartupInitiated = true;
            var layoutManager = IoC.Get<LayoutManager>();
            layoutManager.Load(Tabs);
            layoutManager.SubscribeIfNotSubscribed();
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            if (!TabablzControl.GetIsClosingAsPartOfDragOperation(this))
                IoC.Get<IApplicationService>().ShutDown();
        }
    }
}
