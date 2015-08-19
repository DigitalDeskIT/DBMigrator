using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbMigrator.Core.Migration
{
    public class FileMigration : IMigration
    {
        private string description;
        private bool ignoreIfNotExist;
        private string path;
        private Encoding encoding = Encoding.Default;

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

        public FileMigration(string path, string description, bool ignoreIfNotExist = false)
        {
            this.description = description;
            this.ignoreIfNotExist = ignoreIfNotExist;
            this.path = path;
        }

        public string Identifier
        {
            get { return path; }
        }

        public string GetDescription()
        {
            return description;
        }

        public string GetUpgradeSql()
        {
            if (System.IO.File.Exists(path))
            {
                return System.IO.File.ReadAllText(path, this.Encoding);
            }
            else if (!ignoreIfNotExist)
            {
                throw new ArgumentException(string.Format("The required file \"{0}\" was not found.", path));
            }
            else
            {
                return "";
            }
        }

        public bool ContainsFlag(string key)
        {
            return false;
        }
    }
}
