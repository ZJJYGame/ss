using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol;
using Protocol;
using Cosmos;
namespace AscensionServer
{
   public static class BuyPropManager
    {
        public static void BuyProp(RolepPropDTO roleShopDTO)
        {
            NHCriteria nHCriteria = xRCommon.xRNHCriteria("RoleID", roleShopDTO.RoleID);
            var roleAssets = xRCommon.xRCriteria<RoleAssets>(nHCriteria);
            Utility.Debug.LogInfo("YZQ數據庫映射"+Utility.Json.ToJson(roleAssets));
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, Shop>>(out var shopDict);

            Utility.Debug.LogInfo("YZQ數據庫映射1" + roleShopDTO.PropNum);
            Utility.Debug.LogInfo("YZQ數據庫映射2" + shopDict.Count);
            if (roleAssets.RoleGold >= (shopDict[roleShopDTO.PropID].PropPrice* roleShopDTO.PropNum))
            {
                Utility.Debug.LogInfo("YZQData" + Utility.Json.ToJson(roleShopDTO) + "商店数据" + Utility.Json.ToJson(shopDict));
                roleAssets.RoleGold -= (shopDict[roleShopDTO.PropID].PropPrice * roleShopDTO.PropNum);

                if (shopDict[roleShopDTO.PropID].PropType==0)
                {
                    Dictionary<int, ItemDTO> itemDict = new Dictionary<int, ItemDTO>();
                    ItemDTO itemDTO = new ItemDTO();
                    itemDTO.ItemAmount = roleShopDTO.PropNum;
                    itemDict.Add(roleShopDTO.PropID, itemDTO);
                    InventoryManager.xRAddInventory(roleShopDTO.RoleID, itemDict);
                }
                else
                {
                    ExplorationManager.xRAddExploration(roleShopDTO.RoleID, null, new Dictionary<int, int>() { { roleShopDTO.PropID, roleShopDTO.PropNum } });
                }
                NHibernateQuerier.Update(roleAssets);


                var dataDict= xRCommon.xRS2CParams();
                dataDict.Add((byte)ParameterCode.RoleAsset, roleAssets);
                var dict = xRCommon.xRS2CSub();
                dict.Add((byte)ShopOperate.Buy, Utility.Json.ToJson(dataDict));
                Utility.Debug.LogInfo("YZQData"+Utility.Json.ToJson(dataDict));
                xRCommon.xRS2CSend(roleShopDTO.RoleID,(ushort)ATCmd.SyncShop,(byte)ReturnCode.Success, dict);
            }else
                xRCommon.xRS2CSend(roleShopDTO.RoleID, (ushort)ATCmd.SyncShop, (byte)ReturnCode.Fail, xRCommonTip.xR_err_VerifyAssets);

        }

