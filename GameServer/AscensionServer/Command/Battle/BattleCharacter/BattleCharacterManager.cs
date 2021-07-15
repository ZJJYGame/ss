using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using AscensionProtocol;

namespace AscensionServer
{
    [CustomeModule]
    public class BattleCharacterManager:Module<BattleCharacterManager>
    {
        public Dictionary<int, BattleCharacterEntity> BattleCharacterEntityDict { get; protected set; } = new Dictionary<int, BattleCharacterEntity>();


        public BattleCharacterEntity CreateCharacter(RoleDTO roleDTO,CricketDTO cricketDTO)
        {
            BattleCharacterEntity battleCharacterEntity = GameManager.ReferencePoolManager.Spawn<BattleCharacterEntity>();
            battleCharacterEntity.Init(roleDTO, cricketDTO);
            if (BattleCharacterEntityDict.ContainsKey(battleCharacterEntity.RoleID))
            {
                GameManager.ReferencePoolManager.Despawn(BattleCharacterEntityDict[battleCharacterEntity.RoleID]);
                BattleCharacterEntityDict.Remove(battleCharacterEntity.RoleID);
            }
            BattleCharacterEntityDict.Add(battleCharacterEntity.RoleID, battleCharacterEntity);
            return battleCharacterEntity;
        }
        public BattleCharacterEntity CreateCharacter(RoleDTO roleDTO, CricketDTO cricketDTO,MachineData machineData)
        {
            BattleCharacterEntity battleCharacterEntity = GameManager.ReferencePoolManager.Spawn<BattleCharacterEntity>();
            battleCharacterEntity.Init(roleDTO, cricketDTO,machineData);
            if (BattleCharacterEntityDict.ContainsKey(battleCharacterEntity.RoleID))
            {
                GameManager.ReferencePoolManager.Despawn(BattleCharacterEntityDict[battleCharacterEntity.RoleID]);
                BattleCharacterEntityDict.Remove(battleCharacterEntity.RoleID);
            }
            BattleCharacterEntityDict.Add(battleCharacterEntity.RoleID, battleCharacterEntity);
            return battleCharacterEntity;
        }
        public BattleCharacterEntity CreateCharacter(Tower tower)
        {
            BattleCharacterEntity battleCharacterEntity = GameManager.ReferencePoolManager.Spawn<BattleCharacterEntity>();
            battleCharacterEntity.Init(tower);
            if (BattleCharacterEntityDict.ContainsKey(battleCharacterEntity.RoleID))
            {
                GameManager.ReferencePoolManager.Despawn(BattleCharacterEntityDict[battleCharacterEntity.RoleID]);
                BattleCharacterEntityDict.Remove(battleCharacterEntity.RoleID);
            }
            BattleCharacterEntityDict.Add(battleCharacterEntity.RoleID, battleCharacterEntity);
            return battleCharacterEntity;
        }

        public void RemoveCharacter(int roleId)
        {
            if (BattleCharacterEntityDict.ContainsKey(roleId))
            {
                GameManager.ReferencePoolManager.Despawn(BattleCharacterEntityDict[roleId]);
                BattleCharacterEntityDict.Remove(roleId);
            }
        }
    }
}
