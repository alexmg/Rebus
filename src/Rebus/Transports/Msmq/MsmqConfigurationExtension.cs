using System;
using System.Configuration;
using Rebus.Bus;
using Rebus.Configuration;
using Rebus.Shared;
using Rebus.Transports.Msmq.Management;
using ConfigurationException = Rebus.Configuration.ConfigurationException;

namespace Rebus.Transports.Msmq
{
    public static class MsmqConfigurationExtension
    {
        /// <summary>
        /// Specifies that you want to use MSMQ to both send and receive messages. The input
        /// queue will be automatically created if it doesn't exist.
        /// </summary>
        public static void UseMsmq(this RebusTransportConfigurer configurer, string inputQueue, string errorQueue)
        {
            DoIt(configurer, inputQueue, errorQueue);
        }

        public static void UseMsmqInOneWayClientMode(this RebusTransportConfigurer configurer)
        {
            var msmqMessageQueue = MsmqMessageQueue.Sender();

            configurer.UseSender(msmqMessageQueue);
            var gag = new OneWayClientGag();
            configurer.UseReceiver(gag);
            configurer.UseErrorTracker(gag);
        }

        public class OneWayClientGag : IReceiveMessages, IErrorTracker
        {
            public ReceivedTransportMessage ReceiveMessage(ITransactionContext context)
            {
                throw new NotImplementedException();
            }

            public string InputQueue { get; private set; }
            public string InputQueueAddress { get; private set; }
            public void StopTracking(string id)
            {
                throw new NotImplementedException();
            }

            public bool MessageHasFailedMaximumNumberOfTimes(string id)
            {
                throw new NotImplementedException();
            }

            public string GetErrorText(string id)
            {
                throw new NotImplementedException();
            }

            public PoisonMessageInfo GetPoisonMessageInfo(string id)
            {
                throw new NotImplementedException();
            }

            public void TrackDeliveryFail(string id, Exception exception)
            {
                throw new NotImplementedException();
            }

            public string ErrorQueueAddress { get; private set; }
        }

        /// <summary>
        /// Specifies that you want to use MSMQ to both send and receive messages. The input
        /// queue name will be deduced from the Rebus configuration section in the application
        /// configuration file. The input queue will be automatically created if it doesn't exist.
        /// </summary>
        public static void UseMsmqAndGetInputQueueNameFromAppConfig(this RebusTransportConfigurer configurer)
        {
            try
            {
                var section = RebusConfigurationSection.LookItUp();

                var inputQueueName = section.InputQueue;

                if (string.IsNullOrEmpty(inputQueueName))
                {
                    throw new ConfigurationErrorsException("Could not get input queue name from Rebus configuration section. Did you forget the 'inputQueue' attribute?");
                } 

                var errorQueueName = section.ErrorQueue;

                if (string.IsNullOrEmpty(errorQueueName))
                {
                    throw new ConfigurationErrorsException("Could not get input queue name from Rebus configuration section. Did you forget the 'errorQueue' attribute?");
                } 

                DoIt(configurer, inputQueueName, errorQueueName);
            }
            catch(ConfigurationErrorsException e)
            {
                throw new ConfigurationException(
                    @"
An error occurred when trying to parse out the configuration of the RebusConfigurationSection:

{0}

-

For this way of configuring input queue to work, you need to supply a correct configuration
section declaration in the <configSections> element of your app.config/web.config - like so:

    <configSections>
        <section name=""rebus"" type=""Rebus.Configuration.RebusConfigurationSection, Rebus"" />
        <!-- other stuff in here as well -->
    </configSections>

-and then you need a <rebus> element some place further down the app.config/web.config,
like so:

    <rebus inputQueue=""my.service.input.queue"" errorQueue=""my.service.error.queue"" />

Note also, that specifying the input queue name with the 'inputQueue' attribute is optional.

A more full example configuration snippet can be seen here:

{1}
",
                    e, RebusConfigurationSection.ExampleSnippetForErrorMessages);
            }
        }

        static void DoIt(RebusTransportConfigurer configurer, string inputQueueName, string errorQueueName)
        {
            if (string.IsNullOrEmpty(inputQueueName))
            {
                throw new ConfigurationErrorsException("You need to specify an input queue.");
            }

            var msmqMessageQueue = new MsmqMessageQueue(inputQueueName);

            var errorQueuePath = MsmqUtil.GetPath(errorQueueName);

            MsmqUtil.EnsureMessageQueueExists(errorQueuePath);
            MsmqUtil.EnsureMessageQueueIsTransactional(errorQueuePath);

            configurer.UseSender(msmqMessageQueue);
            configurer.UseReceiver(msmqMessageQueue);
            configurer.UseErrorTracker(new ErrorTracker(errorQueueName));
        }

        /// <summary>
        /// Will force an installation of MSMQ if it is not installed
        /// </summary>
        /// <param name="configurator"></param>
        public static RebusConfigurer VerifyMsmqConfiguration(this RebusConfigurer configurator)
        {
            var management = new MsmqManagement();

            if (!management.IsInstalled())
            {
                management.Install();
            }
            management.Start();

            return configurator;
        }

        /// <summary>
        /// This method will verify that the MS DTC is installed and properly configured. If
        /// the configuration is invalid or the MS DTC is not installed, it will be installed,
        /// configured, and/or started.
        /// </summary>
        /// <param name="configurator"></param>
        public static RebusConfigurer VerifyMsDtcConfiguration(this RebusConfigurer configurator)
        {
            var management = new MsDtcManagement();

            management.VerifyConfiguration(true, true);
            management.VerifyRunning(true);

            return configurator;
        }
    }
}