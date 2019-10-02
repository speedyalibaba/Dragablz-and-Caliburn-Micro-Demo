using Links.Contract.Services;
using System;
using System.Collections.Generic;

namespace Links.Contract
{
    public class Configuration
    {
        #region Properties

        public string AccentColor { get; set; }
        public bool AllowEditingLayouts { get; set; } = true;
        public bool DarkMode { get; set; }
        public string Language { get; set; } = LanguageHelper.DefaultLanguage;
        public string LastLayout { get; set; }
        public List<string> Layouts { get; set; } = new List<string>();
        public string Name { get; set; }
        public List<string> PresetsToIgnore { get; set; } = new List<string>();
        public string PrimaryColor { get; set; }
        public bool ShowLastLayout { get; set; } = true;

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            return Name;
        }

        #endregion Methods
    }
}