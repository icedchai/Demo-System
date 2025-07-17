using DemoSystem.Snapshots;

namespace DemoSystem.Snapshots.PlayerSnapshots
{
    using DemoSystem.Extensions;
    using DemoSystem.SnapshotHandlers;
    using DemoSystem.Snapshots.Enums;
    using DemoSystem.Snapshots.Interfaces;
    using Exiled.API.Features;
    using Exiled.API.Features.Core.Generic;
    using Exiled.API.Features.Toys;
    using PlayerRoles.FirstPersonControl;
    using RelativePositioning;
    using System.IO;
    using UnityEngine;

    /// <summary>
    /// Represents a player's position at a point in time, as well as their rotation.
    /// </summary>
    public class PlayerTransformSnapshot : Snapshot, IPlayerSnapshot, ITransformSnapshot
    {
        public PlayerTransformSnapshot()
        {

        }

        public PlayerTransformSnapshot(Player player, TransformDifference transformDifference)
        {
            Player = player.Id;
            TransformDifference = transformDifference;

            if (transformDifference.HasFlag(TransformDifference.Position))
            {
                Position = player.Position;
            }
            if (transformDifference.HasFlag(TransformDifference.Rotation))
            {
                Rotation = player.CameraTransform.rotation;
            }
            if (transformDifference.HasFlag(TransformDifference.Scale))
            {
                Scale = player.Scale;
            }
        }

        public int Player { get; set; }

        public Vector3 Position { get; set; }

        public Vector3 Scale { get; set; }

        public Quaternion Rotation { get; set; }

        public TransformDifference TransformDifference { get; set; }

        public override void ReadSnapshot()
        {
            base.ReadSnapshot();

            if (SnapshotReader.Singleton.TryGetPlayerActor(Player, out Npc npc))
            {
                if (TransformDifference.HasFlag(TransformDifference.Position))
                {
                    npc.Position = Position;
                }
                if (TransformDifference.HasFlag(TransformDifference.Rotation))
                {
                    npc.Rotation = Rotation;
                }
                if (TransformDifference.HasFlag(TransformDifference.Scale))
                {
                    npc.Scale = Scale;
                }
            }
        }
        void SetPosition(IFpcRole role, Vector3 dir, float distance)
        {
            Vector3 vector = role.FpcModule.Hub.PlayerCameraReference.TransformDirection(dir).NormalizeIgnoreY();
            role.FpcModule.Motor.ReceivedPosition = new RelativePosition(Position + vector * distance);
        }
    }
}
