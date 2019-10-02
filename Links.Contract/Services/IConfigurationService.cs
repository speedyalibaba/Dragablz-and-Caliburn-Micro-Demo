using System.Collections.Generic;

namespace Links.Contract
{
    public interface IConfigurationService
    {
        #region Properties

        Configuration ActiveConfiguration { get; }
        List<Configuration> Configurations { get; }
        Configuration PreviousConfiguration { get; }

        #endregion Properties

        #region Methods

        bool AddConfiguration(Configuration configuration, int index = 0);

        bool RemoveConfiguration(string name);

        void ResetAllSettings();

        void Save();

        void Switch();

        bool Switch(string name);

        bool UpdateConfiguration(string name, Configuration updatedConfiguration);

        #endregion Methods
    }
}