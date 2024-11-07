using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin;
using WheelAddon.Lib.Serializables.Bindings;
using OpenTabletDriver.External.Common.Serializables;
using WheelAddon.Lib.Binding;

namespace WheelAddon.Serializables.Bindings
{
    public class WheelBinding : Binding
    {
        [JsonConstructor]
        public WheelBinding()
        {
        }

        public WheelBinding(PluginSettingStore? store)
        {
            Store = store;
        }

        [JsonProperty("Store")]
        public PluginSettingStore? Store { get; set; }

        [JsonIgnore]
        public IBinding? Binding { get; set; }

        public override void Press() => Binding?.Press();

        public override void Release() => Binding?.Release();

        public override void Construct() => Binding = Store?.Construct<IBinding>();

        public override SerializableWheelBinding ToSerializable(Dictionary<int, TypeInfo> identifierToPlugin)
        {
            var identifier = identifierToPlugin.FirstOrDefault(x => x.Value.FullName == Store?.Path);
            var value = Store?.Settings.FirstOrDefault(x => x.Property == "Property");

            return new SerializableWheelBinding()
            {
                PluginProperty = new SerializablePluginSettings()
                {
                    Identifier = identifier.Key,
                    Value = value?.GetValue<string?>()
                }
            };
        }
    }
}