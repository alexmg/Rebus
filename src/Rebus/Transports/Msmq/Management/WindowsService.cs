using System;
using System.Linq;
using System.ServiceProcess;
using Rebus.Logging;

namespace Rebus.Transports.Msmq.Management
{
    public class WindowsService : IDisposable
    {
        static ILog log;

        ServiceController controller;
        readonly TimeSpan timeout = TimeSpan.FromSeconds(30);

        static WindowsService()
        {
            RebusLoggerFactory.Changed += f => log = f.GetCurrentClassLogger();
        }

        public WindowsService(string serviceName)
        {
            controller = new ServiceController(serviceName);
        }

        public void Dispose()
        {
            controller.Dispose();
            controller = null;
        }

        public bool Start()
        {
            return ControlService(ServiceControllerStatus.Running, x => x.Start());
        }

        public bool Stop()
        {
            return ControlService(ServiceControllerStatus.Stopped, x => x.Stop());
        }

        public bool Restart()
        {
            Stop();

            return Start();
        }

        public bool IsStopped()
        {
            return IsServiceInStatus(ServiceControllerStatus.Stopped, ServiceControllerStatus.StopPending);
        }

        public bool IsRunning()
        {
            return IsServiceInStatus(ServiceControllerStatus.Running, ServiceControllerStatus.StartPending,
                                     ServiceControllerStatus.ContinuePending);
        }

        bool ControlService(ServiceControllerStatus status, Action<ServiceController> controlAction)
        {
            if (controller.Status == status)
            {
                log.Debug("The {0} service is already in the requested state: {1}", controller.ServiceName, status);
                return false;
            }

            log.Debug("Setting the {0} service to {1}", controller.ServiceName, status);

            try
            {
                controlAction(controller);
            }
            catch (Exception ex)
            {
                string message = string.Format("The {0} service could not be set to {1}", controller.ServiceName, status);
                throw new InvalidOperationException(message, ex);
            }

            controller.WaitForStatus(status, timeout);
            if (controller.Status == status)
            {
                log.Debug("The {0} service was set to {1} successfully", controller.ServiceName, status);
            }
            else
            {
                string message = string.Format("A timeout occurred waiting for the {0} service to be {1}",
                                               controller.ServiceName,
                                               status);
                throw new InvalidOperationException(message);
            }

            return true;
        }

        bool IsServiceInStatus(params ServiceControllerStatus[] statuses)
        {
            ServiceControllerStatus serviceStatus = controller.Status;

            return statuses.Contains(serviceStatus);
        }
    }
}