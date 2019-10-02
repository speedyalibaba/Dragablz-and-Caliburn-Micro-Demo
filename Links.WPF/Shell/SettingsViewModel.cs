using AutoMapper;
using Links.Contract;
using Links.Contract.Models;
using Links.Contract.Services;
using Links.Localization;
using Caliburn.Micro;
using MaterialDesignColors;
using MaterialDesignExtensions.Controls;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Links.Common.Base;
using Links.Common;

namespace Links.WPF
{
    public class SettingsViewModel : DirtyScreen<SettingsViewModel>
    {
        #region Fields

        private readonly IApplicationService _applicationService;
        private readonly IConfigurationService _configurationService;
        private readonly IEventAggregator _eventAggregator;
        private System.Windows.Media.Color _accentColor;
        private System.Windows.Media.Color _accentForeground;
        private bool _allowEditingLayouts;
        private bool _darkMode;
        private bool _isConfigurationActive;
        private string _language;
        private List<string> _presetsToIgnore;
        private System.Windows.Media.Color _primaryColor;
        private System.Windows.Media.Color _primaryDarkColor;
        private System.Windows.Media.Color _primaryDarkForeground;
        private System.Windows.Media.Color _primaryForeground;
        private System.Windows.Media.Color _primaryLightColor;
        private System.Windows.Media.Color _primaryLightForeground;
        private bool _showLastLayout;
        private int _transitioneIndex;
        private string _userName;

        #endregion Fields

        #region Constructors

        public SettingsViewModel(
            IMapper mapper,
            IApplicationService applicationService,
            IConfigurationService configurationService,
            IEventAggregator eventAggregator,
            ISnackbarMessageQueue snackbarMessageQueue) : base(mapper)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
            _configurationService = configurationService;
            _applicationService = applicationService;
            DisplayName = Translations.Settings;
            SnackbarMessageQueue = snackbarMessageQueue;
            Swatches = new SwatchesProvider().Swatches;
        }

        #endregion Constructors

        #region Properties

        public string AccentColor { get; set; }

        public System.Windows.Media.Color AccentMidColor
        {
            get { return _accentColor; }
            set { Set(ref _accentColor, value); }
        }

        public System.Windows.Media.Color AccentMidForeground
        {
            get { return _accentForeground; }
            set { Set(ref _accentForeground, value); }
        }

        public bool AllowEditingLayouts
        {
            get { return _allowEditingLayouts; }
            set { Set(ref _allowEditingLayouts, value); }
        }

        public bool CanSave => IsDirty;

        public Configuration Configuration { get; set; }

        public bool DarkMode
        {
            get { return _darkMode; }
            set
            {
                if (Set(ref _darkMode, value))
                {
                    if (_isConfigurationActive)
                    {
                        new PaletteHelper().SetLightDark(value);
                    }
                }
            }
        }

        public string Language
        {
            get { return _language; }
            set { Set(ref _language, value); }
        }

        [AlwaysClean]
        public IObservableCollection<string> Languages => new BindableCollection<string>(LanguageHelper.Languages);

        [AlwaysClean]
        public IObservableCollection<SelectableItem<LayoutStructure>> Presets
        {
            get
            {
                var items = LayoutLibrary.Presets.Select(p => new SelectableItem<LayoutStructure>()
                {
                    Item = p,
                    IsSelected = !PresetsToIgnore?.Contains(p.Name) ?? true,
                    ItemSelected = PresetToIgnoreSelected,
                    ItemUnselected = PresetToIgnoreUnselected
                });
                return new BindableCollection<SelectableItem<LayoutStructure>>(items);
            }
        }

        public List<string> PresetsToIgnore
        {
            get { return _presetsToIgnore; }
            set { Set(ref _presetsToIgnore, value); }
        }

        public string PresetsToIgnoreToString => string.Concat(PresetsToIgnore.OrderBy(s => s).ToArray());
        public string PrimaryColor { get; set; }

        public System.Windows.Media.Color PrimaryDarkColor
        {
            get { return _primaryDarkColor; }
            set { Set(ref _primaryDarkColor, value); }
        }

        public System.Windows.Media.Color PrimaryDarkForeground
        {
            get { return _primaryDarkForeground; }
            set { Set(ref _primaryDarkForeground, value); }
        }

        public System.Windows.Media.Color PrimaryLightColor
        {
            get { return _primaryLightColor; }
            set { Set(ref _primaryLightColor, value); }
        }

        public System.Windows.Media.Color PrimaryLightForeground
        {
            get { return _primaryLightForeground; }
            set { Set(ref _primaryLightForeground, value); }
        }

        public System.Windows.Media.Color PrimaryMidColor
        {
            get { return _primaryColor; }
            set { Set(ref _primaryColor, value); }
        }

        public System.Windows.Media.Color PrimaryMidForeground
        {
            get { return _primaryForeground; }
            set { Set(ref _primaryForeground, value); }
        }

