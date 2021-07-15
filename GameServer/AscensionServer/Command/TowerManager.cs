using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol;
using Cosmos;
using Protocol;
using RedisDotNet;

namespace AscensionServer
{
    [CustomeModule]
     public class TowerManager:Module<TowerManager>
    {
        //每日最大挑战次数
        const int maxChallengeCount = 999;

        public override void OnPreparatory()
        {
            CommandEventCore.Instance.AddEventListener((ushort)ATCmd.SyncTower, TowerC2S);
        }

        //获取玩家爬塔信息并返回给客户端
       async  void GetTowerInfoS2C(int roleId)
        {

            NHCriteria nHCriteria = xRCommon.xRNHCriteria("RoleID", roleId);
            Tower tower= xRCommon.xRCriteria<Tower>(nHCriteria);
            if (tower == null)
            {
                tower = new Tower() { RoleID = roleId };
                NHibernateQuerier.Insert(tower);
            }
            int remainChallengeCount = 0;
            if (await RedisHelper.Hash.HashExistAsync(RedisKeyDefine._TowerChallengeCountPerfix, roleId.ToString()))
                remainChallengeCount = await RedisHelper.Hash.HashGetAsync<int>(RedisKeyDefine._TowerChallengeCountPerfix, roleId.ToString());
            else
            {
                remainChallengeCount = maxChallengeCount;
                await RedisHelper.Hash.HashSetAsync<int>(RedisKeyDefine._TowerChallengeCountPerfix, roleId.ToString(), remainChallengeCount);
            }

            TowerDTO result = new TowerDTO()
            {
                RoleID = roleId,
                MaxDifficulty = tower.MaxDifficulty,
                NowChooseDifficulty = tower.NowChooseDifficulty,
                NowLevel = tower.NowLevel,
                RemainChallengeCount = remainChallengeCount,
                NowEnemyName=tower.NowEnemyName,
                NowEnemyIconId=tower.NowEnemyIconId
            };
            OperationData operationData = new OperationData();
            operationData.DataMessage = Utility.Json.ToJson(result);
            operationData.OperationCode = (ushort)ATCmd.SyncTower;
            operationData.SubOperationCode = (short)TowerOpCode.GetTowerData;
            GameManager.CustomeModule<RoleManager>().SendMessage(roleId, operationData);
            Utility.Debug.LogError("发送爬塔信息信息"+roleId);
        }
        async void ChooseDifficultyS2C(int roleId,int chooseDifficultyId)
        {
            int remainChallengeCount = 0;
            if (await RedisHelper.Hash.HashExistAsync(RedisKeyDefine._TowerChallengeCountPerfix, roleId.ToString()))
                remainChallengeCount = await RedisHelper.Hash.HashGetAsync<int>(RedisKeyDefine._TowerChallengeCountPerfix, roleId.ToString());
            else
                remainChallengeCount = maxChallengeCount;
            if (remainChallengeCount <= 0)//挑战次数不够
                return;
            NHCriteria nHCriteria = xRCommon.xRNHCriteria("RoleID", roleId);
            Tower tower = xRCommon.xRCriteria<Tower>(nHCriteria);
            if (tower == null)
                return;
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, TowerDifficultyData>>(out var towerDifficultyDataDict);
            if (!towerDifficultyDataDict.ContainsKey(chooseDifficultyId))//难度id不正确
                return;
            tower.NowChooseDifficulty = chooseDifficultyId;
            tower.NowLevel = 0;

            //随机敌人的名字头像模型
            RandomEnemyInfo(tower);


            remainChallengeCount--;
            NHibernateQuerier.Update(tower);
            await RedisHelper.Hash.HashSetAsync<int>(RedisKeyDefine._TowerChallengeCountPerfix, roleId.ToString(), remainChallengeCount);
            TowerDTO result = new TowerDTO()
            {
                RoleID = roleId,
                MaxDifficulty = tower.MaxDifficulty,
                NowChooseDifficulty = tower.NowChooseDifficulty,
                NowLevel = tower.NowLevel,
                RemainChallengeCount = remainChallengeCount,
                NowEnemyName = tower.NowEnemyName,
                NowEnemyIconId = tower.NowEnemyIconId
            };
            OperationData operationData = new OperationData();
            operationData.DataMessage = Utility.Json.ToJson(result);
            operationData.OperationCode = (ushort)ATCmd.SyncTower;
            operationData.SubOperationCode = (short)TowerOpCode.ChooseDifficulty;
            GameManager.CustomeModule<RoleManager>().SendMessage(roleId, operationData);
            Utility.Debug.LogError("发送选择难度信息" + roleId);
        }