        /// <summary>
        /// 玩家获得金币
        /// </summary>
        /// <param name="roleid"></param>
        /// <param name="gold"></param>
        public static void UpdateRoleAssets(int roleid,int gold)
        {
            NHCriteria nHCriteria = xRCommon.xRNHCriteria("RoleID", roleid);
            var roleAssets = xRCommon.xRCriteria<RoleAssets>(nHCriteria);
            if (gold>0)
            {
                roleAssets.RoleGold += gold;
                NHibernateQuerier.Update(roleAssets);
                var dataDict = xRCommon.xRS2CParams();
                dataDict.Add((byte)ParameterCode.RoleAsset, roleAssets);
                var dict = xRCommon.xRS2CSub();
                dict.Add((byte)ShopOperate.Buy, Utility.Json.ToJson(dataDict));
                Utility.Debug.LogInfo("YZQData" + Utility.Json.ToJson(dataDict));
                xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncShop, (byte)ReturnCode.Success, dict);
            }
        }
        /// <summary>
        /// 玩家消耗金币
        /// </summary>
        /// <param name="roleid"></param>
        /// <param name="gold"></param>
        public static void ExpenseRoleAssets(int roleid, int gold)
        {
            NHCriteria nHCriteria = xRCommon.xRNHCriteria("RoleID", roleid);
            var roleAssets = xRCommon.xRCriteria<RoleAssets>(nHCriteria);
            if (gold > 0&& roleAssets.RoleGold >= gold)
            {
                roleAssets.RoleGold -= gold;
                NHibernateQuerier.Update(roleAssets);
                var dataDict = xRCommon.xRS2CParams();
                dataDict.Add((byte)ParameterCode.RoleAsset, roleAssets);
                var dict = xRCommon.xRS2CSub();
                dict.Add((byte)ShopOperate.Buy, Utility.Json.ToJson(dataDict));
                Utility.Debug.LogInfo("YZQData" + Utility.Json.ToJson(dataDict));
                xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncShop, (byte)ReturnCode.Success, dict);
            }else
                xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncShop, (byte)ReturnCode.Fail,xRCommonTip.xR_err_VerifyAssets);
        }

        public static void GetAwarad(RolepPropDTO roleShopDTO)
        {
            //GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, ADAward>>(out var adAwardDict);
            switch ((ADAwardType)roleShopDTO.PropType)
            {
                case ADAwardType.gold:
                    GoldAward(roleShopDTO);
                    break;
                case ADAwardType.SkillBook:
                    SkillAward(roleShopDTO);
                    break;
                case ADAwardType.Prop:
                    PropAward(roleShopDTO);
                    break;
                case ADAwardType.Cricket:
                    CricketAward(roleShopDTO);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 领取金币广告奖励
        /// </summary>
        /// <param name="roleShopDTO"></param>
        public static void GoldAward(RolepPropDTO roleShopDTO)
        {
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, ADAward>>(out var adAwardDict);
            var result = adAwardDict.TryGetValue(1701,out var aDAward);
            if (result)
            {
                var num = Utility.Algorithm.CreateRandomInt(aDAward.AddNumber[0], aDAward.AddNumber[0] + 1);
                UpdateRoleAssets(roleShopDTO.RoleID, num);
            }
            else
            {
                xRCommon.xRS2CSend(roleShopDTO.RoleID, (ushort)ATCmd.SyncShop, (byte)ReturnCode.Fail, xRCommonTip.xR_err_VerifyAwardType);
            }
        }
        /// <summary>
        /// 领取技能广告奖励
        /// </summary>
        /// <param name="roleShopDTO"></param>
        public static void SkillAward(RolepPropDTO roleShopDTO)
        {
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, ADAward>>(out var adAwardDict);
            var result = adAwardDict.TryGetValue(1702, out var aDAward);
            if (result)
            {
                var num = Utility.Algorithm.CreateRandomInt(aDAward.AddNumber[0], aDAward.AddNumber[0] + 1);
                InventoryManager.xRAddInventory(roleShopDTO.RoleID,new Dictionary<int, ItemDTO>() { { num, new ItemDTO() {ItemAmount=1 } } });
            }
            else
            {
                xRCommon.xRS2CSend(roleShopDTO.RoleID, (ushort)ATCmd.SyncShop, (byte)ReturnCode.Fail, xRCommonTip.xR_err_VerifyAwardType);
            }
        }
        /// <summary>
        /// 领取道具奖励
        /// </summary>
        /// <param name="roleShopDTO"></param>
        public static void PropAward(RolepPropDTO roleShopDTO)
        {
            var num = Utility.Algorithm.CreateRandomInt(0,1001);
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, ADAward>>(out var adAwardDict);
            if (num<1001&&num>=600)//奖励表奖励
            {
                InventoryManager.xRAddInventory(roleShopDTO.RoleID, new Dictionary<int, ItemDTO>() { { adAwardDict[1703].AddNumber[0], new ItemDTO() { ItemAmount = 1 } } });
            }
            else if (num < 600 && num >= 300)
            {
                InventoryManager.xRAddInventory(roleShopDTO.RoleID, new Dictionary<int, ItemDTO>() { { adAwardDict[1704].AddNumber[0], new ItemDTO() { ItemAmount = 1 } } });
            }
            else if (num < 300 && num >= 160)
            {
                InventoryManager.xRAddInventory(roleShopDTO.RoleID, new Dictionary<int, ItemDTO>() { { adAwardDict[1705].AddNumber[0], new ItemDTO() { ItemAmount = 1 } } });
            }
            else if (num < 160 && num >= 120)
            {
                InventoryManager.xRAddInventory(roleShopDTO.RoleID, new Dictionary<int, ItemDTO>() { { adAwardDict[1706].AddNumber[0], new ItemDTO() { ItemAmount = 1 } } });
            }
            else if (num < 120 && num >= 60)
            {
                InventoryManager.xRAddInventory(roleShopDTO.RoleID, new Dictionary<int, ItemDTO>() { { adAwardDict[1707].AddNumber[0], new ItemDTO() { ItemAmount = 1 } } });
            }
            else if (num < 60 && num >= 10)
            {
                InventoryManager.xRAddInventory(roleShopDTO.RoleID, new Dictionary<int, ItemDTO>() { { adAwardDict[1708].AddNumber[0], new ItemDTO() { ItemAmount = 1 } } });
            }
            else if (num <10 && num >= 9)
            {
                InventoryManager.xRAddInventory(roleShopDTO.RoleID, new Dictionary<int, ItemDTO>() { { adAwardDict[1709].AddNumber[0], new ItemDTO() { ItemAmount = 1 } } });
            }
            else if (num < 9 && num >= 4)
            {
                InventoryManager.xRAddInventory(roleShopDTO.RoleID, new Dictionary<int, ItemDTO>() { { adAwardDict[1710].AddNumber[0], new ItemDTO() { ItemAmount = 1 } } });
            }
            else if (num < 4 && num >=0)
            {
                InventoryManager.xRAddInventory(roleShopDTO.RoleID, new Dictionary<int, ItemDTO>() { { adAwardDict[1711].AddNumber[0], new ItemDTO() { ItemAmount = 1 } } });
            }
            Utility.Debug.LogError("广告奖励技能数据为" + num + "人物id为" + roleShopDTO.RoleID);
        }
        /// <summary>
        /// 领取蛐蛐奖励
        /// </summary>
        /// <param name="roleShopDTO"></param>
        public static void CricketAward(RolepPropDTO roleShopDTO)
        {
            var num = Utility.Algorithm.CreateRandomInt(0, 1001);
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, ADAward>>(out var adAwardDict);
            Utility.Debug.LogError("广告奖励技能数据为" + num + "人物id为" + roleShopDTO.RoleID);
            if (num < 1001 && num >= 900)
            {
                num = Utility.Algorithm.CreateRandomInt(adAwardDict[1712].AddNumber[0], adAwardDict[1712].AddNumber[1]);
                RoleCricketManager.AddCricket(5001, roleShopDTO.RoleID, num);
            } else if (num < 900 && num >= 700)
            {
                num = Utility.Algorithm.CreateRandomInt(adAwardDict[1713].AddNumber[0], adAwardDict[1713].AddNumber[1]);
                RoleCricketManager.AddCricket(5001, roleShopDTO.RoleID, num);
            }
            else if (num < 700 && num >= 300)
            {
                num = Utility.Algorithm.CreateRandomInt(adAwardDict[1714].AddNumber[0], adAwardDict[1714].AddNumber[1]);
                RoleCricketManager.AddCricket(5001, roleShopDTO.RoleID, num);
            }
            else if (num < 300 && num >= 200)
            {
                num = Utility.Algorithm.CreateRandomInt(adAwardDict[1715].AddNumber[0], adAwardDict[1715].AddNumber[1]);
                RoleCricketManager.AddCricket(5001, roleShopDTO.RoleID, num);
            }
            else if (num < 200 && num >= 100)
            {
                num = Utility.Algorithm.CreateRandomInt(adAwardDict[1716].AddNumber[0], adAwardDict[1716].AddNumber[1]);
                RoleCricketManager.AddCricket(5001, roleShopDTO.RoleID, num);
            }
            else if (num < 100 && num >= 10)
            {
                num = Utility.Algorithm.CreateRandomInt(adAwardDict[1717].AddNumber[0], adAwardDict[1717].AddNumber[1]);
                RoleCricketManager.AddCricket(5001, roleShopDTO.RoleID, num);
            }
            else if (num < 10 && num >= 1)
            {
                num = Utility.Algorithm.CreateRandomInt(adAwardDict[1718].AddNumber[0], adAwardDict[1718].AddNumber[1]);
                RoleCricketManager.AddCricket(5001, roleShopDTO.RoleID, num);
            }
            else if (num < 1 && num >= 0)
            {
                num = Utility.Algorithm.CreateRandomInt(adAwardDict[1719].AddNumber[0], adAwardDict[1719].AddNumber[1]);
                RoleCricketManager.AddCricket(5001, roleShopDTO.RoleID, num);
            }
        }
        public enum ADAwardType
        {
            gold=1,
            SkillBook=2,
            Prop=3,
            Cricket=4
        }
    }
}
