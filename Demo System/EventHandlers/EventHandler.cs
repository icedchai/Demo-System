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
using UnityEngine.Assertions.Must;
using Exiled.Events.EventArgs.Map;

namespace DemoSystem.EventHandlers
{
    public class EventHandler
    {
        public void SubscribeEvents()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
            Exiled.Events.Handlers.Player.Spawned += OnSpawned;
            Exiled.Events.Handlers.Player.VoiceChatting += OnVoiceChatting;
            Exiled.Events.Handlers.Player.Verified += OnPlayerVerified;
            Exiled.Events.Handlers.Player.ChangedItem += OnChangedItem;
            Exiled.Events.Handlers.Player.Died += OnDied;
            Exiled.Events.Handlers.Player.Shot += OnShot;

            LabApi.Events.Handlers.PlayerEvents.ToggledNoclip += OnToggledNoclip;
        }

        public void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
            Exiled.Events.Handlers.Player.Spawned -= OnSpawned;
            Exiled.Events.Handlers.Player.VoiceChatting -= OnVoiceChatting;
            Exiled.Events.Handlers.Player.Verified -= OnPlayerVerified;
            Exiled.Events.Handlers.Player.ChangedItem -= OnChangedItem;
            Exiled.Events.Handlers.Player.Died -= OnDied;
            Exiled.Events.Handlers.Player.Shot -= OnShot;

            LabApi.Events.Handlers.PlayerEvents.ToggledNoclip -= OnToggledNoclip;
        }

        private void OnItemSpawned(PickupAddedEventArgs e)
        {

        }

        private void OnWaitingForPlayers()
        {
            if (Plugin.Singleton.Recorder is not null && Plugin.Singleton.Recorder.IsRecording)
            {
                Plugin.Singleton.Recorder.StopRecording();

            }

            Plugin.Singleton.Recorder = new SnapshotRecorder();

            if (Plugin.Singleton.Config.AutoRecordAtRoundStart)
            {
                Plugin.Singleton.Recorder.StartRecording();
            }
        }

        private void OnShot(ShotEventArgs e)
        {
            if (!Plugin.Singleton.Recorder.IsRecording)
            {
                return;
            }

            Plugin.Singleton.Recorder.QueuedSnapshots.Enqueue(new PlayerShotFirearmSnapshot(e.Player));
        }

        private void OnSpawned(SpawnedEventArgs e)
        {
            if (e.Player is null || e.Player.IsDead || e.Player.Role is null || !Plugin.Singleton.Recorder.IsRecording)
            {
                return;
            }

            Plugin.Singleton.Recorder.QueuedSnapshots.Enqueue(new PlayerSpawnedSnapshot(e.Player.Id, e.Player.Role.Type, e.Reason, e.SpawnFlags));
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
