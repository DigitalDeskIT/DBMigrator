using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbMigrator.Core.Migration;

namespace DbMigrator.Core
{
    public class MigrationNode : IMigrationNode
    {
        public string Identifier { get; set; }


        public List<IMigration> Migrations
        {
            get;
            set;
        }


        public bool Root
        {
            get;
            set;
        }
    }
}
