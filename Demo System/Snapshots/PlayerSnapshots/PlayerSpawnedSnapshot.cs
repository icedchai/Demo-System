using DemoSystem.Snapshots.Interfaces;
using DemoSystem.Extensions;
using DemoSystem.SnapshotHandlers;
using DemoSystem.Snapshots;
using DemoSystem.Snapshots.Interfaces;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using MEC;

namespace DemoSystem.Snapshots.PlayerSnapshots
{
    /// <summary>
    /// Snapshot representing a change in player role.
    /// </summary>
    public class PlayerSpawnedSnapshot : Snapshot, IPlayerSnapshot, IRoleSnapshot
    {
        public int Player { get; set; }

        public RoleTypeId Role { get; set; }

        public SpawnReason SpawnReason { get; set; }

        public PlayerSpawnedSnapshot() { }

        public PlayerSpawnedSnapshot(int player, RoleTypeId role, SpawnReason spawnReason, RoleSpawnFlags spawnFlags)
        {
            Player = player;
            Role = role;
            SpawnReason = spawnReason;
        }

        public override void SerializeSpecial(BinaryWriter writer)
        {
            base.SerializeSpecial(writer);
            writer.Write((byte)SpawnReason);
        }

        public override void DeserializeSpecial(BinaryReader reader)
        {
            base.DeserializeSpecial(reader);
            SpawnReason = (SpawnReason)reader.ReadByte();
        }

        public override void ReadSnapshot()
        {
            base.ReadSnapshot();

            if (SnapshotReader.Singleton.TryGetPlayerActor(Player, out Npc npc))
            {
                npc.Role.Set(Role, SpawnReason, RoleSpawnFlags.None);
                Timing.CallDelayed(1f, () => npc.ReferenceHub.characterClassManager.GodMode = true);
            }
        }
    }
}
