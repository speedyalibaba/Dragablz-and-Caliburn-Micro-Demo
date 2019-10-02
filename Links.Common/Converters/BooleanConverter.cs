using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Links.Common.Converters
{
    public abstract class BooleanConverter<T> : IValueConverter
    {
        #region Instantiation

        protected BooleanConverter(T trueValue, T falseValue)
        {
            this.TrueValue = trueValue;
            this.FalseValue = falseValue;
        }

        #endregion Instantiation

        #region Properties

        public T TrueValue { get; set; }

        public T FalseValue { get; set; }

        #endregion Properties

        #region Methods

        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return Object.Equals(value, true) ?
                this.TrueValue : this.FalseValue;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return Object.Equals(value, this.TrueValue);
        }

        #endregion Methods
    }

    public sealed class BooleanToVisibilityConverter
        : BooleanConverter<Visibility>
    {
        #region Instantiation

        public BooleanToVisibilityConverter()
            : base(Visibility.Visible, Visibility.Collapsed) { }

        #endregion Instantiation
    }

    public sealed class BooleanToItalicConverter
        : BooleanConverter<FontStyle>
    {
        #region Instantiation

        public BooleanToItalicConverter()
            : base(FontStyles.Italic, FontStyles.Normal) { }

        #endregion Instantiation
    }

    public sealed class BooleanToVisibilityHiddenConverter
        : BooleanConverter<Visibility>
    {
        #region Instantiation

        public BooleanToVisibilityHiddenConverter()
            : base(Visibility.Visible, Visibility.Hidden) { }

        #endregion Instantiation
    }

    public sealed class InvertBooleanToVisibilityConverter
        : BooleanConverter<Visibility>
    {
        #region Instantiation

        public InvertBooleanToVisibilityConverter()
            : base(Visibility.Collapsed, Visibility.Visible) { }

        #endregion Instantiation
    }

    public sealed class InvertBooleanToVisibilityHiddenConverter
    : BooleanConverter<Visibility>
    {
        #region Instantiation

        public InvertBooleanToVisibilityHiddenConverter()
            : base(Visibility.Hidden, Visibility.Visible) { }

        #endregion Instantiation
    }

    public sealed class InvertBooleanConverter
        : BooleanConverter<bool>
    {
        #region Instantiation

        public InvertBooleanConverter()
            : base(false, true) { }

        #endregion Instantiation
    }

    public sealed class BooleanToBrushConverter : BooleanConverter<Brush>
    {
        #region Instantiation

        public BooleanToBrushConverter()
            : base(Brushes.AliceBlue, Brushes.YellowGreen) { }

        #endregion Instantiation
    }

    public sealed class BooleanToDoubleConverter : BooleanConverter<double>
    {
        #region Instantiation

        public BooleanToDoubleConverter()
            : base(1, 0) { }

        #endregion Instantiation
    }

    public sealed class BooleanAndConverter : IMultiValueConverter
    {
        #region Methods

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (object value in values)
            {
                if ((value is bool) && !(bool)value)
                {
                    return false;
                }
            }
            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("BooleanAndConverter is a OneWay converter.");
        }

        #endregion Methods
    }

    public sealed class NullableBooleanToVisibilityMulitConverter : IMultiValueConverter
    {
        #region Methods

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (object value in values)
            {
                if (value != null && value is bool && !(bool)value)
                    return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}
