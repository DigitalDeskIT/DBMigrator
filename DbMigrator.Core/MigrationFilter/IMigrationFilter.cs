using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbMigrator.Core
{
    public interface IMigrationFilter
    {
        void Filter(MigrationFilterContext migrationFilterContext);
    }
}
