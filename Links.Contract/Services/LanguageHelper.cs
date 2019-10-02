using System.Collections.Generic;
using System.Linq;

namespace Links.Contract.Services
{
    public static class LanguageHelper
    {
        #region Properties

        public static List<string> Languages
        {
            get
            {
                return new string[]
                {
                    "Deutsch",
                    "English"
                }.ToList();
            }
        }

        public static string DefaultLanguage => "Deutsch";

        #endregion Properties

        #region Methods

        public static void ChangeLanguage(string language)
        {
            string ietf = GetIetfFromLanguage(language);

            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.GetCultureInfoByIetfLanguageTag(ietf);
            System.Globalization.CultureInfo.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfoByIetfLanguageTag(ietf);
        }

        private static string GetIetfFromLanguage(string language)
        {
            switch (language)
            {
                case ("Deutsch"):
                    return "de";

                case ("English"):
                    return "en";

                default:
                    return "en";
            }
        }

        #endregion Methods
    }
}