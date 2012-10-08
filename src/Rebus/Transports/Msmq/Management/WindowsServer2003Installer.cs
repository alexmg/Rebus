using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Rebus.Transports.Msmq.Management
{
    public class WindowsServer2003Installer : IMsmqInstaller
    {
        readonly IEnumerable<string> installComponents;

        public WindowsServer2003Installer()
        {
            installComponents = new[] {"msmq_Core", "msmq_LocalStorage"};
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

                foreach (string component in installComponents)
                    writer.WriteLine(component + " = ON");

                writer.Flush();
            }

            string arguments = @"/i:sysoc.inf /x /q /w /u:%temp%\" + Path.GetFileName(fileName);

            return Process.Start("sysocmgr", arguments);
        }
    }
}