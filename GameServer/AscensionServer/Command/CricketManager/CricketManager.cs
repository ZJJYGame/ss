using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol;
using Cosmos;
using Protocol;

namespace AscensionServer
{
    [CustomeModule]
    public class CricketManager : Module<CricketManager>
    {
        public override void OnPreparatory()
        {
            CommandEventCore.Instance.AddEventListener((ushort)ATCmd.SyncCricket, C2SCricket);
        }


        public void C2SCricket(OperationData opData)
        {
            var data = Utility.Json.ToObject<Dictionary<byte,object>>(opData.DataMessage.ToString());
            Utility.Debug.LogInfo("yzqData请求蛐蛐属性:" +Utility.Json.ToJson(data));
            foreach (var item in data)
            {
                var dict = Utility.Json.ToObject<Dictionary<byte, object>>(item.Value.ToString());
                var roleObj = Utility.Json.ToObject<Role>(dict[(byte)ParameterCode.Role].ToString());
                switch ((CricketOperateType)item.Key)
                {
                    case CricketOperateType.AddCricket:
                        var cricket = Utility.Json.ToObject<Cricket>(dict[(byte)ParameterCode.Cricket].ToString());
                        Utility.Debug.LogInfo("yzqData添加蛐蛐:" + roleObj.RoleID + "蛐蛐id" + cricket.ID);
                        RoleCricketManager.InsteadOfPos(cricket.ID, roleObj.RoleID);
                        break;
                    case CricketOperateType.GetCricket:
                        //Utility.Debug.LogInfo("yzqData添加蛐蛐:" + roleObj.RoleID);
                        RoleCricketManager.GetRoleCricket(roleObj.RoleID,CricketOperateType.GetCricket);
                        break;
                    case CricketOperateType.GetTempCricket:
                        //Utility.Debug.LogInfo("yzqData获得临时蛐蛐:" + roleObj.RoleID);
                        RoleCricketManager.GetTempCricket(roleObj.RoleID,CricketOperateType.GetTempCricket);
                        break;
                    case CricketOperateType.RemoveCricket:
                         cricket = Utility.Json.ToObject<Cricket>(dict[(byte)ParameterCode.Cricket].ToString());
                        RoleCricketManager.RemoveCricket(roleObj.RoleID, cricket.ID);
                        break;
                    case CricketOperateType.AddPoint:
                        var pointObj = Utility.Json.ToObject<CricketPointDTO>(dict[(byte)ParameterCode.CricketPoint].ToString());
                        RoleCricketManager.AddPointForScricket(roleObj.RoleID, pointObj.CricketID, pointObj);
                        break;
                    case CricketOperateType.RmvTempCricket:
                        cricket = Utility.Json.ToObject<Cricket>(dict[(byte)ParameterCode.Cricket].ToString());
                        RoleCricketManager.RmvTempCricket(roleObj.RoleID, cricket.ID);
                        break;
                    case CricketOperateType.UseItem:
                       var prop = Utility.Json.ToObject<RolepPropDTO>(dict[(byte)ParameterCode.UseItem].ToString());
                        Utility.Debug.LogInfo("yzqData使用物品:" + Utility.Json.ToJson(prop));
                        RoleCricketManager.DifferentiateGlobal(prop.PropID, roleObj.RoleID, prop.CricketID);
                        break;
                    case CricketOperateType.EnlargeNest:
                        RoleCricketManager.EnlargeNest(roleObj.RoleID);
                        break;
                    case CricketOperateType.UpdateSkill:
                        Utility.Debug.LogInfo("YZQ升级技能" + Utility.Json.ToJson(dict));
                        var tempprop = Utility.Json.ToObject<RolepPropDTO>(dict[(byte)ParameterCode.UseItem].ToString());
                        RoleCricketManager.UpdateCricketSkill(roleObj.RoleID, tempprop, tempprop.CricketID);
                        break;
                    case CricketOperateType.UpdateName:
                        var cricketDTO= Utility.Json.ToObject<CricketDTO>(dict[(byte)ParameterCode.Cricket].ToString());
                        RoleCricketManager.ChangeCricketName(roleObj.RoleID, cricketDTO.ID, cricketDTO.CricketName);
                        break; 
                    default:
                        break;
                }
            }
        }
    }
}
