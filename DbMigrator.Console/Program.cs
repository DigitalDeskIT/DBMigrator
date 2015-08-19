using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbMigrator.ConsoleApp
{
    class Program
    {
        static ConsoleOutputHandler output = new ConsoleOutputHandler();

        static void Main(string[] args)
        {
            var opts = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, opts))
            {
                var dataProvider = new Core.DataAccess.MSSqlDataProvider(opts.ConnectionString);

                Core.MigrationRunner runner = new Core.MigrationRunner(
                    opts.Identifier,
                    dataProvider,
                    new Core.JsonMigrationMapProvider(opts.ScriptsMapPath, opts.ScriptsRootPath)
                );

                if (opts.Dynamic > 0)
                {
                    SetupDynamicRunner(runner, opts.Dynamic);
                }

                runner.SetOutputHandler(output);

                runner.AddFilter(new Core.MigrationFilter.RootMigrationFilter());

                if(!string.IsNullOrWhiteSpace(opts.TrimExpression))
                    runner.AddFilter(new Core.MigrationFilter.FlagMigrationFilter(new Core.Util.FlagFilter(opts.TrimExpression)));

                if (!string.IsNullOrWhiteSpace(opts.FirstNode))
                    runner.AddFilter(new Core.MigrationFilter.FirstNodeMigrationFilter(opts.FirstNode));

                if (!string.IsNullOrWhiteSpace(opts.LastNode))
                    runner.AddFilter(new Core.MigrationFilter.LastNodeMigrationFilter(opts.LastNode));

                if(opts.Mode=="migrate")
                    runner.Migrate(true);
                else if (opts.Mode == "test")
                    runner.Migrate(false);                
                else
                    Console.WriteLine("Invalid mode.");
                
            }
            else
            {
                Console.WriteLine(opts.GetUsage());
            }

#if DEBUG
            Console.ReadKey();
#endif
        }

        private static TEnum? TryReadEnum<TEnum>() where TEnum:struct
        {
            TEnum parsedEnum;
            Type enumType = typeof(TEnum);
            string opts = string.Join(", ", Enum.GetNames(enumType));
            output.Info(opts + "?");
            string input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                return default(TEnum);
            }
            Enum.TryParse<TEnum>(input, true, out parsedEnum);
            return parsedEnum;
        }

        private static void SetupDynamicRunner(Core.MigrationRunner runner, int dynamicLevel)
        {
            if (dynamicLevel > 0)
            {
                runner.SetOnCompletingTransaction((context) =>
                {
                    output.Info("You are about to complete the migration process. What would you like to do?");
                    DbMigrator.Core.OnCompletingTransactionDecision? decision = null;
                    while (decision == null)
                    {
                        decision = TryReadEnum<DbMigrator.Core.OnCompletingTransactionDecision>();
                    }
                    context.Decision = decision.Value;
                });
            }
            if (dynamicLevel > 1)
            {
                runner.SetOnMigrating((context) =>
                {
                    output.Info(string.Format("Migration {0}", context.Migration.Identifier));

                    DbMigrator.Core.OnMigratingDecision? decision = null;
                    while (decision == null)
                    {
                        decision = TryReadEnum<DbMigrator.Core.OnMigratingDecision>();
                    }
                    context.Decision = decision.Value;
                });

                runner.SetOnMigrationError((context) =>
                {
                    output.Info("An error has ocurred. What would you like to do?");
                    DbMigrator.Core.OnMigrationErrorDecision? decision = null;
                    while (decision == null)
                    {
                        decision = TryReadEnum<DbMigrator.Core.OnMigrationErrorDecision>();
                    }
                    context.Decision = decision.Value;
                });
            }
        }
    }

    public class Options
    {
        [Option("mode", Required = false, HelpText = "Migrate or Test.", DefaultValue="test")]
        public string Mode { get; set; }

        [Option("dynamic", Required = false, DefaultValue=0, HelpText = "If the console should ask questions during the process. 0=Never, 1=CommitTransactionOnly, 2=EveryStep")]
        public int Dynamic { get; set; }

        [Option("identifier", Required = true, HelpText = "The migration runner identifier.")]
        public string Identifier { get; set; }

        [Option("verbose", Required=false, DefaultValue=false, HelpText = "Print details during execution.")]
        public bool Verbose { get; set; }

        [Option("scriptsMapPath", Required = true, HelpText = "Path to the migration map path.")]
        public string ScriptsMapPath { get; set; }

        [Option("scriptsRootPath", Required = true, HelpText = "Path to the migration root path (where all sql files are contained).")]
        public string ScriptsRootPath { get; set; }

        [Option("connectionString", Required = true, HelpText = "The database (that will be migrated) connection string.")]
        public string ConnectionString { get; set; }

        [Option("trimExpression", Required = false, DefaultValue = "", HelpText = "An expression to trim the migrations files.")]
        public string TrimExpression { get; set; }

        [HelpVerbOption]
        public string GetUsage()
        {
            return CommandLine.Text.HelpText.AutoBuild(this);
        }

        [Option("firstNode", Required = false, DefaultValue = "", HelpText = "The first node to consider in the map. All previous nodes will be ignored.")]
        public string LastNode { get; set; }

        [Option("lastNode", Required = false, DefaultValue = "", HelpText = "The last node to consider in the map. All next nodes will be ignored.")]
        public string FirstNode { get; set; }
    }
}
