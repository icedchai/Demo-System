using DemoSystem.Snapshots.PlayerSnapshots;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoSystem.Patches.RecordHelperPatches
{
    [HarmonyPatch(typeof(Hitmarker), nameof(Hitmarker.SendHitmarkerDirectly), new Type[] { typeof(ReferenceHub), typeof(float), typeof(bool) })]
    public static class HitmarkRecorderPatch
    {
        public static void Postfix(ReferenceHub hub, float size, bool playAudio)
        {
            Plugin.Singleton.Recorder.QueuedSnapshots.Enqueue(new PlayerReceivedHitmarkerSnapshot() { Player = hub.PlayerId, Size = size, PlayAudio = playAudio });
        }
    }
}
