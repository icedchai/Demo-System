using Demo_System.Snapshots;
using DemoSystem.Extensions;
using DemoSystem.Snapshots;
using Exiled.API.Features;
using Exiled.API.Features.Toys;
using MEC;
using PlayerRoles;

namespace DemoSystem.SnapshotHandlers
{
    public class SnapshotReader
    {
        public static SnapshotReader Singleton { get; set; }

        public Dictionary<int, Npc> Players { get; set; } = new Dictionary<int, Npc>();

        public SnapshotReader()
        {
            try
            {

                FileStream = new FileStream(@"B:/Recordings/recording-1", FileMode.Open, FileAccess.Read);
                Reader = new BinaryReader(FileStream);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }



            //Todo: Remove
            Timing.RunCoroutine(ReadAll());
        }

        private FileStream FileStream;

        private BinaryReader Reader;

        public Npc SpawnPlayer(int id, string name, RoleTypeId? role = null)
        {
            Npc npc = Npc.Spawn(name, role ?? RoleTypeId.Tutorial);
            Players.Add(npc.Id, npc);
            npc.IsGodModeEnabled = true;
            return npc;
        }

        public bool TryGetPlayer(int id, out Npc npc)
        {
            if (!Players.TryGetValue(id, out npc))
            {
                npc = SpawnPlayer(id, $"{id}");
                return true;
            }
            return true;
        }

        private IEnumerator<float> ReadAll()
        {
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
                        snapshot.ReadSnapshot();
                    }
                }
            }
        }
    }
}
