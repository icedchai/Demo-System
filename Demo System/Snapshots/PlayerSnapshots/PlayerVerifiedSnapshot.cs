using DemoSystem;
using DemoSystem.SnapshotHandlers;
using DemoSystem.Snapshots;
using DemoSystem.Snapshots.Interfaces;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoSystem.Snapshots.PlayerSnapshots
{
    public class PlayerVerifiedSnapshot : Snapshot, IPlayerSnapshot
    {
        public PlayerVerifiedSnapshot()
        {

        }

        public PlayerVerifiedSnapshot(Player player)
        {
            Player = player.Id;
            Nickname = player.Nickname;
        }
        public int Player { get; set; }

        public string Nickname { get; set; }

        public override void DeserializeSpecial(BinaryReader reader)
        {
            base.DeserializeSpecial(reader);
            Nickname = reader.ReadString();
        }

        public override void SerializeSpecial(BinaryWriter writer)
        {
            base.SerializeSpecial(writer);
            writer.Write(Nickname);
        }

        public override void ReadSnapshot()
        {
            base.ReadSnapshot();
            SnapshotReader.Singleton.SpawnPlayer(Player, Nickname);
        }
    }
}
