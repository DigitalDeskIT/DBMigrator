using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbMigrator.Core.DataAccess.Entities;
using DbMigrator.Core.Migration;

namespace DbMigrator.Core.DataAccess
{
    public interface IDataProvider
    {
        void UpgradeSchema();

        /// <summary>
        /// Returns true if the table DBM_Data exist in the database.
        /// </summary>
        bool DataTableExist();
        
        /// <summary>
        /// Get a string value.
        /// </summary>
        string GetData(string key);

        /// <summary>
        /// Set a string value.
        /// </summary>
        void SetData(string key, string value);

        /// <summary>
        /// Register a executed migration.
        /// </summary>
        /// <param name="executedMigration"></param>
        void InsertExecutedMigration(ExecutedMigration executedMigration);

        /// <summary>
        /// Test if a runner already has runned.
        /// </summary>
        bool MigrationRunnerAlreadyRunned(string migrationRunnerIdentifier);

        /// <summary>
        /// List all executed migrations.
        /// </summary>
        List<ExecutedMigration> ListExecutedMigrations(string migrationRunnerId);

        /// <summary>
        /// Execute a block of SQL to be wrapped in a TransactionScope.
        /// </summary>
        /// <param name="sql"></param>
        void ExecuteSql(string sql);
    }
}
