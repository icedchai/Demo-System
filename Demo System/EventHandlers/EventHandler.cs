using DemoSystem.Snapshots;
using DemoSystem.Snapshots.PlayerSnapshots;
using DemoSystem.SnapshotHandlers;
using DemoSystem.Snapshots;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using LabApi.Events.Arguments.PlayerEvents;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using Mirror;

namespace DemoSystem.EventHandlers
{
    public class EventHandler
    {
        public void SubscribeEvents()
        {
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
            Exiled.Events.Handlers.Player.Spawned += OnSpawned;
            Exiled.Events.Handlers.Player.VoiceChatting += OnVoiceChatting;
            Exiled.Events.Handlers.Player.Verified += OnPlayerVerified;
            Exiled.Events.Handlers.Player.ChangedItem += OnChangedItem;
            Exiled.Events.Handlers.Player.Died += OnDied;

            LabApi.Events.Handlers.PlayerEvents.ToggledNoclip += OnToggledNoclip;
        }

        public void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
            Exiled.Events.Handlers.Player.Spawned -= OnSpawned;
            Exiled.Events.Handlers.Player.VoiceChatting -= OnVoiceChatting;
            Exiled.Events.Handlers.Player.Verified -= OnPlayerVerified;
            Exiled.Events.Handlers.Player.ChangedItem -= OnChangedItem;
            Exiled.Events.Handlers.Player.Died -= OnDied;

            LabApi.Events.Handlers.PlayerEvents.ToggledNoclip -= OnToggledNoclip;
        }

        private void OnRoundStarted()
        {
            Plugin.Singleton.Recorder = new SnapshotRecorder();
        }

        private void OnRoundEnded(RoundEndedEventArgs e)
        {
            Plugin.Singleton.Recorder.Dispose();
        }

        private void OnSpawned(SpawnedEventArgs e)
        {
            if (!Plugin.Singleton.Recorder.IsRecording)
            {
                return;
            }

            Plugin.Singleton.Recorder.QueuedSnapshots.Enqueue(new PlayerSpawnedSnapshot() { Player = e.Player.Id, Role = e.Player.Role.Type, SpawnReason = e.Reason, SpawnFlags = e.SpawnFlags });
        }

        private void OnVoiceChatting(VoiceChattingEventArgs e)
        {
            if (!Plugin.Singleton.Config.RecordVoiceChat || !Plugin.Singleton.Recorder.IsRecording)
            {
                return;
            }

            Plugin.Singleton.Recorder.QueuedSnapshots.Enqueue(new PlayerVoiceChatSnapshot(e.VoiceMessage));
        }

        private void OnPlayerVerified(VerifiedEventArgs e)
        {
            if (!Plugin.Singleton.Recorder.IsRecording)
            {
                return;
            }

            Plugin.Singleton.Recorder.QueuedSnapshots.Enqueue(new PlayerVerifiedSnapshot(e.Player));
        }

        private void OnToggledNoclip(PlayerToggledNoclipEventArgs e)
        {
            if (!Plugin.Singleton.Recorder.IsRecording)
            {
                return;
            }

            Plugin.Singleton.Recorder.QueuedSnapshots.Enqueue(new PlayerNoclipToggledSnapshot(e.Player, e.IsNoclipping));
        }

        private void OnChangedItem(ChangedItemEventArgs e)
        {
            if (!Plugin.Singleton.Recorder.IsRecording)
            {
                return;
            }

            Plugin.Singleton.Recorder.QueuedSnapshots.Enqueue(new PlayerChangedItemSnapshot(e.Player));
        }

        private void OnDied(DiedEventArgs e)
        {
            if (!Plugin.Singleton.Recorder.IsRecording)
            {
                return;
            }

            Plugin.Singleton.Recorder.QueuedSnapshots.Enqueue(new PlayerDiedSnapshot(e.DamageHandler));
        }
    }
}
