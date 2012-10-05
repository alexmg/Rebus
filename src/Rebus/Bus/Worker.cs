using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Transactions;
using Rebus.Logging;

namespace Rebus.Bus
{
    /// <summary>
    /// Internal worker thread that continually attempts to receive messages and dispatch to handlers.
    /// </summary>
    class Worker : IDisposable
    {
        static ILog log;

        static Worker()
        {
            RebusLoggerFactory.Changed += f => log = f.GetCurrentClassLogger();
        }

        /// <summary>
        /// Caching of dispatcher methods
        /// </summary>
        readonly ConcurrentDictionary<Type, MethodInfo> dispatchMethodCache = new ConcurrentDictionary<Type, MethodInfo>();

        readonly Thread workerThread;
        readonly Dispatcher dispatcher;
        readonly IErrorTracker errorTracker;
        readonly IReceiveMessages receiveMessages;
        readonly ISerializeMessages serializeMessages;

        internal event Action<ReceivedTransportMessage> BeforeTransportMessage = delegate { };

        internal event Action<Exception, ReceivedTransportMessage> AfterTransportMessage = delegate { };

        internal event Action<ReceivedTransportMessage, PoisonMessageInfo> PoisonMessage = delegate { };

        internal event Action<object> BeforeMessage = delegate { };

        internal event Action<Exception, object> AfterMessage = delegate { };

        internal event Action<object, Saga> UncorrelatedMessage = delegate { }; 

        volatile bool shouldExit;
        volatile bool shouldWork;

        public Worker(IErrorTracker errorTracker,
            IReceiveMessages receiveMessages,
            IActivateHandlers activateHandlers,
            IStoreSubscriptions storeSubscriptions,
            ISerializeMessages serializeMessages,
            IStoreSagaData storeSagaData,
            IInspectHandlerPipeline inspectHandlerPipeline,
            string workerThreadName,
            IHandleDeferredMessage handleDeferredMessage)
        {
            this.receiveMessages = receiveMessages;
            this.serializeMessages = serializeMessages;
            this.errorTracker = errorTracker;
            dispatcher = new Dispatcher(storeSagaData, activateHandlers, storeSubscriptions, inspectHandlerPipeline, handleDeferredMessage);
            dispatcher.UncorrelatedMessage += RaiseUncorrelatedMessage;

            workerThread = new Thread(MainLoop) { Name = workerThreadName };
            workerThread.Start();

            log.Info("Worker {0} created and inner thread started", WorkerThreadName);
        }

        void RaiseUncorrelatedMessage(object message, Saga saga)
        {
            UncorrelatedMessage(message, saga);
        }

        /// <summary>
        /// Event that will be raised whenever dispatching a given message has failed MAX number of times
        /// (usually 5 or something like that).
        /// </summary>
        public event Action<ReceivedTransportMessage, string> MessageFailedMaxNumberOfTimes = delegate { };

        /// <summary>
        /// Event that will be raised each time message delivery fails.
        /// </summary>
        public event Action<Worker, Exception> UserException = delegate { };

        /// <summary>
        /// Event that will be raised if an exception occurs outside of user code.
        /// </summary>
        public event Action<Worker, Exception> SystemException = delegate { };

        public void Start()
        {
            log.Info("Starting worker thread {0}", WorkerThreadName);
            shouldWork = true;
        }

        public void Pause()
        {
            log.Info("Pausing worker thread {0}", WorkerThreadName);
            shouldWork = false;
        }

        public void Stop()
        {
            log.Info("Stopping worker thread {0}", WorkerThreadName);
            shouldWork = false;
            shouldExit = true;
        }

        public void Dispose()
        {
            log.Info("Disposing worker thread {0}", WorkerThreadName);

            if (shouldWork)
            {
                log.Info("Worker thread {0} is currently working", WorkerThreadName);
                Stop();
            }

            if (!workerThread.Join(TimeSpan.FromSeconds(30)))
            {
                log.Info("Worker thread {0} did not exit within 30 seconds - aborting!", WorkerThreadName);
                workerThread.Abort();
            }
        }

        public string WorkerThreadName
        {
            get { return workerThread.Name; }
        }

        void MainLoop()
        {
            while (!shouldExit)
            {
                if (!shouldWork)
                {
                    Thread.Sleep(20);
                    continue;
                }

                try
                {
                    TryProcessIncomingMessage();
                }
                catch (Exception e)
                {
                    // if there's two levels of TargetInvocationExceptions, it's user code that threw...
                    if (e is TargetInvocationException && e.InnerException is TargetInvocationException)
                    {
                        UserException(this, e.InnerException.InnerException);
                    }
                    else
                    {
                        SystemException(this, e);
                    }
                }
            }
        }