        public bool ShowLastLayout
        {
            get { return _showLastLayout; }
            set { Set(ref _showLastLayout, value); }
        }

        [AlwaysClean]
        public ISnackbarMessageQueue SnackbarMessageQueue { get; }

        [AlwaysClean]
        public IEnumerable<Swatch> Swatches { get; }

        [AlwaysClean]
        public int TransitionerIndex
        {
            get { return _transitioneIndex; }
            set { Set(ref _transitioneIndex, value); }
        }

        public string UserName  //TODO: maybe Validate not null
        {
            get { return _userName; }
            set { Set(ref _userName, value); }
        }

        [AlwaysClean]
        public ExpandoObject WindowSettings
        {
            get
            {
                dynamic windowSettings = new ExpandoObject();
                windowSettings.Width = 900;
                windowSettings.Height = 632;
                return windowSettings;
            }
        }

        private void PresetToIgnoreSelected(LayoutStructure layout)
        {
            PresetsToIgnore.Remove(layout.Name);
            OnIsDirtyChanged();
        }

        private void PresetToIgnoreUnselected(LayoutStructure layout)
        {
            PresetsToIgnore.Add(layout.Name);
            OnIsDirtyChanged();
        }

        #endregion Properties

        #region Methods

        public void ChooseAccentColor(Swatch swatch)
        {
            AccentColor = swatch?.Name;
            RefreshAccentPreview(swatch);

            if (_isConfigurationActive)
            {
                new PaletteHelper().ReplaceAccentColor(swatch.Name);
            }
        }

        public void ChoosePrimaryColor(Swatch swatch)
        {
            PrimaryColor = swatch?.Name;
            RefreshPrimaryPreview(swatch);

            if (_isConfigurationActive)
            {
                new PaletteHelper().ReplacePrimaryColor(swatch.Name);
            }
        }

        public void Init(Configuration configuration, bool isConfigurationActive)
        {
            Configuration = Mapper.Map<Configuration>(configuration);
            RefreshPreview(Configuration.PrimaryColor, Configuration.AccentColor);

            GetMapper().Map(Configuration, this);

            NotifyOfPropertyChange(() => Presets);
            _isConfigurationActive = isConfigurationActive; //do this last so that color is not being changed on initing
            Init();
        }

        public void Navigate(int index)
        {
            TransitionerIndex = index;
        }

        public override void OnIsDirtyChanged()
        {
            base.OnIsDirtyChanged();

            NotifyOfPropertyChange(() => CanSave);
        }

        public void RestoreColors()
        {
            var primary = Swatches.FirstOrDefault(s => s.Name == Configuration.PrimaryColor);
            var accent = Swatches.FirstOrDefault(s => s.Name == Configuration.AccentColor);
            if (primary != null)
                ChoosePrimaryColor(primary);
            if (accent != null)
                ChooseAccentColor(accent);
            DarkMode = Configuration.DarkMode;
        }

        public void Save()
        {
            GetMapper().Map(this, Configuration);

            TryClose(true);
        }

        private IMapper GetMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Configuration, SettingsViewModel>()
                    .ForMember(dest => dest.UserName, opts => opts.MapFrom(src => src.Name))
                    .ForMember(dest => dest.Configuration, opts => opts.Ignore())
                    .ForMember(dest => dest.SnackbarMessageQueue, opts => opts.Ignore())
                    .ReverseMap();
            });
            return config.CreateMapper();
        }

        private void RefreshAccentPreview(Swatch swatch)
        {
            if (swatch != null)
            {
                AccentMidColor = swatch.AccentExemplarHue.Color;
                AccentMidForeground = swatch.AccentExemplarHue.Foreground;
            }
        }

        private void RefreshPreview(string primary, string accent)
        {
            if (!string.IsNullOrWhiteSpace(primary))
            {
                var primarySwatch = Swatches.FirstOrDefault(s => s.Name == primary);
                RefreshPrimaryPreview(primarySwatch);
            }
            if (!string.IsNullOrWhiteSpace(accent))
            {
                var accentSwatch = Swatches.FirstOrDefault(s => s.Name == accent);
                RefreshAccentPreview(accentSwatch);
            }
        }

        private void RefreshPrimaryPreview(Swatch swatch)
        {
            if (swatch != null)
            {
                PrimaryMidColor = swatch.ExemplarHue.Color;
                PrimaryMidForeground = swatch.ExemplarHue.Foreground;
                var light = swatch.PrimaryHues.Single(h => h.Name == "Primary200");
                PrimaryLightColor = light.Color;
                PrimaryLightForeground = light.Foreground;
                var dark = swatch.PrimaryHues.Single(h => h.Name == "Primary700");
                PrimaryDarkColor = dark.Color;
                PrimaryDarkForeground = dark.Foreground;
            }
        }

        #endregion Methods
    }
}