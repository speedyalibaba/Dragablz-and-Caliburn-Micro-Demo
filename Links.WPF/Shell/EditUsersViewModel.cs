using AutoMapper;
using Links.Contract;
using Links.Contract.Messages;
using Links.Contract.Services;
using Links.Localization;
using Caliburn.Micro;
using System.Dynamic;
using System.Linq;
using System.Windows.Input;

namespace Links.WPF
{
    public class EditUsersViewModel : Screen
    {
        #region Fields

        private readonly IConfigurationService _configurationService;
        private readonly IDialogManager _dialogManager;
        private readonly IMapper _mapper;
        private string _newName;
        private BindableCollection<User> _users;
        private readonly IEventAggregator _eventAggregator;
        private readonly ILayoutManager _layoutManager;

        #endregion Fields

        #region Constructors

        public EditUsersViewModel(IMapper mapper,
            IDialogManager dialogManager,
            ILayoutManager layoutManager,
            IConfigurationService configurationService,
            IEventAggregator eventAggregator)
        {
            DisplayName = Translations.EditUsers;
            _dialogManager = dialogManager;
            _mapper = mapper;
            _configurationService = configurationService;
            _eventAggregator = eventAggregator;
            _layoutManager = layoutManager;

            Users = new BindableCollection<User>(configurationService.Configurations.Select(c => new User { Configuration = c }));
        }

        #endregion Constructors

        #region Properties

        public bool CanAddUser => !string.IsNullOrWhiteSpace(NewName) && !Users.Select(u => u.Name).Contains(NewName);

        public string NewName
        {
            get { return _newName; }
            set
            {
                if (Set(ref _newName, value))
                    NotifyOfPropertyChange(() => CanAddUser);
            }
        }

        public BindableCollection<User> Users
        {
            get { return _users; }
            set { Set(ref _users, value); }
        }

        #endregion Properties

        #region Methods

        public void AddUser()
        {
            var newUser = new User
            {
                Name = NewName
            };

            //Wenn gerade nur eine temporäre config aktiv ist soll die neue von der kopiert werden
            if (!_configurationService.Configurations.Contains(_configurationService.ActiveConfiguration))
            {
                newUser.Configuration = _configurationService.ActiveConfiguration;
                newUser.Name = NewName;
            }

            Users.Add(newUser);
            _configurationService.AddConfiguration(newUser.Configuration);

            NewName = string.Empty;
        }

        public void DeleteUser(User user)
        {
            Users.Remove(user);
            _configurationService.RemoveConfiguration(user.Name);
        }

        public void Duplicate(User user)
        {
            var newUser = _mapper.Map<User>(user);

            string newName = user.Name;
            int i = 0;
            do
            {
                i++;
                newName = $"{user.Name}({i})";
            } while (Users.Select(u => u.Name).Contains(newName));
            newUser.Name = newName;

            int indexToInsert = Users.IndexOf(user) + 1;
            Users.Insert(indexToInsert, newUser);
            _configurationService.AddConfiguration(newUser.Configuration, indexToInsert);
        }

        public void EditKeyPressed(KeyEventArgs e)
        {
            User user = (e.Source as System.Windows.Controls.TextBox).DataContext as User;
            if (user.IsEditingEnabled && (e.Key == Key.Enter || e.Key == Key.Escape))
            {
                if (e.Key == Key.Escape)    //reset name
                {
                    user.Name = user.OldName;
                    e.Handled = true;
                }

                EditUser(user);
            }
        }

        public void EditUser(User user)
        {
            if (user.IsEditingEnabled)
            {
                _eventAggregator.PublishOnUIThread(new ConfigurationChanged());
            }
            user.OldName = user.Name;

            user.IsEditingEnabled = !user.IsEditingEnabled;
        }

        public void NameKeyPressed(KeyEventArgs e)
        {
            if (e.Key == Key.Enter && CanAddUser)
            {
                AddUser();
            }
        }

        public void OpenSettings(User user)
        {
            var dialogViewModel = IoC.Get<SettingsViewModel>();
            var dialogView = ViewLocator.LocateForModel(dialogViewModel, null, null);

            ViewModelBinder.Bind(dialogViewModel, dialogView, null);

            var previousLanguage = _configurationService.ActiveConfiguration.Language;
            var isActiveConfig = _configurationService.ActiveConfiguration == user.Configuration;
            dialogViewModel.Init(user.Configuration, isActiveConfig);
            if (_dialogManager.ShowDialog(dialogViewModel, dialogViewModel.WindowSettings))
            {
                _configurationService.UpdateConfiguration(user.Name, dialogViewModel.Configuration);
                user.Configuration = dialogViewModel.Configuration;
                if (isActiveConfig && previousLanguage != _configurationService.ActiveConfiguration.Language)
                {
                    if (_dialogManager.ShowMessageBox(Translations.DoYouWantToChangeTheLanguageNowMessage, Translations.Language, MessageBoxOptions.YesNo))
                    {
                        _layoutManager.Reload();
                    }
                }
            }
            else
            {
                dialogViewModel.RestoreColors();
            }
        }

        #endregion Methods

        #region Classes

        public class User : Screen
        {
            #region Fields

            private Configuration _configuration = new Configuration();
            private bool _isEditingEnabled;
            private string _name;

            #endregion Fields

            #region Properties

            public bool CanSaveName => !string.IsNullOrEmpty(Name);

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

            public bool IsEditingEnabled
            {
                get { return _isEditingEnabled; }
                set { Set(ref _isEditingEnabled, value); }
            }

            public string Name
            {
                get { return _name; }
                set
                {
                    if (Set(ref _name, value))
                    {
                        Configuration.Name = value;
                        NotifyOfPropertyChange(() => CanSaveName);
                    }
                }
            }

            public string OldName { get; set; }

            #endregion Properties
        }

        #endregion Classes
    }
}