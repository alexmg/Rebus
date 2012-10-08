using System;
using System.Diagnostics;
using System.IO;

namespace Rebus.Transports.Msmq.Management
{
    public class WindowsServer2008R2Installer : IMsmqInstaller
    {
        const string Arguments = "-install MSMQ-Services MSMQ-Server";
        const string ServerManager = "ServerManagerCmd.exe";

        public Process Install()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.System);
            if (IntPtr.Size == 4)
                path = path.Replace("system32", "SysNative");
            string file = Path.Combine(path, ServerManager);

            return Process.Start(file, Arguments);
        }
    }
}