using DemoSystem.SnapshotHandlers;
using DemoSystem.Snapshots.Interfaces;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoSystem.Snapshots.PlayerSnapshots
{
    public class PlayerReceivedItemSnapshot : Snapshot, IPlayerSnapshot, IItemSnapshot
    {
        public int Player { get; set; }

        public ushort Item { get; set; }

        public override void ReadSnapshot()
        {
            base.ReadSnapshot();

            if (SnapshotReader.Singleton.TryGetPlayerActor(Player, out Npc npc) && SnapshotReader.Singleton.TryGetItemActor(Item, out Item item))
            {
                if (item.IsInInventory)
                {
                    item.Owner.RemoveItem(Item, false);
                }

                item.Give(npc);
            }
        }
    }
}
