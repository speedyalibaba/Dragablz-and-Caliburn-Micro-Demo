using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Links.Common.Extensions
{
    public class ItemEnumerationExtension : EnumerationExtension
    {
        public ItemEnumerationExtension(Type enumType) : base(enumType)
        {
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var members = (EnumerationMember[])base.ProvideValue(serviceProvider);
            var enumValues = new List<EnumerationMember>();
            foreach (var member in members)
            {
                enumValues.Add(new EnumerationMember { Value = member.Value, DisplayName = member.DisplayName });
            }
            return enumValues;
        }
    }
}
