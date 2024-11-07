using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using WheelAddon.Lib.Serializables.Bindings;

namespace WheelAddon.Lib.Binding
{
    public abstract class RangedBinding : Binding
    {
        [JsonConstructor]
        public RangedBinding() 
        {
        }

        public RangedBinding(int start, int end)
        {
            Start = start;
            End = end;
        }

        [JsonProperty("Start")]
        public int Start { get; set; } = 0;

        [JsonProperty("End")]
        public int End { get; set; } = 0;
    }
}