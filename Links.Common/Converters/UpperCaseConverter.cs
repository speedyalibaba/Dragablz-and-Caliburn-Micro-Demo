using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Links.Common.Converters
{
	[ValueConversion(typeof(string), typeof(string))]
	public class UpperCaseConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var strValue = value as string;
			if (strValue != null)
			{
				return strValue.ToUpperInvariant().Replace('ß', 'ẞ');
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
