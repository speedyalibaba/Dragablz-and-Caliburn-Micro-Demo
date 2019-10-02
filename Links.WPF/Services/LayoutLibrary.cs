using Links.Contract.Extensions;
using Links.Contract.Models;
using Caliburn.Micro;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Links.WPF
{
    public static class LayoutLibrary
    {
        #region Fields

        private static List<LayoutStructure> _presets;

        #endregion Fields

        #region Properties

        public static List<LayoutStructure> Presets
        {
            get
            {
                if (_presets == null)
                {
                    _presets = new LayoutStructure[]
                    {
                        GetDefaultLayout(),
                        GetSimpleLayout(),
                        GetTwoWindowedLayout()
                    }.ToList();
                }
                return _presets;
            }
        }

        #endregion Properties

        #region Methods

        public static LayoutStructure GetDefaultLayout()
        {
            var structure = CreateStructure("DefaultStructure").AddWindow()
                .Branch(
                f => f.Branch(
                    f1 => f1.AddTabs(),
                    f2 => f.AddTabs(IoC.Get<Test1ViewModel>(null), IoC.Get<NewViewModel>(null)),
                    Orientation.Vertical, 0.4
                    ),
                s => s.AddTabs(IoC.Get<NewViewModel>(null), IoC.Get<NewViewModel>(null)),
                Orientation.Horizontal);
            return structure;
        }

        public static LayoutStructure GetSimpleLayout()
        {
            var structure = CreateStructure("SimpleLayout").AddWindow()
                .AddTabs(IoC.Get<NewViewModel>());
            return structure;
        }

        public static LayoutStructure GetTwoWindowedLayout()
        {
            var structure = CreateStructure("TwoWindowedLayout")
                .AddWindow(800, 500, 200, 100)
                    .Branch(
                        f => f.AddTabs(IoC.Get<NewViewModel>(null)),
                        s => s.Branch(
                            f1 => f1.AddTabs(),
                            s1 => s1.AddTabs(),
                            Orientation.Horizontal),
                        Orientation.Vertical)
                .AddWindow(500, 300, 1000, 300)
                    .AddTabs(IoC.Get<NewViewModel>());
            return structure;
        }

        private static LayoutStructure CreateStructure(string name)
        {
            return new LayoutStructure(name, new LayoutStructureWindow[] { });
        }

        #endregion Methods
    }
}