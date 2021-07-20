using Cosmos;
using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol;

namespace AscensionServer
{
    public partial class ExplorationManager
    {
        /// <summary>
        /// 获取探索
        /// </summary>
        /// <param name="roleId"></param>
        public static void xRGetExploration(int roleId)
        {
            var nHcriteria = xRCommon.xRNHCriteria("RoleID", roleId);
            if (xRCommon.xRVerify<Role>(nHcriteria))
            {
                var xRserver = xRCommon.xRCriteria<Exploration>(nHcriteria);

                Utility.Debug.LogInfo("老陆==>" + xRserver.ExplorationItemDict);
                var pareams = xRCommon.xRS2CParams();
                pareams.Add((byte)ParameterCode.RoleExploration, xRserver.ExplorationItemDict);
                pareams.Add((byte)ParameterCode.RoleExplorationUnlock, xRserver.UnLockDict);
                pareams.Add((byte)ParameterCode.RoleExlorationProp, xRserver.CatchAndTimeDict);
                var subOp = xRCommon.xRS2CSub();
                subOp.Add((byte)SubOperationCode.Get, pareams);
                xRCommon.xRS2CSend(roleId, (byte)ATCmd.SyncExploration, (byte)ReturnCode.Success, subOp);
            }
        }

        /// <summary>
        /// 添加探索   ///  添加探索事件的时候  会随机添加    TimeAndCatchpropInfo 传换个参数就可以啦好我看看  ItemInfo  传入 null  应该没啥的吧或者传入一个空字典我给你加个空判断吧 OK好 
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="ItemInfo"></param>
        public static void xRAddExploration(int roleId, Dictionary<int, ExplorationItemDTO> ItemInfo, Dictionary<int, int> TimeAndCatchpropInfo = null)
        {
            var nHcriteria = xRCommon.xRNHCriteria("RoleID", roleId);
            if (xRCommon.xRVerify<Role>(nHcriteria))
            {
                var xRserver = xRCommon.xRCriteria<Exploration>(nHcriteria);
                var xrDict = Utility.Json.ToObject<Dictionary<int, ExplorationItemDTO>>(xRserver.ExplorationItemDict);
                var xrPropDict = Utility.Json.ToObject<Dictionary<int, int>>(xRserver.CatchAndTimeDict);
                //这个iteminfo为探索的一个初始值
                if (ItemInfo != null)
                {
                    foreach (var info in ItemInfo)
                    {
                        //info.Key为区域号。0~3
                        if (!xrDict.ContainsKey(info.Key))
                        {
                            xrDict[info.Key] = info.Value;
                            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, ExplorationData>>(out var setExploration);
                            for (int i = 0; i < xrDict[info.Key].ItemId.Count; i++)
                            {
                                int xrRandom = 1;//这个地方处理   默认都是一个 添加的时候事件的时候  通过事件id  给定数量    这个地方随机没改呢

                                if (setExploration[xrDict[info.Key].ItemId.ToList()[i].Key].Number.Count == 2)
                                {
                                    xrRandom = RandomManager(info.Key, setExploration[xrDict[info.Key].ItemId.ToList()[i].Key].Number[0], setExploration[xrDict[info.Key].ItemId.ToList()[i].Key].Number[1]);
                                }
                                //应该是这个地方//那为什么这里加道具的 随机在下面
                                xrDict[info.Key].ItemId[xrDict[info.Key].ItemId.ToList()[i].Key] = xrRandom;
                                switch (setExploration[xrDict[info.Key].ItemId.ToList()[i].Key].EventType)
                                {
                                    case "GetPropA":
                                        if (setExploration[xrDict[info.Key].ItemId.ToList()[i].Key].Number.Count == 2)
                                        {
                                            xrDict[info.Key].ItemId[xrDict[info.Key].ItemId.ToList()[i].Key] = RandomManager(info.Key, setExploration[xrDict[info.Key].ItemId.ToList()[i].Key].Number[0], setExploration[xrDict[info.Key].ItemId.ToList()[i].Key].Number[1]);
                                        }
                                        else
                                        {
                                            xrDict[info.Key].ItemId[xrDict[info.Key].ItemId.ToList()[i].Key] = setExploration[xrDict[info.Key].ItemId.ToList()[i].Key].Number[0];
                                        }
                                        break;
                                    case "GetPropB":
                                        break;
                                    case "GetPropC":
                                       
                                        break;
                                    case "GetCricket"://全局id
                                    case "GetSkill":
                                        xrDict[info.Key].ItemId[xrDict[info.Key].ItemId.ToList()[i].Key] = 1;
                                        break;
                                    case "AddExp":
                                        //这儿是算探索经验增加的
                                        var nHcriteriaID = xRCommon.xRNHCriteria("ID", info.Value.CustomId);
                                        var xRserverGrade = xRCommon.xRCriteria<Cricket>(nHcriteriaID);
                                        //每个区域的最低经验
                                        var gradeValue = info.Key == 0 ? 100 : info.Key == 1 ? 400 : info.Key == 2 ? 800 : 1600;
                                        //探索获得经验比
                                        var percentValue = setExploration[xrDict[info.Key].ItemId.ToList()[i].Key].Number[0];
                                        //蛐蛐等级平方*经验比
                                        var levbelValue = xRserverGrade.LevelID * xRserverGrade.LevelID * percentValue;
                                        //区域加成*蛐蛐等级平方*经验比即为最后获得的经验
                                        var expValue = info.Key == 0 ? levbelValue * 6 : info.Key == 1 ? levbelValue * 12 : info.Key == 2 ? levbelValue * 18 : levbelValue * 24;
                                        xrDict[info.Key].ItemId[xrDict[info.Key].ItemId.ToList()[i].Key] = expValue / 100 < gradeValue ? gradeValue : expValue / 100;
                                        break;
                                }
                            }
                        }
                        NHibernateQuerier.Update(new Exploration() { RoleID = roleId, ExplorationItemDict = Utility.Json.ToJson(xrDict), UnLockDict = xRserver.UnLockDict, CatchAndTimeDict = xRserver.CatchAndTimeDict });
                    }
                }

                if (TimeAndCatchpropInfo != null)  //你直接调用这个方法不就可以了嘛
                {
                    foreach (var prop in TimeAndCatchpropInfo)
                    {
                        if (!xrPropDict.ContainsKey(prop.Key))
                        {
                            xrPropDict[prop.Key] = prop.Value;
                        }
                        else
                        {
                            xrPropDict[prop.Key] += prop.Value;
                        }
                        NHibernateQuerier.Update(new Exploration() { RoleID = roleId, ExplorationItemDict = Utility.Json.ToJson(xrDict), UnLockDict = xRserver.UnLockDict, CatchAndTimeDict = Utility.Json.ToJson(xrPropDict) });
                    }
                }
                xRGetExploration(roleId);
            }
        }

