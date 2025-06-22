using DemoSystem.Snapshots;

namespace DemoSystem.Snapshots.PlayerSnapshots
{
    using DemoSystem.Extensions;
    using DemoSystem.SnapshotHandlers;
    using DemoSystem.Snapshots.Interfaces;
    using Exiled.API.Features;
    using Exiled.API.Features.Core.Generic;
    using Exiled.API.Features.Toys;
    using System.IO;
    using UnityEngine;

    /// <summary>
    /// Represents a player's position at a point in time, as well as their rotation.
    /// </summary>
    public class PlayerTransformSnapshot : Snapshot, IPlayerSnapshot, IPositionSnapshot, IRotationSnapshot, IScaleSnapshot
    {
        public PlayerTransformSnapshot()
        {

        }

        public PlayerTransformSnapshot(Player player)
        {
            Player = player.Id;
            Scale = player.Scale;
            Position = player.Position;
            Rotation = player.CameraTransform.rotation;
        }

        public int Player { get; set; }

        public Vector3 Position { get; set; }

        public Vector3 Scale { get; set; }

        public Quaternion Rotation { get; set; }

        public override void ReadSnapshot()
        {
            base.ReadSnapshot();

            if (SnapshotReader.Singleton.TryGetPlayer(Player, out Npc npc))
            {
                npc.Position = Position;
                npc.Rotation = Rotation;
                npc.Scale = Scale;
            }
        }
    }
}
