using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol
{
    [Serializable]
    public class TowerDTO
    {
        public virtual int RoleID { get; set; }
        public virtual int MaxDifficulty { get; set; }
        //玩家当前选择挑战的难度，负数表示玩家未选择,默认为-1
        public virtual int NowChooseDifficulty { get; set; }
        //当前挑战的层数
        public virtual int NowLevel { get; set; }
        //当前敌人名字对应的id
        public virtual string NowEnemyName { get; set; }
        //当前敌人头像模型对应的id
        public virtual int NowEnemyIconId { get; set; }
        //剩余挑战次数
        public virtual int RemainChallengeCount { get; set; }
        public virtual int ChooseCricketId { get; set; }
    }
}
