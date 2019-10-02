using Dragablz;
using System.Windows;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Links
{
    public class CaliburnInterTabClient : IInterTabClient
    {
        public INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
        {
            //         if (source == null) throw new ArgumentNullException("source");
            //         var sourceWindow = Window.GetWindow(source);
            //         if (sourceWindow == null) throw new ApplicationException("Unable to ascertain source window.");
            //         var view = (Window)Activator.CreateInstance(sourceWindow.GetType());
            //view.Title = sourceWindow.Title;
            //view.Topmost = sourceWindow.Topmost;

            //         var type = source.DataContext.GetType();

            //         var activeItem = ((Conductor<IScreen>.Collection.OneActive)source.DataContext).ActiveItem;

            //         var vm = typeof(IoC).GetMethod("Get").MakeGenericMethod(type).Invoke(this, new Object[] { null });

            //         if (vm == null) throw new ApplicationException("No ViewModel received from Ioc.");

            //         var newTabablzControl = view.LogicalTreeDepthFirstTraversal().OfType<TabablzControl>().FirstOrDefault();
            //         if (newTabablzControl == null) throw new ApplicationException("Unable to ascertain tab control.");

            //         if (newTabablzControl.ItemsSource == null)
            //             newTabablzControl.Items.Clear();

            //         ViewModelBinder.Bind(vm, view, null);

            //         ((Conductor<IScreen>.Collection.OneActive)vm).ActivateItem(activeItem);

            //         return new NewTabHost<Window>(view, newTabablzControl);

            var shell = new ShellView();
            shell.Topmost = Window.GetWindow(source).Topmost;
            var vm = IoC.Get<ShellViewModel>();

            ViewModelBinder.Bind(vm, shell, null);
            return new NewTabHost<Window>(shell, shell.Tabs);
        }

        public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window)
        {
            return TabEmptiedResponse.CloseWindowOrLayoutBranch;
        }
    }

    public static class CaliburnDragablzExtensions
    {
        public static IEnumerable<object> LogicalTreeDepthFirstTraversal(this DependencyObject node)
        {
            if (node == null) yield break;
            yield return node;

            foreach (var child in LogicalTreeHelper.GetChildren(node).OfType<DependencyObject>()
                .SelectMany(depObj => depObj.LogicalTreeDepthFirstTraversal()))
                yield return child;
        }
    }
}
