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
        public static void LoginRole(string account, string password,object peer)
        {
            NHCriteria nHCriteriauser = xRCommon.xRNHCriteria("Account", account);
            var user = xRCommon.xRCriteria<User>(nHCriteriauser);
            if (user != null)
            {
                if (user.Password == password)
                {
                    NHCriteria nHCriteriaRole = xRCommon.xRNHCriteria("RoleID", user.RoleID);
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
                    var result = GameManager.CustomeModule<RoleManager>().TryGetValue(user.RoleID, out var roleEntity);
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
                            //GameManager.CustomeModule<LoginManager>().S2CLoginOff(roleEntity.SessionId, "账号在别处登录", ReturnCode.Success);


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

                    //var roleEntity = RoleEntity.Create(role.RoleID, (peer as IPeerEntity).SessionId, role);
                    //IPeerEntity peerAgent;
                    //var result = GameManager.CustomeModule<PeerManager>().TryGetValue((peer as IPeerEntity).SessionId, out peerAgent);
                    //if (result)
                    //{
                    //    var remoteRoleType = typeof(RoleEntity);
                    //    var exist = peerAgent.ContainsKey(remoteRoleType);
                    //    if (!exist)
                    //    {
                    //        //GameManager.CustomeModule<RoleManager>().TryRemove(role.RoleID);
                    //        var isture = GameManager.CustomeModule<RoleManager>().TryAdd(role.RoleID, roleEntity);
                    //        peerAgent.TryAdd(remoteRoleType, roleEntity);
                    //        Utility.Debug.LogError("yzqData登录成功RoleID:" + role.RoleID + isture);
                    //        GameManager.CustomeModule<LoginManager>().S2CLogin((peer as IPeerEntity).SessionId, Utility.Json.ToJson(dataDict), ReturnCode.Success);
                    //    }
                    //    else
                    //    {
                    //        //TODO提示账号已在线阻止登陆
                    //        GameManager.CustomeModule<RoleManager>().TryGetValue(role.RoleID, out var oldRoleEntityObj);
                    //        RoleEntity oldRoleEntity = oldRoleEntityObj as RoleEntity;
                    //        GameManager.CustomeModule<LoginManager>().S2CLoginOff(oldRoleEntity.SessionId, "账号在别处登录", ReturnCode.Success);
                    //        GameManager.CustomeModule<RoleManager>().TryRemove(oldRoleEntity.RoleId);
                    //        GameManager.CustomeModule<RoleManager>().TryAdd(role.RoleID, roleEntity);
                    //        peerAgent.TryAdd(typeof(RoleEntity), roleEntity);
                    //        GameManager.CustomeModule<LoginManager>().S2CLogin((peer as IPeerEntity).SessionId, Utility.Json.ToJson(dataDict), ReturnCode.Success);
                    //        Utility.Debug.LogError(oldRoleEntity.SessionId + "被挤下", (peer as IPeerEntity).SessionId + "重新登录");
                    //    }
                    //}
                    #endregion
                }
                else
                {
                    GameManager.CustomeModule<LoginManager>().S2CLogin((peer as IPeerEntity).SessionId);
                }
            }
            else
            {
                GameManager.CustomeModule<LoginManager>().S2CLogin((peer as IPeerEntity).SessionId);
            }
        }

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
    }
}
