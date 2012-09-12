using System;
using System.Collections.Generic;

namespace Rebus.Bus
{
    /// <summary>
    /// Transaction context that really means "no transaction". Sort of a null object implementation
    /// of a transaction context.
    /// </summary>
    public class NoTransaction : ITransactionContext, IDisposable
    {
        readonly Dictionary<string, object> items = new Dictionary<string, object>();

        public bool IsTransactional { get { return false; } }

        /// <summary>
        /// Constructs the context and sets itself as current in <see cref="TransactionContext"/>.
        /// </summary>
        public NoTransaction()
        {
            TransactionContext.Set(this);
        }

        public object this[string key]
        {
            get { return items.ContainsKey(key) ? items[key] : null; }
            set { items[key] = value; }
        }

        public event Action DoCommit
        {
            add { throw new InvalidOperationException("Don't add commit/rollback events when you're nontransactional"); }
            remove{}
        }

        public event Action DoRollback
        {
            add{ throw new InvalidOperationException("Don't add commit/rollback events when you're nontransactional");}
            remove{}
        }

        public event Action BeforeCommit
        {
            add { throw new InvalidOperationException("Don't add commit/rollback events when you're nontransactional"); }
            remove{}
        }

        public event Action AfterRollback
        {
            add{ throw new InvalidOperationException("Don't add commit/rollback events when you're nontransactional");}
            remove{}
        }

        public event Action Cleanup = delegate { };

        public void Dispose()
        {
            TransactionContext.Clear();
            Cleanup();
        }
    }
}