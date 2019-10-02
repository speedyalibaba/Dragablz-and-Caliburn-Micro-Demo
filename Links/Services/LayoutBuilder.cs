using Links.Contract;
using Links.Contract.Models;
using Links.Contract.Services;
using Links.WPF;
using Caliburn.Micro;
using Dragablz;
using Dragablz.Dockablz;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace Links.Services
{
    public static class LayoutBuilder
    {
        public static bool RestoreLayout(TabablzControl rootTabControl, LayoutStructure layoutStructure,
            string language, string primary = null, string accent = null, bool? darkMode = null)
        {
            try
            {
                if (!ApplicationService.IsStarting)
                {
                    LanguageHelper.ChangeLanguage(language);
                    DepopulateTabControl(ref rootTabControl);
                }

                SetColors(primary, accent, darkMode);

                VerifyWindowPositions(layoutStructure);

                foreach (var layoutStructureWindow in layoutStructure.Windows)
                {
                    TabablzControl tabControl;
                    Window currentWindow = null;
                    if (layoutStructure.Windows.First() == layoutStructureWindow)
                    {
                        tabControl = rootTabControl;
                        var window = Window.GetWindow(rootTabControl);
                        window.Left = layoutStructureWindow.Left;
                        window.Top = layoutStructureWindow.Top;
                        window.Width = layoutStructureWindow.Width;
                        window.Height = layoutStructureWindow.Height;
                        window.Topmost = layoutStructureWindow.Topmost;
                        window.WindowState = layoutStructureWindow.WindowState;
                    }
                    else
                    {
                        var newTabHost = IoC.Get<IInterTabClient>().GetNewHost(rootTabControl.InterTabController.InterTabClient, null, rootTabControl);
                        tabControl = newTabHost.TabablzControl;
                        currentWindow = newTabHost.Container;
                        currentWindow.Left = layoutStructureWindow.Left;
                        currentWindow.Top = layoutStructureWindow.Top;
                        currentWindow.Width = layoutStructureWindow.Width;
                        currentWindow.Height = layoutStructureWindow.Height;
                        currentWindow.Topmost = layoutStructureWindow.Topmost;
                        currentWindow.WindowState = layoutStructureWindow.WindowState;
                        currentWindow.Show();
                    }

                    var layoutStructureTabSets = layoutStructureWindow.TabSets.ToDictionary(tabSet => tabSet.Id);

                    if (layoutStructureWindow.Branches.Any())
                    {
                        var branchIndex = layoutStructureWindow.Branches.ToDictionary(b => b.Id);
                        var rootBranch = GetRoot(branchIndex);

                        //do the nasty recursion to build the layout, populate the tabs after, keep it simple...
                        foreach (var tuple in BuildLayout(tabControl, rootBranch, branchIndex))
                        {
                            PopulateTabControl(tuple.Item2, layoutStructureTabSets[tuple.Item1]);
                        }
                    }
                    else
                    {
                        var layoutStructureTabSet = layoutStructureTabSets.Values.FirstOrDefault();
                        if (layoutStructureTabSet != null)
                            PopulateTabControl(tabControl, layoutStructureTabSet);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void SetColors(string primaryColor, string accentColor, bool? darkMode)
        {
            var swatches = new SwatchesProvider().Swatches;
            var paletteHelper = new PaletteHelper();

            if (primaryColor != null)
            {
                var primary = swatches.FirstOrDefault(s => s.Name == primaryColor);
                if (primary != null)
                {
                    paletteHelper.ReplacePrimaryColor(primary);
                }
            }

            if (accentColor != null)
            {
                var accent = swatches.FirstOrDefault(s => s.Name == accentColor);
                if (accent != null)
                {
                    paletteHelper.ReplaceAccentColor(accent);
                }
            }

            if (darkMode.HasValue)
            {
                paletteHelper.SetLightDark(darkMode.Value);
            }
        }

        private static void DepopulateTabControl(ref TabablzControl tabablzControl)
        {
            try
            {
                ApplicationService.IgnoreShuttingDown = true;
                var rootWindow = Window.GetWindow(tabablzControl);
                var windowsToClose = Application.Current.Windows.OfType<ShellView>().Where(w => w != rootWindow);

                foreach (var window in windowsToClose)
                {
                    window.Close();
                }

                var newTabHost = IoC.Get<IInterTabClient>().GetNewHost(tabablzControl.InterTabController.InterTabClient, null, tabablzControl);
                var tabControl = newTabHost.TabablzControl;
                var newWindow = newTabHost.Container;
                newWindow.Left = rootWindow.Left;
                newWindow.Top = rootWindow.Top;
                newWindow.Width = rootWindow.Width;
                newWindow.Height = rootWindow.Height;
                newWindow.Topmost = rootWindow.Topmost;

                tabablzControl = (newWindow as ShellView).Tabs;
                newWindow.Show();
                rootWindow.Close();

                var temp = TabablzControl.GetLoadedInstances();
            }
            finally
            {
                ApplicationService.IgnoreShuttingDown = false;
            }
        }

        private static void PopulateTabControl(TabablzControl tabablzControl, LayoutStructureTabSet layoutStructureTabSet)
        {
            bool wasAnyTabSelected = false;

            foreach (var tabItem in layoutStructureTabSet.TabItems)
            {
                var view = ViewLocator.LocateForModelType(tabItem.ViewModelType, null, null);
                var vm = typeof(IoC).GetMethod("Get").MakeGenericMethod(tabItem.ViewModelType).Invoke(tabablzControl, new Object[] { null });

                ViewModelBinder.Bind(vm, view, null);
                (tabablzControl.DataContext as ShellViewModel).ActivateItem(vm as IScreen);

                tabablzControl.AddToSource(view);

                if (tabItem.Id == layoutStructureTabSet.SelectedTabItemId)
                {
                    tabablzControl.Dispatcher.BeginInvoke(new System.Action(() =>
                    {
                        tabablzControl.SetCurrentValue(Selector.SelectedItemProperty, view);
                    }), DispatcherPriority.Loaded);
                    wasAnyTabSelected = true;
                }
            }

            if (!wasAnyTabSelected && tabablzControl.Items.Count != 0)
            {
                tabablzControl.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    tabablzControl.SetCurrentValue(Selector.SelectedItemProperty, tabablzControl.Items.OfType<System.Windows.Controls.UserControl>().First());
                }), DispatcherPriority.Loaded);
                wasAnyTabSelected = true;
            }
        }

        private static IEnumerable<Tuple<Guid, TabablzControl>> BuildLayout(
            TabablzControl intoTabablzControl,
            LayoutStructureBranch layoutStructureBranch,
            IDictionary<Guid, LayoutStructureBranch> layoutStructureBranchIndex)
        {
            var newSiblingTabablzControl = new TabablzControl();
            var branchResult = Layout.Branch(intoTabablzControl, newSiblingTabablzControl, layoutStructureBranch.Orientation, false, layoutStructureBranch.Ratio);

            var newInterTabController = new InterTabController
            {
                Partition = intoTabablzControl.InterTabController.Partition,
                InterTabClient = intoTabablzControl.InterTabController.InterTabClient
            };
            CaliburnInterLayoutClient.Clone(intoTabablzControl.InterTabController, newInterTabController);
            newSiblingTabablzControl.SetCurrentValue(TabablzControl.InterTabControllerProperty, newInterTabController);

            if (layoutStructureBranch.ChildFirstBranchId.HasValue)
            {
                var firstChildBranch = layoutStructureBranchIndex[layoutStructureBranch.ChildFirstBranchId.Value];
                foreach (var tuple in BuildLayout(intoTabablzControl, firstChildBranch, layoutStructureBranchIndex))
                    yield return tuple;
            }
            else if (layoutStructureBranch.ChildFirstTabSetId.HasValue)
            {
                SetTabSetId(intoTabablzControl, layoutStructureBranch.ChildFirstTabSetId.Value);
                yield return new Tuple<Guid, TabablzControl>(layoutStructureBranch.ChildFirstTabSetId.Value, intoTabablzControl);
            }

            if (layoutStructureBranch.ChildSecondBranchId.HasValue)
            {
                var secondChildBranch = layoutStructureBranchIndex[layoutStructureBranch.ChildSecondBranchId.Value];
                foreach (var tuple in BuildLayout(branchResult.TabablzControl, secondChildBranch, layoutStructureBranchIndex))
                    yield return tuple;
            }
            else if (layoutStructureBranch.ChildSecondTabSetId.HasValue)
            {
                SetTabSetId(newSiblingTabablzControl, layoutStructureBranch.ChildSecondTabSetId.Value);
                yield return new Tuple<Guid, TabablzControl>(layoutStructureBranch.ChildSecondTabSetId.Value, newSiblingTabablzControl);
            }
        }

        private static void SetTabSetId(DependencyObject element, Guid? value)
        {
            element.SetValue(TabSetIdProperty, value);
        }

        private static LayoutStructureBranch GetRoot(Dictionary<Guid, LayoutStructureBranch> branches)
        {
            var lookup = branches.Values.SelectMany(ChildBranchIds).Distinct().ToLookup(guid => guid);
            return branches.Values.Single(branch => !lookup.Contains(branch.Id));
        }

        private static IEnumerable<Guid> ChildBranchIds(LayoutStructureBranch branch)
        {
            if (branch.ChildFirstBranchId.HasValue)
                yield return branch.ChildFirstBranchId.Value;
            if (branch.ChildSecondBranchId.HasValue)
                yield return branch.ChildSecondBranchId.Value;
        }

        public static readonly DependencyProperty TabSetIdProperty = DependencyProperty.RegisterAttached(
            "TabSetId", typeof(Guid?), typeof(LayoutBuilder), new PropertyMetadata(default(Guid?)));

        private static void VerifyWindowPositions(LayoutStructure layoutStructure)
        {
            foreach (var window in layoutStructure.Windows)
            {
                var location = new System.Drawing.Point((int)window.Left, (int)window.Top);
                bool isInsideBounds = false;

                foreach (var s in System.Windows.Forms.Screen.AllScreens)
                {
                    isInsideBounds = s.Bounds.Contains(location);
                    if (isInsideBounds) { break; }
                }

                if (!isInsideBounds)
                {
                    ResetWindowPosition(window);
                }

                if (window.Height == 0 || window.Width == 0)
                {
                    window.Height = 500;
                    window.Width = 800;
                    ResetWindowPosition(window);
                }
            }
        }

        private static void ResetWindowPosition(LayoutStructureWindow window)
        {
            var primaryScreen = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            window.Left = primaryScreen.Left + primaryScreen.Width / 10;
            window.Top = primaryScreen.Top + primaryScreen.Height / 10;
        }
    }
}
