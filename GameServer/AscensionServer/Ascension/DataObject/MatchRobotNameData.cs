using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer
{
    [Serializable]
    [ConfigData]
    public class MatchRobotNameData
    {
        public int NameID;
        public string UserName;
    }
}
