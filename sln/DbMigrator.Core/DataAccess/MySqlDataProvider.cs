using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DbMigrator.Core.DataAccess.Entities;
using MySql.Data.MySqlClient;

namespace DbMigrator.Core.DataAccess
{
    public class MySqlDataProvider : IDataProvider
    {
        private readonly string _connectionString;

        public MySqlDataProvider(string connectionString)
        {
            this._connectionString = connectionString;
        }

        private MySqlConnection NewConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        #region string util

        /// <summary>
        /// Transform an sql block in many commands to be allocated in a transaction.
        /// </summary>
        private string[] SplitSqlCommandsComplex(string query)
        {
            bool inLiteral = false;
            string inCommentEnder = null;
            List<int> goPositions = new List<int>();
            bool match;
            try
            {
                string text = query.ToUpper();
                for (int i = 0; i < text.Length; i++)
                {
                    char c = text[i];
                    if (inCommentEnder != null)
                    {
                        //IN COMMENT, LOOKING FOR AN END
                        match = true;
                        for (int j = 0; j < inCommentEnder.Length; j++) { if (inCommentEnder[j] != text[i + j]) { match = false; break; } }
                        if (match)
                        {
                            i += (inCommentEnder.Length - 1);
                            inCommentEnder = null;
                        }
                    }
                    else if (inLiteral)
                    {
                        //IN LITERAL, LOOKING FOR AN END
                        if (c == '\'')
                        {
                            int length = 1;
                            while (text[i + length] == '\'')
                            {
                                length++;
                            }
                            i += length - 1;
                            if (length % 2 == 0)
                            {
                                continue;
                            }
                            else
                            {
                                inLiteral = false;
                                continue;
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        //LOOKING FOR GO, COMMENTS OR LITERAL STARTS

                        //test comment start
                        if (c == '-' && text[i + 1] == '-')
                        {
                            inCommentEnder = Environment.NewLine;
                            i += 1;
                            continue;
                        }
                        else if (c == '/' && text[i + 1] == '*')
                        {
                            inCommentEnder = "*/";
                            i += 1;
                            continue;
                        }

                        //test literal start
                        if (c == '\'')
                        {
                            int length = 1;
                            while (text[i + length] == '\'')
                            {
                                length++;
                            }
                            if (length % 2 == 0)
                            {
                                i += (length - 1);
                                continue;
                            }
                            else
                            {
                                inLiteral = true;
                            }
                        }
                        if (c == 'G' && text[i + 1] == 'O')
                        {
                            //test surrounding to see if it's a real GO!
                            int start = i - 4;
                            int end = i + 4;
                            if (start < 0) { start = 0; }
                            if (end > text.Length - 1) { end = text.Length - 1; }
                            var fragment = text.Substring(start, end - start);
                            if (Regex.IsMatch(fragment, @"\sGO\s"))
                            {
                                goPositions.Add(i);
                            }
                            i++;
                        }
                    }

                }
            }
            catch (IndexOutOfRangeException ex)
            {
                //ignore
            }

            List<string> commands = new List<string>();


            goPositions.Add(query.Length);
            if (goPositions[0] != 0)
            {
                goPositions.Insert(0, 0);
            }

            int startSub = 0;
            int subLength = 0;
            for (int i = 0; i + 1 < goPositions.Count; i++)
            {
                startSub = goPositions[i] + (i > 0 ? 2 : 0);
                subLength = goPositions[i + 1] - startSub;
                var command = query.Substring(startSub, subLength);
                command = TrimSqlQuery(command);
                if (!string.IsNullOrWhiteSpace(command))
                    commands.Add(command);
            }



            return commands.ToArray();
        }

        /// <summary>
        /// Remove unnecessary content from the beginning and the end of a sql query.
        /// </summary>
        private string TrimSqlQuery(string query)
        {
            if (query != null)
            {
                query = new Regex("(^GO|GO$)", RegexOptions.Multiline | RegexOptions.IgnoreCase).Replace(query.Trim(), "").Trim();
            }
            return query;
        }

        #endregion

        #region util

        public bool DataTableExist()
        {
            using (var connection = NewConnection())
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(@"SELECT COUNT(*) 
FROM information_schema.tables
WHERE table_schema = @database
    AND table_name = 'DbM_Data'
LIMIT 1;", connection);

                command.Parameters.Add(
                new MySqlParameter("database", connection.Database) { MySqlDbType = MySqlDbType.VarChar, Size = 100 });
                try
                {
                    var result = (int)command.ExecuteScalar() == 1;
                    connection.Close();
                    return result;
                }
                catch (MySqlException ex)
                {
                    connection.Close();
                    throw ex;
                }

            }
        }

        #endregion

        #region get/set data

        public string GetData(string key)
        {
            var data = GetDataRecord(key);
            if (data == null)
            {
                return null;
            }
            else
            {
                return data.Value;
            }
        }

        private Entities.Data GetDataRecord(string key)
        {
            MySqlCommand command = new MySqlCommand("SELECT * FROM DbM_Data WHERE Key = @Key LIMIT 1");

            command.Parameters.Add(
                new MySqlParameter("Key", key) { MySqlDbType = MySqlDbType.VarChar, Size = 100 });

            Entities.Data item = null;

            using (var connection = NewConnection())
            {
                connection.Open();
                command.Connection = connection;
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    item = ParseData(reader);
                }
                reader.Close();
                connection.Close();
            }

            return item;
        }

        private Entities.Data ParseData(MySqlDataReader reader)
        {
            var item = new Entities.Data()
            {
                Id = (int)reader["Id"],
                Value = (string)reader["Value"],
                Key = (string)reader["Key"]
            };
            return item;
        }

        public void SetData(string key, string value)
        {
            var data = GetDataRecord(key);
            MySqlCommand command;
            using (var connection = NewConnection())
            {
                connection.Open();
                if (data == null)
                {
                    command = GetDataInsertQueryCommand(new Entities.Data() { Key = key, Value = value });
                }
                else
                {
                    command = GetDataUpdateByKeyQueryCommand(new Entities.Data() { Key = key, Value = value });
                }
                command.Connection = connection;
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        private MySqlCommand GetDataInsertQueryCommand(Entities.Data data)
        {
            string query = @"INSERT INTO DbM_Data
    (Key 
    ,Value)
VALUES
    (@Key
    ,@Value);
";
            MySqlCommand command = new MySqlCommand(query);
            command.Parameters.Add(new MySqlParameter("Key", data.Key) { MySqlDbType = MySqlDbType.VarChar, Size = 100 });
            command.Parameters.Add(new MySqlParameter("Value", data.Value) { MySqlDbType = MySqlDbType.VarChar, Size = 255 });

            return command;
        }

        private MySqlCommand GetDataUpdateByKeyQueryCommand(Entities.Data data)
        {
            string query = @"UPDATE DbM_Data SET
    Value = @Value
WHERE Key = @Key;
";
            MySqlCommand command = new MySqlCommand(query);
            command.Parameters.Add(new MySqlParameter("Key", data.Key) { MySqlDbType = MySqlDbType.VarChar, Size = 50 });
            command.Parameters.Add(new MySqlParameter("Value", data.Value) { MySqlDbType = MySqlDbType.VarChar, Size = 255 });

            return command;
        }

        #endregion

        public bool MigrationRunnerAlreadyRunned(string migrationRunnerIdentifier)
        {
            throw new NotImplementedException();
        }

        public void ExecuteSql(string sql)
        {
            using (var connection = NewConnection())
            {
                connection.Open();
                foreach (var _sql in SplitSqlCommandsComplex(sql))
                {
                    MySqlCommand command = new MySqlCommand(_sql, connection);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        #region entity ExecutedMigration

        public List<Entities.ExecutedMigration> ListExecutedMigrations(string migrationRunnerId)
        {
            List<ExecutedMigration> list = new List<ExecutedMigration>();

            MySqlCommand command = new MySqlCommand("SELECT * FROM DbM_ExecutedMigration WHERE MigrationRunnerId = @MigrationRunnerId");

            command.Parameters.Add(
                new MySqlParameter("MigrationRunnerId", migrationRunnerId) { MySqlDbType = MySqlDbType.VarChar, Size = 80 });

            using (var connection = NewConnection())
            {
                connection.Open();
                command.Connection = connection;
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var item = ParseExecutedMigration(reader);

                    list.Add(item);
                }
                reader.Close();
                connection.Close();
            }


            return list;
        }

        public void InsertExecutedMigration(Entities.ExecutedMigration executedMigration)
        {
            using (var connection = NewConnection())
            {
                connection.Open();
                MySqlCommand command = GetExecutedMigrationInsertQueryCommand(executedMigration);
                command.Connection = connection;
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        private MySqlCommand GetExecutedMigrationInsertQueryCommand(ExecutedMigration executedMigration)
        {
            string query = @"INSERT INTO DbM_ExecutedMigration
           (MigrationRunnerId
           ,MigrationNodeId
           ,MigrationId
           ,LastRunDate
           ,LastRunScript)
     VALUES
           (@MigrationRunnerId
           ,@MigrationNodeId
           ,@MigrationId
           ,@LastRunDate
           ,@LastRunScript);
";
            MySqlCommand command = new MySqlCommand(query);
            command.Parameters.Add(new MySqlParameter("MigrationRunnerId", executedMigration.MigrationRunnerId) { MySqlDbType = MySqlDbType.VarChar, Size = 80 });
            command.Parameters.Add(new MySqlParameter("MigrationNodeId", executedMigration.MigrationNodeId) { MySqlDbType = MySqlDbType.VarChar, Size = 100 });
            command.Parameters.Add(new MySqlParameter("MigrationId", executedMigration.MigrationId) { MySqlDbType = MySqlDbType.VarChar, Size = 150 });
            command.Parameters.Add(new MySqlParameter("LastRunDate", executedMigration.LastRunDate));
            command.Parameters.Add(new MySqlParameter("LastRunScript", executedMigration.LastRunScript) { MySqlDbType = MySqlDbType.VarChar, Size = Int32.MaxValue });

            return command;
        }

        private ExecutedMigration ParseExecutedMigration(MySqlDataReader reader)
        {
            var item = new ExecutedMigration()
            {
                Id = (int)reader["Id"],
                LastRunDate = (DateTime)reader["LastRunDate"],
                LastRunScript = (string)reader["LastRunScript"],
                MigrationId = (string)reader["MigrationId"],
                MigrationNodeId = (string)reader["MigrationNodeId"],
                MigrationRunnerId = (string)reader["MigrationRunnerId"]
            };
            return item;
        }

        #endregion

        public void UpgradeSchema()
        {
            int version = 0;
            if (DataTableExist())
            {
                version = Int32.Parse(GetData("SchemaVersion"));
            }

            var query = "";
            if (version == 0)
            {
                query += @"
CREATE TABLE DbM_Data(
	Id int(11) NOT NULL AUTO_INCREMENT,
	Key varchar(MAX) NOT NULL,
	Value varchar(255) NULL,
 PRIMARY KEY (Id)
)

CREATE TABLE DbM_ExecutedMigration(
	Id int(11) NOT NULL AUTO_INCREMENT,
	MigrationRunnerId varchar(80) NOT NULL,
    MigrationNodeId varchar(100) NOT NULL,
	MigrationId varchar(150) NOT NULL,
	LastRunDate datetime NOT NULL,
	LastRunScript varchar(max) NOT NULL
 PRIMARY KEY (Id)
)";
                version++;
            }

            using (var connection = NewConnection())
            {
                connection.Open();
                foreach (var queryCmd in SplitSqlCommandsComplex(query))
                {
                    MySqlCommand command = new MySqlCommand(queryCmd, connection);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            SetData("SchemaVersion", version.ToString());
        }
    }
}
