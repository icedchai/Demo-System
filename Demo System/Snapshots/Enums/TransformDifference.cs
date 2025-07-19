using Exiled.API.Features.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoSystem.Snapshots.Enums
{
    [Flags]
    public enum TransformDifference : byte
    {
        None = 0,

        Position = 1,

        Rotation = 2,

        Scale = 4,

        All = 7,
    }
}
