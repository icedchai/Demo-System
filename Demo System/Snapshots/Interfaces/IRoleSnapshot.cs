using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoSystem.Snapshots.Interfaces
{
    public interface IRoleSnapshot
    {
        public RoleTypeId Role { get; set; }
    }
}
