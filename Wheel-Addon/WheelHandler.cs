using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OTD.EnhancedOutputMode.Lib.Interface;

namespace WheelAddon.Filter
{
    [PluginName("Wheel Addon")]
    public class WheelHandler : IFilter, IAuxFilter
    {
        public Vector2 Filter(Vector2 input) => input;

        public IAuxReport AuxFilter(IAuxReport report)
        {
            return report;
        }

        public FilterStage FilterStage => FilterStage.PreTranspose;

        [Property("Numerical Input Box Property"),
         Unit("Some Unit Here"),
         DefaultPropertyValue(727),
         ToolTip("Filter template:\n\n" +
                 "A property that appear as an input box.\n\n" +
                 "Has a numerical value.")
        ]
        public int ExampleNumericalProperty { get; set; }
        
        [BooleanProperty("Boolean Property", ""),
         DefaultPropertyValue(true),
         ToolTip("Area Randomizer:\n\n" +
                 "A property that appear as a check box.\n\n" +
                 "Has a Boolean value")
        ]
        public bool ExampleBooleanProperty { set; get; }
    }
}
