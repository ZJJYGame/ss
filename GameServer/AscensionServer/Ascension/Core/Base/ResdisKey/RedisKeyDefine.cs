using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionServer
{
    /// <summary>
    /// Postfix(后缀);
    /// Perfix(前缀)； 
    /// </summary>
    public class RedisKeyDefine
    {
        #region 每日获取金币限制相关
        //计时标记
        public static readonly string _RankGetMoneyLimitRefreshFlagPerfix = "RankGetMoneyLimitRefreshFlag";
        public static readonly string _RankGetMoneyLimitPerfix = "RankGetMoneyLimit";
        #endregion

        #region 每日爬塔挑战次数相关
        //爬塔刷新计时标记
        public static readonly string _TowerChallengeCountRefreshFlagPerfix = "_TowerChallengeCountRefreshFlagPerfix";
        //爬塔次数记录
        public static readonly string _TowerChallengeCountPerfix = "_TowerChallengeCountLimit";
        #endregion

        #region 每日任务相关
        //每日任务倒计时刷新的标记位
        public static readonly string _DailyTaskRefreshFlagPerfix = "DailyTaskRefreshFlag";
        //每日具体任务信息
        public static readonly string _DailyTaskPerfix = "DailyTask";
        //玩家每日任务完成情况记录
        public static readonly string _RoleDailyTaskRecordPerfix = "RoleDailyTaskRecord";
        #endregion

        #region 排行榜事件
        /// <summary>
        /// 排行榜刷新
        /// </summary>
        public static readonly string _RankListRefreshFlag = "RankListRefreshFlag";
        #endregion
    }
}
