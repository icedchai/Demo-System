using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoSystem.Snapshots
{
    public class RoundPropertiesSnapshot : Snapshot
    {
        public int Seed { get; set; }

        public override void DeserializeSpecial(BinaryReader reader)
        {
            base.DeserializeSpecial(reader);
            Seed = reader.ReadInt32();
        }

        public override void SerializeSpecial(BinaryWriter writer)
        {
            base.SerializeSpecial(writer);
            writer.Write(Seed);
        }

        public override void ReadSnapshot()
        {
            base.ReadSnapshot();
        }
    }
}
