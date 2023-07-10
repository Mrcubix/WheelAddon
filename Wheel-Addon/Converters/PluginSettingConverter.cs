using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTabletDriver.Desktop.Reflection;

namespace WheelAddon.Converters
{
    public class PluginSettingConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PluginSetting);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            // build a PluginSettingsStore using value.Path as argument
            var value = JObject.Load(reader);

            if (value == null)
                return null!;

            if (value.TryGetValue("Property", out var property))
            {
                if (property == null)
                    return null!;
            }
            else
            {
                return null!;
            }

            if (value.TryGetValue("Value", out var settingValue))
            {
                if (settingValue == null)
                    return null!;
            }
            else
            {
                return null!;
            }

            return new PluginSetting(property?.Value<string>()!, settingValue?.Value<string>()!);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            // write a JObject with Path, Settings and Enable properties
            var store = (PluginSetting?)value;

            if (store == null)
            {
                writer.WriteNull();
                return;
            }

            var obj = new JObject
            {
                ["Property"] = store.Property,
                ["Value"] = store.Value,
                ["HasValue"] = store.HasValue
            };

            obj.WriteTo(writer);
        }
    }
}