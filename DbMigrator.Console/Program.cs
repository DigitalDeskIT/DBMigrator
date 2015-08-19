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

                runner.SetOutputHandler(new ConsoleOutputHandler());

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
