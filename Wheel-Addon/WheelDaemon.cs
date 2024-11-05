using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Plugin;
using OTD.Backport.Parsers.Tablet;
using WheelAddon.Filter;
using WheelAddon.Lib.Contracts;
using WheelAddon.Lib.Serializables;
using WheelAddon.Converters;
using System.Linq;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Desktop.Reflection;
using WheelAddon.Serializables.Bindings;
using WheelAddon.Serializables.Modes;
using System.Threading.Tasks;
using OpenTabletDriver.External.Common.Serializables;

namespace WheelAddon
{
    public class WheelDaemon : IWheelDaemon, IDisposable
    {
        private const string NAME = WheelHandler.PLUGIN_NAME + " Daemon";

        private readonly JsonSerializerSettings serializerSettings = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new List<JsonConverter>
            {
                new PluginSettingStoreConverter(),
                new PluginSettingConverter()
            }
        };

        private readonly string wheelSettingsPath = Path.Combine(AppInfo.Current.AppDataDirectory, "Wheel-Addon.json");
        private readonly Dictionary<int, TypeInfo> identifierToPlugin = new();

        #region Initialization

        public WheelDaemon()
        {
            OnReady = null!;
            OnSettingsChanged = null!;
        }

        public WheelDaemon(Settings settings)
        {
            Settings = settings;

            OnReady = null!;
            OnSettingsChanged = null!;

            Initialize(false);
        }

        public void Initialize(bool doLoadSettings = true)
        {
            // load settings
            if (doLoadSettings)
                LoadSettings(wheelSettingsPath);

            if (HasErrored)
            {
                WheelHandler.HandlerLog(this, NAME, "Failed to load settings, aborting initialization.");
                return;
            }

            // identify plugins
            IdentifyPlugins();

            // construct bindings
            Settings.ConstructBindings();

            OnSettingsChanged?.Invoke(this, Settings);

            IsReady = true;
            OnReady?.Invoke(this, null!);

            WheelHandler.HandlerLogSpacer(this, NAME);
            WheelHandler.HandlerLog(this, NAME, "Initialized.");
            WheelHandler.HandlerLogSpacer(this, NAME);
        }

        #endregion

        public Settings Settings { get; private set; } =  Settings.Default;

        public event EventHandler OnReady;
        public event EventHandler<Settings> OnSettingsChanged;

        #region Contract

        public bool IsCalibrating { get; private set; }

        public bool HasErrored { get; private set; }

        public bool IsReady { get; private set; }

        public Task<bool> ApplyWheelBindings(SerializableSettings bindings)
        {
            WheelHandler.HandlerLog(this, NAME, "Applying wheel bindings...");

            // build Settings from SerializableSettings, take each properties and convert them into PluginSettingStore s using the identifierToPlugin dictionary

            //                                                                                      //
            // ------------------------------- Simple Mode Bindings ------------------------------- //
            //                                                                                      //

            var serializableSimpleModeBindings = bindings.SimpleMode;

            // Grab the plugin identifiers and values
            var clockwisePluginIdentifier = serializableSimpleModeBindings.Clockwise.PluginProperty?.Identifier;
            var counterClockwisePluginIdentifier = serializableSimpleModeBindings.CounterClockwise.PluginProperty?.Identifier;

            var clockwisePluginValue = serializableSimpleModeBindings.Clockwise.PluginProperty?.Value;
            var counterClockwisePluginValue = serializableSimpleModeBindings.CounterClockwise.PluginProperty?.Value;

            // convert the simple mode bindings
            var simpleModeBindings = new SimpleModeWheelBindings()
            {
                Clockwise = new WheelBinding()
                {
                    Store = clockwisePluginIdentifier != null ? 
                        new PluginSettingStore(identifierToPlugin[clockwisePluginIdentifier.Value]) : null
                },
                CounterClockwise = new WheelBinding()
                {
                    Store = counterClockwisePluginIdentifier != null ? 
                        new PluginSettingStore(identifierToPlugin[counterClockwisePluginIdentifier.Value]) : null,
                }
            };

            // set the properties values of each stores settings
            if (clockwisePluginIdentifier != null)
                simpleModeBindings.Clockwise.Store?.Settings.SingleOrDefault(x => x.Property == "Property")!.SetValue(clockwisePluginValue!);

            if (counterClockwisePluginIdentifier != null)
                simpleModeBindings.CounterClockwise.Store?.Settings.SingleOrDefault(x => x.Property == "Property")!.SetValue(counterClockwisePluginValue!);

            Settings.SimpleMode = simpleModeBindings;

            //                                                                                        //
            // ------------------------------- Advanced Mode Bindings ------------------------------- //
            //                                                                                        //

            Settings.AdvancedMode.Clear();

            // Grab the plugin identifiers and values for each advanced mode binding
            var serializableAdvancedModeBindings = bindings.AdvancedMode;

            foreach (var serializableAdvancedModeBinding in serializableAdvancedModeBindings)
            {
                var pluginIdentifier = serializableAdvancedModeBinding.PluginProperty?.Identifier;
                var pluginValue = serializableAdvancedModeBinding.PluginProperty?.Value;

                // convert the advanced mode bindings
                var advancedModeBinding = new RangedWheelBinding()
                {
                    Store = pluginIdentifier != null ? 
                        new PluginSettingStore(identifierToPlugin[pluginIdentifier.Value]) : null,
                    Start = serializableAdvancedModeBinding.Start,
                    End = serializableAdvancedModeBinding.End
                };

                // set the properties values of each stores settings
                if (pluginIdentifier != null)
                    advancedModeBinding.Store?.Settings.SingleOrDefault(x => x.Property == "Property")!.SetValue(pluginValue!);

                Settings.AdvancedMode.Add(advancedModeBinding);
            }

            // construct bindings
            Settings.ConstructBindings();

            return Task.FromResult(true);
        }

