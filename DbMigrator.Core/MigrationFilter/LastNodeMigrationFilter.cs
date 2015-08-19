using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbMigrator.Core.MigrationFilter
{
    public class LastNodeMigrationFilter : IMigrationFilter
    {
        private string p;

        public LastNodeMigrationFilter(string nodeName)
        {
            this.p = nodeName;
        }
        public void Filter(MigrationFilterContext migrationFilterContext)
        {
            
        }
    }
}
