using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using Protocol;
using AscensionProtocol;
using Cosmos.Reference;

namespace AscensionServer
{
   public static  class LoginHandler
    {
        //public static void LoginRole(string account, string password,object peer)
        //{
        //    NHCriteria nHCriteriauser = xRCommon.xRNHCriteria("Account", account);
        //    var user = xRCommon.xRCriteria<User>(nHCriteriauser);
        //    if (user != null)
        //    {
        //        if (user.Password == password)
        //        {
        //            NHCriteria nHCriteriaRole = xRCommon.xRNHCriteria("RoleID", user.RoleID);
        //            var role = xRCommon.xRCriteria<Role>(nHCriteriaRole);
        //            var roleAsset = xRCommon.xRCriteria<RoleAssets>(nHCriteriaRole);
        //            var roleCricket = xRCommon.xRCriteria<RoleCricket>(nHCriteriaRole);
        //            var battleCombat = xRCommon.xRCriteria<BattleCombat>(nHCriteriaRole);
        //            RoleCricketDTO roleCricketDTO = new RoleCricketDTO();
        //            roleCricketDTO.RoleID = roleCricket.RoleID;
        //            roleCricketDTO.CricketList = Utility.Json.ToObject<List<int>>(roleCricket.CricketList);
        //            roleCricketDTO.TemporaryCrickets = Utility.Json.ToObject<List<int>>(roleCricket.TemporaryCrickets);

        //            Dictionary<byte, string> dataDict = new Dictionary<byte, string>();
        //            dataDict.Add((byte)ParameterCode.Role, Utility.Json.ToJson(role));
        //            dataDict.Add((byte)ParameterCode.RoleAsset, Utility.Json.ToJson(roleAsset));
        //            dataDict.Add((byte)ParameterCode.RoleCricket, Utility.Json.ToJson(roleCricketDTO));
        //            dataDict.Add((byte)ParameterCode.RoleBattleCombat, Utility.Json.ToJson(battleCombat));
        //            #region 
        //            var result = GameManager.CustomeModule<RoleManager>().TryGetValue(user.RoleID, out var roleEntity);
        //            Utility.Debug.LogError(result);
        //            if (result)//角色已存在，表示在线
        //            {
        //                //挤下旧的登录
        //                IPeerEntity oldPeerEntity;
        //                GameManager.CustomeModule<RoleManager>().TryRemove(roleEntity.RoleId);
        //                if (GameManager.CustomeModule<PeerManager>().TryGetValue(roleEntity.SessionId, out oldPeerEntity))
        //                {
        //                    OperationData operationData = new OperationData();
        //                    operationData.DataMessage = "账号在别处登录";
        //                    operationData.ReturnCode = (short)ReturnCode.Success;
        //                    operationData.OperationCode = (ushort)ATCmd.Logoff;
        //                    oldPeerEntity.SendMessage(operationData); ;

        //                    oldPeerEntity.TryRemove(typeof(RoleEntity));
        //                    //GameManager.CustomeModule<LoginManager>().S2CLoginOff(roleEntity.SessionId, "账号在别处登录", ReturnCode.Success);


        //                    Utility.Debug.LogError("挤下账号=>" + oldPeerEntity.SessionId);
        //                }
        //            }
        //            //创建新的登录
        //            roleEntity = RoleEntity.Create(role.RoleID, (peer as IPeerEntity).SessionId, role);
        //            GameManager.CustomeModule<RoleManager>().TryAdd(roleEntity.RoleId, roleEntity);
        //            IPeerEntity peerEntity;
        //            var isPeerExist = GameManager.CustomeModule<PeerManager>().TryGetValue((peer as IPeerEntity).SessionId, out peerEntity);
        //            if (isPeerExist)
        //            {
        //                peerEntity.TryAdd(typeof(RoleEntity), roleEntity);
        //                Utility.Debug.LogError("yzqData登录成功RoleID:" + roleEntity.SessionId);
        //                GameManager.CustomeModule<LoginManager>().S2CLogin((peer as IPeerEntity).SessionId, Utility.Json.ToJson(dataDict), ReturnCode.Success);
        //            }