        /// <summary>
        /// 注册添加探索
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="ItemInfo"></param>
        /// <param name="TimeAndCatchpropInfo"></param>
        public static void xRRegisterAddExploration(int roleId, Dictionary<int, ExplorationItemDTO> ItemInfo, Dictionary<int, int> TimeAndCatchpropInfo = null)
        {
            var nHcriteria = xRCommon.xRNHCriteria("RoleID", roleId);
            if (xRCommon.xRVerify<Role>(nHcriteria))
            {
                var xRserver = xRCommon.xRCriteria<Exploration>(nHcriteria);
                var xrDict = Utility.Json.ToObject<Dictionary<int, ExplorationItemDTO>>(xRserver.ExplorationItemDict);
                var xrPropDict = Utility.Json.ToObject<Dictionary<int, int>>(xRserver.CatchAndTimeDict);
                //这个iteminfo为探索的一个初始值
                if (ItemInfo != null)
                {
                    foreach (var info in ItemInfo)
                    {
                        //info.Key为区域号。0~3
                        if (!xrDict.ContainsKey(info.Key))
                        {
                            xrDict[info.Key] = info.Value;
                            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, ExplorationData>>(out var setExploration);
                            for (int i = 0; i < xrDict[info.Key].ItemId.Count; i++)
                            {
                                int xrRandom = 1;//这个地方处理   默认都是一个 添加的时候事件的时候  通过事件id  给定数量    这个地方随机没改呢

                                if (setExploration[xrDict[info.Key].ItemId.ToList()[i].Key].Number.Count == 2)
                                {
                                    xrRandom = RandomManager(info.Key, setExploration[xrDict[info.Key].ItemId.ToList()[i].Key].Number[0], setExploration[xrDict[info.Key].ItemId.ToList()[i].Key].Number[1]);
                                }
                                //应该是这个地方//那为什么这里加道具的 随机在下面
                                xrDict[info.Key].ItemId[xrDict[info.Key].ItemId.ToList()[i].Key] = xrRandom;
                                switch (setExploration[xrDict[info.Key].ItemId.ToList()[i].Key].EventType)
                                {
                                    case "GetPropA":
                                        if (setExploration[xrDict[info.Key].ItemId.ToList()[i].Key].Number.Count == 2)
                                        {
                                            xrDict[info.Key].ItemId[xrDict[info.Key].ItemId.ToList()[i].Key] = RandomManager(info.Key, setExploration[xrDict[info.Key].ItemId.ToList()[i].Key].Number[0], setExploration[xrDict[info.Key].ItemId.ToList()[i].Key].Number[1]);
                                        }
                                        else
                                        {
                                            xrDict[info.Key].ItemId[xrDict[info.Key].ItemId.ToList()[i].Key] = setExploration[xrDict[info.Key].ItemId.ToList()[i].Key].Number[0];
                                        }
                                        break;
                                    case "GetPropB":
                                        break;
                                    case "GetPropC":

                                        break;
                                    case "GetCricket"://全局id
                                    case "GetSkill":
                                        xrDict[info.Key].ItemId[xrDict[info.Key].ItemId.ToList()[i].Key] = 1;
                                        break;
                                    case "AddExp":
                                        //这儿是算探索经验增加的
                                        var nHcriteriaID = xRCommon.xRNHCriteria("ID", info.Value.CustomId);
                                        var xRserverGrade = xRCommon.xRCriteria<Cricket>(nHcriteriaID);
                                        //每个区域的最低经验
                                        var gradeValue = info.Key == 0 ? 100 : info.Key == 1 ? 400 : info.Key == 2 ? 800 : 1600;
                                        //探索获得经验比
                                        var percentValue = setExploration[xrDict[info.Key].ItemId.ToList()[i].Key].Number[0];
                                        //蛐蛐等级平方*经验比
                                        var levbelValue = xRserverGrade.LevelID * xRserverGrade.LevelID * percentValue;
                                        //区域加成*蛐蛐等级平方*经验比即为最后获得的经验
                                        var expValue = info.Key == 0 ? levbelValue * 6 : info.Key == 1 ? levbelValue * 12 : info.Key == 2 ? levbelValue * 18 : levbelValue * 24;
                                        xrDict[info.Key].ItemId[xrDict[info.Key].ItemId.ToList()[i].Key] = expValue / 100 < gradeValue ? gradeValue : expValue / 100;
                                        break;
                                }
                            }
                        }
                        NHibernateQuerier.Update(new Exploration() { RoleID = roleId, ExplorationItemDict = Utility.Json.ToJson(xrDict), UnLockDict = xRserver.UnLockDict, CatchAndTimeDict = xRserver.CatchAndTimeDict });
                    }
                }

                if (TimeAndCatchpropInfo != null)  //你直接调用这个方法不就可以了嘛
                {
                    foreach (var prop in TimeAndCatchpropInfo)
                    {
                        if (!xrPropDict.ContainsKey(prop.Key))
                        {
                            xrPropDict[prop.Key] = prop.Value;
                        }
                        else
                        {
                            xrPropDict[prop.Key] += prop.Value;
                        }
                        NHibernateQuerier.Update(new Exploration() { RoleID = roleId, ExplorationItemDict = Utility.Json.ToJson(xrDict), UnLockDict = xRserver.UnLockDict, CatchAndTimeDict = Utility.Json.ToJson(xrPropDict) });
                    }
                }
            }
        }


