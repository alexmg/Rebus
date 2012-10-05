using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Win32;

namespace Rebus.Transports.Msmq.Management
{
    public class MsmqManagement
    {
        readonly IEnumerable<string> installedComponents = GetInstalledComponents();
        readonly IEnumerable<string> requiredComponents;

        public MsmqManagement()
        {
            requiredComponents = new[] {"msmq_Core", "msmq_LocalStorage"};
        }

        public bool IsInstalled()
        {
            if (GetMissingComponents().Any())
                return false;

            return true;
        }

        public void Install()
        {
            IMsmqInstaller installer;
            switch (GetWindowsVersion())
            {
                case WindowsVersion.TooOldToCare:
                case WindowsVersion.Windows2000:
                    throw new NotSupportedException("The Windows version is too old to support automatic installation");

                case WindowsVersion.WindowsXp:
                case WindowsVersion.Windows2003:
                    installer = new WindowsServer2003Installer();
                    break;

                case WindowsVersion.WindowsVista:
                    installer = new WindowsVistaInstaller();
                    break;

                case WindowsVersion.Windows2008:
                    installer = new WindowsServer2008Installer();
                    break;

                case WindowsVersion.Windows7:
                case WindowsVersion.Windows2008R2:
                    installer = new WindowsServer2008R2Installer();
                    break;

                default:
                    throw new NotSupportedException(
                        "The Windows version was not recognized, installation cannot continue.");
            }

            using (Process process = installer.Install())
            {
                process.WaitForExit();
            }
        }

        IEnumerable<string> GetMissingComponents()
        {
            return requiredComponents.Except(installedComponents);
        }

        static WindowsVersion GetWindowsVersion()
        {
            OperatingSystem osInfo = Environment.OSVersion;
            Version version = osInfo.Version;

            if (osInfo.Platform == PlatformID.Win32Windows)
                return WindowsVersion.TooOldToCare;

            if (osInfo.Platform != PlatformID.Win32NT)
                return WindowsVersion.TooOldToCare;

            if (version.Major < 5)
                return WindowsVersion.TooOldToCare;

            switch (version.Major)
            {
                case 5:
                    if (version.Minor == 0)
                        return WindowsVersion.Windows2000;
                    if (version.Minor == 1)
                        return WindowsVersion.WindowsXp;
                    return WindowsVersion.Windows2003;

                case 6:
                    if (version.Minor == 0)
                        return WindowsVersion.WindowsVista;
                    return WindowsVersion.Windows2008R2;

                default:
                    return WindowsVersion.Unknown;
            }
        }

        static IEnumerable<string> GetInstalledComponents()
        {
            IEnumerable<string> installedComponents = Enumerable.Empty<string>();

            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\MSMQ\Setup");
            if (registryKey == null)
                return installedComponents;

            using (registryKey)
            {
                installedComponents = registryKey.GetValueNames();
                registryKey.Close();
            }

            return installedComponents;
        }

        public void Start()
        {
            using (var service = new WindowsService("MSMQ"))
            {
                service.Start();
            }
        }
    }
}