        //            //var roleEntity = RoleEntity.Create(role.RoleID, (peer as IPeerEntity).SessionId, role);
        //            //IPeerEntity peerAgent;
        //            //var result = GameManager.CustomeModule<PeerManager>().TryGetValue((peer as IPeerEntity).SessionId, out peerAgent);
        //            //if (result)
        //            //{
        //            //    var remoteRoleType = typeof(RoleEntity);
        //            //    var exist = peerAgent.ContainsKey(remoteRoleType);
        //            //    if (!exist)
        //            //    {
        //            //        //GameManager.CustomeModule<RoleManager>().TryRemove(role.RoleID);
        //            //        var isture = GameManager.CustomeModule<RoleManager>().TryAdd(role.RoleID, roleEntity);
        //            //        peerAgent.TryAdd(remoteRoleType, roleEntity);
        //            //        Utility.Debug.LogError("yzqData登录成功RoleID:" + role.RoleID + isture);
        //            //        GameManager.CustomeModule<LoginManager>().S2CLogin((peer as IPeerEntity).SessionId, Utility.Json.ToJson(dataDict), ReturnCode.Success);
        //            //    }
        //            //    else
        //            //    {
        //            //        //TODO提示账号已在线阻止登陆
        //            //        GameManager.CustomeModule<RoleManager>().TryGetValue(role.RoleID, out var oldRoleEntityObj);
        //            //        RoleEntity oldRoleEntity = oldRoleEntityObj as RoleEntity;
        //            //        GameManager.CustomeModule<LoginManager>().S2CLoginOff(oldRoleEntity.SessionId, "账号在别处登录", ReturnCode.Success);
        //            //        GameManager.CustomeModule<RoleManager>().TryRemove(oldRoleEntity.RoleId);
        //            //        GameManager.CustomeModule<RoleManager>().TryAdd(role.RoleID, roleEntity);
        //            //        peerAgent.TryAdd(typeof(RoleEntity), roleEntity);
        //            //        GameManager.CustomeModule<LoginManager>().S2CLogin((peer as IPeerEntity).SessionId, Utility.Json.ToJson(dataDict), ReturnCode.Success);
        //            //        Utility.Debug.LogError(oldRoleEntity.SessionId + "被挤下", (peer as IPeerEntity).SessionId + "重新登录");
        //            //    }
        //            //}
        //            #endregion
        //        }
        //    }
        //    else
        //    {
        //        GameManager.CustomeModule<LoginManager>().S2CLogin((peer as IPeerEntity).SessionId);
        //    }
        //}

        public static void LoginOffRole(Role role, object peer)
        {
            IPeerEntity peerAgent;
            var result = GameManager.CustomeModule<PeerManager>().TryGetValue((peer as IPeerEntity).SessionId, out peerAgent);
            if (result)
            {
                var exist = peerAgent.ContainsKey(typeof(RoleEntity));
                if (exist)//退出登录成功
                {
                    GameManager.CustomeModule<RoleManager>().TryRemove(role.RoleID);
                    peerAgent.TryRemove(typeof(RoleEntity));
                    Utility.Debug.LogError("退出登录成功");
                    GameManager.CustomeModule<LoginManager>().S2CLoginOff((peer as IPeerEntity).SessionId, "退出登录成功", ReturnCode.Success);
                }
                else//账号未登录
                {
                    Utility.Debug.LogError("退出登录失败");
                    GameManager.CustomeModule<LoginManager>().S2CLoginOff((peer as IPeerEntity).SessionId, "退出登录失败", ReturnCode.Success);
                }
            }
        }