        /// <summary>
        /// 更新探索
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="ItemInfo"></param>
        public static void xRUpdateExploration(int roleId, Dictionary<int, ExplorationItemDTO> ItemInfo, Dictionary<int, int> propInfo)
        {
            var nHcriteria = xRCommon.xRNHCriteria("RoleID", roleId);
            if (xRCommon.xRVerify<Role>(nHcriteria))
            {
                var xRserver = xRCommon.xRCriteria<Exploration>(nHcriteria);
                var xrDict = Utility.Json.ToObject<Dictionary<int, ExplorationItemDTO>>(xRserver.ExplorationItemDict);
                var xrPropDict = Utility.Json.ToObject<Dictionary<int, int>>(xRserver.CatchAndTimeDict);
                foreach (var info in ItemInfo)
                {
                    if (!xrDict.ContainsKey(info.Key))
                        continue;
                    xrDict[info.Key].TimeType -= info.Value.TimeType;
                    NHibernateQuerier.Update(new Exploration() { RoleID = roleId, ExplorationItemDict = Utility.Json.ToJson(xrDict), UnLockDict = xRserver.UnLockDict, CatchAndTimeDict = xRserver.CatchAndTimeDict });
                }

                foreach (var prop in propInfo)  //这个是使用的  减少道具的数量
                {
                    if (!xrPropDict.ContainsKey(prop.Key)) continue;
                    if (xrPropDict[prop.Key] > 0)
                    {
                        xrPropDict[prop.Key] -= 1;
                    }
                    NHibernateQuerier.Update(new Exploration() { RoleID = roleId, ExplorationItemDict = Utility.Json.ToJson(xrDict), UnLockDict = xRserver.UnLockDict, CatchAndTimeDict = Utility.Json.ToJson(xrPropDict) });
                }
                xRGetExploration(roleId);
            }
        }
        /// <summary>
        /// 移除探索   //获取事件奖励
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="ItemInfo"></param>
        public static void xRRemoveExploration(int roleId, Dictionary<int, ExplorationItemDTO> ItemInfo)
        {
            var nHcriteria = xRCommon.xRNHCriteria("RoleID", roleId);
            if (xRCommon.xRVerify<Role>(nHcriteria))
            {
                var xRserver = xRCommon.xRCriteria<Exploration>(nHcriteria);
                var xrDict = Utility.Json.ToObject<Dictionary<int, ExplorationItemDTO>>(xRserver.ExplorationItemDict);
                var xrPropDict = Utility.Json.ToObject<Dictionary<int, int>>(xRserver.CatchAndTimeDict);
                foreach (var info in ItemInfo)
                {
                    if (!xrDict.ContainsKey(info.Key))
                        return;
                    else
                    {
                        GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, ExplorationData>>(out var setExploration);
                        //GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, BattleSkill>>(out var setExploration);
                        foreach (var itemidInfo in xrDict[info.Key].ItemId)
                        {
                            switch (setExploration[itemidInfo.Key].EventType)
                            {
                                case "AddStr":
                                    RoleCricketManager.AptitudeProp(roleId, new PropData() { PropType = (int)RoleCricketManager.PropType.AddStr, AddNumber = itemidInfo.Value }, xrDict[info.Key].CustomId);
                                    break;
                                case "AddCon":
                                    RoleCricketManager.AptitudeProp(roleId, new PropData() { PropType = (int)RoleCricketManager.PropType.AddCon, AddNumber = itemidInfo.Value }, xrDict[info.Key].CustomId);
                                    break;
                                case "AddDex":
                                    RoleCricketManager.AptitudeProp(roleId, new PropData() { PropType = (int)RoleCricketManager.PropType.AddDex, AddNumber = itemidInfo.Value }, xrDict[info.Key].CustomId);
                                    break;
                                case "AddDef":
                                    RoleCricketManager.AptitudeProp(roleId, new PropData() { PropType = (int)RoleCricketManager.PropType.AddDef, AddNumber = itemidInfo.Value }, xrDict[info.Key].CustomId);
                                    break;
                                case "AddAtk":
                                    RoleCricketManager.StatusProp(roleId, new PropData() { PropType = (int)RoleCricketManager.PropType.AddAtk, AddNumber = itemidInfo.Value, }, xrDict[info.Key].CustomId);
                                    break;
                                case "AddHp":
                                    RoleCricketManager.StatusProp(roleId, new PropData() { PropType = (int)RoleCricketManager.PropType.AddHp, AddNumber = itemidInfo.Value, }, xrDict[info.Key].CustomId);
                                    break;
                                case "AddDefense":
                                    RoleCricketManager.StatusProp(roleId, new PropData() { PropType = (int)RoleCricketManager.PropType.AddDefense, AddNumber = itemidInfo.Value, }, xrDict[info.Key].CustomId);
                                    break;
                                case "AddMp":
                                    RoleCricketManager.StatusProp(roleId, new PropData() { PropType = (int)RoleCricketManager.PropType.AddMp, AddNumber = itemidInfo.Value, }, xrDict[info.Key].CustomId);
                                    break;
                                case "AddMpReply":
                                    RoleCricketManager.StatusProp(roleId, new PropData() { PropType = (int)RoleCricketManager.PropType.AddMpReply, AddNumber = itemidInfo.Value, }, xrDict[info.Key].CustomId);
                                    break;
                                case "AddExp":
                                    RoleCricketManager.UpdateLevel(xrDict[info.Key].CustomId, new PropData() { PropID = -1, AddNumber = itemidInfo.Value, }, roleId);
                                    break;
                                case "GetPropA":
                                    int randomCount;
                                    if (setExploration[itemidInfo.Key].Number.Count == 1)
                                    {
                                        randomCount = setExploration[itemidInfo.Key].Number[0];
                                    }
                                    else
                                    {
                                        randomCount = RandomManager(itemidInfo.Key, setExploration[itemidInfo.Key].Number[0], setExploration[itemidInfo.Key].Number[1]);
                                    }
                                    //策划需求道具数量是随机范围，你之前是写死的    这个不应该在这里看 去客户端
                                    if (setExploration[itemidInfo.Key].PropID.Count == 1)
                                        InventoryManager.xRAddInventory(roleId, new Dictionary<int, ItemDTO> { { setExploration[itemidInfo.Key].PropID[0], new ItemDTO() { ItemAmount = itemidInfo.Value } } });
                                    else
                                    {
                                        var randomValue = RandomManager(itemidInfo.Key, setExploration[itemidInfo.Key].PropID[0], setExploration[itemidInfo.Key].PropID[1]);
                                        InventoryManager.xRAddInventory(roleId, new Dictionary<int, ItemDTO> { { randomValue, new ItemDTO() { ItemAmount = itemidInfo.Value } } });
                                    }
                                    //这里原来是这样，你往背包添加的数量是固定一，没有用你那个随机的    上边默认是一个  我就写的一个  改下就好 ，那你记得怎么改吗，你改改  好改 我试试
                                    //现在是这样，这里的道具数量我改了，正常添加，但是任务结算界面显示不到，怎么办    能演示一遍吗
                                    break;
                                case "GetPropB":
                                case "GetPropC"://   区别道具的类型  道具默认+1
                                    if (xrPropDict.ContainsKey(setExploration[itemidInfo.Key].PropID[0]))
                                        xrPropDict[setExploration[itemidInfo.Key].PropID[0]] += 1;
                                    break;
                                case "GetMoney":
                                    BuyPropManager.UpdateRoleAssets(roleId, itemidInfo.Value);
                                    break;
                                case "GetCricket"://全局id
                                    RoleCricketManager.AddCricket(xrDict[info.Key].GlobalId, roleId);
                                    break;
                                case "GetSkill":
                                    RoleCricketManager.AddSpecialSkill(setExploration[itemidInfo.Key].SkillID, 10, roleId, xrDict[info.Key].CustomId);
                                    break;
                            }
                        }
                        xrDict.Remove(info.Key);
                        NHibernateQuerier.Update(new Exploration() { RoleID = roleId, ExplorationItemDict = Utility.Json.ToJson(xrDict), UnLockDict = xRserver.UnLockDict, CatchAndTimeDict = Utility.Json.ToJson(xrPropDict) });
                    }
                }
                xRGetExploration(roleId);
            }
        }


