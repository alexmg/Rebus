using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Rebus.Transports.Msmq.Management
{
    public class WindowsServer2003Installer : IMsmqInstaller
    {
        readonly IEnumerable<string> installComponents;
        readonly IEnumerable<string> unsupportedComponents;

        public WindowsServer2003Installer()
        {
            installComponents = new[] {"msmq_Core", "msmq_LocalStorage"};
            unsupportedComponents = new[] {"msmq_MulticastInstalled"};
        }

        public Process Install()
        {
            string fileName = Path.GetTempFileName();

            using (StreamWriter writer = File.CreateText(fileName))
            {
                writer.WriteLine("[Version]");
                writer.WriteLine("Signature = \"$Windows NT$\"");
                writer.WriteLine();
                writer.WriteLine("[Global]");
                writer.WriteLine("FreshMode = Custom");
                writer.WriteLine("MaintenanceMode = RemoveAll");
                writer.WriteLine("UpgradeMode = UpgradeOnly");
                writer.WriteLine();
                writer.WriteLine("[Components]");

                foreach (string cmoponent in installComponents.Except(unsupportedComponents))
                    writer.WriteLine(cmoponent + " = ON");

                writer.Flush();
            }

            string arguments = @"/i:sysoc.inf /x /q /w /u:%temp%\" + Path.GetFileName(fileName);

            return Process.Start("sysocmgr", arguments);
        }
    }
}