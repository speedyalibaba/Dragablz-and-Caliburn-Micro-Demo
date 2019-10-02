using Links.Contract;
using Links.Contract.Messages;
using Links.Contract.Models;
using Links.Localization;
using Links.Services;
using Links.WPF;
using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Links.Contract.Services;
using MaterialDesignExtensions.Controls;

namespace Links
{
    public class MenuViewModel : Screen,
        IHandle<LayoutsChanged>,
        IHandle<ConfigurationChanged>
    {
        #region Fields

        private static bool _showPreviousLayout = true;
        private readonly IApplicationService _applicationService;
        private readonly IConfigurationService _configurationService;
        private readonly IDialogManager _dialogManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly LayoutManager _layoutManager;
        private Guid _dialogIdentifier;
        private bool _isEditingLayoutsEnabled;
        private bool _isMenuOpen;
        private IObservableCollection<LayoutStructure> _layouts;
        private string _newLayoutName;
        private LayoutStructure _selectedLayout;
        private LayoutStructure _selectedPreset;

        #endregion Fields

        #region Constructors

        public MenuViewModel(
            IEventAggregator eventAggregator,
            IApplicationService applicationService,
            IConfigurationService configurationService,
            LayoutManager layoutManager,
            IDialogManager dialogManager)
        {
            _eventAggregator = eventAggregator;
            _applicationService = applicationService;
            _configurationService = configurationService;
            _layoutManager = layoutManager;
            _dialogManager = dialogManager;

            eventAggregator.Subscribe(this);

            LoadSettings();
        }

        #endregion Constructors

        #region Properties

        public bool IsEditingLayoutsEnabled
        {
            get { return _isEditingLayoutsEnabled; }
            set { Set(ref _isEditingLayoutsEnabled, value); }
        }

        public bool IsMenuOpen
        {
            get { return _isMenuOpen; }
            set
            {
                Set(ref _isMenuOpen, value);
            }
        }

        public IObservableCollection<LayoutStructure> Layouts
        {
            get
            {
                if (_layouts == null)
                {
                    var layouts = _configurationService.ActiveConfiguration.Layouts.Select(LayoutManager.DeserializeStructure).ToList();
                    var lastLayout = LayoutManager.DeserializeStructure(_configurationService.ActiveConfiguration.LastLayout);
                    if (lastLayout != null && _showPreviousLayout)
                    {
                        layouts.Add(lastLayout);
                    }

                    _layouts = new BindableCollection<LayoutStructure>(layouts);
                }
                return _layouts;
            }
        }

        public string NewLayoutName
        {
            get { return _newLayoutName; }
            set { Set(ref _newLayoutName, value); }
        }

        public IObservableCollection<LayoutStructure> Presets
        {
            get
            {
                return new BindableCollection<LayoutStructure>(LayoutLibrary.Presets.Where(p => !_configurationService.ActiveConfiguration.PresetsToIgnore.Contains(p.Name)));
            }
        }

        public LayoutStructure SelectedLayout
        {
            get { return _selectedLayout; }
            set
            {
                if (Set(ref _selectedLayout, value) && value != null)
                    _layoutManager.LoadAndSavePrevious(value);
            }
        }

        public LayoutStructure SelectedPreset
        {
            get { return _selectedPreset; }
            set
            {
                if (Set(ref _selectedPreset, value) && value != null)
                    _layoutManager.LoadAndSavePrevious(value);
            }
        }

        public Guid ShellDialogHostIdentifier { get; internal set; }

        #endregion Properties

        #region Methods

        public async Task AddLayout()
        {
            string name = string.IsNullOrWhiteSpace(NewLayoutName) ? GetNewLayoutName() : NewLayoutName;
            var currentLayout = LayoutAnalayzer.GetLayoutStructure(name);
            _configurationService.ActiveConfiguration.Layouts.Add(LayoutManager.SerializeStructure(currentLayout));
            _configurationService.Save();
            NewLayoutName = string.Empty;

            await _eventAggregator.PublishOnUIThreadAsync(new LayoutsChanged());
        }

        public async Task DeleteLayout(LayoutStructure layoutStructure)
        {
            var serializedStructure = LayoutManager.SerializeStructure(layoutStructure);

            if (_configurationService.ActiveConfiguration.LastLayout == serializedStructure)
            {
                _showPreviousLayout = false;
            }

            if (_configurationService.ActiveConfiguration.Layouts.Contains(serializedStructure))
            {
                _configurationService.ActiveConfiguration.Layouts.Remove(serializedStructure);
                _configurationService.Save();
            }

            await _eventAggregator.PublishOnUIThreadAsync(new LayoutsChanged());
        }

        public void DialogHostLoaded(RoutedEventArgs e)
        {
            var host = e.Source as DialogHost;
            _dialogIdentifier = Guid.NewGuid();
            host.Identifier = _dialogIdentifier;
        }

        public void Handle(LayoutsChanged message)
        {
            _layouts = null;
            NotifyOfPropertyChange(() => Layouts);
        }

        public void Handle(ConfigurationChanged message)
        {
            LoadSettings();

            _layouts = null;
            NotifyOfPropertyChange(() => Layouts);
            NotifyOfPropertyChange(() => Presets);
        }

        public async Task MoveDown(LayoutStructure layoutStructure)
        {
            var serializedStructure = LayoutManager.SerializeStructure(layoutStructure);

            if (_configurationService.ActiveConfiguration.Layouts.Contains(serializedStructure))
            {
                var index = _configurationService.ActiveConfiguration.Layouts.IndexOf(serializedStructure);
                var maxIndex = _configurationService.ActiveConfiguration.Layouts.Count - 1;
                _configurationService.ActiveConfiguration.Layouts.Remove(serializedStructure);
                _configurationService.ActiveConfiguration.Layouts.Insert(index < maxIndex ? index + 1 : index, serializedStructure);
                _configurationService.Save();
            }

            await _eventAggregator.PublishOnUIThreadAsync(new LayoutsChanged());
        }

        public async Task MoveUp(LayoutStructure layoutStructure)
        {
            var serializedStructure = LayoutManager.SerializeStructure(layoutStructure);

            if (_configurationService.ActiveConfiguration.Layouts.Contains(serializedStructure))
            {
                var index = _configurationService.ActiveConfiguration.Layouts.IndexOf(serializedStructure);
                _configurationService.ActiveConfiguration.Layouts.Remove(serializedStructure);
                _configurationService.ActiveConfiguration.Layouts.Insert(index != 0 ? index - 1 : index, serializedStructure);
                _configurationService.Save();
            }

            await _eventAggregator.PublishOnUIThreadAsync(new LayoutsChanged());
        }

        public async Task NameKeyPressed(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await AddLayout();
            }
        }

