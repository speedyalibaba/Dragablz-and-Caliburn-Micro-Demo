using Links.Contract.Models;
using Links.WPF;
using Dragablz;
using Dragablz.Dockablz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Links.Services
{
    public static class LayoutAnalayzer
    {
        private class BranchContext
        {
            public BranchContext(LayoutStructureBranch parent, bool isSecond)
            {
                if (parent == null) throw new ArgumentNullException(nameof(parent));

                Parent = parent;
                IsSecond = isSecond;
            }

            public LayoutStructureBranch Parent { get; }
            public bool IsSecond { get; }
        }

        public static LayoutStructure GetLayoutStructure(string name)
        {
            return new LayoutStructure(name, FlattenWindowStructure());
        }

        private static IEnumerable<LayoutStructureWindow> FlattenWindowStructure()
        {
            foreach (var layoutItem in Layout.GetLoadedInstances().Select(layout => new { layout, window = Window.GetWindow(layout) }))
            {
                var layoutStructureBranches = new List<LayoutStructureBranch>();
                var layoutStructureTabSets = new List<LayoutStructureTabSet>();

                var layoutAccessor = layoutItem.layout.Query();
                layoutAccessor.Visit(branchAccessor =>
                {
                    VisitAndDocumentChildren(branchAccessor, null, layoutStructureBranches, layoutStructureTabSets);
                },
                    tabablzControl => DocumentTabSet(Guid.NewGuid(), tabablzControl, layoutStructureTabSets)
                    );

                yield return new LayoutStructureWindow(layoutStructureBranches, layoutStructureTabSets)
                {
                    Left = layoutItem.window.Left,
                    Top = layoutItem.window.Top,
                    Width = layoutItem.window.Width,
                    Height = layoutItem.window.Height,
                    Topmost = layoutItem.window.Topmost,
                    WindowState = layoutItem.window.WindowState
                };
            }
        }

        private static void VisitAndDocumentChildren(BranchAccessor branchAccessor, BranchContext context, ICollection<LayoutStructureBranch> branches, ICollection<LayoutStructureTabSet> layoutStructureTabSets)
        {
            //figure out the current ID: if there is no context, this is 
            //root, otherwise, the parent will have assigned the id
            Guid id;
            if (context == null)
                id = Guid.NewGuid();
            else
            {
                id = context.IsSecond
                    ? context.Parent.ChildSecondBranchId.Value
                    : context.Parent.ChildFirstBranchId.Value;
            }

            //document the current branch
            var layoutStructureBranch = new LayoutStructureBranch(
                id,
                IdOrNull(branchAccessor.FirstItemBranchAccessor),
                IdOrNull(branchAccessor.SecondItemBranchAccessor),
                IdOrNull(branchAccessor.FirstItemTabablzControl),
                IdOrNull(branchAccessor.SecondItemTabablzControl),
                branchAccessor.Branch.Orientation,
                branchAccessor.Branch.GetFirstProportion());
            branches.Add(layoutStructureBranch);

            //run throw the child branches or tab controls
            if (branchAccessor.FirstItemBranchAccessor != null)
                VisitAndDocumentChildren(branchAccessor.FirstItemBranchAccessor, new BranchContext(layoutStructureBranch, false), branches, layoutStructureTabSets);
            else
                DocumentTabSet(layoutStructureBranch.ChildFirstTabSetId.Value, branchAccessor.FirstItemTabablzControl, layoutStructureTabSets);
            if (branchAccessor.SecondItemBranchAccessor != null)
                VisitAndDocumentChildren(branchAccessor.SecondItemBranchAccessor, new BranchContext(layoutStructureBranch, true), branches, layoutStructureTabSets);
            else
                DocumentTabSet(layoutStructureBranch.ChildSecondTabSetId.Value, branchAccessor.SecondItemTabablzControl, layoutStructureTabSets);
        }

        private static void DocumentTabSet(Guid id, TabablzControl tabablzControl, ICollection<LayoutStructureTabSet> layoutStructureTabSets)
        {
            var tabItems = tabablzControl.Items.OfType<UserControl>()
                .Select(c => new LayoutStructureTabItem(Guid.NewGuid(), ((c.DataContext as NewViewModel)?.ActiveItem != null)
                                                                            ? (c.DataContext as NewViewModel).ActiveItem
                                                                            : c.DataContext)).ToList();
            var selectedTabItemId = tabablzControl.SelectedIndex >= 0 ? tabItems[tabablzControl.SelectedIndex].Id : (Guid?)null;

            var layoutStructureTabSet = new LayoutStructureTabSet(
                id,
                selectedTabItemId,
                tabItems
                );

            layoutStructureTabSets.Add(layoutStructureTabSet);
        }

        private static Guid? IdOrNull(object o)
        {
            return o == null ? (Guid?)null : Guid.NewGuid();
        }

    }
}
