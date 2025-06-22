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
    public class PlayerHealthSnapshot : Snapshot, IPlayerSnapshot
    {
        public PlayerHealthSnapshot()
        {

        }

        public PlayerHealthSnapshot(Player player)
        {
            Player = player.Id;
            Health = player.Health;
            Artificial = player.ArtificialHealth;
            Hume = player.HumeShield;
        }

        public override void ReadSnapshot()
        {
            base.ReadSnapshot();
            if (SnapshotReader.Singleton.TryGetPlayer(Player, out Npc npc))
            {
                npc.Health = Health;
                npc.ArtificialHealth = Artificial;
                npc.HumeShield = Hume;
            }
        }

        public override void DeserializeSpecial(BinaryReader reader)
        {
            base.DeserializeSpecial(reader);
            Health = reader.ReadSingle();
            Artificial = reader.ReadSingle();
            Hume = reader.ReadSingle();
        }

        public override void SerializeSpecial(BinaryWriter writer)
        {
            base.SerializeSpecial(writer);
            writer.Write(Health);
            writer.Write(Artificial);
            writer.Write(Hume);
        }

        public int Player { get; set; }

        public float Health { get; set; }

        public float Artificial { get; set; }

        public float Hume { get; set; }
    }
}
