using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbMigrator.Core.Migration
{
    public interface IFlaggedMigration : IMigration
    {
        IEnumerable<string> GetFlags();
    }
}
