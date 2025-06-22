using DemoSystem.Snapshots;
using DemoSystem.Extensions;
using DemoSystem.Snapshots;
using Exiled.API.Features;
using Exiled.API.Features.Toys;
using MEC;
using NetworkManagerUtils.Dummies;
using PlayerRoles;

namespace DemoSystem.SnapshotHandlers
{
    public class SnapshotReader : IDisposable
    {
        public void Dispose()
        {
            FileStream.Dispose();
            Reader.Dispose();
            foreach (Npc npc in Players.Values)
            {
                npc.Destroy();
            }
        }
        public static SnapshotReader Singleton { get; set; }

        public Dictionary<int, Npc> Players { get; set; } = new Dictionary<int, Npc>();

        public SnapshotReader(string path = @"recording-1")
        {
            try
            {
                FileStream = new FileStream(@$"B:/Recordings/{path}", FileMode.Open, FileAccess.Read);
                Reader = new BinaryReader(FileStream);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }



            //Todo: Remove
            Timing.CallDelayed(5f, () => Timing.RunCoroutine(ReadAll()));
        }

        private FileStream FileStream;

        private BinaryReader Reader;

        public Npc SpawnPlayer(int id, string name, RoleTypeId? role = null)
        {
            Npc npc = new Npc(DummyUtils.SpawnDummy(name));
            npc.GroupName = "Actor";
            Players.Add(id, npc);
            return npc;
        }

        public bool TryGetPlayer(int id, out Npc npc)
        {
            if (!Players.TryGetValue(id, out npc))
            {
                npc = SpawnPlayer(id, $"Unknown Actor ID: {id}");
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
                    if (snapshot is EndOfFrameSnapshot)
                    {
                        yield return Timing.WaitForOneFrame;
                    }
                    else
                    {
                        try
                        {
                            snapshot.ReadSnapshot();
                        }
                        catch (Exception e)
                        {
                            Log.Error(e);
                        }
                    }
                }
            }
        }
    }
}