        public Task<List<SerializablePlugin>> GetPlugins()
        {
            WheelHandler.HandlerLog(this, NAME, "Getting plugins...");

            List<SerializablePlugin> plugins = new();

            // iterate through each plugin and add it to the list
            foreach (var IdentifierPluginPair in identifierToPlugin)
            {
                var plugin = IdentifierPluginPair.Value;

                var store = new PluginSettingStore(plugin);

                // Construct the binding plugin so we can get the valid properties
                var validateBinding = store.Construct<IValidateBinding>();

                // Convert the plugin to a serializable plugin, so it can be transferred over RPC
                var serializablePlugin = new SerializablePlugin(plugin.GetCustomAttribute<PluginNameAttribute>()?.Name, 
                                                                plugin.FullName, 
                                                                IdentifierPluginPair.Key, 
                                                                validateBinding.ValidProperties);

                plugins.Add(serializablePlugin);
            }

            return Task.FromResult(plugins);
        }

        public Task<int> GetMaxWheelValue()
        {
            if (HasErrored)
                return Task.FromResult(-1);

            return Task.FromResult(Settings.MaxWheelValue);
        }

        public Task<SerializableSettings?> GetWheelBindings()
        {
            WheelHandler.HandlerLog(this, NAME, "Getting wheel bindings...");

            if (HasErrored)
                return Task.FromResult<SerializableSettings?>(null!);

            var serializedSettings = Settings.ToSerializable(identifierToPlugin);
            
            return Task.FromResult<SerializableSettings?>(serializedSettings);
        }

        public Task<bool> SaveWheelBindings() => Task.FromResult(SaveSettings());

        #endregion

        #region Internal methods

        public Task<bool> StartCalibration()
        {
            WheelHandler.HandlerLog(this, NAME, "Starting calibration...");

            Settings.MaxWheelValue = 0;

            if (HasErrored)
                return Task.FromResult(false);

            if (IsCalibrating)
                return Task.FromResult(false);

            this.IsCalibrating = true;

            WheelHandler.OnWheelUsage += OnWheelReport;

            return Task.FromResult(true);
        }

        public Task<int> StopCalibration()
        {
            WheelHandler.HandlerLog(this, NAME, "Stopping calibration...");

            if (!IsCalibrating)
                return Task.FromResult(-1);

            if (HasErrored)
                return Task.FromResult(-1);

            this.IsCalibrating = false;

            WheelHandler.OnWheelUsage -= OnWheelReport;

            return Task.FromResult(Settings.MaxWheelValue);
        }

        private void OnWheelReport(object? sender, IWheelReport report)
        {
            if (!IsCalibrating)
                return;

            if (report.Wheel > Settings.MaxWheelValue)
            {
                Settings.MaxWheelValue = report.Wheel;
#if DEBUG
                WheelHandler.HandlerLog(this, NAME, $"New max wheel value: {Settings.MaxWheelValue}", LogLevel.Debug);
#endif
            }
        }

        #endregion

        #region Settings

        private void IdentifyPlugins()
        {
            // obtain all IBinding plugins
            var plugins = AppInfo.PluginManager.GetChildTypes<IBinding>().ToList();

            for (var i = 0; i < plugins.Count; i++)
            {
                var plugin = plugins[i];

                // build an identifier from all the characters in the plugin name
                var identifier = 0;

                foreach (var character in plugin.Name)
                {
                    identifier += character;
                }

                // add its index to the identifier
                identifier += i;

                identifierToPlugin.Add(identifier, plugin);
            }
        }

        private void LoadSettings(string path)
        {
            if (!File.Exists(wheelSettingsPath))
                SaveSettings();

            var res = Settings.TryLoadFrom(wheelSettingsPath, out var temp);

            if (res)
                Settings = temp;
            else
                HasErrored = true;
        }

        private bool SaveSettings()
        {
            WheelHandler.HandlerLog(this, NAME, "Saving settings...");

            try
            {
                File.WriteAllText(wheelSettingsPath, JsonConvert.SerializeObject(Settings, serializerSettings));
                return true;
            }
            catch (Exception e)
            {
                WheelHandler.HandlerLog(this, NAME, $"Failed to save settings: {e.Message}", LogLevel.Error);
            }

            return false;
        }

        public void Dispose()
        {
            IsReady = false;
            identifierToPlugin.Clear();
            Settings = null!;
        }

        #endregion
    }
}