        void TryProcessIncomingMessage()
        {
            using (var context = new TxBomkarl())
            {
                try
                {
                    using (var transactionScope = BeginTransaction())
                    {
                        DoTry(transactionScope);
                    }
                    context.RaiseDoCommit();
                }
                catch
                {
                    context.RaiseDoRollback();
                    throw;
                }
            }
        }

        void DoTry(TransactionScope transactionScope)
        {
            var transportMessage = receiveMessages.ReceiveMessage(TransactionContext.Current);

            if (transportMessage == null)
            {
                Thread.Sleep(20);
                return;
            }

            var id = transportMessage.Id;
            var label = transportMessage.Label;

            MessageContext context = null;

            if (errorTracker.MessageHasFailedMaximumNumberOfTimes(id))
            {
                log.Error("Handling message {0} has failed the maximum number of times", id);
                var errorText = errorTracker.GetErrorText(id);
                var poisonMessageInfo = errorTracker.GetPoisonMessageInfo(id);

                MessageFailedMaxNumberOfTimes(transportMessage, errorText);
                errorTracker.StopTracking(id);

                try
                {
                    PoisonMessage(transportMessage, poisonMessageInfo);
                }
                catch (Exception exceptionWhileRaisingEvent)
                {
                    log.Error("An exception occurred while raising the PoisonMessage event: {0}",
                              exceptionWhileRaisingEvent);
                }
            }
            else
            {
                try
                {
                    BeforeTransportMessage(transportMessage);

                    var message = serializeMessages.Deserialize(transportMessage);

                    // successfully deserialized the transport message, let's enter a message context
                    context = MessageContext.Enter(message.Headers);

                    foreach (var logicalMessage in message.Messages)
                    {
                        context.SetLogicalMessage(logicalMessage);

                        try
                        {
                            BeforeMessage(logicalMessage);

                            var typeToDispatch = logicalMessage.GetType();

                            log.Debug("Dispatching message {0}: {1}", id, typeToDispatch);

                            GetDispatchMethod(typeToDispatch).Invoke(this, new[] {logicalMessage});

                            AfterMessage(null, logicalMessage);
                        }
                        catch (Exception exception)
                        {
                            try
                            {
                                AfterMessage(exception, logicalMessage);
                            }
                            catch (Exception exceptionWhileRaisingEvent)
                            {
                                log.Error(
                                    "An exception occurred while raising the AfterMessage event, and an exception occurred some time before that as well. The first exception was this: {0}. And then, when raising the AfterMessage event (including the details of the first error), this exception occurred: {1}",
                                    exception, exceptionWhileRaisingEvent);
                            }
                            throw;
                        }
                        finally
                        {
                            context.ClearLogicalMessage();
                        }
                    }

                    AfterTransportMessage(null, transportMessage);
                }
                catch (Exception exception)
                {
                    log.Debug("Handling message {0} ({1}) has failed", label, id);
                    try
                    {
                        AfterTransportMessage(exception, transportMessage);
                    }
                    catch (Exception exceptionWhileRaisingEvent)
                    {
                        log.Error(
                            "An exception occurred while raising the AfterTransportMessage event, and an exception occurred some time before that as well. The first exception was this: {0}. And then, when raising the AfterTransportMessage event (including the details of the first error), this exception occurred: {1}",
                            exception, exceptionWhileRaisingEvent);
                    }
                    errorTracker.TrackDeliveryFail(id, exception);
                    if (context != null) context.Dispose(); //< dispose it if we entered
                    throw;
                }
            }

            transactionScope.Complete();
            if (context != null) context.Dispose(); //< dispose it if we entered
            errorTracker.StopTracking(id);
        }

        TransactionScope BeginTransaction()
        {
            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TransactionManager.DefaultTimeout
            };
            return new TransactionScope(TransactionScopeOption.Required, transactionOptions);
        }

        MethodInfo GetDispatchMethod(Type typeToDispatch)
        {
            MethodInfo method;
            if (dispatchMethodCache.TryGetValue(typeToDispatch, out method))
            {
                return method;
            }

            var newMethod = GetType()
                .GetMethod("DispatchGeneric", BindingFlags.Instance | BindingFlags.NonPublic)
                .MakeGenericMethod(typeToDispatch);

            dispatchMethodCache.TryAdd(typeToDispatch, newMethod);

            return newMethod;
        }

        /// <summary>
        /// Private strongly typed dispatcher method. Will be invoked through reflection to allow
        /// for some strongly typed interaction from this point and on....
        /// </summary>
        internal void DispatchGeneric<T>(T message)
        {
            dispatcher.Dispatch(message);
        }
    }
}