using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using AscensionProtocol;
namespace AscensionServer
{
    public static partial class RoleCricketManager
    {
        /// <summary>
        /// 重置蟋蟀加点
        /// </summary>
        /// <param name="level">当前蟋蟀等级</param>
        /// <returns></returns>
        public static void ReSetPoint(int roleid, int cricketid, int propid)
        {
            int points = 0;
            var nHCriteria = xRCommon.xRNHCriteria("CricketID", cricketid);
            var nHCriteriacricket = xRCommon.xRNHCriteria("ID", cricketid);
            var cricket = xRCommon.xRCriteria<Cricket>(nHCriteriacricket);
            var point = xRCommon.xRCriteria<CricketPoint>(nHCriteria);
            var aptitude = xRCommon.xRCriteria<CricketAptitude>(nHCriteria);
            var addition = xRCommon.xRCriteria<CricketAddition>(nHCriteria);
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, CricketLevel>>(out var cricketLevelDict);
            if (addition != null && aptitude != null && point != null && cricket != null && cricket != null)
            {
                for (int i = 1; i <= cricket.LevelID; i++)
                {
                    points += cricketLevelDict[i].AssignPoint;
                }
                point.FreePoint = points;
                point.Str = 0;
                point.Dex = 0;
                point.Def = 0;
                point.Con = 0;
                NHibernateQuerier.Update(point);
                //var status = CalculateStutas(aptitude, point, addition);
                var status = SkillAdditionStatus(cricket, aptitude, point, addition, out var cricketPoint);
                status.CricketID = cricketid;
                NHibernateQuerier.Update(status);
                var data = xRCommon.xRS2CParams();
                data.Add((byte)ParameterCode.CricketAptitude, aptitude);
                data.Add((byte)ParameterCode.CricketStatus, status);
                data.Add((byte)ParameterCode.CricketPoint, point);
                var dict = xRCommon.xRS2CSub();
                dict.Add((byte)CricketOperateType.AddPoint, Utility.Json.ToJson(data));
                xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncCricket, (byte)ReturnCode.Success, dict);

                InventoryManager.xRUpdateInventory(roleid, new Dictionary<int, ItemDTO> { { propid, new ItemDTO() { ItemAmount = 1 } } });
            }
            else
                xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncCricket, (byte)ReturnCode.Fail, xRCommonTip.xR_err_Verify);



        }
        /// 获取资质等随机随机
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="num"></param>
        public static int RandomNum(int min, int max)
        {
            int num = 50;
            Random random = new Random();
            num = random.Next(min, max);
            return num;
        }
        /// <summary>
        /// 加经验升级
        /// </summary>
        public static void UpdateLevel(int cricketid, PropData propData, int roleid)
        {
            var nHCriteria = xRCommon.xRNHCriteria("CricketID", cricketid);
            var nHCriteriacricket = xRCommon.xRNHCriteria("ID", cricketid);
            var cricket = xRCommon.xRCriteria<Cricket>(nHCriteriacricket);
            var point = xRCommon.xRCriteria<CricketPoint>(nHCriteria);
            if (cricket != null || point != null)
            {
                GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, CricketLevel>>(out var cricketLevelDict);
                cricket.Exp += propData.AddNumber;
                var tempcricket = new Cricket() { Exp = cricket.Exp, LevelID = cricket.LevelID };

                tempcricket = LevelUpCalculate(tempcricket);
                for (int i = cricket.LevelID + 1; i <= tempcricket.LevelID; i++)
                {
                    point.FreePoint += cricketLevelDict[i].AssignPoint;
                }
                cricket.Exp = tempcricket.Exp;
                cricket.LevelID = tempcricket.LevelID;
                var data = xRCommon.xRS2CParams();
                data.Add((byte)ParameterCode.Cricket, SetCricketValue(cricket));
                data.Add((byte)ParameterCode.CricketPoint, point);
                var dict = xRCommon.xRS2CSub();
                dict.Add((byte)CricketOperateType.LevelUp, Utility.Json.ToJson(data));
                xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncCricket, (short)ReturnCode.Success, dict);
                NHibernateQuerier.Update(cricket);
                NHibernateQuerier.Update(point);
                if (propData.PropID != -1)
                {
                    InventoryManager.xRUpdateInventory(roleid, new Dictionary<int, ItemDTO> { { propData.PropID, new ItemDTO() { ItemAmount = 1 } } });
                }
            }
            else
            {
                xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncCricket, (short)ReturnCode.Fail, xRCommonTip.xR_err_Verify);
            }
        }
        /// <summary>
        /// 学习技能
        /// </summary>
        public static void StudySkill(int skillid, int cricketid, int roleid)
        {
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, PropData>>(out var propDict);
            var nHCriteriacricket = xRCommon.xRNHCriteria("ID", cricketid);
            var nHCriteriastatus = xRCommon.xRNHCriteria("CricketID", cricketid);
            var cricket = xRCommon.xRCriteria<Cricket>(nHCriteriacricket);
            var cricketstatus = xRCommon.xRCriteria<CricketStatus>(nHCriteriastatus);
            var aptitude = xRCommon.xRCriteria<CricketAptitude>(nHCriteriastatus);
            var addition = xRCommon.xRCriteria<CricketAddition>(nHCriteriastatus);
            var point = xRCommon.xRCriteria<CricketPoint>(nHCriteriastatus);
            if (cricket != null && cricketstatus != null && aptitude != null && addition != null && point != null)
            {
                #region 
                var skills = Utility.Json.ToObject<Dictionary<int, int>>(cricket.SkillDict);
                if (skills.ContainsKey(skillid))
                {
                    if (skills[skillid] + 1 < 10)
                    {
                        skills[skillid] += 1;
                    }
                    else
                    {
                        skills[skillid] = 10;
                        xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncCricket, (byte)ReturnCode.Fail, xRCommonTip.xR_err_Verify);
                    }
                    return;
                }
                else
                {
                    skills.Add(skillid, 0);
                    //返回成功并更新数据库
                    cricket.SkillDict = Utility.Json.ToJson(skills);
                    var status = SkillAdditionStatus(cricket, aptitude, point, addition, out var cricketPoint);
                    Utility.Debug.LogError("学习技能" + Utility.Json.ToJson(cricketPoint));


                    status.CricketID = cricket.ID;
                    #region
                    aptitude.SkillDex = cricketPoint.Dex;
                    aptitude.SkillDef = cricketPoint.Def;
                    aptitude.SkillStr = cricketPoint.Str;
                    aptitude.SkillCon = cricketPoint.Con;
                    #endregion
                    NHibernateQuerier.Update(cricket);
                    NHibernateQuerier.Update(status);
                    NHibernateQuerier.Update(aptitude);

                    var data = xRCommon.xRS2CParams();
                    data.Add((byte)ParameterCode.Cricket, SetCricketValue(cricket));
                    data.Add((byte)ParameterCode.CricketStatus, status);
                    data.Add((byte)ParameterCode.CricketAptitude, aptitude);
                    data.Add((byte)ParameterCode.CricketPoint, point);
                    var dict = xRCommon.xRS2CSub();
                    dict.Add((byte)CricketOperateType.UpdateSkill, Utility.Json.ToJson(data));
                    xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncCricket, (byte)ReturnCode.Success, dict);

                    return;
                }
                #endregion
            }
            else
            {
                xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncCricket, (byte)ReturnCode.Fail, xRCommonTip.xR_err_Verify);
                return;//返回失败
            }

        }
        /// <summary>
        /// 删除技能
        /// </summary>
        public static void RemoveSkill(int prop, int cricketid, int roleid)
        {
            var nHCriteriacricket = xRCommon.xRNHCriteria("ID", cricketid);
            var nHCriteriastatus = xRCommon.xRNHCriteria("CricketID", cricketid);
            var cricket = xRCommon.xRCriteria<Cricket>(nHCriteriacricket);
            var cricketstatus = xRCommon.xRCriteria<CricketStatus>(nHCriteriastatus);
            var aptitude = xRCommon.xRCriteria<CricketAptitude>(nHCriteriastatus);
            var addition = xRCommon.xRCriteria<CricketAddition>(nHCriteriastatus);
            var point = xRCommon.xRCriteria<CricketPoint>(nHCriteriastatus);
            if (cricket != null && cricketstatus != null && aptitude != null && addition != null && point != null)
            {
                var skillDict = Utility.Json.ToObject<Dictionary<int, int>>(cricket.SpecialDict);

                if (skillDict.Count > 0)
                {
                    Random random = new Random();
                    var num = random.Next(0, skillDict.Count);
                    var skilllist = skillDict.Keys.ToList<int>();
                    skillDict.Remove(skilllist[num]);
                    Utility.Debug.LogInfo("YZQ删除技能后的蛐蛐" + skillDict.Count + "下标" + num);
                    cricket.SpecialDict = Utility.Json.ToJson(skillDict);


                    var status = SkillAdditionStatus(cricket, aptitude, point, addition, out var cricketPoint);
                    Utility.Debug.LogError("学习技能" + Utility.Json.ToJson(cricketPoint));


                    status.CricketID = cricket.ID;
                    #region
                    aptitude.SkillDex = cricketPoint.Dex;
                    aptitude.SkillDef = cricketPoint.Def;
                    aptitude.SkillStr = cricketPoint.Str;
                    aptitude.SkillCon = cricketPoint.Con;
                    #endregion
                    NHibernateQuerier.Update(cricket);
                    NHibernateQuerier.Update(status);
                    NHibernateQuerier.Update(aptitude);
                    var data = xRCommon.xRS2CParams();
                    data.Add((byte)ParameterCode.Cricket, SetCricketValue(cricket));
                    data.Add((byte)ParameterCode.CricketStatus, status);
                    data.Add((byte)ParameterCode.CricketAptitude, aptitude);
                    data.Add((byte)ParameterCode.CricketPoint, point);
                    var dict = xRCommon.xRS2CSub();
                    dict.Add((byte)CricketOperateType.UpdateSkill, Utility.Json.ToJson(data));
                    xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncCricket, (byte)ReturnCode.Success, dict);
                    InventoryManager.xRUpdateInventory(roleid, new Dictionary<int, ItemDTO> { { prop, new ItemDTO() { ItemAmount = 1 } } });
                    return;
                }
                else
                {
                    xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncCricket, (byte)ReturnCode.Fail, xRCommonTip.xR_err_RemoveSkill);
                    Utility.Debug.LogInfo("没有可以删除的技能");
                    return;
                }
            }
            else
            {
                xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncCricket, (byte)ReturnCode.Fail, xRCommonTip.xR_err_Verify);
                Utility.Debug.LogInfo("数据库没有该技能");
            }

        }
        /// <summary>
        /// 添加特殊技能
        /// </summary>
        /// <param name="skillid"></param>
        /// <param name="roleid"></param>
        public static void AddSpecialSkill(int skillid, int level, int roleid, int cricketid)
        {
            Utility.Debug.LogError("添加的特殊技能为" + skillid + "蛐蛐id为" + cricketid + "玩家ID" + roleid);
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, BattleAttackSkillData>>(out var skillDict);
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, PassiveSkill>>(out var specialSkillDict);
            if (!skillDict.ContainsKey(skillid) && !specialSkillDict.ContainsKey(skillid))
            {
                return;
            }
            else
            {
                if (specialSkillDict[skillid].SkillType == 1)
                {
                    StudySkill(skillid, cricketid, roleid);
                    return;
                }
            }
            var nHCriteria = xRCommon.xRNHCriteria("ID", cricketid);
            var nHCriteriacricket = xRCommon.xRNHCriteria("CricketID", cricketid);
            var cricket = xRCommon.xRCriteria<Cricket>(nHCriteria);
            var point = xRCommon.xRCriteria<CricketPoint>(nHCriteriacricket);
            var aptitude = xRCommon.xRCriteria<CricketAptitude>(nHCriteriacricket);
            var addition = xRCommon.xRCriteria<CricketAddition>(nHCriteriacricket);
            var skills = Utility.Json.ToObject<Dictionary<int, int>>(cricket.SpecialDict);
            if (cricket != null && point != null && aptitude != null && addition != null)
            {
                if (!skills.ContainsKey(skillid))
                {
                    skills.Add(skillid, level);
                }
                else
                {
                    if (skills[skillid] + level < 10)
                    {
                        skills[skillid] += level;
                    }
                    else
                        skills[skillid] = 10;
                }

                cricket.SpecialDict = Utility.Json.ToJson(skills);
                Utility.Debug.LogError("添加的特殊技能为" + cricket.SpecialDict);
                //var status = CalculateStutas(aptitude, point, addition);
                var status = SkillAdditionStatus(cricket, aptitude, point, addition, out var cricketPoint);
                status.CricketID = cricket.ID;
                #region
                aptitude.SkillDex = cricketPoint.Dex;
                aptitude.SkillDef = cricketPoint.Def;
                aptitude.SkillStr = cricketPoint.Str;
                aptitude.SkillCon = cricketPoint.Con;
                #endregion
                NHibernateQuerier.Update(cricket);
                NHibernateQuerier.Update(status);
                NHibernateQuerier.Update(aptitude);

                var data = xRCommon.xRS2CParams();
                data.Add((byte)ParameterCode.CricketStatus, status);
                data.Add((byte)ParameterCode.CricketAptitude, aptitude);
                data.Add((byte)ParameterCode.Cricket, SetCricketValue(cricket));
                data.Add((byte)ParameterCode.CricketPoint, point);
                var dict = xRCommon.xRS2CSub();
                dict.Add((byte)CricketOperateType.UpdateSkill, Utility.Json.ToJson(data));
                xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncCricket, (short)ReturnCode.Success, dict);
            }
            else
                xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncCricket, (short)ReturnCode.Fail, xRCommonTip.xR_err_Verify);

        }
        /// <summary>
        /// 计算加经验的
        /// </summary>
        /// <param name="cricket"></param>
        /// <returns></returns>
        public static Cricket LevelUpCalculate(Cricket cricket)
        {

            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, CricketLevel>>(out var cricketLevelDict);
            Utility.Debug.LogInfo("蛐蛐等级" + cricket.LevelID);
            if (cricket.LevelID < 100)
            {
                if (cricket.Exp >= cricketLevelDict[cricket.LevelID].ExpUP)
                {
                    cricket.Exp -= cricketLevelDict[cricket.LevelID].ExpUP;
                    cricket.LevelID += 1;
                    LevelUpCalculate(cricket);
                }
            }

            return cricket;
        }
        /// <summary>
        /// 设置cricketDTO字段赋值
        /// </summary>
        /// <param name="cricket"></param>
        /// <returns></returns>
        public static CricketDTO SetCricketValue(Cricket cricket)
        {
            CricketDTO cricketDTO = new CricketDTO();

            cricketDTO.ID = cricket.ID;
            cricketDTO.CricketID = cricket.CricketID;
            cricketDTO.CricketName = cricket.CricketName;
            cricketDTO.Exp = cricket.Exp;
            cricketDTO.LevelID = cricket.LevelID;
            cricketDTO.RankID = cricket.RankID;
            cricketDTO.SkillDict = Utility.Json.ToObject<Dictionary<int, int>>(cricket.SkillDict);
            cricketDTO.SpecialDict = Utility.Json.ToObject<Dictionary<int, int>>(cricket.SpecialDict);
            cricketDTO.HeadPortraitID = cricket.HeadPortraitID;

            return cricketDTO;
        }
        /// <summary>
        /// 升级蛐蛐技能
        /// </summary>
        /// <param name="roleid"></param>
        /// <param name="propData"></param>
        /// <param name="cricket"></param>
        public static void UpdateCricketSkill(int roleid, RolepPropDTO rolepPropDTO, int cricketid)
        {
            var result = VerifyProp(rolepPropDTO.PropID, PropType.Skill, out var prop);
            if (!InventoryManager.xRVerifyInventory(roleid, rolepPropDTO.PropID, rolepPropDTO.PropNum))
            {
                xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncCricket, (short)ReturnCode.Fail, xRCommonTip.xR_err_VerifyProp);
                return;
            }

            if (result)
            {
                var nHCriteriacricket = xRCommon.xRNHCriteria("ID", cricketid);
                var nHCriteriastatus = xRCommon.xRNHCriteria("CricketID", cricketid);
                var cricket = xRCommon.xRCriteria<Cricket>(nHCriteriacricket);
                var cricketstatus = xRCommon.xRCriteria<CricketStatus>(nHCriteriastatus);
                var point = xRCommon.xRCriteria<CricketPoint>(nHCriteriastatus);
                var aptitude = xRCommon.xRCriteria<CricketAptitude>(nHCriteriastatus);
                var addition = xRCommon.xRCriteria<CricketAddition>(nHCriteriastatus);

                if (cricket != null && cricketstatus != null)
                {
                    var skillDict = Utility.Json.ToObject<Dictionary<int, int>>(cricket.SkillDict);
                    var exits = skillDict.TryGetValue(prop.SkillID, out int level);
                    Utility.Debug.LogInfo("YZQ升级技能" + exits + level);
                    if (exits)
                    {
                        if (level < 10 && (level + 1) == rolepPropDTO.PropNum)
                        {
                            level += 1;
                            skillDict[prop.SkillID] = level;

                            InventoryManager.xRUpdateInventory(roleid, new Dictionary<int, ItemDTO> { { rolepPropDTO.PropID, new ItemDTO() { ItemAmount = rolepPropDTO.PropNum } } });

                            cricket.SkillDict = Utility.Json.ToJson(skillDict);
                            NHibernateQuerier.Update(cricket);

                            var status = SkillAdditionStatus(cricket, aptitude, point, addition, out var cricketPoint);
                            status.CricketID = cricket.ID;
                            #region
                            aptitude.SkillDex = cricketPoint.Dex;
                            aptitude.SkillDef = cricketPoint.Def;
                            aptitude.SkillStr = cricketPoint.Str;
                            aptitude.SkillCon = cricketPoint.Con;
                            #endregion
                            NHibernateQuerier.Update(cricket);
                            NHibernateQuerier.Update(status);
                            NHibernateQuerier.Update(aptitude);

                            var data = xRCommon.xRS2CParams();
                            data.Add((byte)ParameterCode.CricketStatus, status);
                            data.Add((byte)ParameterCode.CricketAptitude, aptitude);
                            data.Add((byte)ParameterCode.Cricket, SetCricketValue(cricket));
                            data.Add((byte)ParameterCode.CricketPoint, point);



                            var dict = xRCommon.xRS2CSub();
                            dict.Add((byte)CricketOperateType.UpdateSkill, Utility.Json.ToJson(data));
                            xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncCricket, (byte)ReturnCode.Success, dict);

                        }
                        else
                            xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncCricket, (short)ReturnCode.Fail, xRCommonTip.xR_err_Verify);
                    }
                }
                else
                    xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncCricket, (short)ReturnCode.Fail, xRCommonTip.xR_err_Verify);
            }
            else
                xRCommon.xRS2CSend(roleid, (ushort)ATCmd.SyncCricket, (short)ReturnCode.Fail, xRCommonTip.xR_err_Verify);
        }

        public static CricketStatus SkillAdditionStatus(Cricket cricket, CricketAptitude cricketAptitude, CricketPoint cricketPoint, CricketAddition cricketAddition, out CricketPoint cricketPointTemp)
        {
            cricketPointTemp = new CricketPoint();
            Utility.Debug.LogError("加成前的數值" + Utility.Json.ToJson(cricket) + ">>>>>>>" + Utility.Json.ToJson(cricketPoint));
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, PassiveSkill>>(out var passiveSkill);
            var statusPercentage = new CricketStatusDTO();
            var statusFixed = new CricketStatusDTO();
            statusPercentage.clear();
            statusFixed.clear();
            var skill = Utility.Json.ToObject<Dictionary<int, int>>(cricket.SkillDict);
            var Special = Utility.Json.ToObject<Dictionary<int, int>>(cricket.SpecialDict);
            var SkillDict = new Dictionary<int, int>();
            if (skill.Count > 0)
            {
                foreach (var item in skill)
                {
                    SkillDict.Add(item.Key, item.Value);
                }
            }
            if (Special.Count > 0)
            {
                foreach (var item in Special)
                {
                    if (!SkillDict.ContainsKey(item.Key))
                    {
                        SkillDict.Add(item.Key, item.Value);
                    }
                }
            }
            if (SkillDict.Count > 0)
            {
                foreach (var item in SkillDict)
                {
                    var result = passiveSkill.TryGetValue(item.Key, out var passive);
                    if (result)
                    {
                        for (int i = 0; i < passive.Attribute.Count; i++)
                        {
                            switch ((SkillAdditionType)passive.Attribute[i])
                            {
                                case SkillAdditionType.Str:
                                    cricketPointTemp.Str += passive.Fixed[i] + passive.LevelFixed[i] * item.Value;
                                    break;
                                case SkillAdditionType.Con:
                                    cricketPointTemp.Con += passive.Fixed[i] + passive.LevelFixed[i] * item.Value;
                                    break;
                                case SkillAdditionType.Dex:
                                    cricketPointTemp.Dex += passive.Fixed[i] + passive.LevelFixed[i] * item.Value;
                                    break;
                                case SkillAdditionType.Def:
                                    cricketPointTemp.Def += passive.Fixed[i] + passive.LevelFixed[i] * item.Value;
                                    break;
                                case SkillAdditionType.Atk:
                                    statusFixed.Atk += passive.Fixed[i] + passive.LevelFixed[i] * item.Value;
                                    statusPercentage.Atk += passive.Percentage[i] + passive.LevelPercentage[i] * item.Value;
                                    break;
                                case SkillAdditionType.Hp:
                                    statusFixed.Hp += passive.Fixed[i] + passive.LevelFixed[i] * item.Value;
                                    statusPercentage.Hp += passive.Percentage[i] + passive.LevelPercentage[i] * item.Value;
                                    break;
                                case SkillAdditionType.Defense:
                                    statusFixed.Defense += passive.Fixed[i] + passive.LevelFixed[i] * item.Value;
                                    statusPercentage.Defense += passive.Percentage[i] + passive.LevelPercentage[i] * item.Value;
                                    break;
                                case SkillAdditionType.Mp:
                                    statusFixed.Mp += passive.Fixed[i] + passive.LevelFixed[i] * item.Value;
                                    statusPercentage.Mp += passive.Percentage[i] + passive.LevelPercentage[i] * item.Value;
                                    break;
                                case SkillAdditionType.MpReply:
                                    statusFixed.MpReply += passive.Fixed[i] + passive.LevelFixed[i] * item.Value;
                                    statusPercentage.MpReply += passive.Percentage[i] + passive.LevelPercentage[i] * item.Value;
                                    break;
                                case SkillAdditionType.Speed:
                                    statusFixed.Speed += passive.Fixed[i] + passive.LevelFixed[i] * item.Value;
                                    statusPercentage.Speed += passive.Percentage[i] + passive.LevelPercentage[i] * item.Value;
                                    break;
                                case SkillAdditionType.Crt:
                                    statusFixed.Crt += passive.Fixed[i] + passive.LevelFixed[i] * item.Value;
                                    statusPercentage.Crt += passive.Percentage[i] + passive.LevelPercentage[i] * item.Value;
                                    break;
                                case SkillAdditionType.Eva:
                                    statusFixed.Eva += passive.Fixed[i] + passive.LevelFixed[i] * item.Value;
                                    statusPercentage.Eva += passive.Percentage[i] + passive.LevelPercentage[i] * item.Value;
                                    break;
                                case SkillAdditionType.ReduceAtk:
                                    statusFixed.ReduceAtk += passive.Fixed[i] + passive.LevelFixed[i] * item.Value;
                                    statusPercentage.ReduceAtk += passive.Percentage[i] + passive.LevelPercentage[i] * item.Value;
                                    break;
                                case SkillAdditionType.ReduceDef:
                                    statusFixed.ReduceDef += passive.Fixed[i] + passive.LevelFixed[i] * item.Value;
                                    statusPercentage.ReduceDef += passive.Percentage[i] + passive.LevelPercentage[i] * item.Value;
                                    break;
                                case SkillAdditionType.Rebound:
                                    statusFixed.Rebound += passive.Fixed[i] + passive.LevelFixed[i] * item.Value;
                                    statusPercentage.Rebound += passive.Percentage[i] + passive.LevelPercentage[i] * item.Value;
                                    break;
                                case SkillAdditionType.CrtAtk:
                                    statusFixed.CrtAtk += passive.Fixed[i] + passive.LevelFixed[i] * item.Value;
                                    statusPercentage.CrtAtk += passive.Percentage[i] + passive.LevelPercentage[i] * item.Value;
                                    break;
                                case SkillAdditionType.CrtDef:
                                    statusFixed.CrtDef += passive.Fixed[i] + passive.LevelFixed[i] * item.Value;
                                    statusPercentage.CrtDef += passive.Percentage[i] + passive.LevelPercentage[i] * item.Value;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            //Utility.Debug.LogError("技能加成后的數值1"+Utility.Json.ToJson(cricketPoint));
            //Utility.Debug.LogError("技能加成后的數值2" + Utility.Json.ToJson(statusPercentage));
            //Utility.Debug.LogError("技能加成后的數值3" + Utility.Json.ToJson(statusFixed));

            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, CricketStatusData>>(out var StatusDict);
            CricketStatus cricketStatuTemp = new CricketStatus();

            cricketStatuTemp.Atk = (int)(statusFixed.Atk + cricketAddition.Atk + StatusDict[1].Atk + ((cricketAptitude.Str + cricketPointTemp.Str + cricketPoint.Str) * (cricketAptitude.StrAptitude + 100) * 0.01f)) * (100 + statusPercentage.Atk) / 100;
            cricketStatuTemp.Defense = (int)(statusFixed.Defense + cricketAddition.Defense + StatusDict[1].Defense + ((cricketAptitude.Def + cricketPoint.Def + cricketPointTemp.Def) * (cricketAptitude.DefAptitude + 100) * 0.005f)) * (100 + statusPercentage.Defense) / 100;
            cricketStatuTemp.Hp = (int)(statusFixed.Hp + cricketAddition.Hp + StatusDict[1].Hp + ((cricketAptitude.Con + cricketPoint.Con + cricketPointTemp.Con) * (cricketAptitude.ConAptitude + 100) * 0.05f)) * (100 + statusPercentage.Hp) / 100;
            cricketStatuTemp.Mp = (int)(StatusDict[1].Mp + cricketAddition.Mp + statusFixed.Mp + cricketStatuTemp.Hp / 100);
            cricketStatuTemp.MpReply = (int)(StatusDict[1].MpReply + cricketAddition.MpReply + statusFixed.MpReply + cricketStatuTemp.Mp / 10);
            cricketStatuTemp.Crt = ((cricketAptitude.Dex + cricketPoint.Dex + cricketPointTemp.Dex) * (300 - (2 * (100 - cricketAptitude.DexAptitude))) / 1000000f) + statusPercentage.Crt / 100;
            cricketStatuTemp.Eva = ((cricketAptitude.Dex + cricketPoint.Dex + cricketPointTemp.Dex) * (300 - (2 * (100 - cricketAptitude.DexAptitude))) / 1000000f) + statusPercentage.Eva / 100;
            cricketStatuTemp.Speed = (int)(statusFixed.Speed + StatusDict[1].Speed - ((cricketAptitude.Dex + cricketPoint.Dex + cricketPointTemp.Dex) * (1.5f - (0.01 * (100 - cricketAptitude.DexAptitude))))) * (100 + statusPercentage.Speed) / 100;
            if (cricketStatuTemp.Speed<=500)
            {
                cricketStatuTemp.Speed = 500;
            }
            cricketStatuTemp.ReduceAtk = (int)(statusFixed.ReduceAtk + StatusDict[1].ReduceAtk) * (100 + statusPercentage.ReduceAtk) / 100;
            cricketStatuTemp.ReduceDef = (int)(statusFixed.ReduceDef + StatusDict[1].ReduceDef) * (100 + statusPercentage.ReduceDef) / 100;
            cricketStatuTemp.Rebound = (int)(statusFixed.Rebound + StatusDict[1].Rebound) * (100 + statusPercentage.Rebound) / 100;
            //Utility.Debug.LogInfo("闪避固定值" + (cricketAptitude.Dex + cricketPoint.Dex) + "资质" + cricketAptitude.DexAptitude + "值" + (300 - (2 * (100f - cricketAptitude.DexAptitude))) / 1000000f + "计算值" + cricketStatuTemp.Eva);
            Utility.Debug.LogError("技能加成后的暴击值" + statusFixed.Crt + "百分比" + statusPercentage.Crt + "闪避" + statusFixed.Eva + "百分比" + statusPercentage.Eva);
            return CalibrationNum(cricketStatuTemp);

        }
        static CricketStatus CalibrationNum(CricketStatus cricketStatus)
        {
            if (cricketStatus.Atk < 0)
            {
                cricketStatus.Atk = 0;
            }
            if (cricketStatus.Crt < 0)
            {
                cricketStatus.Crt = 0;
            }
            if (cricketStatus.CrtAtk < 0)
            {
                cricketStatus.CrtAtk = 0;
            }
            if (cricketStatus.CrtDef < 0)
            {
                cricketStatus.CrtDef = 0;
            }
            if (cricketStatus.Defense < 0)
            {
                cricketStatus.Defense = 0;
            }
            if (cricketStatus.Eva < 0)
            {
                cricketStatus.Eva = 0;
            }
            if (cricketStatus.Hp < 0)
            {
                cricketStatus.Hp = 0;
            }
            if (cricketStatus.Mp < 0)
            {
                cricketStatus.Mp = 0;
            }
            if (cricketStatus.MpReply < 0)
            {
                cricketStatus.MpReply = 0;
            }
            if (cricketStatus.Rebound < 0)
            {
                cricketStatus.Rebound = 0;
            }
            if (cricketStatus.ReduceAtk < 0)
            {
                cricketStatus.ReduceAtk = 0;
            }
            if (cricketStatus.ReduceDef < 0)
            {
                cricketStatus.ReduceDef = 0;
            }
            if (cricketStatus.Speed < 0)
            {
                cricketStatus.Speed = 0;
            }
            return cricketStatus;
        }

        public enum SkillAdditionType
        {
            Str = 1,
            Con = 2,
            Dex = 3,
            Def = 4,
            Atk = 5,
            Hp = 6,
            Defense = 7,
            Mp = 8,
            MpReply = 9,
            Speed = 10,
            Crt = 11,
            Eva = 12,
            ReduceAtk = 13,
            ReduceDef = 14,
            Rebound = 15,
            CrtAtk = 16,
            CrtDef = 17
        }
    }
}
