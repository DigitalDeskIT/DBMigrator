using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbMigrator.Core.MigrationFilter
{
    public class FirstNodeMigrationFilter : IMigrationFilter
    {
        private string nodeName;

        public FirstNodeMigrationFilter(string nodeName)
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
