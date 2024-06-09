using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using PluginAPI.Helpers;
using System.IO;

namespace SwiftZombies
{
    public class Plugin
    {
        public static Plugin Instance;

        public static readonly string PluginFolder = Path.Combine(Paths.LocalPlugins.Plugins, Name);

        public const string Author = "SwiftKraft";

        public const string Name = "SwiftZombies";

        public const string Description = "CoD Zombies in SCP: SL. ";

        public const string Version = "Alpha v0.0.1";

        [PluginPriority(LoadPriority.Lowest)]
        [PluginEntryPoint(Name, Version, Description, Author)]
        public void Init()
        {
            Instance = this;

            EventManager.RegisterEvents<EventHandler>(this);

            Log.Info("SwiftZombies Loaded! Version: " + Version);
        }
    }
}
