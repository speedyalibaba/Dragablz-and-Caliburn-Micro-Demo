using Links.Contract;
using Links.Contract.Messages;
using Links.Contract.Models;
using Links.Contract.Services;
using Links.WPF;
using Caliburn.Micro;
using Dragablz;
using Newtonsoft.Json;
using System.Linq;

namespace Links.Services
{
    public class LayoutManager : IHandle<ConfigurationSwitched>, ILayoutManager
    {
        #region Fields

        private readonly IConfigurationService _configurationService;
        private readonly IEventAggregator _eventAggregator;
        private bool _isSubscribed;
        public static string LAST_LAYOUT_NAME = "LastLayout";   //Translation uses this name too

        #endregion Fields

        #region Constructors

        public LayoutManager(
            IConfigurationService configurationService,
            IEventAggregator eventAggregator)
        {
            _configurationService = configurationService;
            _eventAggregator = eventAggregator;
        }

        #endregion Constructors

        #region Methods

        public static LayoutStructure DeserializeStructure(string serializedStructure)
        {
            if (string.IsNullOrWhiteSpace(serializedStructure))
                return null;

            return JsonConvert.DeserializeObject<LayoutStructure>(serializedStructure);
        }

        public static string SerializeStructure(LayoutStructure layoutStructure)
        {
            if (layoutStructure == null)
                return null;

            return JsonConvert.SerializeObject(layoutStructure, Formatting.None);
        }

        public string GetCurrentLayout(string name)
        {
            LayoutStructure layoutStructure = LayoutAnalayzer.GetLayoutStructure(name);
            return SerializeStructure(layoutStructure);
        }

        public void Handle(ConfigurationSwitched message)
        {
            if (!message.SwitchOnDeletion)
            {
                LayoutStructure layoutStructure = LayoutAnalayzer.GetLayoutStructure(LAST_LAYOUT_NAME);

                _configurationService.PreviousConfiguration.LastLayout = SerializeStructure(layoutStructure);
                _configurationService.Save();
            }
            var activeConfig = _configurationService.ActiveConfiguration;
            var tabs = TabablzControl.GetLoadedInstances().FirstOrDefault();
            Load(tabs, activeConfig.PrimaryColor, activeConfig.AccentColor, activeConfig.DarkMode);
        }

        public void Load(TabablzControl tabablz, string primary = null, string accent = null, bool? darkMode = null)
        {
            var l = _configurationService.ActiveConfiguration.LastLayout;
            string language = _configurationService.ActiveConfiguration.Language;
            LayoutStructure layout;

            if (string.IsNullOrWhiteSpace(l))
            {
                //Open Default Layout
                layout = LayoutLibrary.GetDefaultLayout();
                LayoutBuilder.RestoreLayout(tabablz, layout, language, primary, accent, darkMode);
            }
            else
            {
                // Restore layout
                layout = DeserializeStructure(l);

                Load(tabablz, layout, language, primary, accent, darkMode);
            }
        }

        public void LoadAndSavePrevious(LayoutStructure layout)
        {
            LayoutStructure layoutStructure = LayoutAnalayzer.GetLayoutStructure(LAST_LAYOUT_NAME);

            _configurationService.ActiveConfiguration.LastLayout = SerializeStructure(layoutStructure);
            _configurationService.Save();

            var tabs = TabablzControl.GetLoadedInstances().FirstOrDefault();
            Load(tabs, layout, _configurationService.ActiveConfiguration.Language);
        }

        public void Reload()
        {
            var tabs = TabablzControl.GetLoadedInstances().FirstOrDefault();
            var layout = LayoutAnalayzer.GetLayoutStructure(LAST_LAYOUT_NAME);
            Load(tabs, layout, _configurationService.ActiveConfiguration.Language);
        }

        public void Save()
        {
            LayoutStructure layoutStructure = LayoutAnalayzer.GetLayoutStructure(LAST_LAYOUT_NAME);

            Properties.Settings.Default.LastConfigName = _configurationService.ActiveConfiguration.Name;
            _configurationService.ActiveConfiguration.LastLayout = SerializeStructure(layoutStructure);
            _configurationService.Save();
        }

        public void SubscribeIfNotSubscribed()
        {
            if (_isSubscribed)
                return;
            _isSubscribed = true;

            _eventAggregator.Subscribe(this);
        }

        private void Load(TabablzControl tabablz, LayoutStructure layout, string language, string primary = null, string accent = null, bool? darkMode = null)
        {
            if (layout.Windows.SelectMany(w => w.TabSets).Sum(ts => ts.TabItems.Count()) == 0)  //saving or restoring failed
            {
                layout = LayoutLibrary.GetDefaultLayout();
            }

            LayoutBuilder.RestoreLayout(tabablz, layout, language, primary, accent, darkMode);
        }

        #endregion Methods
    }
}