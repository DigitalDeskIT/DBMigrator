using System;
using System.Collections.Generic;
namespace DbMigrator.Core
{
    public interface IMigrationMapFilter
    {
        void Filter(MigrationMapStateInfo databaseMigrationState);
    }
}
