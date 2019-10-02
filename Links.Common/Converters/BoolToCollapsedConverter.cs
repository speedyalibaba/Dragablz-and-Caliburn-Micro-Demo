using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Links.Common.Converters
{
	[ValueConversion(typeof(bool), typeof(Visibility), ParameterType = typeof(bool))]
	public class BoolToCollapsedConverter : IValueConverter
	{
		#region Methods

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool invert = false;
			if (parameter is bool)
			{
				invert = (bool)parameter;
			}
			else
			{
				bool.TryParse((parameter ?? "").ToString(), out invert);
			}
			var boolValue = value != null && (bool)value;
			if (invert)
			{
				return boolValue ? Visibility.Collapsed : Visibility.Visible;
			}
			return boolValue ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion Methods
	}
}