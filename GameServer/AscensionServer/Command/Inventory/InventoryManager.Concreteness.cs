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
    public partial class InventoryManager
    {
        /// <summary>
        /// 验证是否存在 并且验证数量是否满足
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="ItemId"></param>
        /// <returns></returns>
        public static bool xRVerifyInventory(int roleId,int ItemId,int acount)
        {
            var nHcriteria = xRCommon.xRNHCriteria("RoleID", roleId);
            if (xRCommon.xRVerify<Role>(nHcriteria))
            {
                var xRserver = xRCommon.xRCriteria<Inventory>(nHcriteria);
                var xrDict = Utility.Json.ToObject<Dictionary<int, ItemDTO>>(xRserver.ItemDict);
                if (xrDict.ContainsKey(ItemId) && xrDict[ItemId].ItemAmount>= acount)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 获取背包
        /// </summary>
        /// <param name="roleId"></param>
        public static void xRGetInventory(int roleId)
        {
            var nHcriteria = xRCommon.xRNHCriteria("RoleID", roleId);
            if (xRCommon.xRVerify<Role>(nHcriteria))
            {
                var xRserver = xRCommon.xRCriteria<Inventory>(nHcriteria);
                Utility.Debug.LogInfo("老陆==>" + xRserver.ItemDict);
                var pareams = xRCommon.xRS2CParams();
                pareams.Add((byte)ParameterCode.RoleInventory, xRserver.ItemDict);
                var subOp = xRCommon.xRS2CSub();
                subOp.Add((byte)subInventoryOp.Get, pareams);
                xRCommon.xRS2CSend(roleId, (byte)ATCmd.SyncInventory,(byte)ReturnCode.Success, subOp);
            }
        }
        /// <summary>
        /// 添加背包
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="ItemInfo"></param>
        public static void xRAddInventory(int roleId,Dictionary<int,ItemDTO> ItemInfo)
        {
             var nHcriteria = xRCommon.xRNHCriteria("RoleID", roleId);
            if (xRCommon.xRVerify<Role>(nHcriteria))
            {
                var xRserver = xRCommon.xRCriteria<Inventory>(nHcriteria);
                var xrDict = Utility.Json.ToObject<Dictionary<int, ItemDTO>>(xRserver.ItemDict);
                foreach (var info in ItemInfo)
                {
                    if (!xrDict.ContainsKey(info.Key))
                    {
                        xrDict[info.Key] = info.Value;
                    }
                    else
                    {
                        xrDict[info.Key].ItemAmount += info.Value.ItemAmount;
                    }
                    NHibernateQuerier.Update(new Inventory() { RoleID = roleId, ItemDict = Utility.Json.ToJson(xrDict) });
                }
                xRGetInventory(roleId);
            }
        }
        /// <summary>
        /// 注册添加背包
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="ItemInfo"></param>
        public static void xRRegisterAddInventory(int roleId, Dictionary<int, ItemDTO> ItemInfo)
        {
            var nHcriteria = xRCommon.xRNHCriteria("RoleID", roleId);
            if (xRCommon.xRVerify<Role>(nHcriteria))
            {
                var xRserver = xRCommon.xRCriteria<Inventory>(nHcriteria);
                var xrDict = Utility.Json.ToObject<Dictionary<int, ItemDTO>>(xRserver.ItemDict);
                foreach (var info in ItemInfo)
                {
                    if (!xrDict.ContainsKey(info.Key))
                    {
                        xrDict[info.Key] = info.Value;
                    }
                    else
                    {
                        xrDict[info.Key].ItemAmount += info.Value.ItemAmount;
                    }
                    NHibernateQuerier.Update(new Inventory() { RoleID = roleId, ItemDict = Utility.Json.ToJson(xrDict) });
                }
                
            }
        }

        /// <summary>
        /// 更新背包
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="ItemInfo"></param>
        public static void xRUpdateInventory(int roleId, Dictionary<int, ItemDTO> ItemInfo)
        {
            var nHcriteria = xRCommon.xRNHCriteria("RoleID", roleId);
            if (xRCommon.xRVerify<Role>(nHcriteria))
            {
                var xRserver = xRCommon.xRCriteria<Inventory>(nHcriteria);
                var xrDict = Utility.Json.ToObject<Dictionary<int, ItemDTO>>(xRserver.ItemDict);
                foreach (var info in ItemInfo)
                {
                    if (!xrDict.ContainsKey(info.Key))
                        return;
                    else
                    {
                        if (xrDict[info.Key].ItemAmount > 0 && xrDict[info.Key].ItemAmount >= info.Value.ItemAmount)
                        {
                            xrDict[info.Key].ItemAmount -= info.Value.ItemAmount;
                            if (xrDict[info.Key].ItemAmount<= 0)
                                xrDict.Remove(info.Key);
                        }
                    }
                    NHibernateQuerier.Update(new Inventory() { RoleID = roleId, ItemDict = Utility.Json.ToJson(xrDict) });
                }
                xRGetInventory(roleId);
            }
        }

    }
}
