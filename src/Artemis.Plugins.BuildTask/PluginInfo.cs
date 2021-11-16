using System;

namespace Artemis.Plugins.BuildTask
{
    internal class PluginInfo
    {
        /// <summary>
        ///     The plugins GUID
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        ///     The name of the plugin
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     A short description of the plugin
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets the author of this plugin
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        ///     Gets or sets the website of this plugin or its author
        /// </summary>
        public Uri Website { get; set; }

        /// <summary>
        ///     Gets or sets the repository of this plugin
        /// </summary>
        public Uri Repository { get; set; }

        /// <summary>
        ///     The version of the plugin
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        ///     The main entry DLL, should contain a class implementing Plugin
        /// </summary>
        public string Main { get; set; }

        internal string PreferredPluginDirectory => $"{Main.Split(new string[] { ".dll" }, StringSplitOptions.None)[0].Replace("/", "").Replace("\\", "")}-{Guid.ToString().Substring(0, 8)}";
    }
}
