using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;

namespace Incubie
{
    public class IncubSettings : ISettings
    {
        public ToggleNode Enable { get; set; } = new ToggleNode(true);

        [Menu("Example check box", "This check box disable some functionality")]
        public ToggleNode MyCheckboxOption { get; set; } = new ToggleNode(true);
    }
}