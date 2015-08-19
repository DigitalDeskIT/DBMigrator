using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbMigrator.Core
{
    public class MigrationMapFilter : DbMigrator.Core.IMigrationMapFilter
    {
        public MigrationMapFilter()
        {
            
        }

        public List<IMigrationFilter> Filters = new List<IMigrationFilter>();           

        public void Filter(MigrationMapStateInfo databaseMigrationState)
        {
            MigrationFilterContext context = new MigrationFilterContext()
            {
                MigrationNodes = databaseMigrationState.MigrationNodesInfo
            };

            foreach (var filter in this.Filters)
	        {
                filter.Filter(context);
	        }
        }
    }
}
