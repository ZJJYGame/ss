using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using Cosmos.Reference;
using AscensionProtocol;
using System.Threading;

namespace AscensionServer
{
    public class BattleRoomEntity : IReference
    {
        int roomId;
        BattleCharacterEntity battleCharacterEntity_one;
        BattleCharacterEntity battleCharacterEntity_Two;

        BattleController BattleController { get;  set; }

        public Func<BattleCharacterEntity[], Dictionary<int, BattleResult>> battleResultEvent;

        public void Init(int roomId,MatchDTO matchDTO)
        {
            this.roomId = roomId;
            battleCharacterEntity_one = GameManager.CustomeModule<BattleCharacterManager>().CreateCharacter(matchDTO.selfData,matchDTO.selfCricketData);
            battleCharacterEntity_Two = GameManager.CustomeModule<BattleCharacterManager>().CreateCharacter(matchDTO.otherData,matchDTO.otherCricketData);
            BattleController = new BattleController();
            BattleController.InitController(this,battleCharacterEntity_one, battleCharacterEntity_Two);
            ThreadPool.QueueUserWorkItem(new WaitCallback((obj)=> {
                BattleController.StartBattle();
            }));

           
        }
        //一个是匹配机器人
        public void Init(int roomId, MatchDTO matchDTO,MachineData machineData)
        {
            this.roomId = roomId;
            battleCharacterEntity_one = GameManager.CustomeModule<BattleCharacterManager>().CreateCharacter(matchDTO.selfData, matchDTO.selfCricketData);
            battleCharacterEntity_Two = GameManager.CustomeModule<BattleCharacterManager>().CreateCharacter(matchDTO.otherData, matchDTO.otherCricketData,machineData);
            BattleController = new BattleController();
            BattleController.InitController(this,battleCharacterEntity_one, battleCharacterEntity_Two);
            ThreadPool.QueueUserWorkItem(new WaitCallback((obj) => {
                BattleController.StartBattle();
            }));
        }
        public void Init(int roomId,RoleDTO roleDTO,CricketDTO cricketDTO,Tower tower)
        {
            this.roomId = roomId;
            battleCharacterEntity_one = GameManager.CustomeModule<BattleCharacterManager>().CreateCharacter(roleDTO, cricketDTO);
            battleCharacterEntity_Two = GameManager.CustomeModule<BattleCharacterManager>().CreateCharacter(tower);
            BattleController = new BattleController();
            BattleController.InitController(this,battleCharacterEntity_one, battleCharacterEntity_Two);


            ThreadPool.QueueUserWorkItem(new WaitCallback((obj) => {
                BattleController.StartBattle();
            }));
        }

        public void Clear()
        {
            roomId = 0;
            GameManager.CustomeModule<BattleCharacterManager>().RemoveCharacter(battleCharacterEntity_one.CricketID);
            GameManager.CustomeModule<BattleCharacterManager>().RemoveCharacter(battleCharacterEntity_Two.CricketID);
            battleCharacterEntity_one = null;
            battleCharacterEntity_Two = null;
        }

        public void OnRefresh()
        {
        }
    }
}
