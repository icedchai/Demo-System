using DemoSystem.SnapshotHandlers;
using DemoSystem.Snapshots;
using DemoSystem.Snapshots.Interfaces;
using Exiled.API.Features;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Networking;
using VoiceChat;
using VoiceChat.Networking;
using Snapshot = DemoSystem.Snapshots.Snapshot;

namespace Demo_System.Snapshots
{
    internal class PlayerVoiceChatSnapshot : Snapshot, IPlayerSnapshot
    {
        public PlayerVoiceChatSnapshot()
        {
        }

        public PlayerVoiceChatSnapshot(VoiceMessage message)
        {
            Player = message.Speaker.PlayerId;
            Channel = message.Channel;
            DataLength = message.DataLength;
            Data = message.Data;
        }

        public override void DeserializeSpecial(BinaryReader reader)
        {
            base.DeserializeSpecial(reader);
            Channel = (VoiceChatChannel)reader.ReadInt32();
            DataLength = reader.ReadInt32();
            Data = new byte[reader.ReadInt32()];

            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = reader.ReadByte();
            }
        }

        public override void SerializeSpecial(BinaryWriter writer)
        {
            base.SerializeSpecial(writer);
            writer.Write((int)Channel);
            writer.Write(DataLength);
            writer.Write(Data.Length);

            for (int i = 0; i < Data.Length; i++)
            {
                writer.Write(Data[i]);
            }
        }

        public override void ReadSnapshot()
        {
            base.ReadSnapshot();

            if (SnapshotReader.Singleton.TryGetPlayer(Player, out Npc npc))
            {
                VoiceMessage message = new VoiceMessage();
                message.Channel = Channel;
                message.Data = Data;
                message.DataLength = DataLength;
                message.Speaker = npc.ReferenceHub;
                message.SendToAuthenticated();
            }
        }

        public int Player { get; set; }

        public VoiceChatChannel Channel { get; set; }

        public int DataLength { get; set; }

        public byte[] Data { get; set; }
    }
}
