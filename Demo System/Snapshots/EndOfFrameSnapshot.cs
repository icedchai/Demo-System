using DemoSystem.Snapshots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DemoSystem.Snapshots
{
    public class EndOfFrameSnapshot : Snapshot
    {
        public float DeltaTime { get; set; }

        public EndOfFrameSnapshot()
        {
            DeltaTime = Time.deltaTime;
        }

        public override void DeserializeSpecial(BinaryReader reader)
        {
            base.DeserializeSpecial(reader);
            DeltaTime = reader.ReadSingle();
        }

        public override void SerializeSpecial(BinaryWriter writer)
        {
            base.SerializeSpecial(writer);
            writer.Write(DeltaTime);
        }
    }
}
