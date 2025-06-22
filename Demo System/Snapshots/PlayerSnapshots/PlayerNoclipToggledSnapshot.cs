using DemoSystem.SnapshotHandlers;
using DemoSystem.Snapshots;
using DemoSystem.Snapshots.Interfaces;
using Exiled.API.Features;
using PlayerRoles.FirstPersonControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DemoSystem.Snapshots.PlayerSnapshots
{
    public class PlayerNoclipToggledSnapshot : Snapshot, IPlayerSnapshot
    {
        public PlayerNoclipToggledSnapshot()
        {

        }

        public PlayerNoclipToggledSnapshot(Player player, bool enabled)
        {
            Player = player.Id;
            Enabled = enabled;
        }

        public override void ReadSnapshot()
        {
            base.ReadSnapshot();
            if (SnapshotReader.Singleton.TryGetPlayer(Player, out Npc npc) && npc.Role.Base is IFpcRole fpcRole)
            {
                if (Enabled)
                {
                    fpcRole.FpcModule.Noclip.IsActive = true;
                    fpcRole.FpcModule.Noclip.UpdateNoclip();
                }
                else
                {
                    fpcRole.FpcModule.Noclip.IsActive = false;
                    fpcRole.FpcModule.Noclip.UpdateNoclip();
                }
            }
        }

        public override void SerializeSpecial(BinaryWriter writer)
        {
            base.SerializeSpecial(writer);
            writer.Write(Enabled);
        }

        public override void DeserializeSpecial(BinaryReader reader)
        {
            base.DeserializeSpecial(reader);
            Enabled = reader.ReadBoolean();
        }

        public int Player { get; set; }

        public bool Enabled { get; set; }
    }
}
