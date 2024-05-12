#pragma warning disable S101, S2223, S2696

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using QuieterFaceshieldBreathing.Patches;

namespace QuieterFaceshieldBreathing
{

    [BepInPlugin("com.Arys.QuieterFaceshieldBreathing", "Quieter Faceshield Breathing", "1.0.0")]
    public class QFSBPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource LogSource;
        internal static ConfigEntry<int> Volume;

        internal static bool ShouldChangeVolume { get; set; } = false;

        private void Awake()
        {
            LogSource = Logger;
            Volume = Config.Bind("", "Volume", 100, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));

            new PlayerCheckMuffledState().Enable();
            new SimpleSourcePlay().Enable();
        }
    }
}
