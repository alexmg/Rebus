using System;
using System.Messaging;
using Rebus.Configuration;
using Rebus.Logging;
using Rebus.Shared;
using Rebus.Extensions;

namespace Rebus.Transports.Msmq
{
    /// <summary>
    /// MSMQ implementation of <see cref="ISendMessages"/> and <see cref="IReceiveMessages"/>. Will
    /// enlist in ambient transaction during send and receive if one is present.
    /// </summary>
    public class MsmqMessageQueue : IDuplexTransport, IDisposable
    {
        const string CurrentTransactionKey = "current_transaction";
        static ILog log;
        readonly object disposeLock = new object();
        bool disposed;

        static MsmqMessageQueue()
        {
            RebusLoggerFactory.Changed += f => log = f.GetCurrentClassLogger();
        }

        readonly MessageQueue inputQueue;
        readonly string inputQueuePath;
        readonly string inputQueueName;
        readonly string machineAddress;

        public static MsmqMessageQueue Sender()
        {
            return new MsmqMessageQueue(null);
        }

        public MsmqMessageQueue(string inputQueueName)
        {
            if (inputQueueName == null) return;

            try
            {
                machineAddress = GetMachineAddress();

                inputQueuePath = MsmqUtil.GetPath(inputQueueName);
                MsmqUtil.EnsureMessageQueueExists(inputQueuePath);
                MsmqUtil.EnsureMessageQueueIsTransactional(inputQueuePath);
                EnsureMessageQueueIsLocal(inputQueueName);

                inputQueue = GetMessageQueue(inputQueuePath);

                this.inputQueueName = inputQueueName;
            }
            catch (MessageQueueException e)
            {
                throw new ArgumentException(
                    string.Format(
                        @"An error occurred while initializing MsmqMessageQueue - attempted to use '{0}' as input queue",
                        inputQueueName), e);
            }
        }

        string GetMachineAddress()
        {
            return RebusConfigurationSection.GetConfigurationValueOrDefault(s => s.Address, Environment.MachineName);
        }

        void EnsureMessageQueueIsLocal(string queueName)
        {
            if (!queueName.Contains("@")) return;

            var tokens = queueName.Split('@');

            if (tokens.Length == 2 && tokens[1].In(".", "localhost", "127.0.0.1")) return;

            throw new ArgumentException(string.Format(@"Attempted to use {0} as an input queue, but the input queue must always be local!

If you could use a remote queue as an input queue, one of the nifty benefits of MSMQ would be defeated,
because there would be remote calls involved when you wanted to receive a message.", queueName));
        }

        public string InputQueueAddress
        {
            get { return InputQueue + "@" + machineAddress; }
        }

        public ReceivedTransportMessage ReceiveMessage(ITransactionContext context)
        {
            try
            {
                if (!context.IsTransactional)
                {
                    var transaction = new MessageQueueTransaction();
                    transaction.Begin();
                    var message = inputQueue.Receive(TimeSpan.FromSeconds(1), transaction);
                    if (message == null)
                    {
                        log.Warn("Received NULL message - how weird is that?");
                        transaction.Commit();
                        return null;
                    }
                    var body = message.Body;
                    if (body == null)
                    {
                        log.Warn("Received message with NULL body - how weird is that?");
                        transaction.Commit();
                        return null;
                    }
                    var transportMessage = (ReceivedTransportMessage)body;
                    transaction.Commit();
                    return transportMessage;
                }
                else
                {
                    var transaction = GetTransaction(context);
                    var message = inputQueue.Receive(TimeSpan.FromSeconds(1), transaction);
                    if (message == null)
                    {
                        log.Warn("Received NULL message - how weird is that?");
                        return null;
                    }
                    var body = message.Body;
                    if (body == null)
                    {
                        log.Warn("Received message with NULL body - how weird is that?");
                        return null;
                    }
                    var transportMessage = (ReceivedTransportMessage)body;
                    return transportMessage;
                }
            }
            catch (MessageQueueException)
            {
                return null;
            }
            catch (Exception e)
            {
                log.Error(e, "An error occurred while receiving message from {0}", inputQueuePath);
                throw;
            }
        }

        public string InputQueue
        {
            get { return inputQueueName; }
        }

        public void Send(string destinationQueueName, TransportMessageToSend message, ITransactionContext context)
        {
            var recipientPath = MsmqUtil.GetSenderPath(destinationQueueName);

            if (!context.IsTransactional)
            {
                using (var outputQueue = GetMessageQueue(recipientPath))
                using (var transaction = new MessageQueueTransaction())
                {
                    transaction.Begin();
                    outputQueue.Send(message, transaction);
                    transaction.Commit();
                }
                return;
            }

            using (var outputQueue = GetMessageQueue(recipientPath))
            {
                outputQueue.Send(message, GetTransaction(context));
            }
        }

        static MessageQueueTransaction GetTransaction(ITransactionContext context)
        {
            var transaction = context[CurrentTransactionKey] as MessageQueueTransaction;
            if (transaction != null) return transaction;

            transaction = new MessageQueueTransaction();
            context[CurrentTransactionKey] = transaction;

            context.DoCommit += transaction.Commit;
            context.DoRollback += transaction.Abort;
            context.Cleanup += transaction.Dispose;

            transaction.Begin();

            return transaction;
        }

        public MsmqMessageQueue PurgeInputQueue()
        {
            if (string.IsNullOrEmpty(inputQueuePath)) return this;

            log.Warn("Purging queue {0}", inputQueuePath);
            inputQueue.Purge();

            return this;
        }

        public MsmqMessageQueue DeleteInputQueue()
        {
            if (MessageQueue.Exists(inputQueuePath))
            {
                log.Warn("Deleting {0}", inputQueuePath);
                MessageQueue.Delete(inputQueuePath);
            }
            return this;
        }

        public void Dispose()
        {
            if (disposed) return;

            lock (disposeLock)
            {
                if (disposed) return;

                try
                {
                    if (inputQueue == null) return;

                    log.Info("Disposing message queue {0}", inputQueuePath);
                    inputQueue.Dispose();
                }
                finally
                {
                    disposed = true;
                }
            }
        }

        public override string ToString()
        {
            return string.Format("MsmqMessageQueue: {0}", inputQueuePath);
        }

        MessageQueue GetMessageQueue(string path)
        {
            var messageQueue = new MessageQueue(path)
                {
                    Formatter = new RebusTransportMessageFormatter(),
                    MessageReadPropertyFilter = RebusTransportMessageFormatter.PropertyFilter
                };
            return messageQueue;
        }

    }
}