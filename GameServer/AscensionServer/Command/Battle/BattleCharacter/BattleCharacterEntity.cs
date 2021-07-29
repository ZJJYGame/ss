using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using AscensionProtocol;

namespace AscensionServer
{
    public class BattleCharacterEntity : IReference
    {
        public int RoleID { get; private set; }
        public string RoleName { get; private set; }
        public int CricketID { get; private set; }
        public int RemainActionBar { get; private set; }
        public bool IsRobot { get; private set; }
        public bool IsWin { get;set; }
        //行动次数记录
        public int ActionCount { get; set; }
        public int RoleHeadId { get; private set; }

        public RoleBattleData roleBattleData;

        public BattleBuffController battleBuffController;

        public void Init(RoleDTO roleDTO,CricketDTO cricketDTO)
        {
            RoleID = roleDTO.RoleID;
            RoleName = roleDTO.RoleName;
            CricketID = cricketDTO.ID;
            battleBuffController = new BattleBuffController(roleBattleData);
            roleBattleData = new RoleBattleData(battleBuffController, roleDTO, cricketDTO, this) { };
            RemainActionBar = roleBattleData.ActionBar;
            battleBuffController.roleBattleData = roleBattleData;
            RoleHeadId = roleDTO.HeadPortrait;
            IsRobot = false;
            IsWin = true;
            ActionCount = 1;
        }
        //机器人
        public void Init(RoleDTO roleDTO, CricketDTO cricketDTO,MachineData machineData)
        {
            //todo机器人RoleID
            RoleName = machineData.CricketName;
            //todo蛐蛐唯一ID
            battleBuffController = new BattleBuffController(roleBattleData);
            roleBattleData = new RoleBattleData(battleBuffController, machineData, this) { };
            RemainActionBar = roleBattleData.ActionBar;
            battleBuffController.roleBattleData = roleBattleData;
            RoleHeadId = roleDTO.HeadPortrait;
            IsRobot = true;
            IsWin = true;
            ActionCount = 1;
        }
        public void Init(Tower tower)
        {
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, TowerDifficultyData>>(out var towerDifficultyDataDict);
            int levelId = towerDifficultyDataDict[tower.NowChooseDifficulty].FloorIdArray[tower.NowLevel];
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, TowerFloorData>>(out var towerFloorDataDict);
            TowerFloorData towerFloorData = towerFloorDataDict[levelId];
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, TowerRobotData>>(out var towerRobotDataDict);
            TowerRobotData towerRobotData = towerRobotDataDict[towerFloorData.CricketId];
            //todo机器人RoleID
            RoleName = "第" + (tower.NowLevel + 1) + "层";
            //todo蛐蛐唯一ID
            battleBuffController = new BattleBuffController(roleBattleData);
            roleBattleData = new RoleBattleData(battleBuffController, towerRobotData, this) { };
            RemainActionBar = roleBattleData.ActionBar;
            battleBuffController.roleBattleData = roleBattleData;
            //随机头像
            GameManager.CustomeModule<DataManager>().TryGetValue<Dictionary<int, HeadPortraitData>>(out var headPortraitDataDict);
            List<HeadPortraitData> headPortraitDatas = headPortraitDataDict.Values.ToList();
            RoleHeadId = headPortraitDatas[Utility.Algorithm.CreateRandomInt(0, headPortraitDatas.Count)].PlayerHeadID;
            IsRobot = true;
            IsWin = true;
            ActionCount = 1;
        }
        /// <summary>
        /// 随机使用一个技能
        /// </summary>
        public BattleSkill RandomSkill(bool isAttackSkill)
        {
            if (roleBattleData.Endurance <= 0)
                return null;
            List<BattleSkill> randomSkillList;
            int randomNum;
            if (isAttackSkill)
            {
                randomSkillList = roleBattleData.BattleAttackSkillList;
                Utility.Debug.LogError("随机技能数量" + randomSkillList.Count);
                randomNum = Utility.Algorithm.CreateRandomInt(0, roleBattleData.AllAttackSkillProp);
            }
            else
            {
                randomSkillList = roleBattleData.BattleDefendSkillList;
                randomNum = Utility.Algorithm.CreateRandomInt(0, roleBattleData.AllDefendSkillProp);
            }
            
            BattleSkill resultSkill;
            int propNum=0;
            for (int i = 0; i < randomSkillList.Count; i++)
            {
                propNum += randomSkillList[i].TriggerProb;
                if (randomNum <= propNum)
                {
                    resultSkill = randomSkillList[i];
                    return resultSkill;
                }
            }
            Utility.Debug.LogInfo(CricketID+"没有符合的技能");
            return null;
        }

        public void ChangeActionBar(int num)
        {
            RemainActionBar -= num;

        }
        public void TryRestartActionBar()
        {
                RemainActionBar = roleBattleData.ActionBar;

        }
        public void AddActionCount()
        {
            ActionCount++;
        }




        RoleBattleData GetRoleBattleData(TowerRobotData towerRobotData)
        {
            RoleBattleData roleBattleData = new RoleBattleData(battleBuffController, towerRobotData, this) { };
            return roleBattleData;
        }

        /// <summary>
        /// 使用被动技能
        /// </summary>
        public void UsePassiveSkill()
        {
            List<BattleSkill> battlePassiveSkillList = roleBattleData.BattlePassiveSkillList;
            BattleSkill battlePassiveSkill;
            for (int i = 0; i < battlePassiveSkillList.Count; i++)
            {
                battlePassiveSkill = battlePassiveSkillList[i];
                for (int j = 0; j < battlePassiveSkill.BattleSkillAddBuffList.Count; j++)
                {
                    Utility.Debug.LogError(CricketID+"添加被动技能" + battlePassiveSkill.SkillId);
                    battleBuffController.AddBuff(battlePassiveSkill.BattleSkillAddBuffList[j], battlePassiveSkill.SkillId);
                }
            }
            battleBuffController.TriggerBuff();
        }

        public void S2CSendBattleData(BattleTransferDTO battleTransferDTO)
        {
            if (IsRobot)
                return;
            GameManager.CustomeModule<BattleRoomManager>().S2CEnterBattle(RoleID, battleTransferDTO);
        }

        public void Clear()
        {
            RoleID = 0;
            RoleName = null;
            CricketID = 0;
            RemainActionBar = 0;
            IsRobot = false;
            roleBattleData = null;
            battleBuffController = null;
            ActionCount = 0;
        }

        public void OnRefresh()
        {
        }
    }
}
