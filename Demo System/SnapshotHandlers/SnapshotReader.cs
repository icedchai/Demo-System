﻿using DemoSystem.Snapshots;
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

        public Npc SpawnActor(int id, string name)
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

        public bool TryGetActor(int id, out Npc npc)
        {
            if (!PlayerActors.TryGetValue(id, out npc))
            {
                npc = SpawnActor(id, $"Unknown Actor ID: {id}");
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
                            yield return Timing.WaitForOneFrame;
                            break;
                        case EndOfDemoSnapshot:
                            Dispose();
                            yield break;
                        default:
                            try
                            {
                                snapshot.ReadSnapshot();
                            }
                            catch (Exception e)
                            {
                                Log.Error(e);
                            }
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
