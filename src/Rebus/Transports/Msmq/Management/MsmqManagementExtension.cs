using Rebus.Configuration;

namespace Rebus.Transports.Msmq.Management
{
    public static class MsmqManagementExtension
    {
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