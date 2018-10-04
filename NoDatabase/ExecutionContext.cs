using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoDatabase
{
    public class ExecutionContext
    {
        DataStore m_dataStore;

        public string ContextName { get; internal set; }

        public DataStore Connection
        {
            get { return m_dataStore; }

            set
            {
                if(m_dataStore == null)
                {
                    m_dataStore = value;
                }
                else
                {
                    throw new InvalidOperationException("The connection property is already initialized");
                }
            }
        }

    }

    public class ContextFactory
    {
        public delegate void OnCreateContextHandler(string contextName, out DataStore ds);
        public delegate void OnAfterCreateContextHandler(ExecutionContext ctx);

        public OnCreateContextHandler OnCreateContext;
        public OnAfterCreateContextHandler OnAfterCreateContext;

        private static object m_lockObject = new object();
        private static volatile ContextFactory s_instance;

        private ContextFactory()
        {
        }

        public static ContextFactory Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (m_lockObject)
                    {
                        if (s_instance == null)
                            s_instance = new ContextFactory();
                    }
                }

                return s_instance;
            }
        }

        private List<ExecutionContext> m_contexts = new List<ExecutionContext>();

        private ExecutionContext GetContext(string contextName)
        {
            ExecutionContext retVal = m_contexts.Where(x => x.ContextName.Trim().Equals(contextName.Trim(), StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

            if (retVal != null)
                return retVal;

            if (OnCreateContext == null)
                throw new NullReferenceException("ContextFactory.OnCreateContext must have a handler specified.");

            retVal = new ExecutionContext
            {
                ContextName = contextName
            };

            DataStore dataStore;

            OnCreateContext(contextName, out dataStore);

            retVal.Connection = dataStore;

            m_contexts.Add(retVal);

            OnAfterCreateContext?.Invoke(retVal);

            return retVal;
        }

        public ExecutionContext this[string contextName]
        {
            get
            {
                return GetContext(contextName);
            }
        }
    }
}
