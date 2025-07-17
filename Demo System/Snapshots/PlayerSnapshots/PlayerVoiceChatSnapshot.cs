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
using VoiceChat.Codec;
using VoiceChat.Codec.Enums;
using VoiceChat.Networking;
using NVorbis;
using System.IO;
using System.Diagnostics;
using Snapshot = DemoSystem.Snapshots.Snapshot;
using NAudio.Wave;
using NAudio.Vorbis;
using UnityEngine;
using NVorbis.Contracts;

namespace DemoSystem.Snapshots.PlayerSnapshots
{
    internal class PlayerVoiceChatSnapshot : Snapshot, IPlayerSnapshot
    {
        internal static OpusEncoder OpusEncoder { get; set; } = new OpusEncoder(OpusApplicationType.Audio);

        internal static OpusDecoder OpusDecoder { get; set; } = new OpusDecoder();

        internal static Dictionary<int, FileStream> PlayerIdToFileStream { get; set; } = new Dictionary<int, FileStream>();

        internal static Dictionary<int, WaveFileWriter> PlayerIdToWaveWriter { get; set; } = new Dictionary<int, WaveFileWriter>();

        internal static Dictionary<int, VorbisReader> PlayerIdToVorbisReader { get; set; } = new Dictionary<int, VorbisReader>();

        public PlayerVoiceChatSnapshot()
        {
        }

        public PlayerVoiceChatSnapshot(VoiceMessage message)
        {
            Player = message.Speaker.PlayerId;
            Channel = message.Channel;
            Samples = new float[24000];
            Length = OpusDecoder.Decode(message.Data, message.DataLength, Samples);
        }

        public override void DeserializeSpecial(BinaryReader reader)
        {
            base.DeserializeSpecial(reader);
            Channel = (VoiceChatChannel)reader.ReadInt32();

            if (!PlayerIdToFileStream.TryGetValue(Player, out FileStream fileStream))
            {
                fileStream = new FileStream($"{Plugin.Singleton.Config.RecordingsDirectory}recording-{SnapshotReader.Singleton.DemoProperties.BeganRecording.ToString("yyyy-dd-M--HH-mm-ss")}-{Player}.ogg", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                PlayerIdToFileStream.Add(Player, fileStream);
            }
            if (!PlayerIdToVorbisReader.TryGetValue(Player, out VorbisReader vorbisReader))
            {
                PlayerIdToVorbisReader.Add(Player, (vorbisReader = new VorbisReader(fileStream)));
            }
            int samplesToTake = (int)(Time.deltaTime * 48000);
            float[] _pcm = new float[samplesToTake];
            vorbisReader.ReadSamples(_pcm, 0, samplesToTake);
            Samples = _pcm;
        }

        public override void SerializeSpecial(BinaryWriter writer)
        {
            base.SerializeSpecial(writer);
            writer.Write((int)Channel);
            WaveFileWriter waveWriter;
            if (!PlayerIdToFileStream.TryGetValue(Player, out FileStream fileStream))
            {
                fileStream = new FileStream($"{Plugin.Singleton.Config.RecordingsDirectory}recording-{Plugin.Singleton.Recorder.BeganRecording.ToString("yyyy-dd-M--HH-mm-ss")}-{Player}.wav", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                PlayerIdToFileStream.Add(Player, fileStream);
            }
            if (!PlayerIdToWaveWriter.TryGetValue(Player, out waveWriter))
            {
                PlayerIdToWaveWriter.Add(Player, (waveWriter = new WaveFileWriter(fileStream, new WaveFormat(48000, 1))));
            }
            waveWriter.WriteSamples(Samples, 0, Length);
        }

        public override void ReadSnapshot()
        {
            base.ReadSnapshot();

            if (SnapshotReader.Singleton.TryGetPlayerActor(Player, out Npc npc))
            {
                byte[] encodedData = new byte[512];

                int length = OpusEncoder.Encode(Samples, encodedData);

                VoiceMessage message = new VoiceMessage();
                message.Data = encodedData;
                message.DataLength = length;
                message.Channel = Channel;
                message.Speaker = npc.ReferenceHub;
                message.SendToAuthenticated();
            }
        }

        public int Player { get; set; }

        public VoiceChatChannel Channel { get; set; }

        public float[] Samples { get; set; }

        public int Length { get; set; }
    }
}
