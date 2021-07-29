using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using Protocol;
using AscensionProtocol;

namespace AscensionServer
{
    public static partial class RoleCricketManager
    {
        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="roleid"></param>
        public static void GetRoleCricket(int roleid, CricketOperateType opType)
        {
            Utility.Debug.LogError("开始获取蛐蛐数据");
            var nHCriteriaRole = xRCommon.xRNHCriteria("RoleID", roleid);
            var roleCricket = xRCommon.xRCriteria<RoleCricket>(nHCriteriaRole);
            Dictionary<int, CricketDTO> cricketsDict = new Dictionary<int, CricketDTO>();
            Dictionary<int, CricketStatus> statusDict = new Dictionary<int, CricketStatus>();
            Dictionary<int, CricketPoint> pointDict = new Dictionary<int, CricketPoint>();
            Dictionary<int, CricketAptitude> aptitudeDict = new Dictionary<int, CricketAptitude>();
            var dataDict = xRCommon.xRS2CParams();
            if (roleCricket != null)
            {
                var cricketDict = Utility.Json.ToObject<List<int>>(roleCricket.CricketList);
                for (int i = 0; i < cricketDict.Count; i++)
                {
                    if (cricketDict[i] != -1)
                    {
                        var nHCriteriaCricket = xRCommon.xRNHCriteria("ID", cricketDict[i]);
                        var nHCriteriastatus = xRCommon.xRNHCriteria("CricketID", cricketDict[i]);
                        var crickets = xRCommon.xRCriteria<Cricket>(nHCriteriaCricket);
                        Utility.Debug.LogInfo("yzqData获取蛐蛐属性:" + Utility.Json.ToJson(crickets));
                        CricketDTO cricketDTO = SetCricketValue(crickets);

                        cricketsDict.Add(crickets.ID, cricketDTO);
                        statusDict.Add(crickets.ID, xRCommon.xRCriteria<CricketStatus>(nHCriteriastatus));
                        pointDict.Add(crickets.ID, xRCommon.xRCriteria<CricketPoint>(nHCriteriastatus));
                        aptitudeDict.Add(crickets.ID, xRCommon.xRCriteria<CricketAptitude>(nHCriteriastatus));
                    }
                }
                dataDict.Add((byte)ParameterCode.RoleCricket, cricketDict);
                dataDict.Add((byte)ParameterCode.Cricket, cricketsDict);
                dataDict.Add((byte)ParameterCode.CricketStatus, statusDict);
                dataDict.Add((byte)ParameterCode.CricketPoint, pointDict);
                dataDict.Add((byte)ParameterCode.CricketAptitude, aptitudeDict);
                var messageDict = xRCommon.xRS2CSub();
                messageDict.Add((byte)opType, Utility.Json.ToJson(dataDict));
                xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncCricket, (short)ReturnCode.Success, messageDict);
            }
            Utility.Debug.LogError("蛐蛐信息获取完成");
        }
        /// <summary>
        /// 添加新蛐蛐
        /// </summary>
        /// <param name="cricketid"></param>
        /// <param name="roleid"></param>
        public static void AddCricket(int cricketid, int roleid,int aptitudes=0)
        {
            var nHCriteriaRole = xRCommon.xRNHCriteria("RoleID", roleid);
            var roleCricket = xRCommon.xRCriteria<RoleCricket>(nHCriteriaRole);
           
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, CricketLevel>>(out var cricketLevelDict);
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, CricketNameData>>(out var NameDict);
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, CricketHeadPortraitData>>(out var CricketHeadDict);


            RoleCricketDTO roleCricketDTO = new RoleCricketDTO();
            roleCricketDTO.RoleID = roleCricket.RoleID;
            roleCricketDTO.TemporaryCrickets = Utility.Json.ToObject<List<int>>(roleCricket.TemporaryCrickets);
            roleCricketDTO.CricketList = Utility.Json.ToObject<List<int>>(roleCricket.CricketList);
            if (roleCricketDTO.CricketList.Contains(-1))
            {
                for (int i = 0; i < roleCricketDTO.CricketList.Count; i++)
                {
                    if (roleCricketDTO.CricketList[i] == -1)
                    {
                        var cricketStatus = new CricketStatus();
                        var cricketAptitude = new CricketAptitude();
                        if (aptitudes == 0)
                        {
                            cricketAptitude.ConAptitude = Utility.Algorithm.CreateRandomInt(1, 101);
                            cricketAptitude.StrAptitude = Utility.Algorithm.CreateRandomInt(1, 101);
                            cricketAptitude.DefAptitude = Utility.Algorithm.CreateRandomInt(1, 101);
                            cricketAptitude.DexAptitude = Utility.Algorithm.CreateRandomInt(1, 101);
                        }
                        else
                        {
                            cricketAptitude.ConAptitude = aptitudes;
                            cricketAptitude.StrAptitude = aptitudes;
                            cricketAptitude.DefAptitude = aptitudes;
                            cricketAptitude.DexAptitude = aptitudes;
                        }
                        var cricket = new Cricket();
                        cricket.Roleid = roleid;
                        var headlist = CricketHeadDict.Keys.ToList<int>();
                        var headnum = Utility.Algorithm.CreateRandomInt(0, headlist.Count);
                        cricket.HeadPortraitID = CricketHeadDict[headlist[headnum]].CricketID;
                        var namelist = NameDict.Keys.ToList<int>();
                        var namenum = Utility.Algorithm.CreateRandomInt(0, namelist.Count);
                        cricket.CricketName = NameDict[namelist[namenum]].CricketName;
                        cricket = NHibernateQuerier.Insert(cricket);
                        cricketAptitude.CricketID = cricket.ID;
                        NHibernateQuerier.Insert(cricketAptitude);
                        var cricketAddition = new CricketAddition();
                        cricketAddition.CricketID = cricket.ID;
                        NHibernateQuerier.Insert(cricketAddition);
                        var cricketPoint = new CricketPoint();
                        cricketPoint.FreePoint = cricketLevelDict[cricket.LevelID].AssignPoint;
                        cricketPoint.CricketID = cricket.ID;
                        NHibernateQuerier.Insert(cricketPoint);
                        //cricketStatus = RoleCricketManager.CalculateStutas(cricketAptitude, cricketPoint, cricketAddition);
                        cricketStatus = RoleCricketManager.SkillAdditionStatus(cricket, cricketAptitude, cricketPoint, cricketAddition, out var cricketPointTemp);
                        cricketStatus.CricketID = cricket.ID;
                        #region
                        cricketAptitude.SkillCon = cricketPointTemp.Con;
                        cricketAptitude.SkillDef = cricketPointTemp.Def;
                        cricketAptitude.SkillDex = cricketPointTemp.Dex;
                        cricketAptitude.SkillStr = cricketPointTemp.Str;
                        #endregion
                        NHibernateQuerier.Insert(cricketStatus);
                        NHibernateQuerier.Update(cricketAptitude);
                        roleCricketDTO.CricketList[i] = cricket.ID;
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < roleCricketDTO.TemporaryCrickets.Count; i++)
                {
                    if (roleCricketDTO.TemporaryCrickets[i] == -1)
                    {
                        var cricketStatus = new CricketStatus();
                        var cricketAptitude = new CricketAptitude();
                        if (aptitudes == 0)
                        {
                            cricketAptitude.ConAptitude = Utility.Algorithm.CreateRandomInt(1, 101);
                            cricketAptitude.StrAptitude = Utility.Algorithm.CreateRandomInt(1, 101);
                            cricketAptitude.DefAptitude = Utility.Algorithm.CreateRandomInt(1, 101);
                            cricketAptitude.DexAptitude = Utility.Algorithm.CreateRandomInt(1, 101);
                        }
                        else
                        {
                            cricketAptitude.ConAptitude = aptitudes;
                            cricketAptitude.StrAptitude = aptitudes;
                            cricketAptitude.DefAptitude = aptitudes;
                            cricketAptitude.DexAptitude = aptitudes;
                        }
                        var cricket = new Cricket();
                        cricket.Roleid = roleid;
                        var headlist = CricketHeadDict.Keys.ToList<int>();
                        var headnum = Utility.Algorithm.CreateRandomInt(0, headlist.Count);
                        cricket.HeadPortraitID = CricketHeadDict[headlist[headnum]].CricketID;
                        var namelist = NameDict.Keys.ToList<int>();
                        var namenum = Utility.Algorithm.CreateRandomInt(0, namelist.Count);
                        cricket.CricketName = NameDict[namelist[namenum]].CricketName;
                        cricket = NHibernateQuerier.Insert(cricket);
                        cricketAptitude.CricketID = cricket.ID;
                        NHibernateQuerier.Insert(cricketAptitude);
                        var cricketAddition = new CricketAddition();
                        cricketAddition.CricketID = cricket.ID;
                        NHibernateQuerier.Insert(cricketAddition);
                        var cricketPoint = new CricketPoint();
                        cricketPoint.FreePoint = cricketLevelDict[cricket.LevelID].AssignPoint;
                        cricketPoint.CricketID = cricket.ID;
                        NHibernateQuerier.Insert(cricketPoint);
                        //cricketStatus = RoleCricketManager.CalculateStutas(cricketAptitude, cricketPoint, cricketAddition);
                        cricketStatus = RoleCricketManager.SkillAdditionStatus(cricket, cricketAptitude, cricketPoint, cricketAddition, out var cricketPointTemp);
                        cricketStatus.CricketID = cricket.ID;
                        #region
                        cricketAptitude.SkillCon = cricketPointTemp.Con;
                        cricketAptitude.SkillDef = cricketPointTemp.Def;
                        cricketAptitude.SkillDex = cricketPointTemp.Dex;
                        cricketAptitude.SkillStr = cricketPointTemp.Str;
                        #endregion
                        NHibernateQuerier.Insert(cricketStatus);
                        NHibernateQuerier.Update(cricketAptitude);
                        roleCricketDTO.TemporaryCrickets[i] = cricket.ID;
                        break;
                    }
                }
            }
            roleCricket.CricketList = Utility.Json.ToJson(roleCricketDTO.CricketList);
            roleCricket.TemporaryCrickets = Utility.Json.ToJson(roleCricketDTO.TemporaryCrickets);
            NHibernateQuerier.Update(roleCricket);
            Utility.Debug.LogInfo("YZQ添加新的临时蛐蛐进来了" + cricketid);
            GetTempCricket(roleid, CricketOperateType.UpdTempCricket);
        }
        /// <summary>
        /// 放生小屋蟋蟀
        /// </summary>
        /// <param name="cricketid"></param>
        /// <param name="roleid"></param>
        public static void RemoveCricket( int roleid,int cricketid)
        {
            NHCriteria nHCriteriaRole = xRCommon.xRNHCriteria("RoleID", roleid);
            var roleCricket = xRCommon.xRCriteria<RoleCricket>(nHCriteriaRole);

            var crickets = Utility.Json.ToObject<List<int>>(roleCricket.CricketList);
            if (crickets.Contains(cricketid) && cricketid != -1&& crickets.Count>1)
            {
                Utility.Debug.LogInfo("YZQData" + cricketid);
                NHCriteria nHCriteria = xRCommon.xRNHCriteria("ID", cricketid);
                var cricket = xRCommon.xRCriteria<Cricket>(nHCriteria);
                if (cricket != null)
                {
                    NHibernateQuerier.Delete(cricket);
                }
                NHCriteria nHCriteriaStatus = xRCommon.xRNHCriteria("CricketID", cricketid);

                var cricketStatus = xRCommon.xRCriteria<CricketStatus>(nHCriteriaStatus);
                if (cricketStatus!=null)
                {
                    NHibernateQuerier.Delete(cricketStatus);
                }
                var cricketAddition = xRCommon.xRCriteria<CricketAddition>(nHCriteriaStatus);
                if (cricketAddition != null)
                {
                    NHibernateQuerier.Delete(cricketAddition);
                }

                var cricketAptitude = xRCommon.xRCriteria<CricketAptitude>(nHCriteriaStatus);
                if (cricketAptitude!=null)
                {
                    NHibernateQuerier.Delete(cricketAptitude);
                }
                var cricketPoint = xRCommon.xRCriteria<CricketPoint>(nHCriteriaStatus);
                if (cricketPoint!=null)
                {
                    NHibernateQuerier.Delete(cricketPoint);
                }
                crickets.RemoveAt(crickets.IndexOf(cricketid));
                crickets.Add(-1);
                roleCricket.CricketList = Utility.Json.ToJson(crickets);
                NHibernateQuerier.Update(roleCricket);

                GetRoleCricket(roleid,CricketOperateType.UpdCricket);

            }else
                xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncCricket, (short)ReturnCode.Fail, xRCommonTip.xR_err_VerifyCricket);

        }
        /// <summary>
        /// 放生临时槽位
        /// </summary>
        /// <param name="roleid"></param>
        /// <param name="cricketid"></param>
        public static void RmvTempCricket(int roleid, int cricketid)
        {
            NHCriteria nHCriteriaRole = xRCommon.xRNHCriteria("RoleID", roleid);
            var roleCricket = xRCommon.xRCriteria<RoleCricket>(nHCriteriaRole);

            var crickets = Utility.Json.ToObject<List<int>>(roleCricket.TemporaryCrickets);
            if (crickets.Contains(cricketid) && cricketid != -1)
            {
                NHCriteria nHCriteria = xRCommon.xRNHCriteria("ID", cricketid);
                var cricket = xRCommon.xRCriteria<Cricket>(nHCriteria);
                if (cricket != null)
                {
                    NHibernateQuerier.Delete(cricket);
                }
                NHCriteria nHCriteriaStatus = xRCommon.xRNHCriteria("CricketID", cricketid);

                var cricketStatus = xRCommon.xRCriteria<CricketStatus>(nHCriteriaStatus);
                if (cricketStatus != null)
                {
                    NHibernateQuerier.Delete(cricketStatus);
                }
                var cricketAptitude = xRCommon.xRCriteria<CricketAptitude>(nHCriteriaStatus);
                if (cricketAptitude != null)
                {
                    NHibernateQuerier.Delete(cricketAptitude);
                }
                var cricketAddition = xRCommon.xRCriteria<CricketAddition>(nHCriteriaStatus);
                if (cricketAddition != null)
                {
                    NHibernateQuerier.Delete(cricketAddition);
                }
                var cricketPoint = xRCommon.xRCriteria<CricketPoint>(nHCriteriaStatus);
                if (cricketPoint != null)
                {
                    NHibernateQuerier.Delete(cricketPoint);
                }
                crickets.Remove(cricketid);
                crickets.Add(-1);
                roleCricket.TemporaryCrickets = Utility.Json.ToJson(crickets);
                NHibernateQuerier.Update(roleCricket);
                Utility.Debug.LogInfo("YZQData放生临时蛐蛐" + Utility.Json.ToJson(crickets));
                GetTempCricket(roleid, CricketOperateType.UpdTempCricket);

            }
        }
        /// <summary>
        /// 获取单个蛐蛐的属性及等级
        /// </summary>
        /// <param name="cricketid"></param>
        /// <returns></returns>
        public static Dictionary<byte, object> GetCricketStatus(int cricketid)
        {
            NHCriteria nHCriteriaCricket = xRCommon.xRNHCriteria("ID", cricketid);
            NHCriteria nHCriteriaStatus = xRCommon.xRNHCriteria("CricketID", cricketid);
            var cricketStatus = xRCommon.xRCriteria<CricketStatus>(nHCriteriaStatus);
            var cricket = xRCommon.xRCriteria<Cricket>(nHCriteriaCricket);
            Dictionary<byte, object> cricketData = new Dictionary<byte, object>();

            cricketData.Add((byte)ParameterCode.CricketStatus, cricketStatus);
            cricketData.Add((byte)ParameterCode.Cricket, cricket);
            GameManager.ReferencePoolManager.Despawns(nHCriteriaCricket, nHCriteriaStatus);
            return cricketData;
        }
        /// <summary>
        /// 蛐蛐加点
        /// </summary>
        /// <param name="cricketid"></param>
        public static void AddPointForScricket(int roleid, int cricketid, CricketPointDTO cricketPointDTO)
        {
            NHCriteria nHCriteria = xRCommon.xRNHCriteria("CricketID", cricketid);
            NHCriteria nHCriteriaCricket = xRCommon.xRNHCriteria("ID", cricketid);
            var cricketPoint = xRCommon.xRCriteria<CricketPoint>(nHCriteria);
            var aptitude = xRCommon.xRCriteria<CricketAptitude>(nHCriteria);
            var addition = xRCommon.xRCriteria<CricketAddition>(nHCriteria);
            var cricket = xRCommon.xRCriteria<Cricket>(nHCriteriaCricket);
            if (cricketPoint!=null)
            {
                if ((cricketPointDTO.Dex + cricketPointDTO.Def + cricketPointDTO.Con + cricketPointDTO.Str) > cricketPoint.FreePoint)
                {
                    //返回加点失败
                    xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncCricket, (byte)ReturnCode.Fail, xRCommonTip.xR_err_Verify);
                    return;
                }
                else
                {
                    cricketPoint.Def += cricketPointDTO.Def;
                    cricketPoint.Con += cricketPointDTO.Con;
                    if (cricketPoint.Dex+ cricketPointDTO.Dex+ aptitude.Dex>1000)
                    {
                        cricketPointDTO.Dex = 0;
                        cricketPoint.Dex =1000-(aptitude.Dex);
                    }else
                        cricketPoint.Dex += cricketPointDTO.Dex;
                    cricketPoint.Str += cricketPointDTO.Str;

                    cricketPoint.FreePoint -= (cricketPointDTO.Def + cricketPointDTO.Dex + cricketPointDTO.Con + cricketPointDTO.Str);
                    NHibernateQuerier.Update(cricketPoint);

                    var dataDict = xRCommon.xRS2CSub();
                    var cricketPointDict = xRCommon.xRS2CParams();

                    var status = SkillAdditionStatus(cricket,aptitude, cricketPoint, addition,out var cricketPointTemp);

                    status.CricketID = cricketPoint.CricketID;
                    cricketPointDict.Add((byte)ParameterCode.CricketPoint, cricketPoint);
                    cricketPointDict.Add((byte)ParameterCode.CricketAptitude, aptitude);
                    cricketPointDict.Add((byte)ParameterCode.CricketStatus, status);
                    NHibernateQuerier.Update(status);
                    dataDict.Add((byte)CricketOperateType.AddPoint, Utility.Json.ToJson(cricketPointDict));
                    xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncCricket, (short)ReturnCode.Success, dataDict);

                }
            }
            else
            {
                xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncCricket, (byte)ReturnCode.Fail, xRCommonTip.xR_err_ReLogin);
            }

        }

        public static void ChangeCricketName(int roleId, int cricketId, string name)
        {
            Utility.Debug.LogError("开始改变蛐蛐名字");
            var nHCriteriacricket = xRCommon.xRNHCriteria("ID", cricketId);
            var cricket = xRCommon.xRCriteria<Cricket>(nHCriteriacricket);
            cricket.CricketName = name;
            NHibernateQuerier.Update(cricket);
            var dataDict = xRCommon.xRS2CSub();
            dataDict.Add((byte)CricketOperateType.UpdateName, Utility.Json.ToJson(new CricketDTO() { ID=cricket.ID,CricketName=cricket.CricketName}));
            xRCommon.xRS2CSend(roleId, (ushort)ATCmd.SyncCricket, (byte)ReturnCode.Success, dataDict);
        }
        /// <summary>
        /// 获取临时槽位蟋蟀
        /// </summary>
        /// <param name="roleid"></param>
        public static void GetTempCricket(int roleid,CricketOperateType opType)
        {
            var nHCriteriaRole = xRCommon.xRNHCriteria("RoleID", roleid);
            var roleCricket = xRCommon.xRCriteria<RoleCricket>(nHCriteriaRole);
            Dictionary<int, CricketDTO> cricketsDict = new Dictionary<int, CricketDTO>();
            Dictionary<int, CricketStatus> statusDict = new Dictionary<int, CricketStatus>();
            Dictionary<int, CricketAptitude> aptitudeDict = new Dictionary<int, CricketAptitude>();

            var dataDict = xRCommon.xRS2CParams();
            if (roleCricket != null)
            {
                var cricketDict = Utility.Json.ToObject<List<int>>(roleCricket.TemporaryCrickets);

                for (int i = 0; i < cricketDict.Count; i++)
                {
                    if (cricketDict[i] != -1)
                    {
                        var nHCriteriaCricket = xRCommon.xRNHCriteria("ID", cricketDict[i]);
                        var nHCriteriastatus = xRCommon.xRNHCriteria("CricketID", cricketDict[i]);
                        var crickets = xRCommon.xRCriteria<Cricket>(nHCriteriaCricket);
                        Utility.Debug.LogInfo("yzqData获取临时的蛐蛐:" + Utility.Json.ToJson(crickets));
                        CricketDTO cricketDTO= SetCricketValue(crickets);
                       
                        cricketsDict.Add(cricketDict[i], cricketDTO);
                        statusDict.Add(cricketDict[i], xRCommon.xRCriteria<CricketStatus>(nHCriteriastatus));
                        aptitudeDict.Add(cricketDict[i], xRCommon.xRCriteria<CricketAptitude>(nHCriteriastatus));
                    }
                }
                dataDict.Add((byte)ParameterCode.RoleCricket, cricketDict);
                dataDict.Add((byte)ParameterCode.Cricket, cricketsDict);
                dataDict.Add((byte)ParameterCode.CricketStatus, statusDict);
                dataDict.Add((byte)ParameterCode.CricketAptitude, aptitudeDict);
                var messageDict = xRCommon.xRS2CSub();
                messageDict.Add((byte)opType, Utility.Json.ToJson(dataDict));
                xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncCricket, (short)ReturnCode.Success, messageDict);
                Utility.Debug.LogInfo("YZQ发送临时蛐蛐进来了Yzq");
            }
        }
        /// <summary>
        /// 蟋蟀添加正常槽位
        /// </summary>
        public static void InsteadOfPos(int cricketid,int roleid)
        {
            var nHCriteriaRole = xRCommon.xRNHCriteria("RoleID", roleid);
            var roleCricket = xRCommon.xRCriteria<RoleCricket>(nHCriteriaRole);
            if (roleCricket!=null)
            {
                var tempDict = Utility.Json.ToObject<List<int>>(roleCricket.TemporaryCrickets);
                var normalDict = Utility.Json.ToObject<List<int>>(roleCricket.CricketList);



                if (tempDict.Contains(cricketid) && cricketid != -1)
                {
                    for (int i = 0; i < normalDict.Count; i++)
                    {
                        if (normalDict[i] == -1)
                        {
                            normalDict[i] = cricketid;
                            Utility.Debug.LogInfo("YZQ替换成功" + tempDict.IndexOf(cricketid));
                            var num = tempDict.IndexOf(cricketid);
                            tempDict.RemoveAt(num);
                            tempDict.Add(-1);
                            break;
                        }
                    }
                    roleCricket.CricketList = Utility.Json.ToJson(normalDict);
                    roleCricket.TemporaryCrickets = Utility.Json.ToJson(tempDict);
                    NHibernateQuerier.Update(roleCricket);
                    GetRoleCricket(roleid,CricketOperateType.UpdCricket);
                    GetTempCricket(roleid, CricketOperateType.UpdTempCricket);

                }
                else
                    xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncCricket, (byte)ReturnCode.Fail, xRCommonTip.xR_err_ReLogin);
            }
            else
                xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncCricket, (byte)ReturnCode.Fail, xRCommonTip.xR_err_ReLogin);
        }
        /// <summary>
        /// 扩充蟋蟀窝
        /// </summary>
        /// <param name="roleid"></param>
        public static void EnlargeNest( int roleid)
        {
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, NestLevel>>(out var nestLevelDict);
            NHCriteria nHCriteria = xRCommon.xRNHCriteria("RoleID", roleid);
            var roleAssets = xRCommon.xRCriteria<RoleAssets>(nHCriteria);
            var roleCricket= xRCommon.xRCriteria<RoleCricket>(nHCriteria);
            if (roleAssets!=null)
            {
                if (roleCricket!=null)
                {
                    var crickets = Utility.Json.ToObject<List<int>>(roleCricket.CricketList);
                    if (roleAssets.RoleGold >= nestLevelDict[crickets.Count].Gold)
                    {
                        crickets.Add(-1);
                        roleCricket.CricketList = Utility.Json.ToJson(crickets);
                        roleAssets.RoleGold -= nestLevelDict[crickets.Count].Gold;
                        NHibernateQuerier.Update(roleCricket);
                        NHibernateQuerier.Update(roleAssets);
                        var dataDict = xRCommon.xRS2CParams();
                        dataDict.Add((byte)ParameterCode.RoleAsset, roleAssets);
                        dataDict.Add((byte)ParameterCode.RoleCricket, crickets);
                        var sendDict = xRCommon.xRS2CSub();
                        sendDict.Add((byte)CricketOperateType.EnlargeNest, Utility.Json.ToJson(dataDict));
                        xRCommon.xRS2CSend(roleid,(ushort)ATCmd.SyncCricket,(short)ReturnCode.Success, sendDict);
                    }
                    else
                        xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncCricket, (short)ReturnCode.Fail, xRCommonTip.xR_err_Verify);
                    return;
                }
                else
                    xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncCricket, (short)ReturnCode.Fail, xRCommonTip.xR_err_Verify);
                return;
            }
            else
                xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncCricket, (short)ReturnCode.Fail, xRCommonTip.xR_err_Verify);
            return;
        }
    }
}
