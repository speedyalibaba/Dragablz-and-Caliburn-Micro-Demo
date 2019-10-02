using System;
using System.Globalization;
using System.Windows.Data;

namespace Links.Common.Converters
{
	public class DoubleConverter : IValueConverter
	{
		#region Methods

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var returnValue = value;
			if (value != null && value is double && double.TryParse(parameter.ToString(), out double offset))
			{
				return (double)returnValue + offset;
			}

			return returnValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion Methods
	}
}