        /// <summary>
        /// TapTap登录
        /// </summary>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <param name="peer"></param>
        public static void TapTapLoginRole(string uuid, string name, object peer)
        {
            NHCriteria nHCriteriauser = xRCommon.xRNHCriteria("UUID", uuid);
            var userObj = xRCommon.xRCriteria<User>(nHCriteriauser);
            if (userObj != null)
            {
                NHCriteria nHCriteriaRole = xRCommon.xRNHCriteria("RoleID", userObj.RoleID);
                var role = xRCommon.xRCriteria<Role>(nHCriteriaRole);
                var roleAsset = xRCommon.xRCriteria<RoleAssets>(nHCriteriaRole);
                var roleCricket = xRCommon.xRCriteria<RoleCricket>(nHCriteriaRole);
                var battleCombat = xRCommon.xRCriteria<BattleCombat>(nHCriteriaRole);
                RoleCricketDTO roleCricketDTO = new RoleCricketDTO();
                roleCricketDTO.RoleID = roleCricket.RoleID;
                roleCricketDTO.CricketList = Utility.Json.ToObject<List<int>>(roleCricket.CricketList);
                roleCricketDTO.TemporaryCrickets = Utility.Json.ToObject<List<int>>(roleCricket.TemporaryCrickets);

                Dictionary<byte, string> dataDict = new Dictionary<byte, string>();
                dataDict.Add((byte)ParameterCode.Role, Utility.Json.ToJson(role));
                dataDict.Add((byte)ParameterCode.RoleAsset, Utility.Json.ToJson(roleAsset));
                dataDict.Add((byte)ParameterCode.RoleCricket, Utility.Json.ToJson(roleCricketDTO));
                dataDict.Add((byte)ParameterCode.RoleBattleCombat, Utility.Json.ToJson(battleCombat));
                #region 
                var result = GameManager.CustomeModule<RoleManager>().TryGetValue(userObj.RoleID, out var roleEntity);
                Utility.Debug.LogError(result);
                if (result)//角色已存在，表示在线
                {
                    //挤下旧的登录
                    IPeerEntity oldPeerEntity;
                    GameManager.CustomeModule<RoleManager>().TryRemove(roleEntity.RoleId);
                    if (GameManager.CustomeModule<PeerManager>().TryGetValue(roleEntity.SessionId, out oldPeerEntity))
                    {
                        OperationData operationData = new OperationData();
                        operationData.DataMessage = "账号在别处登录";
                        operationData.ReturnCode = (short)ReturnCode.Success;
                        operationData.OperationCode = (ushort)ATCmd.Logoff;
                        oldPeerEntity.SendMessage(operationData); ;

                        oldPeerEntity.TryRemove(typeof(RoleEntity));

                        Utility.Debug.LogError("挤下账号=>" + oldPeerEntity.SessionId);
                    }
                }
                //创建新的登录
                roleEntity = RoleEntity.Create(role.RoleID, (peer as IPeerEntity).SessionId, role);
                GameManager.CustomeModule<RoleManager>().TryAdd(roleEntity.RoleId, roleEntity);
                IPeerEntity peerEntity;
                var isPeerExist = GameManager.CustomeModule<PeerManager>().TryGetValue((peer as IPeerEntity).SessionId, out peerEntity);
                if (isPeerExist)
                {
                    peerEntity.TryAdd(typeof(RoleEntity), roleEntity);
                    Utility.Debug.LogError("yzqData登录成功RoleID:" + roleEntity.SessionId);
                    GameManager.CustomeModule<LoginManager>().S2CLogin((peer as IPeerEntity).SessionId, Utility.Json.ToJson(dataDict), ReturnCode.Success);
                }

                #endregion
            }
            else
            {
                Utility.Debug.LogError("yzqData进入注册逻辑:");
                RegisterRole(uuid,name,  peer);
            }
        }

