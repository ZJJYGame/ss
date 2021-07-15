using System.Collections;
using System.Collections.Generic;
using Cosmos;
using AscensionProtocol;
using System;
using System.Threading.Tasks;
using Protocol;
using System.Collections.Concurrent;

namespace AscensionServer
{
    /// <summary>
    /// 角色管理器；
    /// key为RoleId或者PlayerId
    /// </summary>
    [CustomeModule]
    public class RoleManager : Module<RoleManager>
    {
        public int RoleCount { get { return roleDict.Count; } }
        ConcurrentDictionary<int, RoleEntity> roleDict = new ConcurrentDictionary<int, RoleEntity>();
        ConcurrentQueue<RoleEntity> playerPoolQueue = new ConcurrentQueue<RoleEntity>();
        /// <summary>
        /// 广播事件消息 ;
        /// </summary>
        event Action<byte, Dictionary<byte, object>> BroadcastEvent
        {
            add { broadcastEvent += value; }
            remove{broadcastEvent -= value;}
        }
        /// <summary>
        /// 广播普通消息;
        /// </summary>
        event Action<OperationData> BroadcastMessage
        {
            add { broadcastMessage += value; }
            remove{broadcastMessage -= value;}
        }
        Action<byte, Dictionary<byte, object>> broadcastEvent;
        Action<OperationData> broadcastMessage;
        public override void OnPreparatory()
        {
            CommandEventCore.Instance.AddEventListener(ProtocolDefine.PORT_CHAT, OnChatMessage);
            CommandEventCore.Instance.AddEventListener(ProtocolDefine.OPR_PLYAER_LOGOFF, OnPlayerLogoff);
        }
        public bool ContainsKey(int roleId)
        {
            return roleDict.ContainsKey(roleId);
        }
        public bool TryAdd(int roleId, RoleEntity role)
        {
            var result = roleDict.TryAdd(roleId, role);
            if (result)
            {
                BroadcastEvent += role.SendEvent;
                BroadcastMessage += role.SendMessage;
            }
            return result;
        }
        public bool TryGetValue(int roleId, out RoleEntity role)
        {
            return roleDict.TryGetValue(roleId, out role);
        }
        public bool TryRemove(int roleId)
        {
            var result = roleDict.TryRemove(roleId, out var role);
            if (result)
            {
                BroadcastEvent -= role.SendEvent;
                BroadcastMessage -= role.SendMessage;
            }
            return result;
        }
        public bool TryRemove(int roleId, out RoleEntity role)
        {
            var result = roleDict.TryRemove(roleId, out role);
            if (result)
            {
                BroadcastEvent -= role.SendEvent;
                BroadcastMessage -= role.SendMessage;
            }
            return result;
        }
        public bool TryUpdate(int roleId, RoleEntity newRole, RoleEntity comparsionRole)
        {
            if (!newRole.Equals(comparsionRole))
                return false;
            if (roleDict.ContainsKey(roleId))
            {

                var oldRole = roleDict[roleId];
                {
                    BroadcastEvent += newRole.SendEvent;
                    BroadcastMessage += newRole.SendMessage;
                    BroadcastEvent -= oldRole.SendEvent;
                    BroadcastMessage -= oldRole.SendMessage;
                }
                roleDict[roleId] = newRole;
                return true;
            }
            return false;
        }
        public bool SendEvent(int roleId, byte opCode, Dictionary<byte, object> userData)
        {
            var result = TryGetValue(roleId, out var role);
            if (result)
                role.SendEvent(opCode, userData);
            return result;
        }
        /// <summary>
        /// 同步广播事件(EVENT)；
        /// 此方法会对所有在线且Available的peer对象进行消息广播；
        /// </summary>
        /// <param name="userData">用户自定义数据</param>
        public void BroadcastEventToAll(byte opCode, Dictionary<byte, object> userData)
        {
            broadcastEvent?.Invoke(opCode, userData);
        }
        /// <summary>
        ///发送消息到具体的SessionId 
        /// </summary>
        public bool SendMessage(int roleId, OperationData message)
        {
            var result = TryGetValue(roleId, out var role);
            if (result)
                role.SendMessage(message);
            else
                Utility.Debug.LogError("roleEntity不存在，无法发送");
            return result;
        }
        /// <summary>
        /// 通过广播普通消息(MSG)；
        /// </summary>
        /// <param name="message">普通消息</param>
        public void BroadcastMessageToAll(OperationData message)
        {
            broadcastMessage?.Invoke(message);
        }
        /// <summary>
        ///异步发送消息到具体的SessionId 
        /// </summary>
        public async Task<bool> SendMessageAsync(int roleId, OperationData message)
        {
            return await Task.Run(() => { return SendMessage(roleId, message); });
        }
        /// <summary>
        ///异步广播消息到具体的sessionId 
        /// </summary>
        public async Task<bool> SendEventAsync(int roleId, byte opCode, Dictionary<byte, object> userData)
        {
            return await Task.Run(() => { return SendEvent(roleId, opCode, userData); });
        }
        /// <summary>
        /// 异步广播事件消息(EVENT)；
        /// </summary>
        /// <param name="userData">用户自定义数据</param>
        /// <param name="callback">广播结束后的回调</param>
        /// <returns>线程Task</returns>
        public async Task BroadcastEventToAllAsync(byte opCode, Dictionary<byte, object> userData, Action callback = null)
        {
            await Task.Run(() => { broadcastEvent?.Invoke(opCode, userData); });
            callback?.Invoke();
        }
        /// <summary>
        /// 异步广播普通消息(MSG)；
        /// </summary>
        /// <param name="message">普通消息</param>
        /// <param name="callback">消息广播完成后的回调</param>
        /// <returns></returns>
        public async Task BroadcastMessageToAllAsync(OperationData message, Action callback = null)
        {
            await Task.Run(() => { broadcastMessage?.Invoke(message); });
            callback?.Invoke();
        }
        void OnChatMessage(OperationData opData)
        {

        }
        void OnPlayerLogoff(OperationData opData)
        {
            #region
            var roleEntity = opData.DataMessage as RoleEntity;
            if (roleEntity != null)
            {
                TryRemove(roleEntity.RoleId);
                Utility.Debug.LogInfo("1yzqData移除成功RoleID:" + roleEntity.RoleId);
            }
            #endregion

        }
    }
}