        /// <summary>
        /// 验证是否解锁
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="ItemInfo"></param>
        public static void xRVerifyExploration(int roleId, Dictionary<int, bool> ItemInfo)
        {
            var nHcriteria = xRCommon.xRNHCriteria("RoleID", roleId);
            if (xRCommon.xRVerify<Role>(nHcriteria))
            {
                var xRserver = xRCommon.xRCriteria<Exploration>(nHcriteria);
                var xrDict = Utility.Json.ToObject<Dictionary<int, bool>>(xRserver.UnLockDict);
                foreach (var info in ItemInfo)
                {
                    if (!xrDict.ContainsKey(info.Key))
                    {
                        xrDict[info.Key] = true;
                        xrDict[info.Key + 1] = false;
                    }
                    else
                    {
                        xrDict[info.Key] = true;
                        xrDict[info.Key + 1] = false;
                    }
                    var glod = info.Key == 1 ? 5000 : info.Key == 2 ? 20000 : 50000;
                    BuyPropManager.ExpenseRoleAssets(roleId, glod);
                    NHibernateQuerier.Update(new Exploration() { RoleID = roleId, ExplorationItemDict = xRserver.ExplorationItemDict, UnLockDict = Utility.Json.ToJson(xrDict), CatchAndTimeDict = xRserver.CatchAndTimeDict });
                }
                xRGetExploration(roleId);
            }
        }





        /// <summary>
        /// 针对服务器中的随机数
        /// </summary>
        /// <param name="ov"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static int RandomManager(int ov, int minValue, int maxValue)
        {
            var targetValue = new Random((int)DateTime.Now.Ticks + ov).Next(minValue, maxValue + 1);
            return targetValue;
        }

    }
}