        /// <summary>
        /// 自动注册逻辑
        /// </summary>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <param name="peer"></param>
        public static void RegisterRole(string uuid,string name, object peer)
        {
            NHCriteria nHCriteriaAccount = xRCommon.xRNHCriteria("UUID", uuid);
            //Utility.Debug.LogInfo("yzqData发送失败" + nHCriteriaAccount.Value.ToString());

            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, CricketStatus>>(out var cricketStatusDict);
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, Cricket>>(out var cricketDict);
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, HeadPortraitData>>(out var HeadPortraitDict);
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, CricketNameData>>(out var NameDict);
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, CricketHeadPortraitData>>(out var CricketHeadDict);

            var userObj = new User() { UUID=uuid};
            var role = new Role() {  };
            var roleAsset = new RoleAssets();
            var cricket = new Cricket();
            var roleCricketObj = new RoleCricketDTO();
            var roleCricket = new RoleCricket();
            var cricketStatus = new CricketStatus();
            var cricketAptitude = new CricketAptitude();
            var cricketPoint = new CricketPoint();
            var cricketAddition = new CricketAddition();
            var spreacode = new SpreaCode();
            #region 注册逻辑
            var headList = HeadPortraitDict.Keys.ToList<int>();
            var num = Utility.Algorithm.CreateRandomInt(0, headList.Count);
            Utility.Debug.LogInfo("头像ID" + HeadPortraitDict[headList[num]].PlayerHeadID);
            role.HeadPortrait = HeadPortraitDict[headList[num]].PlayerHeadID;
            role = NHibernateQuerier.Insert(role);
            if (role.RoleID!=0)
            {
                role.RoleName = name;
                userObj = NHibernateQuerier.Insert(userObj);
                roleAsset.RoleID = role.RoleID;
                NHibernateQuerier.Insert(roleAsset);
                userObj.RoleID = role.RoleID;
                NHibernateQuerier.Update(userObj);
                #region 待换
                cricket.Roleid = role.RoleID;
                #region
                var headlist = CricketHeadDict.Keys.ToList<int>();
                var headnum = Utility.Algorithm.CreateRandomInt(0, headlist.Count);
                cricket.HeadPortraitID = CricketHeadDict[headlist[headnum]].CricketID;
                var namelist = NameDict.Keys.ToList<int>();
                var namenum = Utility.Algorithm.CreateRandomInt(0, namelist.Count);
                cricket.CricketName = NameDict[namelist[namenum]].CricketName;

                #endregion
                cricket = NHibernateQuerier.Insert(cricket);
                roleCricketObj.CricketList[0] = cricket.ID;
                roleCricket.RoleID = role.RoleID;
                roleCricket.CricketList = Utility.Json.ToJson(roleCricketObj.CricketList);
                roleCricket.TemporaryCrickets = Utility.Json.ToJson(roleCricketObj.TemporaryCrickets);
                NHibernateQuerier.Insert(roleCricket);
                //cricketStatus= RoleCricketManager.CalculateStutas(cricketAptitude, cricketPoint, cricketAddition);
                cricketStatus = RoleCricketManager.SkillAdditionStatus(cricket, cricketAptitude, cricketPoint, cricketAddition, out var cricketPointTemp);
                Utility.Debug.LogInfo("yzqData发送成功" + (peer as IPeerEntity).SessionId);
                cricketStatus.CricketID = cricket.ID;
                NHibernateQuerier.Insert(cricketStatus);
                cricketAptitude.CricketID = cricket.ID;
                #region 
                cricketAptitude.SkillStr = cricketPointTemp.Str;
                cricketAptitude.SkillCon = cricketPointTemp.Con;
                cricketAptitude.SkillDef = cricketPointTemp.Def;
                cricketAptitude.SkillDex = cricketPointTemp.Dex;
                #endregion
                NHibernateQuerier.Insert(cricketAptitude);
                cricketPoint.CricketID = cricket.ID;
                cricketPoint.FreePoint = 5;
                NHibernateQuerier.Insert(cricketPoint);
                cricketAddition.CricketID = cricket.ID;
                NHibernateQuerier.Insert(cricketAddition);
                #endregion
                var battleCombat = NHibernateQuerier.Insert(new BattleCombat() { RoleID = role.RoleID });
                #region 插入背包和每日任务 以及探索  以及 战斗结束表格
                NHibernateQuerier.Insert(new Inventory() { RoleID = role.RoleID });
                InventoryManager.xRRegisterAddInventory(role.RoleID, new Dictionary<int, ItemDTO> { { 1201, new ItemDTO() { ItemAmount = 1 } }, { 1001, new ItemDTO() { ItemAmount = 1 } } });
                NHibernateQuerier.Insert(new Exploration() { RoleID = role.RoleID });
                ExplorationManager.xRRegisterAddExploration(role.RoleID, new Dictionary<int, ExplorationItemDTO>(), new Dictionary<int, int> { { 1901, 1 }, { 1902, 1 }, { 1903, 1 }, { 1801, 10 }, { 1802, 10 }, { 1803, 10 }, { 1804, 2 } });
                #endregion
                #region 推广初始化
                spreacode.RoleID = role.RoleID;
                spreacode.CodeID = GameManager.CustomeModule<SpreaCodeManager>().RandomCodeID(role.RoleID);
                var dict = new Dictionary<int, int>() { };
                dict.Add(6002, -1);//10人奖励
                dict.Add(6003, -1);//30人奖励
                dict.Add(6004, -1);//50人奖励
                dict.Add(6005, -1);//100人奖励
                spreacode.SpreaPlayers = Utility.Json.ToJson(dict);
                NHibernateQuerier.Insert(spreacode);
                #endregion
                #region 爬塔初始化
                NHibernateQuerier.Insert(new Tower() { RoleID = role.RoleID });
                #endregion
                RoleCricketDTO roleCricketDTO = new RoleCricketDTO();
                roleCricketDTO.RoleID = roleCricket.RoleID;
                roleCricketDTO.CricketList = Utility.Json.ToObject<List<int>>(roleCricket.CricketList);
                roleCricketDTO.TemporaryCrickets = Utility.Json.ToObject<List<int>>(roleCricket.TemporaryCrickets);
                Dictionary<byte, string> dataDict = new Dictionary<byte, string>();
                dataDict.Add((byte)ParameterCode.Role, Utility.Json.ToJson(role));
                dataDict.Add((byte)ParameterCode.RoleAsset, Utility.Json.ToJson(roleAsset));
                dataDict.Add((byte)ParameterCode.RoleCricket, Utility.Json.ToJson(roleCricketDTO));
                dataDict.Add((byte)ParameterCode.RoleBattleCombat, Utility.Json.ToJson(battleCombat));
                #region 

                //创建新的登录
                var roleEntity = RoleEntity.Create(role.RoleID, (peer as IPeerEntity).SessionId, role);
                GameManager.CustomeModule<RoleManager>().TryAdd(roleEntity.RoleId, roleEntity);
                IPeerEntity peerEntity;
                var isPeerExist = GameManager.CustomeModule<PeerManager>().TryGetValue((peer as IPeerEntity).SessionId, out peerEntity);
                if (isPeerExist)
                {
                    peerEntity.TryAdd(typeof(RoleEntity), roleEntity);
                    Utility.Debug.LogError("yzqData登录成功RoleID:" + roleEntity.SessionId);
                    GameManager.CustomeModule<LoginManager>().S2CLogin((peer as IPeerEntity).SessionId, Utility.Json.ToJson(dataDict), ReturnCode.Success);
                }

                #endregion
                #endregion
            }else
                GameManager.CustomeModule<LoginManager>().S2CLogin((peer as IPeerEntity).SessionId, null, ReturnCode.Fail);
        }

    }
}
