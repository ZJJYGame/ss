using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer
{
    [Serializable]
    public class Tower
    {
        public virtual int RoleID { get; set; }
        //玩家可挑战的最大难度
        public virtual int MaxDifficulty { get; set; }
        //玩家当前选择挑战的难度，负数表示玩家未选择,默认为-1
        public virtual int NowChooseDifficulty { get; set; }
        //当前挑战的层数(第几层)
        public virtual int NowLevel { get; set; }
        public virtual string NowEnemyName { get; set; }
        public virtual int NowEnemyIconId { get; set; }
        public Tower()
        {
            RoleID = 0;
            MaxDifficulty = 0;
            NowChooseDifficulty = -1;
            NowLevel = 0;
            NowEnemyName = default;
            NowEnemyIconId = 0;
        }
    }
}
