using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.SocketServer;
using AscensionProtocol;
using ExitGames.Logging;
using ExitGames.Logging.Log4Net;
using System.IO;
using log4net.Config;
using System.Reflection;
using ExitGames.Concurrency.Fibers;
using Cosmos;


namespace AscensionServer
{
    /// <summary>
    /// 服务器数据转换
    /// </summary>
    [ImplementProvider]
    public class ServerDataConvertor : IDataConvertor
    {
        public void ConvertData()
        {
            try
            {

                #region 获取Json文件转换成对应数据类
                #region  蛐蛐数据,属性,等级,窝升级
                GameManager.CustomeModule<DataManager>().TryGetValue(typeof(CricketData).Name, out var CricketDataData);
                var CricketDataDict = TransObject<List<CricketData>>(CricketDataData).ToDictionary(key => key.CricketID, value => value);
                GameManager.CustomeModule<DataManager>().TryGetValue(typeof(CricketStatusData).Name, out var CricketStatusDataData);
                var CricketStatusDict = TransObject<List<CricketStatusData>>(CricketStatusDataData).ToDictionary(key => key.Level, value => value);
                GameManager.CustomeModule<DataManager>().TryGetValue(typeof(CricketLevel).Name, out var CricketLevelData);
                var CricketLevelDict = TransObject<List<CricketLevel>>(CricketLevelData).ToDictionary(key => key.Level, value => value);

                GameManager.CustomeModule<DataManager>().TryGetValue(typeof(NestLevel).Name, out var nestLevel);
                var nestLevelDict = TransObject<List<NestLevel>>(nestLevel).ToDictionary(key => key.NestID, value => value);

                #endregion

                GameManager.CustomeModule<DataManager>().TryGetValue(typeof(PropData).Name, out var propData);
                var propDataDict = TransObject<List<PropData>>(propData).ToDictionary(key => key.PropID, value => value);
                GameManager.CustomeModule<DataManager>().TryGetValue(typeof(Shop).Name, out var shopData);
                var shopDict = TransObject<List<Shop>>(shopData).ToDictionary(key => key.PropID, value => value);
                GameManager.CustomeModule<DataManager>().TryGetValue(typeof(TaskData).Name, out var taskData);
                var taskDict = TransObject<List<TaskData>>(taskData).ToDictionary(key => key.TaskId, value => value);
                GameManager.CustomeModule<DataManager>().TryGetValue(typeof(ExplorationData).Name, out var explorationData);
                var explorationDict = TransObject<List<ExplorationData>>(explorationData).ToDictionary(key => key.EventID, value => value);

                GameManager.CustomeModule<DataManager>().TryGetValue(typeof(BattleAttackSkillData).Name, out var battleAttackSkillDataData);
                var battleAttackSkillDataDict = TransObject<List<BattleAttackSkillData>>(battleAttackSkillDataData).ToDictionary(key => key.skillId, value => value);
               
                GameManager.CustomeModule<DataManager>().TryGetValue(typeof(BattleBuffData).Name, out var battleBuffDataData);
                var battleBuffDataDict= TransObject<List<BattleBuffData>>(battleBuffDataData).ToDictionary(key => key.buffId, value => value);

                GameManager.CustomeModule<DataManager>().TryGetValue(typeof(ADAward).Name, out var aDAwardData);
                var aDAwardDict = TransObject<List<ADAward>>(aDAwardData).ToDictionary(key => key.PropID, value => value);

                GameManager.CustomeModule<DataManager>().TryGetValue(typeof(PassiveSkill).Name, out var passiveSkill);
                var passiveSkillDict = TransObject<List<PassiveSkill>>(passiveSkill).ToDictionary(key => key.SkillID, value => value);

                GameManager.CustomeModule<DataManager>().TryGetValue(typeof(SpreaAward).Name, out var spreaAward);
                var spreaAwardDict = TransObject<List<SpreaAward>>(spreaAward).ToDictionary(key => key.GiftID, value => value);

                GameManager.CustomeModule<DataManager>().TryGetValue(typeof(MachineData).Name, out var machineData);
                var machineDict = TransObject<List<MachineData>>(machineData).ToDictionary(key => key.RankID, value => value);

                bool sda= GameManager.CustomeModule<DataManager>().TryGetValue(typeof(RankLevel).Name, out var rankLevel);
                var rankLevelDict = TransObject<List<RankLevel>>(rankLevel).ToDictionary(key => key.RankID, value => value);

                GameManager.CustomeModule<DataManager>().TryGetValue(typeof(HeadPortraitData).Name, out var headPortraitData);
                var headPortraitDataDict = TransObject<List<HeadPortraitData>>(headPortraitData).ToDictionary(key => key.PlayerHeadID, value => value);

                GameManager.CustomeModule<DataManager>().TryGetValue(typeof(CricketHeadPortraitData).Name, out var cricketHeadPortraitData);
                var cricketHeadPortraitDataList = TransObject<List<CricketHeadPortraitData>>(cricketHeadPortraitData);
                var cricketHeadPortraitDataDict = cricketHeadPortraitDataList.ToDictionary(key => key.CricketID, value => value);

                GameManager.CustomeModule<DataManager>().TryGetValue(typeof(CricketNameData).Name, out var cricketNameData);
                var cricketNameDataList = TransObject<List<CricketNameData>>(cricketNameData);
                var cricketNameDataDict = cricketNameDataList.ToDictionary(key => key.NameID, value => value);

                GameManager.CustomeModule<DataManager>().TryGetValue(typeof(TowerDifficultyData).Name, out var towerDifficultyData);
                var towerDifficultyDataDict = TransObject<List<TowerDifficultyData>>(towerDifficultyData).ToDictionary(key => key.DifficultyId, value => value);

                GameManager.CustomeModule<DataManager>().TryGetValue(typeof(TowerRobotData).Name, out var towerRobotData);
                var towerRobotDataDict = TransObject<List<TowerRobotData>>(towerRobotData).ToDictionary(key => key.CricketId, value => value);

                GameManager.CustomeModule<DataManager>().TryGetValue(typeof(TowerFloorData).Name, out var towerFloorData);
                var towerFloorDataDict = TransObject<List<TowerFloorData>>(towerFloorData).ToDictionary(key => key.FloorId, value => value);

                GameManager.CustomeModule<DataManager>().TryGetValue(typeof(TowerAwardData).Name, out var towerAwardData);
                var towerAwardDataDict = TransObject<List<TowerAwardData>>(towerAwardData).ToDictionary(key => key.RewardID, value => value);

                GameManager.CustomeModule<DataManager>().TryGetValue(typeof(MatchRobotNameData).Name, out var matchRobotNameData);
                var matchRobotNameDataList = TransObject<List<MatchRobotNameData>>(matchRobotNameData);
                #endregion

                #region 储存方式 
                GameManager.CustomeModule<DataManager>().TryAdd(cricketNameDataList);
                GameManager.CustomeModule<DataManager>().TryAdd(cricketNameDataDict);
                GameManager.CustomeModule<DataManager>().TryAdd(cricketHeadPortraitDataList);
                GameManager.CustomeModule<DataManager>().TryAdd(cricketHeadPortraitDataDict);
                GameManager.CustomeModule<DataManager>().TryAdd(headPortraitDataDict);
                GameManager.CustomeModule<DataManager>().TryAdd(spreaAwardDict);
                GameManager.CustomeModule<DataManager>().TryAdd(passiveSkillDict);
                GameManager.CustomeModule<DataManager>().TryAdd(aDAwardDict);
                GameManager.CustomeModule<DataManager>().TryAdd(explorationDict);
                GameManager.CustomeModule<DataManager>().TryAdd(taskDict);
                GameManager.CustomeModule<DataManager>().TryAdd(shopDict);
                GameManager.CustomeModule<DataManager>().TryAdd(propDataDict);
                GameManager.CustomeModule<DataManager>().TryAdd(nestLevelDict);
                GameManager.CustomeModule<DataManager>().TryAdd(CricketLevelDict);
                GameManager.CustomeModule<DataManager>().TryAdd(CricketDataDict);
                GameManager.CustomeModule<DataManager>().TryAdd(battleAttackSkillDataDict);
                GameManager.CustomeModule<DataManager>().TryAdd(CricketStatusDict);
                GameManager.CustomeModule<DataManager>().TryAdd(battleBuffDataDict);
                GameManager.CustomeModule<DataManager>().TryAdd(machineDict);
                GameManager.CustomeModule<DataManager>().TryAdd(rankLevelDict);
                GameManager.CustomeModule<DataManager>().TryAdd(towerDifficultyDataDict);
                GameManager.CustomeModule<DataManager>().TryAdd(towerRobotDataDict);
                GameManager.CustomeModule<DataManager>().TryAdd(towerFloorDataDict);
                GameManager.CustomeModule<DataManager>().TryAdd(towerAwardDataDict);
                GameManager.CustomeModule<DataManager>().TryAdd(matchRobotNameDataList);
                #endregion

                #region 获取方式
                //GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, MonsterDatas>>(out var set);
                Utility.Debug.LogInfo("<DataManager> 测试 TryGetValue " +Utility.Json.ToJson(cricketNameDataDict));
                #endregion
            }
            catch (Exception e)
            {
                Utility.Debug.LogError(e);
            }
        }

        /// <summary>
        /// 转换为对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public T TransObject<T>(string data)
        {
            return Utility.Json.ToObject<T>(data);
        }
    }
}
