using Demo_System.Snapshots;
using DemoSystem.SnapshotHandlers;
using DemoSystem.Snapshots;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

namespace DemoSystem.EventHandlers
{
    public class RoundEventHandler
    {
        public void SubscribeEvents()
        {
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
            Exiled.Events.Handlers.Player.Spawned += OnSpawned;
        }
        public void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
            Exiled.Events.Handlers.Player.Spawned -= OnSpawned;
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
    }
}
