using Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer
{
    [Serializable]
    public class Role
    {
        public virtual int RoleID { get; set; }
        public virtual string RoleName { get; set; }
        public virtual int NoviceGuide { get; set; }
        public virtual int HeadPortrait  { get; set; }
        public Role()
        {
            RoleName = "";
            NoviceGuide = 1;
        }

    }
}
