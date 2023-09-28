using System;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace SunkenCompass
{
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    public class SunkenCompassPlugin : BaseUnityPlugin

    {
        internal const string ModName = "SunkenCompass";
        internal const string ModVersion = "1.0.1";
        internal const string Author = "Azumatt";
        private const string ModGuid = Author + "." + ModName;
        private static string ConfigFileName = ModGuid + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        private readonly Harmony _harmony = new(ModGuid);
        public static readonly ManualLogSource SunkenCompassLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);
        internal static Sprite CompassSprite = null!;
        internal static Sprite CompassCenter = null!;
        internal static Sprite CompassMask = null!;
        internal static GameObject ObjectCompass = null!;
        internal static GameObject ObjectParent = null!;
        internal static GameObject ObjectCenterMark = null!;
        internal static bool GotCompassImage;
        internal static bool GotCompassMask;
        internal static bool GotCompassCenter;


        private void Awake()
        {
            ConfigEnabled = Config.Bind("1 - Sunken Compass", "Enabled", Toggle.On, "Enable or disable the Sunken Compass.");
            CompassUsePlayerDirection = Config.Bind("2 - Compass Display", "Use Player Direction", Toggle.Off, "Orient the compass based on the direction the player is facing, rather than the middle of the screen.");
            CompassScale = Config.Bind("2 - Compass Display", "Scale (Compass)", 0.75f, "Enlarge or shrink the scale of the compass.");
            CompassYOffset = Config.Bind("2 - Compass Display", "Offset (Y)", 0, "Offset from the top of the screen in pixels.");
            CompassShowCenterMark = Config.Bind("2 - Compass Display", "Show Center Mark", Toggle.Off, "Show center mark graphic.");
            ColorCompass = Config.Bind("3 - Color Adjustment", "Color (Compass)", Color.white, "Adjust the color of the compass.");
            ColorCenterMark = Config.Bind("3 - Color Adjustment", "Color (Center Mark)", Color.yellow, "Adjust the color of the center mark graphic.");
            if (ConfigEnabled.Value == Toggle.On)
            {
                CompassSprite = Utilities.LoadSprite("compass.png");
                CompassCenter = Utilities.LoadSprite("center.png");
                CompassMask = Utilities.LoadSprite("mask.png");

                GotCompassImage = CompassSprite != null;
                GotCompassMask = CompassMask != null;
                GotCompassCenter = CompassCenter != null;


                _harmony.PatchAll();
                SetupWatcher();
            }
            else
                SunkenCompassLogger.LogInfo($"{ModName} v[{ModVersion}] not enabled in configuration.");
        }

        private void OnDestroy()
        {
            Config.Save();
        }

        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                SunkenCompassLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                SunkenCompassLogger.LogError($"There was an issue loading your {ConfigFileName}");
                SunkenCompassLogger.LogError("Please check your config entries for spelling and format!");
            }
        }

        #region ConfigOptions

        internal static ConfigEntry<Toggle> ConfigEnabled = null!;
        internal static ConfigEntry<Color> ColorCompass = null!;
        internal static ConfigEntry<Color> ColorCenterMark = null!;
        internal static ConfigEntry<Toggle> CompassUsePlayerDirection = null!;
        internal static ConfigEntry<int> CompassYOffset = null!;
        internal static ConfigEntry<float> CompassScale = null!;
        internal static ConfigEntry<Toggle> CompassShowCenterMark = null!;

        public enum Toggle
        {
            Off,
            On
        }

        private class ConfigurationManagerAttributes
        {
            [UsedImplicitly] public int? Order;
            [UsedImplicitly] public bool? Browsable;
            [UsedImplicitly] public string? Category;
            [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer;
        }

        #endregion
    }
}