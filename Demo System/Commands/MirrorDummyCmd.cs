using CommandSystem;
using DemoSystem.SnapshotHandlers;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using MEC;
using NetworkManagerUtils.Dummies;
using PlayerRoles.FirstPersonControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using LabPlayer = LabApi.Features.Wrappers.Player;

namespace DemoSystem.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class MirrorDummyCmd : ICommand
    {
        public string[] Aliases => [];

        public string Description => "to mirror";

        public string Command => "mirrordummy";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!Player.TryGet(sender, out Player player))
            {
                response = "must be in-game";
                return false;
            }

            Npc npc = new Npc(DummyUtils.SpawnDummy($"Mirror to {player.Nickname}"));
            npc.Role.Set(player.Role);
            npc.Position = player.Position + new UnityEngine.Vector3(2, 0, 0);
            Timing.RunCoroutine(DummyFollow(player, npc));
            response = "Got it";
            return true;
        }

        private IEnumerator<float> DummyFollow(Player player, Npc npc)
        {
            Role role = player.Role;
            LabPlayer Player = LabPlayer.Get(player.ReferenceHub);
            LabPlayer Npc = LabPlayer.Get(npc.ReferenceHub);

            while (true && role.IsValid)
            {
                Npc.Position = Player.Position + new UnityEngine.Vector3(2, 0, 0);
                Npc.LookRotation = Player.LookRotation;

                yield return Timing.WaitForOneFrame;
            }
            npc.Destroy();
        }
    }
}
