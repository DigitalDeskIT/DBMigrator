using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DbMigrator.Core.Migration
{
    public class FlaggedFileMigration : IFlaggedMigration
    {
        private string baseFilePath;

        private string identifier;
        private string description;
        private string path;
        private string[] flags;
        private string query;
        private Encoding encoding = Encoding.Default;

        public string Path { get { return path; } }

        public Encoding Encoding
        {
            get
            {
                return encoding;
            }
            set
            {
                encoding = value;
            }
        }

        public FlaggedFileMigration(string baseFilePath, string relativePath, string description)
        {
            this.baseFilePath = baseFilePath;
            this.description = description;
            this.identifier = relativePath.Replace(" ", "");
            this.path = System.IO.Path.Combine(baseFilePath, new Regex("^[~\\\\]*").Replace(relativePath, ""));
            var _flags = new Regex("[~/\\\\]").Split(relativePath).Select(x => x.Replace("-", "_")).ToArray();
            this.flags = _flags.Take(_flags.Length - 1).ToArray();
        }

        public string Identifier
        {
            get { return identifier; }
        }

        public string GetDescription()
        {
            return description;
        }

        private string ResolvePath()
        {
            return System.IO.Path.Combine(baseFilePath, path);
        }

        public string Query
        {
            get
            {
                if (query == null)
                {
                    LoadQuery();
                }
                return query;
            }
        }

        public void LoadQuery()
        {
            var path = ResolvePath();
            if (System.IO.File.Exists(path))
            {
                this.query = System.IO.File.ReadAllText(path, encoding);
            }
            else
            {
                throw new ArgumentException(string.Format("The required file \"{0}\" was not found.", path));
            }
        }

        public string GetUpgradeSql()
        {
            return Query;
        }

        public bool ContainsFlags(params string[] flags)
        {
            return flags.Intersect(this.flags).Count() == flags.Count();
        }

        public IEnumerable<string> GetFlags()
        {
            return flags;
        }
    }
}
