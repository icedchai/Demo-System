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
    /// Represents a player's position, rotation, and scale at a point in time.
    /// </summary>
    public class PlayerTransformSnapshot : Snapshot, IPlayerSnapshot, IPlayerTransformSnapshot
    {
        public PlayerTransformSnapshot()
        {

        }

        public PlayerTransformSnapshot(Player player, TransformDifference transformDifference = TransformDifference.All)
        {
            Player = player.Id;
            TransformDifference = transformDifference;

            if (transformDifference.HasFlag(TransformDifference.Position))
            {
                Position = player.Position;
            }

            if (transformDifference.HasFlag(TransformDifference.Rotation))
            {
                Rotation = GetLookRot(player.ReferenceHub);
            }

            if (transformDifference.HasFlag(TransformDifference.Scale))
            {
                Scale = player.Scale;
            }
        }

        public int Player { get; set; }

        public Vector3 Position { get; set; }

        public Vector3 Scale { get; set; }

        public Vector2 Rotation { get; set; }

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
                    npc.ReferenceHub.TryOverrideRotation(Rotation);
                }
                if (TransformDifference.HasFlag(TransformDifference.Scale))
                {
                    npc.Scale = Scale;
                }
            }
        }

        public static Vector2 GetLookRot(ReferenceHub hub)
        {
            if (hub.roleManager.CurrentRole is not IFpcRole fpcRole)
                return Vector2.zero;

            FpcMouseLook mouseLook = fpcRole.FpcModule.MouseLook;
            return new Vector2(mouseLook.CurrentVertical, mouseLook.CurrentHorizontal);
        }

        void SetPosition(IFpcRole role, Vector3 dir, float distance)
        {
            Vector3 vector = role.FpcModule.Hub.PlayerCameraReference.TransformDirection(dir).NormalizeIgnoreY();
            role.FpcModule.Motor.ReceivedPosition = new RelativePosition(Position + vector * distance);
        }
    }
}
