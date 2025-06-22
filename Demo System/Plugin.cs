using System;
using DemoSystem.EventHandlers;
using DemoSystem.Patches;
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

        public Patcher Patcher { get; set; }

        public static Plugin Singleton { get; set; }

        public SnapshotRecorder Recorder { get; set; }

        public EventHandlers.EventHandler RoundEventHandler { get; set; } = new EventHandlers.EventHandler();

        /// <inheritdoc/>
        public override Version Version => new Version(0, 0, 1);

        public override void OnEnabled()
        {
            base.OnEnabled();
            Singleton = this;

            SnapshotEncoder.RegisterSnapshotTypes();

            Patcher = new Patcher();
            Patcher.Patch();

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
            Patcher.UnpatchAll();
            RoundEventHandler.UnsubscribeEvents();
            RoundEventHandler = null;
        }
    }
}
