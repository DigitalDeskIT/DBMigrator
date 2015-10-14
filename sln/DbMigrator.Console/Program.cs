using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DbMigrator.ConsoleApp
{
    class Program
    {
        static ConsoleOutputHandler output = new ConsoleOutputHandler();

        static int Main(string[] args)
        {
            try
            {
                string invokedVerb = "";
                object invokedVerbInstance = null;

                var options = new Options();

                try
                {
                    CommandLine.Parser.Default.ParseArguments(args, options, (verb, subOptions) =>
                    {
                        invokedVerb = verb;
                        invokedVerbInstance = subOptions;
                    });
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine("Accepted verbs: migrate, trim.");
                    return -1;
                }

                if (!string.IsNullOrWhiteSpace(invokedVerb))
                {
                    invokedVerb = invokedVerb.ToLowerInvariant();

                    if (invokedVerb == "migrate")
                    {
                        if (invokedVerbInstance == null)
                        {
                            Console.WriteLine(new MigrateOptions().GetUsage());
                        }
                        else
                        {
                            Migrate((MigrateOptions)invokedVerbInstance);
                            return 0;
                        }
                    }
                    else if (invokedVerb == "trim")
                    {
                        if (invokedVerbInstance == null)
                        {
                            Console.WriteLine(new TrimOptions().GetUsage());
                        }
                        else
                        {
                            Trim((TrimOptions)invokedVerbInstance);
                            return 0;
                        }
                    }
                    else
                    {
                        throw new NotImplementedException("Verb not implemented.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
#if DEBUG
                Console.ReadKey();
#endif
            }
#if DEBUG
            Console.ReadKey();
#endif
            return -1;
        }

        private static void Trim(TrimOptions trimOptions)
        {
            new FolderTrimmer().TrimFolder(trimOptions.Folder, trimOptions.Filter);
        }

        private static void Migrate(MigrateOptions opts)
        {
            var dataProvider = new Core.DataAccess.MSSqlDataProvider(opts.ConnectionString);

            if (string.IsNullOrWhiteSpace(opts.ScriptsRootPath))
                opts.ScriptsRootPath = new FileInfo(opts.ScriptsMapPath).Directory.ToString();

            Core.MigrationRunner runner = new Core.MigrationRunner(
                dataProvider,
                new Core.JsonMigrationMapProvider(opts.ScriptsMapPath, opts.ScriptsRootPath),
                opts.Identifier
            );

            if (opts.Mode != null)
                opts.Mode = opts.Mode.ToLowerInvariant().Trim();
            if (opts.Mode == "migrate")
            {
                SetupMigrateRunner(runner);
            }
            else if (opts.Mode == "test")
            {
                SetupTestRunner(runner);
            }
            else if (opts.Mode == "interactive")
            {
                SetupDynamicRunner(runner);
            }
            else
            {
                throw new ArgumentException("Invalid migration mode.");
            }

            runner.SetOutputHandler(output);

            runner.AddFilter(new Core.MigrationFilter.RootMigrationFilter(dataProvider, opts.Identifier));

            if (!string.IsNullOrWhiteSpace(opts.TrimExpression))
                runner.AddFilter(new Core.MigrationFilter.FlagMigrationFilter(new Core.Util.FlagFilter(opts.TrimExpression)));

            if (!string.IsNullOrWhiteSpace(opts.FirstNode))
                runner.AddFilter(new Core.MigrationFilter.FirstNodeMigrationFilter(opts.FirstNode));

            if (!string.IsNullOrWhiteSpace(opts.LastNode))
                runner.AddFilter(new Core.MigrationFilter.LastNodeMigrationFilter(opts.LastNode));

            runner.Migrate();
        }

        private static void SetupMigrateRunner(Core.MigrationRunner runner)
        {
            runner.SetOnMigrationError((ctx) =>
            {
                output.Info("Migration error!"); Console.WriteLine(ctx.Exception); ctx.Decision = Core.OnMigrationErrorDecision.Stop;
            });
            runner.SetOnCompletingTransaction((ctx) =>
            {
                ctx.Decision = ctx.HasErrors ? Core.OnCompletingTransactionDecision.Rollback : Core.OnCompletingTransactionDecision.Commit;
            });
        }

        private static void SetupTestRunner(Core.MigrationRunner runner)
        {
            runner.SetOnMigrationError((ctx) =>
            {
                output.Info("Migration error!");
                Console.WriteLine(ctx.Exception);
                ctx.Decision = Core.OnMigrationErrorDecision.Stop;
            });
            runner.SetOnCompletingTransaction((ctx) =>
            {
                ctx.Decision = Core.OnCompletingTransactionDecision.Rollback;
            });
        }

        private static TEnum? TryReadEnum<TEnum>() where TEnum:struct
        {
            
            TEnum parsedEnum;
            Type enumType = typeof(TEnum);
            var optsArr = Enum.GetNames(enumType).Select(x => string.Format("[{0}]{1}", (int)Enum.Parse(enumType, x), x)).ToArray();
            string opts = string.Join(", ", optsArr);
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            output.Info(opts + "?");
            Console.ResetColor();
            string input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                return default(TEnum);
            }
            Regex isNumber = new Regex("^[0-9]+$");
            if (isNumber.IsMatch(input))
            {
                return (TEnum)(object)Int32.Parse(input);
            }
            else
            {
                if (!Enum.TryParse<TEnum>(input, true, out parsedEnum))
                    return null;
                return parsedEnum;
            }
        }

        private static void SetupDynamicRunner(Core.MigrationRunner runner)
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
            
            runner.SetOnMigrating((context) =>
            {
                output.Info(string.Format("Current Migration: {0}", context.Migration.Identifier));

                DbMigrator.Core.OnMigratingDecision? decision = null;
                while (decision == null)
                {
                    decision = TryReadEnum<DbMigrator.Core.OnMigratingDecision>();
                }
                context.Decision = decision.Value;
            });

            runner.SetOnMigrationError((context) =>
            {
                output.Info(context.Exception.ToString());
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

    public class Options
    {
        [VerbOption("migrate")]
        public MigrateOptions MigrateOptions { get; set; }

        [VerbOption("trim")]
        public MigrateOptions TrimItems { get; set; }
    }

    public class MigrateOptions
    {
        [Option("mode", Required = false, HelpText = "Migrate, Test or Interactive.", DefaultValue="test")]
        public string Mode { get; set; }

        [Option("identifier", Required = false, HelpText = "The migration runner identifier. Set only if you want to override the map (JSON file) identifier.")]
        public string Identifier { get; set; }

        [Option("verbose", Required=false, DefaultValue=false, HelpText = "Print details during execution.")]
        public bool Verbose { get; set; }

        [Option("scriptsMapPath", Required = true, HelpText = "Path to the migration map path (JSON file).")]
        public string ScriptsMapPath { get; set; }

        [Option("scriptsRootPath", Required = false, HelpText = "Path to the migration root path (where all sql files are contained). The default value is the folder that contains the migration map (JSON).")]
        public string ScriptsRootPath { get; set; }

        [Option("connectionString", Required = true, HelpText = "The database connection string.")]
        public string ConnectionString { get; set; }

        [Option("trimExpression", Required = false, DefaultValue = "", HelpText = "An expression to trim the migrations files.")]
        public string TrimExpression { get; set; }

        [Option("firstNode", Required = false, DefaultValue = "", HelpText = "The first node to consider in the map. All previous nodes will be ignored.")]
        public string LastNode { get; set; }

        [Option("lastNode", Required = false, DefaultValue = "", HelpText = "The last node to consider in the map. All next nodes will be ignored.")]
        public string FirstNode { get; set; }

        [HelpVerbOption]
        public string GetUsage()
        {
            return CommandLine.Text.HelpText.AutoBuild(this);
        }
    }

    public class TrimOptions
    {
        [Option("folder", Required = false, HelpText = "Root folder.")]
        public string Folder { get; set; }

        [Option("filter", Required = true, HelpText = "The expression to filter files.")]
        public string Filter { get; set; }

        [HelpVerbOption]
        public string GetUsage()
        {
            return CommandLine.Text.HelpText.AutoBuild(this);
        }
    }
}
