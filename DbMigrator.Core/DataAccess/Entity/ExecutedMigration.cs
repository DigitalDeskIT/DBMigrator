using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbMigrator.Core.DataAccess.Entities
{
    public class ExecutedMigration
    {
        public int Id { get; set; }
        public DateTime LastRunDate { get; set; }
        public string LastRunScript { get; set; }
        public string MigrationNodeId { get; set; }
        public string MigrationId { get; set; }
        public string MigrationRunnerId { get; set; }
    }
}
