using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbMigrator.Core.MigrationFilter
{
    public class LastNodeMigrationFilter : IMigrationFilter
    {
        private string nodeName;

        public LastNodeMigrationFilter(string nodeName)
        {
            this.nodeName = nodeName;
        }

        public void Filter(MigrationFilterContext migrationFilterContext)
        {
            
        }

        public void AfterTransaction(bool commited)
        {
        }
    }
}
