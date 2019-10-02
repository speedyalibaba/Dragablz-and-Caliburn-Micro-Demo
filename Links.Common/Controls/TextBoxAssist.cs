using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Links.Common.Controls
{
	public static class TextBoxAssist
	{
		#region Members

		public static readonly DependencyProperty SelectAllProperty = DependencyProperty.RegisterAttached(
			"SelectAll", typeof(bool), typeof(TextBoxAssist), new PropertyMetadata(false, SelectAllPropertyChanged));

		public static readonly DependencyProperty DescriptionProperty = DependencyProperty.RegisterAttached(
			"Description", typeof(string), typeof(TextBoxAssist), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty StringFormatOnLostFocusProperty = DependencyProperty.RegisterAttached(
			"StringFormatOnLostFocus", typeof(string), typeof(TextBoxAssist), new FrameworkPropertyMetadata(null, StringFormatOnLostFocusChanged));


		#endregion

		#region Methods

		[AttachedPropertyBrowsableForChildrenAttribute(IncludeDescendants = false)]
		[AttachedPropertyBrowsableForType(typeof(TextBox))]
		public static bool GetSelectAll(DependencyObject @object)
		{
			return (bool)@object.GetValue(SelectAllProperty);
		}

		public static string GetDescription(DependencyObject element)
		{
			return (string)element.GetValue(DescriptionProperty);
		}

		public static string GetStringFormatOnLostFocus(DependencyObject element)
		{
			return (string)element.GetValue(StringFormatOnLostFocusProperty);
		}

		public static void SetSelectAll(DependencyObject @object, bool value)
		{
			@object.SetValue(SelectAllProperty, value);
		}

		public static void SetDescription(DependencyObject element, string value)
		{
			element.SetValue(DescriptionProperty, value);
		}

		public static void SetStringFormatOnLostFocus(DependencyObject element, string value)
		{
			element.SetValue(StringFormatOnLostFocusProperty, value);
		}

		private static void StringFormatOnLostFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var textBox = d as TextBox;
			if (textBox == null)
				return;

			var stringFormat = GetStringFormatOnLostFocus(textBox);
			if (string.IsNullOrEmpty(stringFormat))
			{
				textBox.LostFocus -= TextBox_LostFocus;
			}
			else
			{
				textBox.LostFocus += TextBox_LostFocus;
			}
		}

		private static void TextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			var textBox = sender as TextBox;
			if (textBox == null)
				return;

			var stringFormat = GetStringFormatOnLostFocus(textBox);
			if (!stringFormat.Contains("{0"))
			{
				stringFormat = $"{{0:{stringFormat}}}";
			}

			var value = BindingExpressionHelper.GetValue(textBox.DataContext, textBox.GetBindingExpression(TextBox.TextProperty).ParentBinding);

			textBox.Text = string.Format(stringFormat, value);
		}

		private static void SelectAllPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is TextBox)
			{
				TextBox textBox = d as TextBox;
				if ((e.NewValue as bool?).GetValueOrDefault(false))
				{
					textBox.GotKeyboardFocus += OnKeyboardFocusSelectText;
					textBox.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
				}
				else
				{
					textBox.GotKeyboardFocus -= OnKeyboardFocusSelectText;
					textBox.PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
				}
			}
		}

		private static DependencyObject GetParentFromVisualTree(object source)
		{
			DependencyObject parent = source as UIElement;
			while (parent != null && !(parent is TextBox))
			{
				parent = VisualTreeHelper.GetParent(parent);
			}

			return parent;
		}

		private static void OnKeyboardFocusSelectText(object sender, KeyboardFocusChangedEventArgs e)
		{
			TextBox textBox = e.OriginalSource as TextBox;
			if (textBox != null)
			{
				textBox.SelectAll();
			}
		}

		private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			DependencyObject dependencyObject = GetParentFromVisualTree(e.OriginalSource);

			if (dependencyObject == null)
			{
				return;
			}

			var textBox = (TextBox)dependencyObject;
			if (!textBox.IsKeyboardFocusWithin)
			{
				textBox.Focus();
				e.Handled = true;
			}
		}

		#endregion

		/// <summary>
		/// Gets the value of the AutoTooltipProperty dependency property
		/// </summary>
		public static bool GetAutoTooltip(DependencyObject obj)
		{
			return (bool)obj.GetValue(AutoTooltipProperty);
		}

		/// <summary>
		/// Sets the value of the AutoTooltipProperty dependency property
		/// </summary>
		public static void SetAutoTooltip(DependencyObject obj, bool value)
		{
			obj.SetValue(AutoTooltipProperty, value);
		}

		/// <summary>
		/// Identified the attached AutoTooltip property. When true, this will display a tooltip with the full text whenever the text is trimmed.
		/// </summary>
		public static readonly DependencyProperty AutoTooltipProperty = DependencyProperty.RegisterAttached("AutoTooltip",
				typeof(bool), typeof(TextBoxAssist), new PropertyMetadata(false, OnAutoTooltipPropertyChanged));

		private static void OnAutoTooltipPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var textBox = d as TextBox;
			if (textBox == null)
				return;

			if (e.NewValue.Equals(true))
			{
				ComputeAutoTooltip(textBox);
				textBox.SizeChanged += TextBox_SizeChanged;
			}
			else
			{
				textBox.SizeChanged -= TextBox_SizeChanged;
			}
		}

		private static void TextBox_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			var textBox = sender as TextBox;
			ComputeAutoTooltip(textBox);
		}


		private static void ComputeAutoTooltip(TextBox textBox)
		{
			if (CalculateIsTextBoxTrimmed(textBox))
			{
				var text = textBox.Text;
				ToolTipService.SetToolTip(textBox, text);
			}
			else
			{
				ToolTipService.SetToolTip(textBox, null);
			}
		}

		private static bool CalculateIsTextBoxTrimmed(TextBox textBox)
		{
			if (!textBox.IsArrangeValid)
			{
				return false;
			}

			var typeface = new Typeface(
				textBox.FontFamily,
				textBox.FontStyle,
				textBox.FontWeight,
				textBox.FontStretch);

			// FormattedText is used to measure the whole width of the text held up by TextBlock container
			FormattedText formattedText = new FormattedText(
				textBox.Text,
				System.Threading.Thread.CurrentThread.CurrentCulture,
				textBox.FlowDirection,
				typeface,
				textBox.FontSize,
				textBox.Foreground,
                VisualTreeHelper.GetDpi(textBox).PixelsPerDip);

            formattedText.MaxTextWidth = textBox.ActualWidth;

			// When the maximum text width of the FormattedText instance is set to the actual
			// width of the textBlock, if the textBlock is being trimmed to fit then the formatted
			// text will report a larger height than the textBlock. Should work whether the
			// textBlock is single or multi-line.
			// The width check detects if any single line is too long to fit within the text area, 
			// this can only happen if there is a long span of text with no spaces.
			return (formattedText.Height > textBox.ActualHeight || formattedText.MinWidth > formattedText.MaxTextWidth);
		}

	}
}
