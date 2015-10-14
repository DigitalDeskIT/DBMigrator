using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DbMigrator.Core.DataAccess.Entities;

namespace DbMigrator.Core.DataAccess
{
    public class MSSqlDataProvider : IDataProvider
    {
        private readonly string _connectionString;

        public MSSqlDataProvider(string connectionString)
        {
            this._connectionString = connectionString;
        }

        private SqlConnection NewConnection()
        {
            return new SqlConnection(_connectionString);
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
                if(!string.IsNullOrWhiteSpace(command))
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
                SqlCommand command = new SqlCommand(@"IF OBJECT_ID(N'dbo.DbM_Data', N'U') IS NOT NULL
	SELECT 1 as 'Exist'
ELSE
	SELECT 0 AS 'Exist'", connection);
                try
                {
                    var result = (int)command.ExecuteScalar() == 1;
                    connection.Close();
                    return result;
                }
                catch (SqlException ex)
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
            SqlCommand command = new SqlCommand("SELECT TOP 1 * FROM DbM_Data WHERE [Key] = @Key");

            command.Parameters.Add(
                new SqlParameter("Key", key) { SqlDbType = System.Data.SqlDbType.VarChar, Size = 100 });

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

        private Entities.Data ParseData(SqlDataReader reader)
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
            SqlCommand command;
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

        private SqlCommand GetDataInsertQueryCommand(Entities.Data data)
        {
            string query = @"INSERT INTO [dbo].[DbM_Data]
    ([Key] 
    ,[Value])
VALUES
    (@Key
    ,@Value);
";
            SqlCommand command = new SqlCommand(query);
            command.Parameters.Add(new SqlParameter("Key", data.Key) { SqlDbType = System.Data.SqlDbType.VarChar, Size = 100 });
            command.Parameters.Add(new SqlParameter("Value", data.Value) { SqlDbType = System.Data.SqlDbType.VarChar, Size = 255 });

            return command;
        }

        private SqlCommand GetDataUpdateByKeyQueryCommand(Entities.Data data)
        {
            string query = @"UPDATE [dbo].[DbM_Data] SET
    [Value] = @Value
WHERE [Key] = @Key;
";
            SqlCommand command = new SqlCommand(query);
            command.Parameters.Add(new SqlParameter("Key", data.Key) { SqlDbType = System.Data.SqlDbType.VarChar, Size = 50 });
            command.Parameters.Add(new SqlParameter("Value", data.Value) { SqlDbType = System.Data.SqlDbType.VarChar, Size = 255 });

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
                    SqlCommand command = new SqlCommand(_sql, connection);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        #region entity ExecutedMigration

        public List<Entities.ExecutedMigration> ListExecutedMigrations(string migrationRunnerId)
        {
            List<ExecutedMigration> list = new List<ExecutedMigration>();

            SqlCommand command = new SqlCommand("SELECT * FROM DbM_ExecutedMigration WHERE MigrationRunnerId = @MigrationRunnerId");

            command.Parameters.Add(
                new SqlParameter("MigrationRunnerId", migrationRunnerId) { SqlDbType = System.Data.SqlDbType.VarChar, Size = 80 });

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
                SqlCommand command = GetExecutedMigrationInsertQueryCommand(executedMigration);
                command.Connection = connection;
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        private SqlCommand GetExecutedMigrationInsertQueryCommand(ExecutedMigration executedMigration)
        {
            string query = @"INSERT INTO [dbo].[DbM_ExecutedMigration]
           ([MigrationRunnerId]
           ,[MigrationNodeId]
           ,[MigrationId]
           ,[LastRunDate]
           ,[LastRunScript])
     VALUES
           (@MigrationRunnerId
           ,@MigrationNodeId
           ,@MigrationId
           ,@LastRunDate
           ,@LastRunScript);
";
            SqlCommand command = new SqlCommand(query);
            command.Parameters.Add(new SqlParameter("MigrationRunnerId", executedMigration.MigrationRunnerId) { SqlDbType = System.Data.SqlDbType.VarChar, Size = 80 });
            command.Parameters.Add(new SqlParameter("MigrationNodeId", executedMigration.MigrationNodeId) { SqlDbType = System.Data.SqlDbType.VarChar, Size = 100 });
            command.Parameters.Add(new SqlParameter("MigrationId", executedMigration.MigrationId) { SqlDbType = System.Data.SqlDbType.VarChar, Size = 150 });
            command.Parameters.Add(new SqlParameter("LastRunDate", executedMigration.LastRunDate));
            command.Parameters.Add(new SqlParameter("LastRunScript", executedMigration.LastRunScript) { SqlDbType = System.Data.SqlDbType.VarChar, Size = Int32.MaxValue });

            return command;
        }

        private ExecutedMigration ParseExecutedMigration(SqlDataReader reader)
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
CREATE TABLE [dbo].[DbM_Data](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Key] [varchar](MAX) NOT NULL,
	[Value] [varchar](255) NULL,
 CONSTRAINT [PK_DbM_Data] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[DbM_ExecutedMigration](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MigrationRunnerId] [varchar](80) NOT NULL,
    [MigrationNodeId] [varchar](100) NOT NULL,
	[MigrationId] [varchar](150) NOT NULL,
	[LastRunDate] [datetime] NOT NULL,
	[LastRunScript] [varchar](max) NOT NULL
 CONSTRAINT [PK_DbM_ExecutedMigration] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

CREATE NONCLUSTERED INDEX IX_DbM_ExecutedMigration ON dbo.DbM_ExecutedMigration
	(
	MigrationId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO

CREATE NONCLUSTERED INDEX IX_DbM_ExecutedMigration_1 ON dbo.DbM_ExecutedMigration
	(
	MigrationRunnerId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO

CREATE NONCLUSTERED INDEX IX_DbM_ExecutedMigration_2 ON dbo.DbM_ExecutedMigration
	(
	LastRunDate
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO

CREATE UNIQUE NONCLUSTERED INDEX IX_DbM_ExecutedMigration_3 ON dbo.DbM_ExecutedMigration
	(
	MigrationRunnerId,
	MigrationId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]";
                version++;
            }

            using (var connection = NewConnection())
            {
                connection.Open();
                foreach (var queryCmd in SplitSqlCommandsComplex(query))
                {
                    SqlCommand command = new SqlCommand(queryCmd, connection);
                    command.ExecuteNonQuery();    
                }
                connection.Close();
            }
            SetData("SchemaVersion", version.ToString());
        }
    }
}
