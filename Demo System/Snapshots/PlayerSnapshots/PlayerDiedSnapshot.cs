using DemoSystem.SnapshotHandlers;
using DemoSystem.Snapshots.Interfaces;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.DamageHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoSystem.Snapshots.PlayerSnapshots
{
    public class PlayerDiedSnapshot : Snapshot, IPlayerSnapshot
    {
        public PlayerDiedSnapshot()
        {

        }

        public PlayerDiedSnapshot(DamageHandler handler)
        {
            Player = handler.Target.Id;
            DamageType = handler.Type;
        }

        public override void ReadSnapshot()
        {
            base.ReadSnapshot();

            if (SnapshotReader.Singleton.TryGetPlayer(Player, out Npc npc))
            {
                npc.ReferenceHub.characterClassManager.GodMode = false;
                npc.Kill(DamageType);
            }
        }

        public override void SerializeSpecial(BinaryWriter writer)
        {
            base.SerializeSpecial(writer);
            writer.Write((int)DamageType);
        }

        public override void DeserializeSpecial(BinaryReader reader)
        {
            base.DeserializeSpecial(reader);
            DamageType = (DamageType)reader.ReadInt32();
        }

        public int Player { get; set; }

        public DamageType DamageType { get; set; }
    }
}
