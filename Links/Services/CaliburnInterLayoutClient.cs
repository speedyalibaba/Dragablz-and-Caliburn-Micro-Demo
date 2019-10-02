using Dragablz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Links.Services
{
	public class CaliburnInterLayoutClient : IInterLayoutClient
	{
		public INewTabHost<UIElement> GetNewHost(object partition, TabablzControl source)
		{
			var tabablzControl = new TabablzControl { DataContext = source.DataContext };

			Clone(source, tabablzControl);

			if (source.InterTabController == null)
				throw new InvalidOperationException("Source tab does not have an InterTabController set.  Ensure this is set on initial, and subsequently generated tab controls.");

			var newInterTabController = new InterTabController
			{
				Partition = source.InterTabController.Partition,
				InterTabClient = source.InterTabController.InterTabClient
			};
			Clone(source.InterTabController, newInterTabController);
			tabablzControl.SetCurrentValue(TabablzControl.InterTabControllerProperty, newInterTabController);

			return new NewTabHost<UIElement>(tabablzControl, tabablzControl);
		}

		public static void Clone(DependencyObject from, DependencyObject to)
		{
			var localValueEnumerator = from.GetLocalValueEnumerator();
			while (localValueEnumerator.MoveNext())
			{
				if (localValueEnumerator.Current.Property.ReadOnly ||
					localValueEnumerator.Current.Value is FrameworkElement) continue;

				if (!(localValueEnumerator.Current.Value is BindingExpressionBase))
					to.SetCurrentValue(localValueEnumerator.Current.Property, localValueEnumerator.Current.Value);
			}
		}
	}
}
