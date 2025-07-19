using DemoSystem.Snapshots;
using DemoSystem.Extensions;
using DemoSystem.Snapshots;
using Exiled.API.Features;
using Exiled.API.Features.Toys;
using MEC;
using NetworkManagerUtils.Dummies;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using CommandSystem.Commands.RemoteAdmin.Dummies;
using DemoSystem.Snapshots.PlayerSnapshots;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;

namespace DemoSystem.SnapshotHandlers
{
    public class SnapshotReader : IDisposable
    {
        public void Dispose()
        {
            FileStream.Dispose();
            Reader.Dispose();

            foreach (Npc npc in PlayerActors.Values)
            {
                npc.Destroy();
            }

            foreach (var reader in PlayerVoiceChatSnapshot.PlayerIdToVorbisReader.Values)
            {
                reader.Dispose();
            }
            PlayerVoiceChatSnapshot.PlayerIdToVorbisReader.Clear();

            foreach (var audioPlayer in PlayerVoiceChatSnapshot.PlayerIdToPlayerAudioTransmitter.Values)
            {
                audioPlayer.Stop();
            }
            PlayerVoiceChatSnapshot.PlayerIdToPlayerAudioTransmitter.Clear();

            foreach (var stream in PlayerVoiceChatSnapshot.PlayerIdToFileStream.Values)
            {
                stream.Dispose();
            }
            PlayerVoiceChatSnapshot.PlayerIdToFileStream.Clear();
        }
        public static SnapshotReader Singleton { get; set; }

        public StartOfDemoSnapshot DemoProperties { get; private set; }

        public Dictionary<int, Npc> PlayerActors { get; set; } = new Dictionary<int, Npc>();

        public Dictionary<ushort, Item> ItemActors { get; set; } = new Dictionary<ushort, Item>();

        public SnapshotReader(string path = @"recording-1")
        {
            try
            {
                FileStream = new FileStream(@$"{Plugin.Singleton.Config.RecordingsDirectory}{path}", FileMode.Open, FileAccess.Read);
                Reader = new BinaryReader(FileStream);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            //Todo: Remove
            Timing.CallDelayed(1f, () => Timing.RunCoroutine(ReadAll()));
        }

        private FileStream FileStream;

        private BinaryReader Reader;

        public Pickup SpawnPickupActor(ushort serial, ItemType itemType)
        {
            Pickup pickup = Pickup.Create(itemType);

            ItemActors.Add(serial, Item.Get(pickup.Serial));
            return pickup;
        }

        public Item SpawnItemActor(ushort serial, ItemType itemType)
        {
            Item item = Item.Create(itemType);

            ItemActors.Add(serial, item);
            return item;
        }

        public bool TryGetItemActor(ushort serial, out Item item)
        {
            if (!ItemActors.TryGetValue(serial, out item))
            {
                return false;
            }
            return true;
        }

        public Npc SpawnPlayerActor(int id, string name)
        {
            Npc npc = new Npc(DummyUtils.SpawnDummy(name));
            Timing.CallDelayed(0.5f, () =>
            {
                npc.RankName = "ACTOR";
                npc.SessionVariables.Add("actor", true);
            });

            PlayerActors.Add(id, npc);
            return npc;
        }

        public bool TryGetPlayerActor(int id, out Npc npc)
        {
            if (!PlayerActors.TryGetValue(id, out npc))
            {
                npc = SpawnPlayerActor(id, $"Unknown Actor ID: {id}");
            }
            return true;
        }

        private IEnumerator<float> ReadAll()
        {
            int demoVersion = Reader.ReadInt32();
            if (demoVersion != SnapshotRecorder.Version)
            {
                Log.Error($"DemoRecorder version : Expected {SnapshotRecorder.Version} but got {demoVersion}");
                yield break;
            }

            while (true)
            {
                if (SnapshotEncoder.Deserialize(Reader, out Snapshot snapshot))
                {
                    switch (snapshot)
                    {
                        case StartOfDemoSnapshot startOfDemoSnapshot:
                            DemoProperties = startOfDemoSnapshot;
                            break;
                        case EndOfFrameSnapshot endOfFrameSnapshot:
                            yield return Timing.WaitForSeconds(endOfFrameSnapshot.DeltaTime);
                            break;
                        case EndOfDemoSnapshot:
                            Dispose();
                            yield break;
                        default:
                            snapshot.ReadSnapshot();
                            break;
                    }
                }
                else
                {
                    Log.Error("Demo had error reading snapshots");
                }
            }
        }
    }
}
