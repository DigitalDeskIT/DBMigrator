using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbMigrator.Core.Migration
{
    public class SimpleMigration : IMigration
    {
        private string sql;
        private string description;
        public SimpleMigration(string identifier, string description, string sql)
        {
            this.Identifier = identifier;
            this.sql = sql;
            this.description = description;
        }

        public string Identifier
        {
            get;
            private set;
        }

        public string GetDescription()
        {
            return description;
        }

        public string GetUpgradeSql()
        {
            return sql;
        }
    }
}
