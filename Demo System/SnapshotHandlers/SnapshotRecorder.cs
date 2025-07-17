using DemoSystem.Snapshots;
using DemoSystem.Snapshots.PlayerSnapshots;
using DemoSystem.Extensions;
using DemoSystem.Snapshots;
using Exiled.API.Features;
using MEC;
using System.Runtime.InteropServices;
using UnityEngine;
using VoiceChat.Codec;
using NAudio.Wave;
using DemoSystem.Snapshots.Enums;

namespace DemoSystem.SnapshotHandlers
{
    public class SnapshotRecorder : IDisposable
    {
        public const int Version = 0;

        public DateTime BeganRecording => new DateTime(DemoProperties.BeganRecording, DateTimeKind.Local);

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

        public StartOfDemoSnapshot DemoProperties { get; private set; }

        public void StartRecording()
        {
            DemoProperties = new();
            FileStream = new FileStream($"{Plugin.Singleton.Config.RecordingsDirectory}recording-{DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss")}", FileMode.Append, FileAccess.Write);
            IsRecording = true;

            Writer.Write(Version);

            QueuedSnapshots.Enqueue(DemoProperties);
            foreach (Player player in Player.List)
            {
                QueuedSnapshots.Enqueue(new PlayerVerifiedSnapshot(player));
                QueuedSnapshots.Enqueue(new PlayerSpawnedSnapshot(player.Id, player.Role.Type, Exiled.API.Enums.SpawnReason.RoundStart, PlayerRoles.RoleSpawnFlags.None));
            }
            Timing.RunCoroutine(EncodeSnapshots());
            Timing.RunCoroutine(WriteFile());
            Timing.RunCoroutine(RecordPlayerProperties());
            Timing.RunCoroutine(WriteEndOfFrameSnapshots());
        }

        public void StopRecording()
        {
            QueuedSnapshots.Enqueue(new EndOfDemoSnapshot());
            IsRecording = false;
            Dispose();
        }

        public void Dispose()
        {
            WriteToFile();

            Disposed = true;

            Stream.Dispose();
            Writer.Dispose();
            FileStream.Dispose();

            foreach (var stream in PlayerVoiceChatSnapshot.PlayerIdToFileStream.Values)
            {
                stream.Dispose();
            }
            return;
            foreach (var writer in PlayerVoiceChatSnapshot.PlayerIdToWaveWriter.Values)
            {
                writer.Dispose();
            }
        }

        public Queue<Snapshot> QueuedSnapshots { get; set; } = new Queue<Snapshot>();

        internal FileStream FileStream;

        private MemoryStream Stream;

        private BinaryWriter Writer;

        public void WriteToFile()
        {
            // Log.Info($"{Stream.Length} bytes written.");

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
                if (snapshot is EndOfDemoSnapshot)
                {
                    Log.Info("End of demo");
                }
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

        Dictionary<Player, Vector3> playerLastPos = new Dictionary<Player, Vector3>();

        Dictionary<Player, Quaternion> playerLastRot = new Dictionary<Player, Quaternion>();

        Dictionary<Player, Vector3> playerLastScale = new Dictionary<Player, Vector3>();

        Dictionary<Player, float> playerLastHealth = new Dictionary<Player, float>();

        Dictionary<Player, float> playerLastHume = new Dictionary<Player, float>();

        Dictionary<Player, float> playerLastArtificial = new Dictionary<Player, float>();

        private IEnumerator<float> RecordPlayerProperties()
        {
            while (IsRecording)
            {
                foreach (Player player in Player.List)
                {
                    if (player.Role.IsAlive)
                    {
                        RecordPosition(player);
                        RecordHealth(player);
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

            TransformDifference transformDifference = TransformDifference.None;
            transformDifference = (playerPos != lastRecordedPos ? TransformDifference.Position : TransformDifference.None) |
                                  (playerRot != lastRecordedRot ? TransformDifference.Rotation : TransformDifference.None) |
                                  (playerScale != lastRecordedScale ? TransformDifference.Scale : TransformDifference.None);

            if (recordPosOverride || playerPos != lastRecordedPos || playerRot != lastRecordedRot || playerScale != lastRecordedScale)
            {
                QueuedSnapshots.Enqueue(new PlayerTransformSnapshot(player, transformDifference));
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
                playerLastHume.Add(player, lastRecordedHume);
                recordHealthOverride = true;
            }

            float playerHealth = player.Health;
            float playerArtifical = player.ArtificialHealth;
            float playerHume = player.HumeShield;

            if (recordHealthOverride || playerHealth != lastRecordedHealth || playerArtifical != lastRecordedArtificial || playerHume != lastRecordedHume)
            {
                QueuedSnapshots.Enqueue(new PlayerHealthSnapshot(player));

                playerLastHealth[player] = playerHealth;
                playerLastArtificial[player] = playerArtifical;
                playerLastHume[player] = playerHume;
            }
        }
    }
}
