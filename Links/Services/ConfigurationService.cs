using AutoMapper;
using Links.Contract;
using Caliburn.Micro;
using System.Collections.Generic;
using System.Linq;

namespace Links
{
    public class ConfigurationService : IConfigurationService
    {
        #region Fields

        private readonly IEventAggregator _eventAggregator;
        private readonly IMapper _mapper;

        #endregion Fields

        #region Constructors

        public ConfigurationService(
            IEventAggregator eventAggregator,
            IMapper mapper)
        {
            _eventAggregator = eventAggregator;
            _mapper = mapper;

            Configurations = new List<Configuration>();
            if (Properties.Settings.Default.Configurations != null)
                Configurations.AddRange(Properties.Settings.Default.Configurations);

            if (!Switch(Properties.Settings.Default.LastConfigName))
                Switch();
        }

        #endregion Constructors

        #region Properties

        public Configuration ActiveConfiguration { get; private set; }
        public List<Configuration> Configurations { get; }
        public Configuration PreviousConfiguration { get; private set; }

        #endregion Properties

        #region Methods

        public bool AddConfiguration(Configuration configuration, int index = 0)
        {
            if (Configurations.All(c => c.Name != configuration.Name))
            {
                Configurations.Insert(index, configuration);
                SaveAndNotifyChanges();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Deletes a Configuration
        /// </summary>
        /// <param name="name">configuration name</param>
        /// <returns>success</returns>
        public bool RemoveConfiguration(string name)
        {
            var config = Configurations.FirstOrDefault(c => c.Name == name);
            if (config != null)
            {
                Configurations.Remove(config);
                SaveAndNotifyChanges();
                return true;
            }
            return false;
        }

        public void ResetAllSettings()
        {
            Properties.Settings.Default.Reset();
            SaveAndNotifyChanges();
        }

        public void Save()
        {
            Properties.Settings.Default.Configurations = Configurations;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Switches to a saved configuration
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Switch(string name)
        {
            var config = Configurations.FirstOrDefault(c => c.Name == name);
            if (config != null)
            {
                Switch(config);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Switches to a temporary configuration
        /// </summary>
        /// <returns>success</returns>
        public void Switch()
        {
            Switch(new Configuration());
        }

        public bool UpdateConfiguration(string name, Configuration updatedConfiguration)
        {
            var config = Configurations.FirstOrDefault(c => c.Name == name);
            if (config != null)
            {
                _mapper.Map(updatedConfiguration, config);
                SaveAndNotifyChanges();
                return true;
            }
            return false;
        }

        private void SaveAndNotifyChanges()
        {
            Save();

            _eventAggregator.PublishOnUIThread(new Contract.Messages.ConfigurationChanged());
        }

        private void Switch(Configuration configuration)
        {
            PreviousConfiguration = ActiveConfiguration;
            ActiveConfiguration = configuration;
            Properties.Settings.Default.LastConfigName = ActiveConfiguration.Name;
            Save();

            _eventAggregator.PublishOnUIThread(new Contract.Messages.ConfigurationSwitched { SwitchOnDeletion = !Configurations.Contains(PreviousConfiguration) });
        }

        #endregion Methods
    }
}