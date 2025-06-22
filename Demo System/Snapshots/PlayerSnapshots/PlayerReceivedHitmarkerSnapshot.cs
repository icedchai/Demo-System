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
            writer.Write(Size);
            writer.Write(PlayAudio);
        }

        public override void DeserializeSpecial(BinaryReader reader)
        {
            base.DeserializeSpecial(reader);
            Size = reader.ReadInt32();
            PlayAudio = reader.ReadBoolean();
        }

        public override void ReadSnapshot()
        {
            base.ReadSnapshot();
            if (SnapshotReader.Singleton.TryGetPlayer(Player, out Npc npc))
            {
                Hitmarker.SendHitmarkerDirectly(npc.ReferenceHub, Size, PlayAudio);
            }
        }

        public int Player { get; set; }

        public float Size { get; set; }

        public bool PlayAudio { get; set; }
    }
}
