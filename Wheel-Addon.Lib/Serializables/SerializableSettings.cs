using System.Collections.Generic;
using Newtonsoft.Json;
using WheelAddon.Lib.Serializables.Bindings;
using WheelAddon.Lib.Serializables.Modes;

namespace WheelAddon.Lib.Serializables
{
    public class SerializableSettings
    {
        public SerializableSettings()
        {
            SimpleMode = new SerializableSimpleModeWheelBindings();
            AdvancedMode = new List<SerializableRangedWheelBinding>();
        }

        public SerializableSettings(SerializableSimpleModeWheelBindings simpleMode, List<SerializableRangedWheelBinding> advancedMode)
        {
            SimpleMode = simpleMode;
            AdvancedMode = advancedMode;
        }

        [JsonProperty("SimpleMode")]
        public SerializableSimpleModeWheelBindings SimpleMode { get; set; }

        [JsonProperty("AdvancedMode")]
        public List<SerializableRangedWheelBinding> AdvancedMode { get; set; }
            
        [JsonProperty("MaxWheelValue")]
        public int MaxWheelValue { get; set; } = 71;

        public static SerializableSettings Default => new()
        {
            SimpleMode = new SerializableSimpleModeWheelBindings()
            {
                Clockwise = new SerializableWheelBinding(),
                CounterClockwise = new SerializableWheelBinding()
            },
            AdvancedMode = new List<SerializableRangedWheelBinding>()
            {
                new SerializableRangedWheelBinding()
                {
                    Start = 0,
                    End = 20,
                }
            }
        };
    }
}