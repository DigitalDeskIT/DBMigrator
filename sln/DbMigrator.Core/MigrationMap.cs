using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbMigrator.Core
{
    public class MigrationMap
    {
        public string Identifier { get; set; }
        public List<IMigrationNode> MigrationNodes { get; set; }
    }
}
