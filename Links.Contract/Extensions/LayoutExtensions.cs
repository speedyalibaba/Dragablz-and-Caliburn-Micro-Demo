using Links.Contract.Models;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Controls;

namespace Links.Contract.Extensions
{
    public static class LayoutExtensions
    {
        #region Methods

        public static LayoutStructure AddTabs(this LayoutStructure layoutStructure, params IScreen[] vms)
        {
            var window = layoutStructure.Windows.LastOrDefault();
            if (window == null)
                throw new InvalidOperationException("You have to first add a window!");
            else if (window.Branches.Count > 0)
                throw new InvalidOperationException("You can't add TabItems directly to your Window if you already branched it!");
            else if (window.TabSets.Count > 0)
                throw new InvalidOperationException("You already added TabItems to this Window!");

            window.TabSets.Add(CreateTabSet(vms));

            return layoutStructure;
        }

        public static LayoutStructure AddWindow(this LayoutStructure layoutStructure, double width, double height, double left, double top)
        {
            var window = new LayoutStructureWindow(new List<LayoutStructureBranch>(), new List<LayoutStructureTabSet>());
            window.Width = width;
            window.Height = height;
            window.Left = left;
            window.Top = top;

            layoutStructure.Windows.Add(window);
            return layoutStructure;
        }

        public static LayoutStructure AddWindow(this LayoutStructure layoutStructure, double width, double height)
        {
            return AddWindow(layoutStructure, width, height, 0, 0);
        }

        public static LayoutStructure AddWindow(this LayoutStructure layoutStructure)
        {
            return AddWindow(layoutStructure, 0, 0, 0, 0);
        }

        public static LayoutStructure Branch(
            this LayoutStructure layoutStructure,
            Expression<Func<LayoutStructure, LayoutStructure>> firstBranch,
            Expression<Func<LayoutStructure, LayoutStructure>> secondBranch,
            Orientation orientation, double ratio)
        {
            var result = CreateBranchesAndTabs(layoutStructure, firstBranch, secondBranch, orientation, ratio);

            return result.LayoutStructure;
        }

        public static LayoutStructure Branch(
            this LayoutStructure layoutStructure,
            Expression<Func<LayoutStructure, LayoutStructure>> firstBranch,
            Expression<Func<LayoutStructure, LayoutStructure>> secondBranch,
            Orientation orientation)
        {
            return layoutStructure.Branch(firstBranch, secondBranch, orientation, 0.5);
        }

        public static (LayoutStructure LayoutStructure, Guid? BranchId) CreateBranchesAndTabs(
            LayoutStructure layoutStructure,
            Expression<Func<LayoutStructure, LayoutStructure>> firstBranch,
            Expression<Func<LayoutStructure, LayoutStructure>> secondBranch,
            Orientation orientation, double ratio)
        {
            var window = layoutStructure.Windows.LastOrDefault();
            if (window == null)
                throw new InvalidOperationException("You have to first add a window!");

            Guid? childFirstBranchId = null, childSecondBranchId = null, childFirstTabSetId = null, childSecondTabSetId = null;

            HandleChild(ref layoutStructure, window, firstBranch, ref childFirstBranchId, ref childFirstTabSetId);
            HandleChild(ref layoutStructure, window, secondBranch, ref childSecondBranchId, ref childSecondTabSetId);

            var branchId = Guid.NewGuid();
            window.Branches.Add(new LayoutStructureBranch(branchId, childFirstBranchId, childSecondBranchId, childFirstTabSetId, childSecondTabSetId, orientation, ratio));

            return (layoutStructure, branchId);
        }

        private static LayoutStructureTabItem CreateTabItem(object vm)
        {
            return new LayoutStructureTabItem(Guid.NewGuid(), vm);
        }

        private static LayoutStructureTabSet CreateTabSet(IEnumerable<object> vms)
        {
            var tabItems = vms.Select(CreateTabItem);
            return new LayoutStructureTabSet(Guid.NewGuid(), null, tabItems);
        }

        private static object GetValue(Expression expression)
        {
            var objectMember = Expression.Convert(expression, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            return getter();
        }

        private static void HandleChild(ref LayoutStructure layoutStructure, LayoutStructureWindow window, Expression<Func<LayoutStructure, LayoutStructure>> expression, ref Guid? childFirstBranchId, ref Guid? childFirstTabSetId)
        {
            MethodCallExpression methodExpression = expression.Body as MethodCallExpression;
            if (methodExpression.Method.Name == nameof(AddTabs))    //AddTabs
            {
                var vms = new List<IScreen>();
                vms.AddRange((IScreen[])GetValue(methodExpression.Arguments[1]));

                var newTabset = CreateTabSet(vms);
                window.TabSets.Add(newTabset);
                childFirstTabSetId = newTabset.Id;
                childFirstBranchId = null;
            }
            else if (methodExpression.Method.Name == nameof(Branch))//Branch
            {
                var firstInnerExpression = (methodExpression.Arguments[1] as UnaryExpression).Operand as Expression<Func<LayoutStructure, LayoutStructure>>;
                var secondInnerExpression = (methodExpression.Arguments[2] as UnaryExpression).Operand as Expression<Func<LayoutStructure, LayoutStructure>>;
                var innerOrientation = (Orientation)(methodExpression.Arguments[3] as ConstantExpression).Value;
                var innerRatio = methodExpression.Arguments.Count > 4 ? (double)(methodExpression.Arguments[4] as ConstantExpression).Value : 0.5D;

                var result = CreateBranchesAndTabs(layoutStructure, firstInnerExpression, secondInnerExpression, innerOrientation, innerRatio);
                layoutStructure = result.LayoutStructure;
                childFirstBranchId = result.BranchId;
                childFirstTabSetId = null;
            }
            else
            {
                throw new InvalidOperationException($"{methodExpression.Method.Name} is not supported in this context");
            }
        }

        #endregion Methods
    }
}