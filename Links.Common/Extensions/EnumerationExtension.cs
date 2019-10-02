using Caliburn.Micro;
using System;
using System.Linq;
using System.Windows.Markup;

namespace Links.Common.Extensions
{
        public class EnumerationExtension : MarkupExtension
        {
            #region Members

            private Type _enumType;

            #endregion Members

            #region Instantiation

            public EnumerationExtension(Type enumType)
            {
                if (enumType == null)
                    throw new ArgumentNullException("enumType");

                EnumType = enumType;
            }

            #endregion Instantiation

            #region Properties

            public Type EnumType
            {
                get { return _enumType; }
                private set
                {
                    if (_enumType == value)
                        return;

                    var enumType = Nullable.GetUnderlyingType(value) ?? value;

                    if (!enumType.IsEnum)
                        throw new ArgumentException("Type must be an Enum.");

                    _enumType = value;
                }
            }

            #endregion Properties

            #region Methods

            public override object ProvideValue(IServiceProvider serviceProvider)
            {
                var result = (
                  from object enumValue in Enum.GetValues(EnumType)
                  select new EnumerationMember
                  {
                      Value = enumValue,
                      DisplayName = ((Enum)enumValue).GetDisplayName()
                  }).ToArray();

                return result;
            }

            #endregion Methods

            #region Classes

            public class EnumerationMember : PropertyChangedBase
            {
                #region Members

                private string _displayName;
                private object _value;

                #endregion Members

                #region Properties

                public string DisplayName
                {
                    get { return _displayName; }
                    set { this.Set(ref _displayName, value); }
                }

                public object Value
                {
                    get { return _value; }
                    set { this.Set(ref _value, value); }
                }

                #endregion Properties
            }

            #endregion Classes
        }
    
}
