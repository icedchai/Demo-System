using DemoSystem.Snapshots;
using DemoSystem.Snapshots.PlayerSnapshots;
using DemoSystem.Extensions;
using DemoSystem.Snapshots;
using Exiled.API.Features;
using MEC;
using System.Runtime.InteropServices;
using UnityEngine;

namespace DemoSystem.SnapshotHandlers
{
    public class SnapshotRecorder : IDisposable
    {
        public const int Version = 0;
        public bool Disposed { get; private set; }

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
        }

        public void StartRecording()
        {
            FileStream = new FileStream($"B:/Recordings/recording-{DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss")}", FileMode.Append, FileAccess.Write);
            IsRecording = true;

            Writer.Write(Version);

            foreach (Player player in Player.List)
            {
                QueuedSnapshots.Enqueue(new PlayerVerifiedSnapshot(player));
                QueuedSnapshots.Enqueue(new PlayerSpawnedSnapshot() { Player = player.Id, Role = player.Role.Type, SpawnFlags = PlayerRoles.RoleSpawnFlags.None });
            }
            Timing.RunCoroutine(EncodeSnapshots());
            Timing.RunCoroutine(WriteFile());
            Timing.RunCoroutine(RecordPlayerProperties());
            Timing.RunCoroutine(WriteEndOfFrameSnapshots());
        }

        public void StopRecording()
        {
            IsRecording = false;
            WriteToFile();
            Dispose();
        }

        public void Dispose()
        {
            WriteToFile();

            Disposed = true;
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
            while (!Disposed)
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
            while (!Disposed)
            {
                WriteToFile();
                yield return Timing.WaitForSeconds(5f);
            }
        }

        private IEnumerator<float> WriteEndOfFrameSnapshots()
        {
            while (IsRecording)
            {
                QueuedSnapshots.Enqueue(new EndOfFrameSnapshot());
                yield return Timing.WaitForOneFrame;
            }
        }

        Dictionary<Player, Vector3> playerLastPos { get; set; } = new Dictionary<Player, Vector3>();

        Dictionary<Player, Quaternion> playerLastRot { get; set; } = new Dictionary<Player, Quaternion>();

        Dictionary<Player, Vector3> playerLastScale { get; set; } = new Dictionary<Player, Vector3>();

        Dictionary<Player, float> playerLastHealth { get; set; } = new Dictionary<Player, float>();
        Dictionary<Player, float> playerLastHume { get; set; } = new Dictionary<Player, float>();
        Dictionary<Player, float> playerLastArtificial { get; set; } = new Dictionary<Player, float>();

        private IEnumerator<float> RecordPlayerProperties()
        {
            while (IsRecording)
            {
                foreach (Player player in Player.List)
                {
                    if (player.Role.IsAlive)
                    {
                        RecordHealth(player);
                        RecordPosition(player);
                    }
                }
                yield return Timing.WaitForOneFrame;
            }
        }

        private void RecordPosition(Player player)
        {
            bool recordPosOverride = false;

            if (!playerLastPos.TryGetValue(player, out Vector3 lastRecordedPos))
            {
                lastRecordedPos = player.Position;
                playerLastPos.Add(player, lastRecordedPos);
                recordPosOverride = true;
            }

            if (!playerLastScale.TryGetValue(player, out Vector3 lastRecordedScale))
            {
                lastRecordedScale = player.Scale;
                playerLastScale.Add(player, lastRecordedScale);
                recordPosOverride = true;
            }

            if (!playerLastRot.TryGetValue(player, out Quaternion lastRecordedRot))
            {
                lastRecordedRot = player.Rotation;
                playerLastRot.Add(player, lastRecordedRot);
                recordPosOverride = true;
            }

            Vector3 playerScale = player.Scale;
            Vector3 playerPos = player.Position;
            Quaternion playerRot = player.Rotation;

            if (recordPosOverride || playerPos != lastRecordedPos || playerRot != lastRecordedRot || playerScale != lastRecordedScale)
            {
                QueuedSnapshots.Enqueue(new PlayerTransformSnapshot(player));
                playerLastPos[player] = playerPos;
                playerLastRot[player] = playerRot;
                playerLastScale[player] = playerScale;
            }
        }

        private void RecordHealth(Player player)
        {
            bool recordHealthOverride = false;

            if (!playerLastHealth.TryGetValue(player, out float lastRecordedHealth))
            {
                lastRecordedHealth = player.Health;
                playerLastHealth.Add(player, lastRecordedHealth);
                recordHealthOverride = true;
            }

            if (!playerLastArtificial.TryGetValue(player, out float lastRecordedArtificial))
            {
                lastRecordedArtificial = player.ArtificialHealth;
                playerLastArtificial.Add(player, lastRecordedArtificial);
                recordHealthOverride = true;
            }

            if (!playerLastHume.TryGetValue(player, out float lastRecordedHume))
            {
                lastRecordedHume = player.HumeShield;
                playerLastHealth.Add(player, lastRecordedHume);
                recordHealthOverride = true;
            }

            float playerHealth = player.Health;
            float playerArtifical = player.ArtificialHealth;
            float playerHume = player.HumeShield;

            if (recordHealthOverride || playerHealth != lastRecordedHealth || playerArtifical != lastRecordedArtificial || playerHume != lastRecordedHume)
            {
                QueuedSnapshots.Enqueue(new PlayerHealthSnapshot(player));
            }
        }
    }
}
