using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Linq;
using DbMigrator.Core.Migration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbMigrator.Core.Test
{
    [TestClass]
    public class JsonFileMigrationMapProviderUnitTest
    {

        private Stream StringToStream(string src)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(src);
            return new MemoryStream(byteArray);
        }

        [TestMethod]
        public void TestMethod1()
        {
            string map = @"
{
	""Identifier"": ""CaixaDigitalDb"",
	""Versions"":[
		{ ""Name"":""3.4.0.Pre"", ""Migrations"":[
			{ ""Description"":""Serve apenas para definir uma raiz anterior a 3.4.0.Root."", ""File"":""3.4.0.Pre\\core\\Empty.sql"", ""HelpEmail"":""juliano@digitaldesk.com.br"", ""Type"":""Script"" }
		]}
		,{ ""Name"":""3.4.0.Root"", ""Root"":""true"", ""Migrations"":[
			{ ""Description"":""Cria o schema básico."", ""File"":""3.4.0.Root\\core\\create.sql"", ""Type"":""Script"" }
		]}
		,{ ""Name"":""3.4.0"", ""Migrations"":[
			{ ""Description"":""Executa os scripts de ajuste de dados trazidos das versões anteriores incorporadas a 3.4.0."", ""File"":""3.4.0\\instance\\_any\\InsertDataMerged.sql"", ""Type"":""Script"" }
			,{ ""Description"":""Altera as tabelas MarcaModeloVeiculoHistorico e MarcaModeloVeiculo."", ""File"":""3.4.0\\core\\AlterTables-7728.sql"", ""Type"":""Script"" }
		]}
	]
}
";
            //JsonMigrationMapProvider mapProvider = new JsonMigrationMapProvider(StringToStream(map), "C:\\MigrationScripts");
            //var migrationMap = mapProvider.GetMigrationMap();
        }

        [TestMethod]
        public void TestMethod2()
        {
            FlaggedFileMigration fileMigration = new FlaggedFileMigration("C:\\Migrations", "v1\\Instance01\\Upgrade.sql", "Upgrade instance 01 to v1.");
            var flags = fileMigration.GetFlags();
            if (flags.Count() != 2)
            {
                Assert.Fail();
            }
        }
    }
}
