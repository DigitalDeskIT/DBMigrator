﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using DbMigrator.Core.DataAccess;

namespace DbMigrator.Core
{
    public class MigrationRunner
    {
        private readonly string identifier;
        private readonly IDataProvider dataProvider;
        private readonly IMigrationMapProvider mapProvider;
        private IMigrationRunnerOutputHandler output = new SilentMigratorRunnerOutputHandler();

        public MigrationRunner(
            string identifier,
            IDataProvider dataProvider,
            IMigrationMapProvider mapProvider
        ){
            this.identifier = identifier;
            this.dataProvider = dataProvider;
            this.mapProvider = mapProvider;
        }

        public void SetOutputHandler(IMigrationRunnerOutputHandler outputHandler)
        {
            this.output = outputHandler;
        }

        private List<IMigrationFilter> migrationFilters = new List<IMigrationFilter>();
        public void AddFilter(IMigrationFilter filter)
        {
            this.migrationFilters.Add(filter);
        }

        private Action<OnMigratingContext> onMigrating = (x) => { };
        private Action<OnMigrationErrorContext> onMigrationError = (x) => { };
        private Action<OnCompletingTransactionContext> onCompletingTransaction = (x) => { };

        public void SetOnMigrating(Action<OnMigratingContext> action){ this.onMigrating = action; }
        public void SetOnMigrationError(Action<OnMigrationErrorContext> action) { this.onMigrationError = action; }
        public void SetOnCompletingTransaction(Action<OnCompletingTransactionContext> action) { this.onCompletingTransaction = action; }

        public void Migrate(bool completeTransaction)
        {
            output.EventBegin("migration", "Migration");

            //upgrade the schema to be updated
            dataProvider.UpgradeSchema();

            //get nodes
            var map = mapProvider.GetMigrationMap();

            //get nodes current state in the selected database
            var databaseMigrationState = GetState(map);

            //filter nodes
            MigrationFilterContext filterContext = new MigrationFilterContext() { MigrationNodes = databaseMigrationState.MigrationNodesInfo };
            foreach (var migrationFilter in migrationFilters)
            {
                migrationFilter.Filter(filterContext);
            }

            using (var transactionScope = new TransactionScope(TransactionScopeOption.Required, TimeSpan.MaxValue))
            {
                //migrate
                bool stopNodeProcessing = false;
                bool hasErrors = false;

                foreach (var nodeInfo in databaseMigrationState.MigrationNodesInfo)
                {
                    if (stopNodeProcessing)
                        break;

                    var node = nodeInfo.MigrationNode;
                    foreach (var migrationInfo in nodeInfo.MigrationsInfo)
                    {
                        if (migrationInfo.CurrentState == Migration.MigrationState.ToUpgrade)
                        {
                            bool jumpCurrentExecution = false;
                            bool markExecution = true;
    

                            var migration = migrationInfo.Migration;
                                
                            var migratingContext = new OnMigratingContext{ Migration = migration  };
                            onMigrating(migratingContext);
                            switch (migratingContext.Decision)
                            {
                                case OnMigratingDecision.Continue: { break; }
                                case OnMigratingDecision.Stop: { stopNodeProcessing = true; jumpCurrentExecution = true; markExecution = false; break; }
                                case OnMigratingDecision.Jump: { jumpCurrentExecution = true; markExecution = true; break; }
                                case OnMigratingDecision.JumpAndMark: { jumpCurrentExecution = true; markExecution = true; break; }
                                default: { throw new NotImplementedException(); }
                            }

                            string sql = string.Empty;
                            sql = migration.GetUpgradeSql();

                            if (!jumpCurrentExecution)
                            {
                                try
                                {
                                    dataProvider.ExecuteSql(sql);
                                }
                                catch (Exception ex)
                                {
                                    hasErrors = true;
                                    var migrationErrorContext = new OnMigrationErrorContext { Migration = migration, Exception = ex };
                                    onMigrationError(migrationErrorContext);
                                    switch (migrationErrorContext.Decision)
                                    {
                                        case OnMigrationErrorDecision.Stop: { stopNodeProcessing = true; break; }
                                        case OnMigrationErrorDecision.Continue: { markExecution = false; break; }
                                        case OnMigrationErrorDecision.MarkAnywayAndContinue: { markExecution = true; break; }
                                        default: { throw new NotImplementedException(); }
                                    }
                                }
                            }

                            if (markExecution)
                            {
                                dataProvider.InsertExecutedMigration(new DataAccess.Entities.ExecutedMigration()
                                {
                                    LastRunScript = sql,
                                    LastRunDate = DateTime.Now,
                                    MigrationId = migration.Identifier,
                                    MigrationNodeId = node.Identifier,
                                    MigrationRunnerId = this.identifier
                                });
                            }
                        }
                    }
                }
                if (completeTransaction)
                {
                    var onCompletingTransactionContext = new OnCompletingTransactionContext() { HasErrors = hasErrors };
                    onCompletingTransaction(onCompletingTransactionContext);
                    switch (onCompletingTransactionContext.Decision)
                    {
                        case OnCompletingTransactionDecision.Commit: { transactionScope.Complete(); break; }
                        case OnCompletingTransactionDecision.Rollback: { transactionScope.Dispose(); break; }
                        default: { throw new NotImplementedException(); }
                    }
                }
                else
                {
                    transactionScope.Dispose();
                }
            }

            output.EventEnd("migration", "Migration");
        }       

        public MigrationMapStateInfo GetState(MigrationMap migrationMap)
        {
            var executedMigrations = dataProvider.ListExecutedMigrations(identifier).Select(x => x.MigrationId);
            var migrationMapStateInfo = new MigrationMapStateInfo()
            {
                MigrationNodesInfo = migrationMap.MigrationNodes.Select(x =>{
                    var nodeInfo = new MigrationNodeStateInfo() { MigrationNode=x };
                    nodeInfo.MigrationsInfo = x.Migrations.Select(y =>
                    {
                        var migrationInfo = new MigrationStateInfo() { Migration = y };
                        migrationInfo.CurrentState = executedMigrations.Contains(y.Identifier) ? Migration.MigrationState.Executed : Migration.MigrationState.ToUpgrade;
                        return migrationInfo;
                    }).ToList();
                    return nodeInfo;
                }).ToList()
            };
            return migrationMapStateInfo;
        }

    }
}