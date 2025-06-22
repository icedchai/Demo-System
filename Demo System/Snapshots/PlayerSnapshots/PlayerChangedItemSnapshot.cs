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
    public class PlayerChangedItemSnapshot : Snapshot, IPlayerSnapshot
    {
        public int Player { get; set; }

        public ItemType ItemType { get; set; }

        public PlayerChangedItemSnapshot()
        {

        }
        public PlayerChangedItemSnapshot(Player player)
        {
            Player = player.Id;

            if (player.CurrentItem is null)
            {
                ItemType = ItemType.None;
            }
            else
            {
                ItemType = player.CurrentItem.Type;
            }

        }

        public override void ReadSnapshot()
        {
            base.ReadSnapshot();

            if (SnapshotReader.Singleton.TryGetPlayer(Player, out Npc npc))
            {
                if (ItemType == ItemType.None)
                {
                    npc.CurrentItem = null;
                    return;
                }

                if (npc.IsInventoryFull)
                {
                    npc.RemoveItem(npc.Items.FirstOrDefault(i => !i.IsArmor));
                }

                npc.CurrentItem = npc.AddItem(ItemType);
            }
        }

        public override void SerializeSpecial(BinaryWriter writer)
        {
            base.SerializeSpecial(writer);
            writer.Write((int)ItemType);
        }

        public override void DeserializeSpecial(BinaryReader reader)
        {
            base.DeserializeSpecial(reader);
            ItemType = (ItemType)reader.ReadInt32();
        }
    }
}
