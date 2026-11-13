using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;

namespace StopZombieExpluatation
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class MainPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.sondju.StopZombieExpluatation";
        public const string PluginName = "Stop Zombie Expluatation";
        public const string PluginVersion = "1.0.0";

        public static MainPlugin Instance { get; private set; }
        public static ManualLogSource Log;

        // Делаем конфиги публичными свойствами, чтобы к ним легко обращались файлы патчей
        public ConfigEntry<bool> EnableMod { get; private set; }

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            // 1. Объявление конфигов
            EnableMod = Config.Bind(
                "General", "EnableMod", true,
                "Enable or disable the mod completely."
            );

            // 2. Автоматически ищет и применяет все патчи из ВСЕХ создаваемых нами файлов!
            var harmony = new Harmony(PluginGuid);

            var patchTypes = new[]
            {
                typeof(ForceAutoCraftPatch),
            };

            foreach (var pt in patchTypes)
            {
                try
                {
                    harmony.CreateClassProcessor(pt).Patch();
                    Logger.LogMessage($"{pt.Name} correct patched");
                }
                catch (Exception e)
                {
                    Logger.LogError(
                        $"{pt.Name} NOT patched: {e.GetType().Name}: {e.Message}\n" +
                        $"Stack trace: {e.StackTrace}"
                    );
                }
            }
        }
    }
}