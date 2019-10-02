using Links.Contract.Models;
using Links.Services;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Links.Coverters
{
    public class LayoutToPreviewConverter : IMultiValueConverter
    {
        #region Methods

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.FirstOrDefault() is LayoutStructure)
            {
                var item = values.FirstOrDefault() as LayoutStructure;
                double maxWidth = 180;
                double maxHeight = 80;
                if (values.Skip(1).FirstOrDefault() is double)
                {
                    maxWidth = (double)values.Skip(1).FirstOrDefault();
                }
                if (values.Skip(2).FirstOrDefault() is Thickness)
                {
                    var parentPadding = (Thickness)values.Skip(2).FirstOrDefault();
                    maxWidth -= (parentPadding.Left + parentPadding.Right);
                }
                if (!string.IsNullOrWhiteSpace(parameter.ToString()) && double.TryParse(parameter.ToString(), out var additionalMargin))
                {
                    maxWidth -= 2 * additionalMargin;
                }
                maxWidth = maxWidth < 0 ? 0 : maxWidth;

                return LayoutPreviewBuilder.BuildPreview(item, maxHeight, maxWidth);
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}