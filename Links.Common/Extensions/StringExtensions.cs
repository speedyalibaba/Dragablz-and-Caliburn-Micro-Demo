using System.ComponentModel;

namespace Links.Common.Extensions
{
    public static class StringExtensions
    {
        #region Methods

        public static T ParseTo<T>(this string mystring)
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)(converter.ConvertFromInvariantString(mystring));
        }

        #endregion Methods
    }
}