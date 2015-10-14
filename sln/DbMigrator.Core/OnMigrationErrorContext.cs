using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbMigrator.Core
{
    public class OnMigrationErrorContext
    {
        public Exception Exception { get; set; }
        public Migration.IMigration Migration { get; set; }
        public OnMigrationErrorDecision Decision { get; set; }
    }

    public enum OnMigrationErrorDecision
    {
        Stop, //default
        MarkAnywayAndStop,
        MarkAnywayAndContinue,
        Continue
    }
}
