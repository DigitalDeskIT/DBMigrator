using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbMigrator.Core.Migration;

namespace DbMigrator.Core
{


    /// <summary>
    /// A list of MigrationStateInfo.
    /// </summary>
    public class MigrationNodeStateInfo
    {
        public IMigrationNode MigrationNode { get; set; }
        public List<MigrationStateInfo> MigrationsInfo {get;set;}
    }
}
