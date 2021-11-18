using Microsoft.Build.Framework;
using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using MSBuildTask = Microsoft.Build.Utilities.Task;

namespace Artemis.Plugins.BuildTask
{
    public class PluginCopyTask : MSBuildTask
    {
        [Required]
        public string PluginJson { get; set; }

        [Required]
        public string SourceDirectory { get; set; }

        private string DestinationDirectory { get; set; }

        public override bool Execute()
        {
            if (!File.Exists(PluginJson))
            {
                Log.LogError("Error: could not find plugin.json");

                return false;
            }

            var pluginInfo = JSONSerializer<PluginInfo>.DeSerialize(File.ReadAllText(PluginJson));

            if (pluginInfo == null)
            {
                Log.LogError("Error: plugin.json formatted incorrectly");

                return false;
            }

            DestinationDirectory = GetDestinationDirectory(pluginInfo);

            if (!Directory.Exists(DestinationDirectory))
            {
                Log.LogMessage("Creating plugin destination directory \'{0}\'", DestinationDirectory);

                Directory.CreateDirectory(DestinationDirectory);
            }

            DirectoryCopy(SourceDirectory, DestinationDirectory);

            //copy over stuff
            return true;
        }

        private static string GetDestinationDirectory(PluginInfo info)
        {
            if (!Guid.TryParse(info.Guid, out var validGuid))
                throw new ArgumentException("Invalid GUID in plugin.json");

            var preferredPluginDirectory = $"{info.Main.Split(new string[] { ".dll" }, StringSplitOptions.None)[0].Replace("/", "").Replace("\\", "")}-{validGuid.ToString().Substring(0, 8)}";
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "Artemis",
                    "Plugins",
                    preferredPluginDirectory);
            }
            else
            {
                //linux (and mac?)
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Artemis",
                    "Plugins",
                    preferredPluginDirectory);
            }
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