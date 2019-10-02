using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Links.Contract.Models
{
    public class LayoutStructure
    {
        public LayoutStructure(string name, IEnumerable<LayoutStructureWindow> windows)
        {
            if (windows == null) throw new ArgumentNullException(nameof(windows));

            Windows = windows.ToList();
            Name = name;
        }

        public ICollection<LayoutStructureWindow> Windows { get; }

        public string Name { get; set; }
    }
}
