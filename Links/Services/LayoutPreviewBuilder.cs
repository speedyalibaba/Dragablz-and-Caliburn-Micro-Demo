using Links.Contract.Models;
using Links.Localization;
using Links.WPF;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Links.Services
{
    public static class LayoutPreviewBuilder
    {
        #region Methods

        public static ContentControl BuildPreview(LayoutStructure layout, double maxHeight, double maxWidth)
        {
            var preview = new ContentControl();

            if (layout.Windows.Count == 1)
            {
                var layoutWindow = layout.Windows.First();
                var aspectRatio = layoutWindow.Width / layoutWindow.Height;
                aspectRatio = double.IsNaN(aspectRatio) ? 8D / 5D : aspectRatio;
                var windowPreview = BuildWindow(layoutWindow);
                SetSize(windowPreview, maxHeight, maxWidth, aspectRatio);

                preview.Content = windowPreview;
            }
            else if (layout.Windows.Count > 1)
            {
                preview.Content = BuildLayout(layout, maxHeight, maxWidth);
            }
            else
            {
                preview.Content = new PackIcon() { Kind = PackIconKind.Error, Height = 20, Width = 20 };
                preview.Margin = new Thickness(20);
            }

            string tooltip;
            if (layout.Name.StartsWith("NewLayout"))
            {
                tooltip = $"{Translations.NewLayout}{layout.Name.Substring(9, layout.Name.Length - 9)}";
            }
            else
            {
                tooltip = Translations.ResourceManager.GetString(layout.Name);
                tooltip = string.IsNullOrWhiteSpace(tooltip) ? layout.Name : tooltip;
            }
            preview.ToolTip = tooltip;

            return preview;
        }

        private static Grid BuildLayout(LayoutStructure layout, double maxHeight, double maxWidth)
        {
            var resultGrid = new Grid();

            var xMin = layout.Windows.Min(w => w.Left);
            var xMax = layout.Windows.Max(w => w.Left + w.Width);
            var yMin = layout.Windows.Min(w => w.Top);
            var yMax = layout.Windows.Max(w => w.Top + w.Height);

            var originalWidth = xMax - xMin;
            var originalHeight = yMax - yMin;
            var aspectRatio = originalWidth / originalHeight;

            var resultSize = SetSize(resultGrid, maxHeight, maxWidth, aspectRatio);

            double leftUpperMargin = 0;  //only if no size available
            double rightLowerMargin = 0;
            foreach (var window in layout.Windows)
            {
                var preview = BuildWindow(window);

                var left = (window.Left - xMin) * (resultSize.Width / originalWidth);
                var top = (window.Top - yMin) * (resultSize.Height / originalHeight);
                var right = (originalWidth - (window.Left - xMin + window.Width)) * (resultSize.Width / originalWidth);
                var bottom = (originalHeight - (window.Top - yMin + window.Height)) * (resultSize.Height / originalHeight);

                left = double.IsNaN(left) ? leftUpperMargin : left;
                top = double.IsNaN(top) ? leftUpperMargin : top;
                right = double.IsNaN(right) ? rightLowerMargin : right;
                bottom = double.IsNaN(bottom) ? rightLowerMargin : bottom;
                leftUpperMargin += 15;
                rightLowerMargin -= 15;

                preview.Margin = new Thickness(left, top, right, bottom);
                resultGrid.Children.Add(preview);
            }

            return resultGrid;
        }

        private static Border BuildWindow(LayoutStructureWindow window)
        {
            var windowBorder = GetWindowBorder();

            if (window.Branches.Count > 0)
            {
                var branchIndex = window.Branches.ToDictionary(b => b.Id);
                var rootBranch = GetRoot(branchIndex);

                windowBorder.Child = PopulateWindowBranch(window, rootBranch, branchIndex);
            }
            else
            {
                var layoutStructureTabSet = window.TabSets.FirstOrDefault();
                if (layoutStructureTabSet != null)
                {
                    windowBorder.Child = GetTabBorder(GetIcon(layoutStructureTabSet.TabItems.FirstOrDefault()?.ViewModelType));//TODO multiple icons
                }
            }

            return windowBorder;
        }

        private static IEnumerable<Guid> ChildBranchIds(LayoutStructureBranch branch)
        {
            if (branch.ChildFirstBranchId.HasValue)
                yield return branch.ChildFirstBranchId.Value;
            if (branch.ChildSecondBranchId.HasValue)
                yield return branch.ChildSecondBranchId.Value;
        }

        private static PackIconKind? GetIcon(Type viewModelType)
        {
            if (viewModelType == typeof(SettingsViewModel))
                return PackIconKind.Settings;
            else if (viewModelType == typeof(Test1ViewModel))
                return PackIconKind.Images;
            else if (viewModelType == typeof(Test2ViewModel))
                return PackIconKind.Airplay;
            else if (viewModelType == typeof(NewViewModel))
                return PackIconKind.NewBox;
            return null;
        }

        private static LayoutStructureBranch GetRoot(Dictionary<Guid, LayoutStructureBranch> branches)
        {
            var lookup = branches.Values.SelectMany(ChildBranchIds).Distinct().ToLookup(guid => guid);
            return branches.Values.Single(branch => !lookup.Contains(branch.Id));
        }

        private static Border GetTabBorder(PackIconKind? icon)
        {
            return new Border()
            {
                CornerRadius = new CornerRadius(8),
                Background = new SolidColorBrush(Colors.DarkGray),
                Margin = new Thickness(3),
                Child = icon.HasValue ? new PackIcon()
                {
                    Kind = icon.Value,
                    Width = 20,
                    Height = 20,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                } : null
            };
        }

        private static Border GetWindowBorder()
        {
            return new Border()
            {
                CornerRadius = new CornerRadius(8),
                Background = new SolidColorBrush(Colors.DimGray),
                Padding = new Thickness(3)
            };
        }

        private static Grid PopulateWindowBranch(
            LayoutStructureWindow layoutStructureWindow,
            LayoutStructureBranch layoutStructureBranch,
            IDictionary<Guid, LayoutStructureBranch> layoutStructureBranchIndex)
        {
            var resultGrid = new Grid();
            (int Row, int Column) firstItem = (0, 0);
            (int Row, int Column) secondItem = layoutStructureBranch.Orientation == Orientation.Vertical ? (1, 0) : (0, 1);

            if (layoutStructureBranch.Orientation == Orientation.Vertical)
            {
                RowDefinition gridRow1 = new RowDefinition() { Height = new GridLength(layoutStructureBranch.Ratio, GridUnitType.Star) };
                RowDefinition gridRow2 = new RowDefinition() { Height = new GridLength(1 - layoutStructureBranch.Ratio, GridUnitType.Star) };
                resultGrid.RowDefinitions.Add(gridRow1);
                resultGrid.RowDefinitions.Add(gridRow2);
            }
            else
            {
                ColumnDefinition gridCol1 = new ColumnDefinition() { Width = new GridLength(layoutStructureBranch.Ratio, GridUnitType.Star) };
                ColumnDefinition gridCol2 = new ColumnDefinition() { Width = new GridLength(1 - layoutStructureBranch.Ratio, GridUnitType.Star) };
                resultGrid.ColumnDefinitions.Add(gridCol1);
                resultGrid.ColumnDefinitions.Add(gridCol2);
            }

            if (layoutStructureBranch.ChildFirstBranchId.HasValue)
            {
                var firstChildBranch = layoutStructureBranchIndex[layoutStructureBranch.ChildFirstBranchId.Value];
                var newChild = PopulateWindowBranch(layoutStructureWindow, firstChildBranch, layoutStructureBranchIndex);
                Grid.SetRow(newChild, firstItem.Row);
                Grid.SetColumn(newChild, firstItem.Column);
                resultGrid.Children.Add(newChild);
            }
            else if (layoutStructureBranch.ChildFirstTabSetId.HasValue)
            {
                var newChild = GetTabBorder(GetIcon(layoutStructureWindow.TabSets.Single(t => t.Id == layoutStructureBranch.ChildFirstTabSetId.Value).TabItems.FirstOrDefault()?.ViewModelType));//TODO multiple icons
                Grid.SetRow(newChild, firstItem.Row);
                Grid.SetColumn(newChild, firstItem.Column);
                resultGrid.Children.Add(newChild);
            }

            if (layoutStructureBranch.ChildSecondBranchId.HasValue)
            {
                var secondChildBranch = layoutStructureBranchIndex[layoutStructureBranch.ChildSecondBranchId.Value];
                var newChild = PopulateWindowBranch(layoutStructureWindow, secondChildBranch, layoutStructureBranchIndex);
                Grid.SetRow(newChild, secondItem.Row);
                Grid.SetColumn(newChild, secondItem.Column);
                resultGrid.Children.Add(newChild);
            }
            else if (layoutStructureBranch.ChildSecondTabSetId.HasValue)
            {
                var newChild = GetTabBorder(GetIcon(layoutStructureWindow.TabSets.Single(t => t.Id == layoutStructureBranch.ChildSecondTabSetId.Value).TabItems.FirstOrDefault()?.ViewModelType));//TODO multiple icons
                Grid.SetRow(newChild, secondItem.Row);
                Grid.SetColumn(newChild, secondItem.Column);
                resultGrid.Children.Add(newChild);
            }

            return resultGrid;
        }

        private static (double Width, double Height) SetSize(FrameworkElement element, double maxHeight, double maxWidth, double aspectRatio)
        {
            if (maxWidth / maxHeight < aspectRatio)
            {
                element.Width = maxWidth;
                element.Height = maxWidth / aspectRatio;
            }
            else
            {
                element.Height = maxHeight;
                element.Width = maxHeight * aspectRatio;
            }

            return (element.Width, element.Height);
        }

        #endregion Methods
    }
}