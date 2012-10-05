using System.Diagnostics;

namespace Rebus.Transports.Msmq.Management
{
    public class WindowsVistaInstaller : IMsmqInstaller
    {
        const string Arguments = "MSMQ-Container;MSMQ-Server;MSMQ-Multicasting /passive";
        const string OCSetup = "OCSETUP";

        public Process Install()
        {
            return Process.Start(OCSetup, Arguments);
        }
    }
}