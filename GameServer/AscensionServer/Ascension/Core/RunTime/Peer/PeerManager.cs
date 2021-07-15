using Cosmos;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using AscensionProtocol;
using Protocol;

namespace AscensionServer
{
    /// <summary>
    /// <see cref="AscensionPeer"/>
    /// photon所有登录的peer都存储在此；
    /// SessionId在具体的Peer中，由服务器分配；
    /// 其余各个模块都是从此通过SessionID取得Peer对象；
    /// </summary>
    [CustomeModule]
    public class PeerManager : Module<PeerManager>, IKeyValue<int, IPeerEntity>
    {
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
        ConcurrentDictionary<int, IPeerEntity> peerDict;
        public override void OnInitialization()
        {
            peerDict = new ConcurrentDictionary<int, IPeerEntity>();
        }
        public bool TryAdd(IPeerEntity peer)
        {
            var result = peerDict.TryAdd(peer.SessionId, peer);
            if (result)
            {
                BroadcastEvent += peer.SendEventMsg;
                BroadcastMessage += peer.SendMessage;
            }
            return result;
        }
        public bool TryAdd(int sessionId, IPeerEntity peer)
        {
            var result = peerDict.TryAdd(sessionId, peer);
            if (result)
            {
                BroadcastEvent += peer.SendEventMsg;
                BroadcastMessage += peer.SendMessage;
            }
            return result;
        }
        public bool TryRemove(int sessionId)
        {
            var result = peerDict.TryRemove(sessionId, out var peer);
            if (result)
            {
                BroadcastEvent -= peer.SendEventMsg;
                BroadcastMessage -= peer.SendMessage;
            }
            return result;
        }
        public bool TryRemove(int sessionId, out IPeerEntity peer)
        {
            var result = peerDict.TryRemove(sessionId, out peer);
            if (result)
            {
                BroadcastEvent -= peer.SendEventMsg;
                BroadcastMessage -= peer.SendMessage;
            }
            return result;
        }
        public bool ContainsKey(int sessionId)
        {
            return peerDict.ContainsKey(sessionId);
        }
        /// <summary>
        /// 将指定键的现有值与指定值进行比较，如果相等，则用第三个值更新该键。
        /// </summary>
        public bool TryUpdate(int sessionId, IPeerEntity newPeer, IPeerEntity comparisonPeer)
        {
            var result = peerDict.TryUpdate(sessionId, newPeer, comparisonPeer);
            if (result)
            {
                BroadcastEvent -= comparisonPeer.SendEventMsg;
                BroadcastEvent += newPeer.SendEventMsg;
                BroadcastMessage -= comparisonPeer.SendMessage;
                BroadcastMessage += newPeer.SendMessage;
            }
            return result;
        }
        public bool TryGetValue(int sessionId, out IPeerEntity peer)
        {
            return peerDict.TryGetValue(sessionId, out peer);
        }
        /// <summary>
        ///发送事件(EVENT)到具体的sessionId； 
        ///若不存在session对象，则不发送，并返回false；
        /// </summary>
        public bool SendEvent(int sessionId,byte opCode, Dictionary<byte,object> userData)
        {
            var result = TryGetValue(sessionId, out var peer);
            if(result)
                peer.SendEventMsg(opCode, userData);
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
        public bool SendMessage(int sessionId,OperationData message)
        {
            var result = TryGetValue(sessionId, out var peer);
            if (result)
                peer.SendMessage(message);
            else
                Utility.Debug.LogError("peer不存在，无法发送");
            return result;
        }
        /// <summary>
        /// 通过广播消息(MSG)；
        /// </summary>
        /// <param name="message">普通消息</param>
        public void BroadcastMessageToAll(OperationData message)
        {
            broadcastMessage?.Invoke(message);
        }
        /// <summary>
        ///异步发送消息到具体的SessionId 
        /// </summary>
        public async Task<bool> SendMessageAsync(int sessionId, OperationData message)
        {
            return await Task.Run(() => { return SendMessage(sessionId, message); });
        }
        /// <summary>
        ///异步广播消息到具体的sessionId 
        /// </summary>
        public async Task<bool> SendEventAsync(int sessionId,byte opCode, Dictionary<byte, object> userData)
        {
            return await Task.Run(() => { return SendEvent(sessionId, opCode,userData); });
        }
        /// <summary>
        /// 异步广播事件消息(EVENT)；
        /// </summary>
        /// <param name="userData">用户自定义数据</param>
        /// <param name="callback">广播结束后的回调</param>
        /// <returns>线程Task</returns>
        public async Task BroadcastEventToAllAsync(byte opCode, Dictionary<byte,object> userData, Action callback = null)
        {
            await Task.Run(() => { broadcastEvent?.Invoke(opCode, userData); });
            callback?.Invoke();
        }
        /// <summary>
        /// 异步广播消息(MSG)；
        /// </summary>
        /// <param name="message">普通消息</param>
        /// <param name="callback">消息广播完成后的回调</param>
        /// <returns></returns>
        public async Task BroadcastMessageToAllAsync(OperationData message, Action callback = null)
        {
            await Task.Run(() => { broadcastMessage?.Invoke(message); });
            callback?.Invoke();
        }
    }
}
