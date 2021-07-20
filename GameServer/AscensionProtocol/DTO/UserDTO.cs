using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscensionProtocol
{
    [Serializable]
    public class UserDTO
    {
        public virtual int Account { get; set; }
        public virtual string Name { get; set; }
        public virtual string UUID { get; set; }
        public virtual int RoleID { get; set; }
    }

}