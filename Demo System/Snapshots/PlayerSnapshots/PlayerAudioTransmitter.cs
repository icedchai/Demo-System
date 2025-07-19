using LabApi.Features.Audio;
using LabApi.Features.Wrappers;
using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Utils.Networking;
using VoiceChat;
using VoiceChat.Codec;
using VoiceChat.Codec.Enums;
using VoiceChat.Networking;

namespace DemoSystem.Snapshots.PlayerSnapshots
{
    public class PlayerAudioTransmitter
    {
        public VoiceChatChannel Channel = VoiceChatChannel.Spectator;

        private static readonly float[] EmptyData = new float[480];

        private static readonly float[] TempSampleData = new float[480];

        private static readonly byte[] TempEncodedData = new byte[512];

        public const int SampleRate = 48000;

        public const int FrameSize = 480;

        public const float FramePeriod = 0.01f;

        public const int MaxEncodedSize = 512;

        public ReferenceHub Player;

        public readonly Queue<float[]> AudioClipSamples = new Queue<float[]>();

        public Func<Player, bool>? ValidPlayers;

        public bool Looping;

        public int CurrentPosition;

        private readonly OpusEncoder opusEncoder;

        private CoroutineHandle update;

        private double targetTime;

        private bool breakCurrent;

        private float[] currentSamples = EmptyData;

        public int CurrentSampleCount
        {
            get
            {
                if (currentSamples != EmptyData)
                {
                    return currentSamples.Length;
                }

                return 0;
            }
        }

        public bool IsPlaying
        {
            get
            {
                if (update.IsRunning)
                {
                    return !IsPaused;
                }

                return false;
            }
        }

        public bool IsPaused => update.IsAliveAndPaused;

        private bool CanLoop
        {
            get
            {
                if (AudioClipSamples.Count == 0)
                {
                    return Looping;
                }

                return false;
            }
        }

        public PlayerAudioTransmitter(ReferenceHub controllerId, OpusApplicationType type = OpusApplicationType.Audio)
        {
            Player = controllerId;
            opusEncoder = new OpusEncoder(type);
        }

        public void Play(float[] samples, bool queue, bool loop)
        {
            if (samples.IsEmpty())
            {
                throw new InvalidOperationException("Audio clip samples must not be empty");
            }

            if (!queue)
            {
                breakCurrent = true;
                AudioClipSamples.Clear();
                CurrentPosition = 0;
            }

            AudioClipSamples.Enqueue(samples);
            Looping = loop;
            if (!update.IsRunning)
            {
                update = Timing.RunCoroutine(Transmit(), Segment.Update);
            }
        }

        public void Pause()
        {
            update.IsAliveAndPaused = true;
        }

        public void Resume()
        {
            update.IsAliveAndPaused = false;
            targetTime = NetworkTime.time;
        }

        public void Skip(int count)
        {
            if (count != 0)
            {
                breakCurrent = true;
                CurrentPosition = 0;
                for (int i = 1; i < count; i++)
                {
                    AudioClipSamples.Dequeue();
                }
            }
        }

        public void Stop()
        {
            Timing.KillCoroutines(update);
            AudioClipSamples.Clear();
            CurrentPosition = 0;
        }

        private IEnumerator<float> Transmit()
        {
            float root2 = Mathf.Sqrt(2f);
            targetTime = NetworkTime.time;
            while (!CollectionExtensions.IsEmpty<float[]>(AudioClipSamples))
            {
                currentSamples = AudioClipSamples.Dequeue();
                breakCurrent = false;
                while (!breakCurrent && (CanLoop || CurrentPosition < currentSamples.Length))
                {
                    try
                    {
                        while (targetTime < NetworkTime.time)
                        {
                            int num2;
                            for (int i = 0; i != 480; i += num2)
                            {
                                int num = 480 - i;
                                num2 = Mathf.Max(Mathf.Min(CurrentPosition + num, currentSamples.Length) - CurrentPosition, 0);
                                Array.Copy(currentSamples, CurrentPosition, TempSampleData, i, num2);
                                if (num == num2)
                                {
                                    CurrentPosition += num2;
                                    continue;
                                }

                                CurrentPosition = 0;
                                if (!CanLoop)
                                {
                                    currentSamples = (CollectionExtensions.IsEmpty<float[]>(AudioClipSamples) ? EmptyData : AudioClipSamples.Dequeue());
                                }
                            }

                            for (int j = 0; j < TempSampleData.Length; j++)
                            {
                                TempSampleData[j] *= root2;
                            }

                            int num3 = opusEncoder.Encode(TempSampleData, TempEncodedData);
                            if (num3 > 2)
                            {
                                VoiceMessage audioMessage = default(VoiceMessage);
                                audioMessage.Speaker = Player;
                                audioMessage.DataLength = num3;
                                audioMessage.Data = TempEncodedData;
                                audioMessage.Channel = Channel;
                                VoiceMessage audioMessage2 = audioMessage;
                                if (ValidPlayers != null)
                                {
                                    audioMessage2.SendToHubsConditionally((ReferenceHub x) => x.Mode != 0 && ValidPlayers(LabApi.Features.Wrappers.Player.Get(x)));
                                }
                                else
                                {
                                    audioMessage2.SendToAuthenticated();
                                }
                            }

                            targetTime += 0.0099999997764825821;
                        }
                    }
                    catch (Exception message)
                    {
                        LabApi.Features.Console.Logger.Error(message);
                        CurrentPosition = 0;
                        yield break;
                    }

                    yield return float.NegativeInfinity;
                }
            }

            CurrentPosition = 0;
        }
    }
}
