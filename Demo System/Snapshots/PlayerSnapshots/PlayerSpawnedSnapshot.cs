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

namespace DemoSystem.Snapshots.PlayerSnapshots
{
    public class PlayerSpawnedSnapshot : Snapshot, IPlayerSnapshot, IRoleSnapshot
    {
        public int Player { get; set; }

        public RoleTypeId Role { get; set; }

        public SpawnReason SpawnReason { get; set; }

        public RoleSpawnFlags SpawnFlags { get; set; }

        public override void SerializeSpecial(BinaryWriter writer)
        {
            base.SerializeSpecial(writer);
            writer.Write((byte)SpawnReason);
            writer.Write((int)SpawnFlags);
        }

        public override void DeserializeSpecial(BinaryReader reader)
        {
            base.DeserializeSpecial(reader);
            SpawnReason = (SpawnReason)reader.ReadByte();
            SpawnFlags = (RoleSpawnFlags)reader.ReadInt32();
        }

        public override void ReadSnapshot()
        {
            base.ReadSnapshot();

            if (SnapshotReader.Singleton.TryGetPlayer(Player, out Npc npc))
            {
                npc.Role.Set(Role, SpawnReason, SpawnFlags);
                npc.ReferenceHub.characterClassManager.GodMode = true;
            }
        }
    }
}
