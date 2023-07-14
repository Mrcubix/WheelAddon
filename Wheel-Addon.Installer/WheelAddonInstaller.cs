using System.Reflection;
using System.Threading.Tasks;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Desktop;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace WheelAddon.Installer
{
    [PluginName("Wheel Addon Installer")]
    public class WheelAddonInstaller : ITool
    {
        private static readonly Assembly assembly = Assembly.GetExecutingAssembly();

        private static readonly DirectoryInfo pluginsDirectory = AppInfo.PluginManager.PluginDirectory;

        private static readonly string OTDEnhancedOutputModeDirectoryPath = $"{pluginsDirectory}/OTD.EnhancedOutputMode";
        private readonly DirectoryInfo OTDEnhancedOutputModeDirectory = new(OTDEnhancedOutputModeDirectoryPath);

        private const string dependenciesResourcePath = "Wheel-Addon.Installer.Wheel-Addon.zip";
        private const string group = "Wheel Addon Installer";

        public bool Initialize()
        {
            _ = Task.Run(() => Install(assembly, group, dependenciesResourcePath, OTDEnhancedOutputModeDirectory, ForceInstall));
            return true;
        }

        public bool Install(Assembly assembly, string group, string resourcePath, DirectoryInfo destinationDirectory, bool forceInstall = false)
        {
            var dependencies = assembly.GetManifestResourceStream(resourcePath);

            if (dependencies == null)
            {
                Log.Write($"{group} Installer", "Failed to open embedded dependencies.", LogLevel.Error);
                return false;
            }

            int entriesCount = 0;
            int installed = 0;

            using (ZipArchive archive = new(dependencies, ZipArchiveMode.Read))
            {
                var entries = archive.Entries;
                entriesCount = entries.Count;

                foreach (ZipArchiveEntry entry in entries)
                {
                    FileInfo destinationFile = new($"{destinationDirectory}/{entry.FullName}");

                    if (destinationFile.Exists && !forceInstall)
                        continue;

                    entry.ExtractToFile(destinationFile.FullName, true);
                    installed++;
                }
            }

            // last step: remove OTD.Backport.Parsers.dll in any other plugin directory
            foreach (var pluginDirectory in pluginsDirectory.GetDirectories())
            {
                if (pluginDirectory.Name == "OTD.EnhancedOutputMode")
                    continue;

                var parserDll = new FileInfo($"{pluginDirectory.FullName}/OTD.Backport.Parsers.dll");

                if (parserDll.Exists)
                {
                    Log.Write(group, $"Unable to remove the duplicate dll '{parserDll.FullName}'.", LogLevel.Warning);
                    Log.Write(group, "It is required to remove this dll for this plugin to work.", LogLevel.Warning);
                }
            }

            if (installed > 0)
            {
                string successMessage = $"Successfully installed {installed} of {entriesCount} dependencies.";
                string spacer = new('-', successMessage.Length);
                
                Log.Write($"{group} Installer", spacer, LogLevel.Info);
                Log.Write($"{group} Installer", $"Installed {installed} of {entriesCount} dependencies.", LogLevel.Info);
                Log.Write($"{group} Installer", $"You may need to restart OpenTabletDriver before the plugin can be enabled.", LogLevel.Info);
                Log.Write($"{group} Installer", spacer, LogLevel.Info);
            }

            return true;
        }

        public void Dispose()
        {
            
        }

        [BooleanProperty("Force Install", ""),
         DefaultPropertyValue(false),
         ToolTip("Force install the dependencies even if they are already installed.")]
        public bool ForceInstall { get; set; }
    }
}
