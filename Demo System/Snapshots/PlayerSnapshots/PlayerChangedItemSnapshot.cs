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
    public class PlayerChangedItemSnapshot : Snapshot, IPlayerSnapshot
    {
        public int Player { get; set; }

        public ItemType ItemType { get; set; }

        public List<AttachmentName> FirearmAttachments { get; set; }

        public JailbirdWearState JailbirdWearState { get; set; }

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
                if (player.CurrentItem is Firearm firearm)
                {
                    FirearmAttachments = firearm.Attachments.Select(f => f.Name).ToList();
                }

                if (player.CurrentItem is Jailbird jailbird)
                {
                    JailbirdWearState = jailbird.WearState;
                }
            }

        }

        public override void ReadSnapshot()
        {
            base.ReadSnapshot();

            if (SnapshotReader.Singleton.TryGetActor(Player, out Npc npc))
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
                Item item = npc.AddItem(ItemType);

                if (item is Firearm firearm)
                {
                    firearm.AddAttachment(FirearmAttachments);
                }
/*
                if (item is Jailbird jailbird)
                {
                    jailbird.WearState = JailbirdWearState;
                }*/

                npc.CurrentItem = item;
            }
        }

        public override void SerializeSpecial(BinaryWriter writer)
        {
            base.SerializeSpecial(writer);
            writer.Write((int)ItemType);
            if (ItemType.IsWeapon(false))
            {
                writer.Write(FirearmAttachments.Count);
                foreach (AttachmentName attachment in FirearmAttachments)
                {
                    writer.Write((int)attachment);
                }
            }
            /*else if (ItemType == ItemType.Jailbird)
            {
                writer.Write((int)JailbirdWearState);
            }*/
        }

        public override void DeserializeSpecial(BinaryReader reader)
        {
            base.DeserializeSpecial(reader);
            ItemType = (ItemType)reader.ReadInt32();
            if (ItemType.IsWeapon(false))
            {
                for (int i = 0; i < reader.ReadInt32(); i++)
                {
                    FirearmAttachments.Add((AttachmentName)reader.ReadInt32());
                }
            }
            /*else if (ItemType == ItemType.Jailbird)
            {
                JailbirdWearState = (JailbirdWearState)reader.ReadInt32();
            }*/
        }
    }
}
