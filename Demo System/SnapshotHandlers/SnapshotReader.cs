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
            Timing.CallDelayed(5f, () => Timing.RunCoroutine(ReadAll()));
        }

        private FileStream FileStream;

        private BinaryReader Reader;

        public Npc SpawnPlayer(int id, string name, RoleTypeId? role = null)
        {
            Npc npc = Npc.Spawn(name, role ?? RoleTypeId.Overwatch);
            Players.Add(npc.Id, npc);
            return npc;
        }

        public bool TryGetPlayer(int id, out Npc npc)
        {
            if (!Players.TryGetValue(id, out npc))
            {
                npc = null;
                return false;
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
