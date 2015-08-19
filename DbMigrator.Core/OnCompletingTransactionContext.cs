using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbMigrator.Core
{
    public class OnCompletingTransactionContext
    {
        public OnCompletingTransactionDecision Decision { get; set; }
        public bool HasErrors { get; set; }
    }

    public enum OnCompletingTransactionDecision
    {
        Commit, //default
        Rollback
    }
}
