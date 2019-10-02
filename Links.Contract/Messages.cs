using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Links.Contract.Messages
{
    public sealed class ConfigurationChanged { }

    public sealed class ConfigurationSwitched
    {
        public bool SwitchOnDeletion { get; set; }
    }

    public sealed class LayoutsChanged { }
}
