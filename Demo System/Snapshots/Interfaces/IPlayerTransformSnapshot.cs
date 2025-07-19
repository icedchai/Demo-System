using DemoSystem.Snapshots.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DemoSystem.Snapshots.Interfaces
{
    public interface IPlayerTransformSnapshot
    {
        public TransformDifference TransformDifference { get; set; }

        public Vector3 Position { get; set; }

        public Vector2 Rotation { get; set; }

        public Vector3 Scale { get; set; }
    }
}
