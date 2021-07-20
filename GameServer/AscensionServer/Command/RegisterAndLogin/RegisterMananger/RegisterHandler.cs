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
    public static class RegisterHandler
    {
        public static void RegisterRole(string account, string password, object peer)
        {
            NHCriteria nHCriteriaAccount = xRCommon.xRNHCriteria("Account", account);
            //Utility.Debug.LogInfo("yzqData发送失败" + nHCriteriaAccount.Value.ToString());

            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, CricketStatus>>(out var cricketStatusDict);
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, Cricket>>(out var cricketDict);
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, HeadPortraitData>>(out var HeadPortraitDict);
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, CricketNameData>>(out var NameDict);
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, CricketHeadPortraitData>>(out var CricketHeadDict);

            bool isExist = xRCommon.xRVerify<User>(nHCriteriaAccount);

            var userObj = new User() { Account = account, Password = password };
            var role = new Role() { };
            var roleAsset = new RoleAssets();
            var cricket = new Cricket();
            var roleCricketObj = new RoleCricketDTO();
            var roleCricket = new RoleCricket();
            var cricketStatus = new CricketStatus();
            var cricketAptitude = new CricketAptitude();
            var cricketPoint = new CricketPoint();
            var cricketAddition = new CricketAddition();
            var spreacode = new SpreaCode();
            if (!isExist)
            {
                userObj = NHibernateQuerier.Insert(userObj);
                NHCriteria nHCriteriaUUID = GameManager.ReferencePoolManager.Spawn<NHCriteria>().SetValue("UUID", userObj.UUID);
                var headList = HeadPortraitDict.Keys.ToList<int>();
                var num = Utility.Algorithm.CreateRandomInt(0, headList.Count);
                Utility.Debug.LogInfo("头像ID"+HeadPortraitDict[headList[num]].PlayerHeadID);
                role.HeadPortrait = HeadPortraitDict[headList[num]].PlayerHeadID;
                role = NHibernateQuerier.Insert(role);
                userObj.RoleID = role.RoleID;
                NHibernateQuerier.Update(userObj);
                roleAsset.RoleID = role.RoleID;
                NHibernateQuerier.Insert(roleAsset);
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
                Utility.Debug.LogInfo("yzqData发送成功" + Utility.Json.ToJson(cricketStatus));
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
                NHibernateQuerier.Insert(cricketPoint);
                cricketAddition.CricketID = cricket.ID;
                NHibernateQuerier.Insert(cricketAddition);
                #endregion
                #region 插入背包和每日任务 以及探索  以及 战斗结束表格
                NHibernateQuerier.Insert(new BattleCombat() { RoleID = role.RoleID});
                NHibernateQuerier.Insert(new Inventory() { RoleID = role.RoleID });
                InventoryManager.xRAddInventory(role.RoleID, new Dictionary<int, ItemDTO> { { 1201, new ItemDTO() { ItemAmount = 1 } }, { 1001, new ItemDTO() { ItemAmount = 1 } } });
                NHibernateQuerier.Insert(new Exploration() { RoleID = role.RoleID });
                ExplorationManager.xRAddExploration(role.RoleID, new Dictionary<int, ExplorationItemDTO>(), new Dictionary<int, int> { { 1901, 1 }, { 1902, 1 }, { 1903, 1 }, { 1801, 10 }, { 1802, 10 }, { 1803, 10 }, { 1804, 2 } });
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
                OperationData operationData = new OperationData();
                operationData.DataMessage = Utility.Json.ToJson(role);
                operationData.ReturnCode = (byte)ReturnCode.Success;
                //operationData.OperationCode = (ushort)ATCmd.Register;
                GameManager.CustomeModule<PeerManager>().SendMessage((peer as IPeerEntity).SessionId, operationData);
            }
            else
            {
                OperationData operationData = new OperationData();
                operationData.DataMessage = "账号已存在";
                operationData.ReturnCode = (byte)ReturnCode.Fail;
                //operationData.OperationCode = (ushort)ATCmd.Register;
                Utility.Debug.LogInfo("yzqData发送失败");
                GameManager.CustomeModule<PeerManager>().SendMessage((peer as IPeerEntity).SessionId, operationData);
            }
        }


    }
}
