using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbMigrator.Core.Migration;
using DbMigrator.Core.Util;

namespace DbMigrator.Core.MigrationFilter
{
    public class FlagMigrationFilter : IMigrationFilter
    {
        private FlagFilter filter;

        public FlagMigrationFilter(FlagFilter filter)
        {
            this.filter = filter;
        }

        public void Filter(MigrationFilterContext migrationFilterContext)
        {
            foreach (var node in migrationFilterContext.MigrationNodes)
            {
                List<IMigration> exceptMigrations = new List<IMigration>();
                foreach (var migration in node.MigrationsInfo.Select(x => x.Migration))
                {
                    if (migration is IFlaggedMigration)
                    {
                        var flags = ((IFlaggedMigration)migration).GetFlags();
                        if (!filter.Test(flags.ToArray()))
                        {
                            exceptMigrations.Add(migration);
                        }
                    }
                }
                node.MigrationsInfo = node.MigrationsInfo.Where(x => !exceptMigrations.Contains(x.Migration)).ToList();
            }
        }
    }
}
