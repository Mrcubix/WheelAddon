using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using WheelAddon.Lib.Serializables.Bindings;

namespace WheelAddon.Lib.Binding
{
    public abstract class Binding
    {
        public abstract void Press();
        public abstract void Release();

        public virtual void Invoke()
        {
            _ = Task.Run(async () =>
            {
                Press();
                await Task.Delay(15);
                Release();
            });
        }

        public abstract void Construct();

        public abstract SerializableWheelBinding ToSerializable(Dictionary<int, TypeInfo> identifierToPlugin);
    }
}