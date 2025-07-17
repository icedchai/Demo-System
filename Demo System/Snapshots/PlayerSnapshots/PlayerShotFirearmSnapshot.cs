using DemoSystem.SnapshotHandlers;
using DemoSystem.Snapshots.Interfaces;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using CommandSystem;
using NetworkManagerUtils.Dummies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandSystem.Commands.RemoteAdmin.Dummies;
using PlayerRoles.FirstPersonControl;
using Exiled.API.Extensions;

namespace DemoSystem.Snapshots.PlayerSnapshots
{
    public class PlayerShotFirearmSnapshot : Snapshot, IPlayerSnapshot
    {
        public PlayerShotFirearmSnapshot(Player player)
        {
            Player = player.Id;
        }

        public override void ReadSnapshot()
        {
            base.ReadSnapshot();

            if (SnapshotReader.Singleton.TryGetPlayerActor(Player, out Npc npc))
            {
                if (npc.CurrentItem is Firearm firearm)
                {
                    firearm.Base.DummyEmulator.AddEntry(ActionName.Shoot, true);
                }
            }
        }

        public int Player { get; set; }
    }
}