         void AbandonTowerS2C(int roleId)
        {

            NHCriteria nHCriteria = xRCommon.xRNHCriteria("RoleID", roleId);
            Tower tower = xRCommon.xRCriteria<Tower>(nHCriteria);
            if (tower == null)
                return;
            tower.NowChooseDifficulty = -1;
            tower.NowLevel = 0;
            NHibernateQuerier.Update(tower);
            TowerDTO result = new TowerDTO()
            {
                RoleID = roleId,
                MaxDifficulty = tower.MaxDifficulty,
                NowChooseDifficulty = tower.NowChooseDifficulty,
                NowLevel = tower.NowLevel,
                NowEnemyName = tower.NowEnemyName,
                NowEnemyIconId = tower.NowEnemyIconId
            };
            OperationData operationData = new OperationData();
            operationData.DataMessage = Utility.Json.ToJson(result);
            operationData.OperationCode = (ushort)ATCmd.SyncTower;
            operationData.SubOperationCode = (short)TowerOpCode.Abandon;
            GameManager.CustomeModule<RoleManager>().SendMessage(roleId, operationData);
            Utility.Debug.LogError("发送放弃爬塔信息" + roleId);
        }

        void StartBattle(int roleId, int chooseCricketId)
        {
            NHCriteria nHCriteriaRole = xRCommon.xRNHCriteria("RoleID", roleId);
            NHCriteria nHCriteriaCricket = xRCommon.xRNHCriteria("ID", chooseCricketId);
            Tower tower = xRCommon.xRCriteria<Tower>(nHCriteriaRole);
            Utility.Debug.LogError(Utility.Json.ToJson(tower));   
            Role role = xRCommon.xRCriteria<Role>(nHCriteriaRole);
            RoleDTO roleDTO = Utility.Json.ToObject<RoleDTO>(Utility.Json.ToJson(role));
            Cricket cricket = xRCommon.xRCriteria<Cricket>(nHCriteriaCricket);
            CricketDTO cricketDTO = new CricketDTO()
            {
                ID = cricket.ID,
                CricketID = cricket.CricketID,
                LevelID = cricket.LevelID,
                Exp = cricket.Exp,
                CricketName = cricket.CricketName,
                RankID = cricket.RankID,
                HeadPortraitID = cricket.HeadPortraitID,
                SkillDict = Utility.Json.ToObject<Dictionary<int, int>>(cricket.SkillDict),
                SpecialDict = Utility.Json.ToObject<Dictionary<int, int>>(cricket.SpecialDict),
            };
            //开启战斗
            GameManager.CustomeModule<BattleRoomManager>().CreateRoom(roleDTO, cricketDTO, tower, BattleCombat);
        }
        void AddTowerLevelS2C(Tower tower)
        {
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, TowerDifficultyData>>(out var towerDifficultyDataDict);
            TowerDifficultyData towerDifficultyData = towerDifficultyDataDict[tower.NowChooseDifficulty];
            if (towerDifficultyData.FloorIdArray.Count > tower.NowLevel + 1)//还有下一层挑战
            {
                tower.NowLevel++;
                //随机敌人的名字头像模型
                RandomEnemyInfo(tower);
            }
            else //该难度挑战结束
            {
                tower.NowChooseDifficulty = -1;
                tower.NowLevel = 0;
                if (towerDifficultyDataDict.ContainsKey(tower.MaxDifficulty + 1))
                    tower.MaxDifficulty++;
            }

            NHibernateQuerier.Update(tower);
            TowerDTO result = new TowerDTO()
            {
                RoleID = tower.RoleID,
                MaxDifficulty = tower.MaxDifficulty,
                NowChooseDifficulty = tower.NowChooseDifficulty,
                NowLevel = tower.NowLevel,
                NowEnemyName = tower.NowEnemyName,
                NowEnemyIconId = tower.NowEnemyIconId
            };
            OperationData operationData = new OperationData();
            operationData.DataMessage = Utility.Json.ToJson(result);
            operationData.OperationCode = (ushort)ATCmd.SyncTower;
            operationData.SubOperationCode = (short)TowerOpCode.StartBattle;
            GameManager.CustomeModule<RoleManager>().SendMessage(tower.RoleID, operationData);
            Utility.Debug.LogError("发送增加爬塔层数信息" + tower.RoleID);
        }

        void TowerC2S(OperationData opData)
        {
            TowerDTO towerDTO = Utility.Json.ToObject<TowerDTO>(opData.DataMessage.ToString());
            switch ((TowerOpCode)opData.SubOperationCode)
            {
                case TowerOpCode.GetTowerData:
                    GetTowerInfoS2C(towerDTO.RoleID);
                    break;
                case TowerOpCode.Abandon:
                    AbandonTowerS2C(towerDTO.RoleID);
                    break;
                case TowerOpCode.StartBattle:
                    StartBattle(towerDTO.RoleID, towerDTO.ChooseCricketId);
                    break;
                case TowerOpCode.ChooseDifficulty:
                    ChooseDifficultyS2C(towerDTO.RoleID, towerDTO.NowChooseDifficulty);
                    break;
            }
        }

