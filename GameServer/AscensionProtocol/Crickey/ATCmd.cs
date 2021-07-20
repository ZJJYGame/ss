using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol
{
    /// <summary>
    /// 传输 命令码
    /// </summary>
    public enum ATCmd:byte//区分请求和响应
    {
        Default =0,
        TapTapLogin = 1,
        Logoff=2,
        Register= 3,
        /// <summary>
        /// 同步当前这个角色的数据
        /// </summary>
        SyncRole=4,
        /// <summary>
        /// 同步战斗消息
        /// </summary>
        SyncBattle=5,
        /// <summary>
        /// 同步蛐蛐
        /// </summary>
        SyncCricket =6,
        /// <summary>
        /// 同步背包
        /// </summary>
        SyncInventory = 7,
        /// <summary>
        /// 同步商店
        /// </summary>
        SyncShop = 8,
        /// <summary>
        /// 同步任务
        /// </summary>
        SyncTask = 9,
        SyncExploration = 10,
        SyncRank = 11,
        /// <summary>
        /// 同步邀请码数据
        /// </summary>
        SyncSpreaCode = 12,
        SyncMatch = 13,
        SyncGuide=14,
        /// <summary>
        /// 人物命名
        /// </summary>
        EigeneInfo= 15,
        /// <summary>
        /// 同步战斗结算信息
        /// </summary>
        SyncBattleCombat = 17,
        /// <summary>
        /// 同步爬塔信息
        /// </summary>
        SyncTower=18,
        /// <summary>
        /// 测试消息队列
        /// </summary>
        MessageQueue = 187,
        /// <summary>
        /// 心跳
        /// </summary>
        HeartBeat = 244,
        /// <summary>
        /// 子操作码
        /// </summary>
        SubOpCodeData = 254,
        SubOperationCode = 255
         
    }
}
