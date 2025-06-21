using Demo_System.Snapshots;
using DemoSystem.Extensions;
using DemoSystem.Snapshots;
using Exiled.API.Features;
using MEC;
using UnityEngine;

namespace DemoSystem.SnapshotHandlers
{
    public class SnapshotRecorder : IDisposable
    {
        private bool _disposed;

        public bool IsRecording { get; private set; }

        private static void Clear(MemoryStream source)
        {
            byte[] buffer = source.GetBuffer();
            Array.Clear(buffer, 0, buffer.Length);
            source.Position = 0;
            source.SetLength(0);
        }

        public SnapshotRecorder()
        {
            Stream = new MemoryStream();
            Writer = new BinaryWriter(Stream);
            FileStream = new FileStream($"B:/Recordings/recording-{Round.StartedTime.ToString("yyyy-dd-M--HH-mm-ss")}", FileMode.Append, FileAccess.Write);
        }

        public void StartRecording()
        {
            IsRecording = true;
            Timing.RunCoroutine(EncodeSnapshots());
            Timing.RunCoroutine(WriteFile());
            Timing.RunCoroutine(RecordPositions());
        }

        public void StopRecording()
        {
            IsRecording = false;
            WriteToFile();
        }

        public void Dispose()
        {
            WriteToFile();

            _disposed = true;
            Stream.Dispose();
            Writer.Dispose();
            FileStream.Dispose();
        }

        public Queue<Snapshot> QueuedSnapshots { get; set; } = new Queue<Snapshot>();

        private FileStream FileStream;

        private MemoryStream Stream;

        private BinaryWriter Writer;

        public void WriteToFile()
        {
            Log.Info(Stream.Length);

            Stream.Position = 0;
            Stream.CopyTo(FileStream);
            FileStream.Flush();
            Clear(Stream);
        }

        private IEnumerator<float> EncodeSnapshots()
        {
            int frame = Time.frameCount;
            while (!_disposed)
            {
                while (QueuedSnapshots.Count == 0)
                {
                    if (Round.IsEnded)
                    {
                        yield break;
                    }
                    yield return Timing.WaitForOneFrame;
                }

                Snapshot snapshot = QueuedSnapshots.Dequeue();
                snapshot.Serialize(Writer);
            }
        }

        private IEnumerator<float> WriteFile()
        {
            while (!_disposed)
            {
                WriteToFile();
                yield return Timing.WaitForSeconds(5f);
            }
        }

        Dictionary<Player, Vector3> playerLastPos { get; set; } = new Dictionary<Player, Vector3>();

        Dictionary<Player, Quaternion> playerLastRot { get; set; } = new Dictionary<Player, Quaternion>();

        private IEnumerator<float> RecordPositions()
        {
            while (IsRecording)
            {
                foreach (Player player in Player.List)
                {
                    if (player.Role.IsAlive)
                    {
                        bool recordPosOverride = false;
                        if (!playerLastPos.TryGetValue(player, out Vector3 pos))
                        {
                            pos = player.Position;
                            playerLastPos.Add(player, pos);
                            recordPosOverride = true;
                        }

                        if (!playerLastRot.TryGetValue(player, out Quaternion rot))
                        {
                            rot = player.Rotation;
                            playerLastRot.Add(player, rot);
                            recordPosOverride = true;
                        }

                        Vector3 playerPos = player.Position;
                        Quaternion playerRot = player.Rotation;

                        if (recordPosOverride || playerPos != pos || playerRot != rot)
                        {
                            QueuedSnapshots.Enqueue(new PlayerTransformSnapshot() { Player = player.Id, Rotation = playerRot, Position = playerPos });
                            playerLastPos[player] = playerPos;
                            playerLastRot[player] = playerRot;
                        }
                    }
                }
                yield return Timing.WaitForOneFrame;
                QueuedSnapshots.Enqueue(new EndOfFrameSnapshot());
            }
        }
    }
}
