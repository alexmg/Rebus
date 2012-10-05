using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Win32;
using Rebus.Configuration;

namespace Rebus.Transports.Msmq.Management
{
    /// <summary>
    /// Manages the configuration of the MS DTC, since this screws a lot of 
    /// people up a lot of the time when dealing with MSSQL and MSMQ. This is 
    /// in MSMQ since none of the other queue transports support the DTC.
    /// </summary>
    public class MsDtcManagement
    {
        readonly IEnumerable<string> registryValues;

        public MsDtcManagement()
        {
            registryValues = new[]
            {
                "NetworkDtcAccess",
                "NetworkDtcAccessOutbound",
                "NetworkDtcAccessTransactions",
                "XaTransactions"
            };
        }

        public void VerifyRunning(bool allowStart)
        {
            using (var service = new WindowsService("MSDTC"))
            {
                bool running = service.IsRunning();
                if (running)
                    return;

                if (!allowStart)
                    throw new InvalidOperationException("The MSDTC is not running and allowStart was not specified.");

                service.Start();
            }
        }

        public void VerifyConfiguration(bool allowChanges, bool allowInstall)
        {
            try
            {
                RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\MSDTC\Security",
                                                                           allowChanges);
                if (registryKey == null)
                {
                    if (allowInstall)
                    {
                        Install();

                        registryKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\MSDTC\Security",
                                                                       allowChanges);
                        if (registryKey == null)
                            throw new NotSupportedException("The MSDTC is not installed and could not be installed.");
                    }
                    else
                        throw new NotSupportedException("The MSDTC is not installed.");
                }

                using (registryKey)
                {
                    var incorrectValues = registryValues
                        .Select(key => new {Key = key, Value = (int)registryKey.GetValue(key)})
                        .Where(x => x.Value == 0)
                        .ToArray();

                    if (!incorrectValues.Any())
                        return;

                    if (!allowChanges)
                        throw new ConfigurationException("The MSDTC is not properly configured.");

                    foreach (var incorrectValue in incorrectValues)
                        registryKey.SetValue(incorrectValue.Key, 1, RegistryValueKind.DWord);

                    Restart();
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new InvalidOperationException("The configuration could not be changed due to access permissions", ex);
            }
        }

        void Restart()
        {
            using (var service = new WindowsService("MSDTC"))
            {
                service.Restart();
            }
        }

        void Install()
        {
            using (var process = Process.Start("MSDTC.EXE", "-install"))
            {
                process.WaitForExit();
            }
        }
    }
}
