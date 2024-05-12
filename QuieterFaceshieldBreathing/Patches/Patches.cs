using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;

namespace QuieterFaceshieldBreathing.Patches
{
    internal class PlayerCheckMuffledState : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.UpdateOcclusion));
        }

        [PatchPrefix]
        private static bool PatchPrefix(Player __instance, ref bool ___OcclusionDirty, ref bool ___Muffled)
        {
            if (__instance != Singleton<GameWorld>.Instance.MainPlayer)
            {
                return true;
            }

            // We only care about if the audio is muffled
            if (!___Muffled)
            {
                QFSBPlugin.ShouldChangeVolume = false;
                return true;
            }

            if (___OcclusionDirty && MonoBehaviourSingleton<BetterAudio>.Instantiated)
            {
                ___OcclusionDirty = false;
                AudioMixerGroup audioMixerGroup = Singleton<BetterAudio>.Instance.SelfSpeechReverb;

                if (__instance.SpeechSource != null)
                {
                    __instance.SpeechSource.SetMixerGroup(audioMixerGroup);
                    QFSBPlugin.ShouldChangeVolume = true;
                }
            }

            return false;
        }
    }

    internal class SimpleSourcePlay : ModulePatch
    {
        private const string NORMAL_BREATHING = "breath_ok";
        private static readonly FieldInfo _playerSpeechSource = AccessTools.Field(typeof(Player), "_speechSource");

        private static float _originalSourceVolume = -1f;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(SimpleSource), nameof(SimpleSource.Play));
        }

        [PatchPrefix]
        private static void PatchPrefix(
            SimpleSource __instance,
            ref float ___SourcePlayingVolume,
            AudioClip clip1,
            bool oneShot,
            ref float volume
        )
        {
            Player mainPlayer = Singleton<GameWorld>.Instance.MainPlayer;
            if (mainPlayer == null)
            {
                return;
            }

            if (__instance != _playerSpeechSource.GetValue(mainPlayer) as SimpleSource)
            {
                return;
            }

            if (oneShot || clip1 == null || !clip1.name.Contains(NORMAL_BREATHING) || !QFSBPlugin.ShouldChangeVolume)
            {
                ResetVolume(__instance, ref ___SourcePlayingVolume);
                return;
            }

            _originalSourceVolume = __instance.source1.volume;
            volume *= QFSBPlugin.Volume.Value * 0.01f;
#if DEBUG
            QFSBPlugin.LogSource.LogInfo($"Current speech AudioClip: {clip1.name}, volume: {volume}");
#endif
        }

        private static void ResetVolume(SimpleSource source, ref float sourcePlayingVolume)
        {
            if (_originalSourceVolume >= 0)
            {
                sourcePlayingVolume = _originalSourceVolume / source.OcclusionVolumeFactor;
                source.UpdateSourceVolume(1f);
                _originalSourceVolume = -1f;
            }
        }
    }
}
