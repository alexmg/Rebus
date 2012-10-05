using System.Diagnostics;

namespace Rebus.Transports.Msmq.Management
{
    public interface IMsmqInstaller
    {
        Process Install();
    }
}