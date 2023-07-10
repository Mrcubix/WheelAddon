namespace WheelAddon.Lib.Serializables
{
    public class SerializablePluginProperty
    {
        public SerializablePluginProperty()
        {
            Value = null!;
            Identifier = -1;
        }

        public SerializablePluginProperty(string value, int identifier)
        {
            Value = value;
            Identifier = identifier;
        }

        public int Identifier { get; set; }
        public string? Value { get; set; }
    }
}