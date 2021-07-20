using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using AscensionProtocol;
using Protocol;
namespace AscensionServer
{
    public partial class SpreaCodeManager
    {
        /// <summary>
        ///获取玩家邀请信息
        /// </summary>
        /// <param name="roleid"></param>
        public void GetSpreaCode(int roleid)
        {
            var nHCriteriaRole = xRCommon.xRNHCriteria("RoleID", roleid);
            var spreaCode = xRCommon.xRCriteria<SpreaCode>(nHCriteriaRole);

            //获取玩家信息
            var headPortrait = NHibernateQuerier.GetTable<Role>();
            Dictionary<int, Role> getHeadDic = headPortrait.ToDictionary(key => key.RoleID, value => value);
            //新建一个字典用来存取玩家的昵称所对应的头像id
            Dictionary<int, int> playerHead = new Dictionary<int, int>();

            var roleDict = Utility.Json.ToObject<Dictionary<int, List<int>>>(spreaCode.SpreaLevel);
            var roleList = roleDict.Keys.ToList();

            var roleCickets = new List<RoleCricket>();
            for (int i = 0; i < roleList.Count; i++)
            {
                //把玩家的id和对应的头像id存入字典中
                playerHead.Add(roleList[i], getHeadDic[roleList[i]].HeadPortrait);

                var nHCriteria = xRCommon.xRNHCriteria("RoleID", roleList[i]);
                roleCickets.Add(xRCommon.xRCriteria<RoleCricket>(nHCriteria));
            }

            //Utility.Debug.LogError("邀请的玩家ID" + Utility.Json.ToJson(roleCickets));
            for (int i = 0; i < roleCickets.Count; i++)
            {
                var crickets = Utility.Json.ToObject<List<int>>(roleCickets[i].CricketList);
                var cricketList = new List<Cricket>();
                for (int j = 0; j < crickets.Count; j++)
                {
                    if (crickets[j] != -1)
                    {
                        var nHCriterias = xRCommon.xRNHCriteria("ID", crickets[j]);
                        cricketList.Add(xRCommon.xRCriteria<Cricket>(nHCriterias));
                    }
                }
                if (cricketList.Count > 0)
                {
                    var levelList = cricketList.OrderByDescending(o => o.LevelID).ToList();
                    if (levelList[0].LevelID >= 10)
                    {
                        if (roleDict[roleCickets[i].RoleID][0] == -1)
                        {
                            roleDict[roleCickets[i].RoleID][0] = 0;
                        }
                    }

                    if (levelList[0].LevelID >= 20)
                    {
                        if (roleDict[roleCickets[i].RoleID][1] == -1)
                        {
                            roleDict[roleCickets[i].RoleID][1] = 0;
                        }
                    }

                    if (levelList[0].LevelID >= 30)
                    {
                        if (roleDict[roleCickets[i].RoleID][2] == -1)
                        {
                            roleDict[roleCickets[i].RoleID][2] = 0;
                        }
                    }
                }
            }
            var numDict = Utility.Json.ToObject<Dictionary<int, int>>(spreaCode.SpreaPlayers);
            if (spreaCode.SpreaNum >= 1)
            {
                if (numDict[6002] == -1)
                {
                    numDict[6002] = 0;
                }
            }
            if (spreaCode.SpreaNum >= 3)
            {
                if (numDict[6003] == -1)
                {
                    numDict[6003] = 0;
                }
            }
            if (spreaCode.SpreaNum >= 5)
            {
                if (numDict[6004] == -1)
                {
                    numDict[6004] = 0;
                }
            }
            if (spreaCode.SpreaNum >= 10)
            {
                if (numDict[6005] == -1)
                {
                    numDict[6005] = 0;
                }
            }

            spreaCode.SpreaPlayers = Utility.Json.ToJson(numDict);
            spreaCode.SpreaLevel = Utility.Json.ToJson(roleDict);
            spreaCode.PlayerHeadPortrait = Utility.Json.ToJson(playerHead);


            NHibernateQuerier.Update(spreaCode);
            var spreaCodeDTO = GiveValue(spreaCode, numDict, roleDict, playerHead);
            var dict = xRCommon.xRS2CParams();
            dict.Add((byte)SpreaCodeOperateType.Get, Utility.Json.ToJson(spreaCodeDTO));
            xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncSpreaCode, (short)ReturnCode.Success, dict);
        }
        /// <summary>
        /// 输入玩家邀请码
        /// </summary>
        public void InputSpreaCode(int roleid, int codeid, int awardid = 6001)
        {
            var nHCriteriaRole = xRCommon.xRNHCriteria("CodeID", codeid);
            var spreaCode = xRCommon.xRCriteria<SpreaCode>(nHCriteriaRole);

            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, SpreaAward>>(out var spreaAwardDict);

            if (spreaCode != null)
            {
                spreaCode.SpreaNum += 1;
                var dict = Utility.Json.ToObject<Dictionary<int, List<int>>>(spreaCode.SpreaLevel);
                dict.Add(roleid, new List<int>() { -1, -1, -1, 0 });
                spreaCode.SpreaLevel = Utility.Json.ToJson(dict);
                NHibernateQuerier.Update(spreaCode);
                var dataDict = xRCommon.xRS2CSub();
                dataDict.Add((byte)SpreaCodeOperateType.Input, xRCommonTip.xR_tip_InputSpreaCode);
                xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncSpreaCode, (short)ReturnCode.Success, dataDict);
            }
            else
            {
                //返回失败
                xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncSpreaCode, (short)ReturnCode.Fail, xRCommonTip.xR_err_VerifySpreaCode);
                return;
            }
            var result = spreaAwardDict.TryGetValue(awardid, out var spreaAward);
            if (result)
            {
                BuyPropManager.UpdateRoleAssets(roleid, spreaAward.Money);
                RoleCricketManager.AddCricket(0, roleid, spreaAward.Cricket);
                if (spreaAward.AwardType == 0)
                {
                    for (int i = 0; i < spreaAward.PropID.Count; i++)
                    {
                        InventoryManager.xRAddInventory(roleid, new Dictionary<int, ItemDTO>() { { spreaAward.PropID[i], new ItemDTO() { ItemAmount = spreaAward.PropNumber[i] } } });
                    }
                }
            }


        }
        /// <summary>
        ///领取推广人数奖励
        /// </summary>
        /// <param name="spreaCodeDTO"></param>
        public void ReceiveNumAward(SpreaCodeDTO spreaCodeDTO)
        {
            Utility.Debug.LogInfo("领取人数奖励" + Utility.Json.ToJson(spreaCodeDTO));
            var nHCriteriaRole = xRCommon.xRNHCriteria("RoleID", spreaCodeDTO.RoleID);
            var spreaCode = xRCommon.xRCriteria<SpreaCode>(nHCriteriaRole);

            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, SpreaAward>>(out var spreaAwardDict);

            var numDict = Utility.Json.ToObject<Dictionary<int, int>>(spreaCode.SpreaPlayers);
            var levelDict = Utility.Json.ToObject<Dictionary<int, List<int>>>(spreaCode.SpreaLevel);

            //获取玩家信息
            var headPortrait = NHibernateQuerier.GetTable<Role>();
            Dictionary<int, Role> getHeadDic = headPortrait.ToDictionary(key => key.RoleID, value => value);
            //新建一个字典用来存取玩家的昵称所对应的头像id
            Dictionary<int, int> playerHead = new Dictionary<int, int>();
            for (int i = 0; i < levelDict.Keys.ToList().Count; i++)
            {
                playerHead.Add(levelDict.Keys.ToList()[i], getHeadDic[levelDict.Keys.ToList()[i]].HeadPortrait);
            }

            foreach (var item in spreaCodeDTO.SpreaPlayers)
            {
                if (numDict.ContainsKey(item.Key))
                {
                    if (numDict[item.Key] == 0 && item.Value == 1)
                    {
                        numDict[item.Key] = 1;
                        var result = spreaAwardDict.TryGetValue(item.Key, out var spreaAward);
                        if (result)
                        {
                            for (int i = 0; i < spreaAward.PropID.Count; i++)
                            {
                                InventoryManager.xRAddInventory(spreaCodeDTO.RoleID, new Dictionary<int, ItemDTO>() { { spreaAwardDict[item.Key].PropID[i], new ItemDTO() { ItemAmount = spreaAwardDict[item.Key].PropNumber[i] } } });
                            }
                            BuyPropManager.UpdateRoleAssets(spreaCodeDTO.RoleID, spreaAwardDict[item.Key].Money);
                            RoleCricketManager.AddCricket(0, spreaCodeDTO.RoleID, spreaAward.Cricket);
                        }
                    }
                    else
                    {
                        xRCommon.xRS2CSend(spreaCodeDTO.RoleID, (ushort)ATCmd.SyncSpreaCode, (short)ReturnCode.Fail, xRCommonTip.xR_err_VerifyAward);
                        return;
                    }
                }
                else
                {
                    xRCommon.xRS2CSend(spreaCodeDTO.RoleID, (ushort)ATCmd.SyncSpreaCode, (short)ReturnCode.Fail, xRCommonTip.xR_err_Verify);
                    return;
                }
            }
            spreaCode.SpreaLevel = Utility.Json.ToJson(levelDict);
            spreaCode.SpreaPlayers = Utility.Json.ToJson(numDict);
            spreaCode.PlayerHeadPortrait = Utility.Json.ToJson(playerHead);
            NHibernateQuerier.Update(spreaCode);
            var spreaCodeObj = GiveValue(spreaCode, numDict, levelDict, playerHead);
            var data = xRCommon.xRS2CParams();
            data.Add((byte)SpreaCodeOperateType.Get, Utility.Json.ToJson(spreaCodeObj));
            xRCommon.xRS2CSend(spreaCodeDTO.RoleID, (ushort)ATCmd.SyncSpreaCode, (short)ReturnCode.Success, data);
        }
        /// <summary>
        /// 领取推广人物等级奖励
        /// </summary>
        /// <param name="spreaCodeDTO"></param>
        public void ReceiveLevelAwar(SpreaCodeDTO spreaCodeDTO)
        {
            var nHCriteriaRole = xRCommon.xRNHCriteria("RoleID", spreaCodeDTO.RoleID);
            var spreaCode = xRCommon.xRCriteria<SpreaCode>(nHCriteriaRole);

            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, SpreaAward>>(out var spreaAwardDict);

            var numDict = Utility.Json.ToObject<Dictionary<int, int>>(spreaCode.SpreaPlayers);
            var levelDict = Utility.Json.ToObject<Dictionary<int, List<int>>>(spreaCode.SpreaLevel);

            //获取玩家信息
            var headPortrait = NHibernateQuerier.GetTable<Role>();
            Dictionary<int, Role> getHeadDic = headPortrait.ToDictionary(key => key.RoleID, value => value);
            //新建一个字典用来存取玩家的昵称所对应的头像id
            Dictionary<int, int> playerHead = new Dictionary<int, int>();

            for (int i = 0; i < levelDict.Keys.ToList().Count; i++)
            {
                playerHead.Add(levelDict.Keys.ToList()[i], getHeadDic[levelDict.Keys.ToList()[i]].HeadPortrait);
            }
            foreach (var item in spreaCodeDTO.SpreaLevel)
            {
                if (levelDict.ContainsKey(item.Key))
                {
                    if (levelDict[item.Key][spreaCodeDTO.AwardID] == 0)
                    {
                        levelDict[item.Key][spreaCodeDTO.AwardID] = 1;
                        if (spreaCodeDTO.AwardID == 0)
                        {
                            spreaCodeDTO.AwardID = 6006;//推广玩家等级10级奖励
                        }
                        if (spreaCodeDTO.AwardID == 1)
                        {
                            spreaCodeDTO.AwardID = 6007;//推广玩家等级20级奖励
                        }
                        if (spreaCodeDTO.AwardID == 2)
                        {
                            spreaCodeDTO.AwardID = 6008;//推广玩家等级30级奖励
                        }
                        if (spreaCodeDTO.AwardID == 3)
                        {
                            spreaCodeDTO.AwardID = 6009;//推广玩家登录奖励
                        }
                        var result = spreaAwardDict.TryGetValue(spreaCodeDTO.AwardID, out var spreaAward);
                        for (int i = 0; i < spreaAward.PropID.Count; i++)
                        {
                            InventoryManager.xRAddInventory(spreaCodeDTO.RoleID, new Dictionary<int, ItemDTO>() { { spreaAward.PropID[i], new ItemDTO() { ItemAmount = spreaAward.PropNumber[i] } } });
                        }
                        BuyPropManager.UpdateRoleAssets(spreaCodeDTO.RoleID, spreaAward.Money);
                        for (int i = 0; i < spreaAward.ExploreProp.Count; i++)
                        {
                            ExplorationManager.xRAddExploration(spreaCodeDTO.RoleID, null, new Dictionary<int, int>() { { spreaAward.ExploreProp[i], spreaAward.ExploreNumber[i] } });
                        }

                    }
                }
            }
            spreaCode.SpreaLevel = Utility.Json.ToJson(levelDict);
            spreaCode.SpreaPlayers = Utility.Json.ToJson(numDict);
            spreaCode.PlayerHeadPortrait = Utility.Json.ToJson(playerHead);
            NHibernateQuerier.Update(spreaCode);
            var spreaCodeObj = GiveValue(spreaCode, numDict, levelDict, playerHead);
            var data = xRCommon.xRS2CParams();
            data.Add((byte)SpreaCodeOperateType.Get, Utility.Json.ToJson(spreaCodeObj));
            xRCommon.xRS2CSend(spreaCodeDTO.RoleID, (ushort)ATCmd.SyncSpreaCode, (short)ReturnCode.Success, data);
        }
        /// <summary>
        /// 映射赋值传输类
        /// </summary>
        /// <param name="spreaCode"></param>
        /// <param name="dict"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public SpreaCodeDTO GiveValue(SpreaCode spreaCode, Dictionary<int, int> dict, Dictionary<int, List<int>> data, Dictionary<int, int> headDic)
        {
            SpreaCodeDTO spreaCodeDTO = new SpreaCodeDTO();
            spreaCodeDTO.RoleID = spreaCode.RoleID;
            spreaCodeDTO.CodeID = spreaCode.CodeID;
            spreaCodeDTO.SpreaLevel = data;
            spreaCodeDTO.SpreaPlayers = dict;
            spreaCodeDTO.SpreaNum = spreaCode.SpreaNum;
            spreaCodeDTO.PlayerHeadPortrait = headDic;
            return spreaCodeDTO;
        }
    }
}
