using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbMigrator.Core.Migration
{
    /// <summary>
    /// Interface de uma migração. Uma migração representa um ou mais comandos de SQL que atendem a uma necessidade específica e bem definida.
    /// </summary>
    public interface IMigration
    {
        string Identifier { get; }

        string GetDescription();

        string GetUpgradeSql();
    }
}
