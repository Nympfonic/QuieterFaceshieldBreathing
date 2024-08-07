﻿using SPT.Reflection.Patching;
using Comfort.Common;
using EFT;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;

namespace QuieterFaceshieldBreathing
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
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(SimpleSource), nameof(SimpleSource.Play));
        }

        [PatchPrefix]
        private static void PatchPrefix(
            SimpleSource __instance,
            AudioGroupPreset ___Preset,
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

            if (__instance != QFSBPlugin.PlayerSpeechSource.GetValue(mainPlayer) as SimpleSource)
            {
                return;
            }

            if (oneShot || clip1 == null || !clip1.name.Contains(QFSBPlugin.NORMAL_BREATHING) || !QFSBPlugin.ShouldChangeVolume)
            {
                QFSBPlugin.ResetSourceVolume(__instance, ___Preset, ref ___SourcePlayingVolume);
                return;
            }

            QFSBPlugin.OriginalSourceVolume = __instance.source1.volume;
            volume *= QFSBPlugin.Volume.Value * 0.01f;
#if DEBUG
            QFSBPlugin.LogSource.LogInfo($"Current speech AudioClip: {clip1.name}, volume: {volume}");
#endif
        }
    }
}
