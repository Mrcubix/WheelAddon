using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using WheelAddon.Lib.Serializables.Modes;
using WheelAddon.Serializables.Bindings;

namespace WheelAddon.Serializables.Modes
{
    public class SimpleModeWheelBindings
    {
        [JsonConstructor]
        public SimpleModeWheelBindings()
        {
        }

        public SimpleModeWheelBindings(WheelBinding clockwise, WheelBinding counterClockwise)
        {
            Clockwise = clockwise;
            CounterClockwise = counterClockwise;
        }

        [JsonProperty("Clockwise")]
        public WheelBinding Clockwise { get; set; } = new();

        [JsonProperty("CounterClockwise")]
        public WheelBinding CounterClockwise { get; set; } = new();

        [JsonProperty("ActionValue")]
        public int ActionValue { get; set; } = 20;

        public void Construct()
        {
            Clockwise.Construct();
            CounterClockwise.Construct();
        }

        public SerializableSimpleModeWheelBindings ToSerializable(Dictionary<int, TypeInfo> identifierToPlugin)
        {
            return new SerializableSimpleModeWheelBindings()
            {
                Clockwise = Clockwise.ToSerializable(identifierToPlugin),
                CounterClockwise = CounterClockwise.ToSerializable(identifierToPlugin),
            };
        }
    }
}