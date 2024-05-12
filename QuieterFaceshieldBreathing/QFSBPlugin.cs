#pragma warning disable S101, S2223, S2696

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using EFT;
using HarmonyLib;
using System.Reflection;

namespace QuieterFaceshieldBreathing
{

    [BepInPlugin("com.Arys.QuieterFaceshieldBreathing", "Quieter Faceshield Breathing", "1.0.0")]
    public class QFSBPlugin : BaseUnityPlugin
    {
        internal const string NORMAL_BREATHING = "breath_ok";

        internal static readonly FieldInfo PlayerSpeechSource = AccessTools.Field(typeof(Player), "_speechSource");

        internal static ManualLogSource LogSource;
        internal static ConfigEntry<int> Volume;

        internal static float OriginalSourceVolume { get; set; } = -1f;
        internal static bool ShouldChangeVolume { get; set; } = false;

        internal static void ResetSourceVolume(SimpleSource source, AudioGroupPreset preset, ref float sourcePlayingVolume)
        {
            if (OriginalSourceVolume >= 0)
            {
                sourcePlayingVolume = OriginalSourceVolume * preset.OverallVolume;
                source.UpdateSourceVolume(1f);
                OriginalSourceVolume = -1f;
            }
        }

        private void Awake()
        {
            LogSource = Logger;
            Volume = Config.Bind("", "Volume", 100, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));

            new PlayerCheckMuffledState().Enable();
            new SimpleSourcePlay().Enable();
        }
    }
}
