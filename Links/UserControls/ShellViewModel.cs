using Links.Contract;
using Links.Contract.Messages;
using Links.WPF;
using Caliburn.Micro;
using Dragablz;
using Dragablz.Dockablz;
using MaterialDesignThemes.Wpf;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Links
{
    public class ShellViewModel : Conductor<IScreen>.Collection.AllActive, IShell,
        IHandle<ConfigurationChanged>
    {
        #region Fields

        private readonly IConfigurationService _configurationService;
        private Guid _dialogIdentifier;
        private MenuViewModel _menuViewModel;
        private User _selectedUser;
        private IObservableCollection<User> _users;
        private bool _isDialogHostOpen;

        #endregion Fields

        #region Constructors

        public ShellViewModel(
            IInterTabClient caliburnInterTabClient,
            IInterLayoutClient caliburnInterLayoutClient,
            ISnackbarMessageQueue mainMessageQueue,
            IConfigurationService configurationService,
            IEventAggregator eventAggregator,
            MenuViewModel menuViewModel)
        {
            CaliburnInterTabClient = caliburnInterTabClient;
            CaliburnInterLayoutClient = caliburnInterLayoutClient;
            MainMessageQueue = mainMessageQueue;
            MenuViewModel = menuViewModel;
            _configurationService = configurationService;
            eventAggregator.Subscribe(this);

            Users = new BindableCollection<User>(_configurationService.Configurations.Select(c => new User { Configuration = c }));
            _selectedUser = Users.SingleOrDefault(u => u.Name == _configurationService.ActiveConfiguration.Name);
            NotifyOfPropertyChange(() => SelectedUser);
        }

        #endregion Constructors

        #region Properties

        public static Func<object> NewItemFactory => () =>
        {
            var vm = IoC.Get<NewViewModel>();
            var view = ViewLocator.LocateForModel(vm, null, null);

            ViewModelBinder.Bind(vm, view, null);
            return view;
        };

        public IInterLayoutClient CaliburnInterLayoutClient { get; }

        public IInterTabClient CaliburnInterTabClient { get; set; }

        public ISnackbarMessageQueue MainMessageQueue { get; set; }

        public MenuViewModel MenuViewModel
        {
            get { return _menuViewModel; }
            set { Set(ref _menuViewModel, value); }
        }

        public User SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                if (this.Set(ref _selectedUser, value) && value != null)
                {
                    Task.Run(() =>
                    {
                        if (!_configurationService.Switch(_selectedUser?.Name))
                            _configurationService.Switch();
                    });
                }
            }
        }

        public IObservableCollection<User> Users
        {
            get { return _users; }
            set { this.Set(ref _users, value); }
        }

        #endregion Properties

        #region Methods

        public void Branch(Button button, Orientation orientation)
        {
            var temp = Layout.GetLoadedInstances();

            var parent = VisualTreeHelper.GetParent(button);
            while (parent != null)
            {
                if (parent is TabablzControl)
                {
                    var newTabablz = CaliburnInterLayoutClient.GetNewHost(null, parent as TabablzControl);
                    var branchResult = Layout.Branch(parent as TabablzControl, newTabablz.TabablzControl, orientation, false, .5);

                    var newVm = NewItemFactory();

                    branchResult.TabablzControl.AddToSource(newVm);
                    branchResult.TabablzControl.SelectedItem = newVm;

                    break;
                }

                parent = VisualTreeHelper.GetParent(parent);
            }
        }

        public void BranchHorizontal(Button button)
        {
            Branch(button, Orientation.Horizontal);
        }

        public void BranchVertical(Button button)
        {
            Branch(button, Orientation.Vertical);
        }

        public void DialogHostLoaded(RoutedEventArgs e)
        {
            var host = e.Source as DialogHost;
            _dialogIdentifier = Guid.NewGuid();
            host.Identifier = _dialogIdentifier.ToString();

            MenuViewModel.ShellDialogHostIdentifier = _dialogIdentifier;
        }

        public bool IsDialogHostOpen
        {
            get { return _isDialogHostOpen; }
            set { Set(ref _isDialogHostOpen, value); }
        }

        public async Task EditUsers()
        {
            IsDialogHostOpen = false;

            var vm = IoC.Get<EditUsersViewModel>();
            var view = ViewLocator.LocateForModel(vm, null, null);

            ViewModelBinder.Bind(vm, view, null);
            await DialogHost.Show(view, _dialogIdentifier.ToString());
        }

        public void Handle(ConfigurationChanged message)
        {
            Task.Run(() =>
            {
                Users = new BindableCollection<User>(_configurationService.Configurations.Select(c => new User { Configuration = c }));

                _selectedUser = Users.SingleOrDefault(u => u.Name == _configurationService.ActiveConfiguration.Name);
                NotifyOfPropertyChange(() => SelectedUser);
            });
        }

        #endregion Methods

        #region Classes

        public class User : Screen
        {
            #region Fields

            private Configuration _configuration = new Configuration();
            private string _name;

            #endregion Fields

            #region Properties

            public Configuration Configuration
            {
                get { return _configuration; }
                set
                {
                    if (value != Configuration)
                    {
                        _configuration = value;
                        Name = _configuration?.Name;
                    }
                }
            }

            public string Name
            {
                get { return _name; }
                set
                {
                    if (Set(ref _name, value))
                    {
                        Configuration.Name = value;
                    }
                }
            }

            #endregion Properties
        }

        #endregion Classes
    }
}