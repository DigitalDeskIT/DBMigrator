using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbMigrator.Core.DataAccess;

namespace DbMigrator.Core
{
    public class SimpleMigrationMapProvider : IMigrationMapProvider
    {
        private MigrationMap migrationMap;

        public SimpleMigrationMapProvider(MigrationMap migrationMap)
        {
            this.migrationMap = migrationMap;
        }

        public MigrationMap GetMigrationMap()
        {
            return this.migrationMap;
        }
    }
}
