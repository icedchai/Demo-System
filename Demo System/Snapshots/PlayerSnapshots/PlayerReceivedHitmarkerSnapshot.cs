using DemoSystem.SnapshotHandlers;
using DemoSystem.Snapshots.Interfaces;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoSystem.Snapshots.PlayerSnapshots
{
    public class PlayerReceivedHitmarkerSnapshot : Snapshot, IPlayerSnapshot
    {
        public override void SerializeSpecial(BinaryWriter writer)
        {
            base.SerializeSpecial(writer);
        }

        public override void DeserializeSpecial(BinaryReader reader)
        {
            base.DeserializeSpecial(reader);
        }

        public override void ReadSnapshot()
        {
            base.ReadSnapshot();
            if (SnapshotReader.Singleton.TryGetPlayerActor(Player, out Npc npc))
            {
                Hitmarker.SendHitmarkerDirectly(npc.ReferenceHub, 0.1f);
            }
        }

        public int Player { get; set; }
    }
}
