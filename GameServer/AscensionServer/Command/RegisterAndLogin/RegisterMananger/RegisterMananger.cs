using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol;
using AscensionProtocol;
using Cosmos;
using Protocol;

namespace AscensionServer
{
    [CustomeModule]
    public class RegisterMananger : Module<RegisterMananger>
    {
        public override void OnPreparatory()
        {
            //CommandEventCore.Instance.AddEventListener((ushort)ATCmd.Register, C2SRegister);
        }

        public void C2SRegister(OperationData opData)
        {
            var message = Utility.Json.ToObject<UserDTO>(opData.DataMessage.ToString());
            var dp = opData.DataContract;
            dp.Messages.TryGetValue((byte)ParameterCode.ClientPeer, out var peer);
            Utility.Debug.LogInfo("yzqData/////"+ message.Account);
            //RegisterHandler.RegisterRole(message.Account, message.Password, peer);
        }

        public void S2CRegister(int roleid, string message,ReturnCode returnCode)
        {
            OperationData operationData = new OperationData();
            operationData.DataMessage = message;
            operationData.ReturnCode = (byte)returnCode;
            operationData.OperationCode = ProtocolDefine.OPR_TESTCAHNNEL;
            GameManager.CustomeModule<RoleManager>().SendMessage(roleid, operationData);
        }
    }
}
