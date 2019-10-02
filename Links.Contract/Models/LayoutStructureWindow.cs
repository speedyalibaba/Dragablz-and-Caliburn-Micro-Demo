using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Links.Contract.Models
{
    public class LayoutStructureWindow
    {
        #region Constructors

        public LayoutStructureWindow(IEnumerable<LayoutStructureBranch> branches, IEnumerable<LayoutStructureTabSet> tabSets)
        {
            if (branches == null) throw new ArgumentNullException(nameof(branches));
            if (tabSets == null) throw new ArgumentNullException(nameof(tabSets));

            Branches = branches.ToList();
            TabSets = tabSets.ToList();
        }

        #endregion Constructors

        #region Properties

        public List<LayoutStructureBranch> Branches { get; }

        public double Height { get; set; }

        public double Left { get; set; }

        public List<LayoutStructureTabSet> TabSets { get; }

        public double Top { get; set; }

        public bool Topmost { get; set; }

        public double Width { get; set; }

        public WindowState WindowState { get; set; }

        #endregion Properties
    }
}