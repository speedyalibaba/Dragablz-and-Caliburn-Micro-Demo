using System;
using System.Globalization;
using System.Windows.Data;

namespace Links.Common.Converters
{
	public class FlagsConverter : IValueConverter
	{
		#region Methods

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			int mask = (int)parameter;
			var targetValue = (int)value;
			return ((mask & targetValue) != 0);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion Methods
	}
}