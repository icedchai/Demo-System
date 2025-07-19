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
using LabApi.Features.Audio;
using MEC;
using System.Collections;

namespace DemoSystem.Snapshots.PlayerSnapshots
{
    internal class PlayerVoiceChatSnapshot : Snapshot, IPlayerSnapshot
    {
        internal static OpusEncoder OpusEncoder { get; set; } = new OpusEncoder(OpusApplicationType.Audio);

        internal static OpusDecoder OpusDecoder { get; set; } = new OpusDecoder();

        internal static Dictionary<int, FileStream> PlayerIdToFileStream { get; set; } = new Dictionary<int, FileStream>();

        internal static Dictionary<int, WaveFileWriter> PlayerIdToWaveWriter { get; set; } = new Dictionary<int, WaveFileWriter>();

        internal static Dictionary<int, VorbisReader> PlayerIdToVorbisReader { get; set; } = new Dictionary<int, VorbisReader>();

        internal static Dictionary<int, PlayerAudioTransmitter> PlayerIdToPlayerAudioTransmitter { get; set; } = new Dictionary<int, PlayerAudioTransmitter>();

        public PlayerVoiceChatSnapshot() { }

        public PlayerVoiceChatSnapshot(VoiceMessage message, bool intercom = false)
        {
            Player = message.Speaker.PlayerId;
            if (intercom)
            {
                Channel = VoiceChatChannel.Intercom;
            }
            else
            {
                Channel = message.Channel;
            }
            Samples = new float[24000];
            Length = OpusDecoder.Decode(message.Data, message.DataLength, Samples);
        }

        public override void DeserializeSpecial(BinaryReader reader)
        {
            base.DeserializeSpecial(reader);
            return;
            Channel = (VoiceChatChannel)reader.ReadInt32();
            Log.Info("setup voice snapshot");
            if (!PlayerIdToFileStream.TryGetValue(Player, out FileStream fileStream))
            {
                Log.Info($"{Plugin.Singleton.Config.RecordingsDirectory}recording-{SnapshotReader.Singleton.DemoProperties.BeganRecordingDate.ToString("yyyy-dd-M--HH-mm-ss")}-{Player}.ogg");
                fileStream = new FileStream($"{Plugin.Singleton.Config.RecordingsDirectory}recording-{SnapshotReader.Singleton.DemoProperties.BeganRecordingDate.ToString("yyyy-dd-M--HH-mm-ss")}-{Player}.ogg", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                PlayerIdToFileStream.Add(Player, fileStream);
            }

            if (!PlayerIdToVorbisReader.TryGetValue(Player, out VorbisReader vorbisReader))
            {
                Log.Info("Creating vorbis reader");
                PlayerIdToVorbisReader.Add(Player, (vorbisReader = new VorbisReader(fileStream)));
            }
            Log.Info("Done setting up for voice snapshot");
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
            return;
            if (SnapshotReader.Singleton.TryGetPlayerActor(Player, out Npc npc))
            {
                if (!PlayerIdToPlayerAudioTransmitter.TryGetValue(Player, out var audioPlayer))
                {
                    audioPlayer = new PlayerAudioTransmitter(npc.ReferenceHub);
                    Timing.RunCoroutine(PlayAudioFile(audioPlayer));
                }
                audioPlayer.Channel = Channel;
                Queue.Enqueue(Samples);
            }
        }

        private Queue<float[]> Queue { get; set; } = new Queue<float[]>();

        internal IEnumerator<float> PlayAudioFile(PlayerAudioTransmitter audioPlayer)
        {
            bool shouldRun = true;
            while (shouldRun && audioPlayer.IsPlaying && !Round.IsLobby)
            {
                float[] samples = Queue.Dequeue();
                if (samples is not null && samples.Count() != 0)
                {
                    audioPlayer.Play(samples, true, false);
                }
                yield return Timing.WaitForOneFrame;
            }

            audioPlayer.Stop();
            audioPlayer = null;
        }

        public int Player { get; set; }

        public VoiceChatChannel Channel { get; set; }

        public float[] Samples { get; set; } = new float[24000];

        public int Length { get; set; }
    }
}
