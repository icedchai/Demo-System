using DemoSystem.Snapshots.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoSystem.Snapshots.ItemSnapshots
{
    public class ItemCreatedSnapshot : Snapshot, IItemSnapshot
    {
        public ushort Item { get; set; }

        public ItemType ItemType { get; set; }

        public ItemCreatedSnapshot() { }
    }
}
