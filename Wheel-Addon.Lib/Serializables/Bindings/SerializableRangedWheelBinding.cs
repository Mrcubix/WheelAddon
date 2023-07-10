using Newtonsoft.Json;

namespace WheelAddon.Lib.Serializables.Bindings
{
    public class SerializableRangedWheelBinding : SerializableWheelBinding
    {
        public SerializableRangedWheelBinding()
        {
        }

        public SerializableRangedWheelBinding(int start, int end)
        {
            Start = start;
            End = end;
        }

        [JsonProperty("Start")]
        public int Start { get; set; }

        [JsonProperty("End")]
        public int End { get; set; }
    }
}