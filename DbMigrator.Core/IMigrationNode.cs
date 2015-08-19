using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbMigrator.Core.Migration;

namespace DbMigrator.Core
{
    public interface IMigrationNode
    {
        string Identifier { get; }
        List<IMigration> Migrations { get; set; }
    }
}
