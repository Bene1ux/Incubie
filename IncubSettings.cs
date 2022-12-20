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
        [Menu("Show experience per monster")]
        public ToggleNode ShowExpPerMonster { get; set; } = new ToggleNode(true);

        [Menu("Refresh gem and incubator info")]
        public ButtonNode Refresh { get; set; } = new ButtonNode();
        public RangeNode<int> X { get; set; } = new RangeNode<int>(200, 0, 1920);
        public RangeNode<int> Y { get; set; } = new RangeNode<int>(200, 0, 1080);

        //[Menu("Pause time")]
        //public RangeNode<int> PauseTime { get; set; } = new RangeNode<int>(200,0,5000);
        //[Menu("Party count")]
        //public RangeNode<int> PartyCount { get; set; } = new RangeNode<int>(6, 1, 6);
    }
}