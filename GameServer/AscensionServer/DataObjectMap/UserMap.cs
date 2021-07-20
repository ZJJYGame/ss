using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace AscensionServer
{
    public class UserMap : ClassMap<User>
    {
        public UserMap()
        {
            Id(x => x.UUID).GeneratedBy.Assigned().Column("uuid");
            Map(x => x.Account).Unique().Column("account");
            Map(x => x.Password).Column("password");
            Map(x => x.RoleID).Column("roleid");
            Table("user");
        }
    }
}
