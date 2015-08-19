using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbMigrator.Core
{
    public class MigrationFilterContext
    {
        public List<MigrationNodeStateInfo> MigrationNodes { get; set; }
    }
}
