using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using DbMigrator.Core.Migration;

namespace DbMigrator.Core
{
    public class JsonMigrationMapProvider : IMigrationMapProvider
    {
        //private Stream stream;
        private string basePath;
        private string configPath;

        public JsonMigrationMapProvider(string configPath, string basePath)
        {
            this.configPath = configPath;
            this.basePath = basePath;
            if (this.basePath == null)
            {
                throw new ArgumentException("Could not resolve a base path.");
            }
        }

        private string StreamToString(Stream stream)
        {
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        public MigrationMap GetMigrationMap()
        {
            var migrationMap = new MigrationMap();
            JsonFileMigrationMap jsonMap = JsonFileMigrationMap.FromJsonFile(configPath);
            
            migrationMap.MigrationNodes = new List<IMigrationNode>();
            migrationMap.Identifier = jsonMap.Identifier;

            foreach (var version in jsonMap.Versions)
            {
                var node = new MigrationNode();
                node.Identifier = version.Name;
                node.Migrations = new List<IMigration>();

                foreach (var migration in version.Migrations)
                {
                    var nMigration = new FlaggedFileMigration(basePath, migration.File, migration.Description);
                    node.Migrations.Add(nMigration);
                }
                migrationMap.MigrationNodes.Add(node);
            }

            return migrationMap;
        }
    }

    [DataContract]
    public class JsonFileMigrationMap
    {
        public static JsonFileMigrationMap FromJsonFile(string file)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(JsonFileMigrationMap));
            var json = System.IO.File.ReadAllText(file);
            using (var s = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                return (JsonFileMigrationMap)serializer.ReadObject(s);
            }
        }

        public static JsonFileMigrationMap FromJsonStream(Stream stream)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(JsonFileMigrationMap));
            return (JsonFileMigrationMap)serializer.ReadObject(stream);

        }

        public string ToJson(Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(JsonFileMigrationMap));

            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, this);
                return encoding.GetString(ms.ToArray());
            }
        }

        [DataMember]
        public string Identifier { get; set; }

        public JsonFileMigrationMap()
        {
            this.Versions = new List<Version>();
        }

        [DataMember]
        public List<Version> Versions { get; set; }

        [DataContract(Name = "Migration")]
        public class Migration
        {
            [DataMember]
            public string Type { get; set; }

            [DataMember]
            public string File { get; set; }

            [DataMember]
            public string HelpEmail { get; set; }

            [DataMember]
            public string Description { get; set; }

            [DataMember]
            public string ForEach { get; set; }

            [DataMember]
            public string Encoding { get; set; }
        }

        [DataContract(Name = "Version")]
        public class Version
        {
            public Version()
            {
                this.Migrations = new List<Migration>();
            }

            [DataMember]
            public string Name { get; set; }

            [DataMember]
            public string AfterVersion { get; set; }

            [DataMember]
            public bool Root { get; set; }

            [DataMember]
            public List<Migration> Migrations { get; set; }
        }
    }
}
