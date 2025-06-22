using System;
using DemoSystem.EventHandlers;
using DemoSystem.SnapshotHandlers;
using DemoSystem.Snapshots;
using Exiled.API.Features;
using Exiled.API.Features.Core.Generic;

namespace DemoSystem
{
    public class Plugin : Plugin<Config>
    {
        /// <inheritdoc/>
        public override string Name => "Demo System";

        /// <inheritdoc/>
        public override string Author => "icedchqi";

        /// <inheritdoc/>
        public override string Prefix => "demos";

        public static Plugin Singleton { get; set; }

        public SnapshotRecorder Recorder { get; set; }

        public EventHandlers.EventHandler RoundEventHandler { get; set; } = new EventHandlers.EventHandler();

        /// <inheritdoc/>
        public override Version Version => new Version(0, 0, 1);

        public override void OnEnabled()
        {
            base.OnEnabled();
            Singleton = this;

            foreach (Type type in Assembly.GetTypes().Where(t => t != typeof(Snapshot) && typeof(Snapshot).IsAssignableFrom(t)))
            {
                SnapshotEncoder.RegisterSnapshotType(type);
            }

            foreach (var kvp in SnapshotEncoder.IdToSnapshotType)
            {
                Log.Info($"{kvp.Key} : {kvp.Value.Name}");
            }

            RoundEventHandler = new EventHandlers.EventHandler();
            RoundEventHandler.SubscribeEvents();
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            RoundEventHandler.UnsubscribeEvents();
            RoundEventHandler = null;
        }
    }
}
