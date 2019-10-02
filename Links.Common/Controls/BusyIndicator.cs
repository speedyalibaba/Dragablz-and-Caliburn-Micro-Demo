using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Links.Common.Controls
{
	public class BusyIndicator : ContentControl
	{
		#region Fields

		// Using a DependencyProperty as the backing store for BusyContent.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty BusyContentProperty =
			DependencyProperty.Register("BusyContent", typeof(object), typeof(BusyIndicator), new PropertyMetadata(null));

		// Using a DependencyProperty as the backing store for BusyContentTemplate.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty BusyContentTemplateProperty =
			DependencyProperty.Register("BusyContentTemplate", typeof(DataTemplate), typeof(BusyIndicator), new PropertyMetadata(default(DataTemplate)));

		// Using a DependencyProperty as the backing store for IsBusy.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsBusyProperty =
			DependencyProperty.Register("IsBusy", typeof(bool), typeof(BusyIndicator), new PropertyMetadata(false));

		// Using a DependencyProperty as the backing store for OverlayBrush.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty OverlayBrushProperty =
			DependencyProperty.Register("OverlayBrush", typeof(Brush), typeof(BusyIndicator), new PropertyMetadata(Brushes.White));

		#endregion Fields

		#region Constructors

		static BusyIndicator()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(BusyIndicator), new FrameworkPropertyMetadata(typeof(BusyIndicator)));
		}

		#endregion Constructors

		#region Properties

		public object BusyContent
		{
			get { return (object)GetValue(BusyContentProperty); }
			set { SetValue(BusyContentProperty, value); }
		}

		public DataTemplate BusyContentTemplate
		{
			get { return (DataTemplate)GetValue(BusyContentTemplateProperty); }
			set { SetValue(BusyContentTemplateProperty, value); }
		}

		public bool IsBusy
		{
			get { return (bool)GetValue(IsBusyProperty); }
			set { SetValue(IsBusyProperty, value); }
		}

		public Brush OverlayBrush
		{
			get { return (Brush)GetValue(OverlayBrushProperty); }
			set { SetValue(OverlayBrushProperty, value); }
		}

		#endregion Properties
	}
}