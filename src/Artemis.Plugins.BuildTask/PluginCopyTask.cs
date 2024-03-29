using Microsoft.Build.Framework;
using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using MSBuildTask = Microsoft.Build.Utilities.Task;

namespace Artemis.Plugins.BuildTask
{
    // ReSharper disable once UnusedType.Global
    public class PluginCopyTask : MSBuildTask
    {
        [Required]
        public string SourceDirectory { get; set; }
        
        [Required]
        public string AssemblyName { get; set; }

        public override bool Execute()
        { 
            var sourcePluginJson = Path.Combine(SourceDirectory, "plugin.json");
            
            if (!File.Exists(sourcePluginJson))
            {
                Log.LogError("Error: could not find plugin.json");
                return false;
            }

            var pluginInfo = JSONSerializer<PluginInfo>.DeSerialize(File.ReadAllText(sourcePluginJson));

            if (pluginInfo == null)
            {
                Log.LogError("Error: plugin.json formatted incorrectly");
                return false;
            }

            var entryPoint = $"{AssemblyName}.dll";
            if (pluginInfo.Main != entryPoint)
            {
                Log.LogError("Error: plugin.json main assembly \'{0}\' does not match the assembly  \'{1}\'",pluginInfo.Main, entryPoint);
                return false;
            }

            var pluginsDirectory = GetRootPluginsDirectory();
            if (!Directory.Exists(pluginsDirectory))
            {
                Log.LogError("Destination directory not found \'{0}\'", pluginsDirectory);
                return false;
            }

            var pluginFolderName = GetPluginFolderName(pluginInfo);
            var destinationDirectory = Path.Combine(pluginsDirectory, pluginFolderName);
            if (!Directory.Exists(destinationDirectory))
            {
                Log.LogMessage("Creating plugin destination directory \'{0}\'", destinationDirectory);
                Directory.CreateDirectory(destinationDirectory);
            }
            
            Log.LogMessage("Deleting old plugin files");
            
            foreach (var file in Directory.GetFiles(destinationDirectory))
                File.Delete(file);
            
            Log.LogMessage("Copying plugin to \'{0}\'", destinationDirectory);

            DirectoryCopy(SourceDirectory, destinationDirectory);
            
            Log.LogMessage("Copying plugin.json");
            
            var destinationPluginJson = Path.Combine(destinationDirectory, "plugin.json");
            File.Copy(sourcePluginJson, destinationPluginJson, true);
            File.SetLastWriteTimeUtc(destinationPluginJson, DateTime.UtcNow);
            
            return true;
        }

        private static string GetPluginFolderName(PluginInfo info)
        {
            if (!Guid.TryParse(info.Guid, out var validGuid))
                throw new ArgumentException("Invalid GUID in plugin.json");
            
            var pluginFileName = Path.GetFileNameWithoutExtension(info.Main)
                .Replace("/", "")
                .Replace("\\", "");

            var partialGuid = validGuid.ToString().Substring(0, 8);

            return $"{pluginFileName}-{partialGuid}";
        }
        
        private static string GetRootPluginsDirectory()
        {
            var specialFolder = Environment.OSVersion.Platform switch 
            {
                PlatformID.Win32NT => Environment.SpecialFolder.CommonApplicationData,
                _ => Environment.SpecialFolder.LocalApplicationData
            };
            
            return Path.Combine(Environment.GetFolderPath(specialFolder), "Artemis", "Plugins");
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                // We want to copy the plugin.json last so that the plugin is not loaded before it's fully copied
                if (file.Name == "plugin.json")
                    continue;
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, true);
            }

            foreach (DirectoryInfo subdir in dirs)
            {
                string tempPath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, tempPath);
            }
        }
    }
}