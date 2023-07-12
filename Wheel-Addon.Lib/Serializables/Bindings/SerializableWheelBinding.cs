using Newtonsoft.Json;

namespace WheelAddon.Lib.Serializables.Bindings
{
    public class SerializableWheelBinding
    {
        public SerializableWheelBinding()
        {
            PluginProperty = null;
        }

        public SerializableWheelBinding(SerializablePluginSettings? pluginProperty)
        {
            PluginProperty = pluginProperty;
        }

        [JsonProperty("Store")]
        public SerializablePluginSettings? PluginProperty { get; set; }
    }
}