        //战斗结算
        Dictionary<int, BattleResult> BattleCombat(BattleCharacterEntity[] array)
        {
            Dictionary<int, BattleResult> battleResultDict = new Dictionary<int, BattleResult>();
            for (int i = 0; i < array.Length; i++)
            {
                BattleCharacterEntity battleCharacterEntity = array[i];
                if (battleCharacterEntity.IsRobot)
                    continue;
                BattleResult battleResult = new BattleResult() { IsWinner = battleCharacterEntity.IsWin };
                battleResult.GetMoney = GameManager.CustomeModule<MatchManager>().RandomAddMoney(battleCharacterEntity.RoleID, battleResult.IsWinner).Result;
                battleResult.GetExp = GameManager.CustomeModule<MatchManager>().RandomAddExp(battleCharacterEntity.CricketID, battleCharacterEntity.RoleID, battleResult.IsWinner);
                battleResult.RankLevel = 0;
                battleResult.BattleMode = BattleMode.Tower;
                battleResultDict[battleCharacterEntity.RoleID] = battleResult;
                if (battleResult.IsWinner)//增加爬塔层数
                {
                    NHCriteria nHCriteria = xRCommon.xRNHCriteria("RoleID", battleCharacterEntity.RoleID);
                    Tower tower = xRCommon.xRCriteria<Tower>(nHCriteria);
                    if (tower != null)
                    {
                        battleResult.GetProp = GetTowerAward(tower);
                        AddTowerLevelS2C(tower);
                    }
                }
            }
            return battleResultDict;
        }

        Dictionary<int,int> GetTowerAward(Tower tower)
        {
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, TowerDifficultyData>>(out var towerDifficultyDataDict);
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, TowerFloorData>>(out var towerFloorDataDict);
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, TowerAwardData>>(out var towerAwardDataDict);
            TowerDifficultyData towerDifficultyData = towerDifficultyDataDict[tower.NowChooseDifficulty];
            TowerFloorData towerFloorData = towerFloorDataDict[towerDifficultyData.FloorIdArray[tower.NowLevel]];
            Dictionary<int, int> awardItemDict = new Dictionary<int, int>();
            for (int i = 0; i < towerFloorData.RewardIdArray.Count; i++)
            {
                int count = towerFloorData.RewardCountArray[i];
                TowerAwardData towerAwardData = towerAwardDataDict[towerFloorData.RewardIdArray[i]];
                List<int> propList = new List<int>();
                int prop = 0;
                for (int j = 0; j < towerAwardData.Weight.Count; j++)
                {
                    prop += towerAwardData.Weight[j];
                    propList.Add(prop);
                }
                int randomValue = Utility.Algorithm.CreateRandomInt(1, 101);
                int itemId = 0;
                for (int j = 0; j < propList.Count; j++)
                {
                    if (randomValue <= propList[j])
                    {
                        itemId = towerAwardData.GoodsID[j];
                        Utility.Debug.LogError("randomValue=>" + randomValue + ",prop=>" + propList[j] + ",itemid=>" + itemId);
                        break;
                    }
                }
                if (awardItemDict.ContainsKey(itemId))
                    awardItemDict[itemId] += count;
                else
                    awardItemDict.Add(itemId, count);
                //将物品添加到背包
                GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, PropData>>(out var propDataDict);
                if(propDataDict.ContainsKey(itemId))
                    InventoryManager.xRAddInventory(tower.RoleID, new Dictionary<int, ItemDTO>() { { itemId, new ItemDTO() { ItemAmount = count } } });
                else
                    ExplorationManager.xRAddExploration(tower.RoleID, null, new Dictionary<int, int>() { {itemId, count } });
            }
            return awardItemDict;
        }

        void RandomEnemyInfo(Tower tower)
        {
            //随机敌人的名字头像模型
            GameManager.CustomeModule<DataManager>().TryGetValue<List<CricketNameData>>(out var cricketNameDataList);
            int randomValue = Utility.Algorithm.CreateRandomInt(0, cricketNameDataList.Count);
            tower.NowEnemyName = cricketNameDataList[randomValue].CricketName;
            GameManager.CustomeModule<DataManager>().TryGetValue<List<CricketHeadPortraitData>>(out var cricketHeadPortraitDataList);
            randomValue = Utility.Algorithm.CreateRandomInt(0, cricketHeadPortraitDataList.Count);
            tower.NowEnemyIconId = cricketHeadPortraitDataList[randomValue].CricketID;
        }
    }
}
