using System;
using System.Collections.Generic;
using System.Text;
using Cosmos;
using System.Collections.Concurrent;
using StackExchange.Redis;

namespace RedisDotNet
{
    public class RedisManager : ConcurrentSingleton<RedisManager>
    {
        /// <summary>
        /// 连接配置
        ///// </summary>
        /// <summary>
        /// Redis保存数据时候key的前缀
        /// </summary>
        internal static readonly string RedisKeyPrefix = "JY";
        /// <summary>
        /// Redis对象
        /// </summary>
        public ConnectionMultiplexer Redis { get; private set; }
        public IServer[] RedisServers { get; private set; }
        /// <summary>
        /// Redis中的DB
        /// </summary>
        public IDatabase RedisDB { get; private set; }
        ConcurrentDictionary<string, Action<string>> eventDict 
            = new ConcurrentDictionary<string,  Action<string>>();

        public void ConnectRedis()
        {
            string connectStr = "192.168.0.117:6379,password=123456,DefaultDatabase=4";//内网
            //string connectStr = "39.174.37.118:6379,password=jygame_%Redis,DefaultDatabase=4"; //公网
            //string connectStr = "127.0.0.1:6379,password=123456,DefaultDatabase=3";//lu
            //string connectStr = "127.0.0.1:6379,password=123456,DefaultDatabase=3";//lu
            try
            {
                Redis = ConnectionMultiplexer.Connect(connectStr);
                if(Redis==null)
                    throw new ArgumentNullException("Redis Connect Fail");
                else
                {
                    List<IServer> servers = new List<IServer>();
                    foreach (var endPoint in Redis.GetEndPoints())
                    {

                        var server = Redis.GetServer(endPoint);
                        servers.Add(server);
                    }
                    RedisServers = servers.ToArray();
                }
                RedisDB = Redis.GetDatabase();
            }
            catch (Exception)
            {
                throw new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Redis Connect Fail");
            }

        }
        /// <summary>
        /// 订阅key过期事件；
        /// 添加回调，泛型参数为失效的KEY；
        /// 举例：
        /// 完整的key为： JY_VAREITY_SHOP1；
        /// 若监听：则key为JY_VAREITY即可；
        /// 若触发，则回调中传入的参数为：JY_VAREITY_SHOP1；
        /// </summary>
        public void AddKeyExpireListener(string key,Action<string> callback)
        {
            key = $"{RedisKeyPrefix}_{key}";
            if( eventDict.TryGetValue(key, out var handle))
            {
                handle += callback;
            }
            else
            {
                eventDict.TryAdd(key,callback);
            }
            var mutlti =this. Redis;
            var subscriber = mutlti.GetSubscriber();
            subscriber.Subscribe("__keyevent@4__:expired", (channel, keyVar) =>
            {
                if (keyVar.StartsWith(key))
                {
                    eventDict.TryGetValue(key, out var hdl);
                    hdl?.Invoke(keyVar);
                }
            });
        }
        /// <summary>
        ///移除key过期事件；
        ///callback中的参数为完整的key参数；
        /// </summary>
        public void RemoveKeyExpireListener(string key,Action<string>callback)
        {
            key = $"{RedisKeyPrefix}_{key}";
            if (eventDict.TryGetValue(key, out var handle))
            {
                try{handle -= callback;}
                catch (Exception e)
                {
                    Utility.Debug.LogError(e);
                }
            }
        }
        public void OnTermination()
        {
        }
    }
}
