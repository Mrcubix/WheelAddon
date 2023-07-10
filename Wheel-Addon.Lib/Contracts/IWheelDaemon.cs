using System.Collections.Generic;
using System.Threading.Tasks;
using WheelAddon.Lib.Serializables;

namespace WheelAddon.Lib.Contracts
{
    public interface IWheelDaemon
    {
        public Task<List<SerializablePlugin>> GetPlugins();
        public Task<int> GetMaxWheelValue();
        public Task<SerializableSettings?> GetWheelBindings();
        public Task<bool> ApplyWheelBindings(SerializableSettings bindings);
        public Task<bool> SaveWheelBindings();
        public Task<bool> StartCalibration();
        public Task<int> StopCalibration();
    }
}