        public async Task OpenFile()
        {
            // open file
            OpenFileDialogArguments dialogArgs = new OpenFileDialogArguments()
            {
                Width = 600,
                Height = 500,
                Filters = "All files (*.*)|*.*|Excel Worksheets (*.xls, *.xlsx)|*.xls;*.xlsx"
            };

            OpenFileDialogResult result = await OpenFileDialog.ShowDialogAsync(ShellDialogHostIdentifier.ToString(), dialogArgs);
        }

        public void OpenSettings()
        {
            IsMenuOpen = false;

            var dialogViewModel = IoC.Get<SettingsViewModel>();
            var dialogView = ViewLocator.LocateForModel(dialogViewModel, null, null);

            ViewModelBinder.Bind(dialogViewModel, dialogView, null);

            var previousLanguage = _configurationService.ActiveConfiguration.Language;
            dialogViewModel.Init(_configurationService.ActiveConfiguration, true);
            if (_dialogManager.ShowDialog(dialogViewModel, dialogViewModel.WindowSettings))
            {
                _configurationService.UpdateConfiguration(_configurationService.ActiveConfiguration.Name, dialogViewModel.Configuration);
                if (previousLanguage != _configurationService.ActiveConfiguration.Language)
                {
                    if (_dialogManager.ShowMessageBox(Translations.DoYouWantToChangeTheLanguageNowMessage, Translations.Language, Contract.Services.MessageBoxOptions.YesNo))
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

        public async Task OverrideLayout(LayoutStructure layoutStructure)
        {
            var currentLayout = LayoutAnalayzer.GetLayoutStructure(layoutStructure.Name);
            var oldSerializedStructure = LayoutManager.SerializeStructure(layoutStructure);
            var currentSerializedStructure = LayoutManager.SerializeStructure(currentLayout);

            if (_configurationService.ActiveConfiguration.Layouts.Contains(oldSerializedStructure))
            {
                var index = _configurationService.ActiveConfiguration.Layouts.IndexOf(oldSerializedStructure);
                _configurationService.ActiveConfiguration.Layouts.Remove(oldSerializedStructure);
                _configurationService.ActiveConfiguration.Layouts.Insert(index, currentSerializedStructure);
                _configurationService.Save();
            }

            await _eventAggregator.PublishOnUIThreadAsync(new LayoutsChanged());
        }

        public async Task RenameLayout(LayoutStructure layoutStructure)
        {
            var vm = IoC.Get<RenameViewModel>();
            var view = ViewLocator.LocateForModel(vm, null, null);
            vm.Name = layoutStructure.Name;
            vm.Init();

            ViewModelBinder.Bind(vm, view, null);

            var result = await DialogHost.Show(view, _dialogIdentifier);

            if ((bool)result)
            {
                var serializedStructure = LayoutManager.SerializeStructure(layoutStructure);
                layoutStructure.Name = vm.Name;
                var renamedSerializedStructure = LayoutManager.SerializeStructure(layoutStructure);

                if (_configurationService.ActiveConfiguration.Layouts.Contains(serializedStructure))
                {
                    var index = _configurationService.ActiveConfiguration.Layouts.IndexOf(serializedStructure);
                    _configurationService.ActiveConfiguration.Layouts.Remove(serializedStructure);
                    _configurationService.ActiveConfiguration.Layouts.Insert(index, renamedSerializedStructure);
                    _configurationService.Save();
                }

                await _eventAggregator.PublishOnUIThreadAsync(new LayoutsChanged());
            }
        }

        public void Shutdown()
        {
            _applicationService.ShutDown();
        }

        private string GetNewLayoutName()
        {
            string defaultName = "NewLayout";
            string newName = defaultName;
            int i = 0;
            while (Layouts.Select(l => l.Name).Contains(newName))
            {
                i++;
                newName = $"{defaultName}({i})";
            }
            return newName;
        }

        private void LoadSettings()
        {
            _showPreviousLayout = _configurationService.ActiveConfiguration.ShowLastLayout;
            IsEditingLayoutsEnabled = _configurationService.ActiveConfiguration.AllowEditingLayouts;
        }

        #endregion Methods
    }
}