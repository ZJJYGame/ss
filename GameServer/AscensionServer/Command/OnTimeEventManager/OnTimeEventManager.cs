using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using AscensionProtocol;
using Protocol;
using RedisDotNet;
using System.Reflection;

namespace AscensionServer
{
    [CustomeModule]
    public class OnTimeEventManager : Module<OnTimeEventManager>
    {

        public override void OnPreparatory()
        {

            //清除战斗获得金钱限制的事件
            RefreshGetMoneyLimitEvent();
            //清除重置 每日任务的事件
            RefreshDailyTaskEvent();
            //排行榜刷新事件
            RefreshRankListEvent();
        }
        /// <summary>
        /// 添加Redis周期刷新事件
        /// </summary>
        /// <param name="key">redis记录时间的标记</param>
        /// <param name="onTimeEventStruct">时间与事件相关的数据</param>
        public async void AddOnTImeEventByDay(string key, OnTimeEventStruct onTimeEventStruct)
        {
            DateTime today = DateTime.Now;
            DateTime tommorow;
            DateTime tempDateTime = new DateTime(today.Year, today.Month, today.Day, onTimeEventStruct.hours, onTimeEventStruct.minutes, onTimeEventStruct.seconds);
            int index = DateTime.Compare(today, tempDateTime);
            if (index < 0)//还没到刷新时间
            {
                tommorow = DateTime.Now;
            }
            else
            {
                tommorow = DateTime.Now.AddDays(1);
            }
            tommorow = new DateTime(tommorow.Year, tommorow.Month, tommorow.Day, onTimeEventStruct.hours, onTimeEventStruct.minutes, onTimeEventStruct.seconds);
            TimeSpan timeSpan = tommorow.Subtract(today);
            Utility.Debug.LogError(timeSpan.Days + "天" + timeSpan.Hours + "小时" + timeSpan.Minutes + "分钟" + timeSpan.Seconds + "秒");

            //如果当前不存在key，需要先添加限时key,
            //不存在代表当天第一次开启服务器，需判断执行昨天的刷新事件
            if (!await RedisHelper.KeyExistsAsync(key))
            {
                await RedisHelper.String.StringSetAsync<string>(key, DateTime.Now.ToString(), timeSpan);
                if (onTimeEventStruct.dayInWeek.Contains((int)today.DayOfWeek))
                    if (index > 0)
                        onTimeEventStruct.actionCallBack?.Invoke(key);
            }

            RedisManager.Instance.AddKeyExpireListener(key, (string str) =>
            {
                Utility.Debug.LogError("监听成功并继续添加事件");
                AddOnTImeEventByDay(key, onTimeEventStruct);
            });
        }

        /// <summary>
        /// 刷新获得金钱限制的事件
        /// </summary>
        void RefreshGetMoneyLimitEvent()
        {
            AddOnTImeEventByDay(RedisKeyDefine._RankGetMoneyLimitRefreshFlagPerfix, new OnTimeEventStruct(9, 30, 0, new int[] { 0, 1, 2, 3, 4, 5, 6 }, async (string str) =>
             {
                 Utility.Debug.LogError("金钱获取限制刷新");
                 await RedisHelper.KeyDeleteAsync(RedisKeyDefine._RankGetMoneyLimitPerfix);
             }));
        }
        /// <summary>
        /// 刷新每日任务的事件
        /// </summary>
        void RefreshDailyTaskEvent()
        {
            AddOnTImeEventByDay(RedisKeyDefine._DailyTaskRefreshFlagPerfix, new OnTimeEventStruct(9, 30, 0, new int[] { 0, 1, 2, 3, 4, 5, 6 }, async (string str) =>
            {
                Utility.Debug.LogError("每日任务刷新");
                Dictionary<int, TaskItemDTO> dailyTaskDict = GameManager.CustomeModule<TaskManager>().GetRandomTask(3);
                //设置每日任务数据
                await RedisHelper.String.StringSetAsync(RedisKeyDefine._DailyTaskPerfix, dailyTaskDict);
                //清除所有玩家每日完成任务的记录
                await RedisHelper.KeyDeleteAsync(RedisKeyDefine._RoleDailyTaskRecordPerfix);
            }));
        }
        /// <summary>
        /// 刷新排行榜事件
        /// </summary>
        void RefreshRankListEvent()
        {
            AddOnTImeEventByDay(RedisKeyDefine._RankListRefreshFlag, new OnTimeEventStruct(9, 30, 0, new int[] { 0, 1, 2, 3, 4, 5, 6 }, (string str) =>
           {
               Utility.Debug.LogError("排行榜刷新");
               GameManager.CustomeModule<RankManager>().ClearRankDict();
           }));
        }
    }
}
