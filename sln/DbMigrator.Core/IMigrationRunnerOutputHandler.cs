using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbMigrator.Core
{
    public interface IMigrationRunnerOutputHandler
    {
        void EventBegin(string eventKey, string eventName, string info = null);

        void EventEnd(string eventKey, string eventName, string info = null);

        void Info(string info);

        void ProgressUpdate(string processName, int percent, string currentMessage = null);
    }

    internal class SilentMigratorRunnerOutputHandler : IMigrationRunnerOutputHandler
    {
        public void EventBegin(string eventKey, string eventName, string info = null) { }

        public void EventEnd(string eventKey, string eventName, string info = null) { }

        public void Info(string info) { }

        public void ProgressUpdate(string processName, int percent, string currentMessage) { }
    }
}
