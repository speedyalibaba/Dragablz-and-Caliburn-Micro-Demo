using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Links.Contract.Models
{
    public class LayoutStructureTabItem
    {
        public LayoutStructureTabItem(Guid id, object tabContentViewModel)
        {
            Id = id;
            ViewModelType = tabContentViewModel.GetType();
        }

        [JsonConstructor]
        public LayoutStructureTabItem(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }

        public Type ViewModelType { get; set; }
    }
}
