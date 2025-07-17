using DemoSystem;
using DemoSystem.SnapshotHandlers;
using DemoSystem.Snapshots;
using DemoSystem.Snapshots.Interfaces;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Attachments.Components;
using InventorySystem.Items.Jailbird;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoSystem.Snapshots.PlayerSnapshots
{
    public class PlayerChangedItemSnapshot : Snapshot, IPlayerSnapshot, IItemSnapshot
    {
        public int Player { get; set; }

        public ushort Item { get; set; }

        public PlayerChangedItemSnapshot()
        {

        }
        public PlayerChangedItemSnapshot(Player player)
        {
            Player = player.Id;

            if (player.CurrentItem is null)
            {
                Item = 0;
            }
            else
            {
                Item = player.CurrentItem.Serial;
            }

        }

        public override void ReadSnapshot()
        {
            base.ReadSnapshot();

            if (SnapshotReader.Singleton.TryGetPlayerActor(Player, out Npc npc))
            {
                if (Item == 0)
                {
                    npc.CurrentItem = null;
                    return;
                }

                if (SnapshotReader.Singleton.TryGetItemActor(Item, out Item item))
                {
                    npc.CurrentItem = item;
                }
            }
        }
    }
}
