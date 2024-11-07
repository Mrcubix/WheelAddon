using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using OpenTabletDriver.Plugin;
using WheelAddon.Filter;
using WheelAddon.Serializables.Bindings;
using WheelAddon.Serializables.Modes;
using WheelAddon.Converters;
using WheelAddon.Lib.Serializables;
using System.Reflection;

namespace WheelAddon
{
    public class Settings
    {
        private static readonly JsonSerializer _serializer = new()
        {
            Formatting = Formatting.Indented,
        };
        
        private static readonly JsonSerializerSettings _serializerSettings = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new List<JsonConverter>
            {
                new PluginSettingStoreConverter(),
                new PluginSettingConverter()
            }
        };

        [JsonConstructor]
        public Settings()
        {
            SimpleMode = new SimpleModeWheelBindings();
            AdvancedMode = new List<RangedWheelBinding>();
        }

        public Settings(SimpleModeWheelBindings simpleMode, List<RangedWheelBinding> advancedMode)
        {
            SimpleMode = simpleMode;
            AdvancedMode = advancedMode;
        }

        [JsonProperty("SimpleMode")]
        public SimpleModeWheelBindings SimpleMode { get; set; }

        [JsonProperty("AdvancedMode")]
        public List<RangedWheelBinding> AdvancedMode { get; set; }

        [JsonProperty("MaxWheelValue")]
        public int MaxWheelValue { get; set; } = 71;

        public static bool TryLoadFrom(string path, out Settings settings)
        {
            settings = null!;

            if (File.Exists(path))
            {
                try
                {
                    settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(path), _serializerSettings)!;

                    return true;
                }
                catch(Exception e)
                {
                    WheelHandler.HandlerLog(null, WheelHandler.PLUGIN_NAME, $"Failed to load settings from {path}: {e}", LogLevel.Error);
                }
            }
            else
            {
                WheelHandler.HandlerLog(null, WheelHandler.PLUGIN_NAME, $"Failed to load settings from {path}: file does not exist", LogLevel.Error);
            }

            return false;
        }

        public static Settings Default => new();

        public void ConstructBindings()
        {
            SimpleMode.Construct();
            ConstructAdvancedModeBindings();
        }

        public void ConstructAdvancedModeBindings()
        {
            // advanced mode
            var advancedMode = this.AdvancedMode;

            foreach (var advancedBinding in advancedMode)
                advancedBinding.Construct();
        }

        public SerializableSettings ToSerializable(Dictionary<int, TypeInfo> identifierToPlugin)
        {
            return new SerializableSettings
            {
                SimpleMode = SimpleMode.ToSerializable(identifierToPlugin),
                AdvancedMode = AdvancedMode.Select(b => b.ToSerializable(identifierToPlugin)).ToList(),
                MaxWheelValue = MaxWheelValue
            };
        }
    }
}