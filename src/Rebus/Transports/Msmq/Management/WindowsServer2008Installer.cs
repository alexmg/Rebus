using System.Diagnostics;

namespace Rebus.Transports.Msmq.Management
{
    public class WindowsServer2008Installer : IMsmqInstaller
    {
        const string Arguments = "MSMQ-Server;MSMQ-Multicasting /passive";
        const string OCSetup = "OCSETUP";

        public Process Install()
        {
            return Process.Start(OCSetup, Arguments);
        }
    }
}