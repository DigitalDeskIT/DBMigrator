using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbMigrator.Core
{
    public class OnMigratingContext
    {
        public Migration.IMigration Migration { get; set; }
        public OnMigratingDecision Decision { get; set; }
    }

    public enum OnMigratingDecision
    {
        Run, //default
        Jump,
        JumpAndMark,
        Stop
    }
}
