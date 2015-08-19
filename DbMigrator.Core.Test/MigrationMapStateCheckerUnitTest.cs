using System;
using System.Linq;
using System.Collections.Generic;
using DbMigrator.Core.Migration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbMigrator.Core.Test
{
    [TestClass]
    public class MigrationMapStateCheckerUnitTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            MigrationMap migrationMap = new MigrationMap()
            {
                Identifier="Some",
                MigrationNodes = new List<IMigrationNode>()
            };

            migrationMap.MigrationNodes.Add(new MigrationNode()
            {
                Identifier = "N1",
                Migrations = (new IMigration[]{
                    new SimpleMigration("M1","M1",""),
                    new SimpleMigration("M2","M2",""){
                    }
                }).ToList()
            });
            migrationMap.MigrationNodes.Add(new MigrationNode()
            {
                Identifier = "N1",
                Migrations = (new IMigration[]{
                    new SimpleMigration("M3","M3",""),
                    new SimpleMigration("M4","M4","")
                }).ToList()
            });
        }
    }
}
