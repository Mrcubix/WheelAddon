
using Newtonsoft.Json;
using WheelAddon.Lib.Serializables.Bindings;

namespace WheelAddon.Lib.Serializables.Modes
{
    public class SerializableSimpleModeWheelBindings
    {
        public SerializableSimpleModeWheelBindings()
        {
            Clockwise = new SerializableWheelBinding();
            CounterClockwise = new SerializableWheelBinding();
        }

        public SerializableSimpleModeWheelBindings(SerializableWheelBinding clockwise, SerializableWheelBinding counterClockwise)
        {
            Clockwise = clockwise;
            CounterClockwise = counterClockwise;
        }

        [JsonProperty("Clockwise")]
        public SerializableWheelBinding Clockwise { get; set; }

        [JsonProperty("CounterClockwise")]
        public SerializableWheelBinding CounterClockwise { get; set; }
    }
}