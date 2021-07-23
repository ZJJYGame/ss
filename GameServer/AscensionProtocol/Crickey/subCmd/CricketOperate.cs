using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol
{
    public enum CricketOperateType:byte
    {
        None=0,
        AddCricket=1,
        GetCricket = 2,
        GetTempCricket=3,
        RemoveCricket = 4,
        AddPoint=5,//单个蛐蛐的所有数值
        RmvTempCricket = 6,
        UseItem = 7,
        UpdCricket = 8,
        UpdTempCricket =9,
        EnlargeNest=10,//扩充窝
        LevelUp=11,
        UpdateSkill=12,
        UpdateName=13,//改名字
    }
}
