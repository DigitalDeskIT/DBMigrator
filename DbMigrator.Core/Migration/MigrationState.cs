using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbMigrator.Core.Migration
{
    public enum MigrationState
    {
        Undefined = 0,
        ToUpgrade = 1,
        Executed = 2,
        Filtered = 3
    }
}
