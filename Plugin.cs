using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[MycoMod(null, ModFlags.IsClientSide)]
public class SkinRandomizerPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.skinrandomizer";
    public const string PluginName = "SkinRandomizer";
    public const string PluginVersion = "1.1.0";


    internal static new ManualLogSource Logger;
    internal static ConfigEntry<bool> enabled;

    private FileSystemWatcher _configWatcher;
    private DateTime _lastReloadTime = DateTime.MinValue;
    private const int ReloadDebounceMs = 500;

    private void Awake()
    {
        Logger = base.Logger;

        enabled = Config.Bind(
            "General",
            "Enabled",
            true,
            "If true, randomly equips favorite skins on mission start.");

        SetupConfigHotReload();

        try
        {
            var harmony = new Harmony(PluginGUID);
            harmony.PatchAll(typeof(DropPodPatches));
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error applying patches: {ex.Message}");
        }

        Logger.LogInfo($"{PluginName} loaded successfully.");
    }

    private void SetupConfigHotReload()
    {
        try
        {
            var configPath = Config.ConfigFilePath;
            var dir = Path.GetDirectoryName(configPath);
            var file = Path.GetFileName(configPath);

            if (string.IsNullOrEmpty(dir) || string.IsNullOrEmpty(file))
            {
                Logger.LogWarning("Could not set up config hot-reload: invalid config path.");
                return;
            }

            _configWatcher = new FileSystemWatcher(dir, file)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
            };

            _configWatcher.Changed += OnConfigFileChanged;
            _configWatcher.EnableRaisingEvents = true;

            Logger.LogInfo("Config hot-reload enabled.");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to set up config hot-reload: {ex.Message}");
        }
    }

    private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        var now = DateTime.UtcNow;
        if ((now - _lastReloadTime).TotalMilliseconds < ReloadDebounceMs)
            return;

        _lastReloadTime = now;

        try
        {
            Config.Reload();
            Logger.LogInfo($"Config reloaded. Enabled = {enabled.Value}");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to reload config: {ex.Message}");
        }
    }

    private void OnDestroy()
    {
        if (_configWatcher == null)
            return;

        _configWatcher.Changed -= OnConfigFileChanged;
        _configWatcher.EnableRaisingEvents = false;
        _configWatcher.Dispose();
        _configWatcher = null;
    }
}
