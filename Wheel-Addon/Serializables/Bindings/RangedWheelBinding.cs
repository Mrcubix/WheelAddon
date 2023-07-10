using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Reflection;
using WheelAddon.Lib.Serializables.Bindings;

namespace WheelAddon.Serializables.Bindings
{
    public class RangedWheelBinding : WheelBinding
    {
        [JsonConstructor]
        public RangedWheelBinding() 
        {
        }

        public RangedWheelBinding(int start, int end, PluginSettingStore? store) : base(store)
        {
            Start = start;
            End = end;
        }

        [JsonProperty("Start")]
        public int Start { get; set; } = 0;

        [JsonProperty("End")]
        public int End { get; set; } = 0;

        public new SerializableRangedWheelBinding ToSerializable(Dictionary<int, TypeInfo> identifierToPlugin)
        {
            var serializableWheelBinding = base.ToSerializable(identifierToPlugin);

            return new SerializableRangedWheelBinding()
            {
                Start = Start,
                End = End,
                PluginProperty = serializableWheelBinding.PluginProperty
            };
        }
    }
}