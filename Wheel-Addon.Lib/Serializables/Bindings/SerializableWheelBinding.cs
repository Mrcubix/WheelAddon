using Newtonsoft.Json;

namespace WheelAddon.Lib.Serializables.Bindings
{
    public class SerializableWheelBinding
    {
        public SerializableWheelBinding()
        {
            PluginProperty = null;
        }

        public SerializableWheelBinding(SerializablePluginProperty? pluginProperty)
        {
            PluginProperty = pluginProperty;
        }

        [JsonProperty("Store")]
        public SerializablePluginProperty? PluginProperty { get; set; }
    }
}