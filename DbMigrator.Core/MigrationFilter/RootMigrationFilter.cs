using DbMigrator.Core.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbMigrator.Core.MigrationFilter
{
    public class RootMigrationFilter : IMigrationFilter
    {
        private IDataProvider dataProvider;
        private string rootKey;
        private string runnedRootIdentifier;

        public RootMigrationFilter(IDataProvider dataProvider, string runnerIdentifier)
        {
            this.dataProvider = dataProvider;
            this.rootKey = "MigrationRunner."+ runnerIdentifier +".Root";
        }

        public void Filter(MigrationFilterContext migrationFilterContext)
        {
            runnedRootIdentifier = dataProvider.GetData(rootKey);
            MigrationNodeStateInfo root;
            if (runnedRootIdentifier != null) //já possui root
            {
                root = migrationFilterContext.MigrationNodes.Where(x => x.MigrationNode.Identifier == runnedRootIdentifier).SingleOrDefault();
                if (root == null)
                {
                    throw new InvalidOperationException("The runned root node could not be found.");
                }
            }
            else //não possui root
            {
                root = migrationFilterContext.MigrationNodes.Where(x => x.MigrationNode.Root).LastOrDefault();
                if (root == null)
                {
                    root = migrationFilterContext.MigrationNodes.FirstOrDefault();
                    if (root == null)
                    {
                        throw new InvalidOperationException("Could not set a root node.");
                    }
                    runnedRootIdentifier = root.MigrationNode.Identifier;
                }
            }

            bool rootTouched = false;
            foreach (var item in migrationFilterContext.MigrationNodes)
            {
                if (item == root)
                    rootTouched = true;

                if (!rootTouched)
                {
                    foreach (var migration in item.MigrationsInfo)
                    {
                        migration.CurrentState = Migration.MigrationState.Filtered;
                    }
                }
            }
        }


        public void AfterTransaction(bool commited)
        {
            if (commited)
                dataProvider.SetData(rootKey, runnedRootIdentifier);
        }
    }
}
