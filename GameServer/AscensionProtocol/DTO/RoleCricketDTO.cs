using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol
{
    [Serializable]
    public  class RoleCricketDTO
    {
        public virtual int RoleID { set; get; }
        public virtual List<int> CricketList { set; get; }
        public virtual List<int> TemporaryCrickets { get; set; }

        public RoleCricketDTO()
        {
            CricketList =new List<int>() { -1,-1,-1};
            TemporaryCrickets = new List<int>() {-1,-1,-1,-1, -1, -1, -1, -1, -1, -1 };
        }
    }
}
