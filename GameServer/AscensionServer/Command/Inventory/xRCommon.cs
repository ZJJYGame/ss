using Cosmos;
using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AscensionProtocol;

namespace AscensionServer
{
    public partial class xRCommon
    {
        /// <summary>
        /// 映射T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nHCriteria"></param>
        /// <returns></returns>
        public static T xRCriteria<T>(NHCriteria nHCriteria)
        {
            return NHibernateQuerier.CriteriaSelect<T>(nHCriteria);
        }
        /// <summary>
        ///映射 NHCriteria
        /// </summary>
        /// <param name="customs"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static NHCriteria xRNHCriteria(string customs, object values)
        {
            return GameManager.ReferencePoolManager.Spawn<NHCriteria>().SetValue(customs, values);
        }
        /// <summary>
        /// 验证的映射
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nHCriteria"></param>
        /// <returns></returns>
        public static bool xRVerify<T>(NHCriteria nHCriteria)
        {
            return NHibernateQuerier.Verify<T>(nHCriteria);
        }
        /// <summary>
        /// 统一参数
        /// </summary>
        /// <returns></returns>
        public static Dictionary<byte, object> xRS2CParams()
        {
            Dictionary<byte, object> paramsS2CDict = new Dictionary<byte, object>();
            return paramsS2CDict;
        }
        /// <summary>
        /// 统一sub码
        /// </summary>
        /// <returns></returns>
        public static Dictionary<byte, object> xRS2CSub()
        {
            Dictionary<byte, object> subS2CDict = new Dictionary<byte, object>();
            return subS2CDict;
        }

        /// <summary>
        /// 统一发送成功 
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="op"></param>
        public static void xRS2CSend(int roleId, ushort op, short rc,object tip =null)
        {
            OperationData opData = new OperationData();
            opData.OperationCode = op;
            opData.ReturnCode = rc;
            opData.DataMessage = rc !=(short)ReturnCode.Success? tip : tip;
            GameManager.CustomeModule<RoleManager>().SendMessage(roleId, opData);
        }

        public static void xRS2CRegisterSend(int SessionId, ushort op, short rc, object tip = null)
        {
            OperationData opData = new OperationData();
            opData.OperationCode = op;
            opData.ReturnCode = rc;
            opData.DataMessage = rc != (short)ReturnCode.Success ? tip : tip;
            GameManager.CustomeModule<PeerManager>().SendMessage(SessionId, opData);
        }

    }
}
