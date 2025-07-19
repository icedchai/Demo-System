using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoSystem.Snapshots
{
    public class StartOfDemoSnapshot : Snapshot
    {
        public int Seed { get; set; }

        public long BeganRecording { get; set; }

        public DateTime BeganRecordingDate => new DateTime(BeganRecording);

        public int DateTimeKind { get; set; }

        public StartOfDemoSnapshot()
        {
            Seed = Map.Seed;
            BeganRecording = DateTime.Now.Ticks;
        }

        public override void DeserializeSpecial(BinaryReader reader)
        {
            base.DeserializeSpecial(reader);
            Seed = reader.ReadInt32();
            BeganRecording = reader.ReadInt64();
        }

        public override void SerializeSpecial(BinaryWriter writer)
        {
            base.SerializeSpecial(writer);
            writer.Write(Seed);
            writer.Write(BeganRecording);
        }

        public override void ReadSnapshot()
        {
            base.ReadSnapshot();
        }
    }
}
