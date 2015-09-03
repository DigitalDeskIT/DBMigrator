using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbMigrator.Core.Migration;

namespace DbMigrator.Core
{
    /// <summary>
    /// A migration with it current state.
    /// </summary>
    public class MigrationStateInfo
    {
        public IMigration Migration { get; set; }
        public MigrationState CurrentState { get; set; }
    }
}
