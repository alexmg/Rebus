

//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Host.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf
{
    /// <summary>
    ///   A Host can be a number of configured service hosts, from installers to service runners
    /// </summary>
    internal interface Host
    {
        /// <summary>
        ///   Runs the configured host
        /// </summary>
        TopshelfExitCode Run();
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\HostControl.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf
{
    using System;

    /// <summary>
    /// Allows the service to control the host while running
    /// </summary>
    internal interface HostControl
    {
        /// <summary>
        /// Tells the Host that the service is still starting, which resets the
        /// timeout.
        /// </summary>
        void RequestAdditionalTime(TimeSpan timeRemaining);

        /// <summary>
        /// Stops the Host
        /// </summary>
        void Stop();

        /// <summary>
        /// Restarts the Host
        /// </summary>
        void Restart();
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\HostFactory.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf
{
    using System;
    using Configurators;
    using HostConfigurators;
    using Logging;

    /// <summary>
    ///   Configure and run a service host using the HostFactory
    /// </summary>
    internal static class HostFactory
    {
        /// <summary>
        ///   Configures a new service host
        /// </summary>
        /// <param name="configureCallback"> Configuration method to call </param>
        /// <returns> A Topshelf service host, ready to run </returns>
        public static Host New(Action<HostConfigurator> configureCallback)
        {
            if (configureCallback == null)
                throw new ArgumentNullException("configureCallback");

            var configurator = new HostConfiguratorImpl();

            Type declaringType = configureCallback.Method.DeclaringType;
            if (declaringType != null)
            {
                string defaultServiceName = declaringType.Namespace;
                if (!string.IsNullOrEmpty(defaultServiceName))
                    configurator.SetServiceName(defaultServiceName);
            }

            configureCallback(configurator);

            configurator.ApplyCommandLine();

            ConfigurationResult result = ValidateConfigurationResult.CompileResults(configurator.Validate());

            if (result.Message.Length > 0)
                HostLogger.Get(typeof(HostFactory))
                    .InfoFormat("Configuration Result:\n{0}", result.Message);

            return configurator.CreateHost();
        }

        /// <summary>
        ///   Configures and runs a new service host, handling any exceptions and writing them to the log.
        /// </summary>
        /// <param name="configureCallback"> Configuration method to call </param>
        /// <returns> Returns the exit code of the process that should be returned by your application's main method </returns>
        public static TopshelfExitCode Run(Action<HostConfigurator> configureCallback)
        {
            try
            {
                return New(configureCallback)
                    .Run();
            }
            catch (Exception ex)
            {
                HostLogger.Get(typeof(HostFactory))
                    .Error("The service terminated abnormally", ex);

                return TopshelfExitCode.AbnormalExit;
            }
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\HostStartContext.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf
{
    internal interface HostStartContext :
        HostControl
    {
        /// <summary>
        /// If called, prevents the service from starting
        /// </summary>
        void CancelStart();
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\HostStartedContext.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf
{
    internal interface HostStartedContext :
        HostControl
    {
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\HostStopContext.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf
{
    internal interface HostStopContext : 
        HostControl
    {
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\HostStoppedContext.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf
{
    internal interface HostStoppedContext :
        HostControl
    {
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\ServiceControl.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf
{
    internal interface ServiceControl
    {
        bool Start(HostControl hostControl);
        bool Stop(HostControl hostControl);
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\ServiceShutdown.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf
{
    /// <summary>
    /// Implemented by services that support service shutdown
    /// </summary>
    internal interface ServiceShutdown
    {
        /// <summary>
        /// Called when the operating system invokes the service shutdown method. There is little
        /// time to react here, but the application try to use RequestAdditionalTime if necessary,
        /// but this is really a shut down quick and bail method.
        /// </summary>
        /// <param name="hostControl"></param>
        void Shutdown(HostControl hostControl);
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\ServiceSuspend.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf
{
    /// <summary>
    /// If implemented by a service, used to pause/continue the service
    /// </summary>
    internal interface ServiceSuspend
    {
        bool Pause(HostControl hostControl);
        bool Continue(HostControl hostControl);
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\TopshelfExitCode.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf
{
    internal enum TopshelfExitCode
    {
        Ok = 0,
        AbnormalExit = 1,
        SudoRequired = 2,
        ServiceAlreadyInstalled = 3,
        ServiceNotInstalled = 4,
        StartServiceFailed = 5,
        StopServiceFailed = 6
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Credentials.cs
//------------------------------------------------------------------------------
// Copyright 2007-2011 The Apache Software Foundation.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf
{
	using System.ServiceProcess;


	internal class Credentials
	{
		public Credentials(string username, string password, ServiceAccount account)
		{
			Username = username;
			Account = account;
			Password = password;
		}

		public string Username { get; private set; }
		public string Password { get; private set; }
		public ServiceAccount Account { get; private set; }
	}
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\DependencyExtensions.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf
{
    using System;
    using Constants;
    using HostConfigurators;

    internal static class DependencyExtensions
    {
        public static HostConfigurator AddDependency(this HostConfigurator configurator, string name)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            if (name == null)
                throw new ArgumentNullException("name");

            var dependencyConfigurator = new DependencyHostConfigurator(name);

            configurator.AddConfigurator(dependencyConfigurator);

            return configurator;
        }

        public static HostConfigurator DependsOn(this HostConfigurator configurator, string name)
        {
            return AddDependency(configurator, name);
        }

        public static HostConfigurator DependsOnMsmq(this HostConfigurator configurator)
        {
            return AddDependency(configurator, KnownServiceNames.Msmq);
        }

        public static HostConfigurator DependsOnMsSql(this HostConfigurator configurator)
        {
            return AddDependency(configurator, KnownServiceNames.SqlServer);
        }

        public static HostConfigurator DependsOnEventLog(this HostConfigurator configurator)
        {
            return AddDependency(configurator, KnownServiceNames.EventLog);
        }

        public static HostConfigurator DependsOnIis(this HostConfigurator configurator)
        {
            return AddDependency(configurator, KnownServiceNames.IIS);
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\HelpHostConfiguratorExtensions.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf
{
    using System.Reflection;
    using HostConfigurators;

    internal static class HelpHostConfiguratorExtensions
    {
        /// <summary>
        /// Sets additional text to be displayed before the built-in help text is displayed
        /// </summary>
        /// <param name="hostConfigurator"></param>
        /// <param name="text"></param>
        public static HostConfigurator SetHelpTextPrefix(this HostConfigurator hostConfigurator, string text)
        {
            var configurator = new PrefixHelpTextHostConfigurator(text);

            hostConfigurator.AddConfigurator(configurator);

            return hostConfigurator;
        }

        /// <summary>
        /// Specifies a text resource to be loaded and displayed before the built-in system help text is displayed
        /// </summary>
        /// <param name="hostConfigurator"></param>
        /// <param name="assembly">The assembly containing the text resource</param>
        /// <param name="resourceName">The name of the embedded resource</param>
        public static HostConfigurator LoadHelpTextPrefix(this HostConfigurator hostConfigurator, Assembly assembly,
            string resourceName)
        {
            var configurator = new PrefixHelpTextHostConfigurator(assembly, resourceName);

            hostConfigurator.AddConfigurator(configurator);

            return hostConfigurator;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\InstallHostConfiguratorExtensions.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf
{
    using System;
    using HostConfigurators;
    using Runtime;

    internal static class InstallHostConfiguratorExtensions
    {
        public static HostConfigurator BeforeInstall(this HostConfigurator configurator, Action callback)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.AddConfigurator(new InstallHostConfiguratorAction("BeforeInstall",
                x => x.BeforeInstall(settings => callback())));

            return configurator;
        }

        public static HostConfigurator BeforeInstall(this HostConfigurator configurator,
            Action<InstallHostSettings> callback)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.AddConfigurator(new InstallHostConfiguratorAction("BeforeInstall",
                x => x.BeforeInstall(callback)));

            return configurator;
        }

        public static HostConfigurator AfterInstall(this HostConfigurator configurator, Action callback)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.AddConfigurator(new InstallHostConfiguratorAction("AfterInstall",
                x => x.AfterInstall(settings => callback())));

            return configurator;
        }

        public static HostConfigurator AfterInstall(this HostConfigurator configurator,
            Action<InstallHostSettings> callback)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.AddConfigurator(new InstallHostConfiguratorAction("AfterInstall", x => x.AfterInstall(callback)));

            return configurator;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\RunAsExtensions.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf
{
    using System;
    using System.ServiceProcess;
    using HostConfigurators;

    internal static class RunAsExtensions
    {
        public static HostConfigurator RunAs(this HostConfigurator configurator, string username, string password)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            var runAsConfigurator = new RunAsUserHostConfigurator(username, password);

            configurator.AddConfigurator(runAsConfigurator);

            return configurator;
        }

        public static HostConfigurator RunAsPrompt(this HostConfigurator configurator)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            var runAsConfigurator = new RunAsServiceAccountHostConfigurator(ServiceAccount.User);

            configurator.AddConfigurator(runAsConfigurator);

            return configurator;
        }

        public static HostConfigurator RunAsNetworkService(this HostConfigurator configurator)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            var runAsConfigurator = new RunAsServiceAccountHostConfigurator(ServiceAccount.NetworkService);

            configurator.AddConfigurator(runAsConfigurator);

            return configurator;
        }

        public static HostConfigurator RunAsLocalSystem(this HostConfigurator configurator)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            var runAsConfigurator = new RunAsServiceAccountHostConfigurator(ServiceAccount.LocalSystem);

            configurator.AddConfigurator(runAsConfigurator);

            return configurator;
        }

        public static HostConfigurator RunAsLocalService(this HostConfigurator configurator)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            var runAsConfigurator = new RunAsServiceAccountHostConfigurator(ServiceAccount.LocalService);

            configurator.AddConfigurator(runAsConfigurator);

            return configurator;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\ServiceConfiguratorExtensions.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf
{
    using System;
    using ServiceConfigurators;

    internal static class ServiceConfiguratorExtensions
    {
        public static ServiceConfigurator<T> ConstructUsing<T>(this ServiceConfigurator<T> configurator, Func<T> factory)
            where T : class
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.ConstructUsing(settings => factory());

            return configurator;
        }

        public static ServiceConfigurator<T> ConstructUsing<T>(this ServiceConfigurator<T> configurator,
            Func<string, T> factory)
            where T : class
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.ConstructUsing(settings => factory(typeof(T).Name));

            return configurator;
        }

        public static ServiceConfigurator<T> WhenStarted<T>(this ServiceConfigurator<T> configurator, Action<T> callback)
            where T : class
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.WhenStarted((service, control) =>
                {
                    callback(service);

                    return true;
                });

            return configurator;
        }

        public static ServiceConfigurator<T> WhenStopped<T>(this ServiceConfigurator<T> configurator, Action<T> callback)
            where T : class
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.WhenStopped((service, control) =>
                {
                    callback(service);

                    return true;
                });

            return configurator;
        }

        public static ServiceConfigurator<T> WhenPaused<T>(this ServiceConfigurator<T> configurator, Action<T> callback)
            where T : class
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.WhenPaused((service, control) =>
                {
                    callback(service);

                    return true;
                });

            return configurator;
        }

        public static ServiceConfigurator<T> WhenContinued<T>(this ServiceConfigurator<T> configurator,
            Action<T> callback)
            where T : class
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.WhenContinued((service, control) =>
                {
                    callback(service);

                    return true;
                });

            return configurator;
        }

        public static ServiceConfigurator<T> WhenShutdown<T>(this ServiceConfigurator<T> configurator,
            Action<T> callback)
            where T : class
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.WhenShutdown((service, control) =>
                {
                    callback(service);
                });

            return configurator;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\ServiceEventConfiguratorExtensions.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf
{
    using System;
    using ServiceConfigurators;

    internal static class ServiceEventConfiguratorExtensions
    {
        /// <summary>
        /// Registers a callback invoked before the service Start method is called.
        /// </summary>
        public static T BeforeStartingService<T>(this T configurator, Action callback)
            where T : ServiceConfigurator
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.BeforeStartingService(x => callback());

            return configurator;
        }

        /// <summary>
        /// Registers a callback invoked after the service Start method is called.
        /// </summary>
        public static T AfterStartingService<T>(this T configurator, Action callback)
            where T : ServiceConfigurator
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.BeforeStartingService(x => callback());

            return configurator;
        }

        /// <summary>
        /// Registers a callback invoked before the service Stop method is called.
        /// </summary>
        public static T BeforeStoppingService<T>(this T configurator, Action callback)
            where T : ServiceConfigurator
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.BeforeStartingService(x => callback());

            return configurator;
        }

        /// <summary>
        /// Registers a callback invoked after the service Stop method is called.
        /// </summary>
        public static T AfterStoppingService<T>(this T configurator, Action callback)
            where T : ServiceConfigurator
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.BeforeStartingService(x => callback());

            return configurator;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\ServiceExtensions.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf
{
    using System;
    using System.Linq;
    using Builders;
    using Configurators;
    using HostConfigurators;
    using Runtime;
    using ServiceConfigurators;

    internal static class ServiceExtensions
    {
        public static HostConfigurator Service<TService>(this HostConfigurator configurator,
            Func<HostSettings, TService> serviceFactory, Action<ServiceConfigurator> callback)
            where TService : class, ServiceControl
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            ServiceBuilderFactory serviceBuilderFactory = CreateServiceBuilderFactory(serviceFactory, callback);

            configurator.UseServiceBuilder(serviceBuilderFactory);

            return configurator;
        }

        public static ServiceBuilderFactory CreateServiceBuilderFactory<TService>(
            Func<HostSettings, TService> serviceFactory,
            Action<ServiceConfigurator> callback)
            where TService : class, ServiceControl
        {
            if (serviceFactory == null)
                throw new ArgumentNullException("serviceFactory");
            if (callback == null)
                throw new ArgumentNullException("callback");

            var serviceConfigurator = new ControlServiceConfigurator<TService>(serviceFactory);

            callback(serviceConfigurator);

            ServiceBuilderFactory serviceBuilderFactory = x =>
                {
                    ConfigurationResult configurationResult =
                        ValidateConfigurationResult.CompileResults(serviceConfigurator.Validate());
                    if (configurationResult.Results.Any())
                        throw new HostConfigurationException("The service was not properly configured");

                    ServiceBuilder serviceBuilder = serviceConfigurator.Build();

                    return serviceBuilder;
                };
            return serviceBuilderFactory;
        }

        public static HostConfigurator Service<T>(this HostConfigurator configurator)
            where T : class, ServiceControl, new()
        {
            return Service(configurator, x => new T(), x => { });
        }

        public static HostConfigurator Service<T>(this HostConfigurator configurator, Func<T> serviceFactory)
            where T : class, ServiceControl
        {
            return Service(configurator, x => serviceFactory(), x => { });
        }

        public static HostConfigurator Service<T>(this HostConfigurator configurator, Func<T> serviceFactory,
            Action<ServiceConfigurator> callback)
            where T : class, ServiceControl
        {
            return Service(configurator, x => serviceFactory(), callback);
        }

        public static HostConfigurator Service<T>(this HostConfigurator configurator,
            Func<HostSettings, T> serviceFactory)
            where T : class, ServiceControl
        {
            return Service(configurator, serviceFactory, x => { });
        }


        public static HostConfigurator Service<TService>(this HostConfigurator configurator,
            Action<ServiceConfigurator<TService>> callback)
            where TService : class
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");
            
            ServiceBuilderFactory serviceBuilderFactory = CreateServiceBuilderFactory(callback);

            configurator.UseServiceBuilder(serviceBuilderFactory);

            return configurator;
        }

        public static ServiceBuilderFactory CreateServiceBuilderFactory<TService>(Action<ServiceConfigurator<TService>> callback)
            where TService : class
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            var serviceConfigurator = new DelegateServiceConfigurator<TService>();

            callback(serviceConfigurator);

            ServiceBuilderFactory serviceBuilderFactory = x =>
                {
                    ConfigurationResult configurationResult =
                        ValidateConfigurationResult.CompileResults(serviceConfigurator.Validate());
                    if (configurationResult.Results.Any())
                        throw new HostConfigurationException("The service was not properly configured");

                    ServiceBuilder serviceBuilder = serviceConfigurator.Build();

                    return serviceBuilder;
                };
            return serviceBuilderFactory;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\ServiceRecoveryConfigurator.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf
{
    internal interface ServiceRecoveryConfigurator
    {
        /// <summary>
        ///   Restart the service after waiting the delay period specified
        /// </summary>
        /// <param name="delayInMinutes"> </param>
        ServiceRecoveryConfigurator RestartService(int delayInMinutes);

        /// <summary>
        ///   Restart the computer after waiting the delay period in minutes
        /// </summary>
        /// <param name="delayInMinutes"> </param>
        /// <param name="message"> </param>
        ServiceRecoveryConfigurator RestartComputer(int delayInMinutes, string message);

        /// <summary>
        ///   Run the command specified
        /// </summary>
        /// <param name="delayInMinutes"> </param>
        /// <param name="command"> </param>
        ServiceRecoveryConfigurator RunProgram(int delayInMinutes, string command);

        /// <summary>
        ///   Specifies the reset period for the restart options
        /// </summary>
        /// <param name="days"> </param>
        ServiceRecoveryConfigurator SetResetPeriod(int days);
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\ServiceRecoveryConfiguratorExtensions.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf
{
    using System;
    using HostConfigurators;

    internal static class ServiceRecoveryConfiguratorExtensions
    {
        public static HostConfigurator EnableServiceRecovery(this HostConfigurator configurator, Action<ServiceRecoveryConfigurator> configureCallback)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");
            if (configureCallback == null)
                throw new ArgumentNullException("configureCallback");

            var recoveryHostConfigurator = new ServiceRecoveryHostConfigurator();

            configureCallback(recoveryHostConfigurator);

            configurator.AddConfigurator(recoveryHostConfigurator);

            return configurator;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\StartModeExtensions.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf
{
    using System;
    using HostConfigurators;
    using Runtime;

    internal static class StartModeExtensions
    {
        public static HostConfigurator StartAutomatically(this HostConfigurator configurator)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.AddConfigurator(new StartModeHostConfigurator(HostStartMode.Automatic));

            return configurator;
        }

        public static HostConfigurator StartAutomaticallyDelayed(this HostConfigurator configurator)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.AddConfigurator(new StartModeHostConfigurator(HostStartMode.AutomaticDelayed));

            return configurator;
        }

        public static HostConfigurator StartManually(this HostConfigurator configurator)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.AddConfigurator(new StartModeHostConfigurator(HostStartMode.Manual));

            return configurator;
        }

        public static HostConfigurator Disabled(this HostConfigurator configurator)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.AddConfigurator(new StartModeHostConfigurator(HostStartMode.Disabled));

            return configurator;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\TestHostExtensions.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf
{
    using Builders;
    using HostConfigurators;

    internal static class TestHostExtensions
    {
        /// <summary>
        /// Configures the test host, which simply starts and stops the service. Meant to be used
        /// to verify the service can be created, started, stopped, and disposed without issues.
        /// </summary>
        public static HostConfigurator UseTestHost(this HostConfigurator configurator)
        {
            configurator.UseHostBuilder((environment, settings) => new TestBuilder(environment, settings));
            configurator.ApplyCommandLine("");

            return configurator;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\UninstallHostConfiguratorExtensions.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf
{
    using System;
    using HostConfigurators;

    internal static class UninstallHostConfiguratorExtensions
    {
        public static HostConfigurator BeforeUninstall(this HostConfigurator configurator, Action callback)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.AddConfigurator(new UninstallHostConfiguratorAction("BeforeUninstall",
                x => x.BeforeUninstall(callback)));

            return configurator;
        }

        public static HostConfigurator AfterUninstall(this HostConfigurator configurator, Action callback)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.AddConfigurator(new UninstallHostConfiguratorAction("AfterUninstall",
                x => x.AfterUninstall(callback)));

            return configurator;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Builders\ControlServiceBuilder.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Builders
{
    using System;
    using Runtime;

    internal class ControlServiceBuilder<T> :
        ServiceBuilder
        where T : class, ServiceControl
    {
        readonly ServiceEvents _serviceEvents;
        readonly Func<HostSettings, T> _serviceFactory;

        public ControlServiceBuilder(Func<HostSettings, T> serviceFactory, ServiceEvents serviceEvents)
        {
            _serviceFactory = serviceFactory;
            _serviceEvents = serviceEvents;
        }

        public ServiceHandle Build(HostSettings settings)
        {
            try
            {
                T service = _serviceFactory(settings);

                return new ControlServiceHandle(service, _serviceEvents);
            }
            catch (Exception ex)
            {
                throw new ServiceBuilderException("An exception occurred creating the service: " + typeof(T).Name, ex);
            }
        }

        class ControlServiceHandle :
            ServiceHandle
        {
            readonly T _service;
            readonly ServiceEvents _serviceEvents;

            public ControlServiceHandle(T service, ServiceEvents serviceEvents)
            {
                _service = service;
                _serviceEvents = serviceEvents;
            }

            public void Dispose()
            {
                var disposable = _service as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }

            public bool Start(HostControl hostControl)
            {
                _serviceEvents.BeforeStart(hostControl);
                bool started = _service.Start(hostControl);
                if (started)
                    _serviceEvents.AfterStart(hostControl);
                return started;
            }

            public bool Stop(HostControl hostControl)
            {
                _serviceEvents.BeforeStop(hostControl);
                bool stopped = _service.Stop(hostControl);
                if (stopped)
                    _serviceEvents.AfterStop(hostControl);
                return stopped;
            }

            public bool Pause(HostControl hostControl)
            {
                var service = _service as ServiceSuspend;

                return service != null && service.Pause(hostControl);
            }

            public bool Continue(HostControl hostControl)
            {
                var service = _service as ServiceSuspend;

                return service != null && service.Continue(hostControl);
            }

            public void Shutdown(HostControl hostControl)
            {
                var serviceShutdown = _service as ServiceShutdown;
                if (serviceShutdown != null)
                {
                    serviceShutdown.Shutdown(hostControl);
                }
            }
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Builders\DelegateServiceBuilder.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Builders
{
    using System;
    using Runtime;

    internal class DelegateServiceBuilder<T> :
        ServiceBuilder
        where T : class
    {
        readonly Func<T, HostControl, bool> _continue;
        readonly Func<T, HostControl, bool> _pause;
        readonly ServiceEvents _serviceEvents;
        readonly ServiceFactory<T> _serviceFactory;
        readonly Action<T, HostControl> _shutdown;
        readonly Func<T, HostControl, bool> _start;
        readonly Func<T, HostControl, bool> _stop;

        public DelegateServiceBuilder(ServiceFactory<T> serviceFactory, Func<T, HostControl, bool> start,
            Func<T, HostControl, bool> stop, Func<T, HostControl, bool> pause, Func<T, HostControl, bool> @continue,
            Action<T, HostControl> shutdown, ServiceEvents serviceEvents)
        {
            _serviceFactory = serviceFactory;
            _start = start;
            _stop = stop;
            _pause = pause;
            _continue = @continue;
            _shutdown = shutdown;
            _serviceEvents = serviceEvents;
        }

        public ServiceHandle Build(HostSettings settings)
        {
            try
            {
                T service = _serviceFactory(settings);

                return new DelegateServiceHandle(service, _start, _stop, _pause, _continue, _shutdown, _serviceEvents);
            }
            catch (Exception ex)
            {
                throw new ServiceBuilderException("An exception occurred creating the service: " + typeof(T).Name, ex);
            }
        }

        class DelegateServiceHandle :
            ServiceHandle
        {
            readonly Func<T, HostControl, bool> _continue;
            readonly Func<T, HostControl, bool> _pause;
            readonly T _service;
            readonly ServiceEvents _serviceEvents;
            readonly Action<T, HostControl> _shutdown;
            readonly Func<T, HostControl, bool> _start;
            readonly Func<T, HostControl, bool> _stop;

            public DelegateServiceHandle(T service, Func<T, HostControl, bool> start, Func<T, HostControl, bool> stop,
                Func<T, HostControl, bool> pause, Func<T, HostControl, bool> @continue, Action<T, HostControl> shutdown,
                ServiceEvents serviceEvents)
            {
                _service = service;
                _start = start;
                _stop = stop;
                _pause = pause;
                _continue = @continue;
                _shutdown = shutdown;
                _serviceEvents = serviceEvents;
            }

            public void Dispose()
            {
                var disposable = _service as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }

            public bool Start(HostControl hostControl)
            {
                _serviceEvents.BeforeStart(hostControl);

                bool started = _start(_service, hostControl);
                if (started)
                {
                    _serviceEvents.AfterStart(hostControl);
                }
                return started;
            }

            public bool Pause(HostControl hostControl)
            {
                return _pause(_service, hostControl);
            }

            public bool Continue(HostControl hostControl)
            {
                return _continue(_service, hostControl);
            }

            public bool Stop(HostControl hostControl)
            {
                _serviceEvents.BeforeStop(hostControl);

                bool stopped = _stop(_service, hostControl);
                if (stopped)
                {
                    _serviceEvents.AfterStop(hostControl);
                }
                return stopped;
            }

            public void Shutdown(HostControl hostControl)
            {
                _shutdown(_service, hostControl);
            }
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Builders\EnvironmentBuilder.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Builders
{
    using Runtime;

    internal interface EnvironmentBuilder
    {
        HostEnvironment Build();
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Builders\HelpBuilder.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Builders
{
    using System;
    using Hosts;
    using Runtime;

    internal class HelpBuilder :
        HostBuilder
    {
        readonly HostEnvironment _environment;
        readonly HostSettings _settings;
        string _prefixText;
        bool _systemHelpTextOnly;

        public HelpBuilder(HostEnvironment environment, HostSettings settings)
        {
            _settings = settings;
            _environment = environment;
        }

        public HostEnvironment Environment
        {
            get { return _environment; }
        }

        public HostSettings Settings
        {
            get { return _settings; }
        }

        public Host Build(ServiceBuilder serviceBuilder)
        {
            string prefixText = _systemHelpTextOnly
                                    ? null
                                    : _prefixText;

            return new HelpHost(prefixText);
        }

        public void Match<T>(Action<T> callback)
            where T : class, HostBuilder
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            var self = this as T;
            if (self != null)
            {
                callback(self);
            }
        }

        public void SetAdditionalHelpText(string prefixText)
        {
            _prefixText = prefixText;
        }

        public void SystemHelpTextOnly()
        {
            _systemHelpTextOnly = true;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Builders\HostBuilder.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Builders
{
    using System;
    using Runtime;

    /// <summary>
    /// Using the service configuration, the host builder will create the host
    /// that will be ran by the service console.
    /// </summary>
    internal interface HostBuilder
    {
        HostEnvironment Environment { get; }
        HostSettings Settings { get; }

        Host Build(ServiceBuilder serviceBuilder);

        void Match<T>(Action<T> callback)
            where T : class, HostBuilder;
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Builders\InstallBuilder.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceProcess;
    using Hosts;
    using Runtime;

    internal class InstallBuilder :
        HostBuilder
    {
        readonly IList<string> _dependencies;
        readonly HostEnvironment _environment;
        readonly IList<Action<InstallHostSettings>> _postActions;
        readonly IList<Action<InstallHostSettings>> _preActions;
        readonly HostSettings _settings;
        Credentials _credentials;
        HostStartMode _startMode;
        bool _sudo;

        public InstallBuilder(HostEnvironment environment, HostSettings settings)
        {
            _preActions = new List<Action<InstallHostSettings>>();
            _postActions = new List<Action<InstallHostSettings>>();
            _dependencies = new List<string>();
            _startMode = HostStartMode.Automatic;
            _credentials = new Credentials("", "", ServiceAccount.LocalSystem);

            _environment = environment;
            _settings = settings;
        }

        public HostEnvironment Environment
        {
            get { return _environment; }
        }

        public HostSettings Settings
        {
            get { return _settings; }
        }

        public Host Build(ServiceBuilder serviceBuilder)
        {
            return new InstallHost(_environment, _settings, _startMode, _dependencies.ToArray(), _credentials,
                _preActions, _postActions, _sudo);
        }

        public void Match<T>(Action<T> callback)
            where T : class, HostBuilder
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            var self = this as T;
            if (self != null)
            {
                callback(self);
            }
        }

        public void RunAs(string username, string password, ServiceAccount accountType)
        {
            _credentials = new Credentials(username, password, accountType);
        }

        public void Sudo()
        {
            _sudo = true;
        }

        public void SetStartMode(HostStartMode startMode)
        {
            _startMode = startMode;
        }

        public void BeforeInstall(Action<InstallHostSettings> callback)
        {
            _preActions.Add(callback);
        }

        public void AfterInstall(Action<InstallHostSettings> callback)
        {
            _postActions.Add(callback);
        }

        public void AddDependency(string name)
        {
            _dependencies.Add(name);
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Builders\RunBuilder.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Builders
{
    using System;
    using Hosts;
    using Logging;
    using Runtime;


    internal class RunBuilder :
        HostBuilder
    {
        static readonly LogWriter _log = HostLogger.Get<RunBuilder>();

        readonly HostSettings _settings;
        readonly HostEnvironment _environment;

        public RunBuilder(HostEnvironment environment, HostSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            _environment = environment;
            _settings = settings;
        }

        public HostEnvironment Environment
        {
            get { return _environment; }
        }

        public HostSettings Settings
        {
            get { return _settings; }
        }

        public virtual Host Build(ServiceBuilder serviceBuilder)
        {
            ServiceHandle serviceHandle = serviceBuilder.Build(_settings);

            return CreateHost(serviceHandle);
        }

        public void Match<T>(Action<T> callback)
            where T : class, HostBuilder
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            var self = this as T;
            if (self != null)
            {
                callback(self);
            }
        }

        Host CreateHost(ServiceHandle serviceHandle)
        {
            if (_environment.IsRunningAsAService)
            {
                _log.Debug("Running as a service, creating service host.");
                return _environment.CreateServiceHost(_settings, serviceHandle);
            }

            _log.Debug("Running as a console application, creating the console host.");
            return new ConsoleRunHost(_settings, _environment, serviceHandle);
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Builders\ServiceBuilder.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Builders
{
    using Runtime;

    internal interface ServiceBuilder
    {
        ServiceHandle Build(HostSettings settings);
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Builders\StartBuilder.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Builders
{
    using System;
    using Hosts;
    using Runtime;

    internal class StartBuilder :
        HostBuilder
    {
        readonly HostBuilder _builder;
        readonly HostEnvironment _environment;
        readonly HostSettings _settings;

        public StartBuilder(HostBuilder builder)
        {
            _builder = GetParentBuilder(builder);
            _settings = builder.Settings;
            _environment = builder.Environment;
        }

        public HostEnvironment Environment
        {
            get { return _environment; }
        }

        public HostSettings Settings
        {
            get { return _settings; }
        }

        public Host Build(ServiceBuilder serviceBuilder)
        {
            if (_builder != null)
            {
                Host parentHost = _builder.Build(serviceBuilder);

                return new StartHost(_environment, _settings, parentHost);
            }

            return new StartHost(_environment, _settings);
        }

        public void Match<T>(Action<T> callback)
            where T : class, HostBuilder
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            var self = this as T;
            if (self != null)
            {
                callback(self);
            }
        }

        static HostBuilder GetParentBuilder(HostBuilder builder)
        {
            HostBuilder result = null;

            builder.Match<InstallBuilder>(x => { result = builder; });

            return result;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Builders\StopBuilder.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Builders
{
    using System;
    using Hosts;
    using Runtime;

    internal class StopBuilder :
        HostBuilder
    {
        readonly HostEnvironment _environment;
        readonly HostSettings _settings;

        public StopBuilder(HostEnvironment environment, HostSettings settings)
        {
            _environment = environment;
            _settings = settings;
        }

        public HostEnvironment Environment
        {
            get { return _environment; }
        }

        public HostSettings Settings
        {
            get { return _settings; }
        }

        public Host Build(ServiceBuilder serviceBuilder)
        {
            return new StopHost(_environment, _settings);
        }

        public void Match<T>(Action<T> callback)
            where T : class, HostBuilder
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            var self = this as T;
            if (self != null)
            {
                callback(self);
            }
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Builders\TestBuilder.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Builders
{
    using System;
    using Hosts;
    using Logging;
    using Runtime;

    internal class TestBuilder :
        HostBuilder
    {
        static readonly LogWriter _log = HostLogger.Get<TestBuilder>();

        readonly HostEnvironment _environment;
        readonly HostSettings _settings;

        public TestBuilder(HostEnvironment environment, HostSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            _environment = environment;
            _settings = settings;
        }

        public HostEnvironment Environment
        {
            get { return _environment; }
        }

        public HostSettings Settings
        {
            get { return _settings; }
        }

        public virtual Host Build(ServiceBuilder serviceBuilder)
        {
            ServiceHandle serviceHandle = serviceBuilder.Build(_settings);

            return CreateHost(serviceHandle);
        }

        public void Match<T>(Action<T> callback)
            where T : class, HostBuilder
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            var self = this as T;
            if (self != null)
            {
                callback(self);
            }
        }

        Host CreateHost(ServiceHandle serviceHandle)
        {
            _log.Debug("Running as a test host.");
            return new TestHost(_settings, _environment, serviceHandle);
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Builders\UninstallBuilder.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Builders
{
    using System;
    using System.Collections.Generic;
    using Hosts;
    using Runtime;

    internal class UninstallBuilder :
        HostBuilder
    {
        readonly HostEnvironment _environment;
        readonly IList<Action> _postActions;
        readonly IList<Action> _preActions;
        readonly HostSettings _settings;
        bool _sudo;

        public UninstallBuilder(HostEnvironment environment, HostSettings settings)
        {
            _preActions = new List<Action>();
            _postActions = new List<Action>();

            _environment = environment;
            _settings = settings;
        }

        public HostEnvironment Environment
        {
            get { return _environment; }
        }

        public HostSettings Settings
        {
            get { return _settings; }
        }

        public Host Build(ServiceBuilder serviceBuilder)
        {
            return new UninstallHost(_environment, _settings, _preActions, _postActions, _sudo);
        }

        public void Match<T>(Action<T> callback)
            where T : class, HostBuilder
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            var self = this as T;
            if (self != null)
            {
                callback(self);
            }
        }

        public void Sudo()
        {
            _sudo = true;
        }

        public void BeforeUninstall(Action callback)
        {
            _preActions.Add(callback);
        }

        public void AfterUninstall(Action callback)
        {
            _postActions.Add(callback);
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\CommandLineParser\AbstractParser.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.CommandLineParser
{
    using System.Linq;

    abstract class AbstractParser<TInput>
    {
        public Parser<TInput, TValue> Succeed<TValue>(TValue value)
        {
            return input => new Result<TInput, TValue>(value, input);
        }

        public Parser<TInput, TValue[]> Rep<TValue>(Parser<TInput, TValue> parser)
        {
            return Rep1(parser).Or(Succeed(new TValue[0]));
        }

        public Parser<TInput, TValue[]> Rep1<TValue>(Parser<TInput, TValue> parser)
        {
            return from x in parser
                   from xs in Rep(parser)
                   select (new[] {x}).Concat(xs).ToArray();
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\CommandLineParser\ArgumentElement.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.CommandLineParser
{
    class ArgumentElement :
        IArgumentElement
    {
        public ArgumentElement(string id)
        {
            Id = id;
        }

        public string Id { get; private set; }

        public override string ToString()
        {
            return "ARGUMENT: " + Id;
        }

        public bool Equals(ArgumentElement other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Equals(other.Id, Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(ArgumentElement))
                return false;
            return Equals((ArgumentElement)obj);
        }

        public override int GetHashCode()
        {
            return (Id != null
                        ? Id.GetHashCode()
                        : 0);
        }

        public static ICommandLineElement New(string id)
        {
            return new ArgumentElement(id);
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\CommandLineParser\CommandLine.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.CommandLineParser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///   Tools for parsing the command line
    /// </summary>
    static class CommandLine
    {
        static readonly StringCommandLineParser _parser = new StringCommandLineParser();

        /// <summary>
        ///   Gets the command line from the Environment.CommandLine, removing the application name if present
        /// </summary>
        /// <returns> The complete, unparsed command line that was specified when the program was executed </returns>
        public static string GetUnparsedCommandLine()
        {
            string line = Environment.CommandLine;

            string applicationPath = Environment.GetCommandLineArgs().First();

            if (line == applicationPath)
                return "";

            if (line.Substring(0, applicationPath.Length) == applicationPath)
                return line.Substring(applicationPath.Length);

            string quotedApplicationPath = "\"" + applicationPath + "\"";

            if (line.Substring(0, quotedApplicationPath.Length) == quotedApplicationPath)
                return line.Substring(quotedApplicationPath.Length);

            return line;
        }

        /// <summary>
        ///   Parses the command line
        /// </summary>
        /// <param name="commandLine"> The command line to parse </param>
        /// <returns> The command line elements that were found </returns>
        static IEnumerable<ICommandLineElement> Parse(string commandLine)
        {
            Result<string, ICommandLineElement> result = _parser.All(commandLine);
            while (result != null)
            {
                yield return result.Value;

                string rest = result.Rest;

                result = _parser.All(rest);
            }
        }

        public static IEnumerable<T> Parse<T>(Action<ICommandLineElementParser<T>> initializer)
        {
            return Parse(initializer, GetUnparsedCommandLine());
        }

        /// <summary>
        ///   Parses the command line and matches any specified patterns
        /// </summary>
        /// <typeparam name="T"> The output type of the parser </typeparam>
        /// <param name="commandLine"> The command line text </param>
        /// <param name="initializer"> Used by the caller to add patterns and object generators </param>
        /// <returns> The elements that were found on the command line </returns>
        public static IEnumerable<T> Parse<T>(Action<ICommandLineElementParser<T>> initializer, string commandLine)
        {
            var elementParser = new CommandLineElementParser<T>();

            initializer(elementParser);

            return elementParser.Parse(Parse(commandLine));
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\CommandLineParser\CommandLineElementParser.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.CommandLineParser
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    class CommandLineElementParser<TResult> :
        AbstractParser<IEnumerable<ICommandLineElement>>,
        ICommandLineElementParser<TResult>
    {
        readonly IList<Parser<IEnumerable<ICommandLineElement>, TResult>> _parsers;

        public CommandLineElementParser()
        {
            _parsers = new List<Parser<IEnumerable<ICommandLineElement>, TResult>>();

            All = from element in _parsers.FirstMatch() select element;
        }

        public Parser<IEnumerable<ICommandLineElement>, ICommandLineElement> AnyElement
        {
            get
            {
                return input => input.Any()
                                    ? new Result<IEnumerable<ICommandLineElement>, ICommandLineElement>(input.First(),
                                          input.Skip(1))
                                    : null;
            }
        }

        public Parser<IEnumerable<ICommandLineElement>, TResult> All { get; set; }

        public void Add(Parser<IEnumerable<ICommandLineElement>, TResult> parser)
        {
            _parsers.Add(parser);
        }

        public Parser<IEnumerable<ICommandLineElement>, IDefinitionElement> Definition()
        {
            return from c in AnyElement
                   where c.GetType() == typeof(DefinitionElement)
                   select (IDefinitionElement)c;
        }

        public Parser<IEnumerable<ICommandLineElement>, IDefinitionElement> Definition(string key)
        {
            return from def in Definition()
                   where def.Key == key
                   select def;
        }

        public Parser<IEnumerable<ICommandLineElement>, IDefinitionElement> Definitions(params string[] keys)
        {
            return from def in Definition()
                   where keys.Contains(def.Key)
                   select def;
        }

        public Parser<IEnumerable<ICommandLineElement>, ISwitchElement> Switch()
        {
            return from c in AnyElement
                   where c.GetType() == typeof(SwitchElement)
                   select (ISwitchElement)c;
        }

        public Parser<IEnumerable<ICommandLineElement>, ISwitchElement> Switch(string key)
        {
            return from sw in Switch()
                   where sw.Key == key
                   select sw;
        }

        public Parser<IEnumerable<ICommandLineElement>, ISwitchElement> Switches(params string[] keys)
        {
            return from sw in Switch()
                   where keys.Contains(sw.Key)
                   select sw;
        }

        public Parser<IEnumerable<ICommandLineElement>, IArgumentElement> Argument()
        {
            return from c in AnyElement
                   where c.GetType() == typeof(ArgumentElement)
                   select (IArgumentElement)c;
        }

        public Parser<IEnumerable<ICommandLineElement>, IArgumentElement> Argument(string value)
        {
            return from arg in Argument()
                   where arg.Id == value
                   select arg;
        }

        public Parser<IEnumerable<ICommandLineElement>, IArgumentElement> Argument(Predicate<IArgumentElement> pred)
        {
            return from arg in Argument()
                   where pred(arg)
                   select arg;
        }

        public Parser<IEnumerable<ICommandLineElement>, IArgumentElement> ValidPath()
        {
            return from c in AnyElement
                   where c.GetType() == typeof(ArgumentElement)
                   where IsValidPath(((ArgumentElement)c).Id)
                   select (IArgumentElement)c;
        }

        public IEnumerable<TResult> Parse(IEnumerable<ICommandLineElement> elements)
        {
            Result<IEnumerable<ICommandLineElement>, TResult> result = All(elements);
            while (result != null)
            {
                yield return result.Value;

                result = All(result.Rest);
            }
        }

        static bool IsValidPath(string path)
        {
            if (!Path.IsPathRooted(path))
                path = Path.Combine(Directory.GetCurrentDirectory(), path);

            string directoryName = Path.GetDirectoryName(path) ?? path;
            if (!Directory.Exists(directoryName))
                return false;

            return true;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\CommandLineParser\DefinitionElement.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.CommandLineParser
{
    class DefinitionElement :
        IDefinitionElement
    {
        public DefinitionElement(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; private set; }
        public string Value { get; private set; }

        public override string ToString()
        {
            return "DEFINE: " + Key + " = " + Value;
        }

        public bool Equals(DefinitionElement other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Equals(other.Key, Key) && Equals(other.Value, Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(DefinitionElement))
                return false;
            return Equals((DefinitionElement)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Key != null
                             ? Key.GetHashCode()
                             : 0)*397) ^ (Value != null
                                              ? Value.GetHashCode()
                                              : 0);
            }
        }

        public static ICommandLineElement New(string key, string value)
        {
            return new DefinitionElement(key, value);
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\CommandLineParser\ExtensionForCommandLineElementParsers.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.CommandLineParser
{
    using System.Collections.Generic;
    using System.Linq;

    static class ExtensionForCommandLineElementParsers
    {
        public static Parser<IEnumerable<ICommandLineElement>, ISwitchElement> Optional(
            this Parser<IEnumerable<ICommandLineElement>, ISwitchElement> source, string key, bool defaultValue)
        {
            return input =>
                {
                    IEnumerable<ICommandLineElement> query = input
                        .Where(x => x.GetType() == typeof(SwitchElement))
                        .Where(x => ((SwitchElement)x).Key == key);

                    if (query.Any())
                        return
                            new Result<IEnumerable<ICommandLineElement>, ISwitchElement>(
                                query.First() as ISwitchElement, input.Except(query));

                    return
                        new Result<IEnumerable<ICommandLineElement>, ISwitchElement>(
                            new SwitchElement(key, defaultValue), input);
                };
        }

        public static Parser<IEnumerable<ICommandLineElement>, IDefinitionElement> Optional(
            this Parser<IEnumerable<ICommandLineElement>, IDefinitionElement> source, string key, string defaultValue)
        {
            return input =>
                {
                    IEnumerable<ICommandLineElement> query = input
                        .Where(x => x.GetType() == typeof(DefinitionElement))
                        .Where(x => ((DefinitionElement)x).Key == key);

                    if (query.Any())
                        return
                            new Result<IEnumerable<ICommandLineElement>, IDefinitionElement>(
                                query.First() as IDefinitionElement, input.Except(query));

                    return
                        new Result<IEnumerable<ICommandLineElement>, IDefinitionElement>(
                            new DefinitionElement(key, defaultValue), input);
                };
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\CommandLineParser\IArgumentElement.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.CommandLineParser
{
    interface IArgumentElement :
        ICommandLineElement
    {
        string Id { get; }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\CommandLineParser\ICommandLineElement.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.CommandLineParser
{
    interface ICommandLineElement
    {
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\CommandLineParser\ICommandLineElementParser.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.CommandLineParser
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///   Used to configure the command line element parser
    /// </summary>
    /// <typeparam name="TResult"> The type of object returned as a result of the parse </typeparam>
    interface ICommandLineElementParser<TResult>
    {
        /// <summary>
        ///   Adds a new pattern to the parser
        /// </summary>
        /// <param name="parser"> The pattern to match and return the resulting object </param>
        void Add(Parser<IEnumerable<ICommandLineElement>, TResult> parser);
        
        Parser<IEnumerable<ICommandLineElement>, IArgumentElement> Argument();
        Parser<IEnumerable<ICommandLineElement>, IArgumentElement> Argument(string value);
        Parser<IEnumerable<ICommandLineElement>, IArgumentElement> Argument(Predicate<IArgumentElement> pred);

        Parser<IEnumerable<ICommandLineElement>, IDefinitionElement> Definition();
        Parser<IEnumerable<ICommandLineElement>, IDefinitionElement> Definition(string key);
        Parser<IEnumerable<ICommandLineElement>, IDefinitionElement> Definitions(params string[] keys);

        Parser<IEnumerable<ICommandLineElement>, ISwitchElement> Switch();
        Parser<IEnumerable<ICommandLineElement>, ISwitchElement> Switch(string key);
        Parser<IEnumerable<ICommandLineElement>, ISwitchElement> Switches(params string[] keys);

        Parser<IEnumerable<ICommandLineElement>, IArgumentElement> ValidPath();
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\CommandLineParser\IDefinitionElement.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.CommandLineParser
{
    interface IDefinitionElement :
        ICommandLineElement
    {
        string Key { get; }
        string Value { get; }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\CommandLineParser\ISwitchElement.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.CommandLineParser
{
    interface ISwitchElement :
        ICommandLineElement
    {
        string Key { get; }
        bool Value { get; }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\CommandLineParser\ITokenElement.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.CommandLineParser
{
    interface ITokenElement :
        ICommandLineElement
    {
        string Token { get; }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\CommandLineParser\MonadParserExtensions.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.CommandLineParser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    static class MonadParserExtensions
    {
        public static Parser<TInput, TValue> Where<TInput, TValue>(this Parser<TInput, TValue> parser,
            Func<TValue, bool> pred)
        {
            return input =>
                {
                    Result<TInput, TValue> result = parser(input);
                    if (result == null || !pred(result.Value))
                        return null;

                    return result;
                };
        }

        public static Parser<TInput, TSelect> Select<TInput, TValue, TSelect>(this Parser<TInput, TValue> parser,
            Func<TValue, TSelect> selector)
        {
            return input =>
                {
                    Result<TInput, TValue> result = parser(input);
                    if (result == null)
                        return null;

                    return new Result<TInput, TSelect>(selector(result.Value), result.Rest);
                };
        }

        public static Parser<TInput, TSelect> SelectMany<TInput, TValue, TIntermediate, TSelect>(
            this Parser<TInput, TValue> parser, Func<TValue, Parser<TInput, TIntermediate>> selector,
            Func<TValue, TIntermediate, TSelect> projector)
        {
            return input =>
                {
                    Result<TInput, TValue> result = parser(input);
                    if (result == null)
                        return null;

                    TValue val = result.Value;
                    Result<TInput, TIntermediate> nextResult = selector(val)(result.Rest);
                    if (nextResult == null)
                        return null;

                    return new Result<TInput, TSelect>(projector(val, nextResult.Value), nextResult.Rest);
                };
        }

        public static Parser<TInput, TValue> Or<TInput, TValue>(this Parser<TInput, TValue> first,
            Parser<TInput, TValue> second)
        {
            return input => first(input) ?? second(input);
        }

        public static Parser<TInput, TValue> FirstMatch<TInput, TValue>(this IEnumerable<Parser<TInput, TValue>> options)
        {
            return input =>
                {
                    return options
                        .Select(option => option(input))
                        .Where(result => result != null)
                        .FirstOrDefault();
                };
        }

        public static Parser<TInput, TSecondValue> And<TInput, TFirstValue, TSecondValue>(
            this Parser<TInput, TFirstValue> first,
            Parser<TInput, TSecondValue> second)
        {
            return input => second(first(input).Rest);
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\CommandLineParser\Parser.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.CommandLineParser
{
    // Reference to further information
    //
    // http://blogs.msdn.com/lukeh/archive/2007/08/19/monadic-parser-combinators-using-c-3-0.aspx

    delegate Result<TInput, TValue> Parser<TInput, TValue>(TInput input);
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\CommandLineParser\Result.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.CommandLineParser
{
    class Result<TInput, TValue>
    {
        public Result(TValue value, TInput rest)
        {
            Value = value;
            Rest = rest;
        }

        public TValue Value { get; private set; }
        public TInput Rest { get; private set; }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\CommandLineParser\StringCommandLineParser.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.CommandLineParser
{
    using System;
    using System.Linq;

    class StringCommandLineParser :
        AbstractParser<string>
    {
        public StringCommandLineParser()
        {
            Whitespace = Rep(Char(' ').Or(Char('\t').Or(Char('\n')).Or(Char('\r'))));
            NewLine = Rep(Char('\r').Or(Char('\n')));

            EscChar = (from bs in Char('\\')
                       from ch in Char('\\').Or(Char('\"')).Or(Char('-')).Or(Char('/')).Or(Char('\''))
                       select ch)
                .Or(from ch in Char(x => x != '"') select ch);

            Id = from w in Whitespace
                 from c in Char(char.IsLetter)
                 from cs in Rep(Char(char.IsLetterOrDigit))
                 select cs.Aggregate(c.ToString(), (s, ch) => s + ch);

            Key = from w in Whitespace
                  from c in Char(char.IsLetter)
                  from cs in Rep(Char(char.IsLetterOrDigit).Or(Char('.')))
                  select cs.Aggregate(c.ToString(), (s, ch) => s + ch);

            Value = (from symbol in Rep(Char(char.IsLetterOrDigit).Or(Char(char.IsPunctuation)))
                     select symbol.Aggregate("", (s, ch) => s + ch));

            Definition = (from w in Whitespace
                          from c in Char('-').Or(Char('/'))
                          from key in Id
                          from eq in Char(':').Or(Char('='))
                          from value in Value
                          select DefinitionElement.New(key, value))
                .Or(from w in Whitespace
                    from c in Char('-').Or(Char('/'))
                    from key in Id
                    from ws in Whitespace
                    from oq in Char('"')
                    from value in Rep(EscChar)
                    from cq in Char('"')
                    select DefinitionElement.New(key, value.Aggregate("", (s, ch) => s + ch)));

            EmptyDefinition = (from w in Whitespace
                               from c in Char('-').Or(Char('/'))
                               from key in Id
                               from ws in Whitespace
                               select DefinitionElement.New(key, ""));

            Argument = from w in Whitespace
                       from c in Char(char.IsLetterOrDigit).Or(Char(char.IsPunctuation))
                       from cs in Rep(Char(char.IsLetterOrDigit).Or(Char(char.IsPunctuation)))
                       select ArgumentElement.New(cs.Aggregate(c.ToString(), (s, ch) => s + ch));

            Switch = (from w in Whitespace
                      from c in Char('-').Or(Char('/'))
                      from arg in Char(char.IsLetterOrDigit)
                      from non in Rep(Char(char.IsLetterOrDigit))
                      where non.Count() == 0
                      select SwitchElement.New(arg))
                .Or(from w in Whitespace
                    from c in Char('-').Or(Char('/'))
                    from arg in Char(char.IsLetterOrDigit)
                    from n in Char('-')
                    select SwitchElement.New(arg, false))
                .Or(from w in Whitespace
                    from c1 in Char('-')
                    from c2 in Char('-')
                    from arg in Id
                    select SwitchElement.New(arg));

            Token = from w in Whitespace
                    from o in Char('[')
                    from t in Key
                    from c in Char(']')
                    select TokenElement.New(t);

            All =
                (from element in Definition select element)
                    .Or(from element in Switch select element)
                    .Or(from element in EmptyDefinition select element)
                    .Or(from element in Token select element)
                    .Or(from element in Argument select element);
        }


        Parser<string, char[]> Whitespace { get; set; }
        Parser<string, char[]> NewLine { get; set; }

        Parser<string, char> EscChar { get; set; }

        Parser<string, string> Id { get; set; }
        Parser<string, string> Key { get; set; }
        Parser<string, string> Value { get; set; }

        Parser<string, ICommandLineElement> Definition { get; set; }
        Parser<string, ICommandLineElement> EmptyDefinition { get; set; }
        Parser<string, ICommandLineElement> Argument { get; set; }
        Parser<string, ICommandLineElement> Token { get; set; }
        Parser<string, ICommandLineElement> Switch { get; set; }
        public Parser<string, ICommandLineElement> All { get; private set; }

        Parser<string, char> AnyChar
        {
            get
            {
                return input => input.Length > 0
                                    ? new Result<string, char>(input[0], input.Substring(1))
                                    : null;
            }
        }

        Parser<string, char> Char(char ch)
        {
            return from c in AnyChar where c == ch select c;
        }

        Parser<string, char> Char(Predicate<char> pred)
        {
            return from c in AnyChar where pred(c) select c;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\CommandLineParser\SwitchElement.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.CommandLineParser
{
    class SwitchElement :
        ISwitchElement
    {
        public SwitchElement(char key)
            : this(key.ToString())
        {
        }

        public SwitchElement(string key)
            : this(key, true)
        {
        }

        public SwitchElement(string key, bool value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; private set; }
        public bool Value { get; private set; }

        public override string ToString()
        {
            return "SWITCH: " + Key + " (" + Value + ")";
        }

        public bool Equals(SwitchElement other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Equals(other.Key, Key) && other.Value.Equals(Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(SwitchElement))
                return false;
            return Equals((SwitchElement)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Key != null
                             ? Key.GetHashCode()
                             : 0)*397) ^ Value.GetHashCode();
            }
        }

        public static ICommandLineElement New(char key)
        {
            return new SwitchElement(key);
        }

        public static ICommandLineElement New(string key)
        {
            return new SwitchElement(key);
        }

        public static ICommandLineElement New(char key, bool value)
        {
            return new SwitchElement(key.ToString(), value);
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\CommandLineParser\TokenElement.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.CommandLineParser
{
    class TokenElement :
        ITokenElement
    {
        public TokenElement(string token)
        {
            Token = token;
        }

        public string Token { get; private set; }

        public override string ToString()
        {
            return "TOKEN: " + Token;
        }

        public bool Equals(TokenElement other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Equals(other.Token, Token);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(TokenElement))
                return false;
            return Equals((TokenElement)obj);
        }

        public override int GetHashCode()
        {
            return (Token != null
                        ? Token.GetHashCode()
                        : 0);
        }

        public static ICommandLineElement New(string token)
        {
            return new TokenElement(token);
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Configurators\ConfigurationResult.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Configurators
{
    using System.Collections.Generic;

    internal interface ConfigurationResult
    {
        IEnumerable<ValidateResult> Results { get; }

        string Message { get; }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Configurators\Configurator.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Configurators
{
    using System.Collections.Generic;

    internal interface Configurator
    {
        IEnumerable<ValidateResult> Validate();
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Configurators\ValidateConfigurationResult.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Configurators
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    [Serializable, DebuggerDisplay("{DebuggerString()}")]
    internal class ValidateConfigurationResult :
        ConfigurationResult
    {
        readonly IList<ValidateResult> _results;

        ValidateConfigurationResult(IEnumerable<ValidateResult> results)
        {
            _results = results.ToList();
        }

        public bool ContainsFailure
        {
            get { return _results.Any(x => x.Disposition == ValidationResultDisposition.Failure); }
        }

        public IEnumerable<ValidateResult> Results
        {
            get { return _results; }
        }

        public string Message
        {
            get
            {
#if NET40
            var debuggerString = string.Join(", ", _results);
#else
                string debuggerString = string.Join(Environment.NewLine, _results.Select(x => x.ToString()).ToArray());
#endif

#if NET40
            return string.IsNullOrWhiteSpace(debuggerString)
#else
                return string.IsNullOrEmpty(debuggerString)
#endif
                           ? ""
                           : debuggerString;
            }
        }

        public static ConfigurationResult CompileResults(IEnumerable<ValidateResult> results)
        {
            var result = new ValidateConfigurationResult(results);

            if (result.ContainsFailure)
            {
                string message = "The service was not properly configured: " 
                    + Environment.NewLine
                    + result.Message;

                throw new HostConfigurationException(message);
            }

            return result;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Configurators\ValidateResult.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Configurators
{
    /// <summary>
    /// Reports information about the configuration before configuring
    /// so that corrections can be made without allocating resources, etc.
    /// </summary>
    internal interface ValidateResult
    {
        /// <summary>
        /// The disposition of the result, any Failure items will prevent
        /// the configuration from completing.
        /// </summary>
        ValidationResultDisposition Disposition { get; }

        /// <summary>
        /// The message associated with the result
        /// </summary>
        string Message { get; }

        /// <summary>
        /// The key associated with the result (chained if configurators are nested)
        /// </summary>
        string Key { get; }

        /// <summary>
        /// The value associated with the result
        /// </summary>
        string Value { get; }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Configurators\ValidateResultImpl.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Configurators
{
    using System;

    [Serializable]
    internal class ValidateResultImpl :
        ValidateResult
    {
        public ValidateResultImpl(ValidationResultDisposition disposition, string key, string value, string message)
        {
            Disposition = disposition;
            Key = key;
            Value = value;
            Message = message;
        }

        public ValidateResultImpl(ValidationResultDisposition disposition, string key, string message)
        {
            Disposition = disposition;
            Key = key;
            Message = message;
        }

        public ValidateResultImpl(ValidationResultDisposition disposition, string message)
        {
            Key = "";
            Disposition = disposition;
            Message = message;
        }

        public ValidationResultDisposition Disposition { get; private set; }
        public string Key { get; private set; }
        public string Value { get; set; }
        public string Message { get; private set; }

        public override string ToString()
        {
            return string.Format("[{0}] {1}", Disposition, string.IsNullOrEmpty(Key)
                                                               ? Message
                                                               : Key + " " + Message);
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Configurators\ValidationResultDisposition.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Configurators
{
    using System;

    [Serializable]
    internal enum ValidationResultDisposition
    {
        Success,
        Warning,
        Failure,
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Configurators\ValidationResultExtensions.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Configurators
{
    internal static class ValidationResultExtensions
    {
        public static ValidateResult Failure(this Configurator configurator, string message)
        {
            return new ValidateResultImpl(ValidationResultDisposition.Failure, message);
        }

        public static ValidateResult Failure(this Configurator configurator, string key, string message)
        {
            return new ValidateResultImpl(ValidationResultDisposition.Failure, key, message);
        }

        public static ValidateResult Failure(this Configurator configurator, string key, string value, string message)
        {
            return new ValidateResultImpl(ValidationResultDisposition.Failure, key, value, message);
        }

        public static ValidateResult Warning(this Configurator configurator, string message)
        {
            return new ValidateResultImpl(ValidationResultDisposition.Warning, message);
        }

        public static ValidateResult Warning(this Configurator configurator, string key, string message)
        {
            return new ValidateResultImpl(ValidationResultDisposition.Warning, key, message);
        }

        public static ValidateResult Warning(this Configurator configurator, string key, string value, string message)
        {
            return new ValidateResultImpl(ValidationResultDisposition.Warning, key, value, message);
        }

        public static ValidateResult Success(this Configurator configurator, string message)
        {
            return new ValidateResultImpl(ValidationResultDisposition.Success, message);
        }

        public static ValidateResult Success(this Configurator configurator, string key, string message)
        {
            return new ValidateResultImpl(ValidationResultDisposition.Success, key, message);
        }

        public static ValidateResult Success(this Configurator configurator, string key, string value, string message)
        {
            return new ValidateResultImpl(ValidationResultDisposition.Success, key, value, message);
        }

        public static ValidateResult WithParentKey(this ValidateResult result, string parentKey)
        {
            string key = parentKey + "." + result.Key;

            return new ValidateResultImpl(result.Disposition, key, result.Value, result.Message);
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Constants\KnownServiceNames.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Constants
{
    /// <summary>
    /// A selection of commonly-used Windows services.
    /// </summary>
    internal static class KnownServiceNames
    {
        /// <summary>
        /// The Microsoft Message Queue service.
        /// </summary>
        public static string Msmq
        {
            get { return "MSMQ"; }
        }

        /// <summary>
        /// The Microsoft SQL Server service.
        /// </summary>
        public static string SqlServer
        {
            get { return "MSSQLSERVER"; }
        }

        /// <summary>
        /// The Internet Information Server service.
        /// </summary>
        public static string IIS
        {
            get { return "W3SVC"; }
        }

        /// <summary>
        /// The Event Log service.
        /// </summary>
        public static string EventLog
        {
            get { return "Eventlog"; }
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\HostConfigurators\CommandLineConfigurator.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.HostConfigurators
{
    using CommandLineParser;
    using Options;

    interface CommandLineConfigurator
    {
        void Configure(ICommandLineElementParser<Option> parser);
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\HostConfigurators\CommandLineDefinitionConfigurator.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.HostConfigurators
{
    using System;
    using CommandLineParser;
    using Options;

    class CommandLineDefinitionConfigurator :
        CommandLineConfigurator
    {
        readonly Action<string> _callback;
        readonly string _name;

        public CommandLineDefinitionConfigurator(string name, Action<string> callback)
        {
            _name = name;
            _callback = callback;
        }

        public void Configure(ICommandLineElementParser<Option> parser)
        {
            parser.Add(from s in parser.Definition(_name)
                       select (Option)new ServiceDefinitionOption(s, _callback));
        }

        class ServiceDefinitionOption :
            Option
        {
            readonly Action<string> _callback;
            readonly string _value;

            public ServiceDefinitionOption(IDefinitionElement element, Action<string> callback)
            {
                _callback = callback;
                _value = element.Value;
            }

            public void ApplyTo(HostConfigurator configurator)
            {
                _callback(_value);
            }
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\HostConfigurators\CommandLineParserOptions.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.HostConfigurators
{
    using CommandLineParser;
    using Options;

    static class CommandLineParserOptions
    {
        internal static void AddTopshelfOptions(ICommandLineElementParser<Option> x)
        {
            x.Add((from arg in x.Argument("install")
                   select (Option)new InstallOption())
                .Or(from arg in x.Argument("uninstall")
                    select (Option)new UninstallOption())
                .Or(from arg in x.Argument("start")
                    select (Option)new StartOption())
                .Or(from arg in x.Argument("help")
                    select (Option)new HelpOption())
                .Or(from arg in x.Argument("stop")
                    select (Option)new StopOption())
                .Or(from arg in x.Switch("sudo")
                    select (Option)new SudoOption())
                .Or(from arg in x.Argument("run")
                    select (Option)new RunOption())
                .Or(from username in x.Definition("username")
                    from password in x.Definition("password")
                    select (Option)new ServiceAccountOption(username.Value, password.Value))
                .Or(from autostart in x.Switch("autostart")
                    select (Option)new AutostartOption())
                .Or(from manual in x.Switch("manual")
                    select (Option)new ManualStartOption())
                .Or(from disabled in x.Switch("disabled")
                    select (Option)new DisabledOption())
                .Or(from delayed in x.Switch("delayed")
                    select (Option)new DelayedOption())
                .Or(from interactive in x.Switch("interactive")
                    select (Option)new InteractiveOption())
                .Or(from autostart in x.Switch("localservice")
                    select (Option)new LocalServiceOption())
                .Or(from autostart in x.Switch("networkservice")
                    select (Option)new NetworkServiceOption())
                .Or(from help in x.Switch("help")
                    select (Option)new HelpOption())
                .Or(from systemHelp in x.Switch("systemonly")
                    select (Option)new SystemOnlyHelpOption())
                .Or(from name in x.Definition("servicename")
                    select (Option)new ServiceNameOption(name.Value))
                .Or(from desc in x.Definition("description")
                    select (Option)new ServiceDescriptionOption(desc.Value))
                .Or(from disp in x.Definition("displayname")
                    select (Option)new DisplayNameOption(disp.Value))
                .Or(from instance in x.Definition("instance")
                    select (Option)new InstanceOption(instance.Value)));
        }

        internal static void AddUnknownOptions(ICommandLineElementParser<Option> x)
        {
            x.Add((from unknown in x.Definition()
                   select (Option)new UnknownOption(unknown.ToString()))
                .Or(from unknown in x.Switch()
                    select (Option)new UnknownOption(unknown.ToString()))
                .Or(from unknown in x.Argument()
                    select (Option)new UnknownOption(unknown.ToString())));
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\HostConfigurators\CommandLineSwitchConfigurator.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.HostConfigurators
{
    using System;
    using CommandLineParser;
    using Options;

    class CommandLineSwitchConfigurator :
        CommandLineConfigurator
    {
        readonly Action<bool> _callback;
        readonly string _name;

        public CommandLineSwitchConfigurator(string name, Action<bool> callback)
        {
            _name = name;
            _callback = callback;
        }

        public void Configure(ICommandLineElementParser<Option> parser)
        {
            parser.Add(from s in parser.Switch(_name)
                       select (Option)new ServiceSwitchOption(s, _callback));
        }

        class ServiceSwitchOption :
            Option
        {
            readonly Action<bool> _callback;
            readonly bool _value;

            public ServiceSwitchOption(ISwitchElement element, Action<bool> callback)
            {
                _callback = callback;
                _value = element.Value;
            }

            public void ApplyTo(HostConfigurator configurator)
            {
                _callback(_value);
            }
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\HostConfigurators\DependencyHostConfigurator.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.HostConfigurators
{
    using System;
    using System.Collections.Generic;
    using Builders;
    using Configurators;

    /// <summary>
    /// Adds a dependency to the InstallBuilder (ignored otherwise)
    /// </summary>
    internal class DependencyHostConfigurator :
        HostBuilderConfigurator
    {
        readonly string _name;

        public DependencyHostConfigurator(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            _name = name;
        }

        public IEnumerable<ValidateResult> Validate()
        {
            if (string.IsNullOrEmpty(_name))
                yield return this.Failure("Dependency", "must not be null");
        }

        public HostBuilder Configure(HostBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");

            builder.Match<InstallBuilder>(x => x.AddDependency(_name));

            return builder;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\HostConfigurators\EnvironmentBuilderFactory.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.HostConfigurators
{
    using Builders;

    internal delegate EnvironmentBuilder EnvironmentBuilderFactory();
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\HostConfigurators\HostBuilderConfigurator.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.HostConfigurators
{
    using Builders;
    using Configurators;

    /// <summary>
    /// Can configure/replace the input HostBuilder, returning the original
    /// or a new HostBuilder
    /// </summary>
    internal interface HostBuilderConfigurator :
        Configurator
    {
        HostBuilder Configure(HostBuilder builder);
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\HostConfigurators\HostBuilderFactory.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.HostConfigurators
{
    using Builders;
    using Runtime;

    internal delegate HostBuilder HostBuilderFactory(HostEnvironment environment, HostSettings settings);
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\HostConfigurators\HostConfigurator.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.HostConfigurators
{
    using System;
    using Configurators;

    internal interface HostConfigurator :
        Configurator
    {
        /// <summary>
        ///   Specifies the name of the service as it should be displayed in the service control manager
        /// </summary>
        /// <param name="name"> </param>
        void SetDisplayName(string name);

        /// <summary>
        ///   Specifies the name of the service as it is registered in the service control manager
        /// </summary>
        /// <param name="name"> </param>
        void SetServiceName(string name);

        /// <summary>
        ///   Specifies the description of the service that is displayed in the service control manager
        /// </summary>
        /// <param name="description"> </param>
        void SetDescription(string description);

        /// <summary>
        ///   Specifies the service instance name that should be used when the service is registered
        /// </summary>
        /// <param name="instanceName"> </param>
        void SetInstanceName(string instanceName);

        /// <summary>
        /// Enable pause and continue support for the service (default is disabled)
        /// </summary>
        void EnablePauseAndContinue();

        /// <summary>
        /// Enable support for service shutdown (signaled by the host OS)
        /// </summary>
        void EnableShutdown();

        /// <summary>
        ///   Specifies the builder factory to use when the service is invoked
        /// </summary>
        /// <param name="hostBuilderFactory"> </param>
        void UseHostBuilder(HostBuilderFactory hostBuilderFactory);

        /// <summary>
        ///   Sets the service builder to use for creating the service
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="serviceBuilderFactory"> </param>
        void UseServiceBuilder(ServiceBuilderFactory serviceBuilderFactory);

        /// <summary>
        ///   Sets the environment builder to use for creating the service (defaults to Windows)
        /// </summary>
        /// <param name="environmentBuilderFactory"> </param>
        void UseEnvironmentBuilder(EnvironmentBuilderFactory environmentBuilderFactory);

        /// <summary>
        ///   Adds a a configurator for the host builder to the configurator
        /// </summary>
        /// <param name="configurator"> </param>
        void AddConfigurator(HostBuilderConfigurator configurator);

        /// <summary>
        /// Parses the command line options and applies them to the host configurator
        /// </summary>
        void ApplyCommandLine();

        /// <summary>
        /// Parses the command line options from the specified command line and applies them to the host configurator
        /// </summary>
        /// <param name="commandLine"></param>
        void ApplyCommandLine(string commandLine);

        /// <summary>
        /// Adds a command line switch (--name) that can be either true or false. Switches are CASE SeNsITiVe
        /// </summary>
        /// <param name="name">The name of the switch, as it will appear on the command line</param>
        void AddCommandLineSwitch(string name, Action<bool> callback);

        /// <summary>
        /// Adds a command line definition (-name:value) that can be specified. the name is case sensitive. If the 
        /// definition 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        void AddCommandLineDefinition(string name, Action<string> callback);
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\HostConfigurators\HostConfiguratorImpl.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.HostConfigurators
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Builders;
    using CommandLineParser;
    using Configurators;
    using Logging;
    using Options;
    using Runtime;
    using Runtime.Windows;

    internal class HostConfiguratorImpl :
        HostConfigurator
    {
        readonly IList<HostBuilderConfigurator> _configurators;
        readonly WindowsHostSettings _settings;
        bool _commandLineApplied;
        EnvironmentBuilderFactory _environmentBuilderFactory;
        HostBuilderFactory _hostBuilderFactory;
        ServiceBuilderFactory _serviceBuilderFactory;
        IList<CommandLineConfigurator> _commandLineOptionConfigurators;

        public HostConfiguratorImpl()
        {
            _configurators = new List<HostBuilderConfigurator>();
            _commandLineOptionConfigurators = new List<CommandLineConfigurator>();
            _settings = new WindowsHostSettings();

            _environmentBuilderFactory = DefaultEnvironmentBuilderFactory;
            _hostBuilderFactory = DefaultHostBuilderFactory;
        }

        public IEnumerable<ValidateResult> Validate()
        {
            if (_hostBuilderFactory == null)
                yield return this.Failure("HostBuilderFactory", "must not be null");

            if (_serviceBuilderFactory == null)
                yield return this.Failure("ServiceBuilderFactory", "must not be null");

            if (_environmentBuilderFactory == null)
                yield return this.Failure("EnvironmentBuilderFactory", "must not be null");

            if (string.IsNullOrEmpty(_settings.DisplayName) && string.IsNullOrEmpty(_settings.Name))
                yield return this.Failure("DisplayName", "must be specified and not empty");

            if (string.IsNullOrEmpty(_settings.Name))
                yield return this.Failure("Name", "must be specified and not empty");
            else
            {
                var disallowed = new[] {' ', '\t', '\r', '\n', '\\', '/'};
                if (_settings.Name.IndexOfAny(disallowed) >= 0)
                    yield return this.Failure("Name", "must not contain whitespace, '/', or '\\' characters");
            }

            foreach (ValidateResult result in _configurators.SelectMany(x => x.Validate()))
            {
                yield return result;
            }

            yield return this.Success("Name", _settings.Name);

            if (_settings.Name != _settings.DisplayName)
                yield return this.Success("DisplayName", _settings.DisplayName);

            if (_settings.Name != _settings.Description)
                yield return this.Success("Description", _settings.Description);

            if (!string.IsNullOrEmpty(_settings.InstanceName))
                yield return this.Success("InstanceName", _settings.InstanceName);

            yield return this.Success("ServiceName", _settings.ServiceName);
        }

        public void SetDisplayName(string name)
        {
            _settings.DisplayName = name;
        }

        public void SetServiceName(string name)
        {
            _settings.Name = name;
        }

        public void SetDescription(string description)
        {
            _settings.Description = description;
        }

        public void SetInstanceName(string instanceName)
        {
            _settings.InstanceName = instanceName;
        }

        public void EnablePauseAndContinue()
        {
            _settings.CanPauseAndContinue = true;
        }

        public void EnableShutdown()
        {
            _settings.CanShutdown = true;
        }

        public void UseHostBuilder(HostBuilderFactory hostBuilderFactory)
        {
            _hostBuilderFactory = hostBuilderFactory;
        }

        public void UseServiceBuilder(ServiceBuilderFactory serviceBuilderFactory)
        {
            _serviceBuilderFactory = serviceBuilderFactory;
        }

        public void UseEnvironmentBuilder(EnvironmentBuilderFactory environmentBuilderFactory)
        {
            _environmentBuilderFactory = environmentBuilderFactory;
        }

        public void AddConfigurator(HostBuilderConfigurator configurator)
        {
            _configurators.Add(configurator);
        }

        public void ApplyCommandLine()
        {
            if (_commandLineApplied)
                return;

            IEnumerable<Option> options = CommandLine.Parse<Option>(ConfigureCommandLineParser);
            ApplyCommandLineOptions(options);
        }

        public void ApplyCommandLine(string commandLine)
        {
            IEnumerable<Option> options = CommandLine.Parse<Option>(ConfigureCommandLineParser, commandLine);
            ApplyCommandLineOptions(options);

            _commandLineApplied = true;
        }

        public void AddCommandLineSwitch(string name, Action<bool> callback)
        {
            var configurator = new CommandLineSwitchConfigurator(name, callback);

            _commandLineOptionConfigurators.Add(configurator);
        }

        public void AddCommandLineDefinition(string name, Action<string> callback)
        {
            var configurator = new CommandLineDefinitionConfigurator(name, callback);

            _commandLineOptionConfigurators.Add(configurator);
        }

        public Host CreateHost()
        {
            Type type = typeof(HostFactory);
            HostLogger.Get<HostConfiguratorImpl>()
                .InfoFormat("{0} v{1}, .NET Framework v{2}", type.Namespace, type.Assembly.GetName().Version,
                    Environment.Version);

            EnvironmentBuilder environmentBuilder = _environmentBuilderFactory();

            HostEnvironment environment = environmentBuilder.Build();

            ServiceBuilder serviceBuilder = _serviceBuilderFactory(_settings);

            HostBuilder builder = _hostBuilderFactory(environment, _settings);

            foreach (HostBuilderConfigurator configurator in _configurators)
            {
                builder = configurator.Configure(builder);
            }

            return builder.Build(serviceBuilder);
        }

        void ApplyCommandLineOptions(IEnumerable<Option> options)
        {
            foreach (Option option in options)
            {
                option.ApplyTo(this);
            }
        }

        void ConfigureCommandLineParser(ICommandLineElementParser<Option> parser)
        {
            CommandLineParserOptions.AddTopshelfOptions(parser);

            foreach (var optionConfigurator in _commandLineOptionConfigurators)
            {
                optionConfigurator.Configure(parser);
            }

            CommandLineParserOptions.AddUnknownOptions(parser);
        }

        static HostBuilder DefaultHostBuilderFactory(HostEnvironment environment, HostSettings settings)
        {
            return new RunBuilder(environment, settings);
        }

        static EnvironmentBuilder DefaultEnvironmentBuilderFactory()
        {
            return new WindowsHostEnvironmentBuilder();
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\HostConfigurators\InstallHostConfiguratorAction.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.HostConfigurators
{
    using System;
    using System.Collections.Generic;
    using Builders;
    using Configurators;

    internal class InstallHostConfiguratorAction :
        HostBuilderConfigurator
    {
        readonly Action<InstallBuilder> _callback;
        readonly string _key;

        public InstallHostConfiguratorAction(string key, Action<InstallBuilder> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            _key = key;
            _callback = callback;
        }

        public IEnumerable<ValidateResult> Validate()
        {
            if (_callback == null)
                yield return this.Failure(_key, "A null callback was specified");
        }

        public HostBuilder Configure(HostBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");

            builder.Match<InstallBuilder>(x => _callback(x));

            return builder;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\HostConfigurators\PrefixHelpTextHostConfigurator.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.HostConfigurators
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Builders;
    using Configurators;

    internal class PrefixHelpTextHostConfigurator :
        HostBuilderConfigurator
    {
        readonly Assembly _assembly;
        readonly string _resourceName;
        string _text;

        public PrefixHelpTextHostConfigurator(Assembly assembly, string resourceName)
        {
            _assembly = assembly;
            _resourceName = resourceName;
        }

        public PrefixHelpTextHostConfigurator(string text)
        {
            _text = text;
        }

        public IEnumerable<ValidateResult> Validate()
        {
            ValidateResult loadResult = null;
            if (_assembly != null)
            {
                if (_resourceName == null)
                    yield return this.Failure("A resource name must be specified");

                try
                {
                    Stream stream = _assembly.GetManifestResourceStream(_resourceName);
                    if (stream == null)
                        loadResult = this.Failure("Resource", "Unable to load resource stream: " + _resourceName);
                    else
                    {
                        using (TextReader reader = new StreamReader(stream))
                        {
                            _text = reader.ReadToEnd();
                        }
                    }
                }
                catch (Exception ex)
                {
                    loadResult = this.Failure("Failed to load help source: " + ex.Message);
                }

                if (loadResult != null)
                    yield return loadResult;
            }

            if (_text == null)
                yield return this.Failure("No additional help text was specified");
        }

        public HostBuilder Configure(HostBuilder builder)
        {
            builder.Match<HelpBuilder>(x => x.SetAdditionalHelpText(_text));

            return builder;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\HostConfigurators\RunAsServiceAccountHostConfigurator.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.HostConfigurators
{
    using System;
    using System.Collections.Generic;
    using System.ServiceProcess;
    using Builders;
    using Configurators;

    internal class RunAsServiceAccountHostConfigurator :
        HostBuilderConfigurator
    {
        readonly ServiceAccount _accountType;

        public RunAsServiceAccountHostConfigurator(ServiceAccount accountType)
        {
            _accountType = accountType;
        }

        public HostBuilder Configure(HostBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");

            builder.Match<InstallBuilder>(x => x.RunAs("", "", _accountType));

            return builder;
        }

        public IEnumerable<ValidateResult> Validate()
        {
            yield break;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\HostConfigurators\RunAsUserHostConfigurator.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.HostConfigurators
{
    using System;
    using System.Collections.Generic;
    using System.ServiceProcess;
    using Builders;
    using Configurators;

    internal class RunAsUserHostConfigurator :
        HostBuilderConfigurator
    {
        readonly string _password;
        readonly string _username;

        public RunAsUserHostConfigurator(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public HostBuilder Configure(HostBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");

            builder.Match<InstallBuilder>(x => x.RunAs(_username, _password, ServiceAccount.User));

            return builder;
        }

        public IEnumerable<ValidateResult> Validate()
        {
            if (string.IsNullOrEmpty(_username))
                yield return this.Failure("Username", "must be specified for a User account type");
            if (string.IsNullOrEmpty(_password))
                yield return this.Failure("Password", "must be specified for a User account type");
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\HostConfigurators\RunHostConfiguratorAction.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.HostConfigurators
{
    using System;
    using System.Collections.Generic;
    using Builders;
    using Configurators;

    internal class RunHostConfiguratorAction :
        HostBuilderConfigurator
    {
        readonly Action<RunBuilder> _callback;
        readonly string _key;

        public RunHostConfiguratorAction(string key, Action<RunBuilder> callback)
        {
            _key = key;
            _callback = callback;
        }

        public HostBuilder Configure(HostBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");

            builder.Match<RunBuilder>(x => _callback(x));

            return builder;
        }

        public IEnumerable<ValidateResult> Validate()
        {
            if (_callback == null)
                yield return this.Failure(_key, "must not be null");
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\HostConfigurators\ServiceBuilderFactory.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.HostConfigurators
{
    using Builders;
    using Runtime;

    internal delegate ServiceBuilder ServiceBuilderFactory(HostSettings settings);
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\HostConfigurators\ServiceRecoveryHostConfigurator.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.HostConfigurators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Builders;
    using Configurators;
    using Runtime;
    using Runtime.Windows;

    internal class ServiceRecoveryHostConfigurator :
        ServiceRecoveryConfigurator,
        HostBuilderConfigurator
    {
        ServiceRecoveryOptions _options;
        HostSettings _settings;

        ServiceRecoveryOptions Options
        {
            get { return _options ?? (_options = new ServiceRecoveryOptions()); }
        }

        public IEnumerable<ValidateResult> Validate()
        {
            if (_options == null)
                yield return this.Failure("No service recovery options were specified");
            else
            {
                if (!_options.Actions.Any())
                    yield return this.Failure("No service recovery actions were specified.");

                if (_options.Actions.Count(x => x.GetType() == typeof(RestartServiceRecoveryAction)) > 1)
                    yield return this.Failure("Only a single restart service action can be specified.");

                if (_options.Actions.Count(x => x.GetType() == typeof(RestartSystemRecoveryAction)) > 1)
                    yield return this.Failure("Only a single restart system action can be specified.");

                if (_options.Actions.Count(x => x.GetType() == typeof(RunProgramRecoveryAction)) > 1)
                    yield return this.Failure("Only a single run program action can be specified.");
            }
        }

        public HostBuilder Configure(HostBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");

            _settings = builder.Settings;

            builder.Match<InstallBuilder>(x => x.AfterInstall(ConfigureServiceRecovery));

            return builder;
        }

        public ServiceRecoveryConfigurator RestartService(int delayInMinutes)
        {
            Options.AddAction(new RestartServiceRecoveryAction(delayInMinutes));

            return this;
        }

        public ServiceRecoveryConfigurator RestartComputer(int delayInMinutes, string message)
        {
            Options.AddAction(new RestartSystemRecoveryAction(delayInMinutes, message));

            return this;
        }

        public ServiceRecoveryConfigurator RunProgram(int delayInMinutes, string command)
        {
            Options.AddAction(new RunProgramRecoveryAction(delayInMinutes, command));

            return this;
        }

        public ServiceRecoveryConfigurator SetResetPeriod(int days)
        {
            Options.ResetPeriod = days;

            return this;
        }

        void ConfigureServiceRecovery(InstallHostSettings installSettings)
        {
            var controller = new WindowsServiceRecoveryController();
            controller.SetServiceRecoveryOptions(_settings, _options);
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\HostConfigurators\StartConfigurator.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.HostConfigurators
{
    using System.Collections.Generic;
    using Builders;
    using Configurators;

    internal class StartConfigurator :
        HostBuilderConfigurator
    {
        public IEnumerable<ValidateResult> Validate()
        {
            yield break;
        }

        public HostBuilder Configure(HostBuilder builder)
        {
            return new StartBuilder(builder);
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\HostConfigurators\StartModeHostConfigurator.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.HostConfigurators
{
    using System;
    using System.Collections.Generic;
    using Builders;
    using Configurators;
    using Runtime;

    internal class StartModeHostConfigurator :
        HostBuilderConfigurator
    {
        readonly HostStartMode _startMode;

        public StartModeHostConfigurator(HostStartMode startMode)
        {
            _startMode = startMode;
        }

        public IEnumerable<ValidateResult> Validate()
        {
#if NET35
            if (_startMode == HostStartMode.AutomaticDelayed)
                yield return this.Failure("StartMode", "Automatic (Delayed) is only available on .NET 4.0 or later");
#endif
            yield break;
        }

        public HostBuilder Configure(HostBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");

            builder.Match<InstallBuilder>(x => x.SetStartMode(_startMode));

            return builder;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\HostConfigurators\SudoConfigurator.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.HostConfigurators
{
    using System;
    using System.Collections.Generic;
    using Builders;
    using Configurators;

    internal class SudoConfigurator :
        HostBuilderConfigurator
    {
        public IEnumerable<ValidateResult> Validate()
        {
            yield break;
        }

        public HostBuilder Configure(HostBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");

            builder.Match<InstallBuilder>(x => x.Sudo());
            builder.Match<UninstallBuilder>(x => x.Sudo());

            return builder;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\HostConfigurators\SystemOnlyHelpHostConfigurator.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.HostConfigurators
{
    using System.Collections.Generic;
    using Builders;
    using Configurators;

    internal class SystemOnlyHelpHostConfigurator :
        HostBuilderConfigurator
    {
        public IEnumerable<ValidateResult> Validate()
        {
            yield break;
        }

        public HostBuilder Configure(HostBuilder builder)
        {
            builder.Match<HelpBuilder>(x => x.SystemHelpTextOnly());

            return builder;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\HostConfigurators\UninstallHostConfiguratorAction.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.HostConfigurators
{
    using System;
    using System.Collections.Generic;
    using Builders;
    using Configurators;

    internal class UninstallHostConfiguratorAction :
        HostBuilderConfigurator
    {
        readonly Action<UninstallBuilder> _callback;
        readonly string _key;

        public UninstallHostConfiguratorAction(string key, Action<UninstallBuilder> callback)
        {
            _key = key;
            _callback = callback;
        }

        public HostBuilder Configure(HostBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");

            builder.Match<UninstallBuilder>(x => _callback(x));

            return builder;
        }

        public IEnumerable<ValidateResult> Validate()
        {
            if (_callback == null)
                yield return this.Failure(_key, "must not be null");
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\HostConfigurators\UnknownCommandLineOptionHostConfigurator.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.HostConfigurators
{
    using System.Collections.Generic;
    using Builders;
    using Configurators;

    internal class UnknownCommandLineOptionHostConfigurator :
        HostBuilderConfigurator
    {
        readonly string _text;

        public UnknownCommandLineOptionHostConfigurator(string text)
        {
            _text = text;
        }

        public IEnumerable<ValidateResult> Validate()
        {
            yield return this.Failure("Command Line", "An unknown command-line option was found: " + _text);
        }

        public HostBuilder Configure(HostBuilder builder)
        {
            return builder;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Options\AutostartOption.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Options
{
    using HostConfigurators;

    internal class AutostartOption :
        Option
    {
        public void ApplyTo(HostConfigurator configurator)
        {
                configurator.StartAutomatically();
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Options\DelayedOption.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Options
{
    using HostConfigurators;

    internal class DelayedOption :
        Option
    {
        public void ApplyTo(HostConfigurator configurator)
        {
            configurator.StartAutomaticallyDelayed();
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Options\DisabledOption.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Options
{
    using HostConfigurators;

    internal class DisabledOption :
        Option
    {
        public void ApplyTo(HostConfigurator configurator)
        {
            configurator.Disabled();
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Options\DisplayNameOption.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Options
{
    using HostConfigurators;

    internal class DisplayNameOption : Option
    {
        string _name;

        public DisplayNameOption(string name)
        {
            _name = name;
        }

        public void ApplyTo(HostConfigurator configurator)
        {
            configurator.SetDisplayName(_name);
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Options\HelpOption.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Options
{
    using System;
    using Builders;
    using HostConfigurators;

    internal class HelpOption :
        Option
    {
        public void ApplyTo(HostConfigurator configurator)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.UseHostBuilder((environment, settings) => new HelpBuilder(environment, settings));
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Options\InstallOption.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Options
{
    using System;
    using Builders;
    using HostConfigurators;

    internal class InstallOption :
        Option
    {
        public void ApplyTo(HostConfigurator configurator)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.UseHostBuilder((environment, settings) => new InstallBuilder(environment, settings));
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Options\InstanceOption.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Options
{
    using System;
    using HostConfigurators;

    internal class InstanceOption :
        Option
    {
        readonly string _instanceName;

        public InstanceOption(string instanceName)
        {
            _instanceName = instanceName;
        }

        public void ApplyTo(HostConfigurator configurator)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.SetInstanceName(_instanceName);
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Options\InteractiveOption.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Options
{
    using HostConfigurators;

    internal class InteractiveOption : Option
    {
        public void ApplyTo(HostConfigurator configurator)
        {
            configurator.RunAsPrompt();
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Options\LocalServiceOption.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Options
{
    using HostConfigurators;

    internal class LocalServiceOption :
        Option
    {
        public void ApplyTo(HostConfigurator configurator)
        {
            configurator.RunAsLocalService();
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Options\ManualStartOption.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Options
{
    using HostConfigurators;

    internal class ManualStartOption :
        Option
    {
        public void ApplyTo(HostConfigurator configurator)
        {
            configurator.StartManually();
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Options\NetworkServiceOption.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Options
{
    using HostConfigurators;

    internal class NetworkServiceOption :
        Option
    {
        public void ApplyTo(HostConfigurator configurator)
        {
            configurator.RunAsNetworkService();
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Options\Option.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Options
{
    using HostConfigurators;

    internal interface Option
    {
        void ApplyTo(HostConfigurator configurator);
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Options\RunOption.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Options
{
    using System;
    using Builders;
    using HostConfigurators;

    internal class RunOption :
        Option
    {
        public void ApplyTo(HostConfigurator configurator)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.UseHostBuilder((environment, settings) => new RunBuilder(environment,settings));
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Options\ServiceAccountOption.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Options
{
    using HostConfigurators;

    internal class ServiceAccountOption :
        Option
    {
        readonly string _password;
        readonly string _username;

        public ServiceAccountOption(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public void ApplyTo(HostConfigurator configurator)
        {
            configurator.RunAs(_username, _password);
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Options\ServiceDescriptionOption.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Options
{
    using HostConfigurators;

    internal class ServiceDescriptionOption : 
        Option
    {
        string _description;

        public ServiceDescriptionOption(string description)
        {
            _description = description;
        }

        public void ApplyTo(HostConfigurator configurator)
        {
            configurator.SetDescription(_description);
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Options\ServiceNameOption.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Options
{
    using HostConfigurators;

    internal class ServiceNameOption :
        Option
    {
        string _serviceName;

        public ServiceNameOption(string name)
        {
            _serviceName = name;
        }

        public void ApplyTo(HostConfigurator configurator)
        {
            configurator.SetServiceName(_serviceName);
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Options\StartOption.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Options
{
    using System;
    using HostConfigurators;

    internal class StartOption :
        Option
    {
        public void ApplyTo(HostConfigurator configurator)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            var startConfigurator = new StartConfigurator();

            configurator.AddConfigurator(startConfigurator);
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Options\StopOption.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Options
{
    using System;
    using Builders;
    using HostConfigurators;

    internal class StopOption :
        Option
    {
        public void ApplyTo(HostConfigurator configurator)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.UseHostBuilder((environment, settings) => new StopBuilder(environment, settings));
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Options\SudoOption.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Options
{
    using System;
    using HostConfigurators;

    internal class SudoOption :
        Option
    {
        public void ApplyTo(HostConfigurator configurator)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.AddConfigurator(new SudoConfigurator());
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Options\SystemOnlyHelpOption.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Options
{
    using HostConfigurators;

    internal class SystemOnlyHelpOption :
        Option
    {
        public void ApplyTo(HostConfigurator configurator)
        {
            configurator.AddConfigurator(new SystemOnlyHelpHostConfigurator());
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Options\UninstallOption.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Options
{
    using System;
    using Builders;
    using HostConfigurators;

    internal class UninstallOption :
        Option
    {
        public void ApplyTo(HostConfigurator configurator)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");

            configurator.UseHostBuilder((environment, settings) => new UninstallBuilder(environment, settings));
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\Options\UnknownOption.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Options
{
    using HostConfigurators;

    internal class UnknownOption :
        Option
    {
        readonly string _text;

        public UnknownOption(string text)
        {
            _text = text;
        }

        public void ApplyTo(HostConfigurator configurator)
        {
            configurator.AddConfigurator(new UnknownCommandLineOptionHostConfigurator(_text));
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\ServiceConfigurators\ControlServiceConfigurator.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.ServiceConfigurators
{
    using System;
    using System.Collections.Generic;
    using Builders;
    using Configurators;
    using HostConfigurators;
    using Runtime;

    internal class ControlServiceConfigurator<T> :
        ServiceConfiguratorBase,
        ServiceConfigurator,
        Configurator
        where T : class, ServiceControl
    {
        readonly Func<HostSettings, T> _serviceFactory;

        public ControlServiceConfigurator(Func<HostSettings, T> serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        public IEnumerable<ValidateResult> Validate()
        {
            yield break;
        }


        public ServiceBuilder Build()
        {
            var serviceBuilder = new ControlServiceBuilder<T>(_serviceFactory, ServiceEvents);
            return serviceBuilder;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\ServiceConfigurators\DelegateServiceConfigurator.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.ServiceConfigurators
{
    using System;
    using System.Collections.Generic;
    using Builders;
    using Configurators;
    using Runtime;

    internal class DelegateServiceConfigurator<T> :
        ServiceConfiguratorBase,
        ServiceConfigurator<T>,
        Configurator
        where T : class
    {
        Func<T, HostControl, bool> _continue;
        ServiceFactory<T> _factory;
        Func<T, HostControl, bool> _pause;
        Func<T, HostControl, bool> _start;
        Func<T, HostControl, bool> _stop;
        Action<T, HostControl> _shutdown;


        bool _canPauseAndContinue;
        bool _canShutdown;

        public bool CanPauseAndContinue
        {
            get { return _canPauseAndContinue; }
        }

        public bool CanShutdown
        {
            get { return _canShutdown; }
        }

        public IEnumerable<ValidateResult> Validate()
        {
            if (_factory == null)
                yield return this.Failure("Factory", "must not be null");

            if (_start == null)
                yield return this.Failure("Start", "must not be null");
            if (_stop == null)
                yield return this.Failure("Stop", "must not be null");
            if (_pause != null && _continue == null)
                yield return this.Failure("Continue", "must not be null if pause is specified");
            if (_pause == null && _continue != null)
                yield return this.Failure("Pause", "must not be null if continue is specified");
        }

        public void ConstructUsing(ServiceFactory<T> serviceFactory)
        {
            _factory = serviceFactory;
        }

        public void WhenStarted(Func<T, HostControl, bool> start)
        {
            _start = start;
        }

        public void WhenStopped(Func<T, HostControl, bool> stop)
        {
            _stop = stop;
        }

        public void WhenPaused(Func<T, HostControl, bool> pause)
        {
            _canPauseAndContinue = true;
            _pause = pause;
        }

        public void WhenContinued(Func<T, HostControl, bool> @continue)
        {
            _canPauseAndContinue = true;
            _continue = @continue;
        }

        public void WhenShutdown(Action<T,HostControl> shutdown)
        {
            _canShutdown = true;
            _shutdown = shutdown;
        }

        public ServiceBuilder Build()
        {
            var serviceBuilder = new DelegateServiceBuilder<T>(_factory, _start, _stop, _pause, _continue, _shutdown, ServiceEvents);
            return serviceBuilder;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\ServiceConfigurators\ServiceConfigurator.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.ServiceConfigurators
{
    using System;
    using Runtime;

    internal interface ServiceConfigurator
    {
        /// <summary>
        /// Registers a callback invoked before the service Start method is called.
        /// </summary>
        void BeforeStartingService(Action<HostStartContext> callback);

        /// <summary>
        /// Registers a callback invoked after the service Start method is called.
        /// </summary>
        void AfterStartingService(Action<HostStartedContext> callback);

        /// <summary>
        /// Registers a callback invoked before the service Stop method is called.
        /// </summary>
        void BeforeStoppingService(Action<HostStopContext> callback);

        /// <summary>
        /// Registers a callback invoked after the service Stop method is called.
        /// </summary>
        void AfterStoppingService(Action<HostStoppedContext> callback);
    }

    internal interface ServiceConfigurator<T> :
        ServiceConfigurator
        where T : class
    {
        void ConstructUsing(ServiceFactory<T> serviceFactory);
        void WhenStarted(Func<T, HostControl, bool> start);
        void WhenStopped(Func<T, HostControl, bool> stop);
        void WhenPaused(Func<T, HostControl, bool> pause);
        void WhenContinued(Func<T, HostControl, bool> @continue);
        void WhenShutdown(Action<T, HostControl> shutdown);
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Configuration\ServiceConfigurators\ServiceConfiguratorBase.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.ServiceConfigurators
{
    using System;
    using Runtime;

    internal abstract class ServiceConfiguratorBase
    {
        protected readonly ServiceEventsImpl ServiceEvents;

        protected ServiceConfiguratorBase()
        {
            ServiceEvents = new ServiceEventsImpl();
        }

        public void BeforeStartingService(Action<HostStartContext> callback)
        {
            ServiceEvents.AddBeforeStart(callback);
        }

        public void AfterStartingService(Action<HostStartedContext> callback)
        {
            ServiceEvents.AddAfterStart(callback);
        }

        public void BeforeStoppingService(Action<HostStopContext> callback)
        {
            ServiceEvents.AddBeforeStop(callback);
        }

        public void AfterStoppingService(Action<HostStoppedContext> callback)
        {
            ServiceEvents.AddAfterStop(callback);
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Exceptions\HostConfigurationException.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class HostConfigurationException :
        TopshelfException
    {
        public HostConfigurationException()
        {
        }

        public HostConfigurationException(string message)
            : base(message)
        {
        }

        public HostConfigurationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected HostConfigurationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Exceptions\ServiceBuilderException.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class ServiceBuilderException :
        TopshelfException
    {
        public ServiceBuilderException()
        {
        }

        public ServiceBuilderException(string message)
            : base(message)
        {
        }

        public ServiceBuilderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ServiceBuilderException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Exceptions\ServiceControlException.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class ServiceControlException :
        TopshelfException
    {
        public ServiceControlException()
        {
        }

        public ServiceControlException(string message)
            : base(message)
        {
        }

        public ServiceControlException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ServiceControlException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ServiceControlException(string format, Type serviceType, string command, Exception innerException)
            : this(FormatMessage(format, serviceType, command), innerException)
        {
            
        }

        static string FormatMessage(string format, Type serviceType, string command)
        {
            return string.Format(format, serviceType, command);
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Exceptions\TopshelfException.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class TopshelfException :
        Exception
    {
        public TopshelfException()
        {
        }

        public TopshelfException(string message)
            : base(message)
        {
        }

        public TopshelfException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected TopshelfException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Hosts\ConsoleRunHost.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Hosts
{
    using System;
    using System.IO;
    using System.Threading;
    using Logging;
    using Runtime;

    internal class ConsoleRunHost :
        Host,
        HostControl
    {
        readonly LogWriter _log = HostLogger.Get<ConsoleRunHost>();
        readonly HostEnvironment _environment;
        readonly ServiceHandle _serviceHandle;
        readonly HostSettings _settings;
        int _deadThread;

        ManualResetEvent _exit;
        volatile bool _hasCancelled;

        public ConsoleRunHost(HostSettings settings, HostEnvironment environment, ServiceHandle serviceHandle)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");
            if (environment == null)
                throw new ArgumentNullException("environment");

            _settings = settings;
            _environment = environment;
            _serviceHandle = serviceHandle;
        }

        public TopshelfExitCode Run()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            AppDomain.CurrentDomain.UnhandledException += CatchUnhandledException;

            if (_environment.IsServiceInstalled(_settings.ServiceName))
            {
                _log.ErrorFormat("The {0} service is installed as a service", _settings.ServiceName);
                return TopshelfExitCode.ServiceAlreadyInstalled;
            }

            try
            {
                _log.Debug("Starting up as a console application");

                _exit = new ManualResetEvent(false);

                Console.CancelKeyPress += HandleCancelKeyPress;

                _serviceHandle.Start(this);

                _log.InfoFormat("The {0} service is now running, press Control+C to exit.", _settings.ServiceName);

                _exit.WaitOne();
            }
            catch (Exception ex)
            {
                _log.Error("An exception occurred", ex);

                return TopshelfExitCode.AbnormalExit;
            }
            finally
            {
                StopService();

                _exit.Close();
                (_exit as IDisposable).Dispose();

                HostLogger.Shutdown();
            }

            return TopshelfExitCode.Ok;
        }

        void HostControl.RequestAdditionalTime(TimeSpan timeRemaining)
        {
            // good for you, maybe we'll use a timer for startup at some point but for debugging
            // it's a pain in the ass
        }

        void HostControl.Stop()
        {
            _log.Info("Service Stop requested, exiting.");
            _exit.Set();
        }

        void HostControl.Restart()
        {
            _log.Info("Service Restart requested, but we don't support that here, so we are exiting.");
            _exit.Set();
        }

        void CatchUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _log.Error("The service threw an unhandled exception", (Exception)e.ExceptionObject);

            if (e.IsTerminating)
            {
                _exit.Set();

                // this is evil, but perhaps a good thing to let us clean up properly.

                int deadThreadId = Interlocked.Increment(ref _deadThread);
                Thread.CurrentThread.IsBackground = true;
                Thread.CurrentThread.Name = "Unhandled Exception " + deadThreadId.ToString();
                while (true)
                    Thread.Sleep(TimeSpan.FromHours(1));
            }
        }

        void StopService()
        {
            try
            {
                if (_hasCancelled == false)
                {
                    _log.InfoFormat("Stopping the {0} service", _settings.ServiceName);

                    _serviceHandle.Stop(this);
                }
            }
            catch (Exception ex)
            {
                _log.Error("The service did not shut down gracefully", ex);
            }
            finally
            {
                _serviceHandle.Dispose();

                _log.InfoFormat("The {0} service has stopped.", _settings.ServiceName);
            }
        }

        void HandleCancelKeyPress(object sender, ConsoleCancelEventArgs consoleCancelEventArgs)
        {
            if (consoleCancelEventArgs.SpecialKey == ConsoleSpecialKey.ControlBreak)
            {
                _log.Error("Control+Break detected, terminating service (not cleanly, use Control+C to exit cleanly)");
                return;
            }

            consoleCancelEventArgs.Cancel = true;

            if (_hasCancelled)
                return;

            _log.Info("Control+C detected, attempting to stop service.");
            if (_serviceHandle.Stop(this))
            {
                _exit.Set();
                _hasCancelled = true;
            }
            else
            {
                _hasCancelled = false;
                _log.Error("The service is not in a state where it can be stopped.");
            }
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Hosts\HelpHost.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Hosts
{
    using System;
    using System.IO;

    /// <summary>
    ///   Displays the Topshelf command line reference
    /// </summary>
    internal class HelpHost :
        Host
    {
        readonly string _prefixText;

        public HelpHost(string prefixText)
        {
            _prefixText = prefixText;
        }

        public string PrefixText
        {
            get { return _prefixText; }
        }

        public TopshelfExitCode Run()
        {
            if (!string.IsNullOrEmpty(_prefixText))
                Console.WriteLine(_prefixText);

            const string helpText = "Topshelf.HelpText.txt";

            Stream stream = typeof(HelpHost).Assembly.GetManifestResourceStream(helpText);
            if (stream == null)
            {
                Console.WriteLine("Unable to load help text");
                return TopshelfExitCode.AbnormalExit;
            }

            using (TextReader reader = new StreamReader(stream))
            {
                string text = reader.ReadToEnd();
                Console.WriteLine(text);
            }

            return TopshelfExitCode.Ok;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Hosts\InstallHost.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Hosts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceProcess;
    using Logging;
    using Runtime;

    internal class InstallHost :
        Host
    {
        static readonly LogWriter _log = HostLogger.Get<InstallHost>();

        readonly HostEnvironment _environment;
        readonly InstallHostSettings _installSettings;
        readonly IEnumerable<Action<InstallHostSettings>> _postActions;
        readonly IEnumerable<Action<InstallHostSettings>> _preActions;
        readonly HostSettings _settings;
        readonly bool _sudo;

        public InstallHost(HostEnvironment environment, HostSettings settings, HostStartMode startMode,
            IEnumerable<string> dependencies,
            Credentials credentials, IEnumerable<Action<InstallHostSettings>> preActions,
            IEnumerable<Action<InstallHostSettings>> postActions, bool sudo)
        {
            _environment = environment;
            _settings = settings;

            _installSettings = new InstallServiceSettingsImpl(settings, credentials, startMode, dependencies.ToArray());

            _preActions = preActions;
            _postActions = postActions;
            _sudo = sudo;
        }

        public InstallHostSettings InstallSettings
        {
            get { return _installSettings; }
        }

        public HostSettings Settings
        {
            get { return _settings; }
        }

        public TopshelfExitCode Run()
        {
            if (_environment.IsServiceInstalled(_settings.ServiceName))
            {
                _log.ErrorFormat("The {0} service is already installed.", _settings.ServiceName);
                return TopshelfExitCode.ServiceAlreadyInstalled;
            }

            if (!_environment.IsAdministrator)
            {
                if (_sudo)
                {
                    if (_environment.RunAsAdministrator())
                        return TopshelfExitCode.Ok;
                }

                _log.ErrorFormat("The {0} service can only be installed as an administrator", _settings.ServiceName);
                return TopshelfExitCode.SudoRequired;
            }

            _log.DebugFormat("Attempting to install '{0}'", _settings.ServiceName);

            _environment.InstallService(_installSettings, ExecutePreActions, ExecutePostActions);

            return TopshelfExitCode.Ok;
        }

        void ExecutePreActions()
        {
            foreach (Action<InstallHostSettings> action in _preActions)
            {
                action(_installSettings);
            }
        }

        void ExecutePostActions()
        {
            foreach (Action<InstallHostSettings> action in _postActions)
            {
                action(_installSettings);
            }
        }

        class InstallServiceSettingsImpl :
            InstallHostSettings
        {
            readonly Credentials _credentials;
            readonly string[] _dependencies;
            readonly HostSettings _settings;
            readonly HostStartMode _startMode;

            public InstallServiceSettingsImpl(HostSettings settings, Credentials credentials, HostStartMode startMode,
                string[] dependencies)
            {
                _credentials = credentials;
                _settings = settings;
                _startMode = startMode;
                _dependencies = dependencies;
            }

            public string Name
            {
                get { return _settings.Name; }
            }

            public string DisplayName
            {
                get { return _settings.DisplayName; }
            }

            public string Description
            {
                get { return _settings.Description; }
            }

            public string InstanceName
            {
                get { return _settings.InstanceName; }
            }

            public string ServiceName
            {
                get { return _settings.ServiceName; }
            }

            public bool CanPauseAndContinue
            {
                get { return _settings.CanPauseAndContinue; }
            }

            public bool CanShutdown
            {
                get { return _settings.CanShutdown; }
            }

            public ServiceAccount Account
            {
                get { return _credentials.Account; }
            }

            public string Username
            {
                get { return _credentials.Username; }
            }

            public string Password
            {
                get { return _credentials.Password; }
            }

            public string[] Dependencies
            {
                get { return _dependencies; }
            }

            public HostStartMode StartMode
            {
                get { return _startMode; }
            }
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Hosts\StartHost.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Hosts
{
    using System;
    using Logging;
    using Runtime;

    internal class StartHost :
        Host
    {
        readonly HostEnvironment _environment;
        readonly LogWriter _log = HostLogger.Get<StartHost>();
        readonly Host _parentHost;
        HostSettings _settings;

        public StartHost(HostEnvironment environment, HostSettings settings, Host parentHost)
        {
            _environment = environment;
            _settings = settings;
            _parentHost = parentHost;
        }

        public StartHost(HostEnvironment environment, HostSettings settings)
            : this(environment, settings, null)
        {
        }

        public TopshelfExitCode Run()
        {
            if (!_environment.IsAdministrator)
            {
                if (!_environment.RunAsAdministrator())
                    _log.ErrorFormat("The {0} service can only be started by an administrator", _settings.ServiceName);

                return TopshelfExitCode.SudoRequired;
            }

            if (_parentHost != null)
                _parentHost.Run();

            if (!_environment.IsServiceInstalled(_settings.ServiceName))
            {
                _log.ErrorFormat("The {0} service is not installed.", _settings.ServiceName);
                return TopshelfExitCode.ServiceNotInstalled;
            }

            _log.DebugFormat("Starting {0}", _settings.ServiceName);

            try
            {
                _environment.StartService(_settings.ServiceName);

                _log.InfoFormat("The {0} service was started.", _settings.ServiceName);
                return TopshelfExitCode.Ok;
            }
            catch (Exception ex)
            {
                _log.Error("The service failed to start.", ex);
                return TopshelfExitCode.StartServiceFailed;
            }
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Hosts\StopHost.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Hosts
{
    using System;
    using Logging;
    using Runtime;

    internal class StopHost :
        Host
    {
        static readonly LogWriter _log = HostLogger.Get<StopHost>();
        readonly HostEnvironment _environment;
        readonly HostSettings _settings;

        public StopHost(HostEnvironment environment, HostSettings settings)
        {
            _environment = environment;
            _settings = settings;
        }

        public TopshelfExitCode Run()
        {
            if (!_environment.IsServiceInstalled(_settings.ServiceName))
            {
                string message = string.Format("The {0} service is not installed.", _settings.ServiceName);
                _log.Error(message);

                return TopshelfExitCode.ServiceNotInstalled;
            }

            if (!_environment.IsAdministrator)
            {
                if (!_environment.RunAsAdministrator())
                    _log.ErrorFormat("The {0} service can only be stopped by an administrator", _settings.ServiceName);

                return TopshelfExitCode.SudoRequired;
            }

            _log.DebugFormat("Stopping {0}", _settings.ServiceName);

            try
            {
                _environment.StopService(_settings.ServiceName);

                _log.InfoFormat("The {0} service was stopped.", _settings.ServiceName);
                return TopshelfExitCode.Ok;
            }
            catch (Exception ex)
            {
                _log.Error("The service failed to stop.", ex);
                return TopshelfExitCode.StopServiceFailed;
            }
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Hosts\TestHost.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Hosts
{
    using System;
    using System.Threading;
    using Logging;
    using Runtime;

    internal class TestHost :
        Host,
        HostControl
    {
        readonly HostEnvironment _environment;
        readonly LogWriter _log = HostLogger.Get<TestHost>();
        readonly ServiceHandle _serviceHandle;
        readonly HostSettings _settings;

        public TestHost(HostSettings settings, HostEnvironment environment, ServiceHandle serviceHandle)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");
            if (environment == null)
                throw new ArgumentNullException("environment");

            _settings = settings;
            _environment = environment;
            _serviceHandle = serviceHandle;
        }

        public TopshelfExitCode Run()
        {
            try
            {
                _log.InfoFormat("The {0} service is being started.", _settings.ServiceName);
                _serviceHandle.Start(this);
                _log.InfoFormat("The {0} service was started.", _settings.ServiceName);

                Thread.Sleep(100);

                _log.InfoFormat("The {0} service is being stopped.", _settings.ServiceName);
                _serviceHandle.Stop(this);
            }
            catch (Exception ex)
            {
                _log.Error("The service did not shut down gracefully", ex);
            }
            finally
            {
                _serviceHandle.Dispose();
                _log.InfoFormat("The {0} service was stopped.", _settings.ServiceName);
            }

            return TopshelfExitCode.Ok;
        }

        void HostControl.RequestAdditionalTime(TimeSpan timeRemaining)
        {
            // good for you, maybe we'll use a timer for startup at some point but for debugging
            // it's a pain in the ass
        }

        void HostControl.Stop()
        {
            _log.Info("Service Stop requested, exiting.");
        }

        void HostControl.Restart()
        {
            _log.Info("Service Restart requested, but we don't support that here, so we are exiting.");
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Hosts\UninstallHost.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Hosts
{
    using System;
    using System.Collections.Generic;
    using Logging;
    using Runtime;

    internal class UninstallHost :
        Host
    {
        static readonly LogWriter _log = HostLogger.Get<UninstallHost>();

        readonly HostEnvironment _environment;
        readonly IEnumerable<Action> _postActions;
        readonly IEnumerable<Action> _preActions;
        readonly HostSettings _settings;
        readonly bool _sudo;


        public UninstallHost(HostEnvironment environment, HostSettings settings, IEnumerable<Action> preActions,
            IEnumerable<Action> postActions,
            bool sudo)
        {
            _environment = environment;
            _settings = settings;
            _preActions = preActions;
            _postActions = postActions;
            _sudo = sudo;
        }

        public TopshelfExitCode Run()
        {
            if (!_environment.IsServiceInstalled(_settings.ServiceName))
            {
                _log.ErrorFormat("The {0} service is not installed.", _settings.ServiceName);
                return TopshelfExitCode.ServiceNotInstalled;
            }

            if (!_environment.IsAdministrator)
            {
                if (_sudo)
                {
                    if (_environment.RunAsAdministrator())
                        return TopshelfExitCode.Ok;
                }

                _log.ErrorFormat("The {0} service can only be uninstalled as an administrator", _settings.ServiceName);
                return TopshelfExitCode.SudoRequired;
            }

            _log.DebugFormat("Uninstalling {0}", _settings.ServiceName);

            _environment.UninstallService(_settings, ExecutePreActions, ExecutePostActions);

            return TopshelfExitCode.Ok;
        }

        void ExecutePreActions()
        {
            foreach (Action action in _preActions)
            {
                action();
            }
        }

        void ExecutePostActions()
        {
            foreach (Action action in _postActions)
            {
                action();
            }
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Algorithms\DependencyGraph.cs
//------------------------------------------------------------------------------
namespace Internals.Algorithms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Implementations;

    class DependencyGraph<T>
    {
        readonly AdjacencyList<T, DependencyGraphNode<T>> _adjacencyList;

        public DependencyGraph()
        {
            _adjacencyList = new AdjacencyList<T, DependencyGraphNode<T>>(DefaultNodeFactory);
        }

        static DependencyGraphNode<T> DefaultNodeFactory(int index, T value)
        {
            return new DependencyGraphNode<T>(index, value);
        }

        public void Add(T source)
        {
            _adjacencyList.GetNode(source);
        }

        public void Add(T source, T target)
        {
            _adjacencyList.AddEdge(source, target, 0);
        }

        public void Add(T source, IEnumerable<T> targets)
        {
            foreach (T target in targets)
            {
                _adjacencyList.AddEdge(source, target, 0);
            }
        }

        public IEnumerable<T> GetItemsInDependencyOrder()
        {
            EnsureGraphIsAcyclic();

            var sort = new TopologicalSort<T, DependencyGraphNode<T>>(_adjacencyList.Clone());

            return sort.Result.Select(x => x.Value);
        }

        public IEnumerable<T> GetItemsInDependencyOrder(T source)
        {
            EnsureGraphIsAcyclic();

            var sort = new TopologicalSort<T, DependencyGraphNode<T>>(_adjacencyList.Clone(), source);

            return sort.Result.Select(x => x.Value);
        }

        void EnsureGraphIsAcyclic()
        {
            var tarjan = new Tarjan<T, DependencyGraphNode<T>>(_adjacencyList);

            if (tarjan.Result.Count == 0)
                return;

            var message = new StringBuilder();
            foreach (var cycle in tarjan.Result)
            {
                message.Append("(");
                for (int i = 0; i < cycle.Count; i++)
                {
                    if (i > 0)
                        message.Append(",");

                    message.Append(cycle[i].Value);
                }
                message.Append(")");
            }

            throw new InvalidOperationException("The dependency graph contains cycles: " + message);
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Algorithms\Implementations\AdjacencyList.cs
//------------------------------------------------------------------------------
namespace Internals.Algorithms.Implementations
{
    using System;
    using System.Collections.Generic;

    class AdjacencyList<T, TNode>
        where TNode : Node<T>
    {
        readonly Func<int, T, TNode> _nodeFactory;
        readonly NodeList<T, TNode> _nodeList;
        IDictionary<TNode, HashSet<Edge<T, TNode>>> _adjacencies;


        public AdjacencyList(Func<int, T, TNode> nodeFactory)
        {
            _nodeFactory = nodeFactory;
            _nodeList = new NodeList<T, TNode>(nodeFactory);
            _adjacencies = new Dictionary<TNode, HashSet<Edge<T, TNode>>>();
        }

        public ICollection<TNode> SourceNodes
        {
            get { return _adjacencies.Keys; }
        }

        public HashSet<Edge<T, TNode>> GetEdges(TNode index)
        {
            HashSet<Edge<T, TNode>> edges;
            if (_adjacencies.TryGetValue(index, out edges))
                return edges;

            return new HashSet<Edge<T, TNode>>();
        }

        public HashSet<Edge<T, TNode>> GetEdges(T index)
        {
            return GetEdges(_nodeList[index]);
        }

        public void AddEdge(T source, T target, int weight)
        {
            TNode sourceNode = _nodeList[source];
            TNode targetNode = _nodeList[target];

            AddEdge(sourceNode, targetNode, weight);
        }

        public void AddEdge(TNode source, TNode target, int weight)
        {
            HashSet<Edge<T, TNode>> edges;
            if (!_adjacencies.TryGetValue(source, out edges))
            {
                edges = new HashSet<Edge<T, TNode>>();
                _adjacencies.Add(source, edges);
            }

            edges.Add(new Edge<T, TNode>(source, target, weight));
        }

        public void ReverseEdge(Edge<T, TNode> edge)
        {
            HashSet<Edge<T, TNode>> edges;
            if (_adjacencies.TryGetValue(edge.Source, out edges))
                edges.Remove(edge);

            AddEdge(edge.Target, edge.Source, edge.Weight);
        }

        public void ReverseList()
        {
            _adjacencies = Reverse()._adjacencies;
        }

        public AdjacencyList<T, TResultNode> Transform<TResultNode>(Func<int, T, TResultNode> nodeFactory)
            where TResultNode : Node<T>
        {
            var result = new AdjacencyList<T, TResultNode>(nodeFactory);

            foreach (var adjacency in _adjacencies.Values)
            {
                foreach (var edge in adjacency)
                    result.AddEdge(edge.Source.Value, edge.Target.Value, edge.Weight);
            }

            return result;
        }

        public AdjacencyList<T, TNode> Clone()
        {
            return Transform(_nodeFactory);
        }

        public AdjacencyList<T, TNode> Reverse()
        {
            var result = new AdjacencyList<T, TNode>(_nodeFactory);
            foreach (var adjacency in _adjacencies.Values)
            {
                foreach (var edge in adjacency)
                    result.AddEdge(edge.Target.Value, edge.Source.Value, edge.Weight);
            }

            return result;
        }

        public TNode GetNode(T key)
        {
            return _nodeList[key];
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Algorithms\Implementations\DependencyGraphNode.cs
//------------------------------------------------------------------------------
namespace Internals.Algorithms.Implementations
{
    using System;

    class DependencyGraphNode<T> :
        Node<T>,
        TopologicalSortNodeProperties,
        TarjanNodeProperties,
        IComparable<DependencyGraphNode<T>>
    {
        public int Index;
        public int LowLink;
        public bool Visited;

        public DependencyGraphNode(int index, T value)
            : base(index, value)
        {
            Visited = false;
            LowLink = -1;
            Index = -1;
        }


        int TarjanNodeProperties.Index
        {
            get { return Index; }
            set { Index = value; }
        }

        int TarjanNodeProperties.LowLink
        {
            get { return LowLink; }
            set { LowLink = value; }
        }

        bool TopologicalSortNodeProperties.Visited
        {
            get { return Visited; }
            set { Visited = value; }
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Algorithms\Implementations\Edge.cs
//------------------------------------------------------------------------------
namespace Internals.Algorithms.Implementations
{
    using System;

    struct Edge<T, TNode> :
        IComparable<Edge<T, TNode>>
        where TNode : Node<T>
    {
        public readonly TNode Source;
        public readonly TNode Target;
        public readonly int Weight;

        public Edge(TNode source, TNode target, int weight)
        {
            Source = source;
            Target = target;
            Weight = weight;
        }

        public int CompareTo(Edge<T, TNode> other)
        {
            return Weight - other.Weight;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Algorithms\Implementations\Node.cs
//------------------------------------------------------------------------------
namespace Internals.Algorithms.Implementations
{
    class Node<T>
    {
        public readonly T Value;
        readonly int _index;

        public Node(int index, T value)
        {
            _index = index;
            Value = value;
        }

        public int CompareTo(DependencyGraphNode<T> other)
        {
            return !Equals(other)
                       ? 0
                       : -1;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            return _index;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Algorithms\Implementations\NodeList.cs
//------------------------------------------------------------------------------
namespace Internals.Algorithms.Implementations
{
    using System;
    using System.Collections.Generic;

    class NodeList<T> :
        NodeList<T, Node<T>>
    {
        public NodeList()
            : base(DefaultNodeFactory)
        {
        }

        public NodeList(int capacity)
            : base(DefaultNodeFactory, capacity)
        {
        }

        static Node<T> DefaultNodeFactory(int index, T value)
        {
            return new Node<T>(index, value);
        }
    }

    /// <summary>
    /// Maintains a list of nodes for a given set of instances of T
    /// </summary>
    /// <typeparam name="T">The type encapsulated in the node</typeparam>
    /// <typeparam name="TNode">The type of node contained in the list</typeparam>
    class NodeList<T, TNode>
        where TNode : Node<T>
    {
        readonly Func<int, T, TNode> _nodeFactory;
        readonly NodeTable<T> _nodeTable;
        readonly IList<TNode> _nodes;

        public NodeList(Func<int, T, TNode> nodeFactory)
        {
            _nodeFactory = nodeFactory;
            _nodes = new List<TNode>();
            _nodeTable = new NodeTable<T>();
        }

        public NodeList(Func<int, T, TNode> nodeFactory, int capacity)
        {
            _nodeFactory = nodeFactory;
            _nodes = new List<TNode>(capacity);
            _nodeTable = new NodeTable<T>(capacity);
        }

        /// <summary>
        /// Retrieves the node for the given key
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The unique node that relates to the specified key</returns>
        public TNode this[T key]
        {
            get { return _nodes[Index(key) - 1]; }
        }

        /// <summary>
        /// Retrieve the index for a given key
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The index</returns>
        public int Index(T key)
        {
            int index = _nodeTable[key];

            if (index <= _nodes.Count)
                return index;

            TNode node = _nodeFactory(index, key);
            _nodes.Add(node);

            return index;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Algorithms\Implementations\NodeTable.cs
//------------------------------------------------------------------------------
namespace Internals.Algorithms.Implementations
{
    using System.Collections.Generic;

    /// <summary>
    /// Maintains an index of nodes so that regular ints can be used to execute algorithms
    /// against objects with int-compare speed vs. .Equals() speed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class NodeTable<T>
    {
        readonly IDictionary<T, int> _nodes;
        int _count;

        public NodeTable()
        {
            _nodes = new Dictionary<T, int>();
        }

        public NodeTable(int capacity)
        {
            _nodes = new Dictionary<T, int>(capacity);
        }

        /// <summary>
        /// Returns the index for the specified key, which can be any type that supports
        /// equality comparison
        /// </summary>
        /// <param name="key">The key to retrieve</param>
        /// <returns>The index that uniquely relates to the specified key</returns>
        public int this[T key]
        {
            get
            {
                int value;
                if (_nodes.TryGetValue(key, out value))
                    return value;

                value = ++_count;
                _nodes.Add(key, value);

                return value;
            }
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Algorithms\Implementations\Tarjan.cs
//------------------------------------------------------------------------------
namespace Internals.Algorithms.Implementations
{
    using System;
    using System.Collections.Generic;

    class Tarjan<T, TNode>
        where TNode : Node<T>, TarjanNodeProperties
    {
        readonly AdjacencyList<T, TNode> _list;
        readonly IList<IList<TNode>> _result;
        readonly Stack<TNode> _stack;
        int _index;

        public Tarjan(AdjacencyList<T, TNode> list)
        {
            _list = list;
            _index = 0;
            _result = new List<IList<TNode>>();
            _stack = new Stack<TNode>();

            foreach (TNode node in _list.SourceNodes)
            {
                if (node.Index != -1)
                    continue;

                Compute(node);
            }
        }

        public IList<IList<TNode>> Result
        {
            get { return _result; }
        }

        void Compute(TNode v)
        {
            v.Index = _index;
            v.LowLink = _index;
            _index++;

            _stack.Push(v);

            foreach (var edge in _list.GetEdges(v))
            {
                TNode n = edge.Target;
                if (n.Index == -1)
                {
                    Compute(n);
                    v.LowLink = Math.Min(v.LowLink, n.LowLink);
                }
                else if (_stack.Contains(n))
                    v.LowLink = Math.Min(v.LowLink, n.Index);
            }

            if (v.LowLink == v.Index)
            {
                TNode n;
                IList<TNode> component = new List<TNode>();
                do
                {
                    n = _stack.Pop();
                    component.Add(n);
                }
                while (!v.Equals(n));

                if (component.Count != 1 || !v.Equals(component[0]))
                    _result.Add(component);
            }
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Algorithms\Implementations\TarjanNodeProperties.cs
//------------------------------------------------------------------------------
namespace Internals.Algorithms.Implementations
{
    interface TarjanNodeProperties
    {
        int Index { get; set; }
        int LowLink { get; set; }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Algorithms\Implementations\TopologicalSort.cs
//------------------------------------------------------------------------------
namespace Internals.Algorithms.Implementations
{
    using System.Collections.Generic;
    using System.Linq;

    class TopologicalSort<T, TNode>
        where TNode : Node<T>, TopologicalSortNodeProperties
    {
        readonly AdjacencyList<T, TNode> _list;
        readonly IList<TNode> _results;
        readonly IEnumerable<TNode> _sourceNodes;

        public TopologicalSort(AdjacencyList<T, TNode> list)
        {
            _list = list;
            _results = new List<TNode>();
            _sourceNodes = _list.SourceNodes;

            Sort();
        }

        public TopologicalSort(AdjacencyList<T, TNode> list, T source)
        {
            _list = list;
            _results = new List<TNode>();

            TNode sourceNode = list.GetNode(source);
            _sourceNodes = Enumerable.Repeat(sourceNode, 1);

            Sort();
        }

        public IEnumerable<TNode> Result
        {
            get { return _results; }
        }

        void Sort()
        {
            foreach (TNode node in _sourceNodes)
            {
                if (!node.Visited)
                    Sort(node);
            }
        }

        void Sort(TNode node)
        {
            node.Visited = true;
            foreach (var edge in _list.GetEdges(node))
            {
                if (!edge.Target.Visited)
                    Sort(edge.Target);
            }

            _results.Add(node);
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Algorithms\Implementations\TopologicalSortNodeProperties.cs
//------------------------------------------------------------------------------
namespace Internals.Algorithms.Implementations
{
    interface TopologicalSortNodeProperties
    {
        bool Visited { get; set; }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Caching\AbstractCacheDecorator.cs
//------------------------------------------------------------------------------
namespace Internals.Caching
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    abstract class AbstractCacheDecorator<TKey, TValue> :
        Cache<TKey, TValue>
    {
        readonly Cache<TKey, TValue> _cache;

        protected AbstractCacheDecorator(Cache<TKey, TValue> cache)
        {
            _cache = cache;
        }

        public virtual IEnumerator<TValue> GetEnumerator()
        {
            return _cache.GetEnumerator();
        }

        public virtual int Count
        {
            get { return _cache.Count; }
        }

        public virtual bool Has(TKey key)
        {
            return _cache.Has(key);
        }

        public virtual bool HasValue(TValue value)
        {
            return _cache.HasValue(value);
        }

        public virtual void Each(Action<TValue> callback)
        {
            _cache.Each(callback);
        }

        public virtual void Each(Action<TKey, TValue> callback)
        {
            _cache.Each(callback);
        }

        public virtual bool Exists(Predicate<TValue> predicate)
        {
            return _cache.Exists(predicate);
        }

        public virtual bool Find(Predicate<TValue> predicate, out TValue result)
        {
            return _cache.Find(predicate, out result);
        }

        public virtual TKey[] GetAllKeys()
        {
            return _cache.GetAllKeys();
        }

        public virtual TValue[] GetAll()
        {
            return _cache.GetAll();
        }

        public virtual MissingValueProvider<TKey, TValue> MissingValueProvider
        {
            set { _cache.MissingValueProvider = value; }
        }

        public virtual CacheItemCallback<TKey, TValue> ValueAddedCallback
        {
            set { _cache.ValueAddedCallback = value; }
        }

        public virtual CacheItemCallback<TKey, TValue> ValueRemovedCallback
        {
            set { _cache.ValueRemovedCallback = value; }
        }

        public virtual CacheItemCallback<TKey, TValue> DuplicateValueAdded
        {
            set { _cache.DuplicateValueAdded = value; }
        }

        public virtual KeySelector<TKey, TValue> KeySelector
        {
            set { _cache.KeySelector = value; }
        }

        public virtual TValue this[TKey key]
        {
            get { return _cache[key]; }
            set { _cache[key] = value; }
        }

        public virtual TValue Get(TKey key)
        {
            return _cache.Get(key);
        }

        public virtual TValue Get(TKey key, MissingValueProvider<TKey, TValue> missingValueProvider)
        {
            return _cache.Get(key, missingValueProvider);
        }

        public TValue GetValue(TKey key, TValue defaultValue)
        {
            return _cache.GetValue(key, defaultValue);
        }

        public TValue GetValue(TKey key, Func<TValue> defaultValueProvider)
        {
            return _cache.GetValue(key, defaultValueProvider);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _cache.TryGetValue(key, out value);
        }

        public virtual void Add(TKey key, TValue value)
        {
            _cache.Add(key, value);
        }

        public virtual void AddValue(TValue value)
        {
            _cache.AddValue(value);
        }

        public virtual void Remove(TKey key)
        {
            _cache.Remove(key);
        }

        public virtual void RemoveValue(TValue value)
        {
            _cache.RemoveValue(value);
        }

        public virtual void Clear()
        {
            _cache.Clear();
        }

        public virtual void Fill(IEnumerable<TValue> values)
        {
            _cache.Fill(values);
        }

        public virtual bool WithValue(TKey key, Action<TValue> callback)
        {
            return _cache.WithValue(key, callback);
        }

        public virtual TResult WithValue<TResult>(TKey key, Func<TValue, TResult> callback,
            TResult defaultValue)
        {
            return _cache.WithValue(key, callback, defaultValue);
        }

        public TResult WithValue<TResult>(TKey key, Func<TValue, TResult> callback, Func<TKey, TResult> defaultValue)
        {
            return _cache.WithValue(key, callback, defaultValue);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Caching\Cache.cs
//------------------------------------------------------------------------------
namespace Internals.Caching
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A cache implementation that extends the capability of most dictionary style classes to
    /// have a more complete set of methods commonly used in a dictionary scenario.
    /// </summary>
    /// <typeparam name="TKey">The key type of the cache</typeparam>
    /// <typeparam name="TValue">The value type of the cache</typeparam>
    interface Cache<TKey, TValue> :
        ReadCache<TKey, TValue>
    {
        /// <summary>
        /// Sets the missing value provider used by the cache to create requested values that do not exist in the cache
        /// </summary>
        MissingValueProvider<TKey, TValue> MissingValueProvider { set; }

        /// <summary>
        /// Sets the callback that is called when a new value is added to the cache
        /// </summary>
        CacheItemCallback<TKey, TValue> ValueAddedCallback { set; }

        /// <summary>
        /// Sets the callback that is called when a value is removed or replaced from the cache
        /// </summary>
        CacheItemCallback<TKey, TValue> ValueRemovedCallback { set; }

        /// <summary>
        /// Sets the callback that is called when a duplicate value is added to the cache
        /// </summary>
        CacheItemCallback<TKey, TValue> DuplicateValueAdded { set; }

        /// <summary>
        /// Specifies a selector that returns the key from a value which is used when a value is added to the cache
        /// </summary>
        KeySelector<TKey, TValue> KeySelector { set; }

        /// <summary>
        /// References a value in the cache, returning a newly created or existing value for the specified key, and
        /// adding a new or replacing an existing value in the cache
        /// </summary>
        /// <param name="key">The key references the value</param>
        /// <returns>The value from the cache</returns>
        TValue this[TKey key] { get; set; }

        /// <summary>
        /// Get the value for the specified key
        /// </summary>
        /// <param name="key">The key referencing the value in the cache</param>
        /// <returns>The matching value if the key exists in the cache, otherwise an exception is thrown</returns>
        TValue Get(TKey key);

        /// <summary>
        /// Get the value for the specified key, overriding the default missing value provider
        /// </summary>
        /// <param name="key">The key referencing the value in the cache</param>
        /// <param name="missingValueProvider">An overloaded missing value provider to create the value if it is not found in the cache</param>
        /// <returns>The matching value if the key exists in the cache, otherwise an exception is thrown</returns>
        TValue Get(TKey key, MissingValueProvider<TKey, TValue> missingValueProvider);

        /// <summary>
        /// Get a value for the specified key, if not found returns the specified default value
        /// </summary>
        /// <param name="key">The key referencing the value in the cache</param>
        /// <param name="defaultValue">The default value to return if the key is not found in the cache</param>
        /// <returns>The matching value if it exists in the cache, otherwise the default value</returns>
        TValue GetValue(TKey key, TValue defaultValue);

        /// <summary>
        /// Get a value for the specified key, if not found returns the specified default value
        /// </summary>
        /// <param name="key">The key referencing the value in the cache</param>
        /// <param name="defaultValueProvider">The default value to return if the key is not found in the cache</param>
        /// <returns>The matching value if it exists in the cache, otherwise the default value</returns>
        TValue GetValue(TKey key, Func<TValue> defaultValueProvider);

        /// <summary>
        /// Gets a value for the specified key if it exists
        /// </summary>
        /// <param name="key">The key referencing the value in the cache</param>
        /// <param name="value">The value if it exists in the cache, otherwise the default value</param>
        /// <returns>True if the item was in the cache, otherwise false</returns>
        bool TryGetValue(TKey key, out TValue value);

        /// <summary>
        /// Adds a value to the cache using the specified key. If the key already exists in the cache, an exception is thrown.
        /// </summary>
        /// <param name="key">The key referencing the value</param>
        /// <param name="value">The value</param>
        void Add(TKey key, TValue value);

        /// <summary>
        /// Adds a value to the cache using the KeySelector to extract the key from the value. If the key already exists
        /// in the cache, an exception is thrown.
        /// </summary>
        /// <param name="value">The value</param>
        void AddValue(TValue value);

        /// <summary>
        /// Remove an existing value from the cache
        /// </summary>
        /// <param name="key">The key referencing the value</param>
        void Remove(TKey key);

        /// <summary>
        /// Remove an existing value from the cache, using the KeySelector to extract the key to find the value
        /// </summary>
        /// <param name="value">The value to remove</param>
        void RemoveValue(TValue value);

        /// <summary>
        /// Removes all items from the cache
        /// </summary>
        void Clear();

        /// <summary>
        /// Fills the cache from a list of values, using the KeySelector to extract the key for each value.
        /// </summary>
        /// <param name="values"></param>
        void Fill(IEnumerable<TValue> values);

        /// <summary>
        /// Calls the callback with the value matching the specified key
        /// </summary>
        /// <param name="key">The key referencing the value</param>
        /// <param name="callback">The callback to call</param>
        /// <returns>True if the value exists and the callback was called</returns>
        bool WithValue(TKey key, Action<TValue> callback);

        /// <summary>
        /// Calls the function with the value matching the specified key, returning the result of that function
        /// </summary>
        /// <typeparam name="TResult">The result type of the function</typeparam>
        /// <param name="key">The key references the value</param>
        /// <param name="callback">The function to call</param>
        /// <param name="defaultValue">The default return value if the item does not exist in the cache</param>
        /// <returns>The return value of the function, or the defaultValue specified if the item does not exist in the cache</returns>
        TResult WithValue<TResult>(TKey key, Func<TValue, TResult> callback, TResult defaultValue);

        /// <summary>
        /// Calls the function with the value matching the specified key, returning the result of that function
        /// </summary>
        /// <typeparam name="TResult">The result type of the function</typeparam>
        /// <param name="key">The key references the value</param>
        /// <param name="callback">The function to call</param>
        /// <param name="defaultValue">The default return value if the item does not exist in the cache</param>
        /// <returns>The return value of the function, or the defaultValue specified if the item does not exist in the cache</returns>
        TResult WithValue<TResult>(TKey key, Func<TValue, TResult> callback, Func<TKey, TResult> defaultValue);
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Caching\CacheItemCallback.cs
//------------------------------------------------------------------------------
namespace Internals.Caching
{
    delegate void CacheItemCallback<TKey, TValue>(TKey key, TValue value);
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Caching\ConcurrentCache.cs
//------------------------------------------------------------------------------
namespace Internals.Caching
{
#if !NET35
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    [Serializable]
    class ConcurrentCache<TKey, TValue> :
        Cache<TKey, TValue>
    {
        readonly ConcurrentDictionary<TKey, TValue> _values;
        CacheItemCallback<TKey, TValue> _duplicateValueAdded = ThrowOnDuplicateValue;

        KeySelector<TKey, TValue> _keySelector = DefaultKeyAccessor;
        MissingValueProvider<TKey, TValue> _missingValueProvider = ThrowOnMissingValue;
        CacheItemCallback<TKey, TValue> _valueAddedCallback = DefaultCacheItemCallback;
        CacheItemCallback<TKey, TValue> _valueRemovedCallback = DefaultCacheItemCallback;

        public ConcurrentCache()
        {
            _values = new ConcurrentDictionary<TKey, TValue>();
        }

        public ConcurrentCache(MissingValueProvider<TKey, TValue> missingValueProvider)
            : this()
        {
            _missingValueProvider = missingValueProvider;
        }

        public ConcurrentCache(IEqualityComparer<TKey> equalityComparer)
        {
            _values = new ConcurrentDictionary<TKey, TValue>(equalityComparer);
        }

        public ConcurrentCache(KeySelector<TKey, TValue> keySelector)
        {
            _values = new ConcurrentDictionary<TKey, TValue>();
            _keySelector = keySelector;
        }

        public ConcurrentCache(KeySelector<TKey, TValue> keySelector, IEnumerable<TValue> values)
            : this(keySelector)
        {
            Fill(values);
        }

        public ConcurrentCache(IEqualityComparer<TKey> equalityComparer,
            MissingValueProvider<TKey, TValue> missingValueProvider)
            : this(equalityComparer)
        {
            _missingValueProvider = missingValueProvider;
        }

        public ConcurrentCache(IEnumerable<KeyValuePair<TKey, TValue>> values)
        {
            _values = new ConcurrentDictionary<TKey, TValue>(values);
        }

        public ConcurrentCache(IDictionary<TKey, TValue> values,
            MissingValueProvider<TKey, TValue> missingValueProvider)
            : this(values)
        {
            _missingValueProvider = missingValueProvider;
        }

        public ConcurrentCache(IEnumerable<KeyValuePair<TKey, TValue>> values, IEqualityComparer<TKey> equalityComparer)
        {
            _values = new ConcurrentDictionary<TKey, TValue>(values, equalityComparer);
        }

        public ConcurrentCache(IDictionary<TKey, TValue> values,
            IEqualityComparer<TKey> equalityComparer,
            MissingValueProvider<TKey, TValue> missingValueProvider)
            : this(values, equalityComparer)
        {
            _missingValueProvider = missingValueProvider;
        }

        public MissingValueProvider<TKey, TValue> MissingValueProvider
        {
            set { _missingValueProvider = value ?? ThrowOnMissingValue; }
        }

        public CacheItemCallback<TKey, TValue> ValueAddedCallback
        {
            set { _valueAddedCallback = value ?? DefaultCacheItemCallback; }
        }

        public CacheItemCallback<TKey, TValue> ValueRemovedCallback
        {
            set { _valueRemovedCallback = value ?? DefaultCacheItemCallback; }
        }

        public CacheItemCallback<TKey, TValue> DuplicateValueAdded
        {
            set { _duplicateValueAdded = value ?? ThrowOnDuplicateValue; }
        }

        public KeySelector<TKey, TValue> KeySelector
        {
            set { _keySelector = value ?? DefaultKeyAccessor; }
        }

        public int Count
        {
            get { return _values.Count; }
        }

        public TValue this[TKey key]
        {
            get { return Get(key); }
            set
            {
                TValue existingValue;
                if (_values.TryGetValue(key, out existingValue))
                {
                    _valueRemovedCallback(key, existingValue);
                    _values[key] = value;
                    _valueAddedCallback(key, value);
                }
                else
                    Add(key, value);
            }
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return _values.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values.Values.GetEnumerator();
        }

        public TValue Get(TKey key)
        {
            return Get(key, _missingValueProvider);
        }

        public TValue Get(TKey key, MissingValueProvider<TKey, TValue> missingValueProvider)
        {
            bool added = false;

            TValue value = _values.GetOrAdd(key, x =>
                {
                    added = true;
                    return missingValueProvider(x);
                });

            if (added)
                _valueAddedCallback(key, value);

            return value;
        }

        public TValue GetValue(TKey key, TValue defaultValue)
        {
            TValue value;
            if (_values.TryGetValue(key, out value))
                return value;

            return defaultValue;
        }

        public TValue GetValue(TKey key, Func<TValue> defaultValueProvider)
        {
            TValue value;
            if (_values.TryGetValue(key, out value))
                return value;

            return defaultValueProvider();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _values.TryGetValue(key, out value);
        }

        public bool Has(TKey key)
        {
            return _values.ContainsKey(key);
        }

        public bool HasValue(TValue value)
        {
            TKey key = _keySelector(value);

            return Has(key);
        }

        public void Add(TKey key, TValue value)
        {
            bool added = _values.TryAdd(key, value);
            if (added)
                _valueAddedCallback(key, value);
            else
                _duplicateValueAdded(key, value);
        }

        public void AddValue(TValue value)
        {
            TKey key = _keySelector(value);
            Add(key, value);
        }

        public void Fill(IEnumerable<TValue> values)
        {
            foreach (TValue value in values)
            {
                TKey key = _keySelector(value);
                Add(key, value);
            }
        }

        public void Each(Action<TValue> callback)
        {
            foreach (var value in _values)
                callback(value.Value);
        }

        public void Each(Action<TKey, TValue> callback)
        {
            foreach (var value in _values)
                callback(value.Key, value.Value);
        }

        public bool Exists(Predicate<TValue> predicate)
        {
            return _values.Any(value => predicate(value.Value));
        }

        public bool Find(Predicate<TValue> predicate, out TValue result)
        {
            foreach (var value in _values.Where(value => predicate(value.Value)))
            {
                result = value.Value;
                return true;
            }

            result = default(TValue);
            return false;
        }

        public TKey[] GetAllKeys()
        {
            return _values.Keys.ToArray();
        }

        public TValue[] GetAll()
        {
            return _values.Values.ToArray();
        }

        public void Remove(TKey key)
        {
            TValue existingValue;
            if (_values.TryRemove(key, out existingValue))
                _valueRemovedCallback(key, existingValue);
        }

        public void RemoveValue(TValue value)
        {
            TKey key = _keySelector(value);

            Remove(key);
        }

        public void Clear()
        {
            _values.Clear();
        }

        public bool WithValue(TKey key, Action<TValue> callback)
        {
            TValue value;
            if (_values.TryGetValue(key, out value))
            {
                callback(value);
                return true;
            }

            return false;
        }

        public TResult WithValue<TResult>(TKey key,Func<TValue, TResult> callback,TResult defaultValue)
        {
            TValue value;
            if (_values.TryGetValue(key, out value))
                return callback(value);

            return defaultValue;
        }

        public TResult WithValue<TResult>(TKey key, Func<TValue, TResult> callback, Func<TKey, TResult> defaultValue)
        {
            TValue value;
            if (_values.TryGetValue(key, out value))
                return callback(value);

            return defaultValue(key);
        }

        static TValue ThrowOnMissingValue(TKey key)
        {
            throw new KeyNotFoundException("The specified element was not found: " + key);
        }

        static void ThrowOnDuplicateValue(TKey key, TValue value)
        {
            throw new ArgumentException(
                string.Format("An item with the same key already exists in the cache: {0}", key), "key");
        }

        static void DefaultCacheItemCallback(TKey key, TValue value)
        {
        }

        static TKey DefaultKeyAccessor(TValue value)
        {
            throw new InvalidOperationException("No default key accessor has been specified");
        }
    }

#endif
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Caching\DictionaryCache.cs
//------------------------------------------------------------------------------
namespace Internals.Caching
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    [Serializable]
    class DictionaryCache<TKey, TValue> :
        Cache<TKey, TValue>
    {
        readonly IDictionary<TKey, TValue> _values;
        CacheItemCallback<TKey, TValue> _duplicateValueAdded;

        KeySelector<TKey, TValue> _keySelector = DefaultKeyAccessor;
        MissingValueProvider<TKey, TValue> _missingValueProvider = ThrowOnMissingValue;
        CacheItemCallback<TKey, TValue> _valueAddedCallback = DefaultCacheItemCallback;
        CacheItemCallback<TKey, TValue> _valueRemovedCallback = DefaultCacheItemCallback;

        public DictionaryCache()
        {
            _values = new Dictionary<TKey, TValue>();
        }

        public DictionaryCache(MissingValueProvider<TKey, TValue> missingValueProvider)
            : this()
        {
            _missingValueProvider = missingValueProvider;
        }

        public DictionaryCache(IEqualityComparer<TKey> equalityComparer)
        {
            _values = new Dictionary<TKey, TValue>(equalityComparer);
        }

        public DictionaryCache(KeySelector<TKey, TValue> keySelector)
        {
            _values = new Dictionary<TKey, TValue>();
            _keySelector = keySelector;
        }

        public DictionaryCache(KeySelector<TKey, TValue> keySelector, IEnumerable<TValue> values)
            : this(keySelector)
        {
            Fill(values);
        }

        public DictionaryCache(IEqualityComparer<TKey> equalityComparer,
            MissingValueProvider<TKey, TValue> missingValueProvider)
            : this(equalityComparer)
        {
            _missingValueProvider = missingValueProvider;
        }

        public DictionaryCache(IDictionary<TKey, TValue> values) :
            this(values, true)
        {
        }

        public DictionaryCache(IDictionary<TKey, TValue> values, bool copy)
        {
            _values = copy
                          ? new Dictionary<TKey, TValue>(values)
                          : values;
        }

        public DictionaryCache(IDictionary<TKey, TValue> values,
            MissingValueProvider<TKey, TValue> missingValueProvider)
            : this(values, true)
        {
            _missingValueProvider = missingValueProvider;
        }

        public DictionaryCache(IDictionary<TKey, TValue> values,
            MissingValueProvider<TKey, TValue> missingValueProvider,
            bool copy)
            : this(values, copy)
        {
            _missingValueProvider = missingValueProvider;
        }

        public DictionaryCache(IDictionary<TKey, TValue> values, IEqualityComparer<TKey> equalityComparer)
        {
            _values = new Dictionary<TKey, TValue>(values, equalityComparer);
        }

        public DictionaryCache(IDictionary<TKey, TValue> values,
            IEqualityComparer<TKey> equalityComparer,
            MissingValueProvider<TKey, TValue> missingValueProvider)
            : this(values, equalityComparer)
        {
            _missingValueProvider = missingValueProvider;
        }

        public MissingValueProvider<TKey, TValue> MissingValueProvider
        {
            set { _missingValueProvider = value ?? ThrowOnMissingValue; }
        }

        public CacheItemCallback<TKey, TValue> ValueAddedCallback
        {
            set { _valueAddedCallback = value ?? DefaultCacheItemCallback; }
        }

        public CacheItemCallback<TKey, TValue> ValueRemovedCallback
        {
            set { _valueRemovedCallback = value ?? DefaultCacheItemCallback; }
        }

        public CacheItemCallback<TKey, TValue> DuplicateValueAdded
        {
            set { _duplicateValueAdded = value ?? ThrowOnDuplicateValue; }
        }

        public KeySelector<TKey, TValue> KeySelector
        {
            set { _keySelector = value ?? DefaultKeyAccessor; }
        }

        public int Count
        {
            get { return _values.Count; }
        }

        public TValue this[TKey key]
        {
            get { return Get(key); }
            set
            {
                TValue existingValue;
                if (_values.TryGetValue(key, out existingValue))
                {
                    _valueRemovedCallback(key, existingValue);
                    _values[key] = value;
                    _valueAddedCallback(key, value);
                }
                else
                    Add(key, value);
            }
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return _values.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values.Values.GetEnumerator();
        }

        public TValue Get(TKey key)
        {
            return Get(key, _missingValueProvider);
        }

        public TValue Get(TKey key, MissingValueProvider<TKey, TValue> missingValueProvider)
        {
            TValue value;
            if (_values.TryGetValue(key, out value))
                return value;

            value = missingValueProvider(key);

            Add(key, value);

            return value;
        }

        public TValue GetValue(TKey key, TValue defaultValue)
        {
            TValue value;
            if (_values.TryGetValue(key, out value))
                return value;

            return defaultValue;
        }

        public TValue GetValue(TKey key, Func<TValue> defaultValueProvider)
        {
            TValue value;
            if (_values.TryGetValue(key, out value))
                return value;

            return defaultValueProvider();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _values.TryGetValue(key, out value);
        }

        public bool Has(TKey key)
        {
            return _values.ContainsKey(key);
        }

        public bool HasValue(TValue value)
        {
            TKey key = _keySelector(value);

            return Has(key);
        }

        public void Add(TKey key, TValue value)
        {
            _values.Add(key, value);
            _valueAddedCallback(key, value);
        }

        public void AddValue(TValue value)
        {
            TKey key = _keySelector(value);
            Add(key, value);
        }

        public void Fill(IEnumerable<TValue> values)
        {
            foreach (TValue value in values)
            {
                TKey key = _keySelector(value);
                Add(key, value);
            }
        }

        public void Each(Action<TValue> callback)
        {
            foreach (var value in _values)
                callback(value.Value);
        }

        public void Each(Action<TKey, TValue> callback)
        {
            foreach (var value in _values)
                callback(value.Key, value.Value);
        }

        public bool Exists(Predicate<TValue> predicate)
        {
            return _values.Any(value => predicate(value.Value));
        }

        public bool Find(Predicate<TValue> predicate, out TValue result)
        {
            foreach (var value in _values.Where(value => predicate(value.Value)))
            {
                result = value.Value;
                return true;
            }

            result = default(TValue);
            return false;
        }

        public TKey[] GetAllKeys()
        {
            return _values.Keys.ToArray();
        }

        public TValue[] GetAll()
        {
            return _values.Values.ToArray();
        }

        public void Remove(TKey key)
        {
            TValue existingValue;
            if (_values.TryGetValue(key, out existingValue))
            {
                _valueRemovedCallback(key, existingValue);
                _values.Remove(key);
            }
        }

        public void RemoveValue(TValue value)
        {
            TKey key = _keySelector(value);

            Remove(key);
        }

        public void Clear()
        {
            _values.Clear();
        }

        public bool WithValue(TKey key, Action<TValue> callback)
        {
            TValue value;
            if (_values.TryGetValue(key, out value))
            {
                callback(value);
                return true;
            }

            return false;
        }

        public TResult WithValue<TResult>(TKey key,
            Func<TValue, TResult> callback,
            TResult defaultValue)
        {
            TValue value;
            if (_values.TryGetValue(key, out value))
                return callback(value);

            return defaultValue;
        }

        public TResult WithValue<TResult>(TKey key, Func<TValue, TResult> callback, Func<TKey, TResult> defaultValue)
        {
            TValue value;
            if (_values.TryGetValue(key, out value))
                return callback(value);

            return defaultValue(key);
        }

        static TValue ThrowOnMissingValue(TKey key)
        {
            throw new KeyNotFoundException("The specified element was not found: " + key);
        }

        static void ThrowOnDuplicateValue(TKey key, TValue value)
        {
            throw new ArgumentException(
                string.Format("An item with the same key already exists in the cache: {0}", key), "key");
        }

        static void DefaultCacheItemCallback(TKey key, TValue value)
        {
        }

        static TKey DefaultKeyAccessor(TValue value)
        {
            throw new InvalidOperationException("No default key accessor has been specified");
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Caching\GenericTypeCache.cs
//------------------------------------------------------------------------------
namespace Internals.Caching
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    class GenericTypeCache<TInterface> :
        Cache<Type, TInterface>
    {
        readonly Cache<Type, TInterface> _cache;
        readonly Type _genericType;

        GenericTypeCache(Type genericType, Cache<Type, TInterface> cache)
        {
            if (!genericType.IsGenericType)
                throw new ArgumentException("The type specified must be a generic type", "genericType");
            if (genericType.GetGenericArguments().Length != 1)
                throw new ArgumentException("The generic type must have a single generic argument");

            _genericType = genericType;
            _cache = cache;
        }

        /// <summary>
        /// Constructs a cache for the specified generic type
        /// </summary>
        /// <param name="genericType">The generic type to close</param>
        public GenericTypeCache(Type genericType)
#if NET35
            : this(genericType, new ReaderWriterLockedCache<Type, TInterface>(
                new DictionaryCache<Type,TInterface>(DefaultMissingValueProvider(genericType))))
#else
            : this(genericType, new ConcurrentCache<Type, TInterface>(DefaultMissingValueProvider(genericType)))
#endif
        {
        }

        /// <summary>
        /// Constructs a cache for the specified generic type.
        /// </summary>
        /// <param name="genericType">The generic type to close</param>
        /// <param name="missingValueProvider">The implementation provider, which must close the generic type with the passed type</param>
        public GenericTypeCache(Type genericType, MissingValueProvider<Type, TInterface> missingValueProvider)
#if NET35
            : this(genericType, new ReaderWriterLockedCache<Type, TInterface>(
                new DictionaryCache<Type,TInterface>(missingValueProvider)))
#else
            : this(genericType, new ConcurrentCache<Type, TInterface>(missingValueProvider))
#endif
        {
        }

        public Type GenericType
        {
            get { return _genericType; }
        }

        public IEnumerator<TInterface> GetEnumerator()
        {
            return _cache.GetEnumerator();
        }

        public int Count
        {
            get { return _cache.Count; }
        }

        public bool Has(Type key)
        {
            return _cache.Has(key);
        }

        public bool HasValue(TInterface value)
        {
            return _cache.HasValue(value);
        }

        public void Each(Action<TInterface> callback)
        {
            _cache.Each(callback);
        }

        public void Each(Action<Type, TInterface> callback)
        {
            _cache.Each(callback);
        }

        public bool Exists(Predicate<TInterface> predicate)
        {
            return _cache.Exists(predicate);
        }

        public bool Find(Predicate<TInterface> predicate, out TInterface result)
        {
            return _cache.Find(predicate, out result);
        }

        public Type[] GetAllKeys()
        {
            return _cache.GetAllKeys();
        }

        public TInterface[] GetAll()
        {
            return _cache.GetAll();
        }

        public MissingValueProvider<Type, TInterface> MissingValueProvider
        {
            set { _cache.MissingValueProvider = value; }
        }

        public CacheItemCallback<Type, TInterface> ValueAddedCallback
        {
            set { _cache.ValueAddedCallback = value; }
        }

        public CacheItemCallback<Type, TInterface> DuplicateValueAdded
        {
            set { _cache.DuplicateValueAdded = value; }
        }

        public CacheItemCallback<Type, TInterface> ValueRemovedCallback
        {
            set { _cache.ValueRemovedCallback = value; }
        }

        public KeySelector<Type, TInterface> KeySelector
        {
            set { _cache.KeySelector = value; }
        }

        public TInterface Get(Type key)
        {
            return _cache.Get(key);
        }

        public TInterface Get(Type key, MissingValueProvider<Type, TInterface> missingValueProvider)
        {
            return _cache.Get(key, missingValueProvider);
        }

        public TInterface GetValue(Type key, TInterface defaultValue)
        {
            return _cache.GetValue(key, defaultValue);
        }

        public TInterface GetValue(Type key, Func<TInterface> defaultValueProvider)
        {
            return _cache.GetValue(key, defaultValueProvider);
        }

        public bool TryGetValue(Type key, out TInterface value)
        {
            return _cache.TryGetValue(key, out value);
        }

        public TInterface this[Type key]
        {
            get { return _cache[key]; }
            set { _cache[key] = value; }
        }

        public void Add(Type key, TInterface value)
        {
            _cache.Add(key, value);
        }

        public void AddValue(TInterface value)
        {
            _cache.AddValue(value);
        }

        public void Remove(Type key)
        {
            _cache.Remove(key);
        }

        public void RemoveValue(TInterface value)
        {
            _cache.RemoveValue(value);
        }

        public void Clear()
        {
            _cache.Clear();
        }

        public void Fill(IEnumerable<TInterface> values)
        {
            _cache.Fill(values);
        }

        public bool WithValue(Type key, Action<TInterface> callback)
        {
            return _cache.WithValue(key, callback);
        }

        public TResult WithValue<TResult>(Type key,
            Func<TInterface, TResult> callback,
            TResult defaultValue)
        {
            return _cache.WithValue(key, callback, defaultValue);
        }

        public TResult WithValue<TResult>(Type key, Func<TInterface, TResult> callback, Func<Type, TResult> defaultValue)
        {
            return _cache.WithValue(key, callback, defaultValue);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        static MissingValueProvider<Type, TInterface> DefaultMissingValueProvider(Type genericType)
        {
            return type =>
                {
                    Type buildType = genericType.MakeGenericType(type);

                    return (TInterface)Activator.CreateInstance(buildType);
                };
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Caching\KeySelector.cs
//------------------------------------------------------------------------------
namespace Internals.Caching
{
    delegate TKey KeySelector<TKey, TValue>(TValue value);
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Caching\MissingValueProvider.cs
//------------------------------------------------------------------------------
namespace Internals.Caching
{
    delegate TValue MissingValueProvider<TKey, TValue>(TKey key);
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Caching\ReadCache.cs
//------------------------------------------------------------------------------
namespace Internals.Caching
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A read-only view of a cache. Methods that are able to modify the cache contents are not
    /// available in this reduced interface. Methods on this interface will NOT invoke a missing
    /// item provider.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    interface ReadCache<TKey, TValue> :
        IEnumerable<TValue>
    {
        /// <summary>
        /// The number of items in the cache
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Checks if the key exists in the cache
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>True if the key exists, otherwise false</returns>
        bool Has(TKey key);

        /// <summary>
        /// Checks if a value exists in the cache
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <returns>True if the value exists, otherwise false</returns>
        bool HasValue(TValue value);

        /// <summary>
        /// Calls the specified callback with each value in the cache
        /// </summary>
        /// <param name="callback">A callback that accepts the value for each item in the cache</param>
        void Each(Action<TValue> callback);

        /// <summary>
        /// Calls the specified callback with each item in the cache
        /// </summary>
        /// <param name="callback">A callback that accepts the key and value for each item in the cache</param>
        void Each(Action<TKey, TValue> callback);

        /// <summary>
        /// Uses a predicate to scan the cache for a matching value
        /// </summary>
        /// <param name="predicate">The predicate to run against each value</param>
        /// <returns>True if a matching value exists, otherwise false</returns>
        bool Exists(Predicate<TValue> predicate);

        /// <summary>
        /// Uses a predicate to scan the cache for a matching value
        /// </summary>
        /// <param name="predicate">The predicate to run against each value</param>
        /// <param name="result">The matching value</param>
        /// <returns>True if a matching value was found, otherwise false</returns>
        bool Find(Predicate<TValue> predicate, out TValue result);

        /// <summary>
        /// Gets all keys that are stored in the cache
        /// </summary>
        /// <returns>An array of every key in the dictionary</returns>
        TKey[] GetAllKeys();

        /// <summary>
        /// Gets all values that are stored in the cache
        /// </summary>
        /// <returns>An array of every value in the dictionary</returns>
        TValue[] GetAll();
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Caching\ReaderWriterLockedCache.cs
//------------------------------------------------------------------------------
namespace Internals.Caching
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    class ReaderWriterLockedCache<TKey, TValue> :
        Cache<TKey, TValue>,
        IDisposable
    {
        readonly Cache<TKey, TValue> _cache;
        readonly ReaderWriterLockSlim _lock;
        bool _disposed;

        public ReaderWriterLockedCache(Cache<TKey, TValue> cache)
        {
            _cache = cache;
            _lock = new ReaderWriterLockSlim();
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            _lock.EnterReadLock();
            try
            {
                return _cache.ToList().GetEnumerator();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _cache.Count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public bool Has(TKey key)
        {
            _lock.EnterReadLock();
            try
            {
                return _cache.Has(key);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool HasValue(TValue value)
        {
            _lock.EnterReadLock();
            try
            {
                return _cache.HasValue(value);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Each(Action<TValue> callback)
        {
            _lock.EnterReadLock();
            try
            {
                _cache.Each(callback);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Each(Action<TKey, TValue> callback)
        {
            _lock.EnterReadLock();
            try
            {
                _cache.Each(callback);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool Exists(Predicate<TValue> predicate)
        {
            _lock.EnterReadLock();
            try
            {
                return _cache.Exists(predicate);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool Find(Predicate<TValue> predicate, out TValue result)
        {
            _lock.EnterReadLock();
            try
            {
                return _cache.Find(predicate, out result);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public TKey[] GetAllKeys()
        {
            _lock.EnterReadLock();
            try
            {
                return _cache.GetAllKeys();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public TValue[] GetAll()
        {
            _lock.EnterReadLock();
            try
            {
                return _cache.GetAll();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public MissingValueProvider<TKey, TValue> MissingValueProvider
        {
            set { _cache.MissingValueProvider = value; }
        }

        public CacheItemCallback<TKey, TValue> ValueAddedCallback
        {
            set { _cache.ValueAddedCallback = value; }
        }

        public CacheItemCallback<TKey, TValue> ValueRemovedCallback
        {
            set { _cache.ValueRemovedCallback = value; }
        }

        public CacheItemCallback<TKey, TValue> DuplicateValueAdded
        {
            set { _cache.DuplicateValueAdded = value; }
        }

        public KeySelector<TKey, TValue> KeySelector
        {
            set { _cache.KeySelector = value; }
        }

        public TValue Get(TKey key)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (_cache.Has(key))
                    return _cache.Get(key);

                _lock.EnterWriteLock();
                try
                {
                    return _cache.Get(key);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public TValue Get(TKey key, MissingValueProvider<TKey, TValue> missingValueProvider)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (_cache.Has(key))
                    return _cache.Get(key, missingValueProvider);

                _lock.EnterWriteLock();
                try
                {
                    return _cache.Get(key, missingValueProvider);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public TValue GetValue(TKey key, TValue defaultValue)
        {
            _lock.EnterReadLock();
            try
            {
                return _cache.GetValue(key, defaultValue);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public TValue GetValue(TKey key, Func<TValue> defaultValueProvider)
        {
            _lock.EnterReadLock();
            try
            {
                return _cache.GetValue(key, defaultValueProvider);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            _lock.EnterReadLock();
            try
            {
                return _cache.TryGetValue(key, out value);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public TValue this[TKey key]
        {
            get { return Get(key); }
            set
            {
                _lock.EnterWriteLock();
                try
                {
                    _cache[key] = value;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        public void Add(TKey key, TValue value)
        {
            _lock.EnterWriteLock();
            try
            {
                _cache.Add(key, value);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void AddValue(TValue value)
        {
            _lock.EnterWriteLock();
            try
            {
                _cache.AddValue(value);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Remove(TKey key)
        {
            _lock.EnterWriteLock();
            try
            {
                _cache.Remove(key);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void RemoveValue(TValue value)
        {
            _lock.EnterWriteLock();
            try
            {
                _cache.RemoveValue(value);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _cache.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Fill(IEnumerable<TValue> values)
        {
            _lock.EnterWriteLock();
            try
            {
                _cache.Fill(values);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool WithValue(TKey key, Action<TValue> callback)
        {
            _lock.EnterReadLock();
            try
            {
                return _cache.WithValue(key, callback);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public TResult WithValue<TResult>(TKey key,
            Func<TValue, TResult> callback,
            TResult defaultValue)
        {
            _lock.EnterReadLock();
            try
            {
                return _cache.WithValue(key, callback, defaultValue);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public TResult WithValue<TResult>(TKey key, Func<TValue, TResult> callback, Func<TKey, TResult> defaultValue)
        {
            _lock.EnterReadLock();
            try
            {
                return _cache.WithValue(key, callback, defaultValue);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ReaderWriterLockedCache()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {
                _lock.Dispose();
            }

            _disposed = true;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Extensions\CastExtensions.cs
//------------------------------------------------------------------------------
namespace Internals.Extensions
{
    using System;

    static class CastExtensions
    {
        public static T CastAs<T>(this object obj)
            where T : class
        {
            var self = obj as T;
            if (self == null)
            {
                string message = string.Format("Failed to cast {0} to {1}",
                    obj.GetType().FullName, typeof(T).FullName);
                throw new InvalidOperationException(message);
            }

            return self;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Extensions\ExpressionExtensions.cs
//------------------------------------------------------------------------------
namespace Internals.Extensions
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    static class ExpressionExtensions
    {
        /// <summary>
        /// Gets the name of the member specified
        /// </summary>
        /// <typeparam name="T">The type referenced</typeparam>
        /// <typeparam name="TMember">The type of the member referenced</typeparam>
        /// <param name="expression">The expression referencing the member</param>
        /// <returns>The name of the member referenced by the expression</returns>
        public static string GetMemberName<T, TMember>(this Expression<Func<T, TMember>> expression)
        {
            return expression.GetMemberExpression().Member.Name;
        }

        /// <summary>
        /// Gets the name of the member specified
        /// </summary>
        /// <typeparam name="T">The type referenced</typeparam>
        /// <param name="expression">The expression referencing the member</param>
        /// <returns>The name of the member referenced by the expression</returns>
        public static string GetMemberName<T>(this Expression<Action<T>> expression)
        {
            return expression.GetMemberExpression().Member.Name;
        }

        public static string GetMemberName<T>(this Expression<Func<T>> expression)
        {
            return expression.GetMemberExpression().Member.Name;
        }

        public static PropertyInfo GetPropertyInfo<T, TMember>(this Expression<Func<T, TMember>> expression)
        {
            return expression.GetMemberExpression().Member as PropertyInfo;
        }

        public static PropertyInfo GetPropertyInfo<T>(this Expression<Func<T>> expression)
        {
            return expression.GetMemberExpression().Member as PropertyInfo;
        }

        public static MemberInfo GetMemberInfo<T>(this Expression<Action<T>> expression)
        {
            return expression.GetMemberExpression().Member;
        }

        public static MemberExpression GetMemberExpression<T, TMember>(this Expression<Func<T, TMember>> expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            return GetMemberExpression(expression.Body);
        }

        public static MemberExpression GetMemberExpression<T>(this Expression<Action<T>> expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            return GetMemberExpression(expression.Body);
        }

        public static MemberExpression GetMemberExpression<T>(this Expression<Func<T>> expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            return GetMemberExpression(expression.Body);
        }

        public static MemberExpression GetMemberExpression<T1, T2>(this Expression<Action<T1, T2>> expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            return GetMemberExpression(expression.Body);
        }

        static MemberExpression GetMemberExpression(Expression body)
        {
            if (body == null)
                throw new ArgumentNullException("body");

            MemberExpression memberExpression = null;
            if (body.NodeType == ExpressionType.Convert)
            {
                var unaryExpression = (UnaryExpression)body;
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
            else if (body.NodeType == ExpressionType.MemberAccess)
                memberExpression = body as MemberExpression;

            if (memberExpression == null)
                throw new ArgumentException("Expression is not a member access");

            return memberExpression;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Extensions\InterfaceExtensions.cs
//------------------------------------------------------------------------------
namespace Internals.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Reflection;

    static class InterfaceExtensions
    {
        static readonly InterfaceReflectionCache _cache;

        static InterfaceExtensions()
        {
            _cache = new InterfaceReflectionCache();
        }

        public static bool HasInterface<T>(this object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            Type type = obj.GetType();

            return HasInterface(type, typeof(T));
        }

        public static bool HasInterface<T>(this Type type)
        {
            return HasInterface(type, typeof(T));
        }

        public static bool HasInterface(this Type type, Type interfaceType)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (interfaceType == null)
                throw new ArgumentNullException("interfaceType");
            if (!interfaceType.IsInterface)
                throw new ArgumentException("The interface type must be an interface: " + interfaceType.Name);

            if (interfaceType.IsGenericTypeDefinition)
                return _cache.GetGenericInterface(type, interfaceType) != null;

            return interfaceType.IsAssignableFrom(type);
        }

        public static bool HasInterface(this object obj, Type interfaceType)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            Type type = obj.GetType();

            return HasInterface(type, interfaceType);
        }

        public static Type GetInterface<T>(this object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            Type type = obj.GetType();

            return GetInterface(type, typeof(T));
        }

        public static Type GetInterface<T>(this Type type)
        {
            return GetInterface(type, typeof(T));
        }

        public static Type GetInterface(this Type type, Type interfaceType)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (interfaceType == null)
                throw new ArgumentNullException("interfaceType");
            if (!interfaceType.IsInterface)
                throw new ArgumentException("The interface type must be an interface: " + interfaceType.Name);

            return _cache.Get(type, interfaceType);
        }

        public static bool ClosesType(this Type type, Type openType)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (openType == null)
                throw new ArgumentNullException("openType");

            if (!openType.IsOpenGeneric())
                throw new ArgumentException("The interface type must be an open generic interface: " + openType.Name);

            if (openType.IsInterface)
            {
                if (!openType.IsOpenGeneric())
                    throw new ArgumentException("The interface type must be an open generic interface: " + openType.Name);

                Type interfaceType = type.GetInterface(openType);
                if (interfaceType == null)
                    return false;

                return !interfaceType.IsGenericTypeDefinition && !interfaceType.ContainsGenericParameters;
            }

            Type baseType = type;
            while (baseType != null && baseType != typeof(object))
            {
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == openType)
                    return !baseType.IsGenericTypeDefinition && !baseType.ContainsGenericParameters;

                if (!baseType.IsGenericType && baseType == openType)
                    return true;

                baseType = baseType.BaseType;
            }

            return false;
        }

        public static IEnumerable<Type> GetClosingArguments(this Type type, Type openType)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (openType == null)
                throw new ArgumentNullException("openType");

            if (!openType.IsOpenGeneric())
                throw new ArgumentException("The interface type must be an open generic interface: " + openType.Name);

            if (openType.IsInterface)
            {
                if (!openType.IsOpenGeneric())
                    throw new ArgumentException("The interface type must be an open generic interface: " + openType.Name);

                Type interfaceType = type.GetInterface(openType);
                if (interfaceType == null)
                    throw new ArgumentException("The interface type is not implemented by: " + type.Name);

                return interfaceType.GetGenericArguments().Where(x => !x.IsGenericParameter);
            }

            Type baseType = type;
            while (baseType != null && baseType != typeof(object))
            {
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == openType)
                    return baseType.GetGenericArguments().Where(x => !x.IsGenericParameter);

                if (!baseType.IsGenericType && baseType == openType)
                    return baseType.GetGenericArguments().Where(x => !x.IsGenericParameter);

                baseType = baseType.BaseType;
            }
            
            throw new ArgumentException("Could not find open type in type: " + type.Name);
        }

        public static IEnumerable<Type> GetClosingArguments(this object obj, Type openType)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            Type objectType = obj.GetType();

            return GetClosingArguments(objectType, openType);
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Extensions\TimeSpanExtensions.cs
//------------------------------------------------------------------------------
namespace Internals.Extensions
{
    using System;
    using System.Text;

    static class TimeSpanExtensions
    {
        static readonly TimeSpan _day = TimeSpan.FromDays(1);
        static readonly TimeSpan _hour = TimeSpan.FromHours(1);
        static readonly TimeSpan _month = TimeSpan.FromDays(30);
        static readonly TimeSpan _year = TimeSpan.FromDays(365);


        public static string ToFriendlyString(this TimeSpan ts)
        {
            if (ts.Equals(_month))
                return "1M";
            if (ts.Equals(_year))
                return "1y";
            if (ts.Equals(_day))
                return "1d";
            if (ts.Equals(_hour))
                return "1h";

            var sb = new StringBuilder();

            int years = ts.Days/365;
            int months = (ts.Days%365)/30;
            int weeks = ((ts.Days%365)%30)/7;
            int days = (((ts.Days%365)%30)%7);

            if (years > 0)
                sb.Append(years).Append("y");

            if (months > 0)
                sb.Append(months).Append("M");

            if (weeks > 0)
                sb.Append(weeks).Append("w");

            if (days > 0)
                sb.Append(days).Append("d");

            if (ts.Hours > 0)
                sb.Append(ts.Hours).Append("h");
            if (ts.Minutes > 0)
                sb.Append(ts.Minutes).Append("m");
            if (ts.Seconds > 0)
                sb.Append(ts.Seconds).Append("s");
            if (ts.Milliseconds > 0)
                sb.Append(ts.Milliseconds).Append("ms");

            if (ts.Ticks == 0)
                sb.Append("-0-");
            else if (sb.Length == 0)
                sb.Append(ts.Ticks*100).Append("ns");

            return sb.ToString();
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Extensions\TypeExtensions.cs
//------------------------------------------------------------------------------
namespace Internals.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Reflection;

    static class TypeExtensions
    {
        static readonly TypeNameFormatter _typeNameFormatter = new TypeNameFormatter();

        public static IEnumerable<PropertyInfo> GetAllProperties(this Type type)
        {
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;

            PropertyInfo[] properties = type.GetProperties(bindingFlags);
            if (type.IsInterface)
            {
                return properties.Concat(type.GetInterfaces().SelectMany(x => x.GetProperties(bindingFlags)));
            }

            return properties;
        }

        public static IEnumerable<PropertyInfo> GetAllStaticProperties(this Type type)
        {
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

            return type.GetProperties(bindingFlags);
        }

        /// <summary>
        /// Determines if a type is neither abstract nor an interface and can be constructed.
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>True if the type can be constructed, otherwise false.</returns>
        public static bool IsConcreteType(this Type type)
        {
            return !type.IsAbstract && !type.IsInterface;
        }

        /// <summary>
        /// Determines if a type can be constructed, and if it can, additionally determines
        /// if the type can be assigned to the specified type.
        /// </summary>
        /// <param name="type">The type to evaluate</param>
        /// <param name="assignableType">The type to which the subject type should be checked against</param>
        /// <returns>True if the type is concrete and can be assigned to the assignableType, otherwise false.</returns>
        public static bool IsConcreteAndAssignableTo(this Type type, Type assignableType)
        {
            return IsConcreteType(type) && assignableType.IsAssignableFrom(type);
        }

        /// <summary>
        /// Determines if a type can be constructed, and if it can, additionally determines
        /// if the type can be assigned to the specified type.
        /// </summary>
        /// <param name="type">The type to evaluate</param>
        /// <typeparam name="T">The type to which the subject type should be checked against</typeparam>
        /// <returns>True if the type is concrete and can be assigned to the assignableType, otherwise false.</returns>
        public static bool IsConcreteAndAssignableTo<T>(this Type type)
        {
            return IsConcreteType(type) && typeof(T).IsAssignableFrom(type);
        }

        /// <summary>
        /// Determines if the type is an open generic with at least one unspecified generic argument
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>True if the type is an open generic</returns>
        public static bool IsOpenGeneric(this Type type)
        {
            return type.IsGenericTypeDefinition || type.ContainsGenericParameters;
        }

        public static string GetTypeName(this Type type)
        {
            return _typeNameFormatter.GetTypeName(type);
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Primitives\Enumeration.cs
//------------------------------------------------------------------------------
namespace Internals.Primitives
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Caching;
    using Extensions;

    [Serializable]
    internal abstract class Enumeration :
        IComparable
    {
        static readonly Cache<Type, EnumerationInfo> _cache;

        public readonly int Index;
        public readonly string Name;

        static Enumeration()
        {
#if NET35
            _cache = new ReaderWriterLockedCache<Type, EnumerationInfo>(new DictionaryCache<Type, EnumerationInfo>(type => new EnumerationInfo(type)));
#else
            _cache = new ConcurrentCache<Type, EnumerationInfo>(type => new EnumerationInfo(type));
#endif
        }

        protected Enumeration()
        {
        }

        protected Enumeration(int index, string name)
        {
            Index = index;
            Name = name;
        }

        public virtual int CompareTo(object other)
        {
            return Index.CompareTo(((Enumeration)other).Index);
        }

        public override string ToString()
        {
            return Name;
        }

        public static IEnumerable<T> GetAll<T>()
            where T : Enumeration, new()
        {
            return _cache[typeof(T)].Cast<T>();
        }

        public static IEnumerable<Enumeration> GetAll(Type type)
        {
            return _cache[type];
        }

        public static T FromIndex<T>(int index)
            where T : Enumeration
        {
            return _cache[typeof(T)].FromIndex(index) as T;
        }

        public static T Parse<T>(string name)
            where T : Enumeration
        {
            return _cache[typeof(T)].Parse(name) as T;
        }

        public override bool Equals(object obj)
        {
            var otherValue = obj as Enumeration;
            if (otherValue == null)
                return false;

            return GetType() == obj.GetType() && Index.Equals(otherValue.Index);
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }

        protected static T Init<T>(Expression<Func<T>> expression, int index, params object[] args)
        {
            string name = expression.GetMemberName();

            IEnumerable<object> arguments = new object[] {index, name};
            if (args != null)
                arguments = arguments.Concat(args);

            return (T)Activator.CreateInstance(typeof(T), arguments.ToArray());
        }

        class EnumerationInfo :
            IEnumerable<Enumeration>
        {
            readonly Cache<int, Enumeration> _indices;
            readonly Cache<string, Enumeration> _names;

            public EnumerationInfo(Type type)
            {
                _names = new DictionaryCache<string, Enumeration>();
                _indices = new DictionaryCache<int, Enumeration>();

                const BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;
                IEnumerable<FieldInfo> fields = type.GetFields(flags);

                foreach (FieldInfo field in fields)
                {
                    var value = (Enumeration)field.GetValue(null);

                    _names.Add(value.Name, value);
                    _indices.Add(value.Index, value);
                }
            }

            public IEnumerator<Enumeration> GetEnumerator()
            {
                return _indices.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public Enumeration FromIndex(int index)
            {
                if (_indices.Has(index))
                    return _indices[index];

                throw new ArgumentException("The index was not found: " + index, "index");
            }

            public Enumeration Parse(string name)
            {
                if (_names.Has(name))
                    return _names[name];

                throw new ArgumentException("The name was not found: " + name, "name");
            }
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Primitives\Observable.cs
//------------------------------------------------------------------------------
namespace Internals.Primitives
{
#if !NET35
    using System;
    using System.Threading;
    using Caching;

    class Observable<T> :
        IObservable<T>,
        IObserver<T>
    {
        readonly Cache<int, IObserver<T>> _observers;

        int _key;

        public Observable()
        {
            _observers = new ConcurrentCache<int, IObserver<T>>();
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null)
                throw new ArgumentNullException("observer");

            int observerId = Interlocked.Increment(ref _key);

            _observers.Add(observerId, observer);

            return new ObserverReference(observerId, id => _observers.Remove(id));
        }

        public void OnNext(T value)
        {
            _observers.Each(x => x.OnNext(value));
        }

        public void OnError(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException("exception");

            _observers.Each(x => x.OnError(exception));
        }

        public void OnCompleted()
        {
            _observers.Each(x => x.OnCompleted());
        }


        class ObserverReference :
            IDisposable
        {
            readonly int _observerId;
            readonly Action<int> _removeObserver;

            public ObserverReference(int observerId, Action<int> removeObserver)
            {
                _observerId = observerId;
                _removeObserver = removeObserver;
            }

            public void Dispose()
            {
                _removeObserver(_observerId);
            }
        }
    }
#endif
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Reflection\ComponentFactory.cs
//------------------------------------------------------------------------------
namespace Internals.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;

    /// <summary>
    /// Provides registration and resolution of components within an AppDomain. The registrations are static,
    /// but done to provide a global style of registration without the management of a full container.
    /// </summary>
    static class ComponentFactory
    {
        static readonly MethodInfo _add = new Action<Func<object>>(Add).Method.GetGenericMethodDefinition();
        static readonly MethodInfo _get = new Func<object>(Get<object>).Method.GetGenericMethodDefinition();
        static readonly MethodInfo _remove = new Action(Remove<object>).Method.GetGenericMethodDefinition();

        /// <summary>
        /// Add a type with a factory method for creating the type
        /// </summary>
        /// <typeparam name="T">The type to add</typeparam>
        /// <param name="factoryMethod">The factory method for the class that implements the added type</param>
        public static void Add<T>(Func<T> factoryMethod)
        {
            if (factoryMethod == null)
                throw new ArgumentNullException("factoryMethod", "A valid factory method must be supplied");

            if (Factory<T>.Get != null)
                throw new ArgumentException(string.Format("A factory method for the type '{0}' was already added",
                    typeof(T).Name));

            Factory<T>.Get = factoryMethod;
        }

        /// <summary>
        /// Adds a type with an implementation that has no dependencies
        /// </summary>
        /// <typeparam name="T">The type to add</typeparam>
        /// <typeparam name="TImplementation">The implementation type to add</typeparam>
        public static void Add<T, TImplementation>()
            where TImplementation : T, new()
        {
            Add<T>(() => new TImplementation());
        }

        /// <summary>
        /// Add a type with a implementation type that may have dependencies
        /// </summary>
        /// <typeparam name="T">The type to add</typeparam>
        /// <param name="implementationType">The implementation type to add</param>
        public static void Add<T>(Type implementationType)
        {
            Add(typeof(T), implementationType);
        }

        /// <summary>
        /// Add a concrete type with no dependencies
        /// </summary>
        /// <typeparam name="T">The type to add</typeparam>
        public static void Add<T>()
            where T : new()
        {
            Add(() => new T());
        }

        /// <summary>
        /// Add a type with a single instance implementing the type
        /// </summary>
        /// <typeparam name="T">The type to add</typeparam>
        /// <param name="instance">The instance of a class implementing the added type</param>
        public static void Add<T>(T instance)
        {
            if (Factory<T>.Get != null)
                throw new ArgumentException(string.Format("A factory method for the type '{0}' was already added",
                    typeof(T).Name));

            Factory<T>.Get = () => instance;
        }

        /// <summary>
        /// Add a type with the same implementation type, automatically resolving any dependencies
        /// </summary>
        /// <param name="addType">The type to add</param>
        public static void Add(Type addType)
        {
            try
            {
                Add(addType, addType);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Add a type with an implementation type selecting the greediest constructor on the implementation
        /// that can be satisfied by the registered types
        /// </summary>
        /// <param name="addType">The type to add</param>
        /// <param name="implementationType">The implementation type to add</param>
        public static void Add(Type addType, Type implementationType)
        {
            if (!implementationType.IsConcreteType())
                throw new ArgumentException(string.Format("The type '{0}' must be a concrete type",
                    addType.Name));

            IOrderedEnumerable<ConstructorInfo> candidates = implementationType.GetConstructors()
                .OrderByDescending(x => x.GetParameters().Length);

            Exception lastException = null;
            foreach (ConstructorInfo candidate in candidates)
            {
                try
                {
                    Add(addType, implementationType, candidate.GetParameters().Select(x => x.ParameterType).ToArray());
                    return;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }
            }

            throw lastException ?? new InvalidOperationException(
                                       string.Format("No constructor on type '{0}' could be satisfied", addType.Name));
        }


        /// <summary>
        /// Add a type with a specific list of dependency types matching an existing constructor
        /// on the implementation type
        /// </summary>
        /// <param name="addType"> </param>
        /// <param name="implementationType"> </param>
        /// <param name="dependencies"></param>
        public static void Add(Type addType, Type implementationType, params Type[] dependencies)
        {
            if (!implementationType.IsConcreteType())
                throw new ArgumentException(string.Format("The implementation type '{0}' must be a concrete type",
                    implementationType.Name));

            if (!addType.IsAssignableFrom(implementationType))
                throw new ArgumentException(
                    string.Format("The implementation type '{0}' must be assignable to the added type '{1}'",
                        implementationType.Name, addType.Name));

            ConstructorInfo constructor = implementationType.GetConstructor(dependencies);
            if (constructor == null)
                throw new ArgumentException(
                    string.Format("No constructor on type '{0}' accepts ({1})", implementationType.Name,
#if !NET35
                        string.Join(",", dependencies.Select(x => x.Name))));
#else
                        string.Join(",", dependencies.Select(x => x.Name).ToArray())));
#endif

            ParameterInfo[] parameters = constructor.GetParameters();

            IEnumerable<Expression> arguments = dependencies
                .Select((type, index) => GetResolveExpression(parameters[index].ParameterType));

            NewExpression newExpression = Expression.New(constructor, arguments);

            Delegate factoryMethod = Expression.Lambda(Expression.TypeAs(newExpression, implementationType)).Compile();

            try
            {
                _add.MakeGenericMethod(addType).Invoke(null, new object[] {factoryMethod});
            }
            catch (Exception ex)
            {
                throw ex.InnerException ?? ex;
            }
        }

        /// <summary>
        /// Resolve the specified type
        /// </summary>
        /// <typeparam name="T">The type to resolve</typeparam>
        /// <returns>An implementation of the specified type</returns>
        public static T Get<T>()
        {
            if (Factory<T>.Get == null)
                TryToResolveTypeFactory(typeof(T));

            if (Factory<T>.Get == null)
                throw new InvalidOperationException(
                    string.Format("The type '{0}' has not been added", typeof(T).Name));

            return Factory<T>.Get();
        }

        static void TryToResolveTypeFactory(Type componentType)
        {
            try
            {
                Add(componentType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    string.Format("The type '{0}' has not been added", componentType.Name), ex);
            }
        }

        /// <summary>
        /// Remove the type
        /// </summary>
        /// <typeparam name="T">The type to remove</typeparam>
        public static void Remove<T>()
        {
            Factory<T>.Get = null;
        }

        /// <summary>
        /// Remove the type
        /// </summary>
        /// <param name="type">The type to remove</param>
        public static void Remove(Type type)
        {
            try
            {
                _remove.MakeGenericMethod(type)
                    .Invoke(null, null);
            }
            catch (Exception e)
            {
                throw e.InnerException;
            }
        }

        static Expression GetResolveExpression(Type type)
        {
            return Expression.Call(null, _get.MakeGenericMethod(type));
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Reflection\Factory.cs
//------------------------------------------------------------------------------
namespace Internals.Reflection
{
    using System;

    /// <summary>
    /// Type factory
    /// </summary>
    /// <typeparam name="T">The factory component type</typeparam>
    static class Factory<T>
    {
        /// <summary>
        /// The factory method to get an instance of the component type
        /// </summary>
        internal static Func<T> Get { get; set; }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Reflection\InterfaceReflectionCache.cs
//------------------------------------------------------------------------------
namespace Internals.Reflection
{
    using System;
    using Caching;

    class InterfaceReflectionCache
    {
        readonly Cache<Type, Cache<Type, Type>> _cache;

        public InterfaceReflectionCache()
        {
#if NET35
            _cache = new ReaderWriterLockedCache<Type, Cache<Type, Type>>(new DictionaryCache<Type, Cache<Type,Type>>(typeKey =>
            {
                MissingValueProvider<Type, Type> missingValueProvider = x => GetInterfaceInternal(typeKey, x);

                return new ReaderWriterLockedCache<Type, Type>(new DictionaryCache<Type,Type>(missingValueProvider));
            }));
#else
            _cache = new ConcurrentCache<Type, Cache<Type, Type>>(typeKey =>
                {
                    MissingValueProvider<Type, Type> missingValueProvider = x => GetInterfaceInternal(typeKey, x);

                    return new ConcurrentCache<Type, Type>(missingValueProvider);
                });
#endif
        }

        Type GetInterfaceInternal(Type type, Type interfaceType)
        {
            if (interfaceType.IsGenericTypeDefinition)
                return GetGenericInterface(type, interfaceType);

            Type[] interfaces = type.GetInterfaces();
            for (int i = 0; i < interfaces.Length; i++)
            {
                if (interfaces[i] == interfaceType)
                {
                    return interfaces[i];
                }
            }

            return null;
        }

        public Type GetGenericInterface(Type type, Type interfaceType)
        {
            if (!interfaceType.IsGenericTypeDefinition)
                throw new ArgumentException(
                    "The interface must be a generic interface definition: " + interfaceType.Name,
                    "interfaceType");

            // our contract states that we will not return generic interface definitions without generic type arguments
            if (type == interfaceType)
                return null;

            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == interfaceType)
                {
                    return type;
                }
            }

            Type[] interfaces = type.GetInterfaces();
            for (int i = 0; i < interfaces.Length; i++)
            {
                if (interfaces[i].IsGenericType)
                {
                    if (interfaces[i].GetGenericTypeDefinition() == interfaceType)
                    {
                        return interfaces[i];
                    }
                }
            }

            return null;
        }

        public Type Get(Type type, Type interfaceType)
        {
            return _cache[type][interfaceType];
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Reflection\ReadOnlyProperty.cs
//------------------------------------------------------------------------------
namespace Internals.Reflection
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;

    class ReadOnlyProperty
    {
        public readonly Func<object, object> GetProperty;

        public ReadOnlyProperty(PropertyInfo property)
        {
            Property = property;
            GetProperty = GetGetMethod(Property);
        }

        public PropertyInfo Property { get; private set; }

        public object Get(object instance)
        {
            return GetProperty(instance);
        }

        static Func<object, object> GetGetMethod(PropertyInfo property)
        {
            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            UnaryExpression instanceCast;
            if (property.DeclaringType.IsValueType)
                instanceCast = Expression.Convert(instance, property.DeclaringType);
            else
                instanceCast = Expression.TypeAs(instance, property.DeclaringType);

            MethodCallExpression call = Expression.Call(instanceCast, property.GetGetMethod());
            UnaryExpression typeAs = Expression.TypeAs(call, typeof(object));

            return Expression.Lambda<Func<object, object>>(typeAs, instance).Compile();
        }
    }

    class ReadOnlyProperty<T>
    {
        public readonly Func<T, object> GetProperty;

        public ReadOnlyProperty(Expression<Func<T, object>> propertyExpression)
            : this(propertyExpression.GetPropertyInfo())
        {
        }

        public ReadOnlyProperty(PropertyInfo property)
        {
            Property = property;
            GetProperty = GetGetMethod(Property);
        }

        public PropertyInfo Property { get; private set; }

        public object Get(T instance)
        {
            return GetProperty(instance);
        }

        static Func<T, object> GetGetMethod(PropertyInfo property)
        {
            ParameterExpression instance = Expression.Parameter(typeof(T), "instance");
            MethodCallExpression call = Expression.Call(instance, property.GetGetMethod());
            UnaryExpression typeAs = Expression.TypeAs(call, typeof(object));
            return Expression.Lambda<Func<T, object>>(typeAs, instance).Compile();
        }
    }

    class ReadOnlyProperty<T, TProperty>
    {
        public readonly Func<T, TProperty> GetProperty;

        public ReadOnlyProperty(Expression<Func<T, object>> propertyExpression)
            : this(propertyExpression.GetPropertyInfo())
        {
        }

        public ReadOnlyProperty(PropertyInfo property)
        {
            Property = property;
            GetProperty = GetGetMethod(Property);
        }

        public PropertyInfo Property { get; private set; }

        public TProperty Get(T instance)
        {
            return GetProperty(instance);
        }

        static Func<T, TProperty> GetGetMethod(PropertyInfo property)
        {
            ParameterExpression instance = Expression.Parameter(typeof(T), "instance");
            MethodCallExpression call = Expression.Call(instance, property.GetGetMethod());

            return Expression.Lambda<Func<T, TProperty>>(call, instance).Compile();
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Reflection\ReadOnlyPropertyCache.cs
//------------------------------------------------------------------------------
namespace Internals.Reflection
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Caching;
    using Extensions;

    class ReadOnlyPropertyCache<T> :
        AbstractCacheDecorator<string, ReadOnlyProperty<T>>
    {
        public ReadOnlyPropertyCache()
            : base(CreatePropertyCache())
        {
        }

        static Cache<string, ReadOnlyProperty<T>> CreatePropertyCache()
        {
            return new DictionaryCache<string, ReadOnlyProperty<T>>(typeof(T).GetAllProperties()
                .Where(x => x.CanRead)
                .Select(x => new ReadOnlyProperty<T>(x))
                .ToDictionary(x => x.Property.Name));
        }

        public object Get(Expression<Func<T, object>> propertyExpression, T instance)
        {
            return this[propertyExpression.GetMemberName()].Get(instance);
        }

        public void Each(T instance, Action<ReadOnlyProperty<T>, object> action)
        {
            Each(property => action(property, property.Get(instance)));
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Reflection\ReadWriteProperty.cs
//------------------------------------------------------------------------------
namespace Internals.Reflection
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;

    class ReadWriteProperty :
        ReadOnlyProperty
    {
        public readonly Action<object, object> SetProperty;

        public ReadWriteProperty(PropertyInfo property)
            : this(property, false)
        {
        }

        public ReadWriteProperty(PropertyInfo property, bool includeReadOnly)
            : base(property)
        {
            SetProperty = GetSetMethod(Property, includeReadOnly);
        }

        public void Set(object instance, object value)
        {
            SetProperty(instance, value);
        }

        static Action<object, object> GetSetMethod(PropertyInfo property, bool includeNonPublic)
        {
            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            ParameterExpression value = Expression.Parameter(typeof(object), "value");

            // value as T is slightly faster than (T)value, so if it's not a value type, use that
            UnaryExpression instanceCast;
            if (property.DeclaringType.IsValueType)
                instanceCast = Expression.Convert(instance, property.DeclaringType);
            else
                instanceCast = Expression.TypeAs(instance, property.DeclaringType);

            UnaryExpression valueCast;
            if (property.PropertyType.IsValueType)
                valueCast = Expression.Convert(value, property.PropertyType);
            else
                valueCast = Expression.TypeAs(value, property.PropertyType);

            MethodCallExpression call = Expression.Call(instanceCast, property.GetSetMethod(includeNonPublic), valueCast);

            return Expression.Lambda<Action<object, object>>(call, new[] {instance, value}).Compile();
        }
    }

    class ReadWriteProperty<T> :
        ReadOnlyProperty<T>
    {
        public readonly Action<T, object> SetProperty;

        public ReadWriteProperty(Expression<Func<T, object>> propertyExpression)
            : this(propertyExpression.GetPropertyInfo(), false)
        {
        }

        public ReadWriteProperty(Expression<Func<T, object>> propertyExpression, bool includeNonPublic)
            : this(propertyExpression.GetPropertyInfo(), includeNonPublic)
        {
        }

        public ReadWriteProperty(PropertyInfo property)
            : this(property, false)
        {
        }

        public ReadWriteProperty(PropertyInfo property, bool includeNonPublic)
            : base(property)
        {
            SetProperty = GetSetMethod(Property, includeNonPublic);
        }

        public void Set(T instance, object value)
        {
            SetProperty(instance, value);
        }

        static Action<T, object> GetSetMethod(PropertyInfo property, bool includeNonPublic)
        {
            if (!property.CanWrite)
                return (x, i) => { throw new InvalidOperationException("No setter available on " + property.Name); };


            ParameterExpression instance = Expression.Parameter(typeof(T), "instance");
            ParameterExpression value = Expression.Parameter(typeof(object), "value");
            UnaryExpression valueCast;
            if (property.PropertyType.IsValueType)
                valueCast = Expression.Convert(value, property.PropertyType);
            else
                valueCast = Expression.TypeAs(value, property.PropertyType);

            MethodCallExpression call = Expression.Call(instance, property.GetSetMethod(includeNonPublic), valueCast);

            return Expression.Lambda<Action<T, object>>(call, new[] {instance, value}).Compile();
        }
    }

    class ReadWriteProperty<T, TProperty> :
        ReadOnlyProperty<T, TProperty>
    {
        public readonly Action<T, TProperty> SetProperty;

        public ReadWriteProperty(Expression<Func<T, object>> propertyExpression)
            : this(propertyExpression.GetPropertyInfo(), false)
        {
        }

        public ReadWriteProperty(Expression<Func<T, object>> propertyExpression, bool includeNonPublic)
            : this(propertyExpression.GetPropertyInfo(), includeNonPublic)
        {
        }

        public ReadWriteProperty(PropertyInfo property)
            : this(property, false)
        {
        }

        public ReadWriteProperty(PropertyInfo property, bool includeNonPublic)
            : base(property)
        {
            SetProperty = GetSetMethod(Property, includeNonPublic);
        }

        public void Set(T instance, TProperty value)
        {
            SetProperty(instance, value);
        }

        static Action<T, TProperty> GetSetMethod(PropertyInfo property, bool includeNonPublic)
        {
            ParameterExpression instance = Expression.Parameter(typeof(T), "instance");
            ParameterExpression value = Expression.Parameter(typeof(TProperty), "value");
            MethodCallExpression call = Expression.Call(instance, property.GetSetMethod(includeNonPublic), value);

            return Expression.Lambda<Action<T, TProperty>>(call, new[] {instance, value}).Compile();
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Reflection\ReadWritePropertyCache.cs
//------------------------------------------------------------------------------
namespace Internals.Reflection
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Caching;
    using Extensions;

    class ReadWritePropertyCache<T> :
        AbstractCacheDecorator<string, ReadWriteProperty<T>>
    {
        public ReadWritePropertyCache()
            : this(false)
        {
        }

        public ReadWritePropertyCache(bool includeNonPublic)
            : base(CreatePropertyCache(includeNonPublic))
        {
        }

        static Cache<string, ReadWriteProperty<T>> CreatePropertyCache(bool includeNonPublic)
        {
            return new DictionaryCache<string, ReadWriteProperty<T>>(typeof(T).GetAllProperties()
                .Where(x => x.CanRead && (includeNonPublic || x.CanWrite))
                .Where(x => x.GetSetMethod(includeNonPublic) != null)
                .Select(x => new ReadWriteProperty<T>(x, includeNonPublic))
                .ToDictionary(x => x.Property.Name));
        }

        public void Set(Expression<Func<T, object>> propertyExpression, T instance, object value)
        {
            this[propertyExpression.GetMemberName()].Set(instance, value);
        }

        public object Get(Expression<Func<T, object>> propertyExpression, T instance)
        {
            return this[propertyExpression.GetMemberName()].Get(instance);
        }

        public void Each(T instance, Action<ReadWriteProperty<T>, object> action)
        {
            Each(property => action(property, property.Get(instance)));
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Internals\Reflection\TypeNameFormatter.cs
//------------------------------------------------------------------------------
namespace Internals.Reflection
{
    using System;
    using System.Text;
    using Caching;

    class TypeNameFormatter
    {
        readonly Cache<Type, string> _cache;
        readonly string _genericArgumentSeparator;
        readonly string _genericOpen;
        readonly string _genericClose;
        readonly string _namespaceSeparator;
        readonly string _nestedTypeSeparator;

        public TypeNameFormatter()
            : this(",", "<", ">", ".", "+")
        {
        }

        public TypeNameFormatter(string genericArgumentSeparator, string genericOpen, string genericClose,
            string namespaceSeparator, string nestedTypeSeparator)
        {
            _genericArgumentSeparator = genericArgumentSeparator;
            _genericOpen = genericOpen;
            _genericClose = genericClose;
            _namespaceSeparator = namespaceSeparator;
            _nestedTypeSeparator = nestedTypeSeparator;

#if NET35
            _cache = new ReaderWriterLockedCache<Type, string>(new DictionaryCache<Type, string>(FormatTypeName));
#else
            _cache = new ConcurrentCache<Type, string>(FormatTypeName);
#endif
        }

        public string GetTypeName(Type type)
        {
            return _cache[type];
        }

        string FormatTypeName(Type type)
        {
            if (type.IsGenericTypeDefinition)
                throw new ArgumentException("An open generic type cannot be used as a message name");

            var sb = new StringBuilder("");

            return FormatTypeName(sb, type, null);
        }

        string FormatTypeName(StringBuilder sb, Type type, string scope)
        {
            if (type.IsGenericParameter)
                return "";

            if (type.Namespace != null)
            {
                string ns = type.Namespace;
                if (!ns.Equals(scope))
                {
                    sb.Append(ns);
                    sb.Append(_namespaceSeparator);
                }
            }

            if (type.IsNested)
            {
                FormatTypeName(sb, type.DeclaringType, type.Namespace);
                sb.Append(_nestedTypeSeparator);
            }

            if (type.IsGenericType)
            {
                string name = type.GetGenericTypeDefinition().Name;

                //remove `1
                int index = name.IndexOf('`');
                if (index > 0)
                    name = name.Remove(index);

                sb.Append(name);
                sb.Append(_genericOpen);

                Type[] arguments = type.GetGenericArguments();
                for (int i = 0; i < arguments.Length; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(_genericArgumentSeparator);
                    }

                    FormatTypeName(sb, arguments[i], type.Namespace);
                }

                sb.Append(_genericClose);
            }
            else
                sb.Append(type.Name);

            return sb.ToString();
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Logging\HostLogger.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Logging
{
    using System;
    using Internals.Extensions;

    internal static class HostLogger
    {
        static readonly object _locker = new object();
        static HostLoggerConfigurator _configurator;
        static LogWriterFactory _logWriterFactory;

        public static LogWriterFactory Current
        {
            get
            {
                lock (_locker)
                {
                    return _logWriterFactory ?? CreateLogWriterFactory();
                }
            }
        }

        public static HostLoggerConfigurator CurrentHostLoggerConfigurator
        {
            get { return _configurator ?? (_configurator = new TraceHostLoggerConfigurator()); }
        }

        static LogWriterFactory CreateLogWriterFactory()
        {
            _logWriterFactory = CurrentHostLoggerConfigurator.CreateLogWriterFactory();

            return _logWriterFactory;
        }

        public static LogWriter Get<T>()
            where T : class
        {
            return Get(typeof(T).GetTypeName());
        }

        public static LogWriter Get(Type type)
        {
            return Get(type.GetTypeName());
        }

        static LogWriter Get(string name)
        {
            return Current.Get(name);
        }

        public static void UseLogger(HostLoggerConfigurator configurator)
        {
            lock (_locker)
            {
                _configurator = configurator;

                LogWriterFactory logger = _configurator.CreateLogWriterFactory();

                if (_logWriterFactory != null)
                    _logWriterFactory.Shutdown();
                _logWriterFactory = null;

                _logWriterFactory = logger;
            }
        }

        public static void Shutdown()
        {
            lock (_locker)
            {
                if (_logWriterFactory != null)
                {
                    _logWriterFactory.Shutdown();
                    _logWriterFactory = null;
                }
            }
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Logging\HostLoggerConfigurator.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Logging
{
    internal interface HostLoggerConfigurator
    {
        LogWriterFactory CreateLogWriterFactory();
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Logging\LoggingLevel.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Logging
{
    using System.Collections.Generic;
    using System.Diagnostics;

    internal class LoggingLevel
    {
        public static readonly LoggingLevel All = new LoggingLevel("All", 6, SourceLevels.All, TraceEventType.Verbose);
        public static readonly LoggingLevel Debug = new LoggingLevel("Debug", 5, SourceLevels.Verbose, TraceEventType.Verbose);
        public static readonly LoggingLevel Error = new LoggingLevel("Error", 2, SourceLevels.Error, TraceEventType.Error);
        public static readonly LoggingLevel Fatal = new LoggingLevel("Fatal", 1, SourceLevels.Critical, TraceEventType.Critical);

        public static readonly LoggingLevel Info = new LoggingLevel("Info", 4, SourceLevels.Information,
            TraceEventType.Information);

        public static readonly LoggingLevel None = new LoggingLevel("None", 0, SourceLevels.Off, TraceEventType.Critical);
        public static readonly LoggingLevel Warn = new LoggingLevel("Warn", 3, SourceLevels.Warning, TraceEventType.Warning);

        readonly int _index;
        readonly string _name;
        readonly SourceLevels _sourceLevel;
        readonly TraceEventType _traceEventType;

        LoggingLevel(string name, int index, SourceLevels sourceLevel, TraceEventType traceEventType)
        {
            _name = name;
            _index = index;
            _sourceLevel = sourceLevel;
            _traceEventType = traceEventType;
        }

        public static IEnumerable<LoggingLevel> Values
        {
            get
            {
                yield return All;
                yield return Debug;
                yield return Info;
                yield return Warn;
                yield return Error;
                yield return Fatal;
                yield return None;
            }
        }

        public TraceEventType TraceEventType
        {
            get { return _traceEventType; }
        }

        public string Name
        {
            get { return _name; }
        }

        public SourceLevels SourceLevel
        {
            get { return _sourceLevel; }
        }

        public override string ToString()
        {
            return _name;
        }

        public static bool operator >(LoggingLevel left, LoggingLevel right)
        {
            return right != null && (left != null && left._index > right._index);
        }

        public static bool operator <(LoggingLevel left, LoggingLevel right)
        {
            return right != null && (left != null && left._index < right._index);
        }

        public static bool operator >=(LoggingLevel left, LoggingLevel right)
        {
            return right != null && (left != null && left._index >= right._index);
        }

        public static bool operator <=(LoggingLevel left, LoggingLevel right)
        {
            return right != null && (left != null && left._index <= right._index);
        }

        public static LoggingLevel FromSourceLevels(SourceLevels level)
        {
            switch (level)
            {
                case SourceLevels.Information:
                    return Info;
                case SourceLevels.Verbose:
                    return Debug;
                case ~SourceLevels.Off:
                    return Debug;
                case SourceLevels.Critical:
                    return Fatal;
                case SourceLevels.Error:
                    return Error;
                case SourceLevels.Warning:
                    return Warn;
                default:
                    return None;
            }
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Logging\LogWriter.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Logging
{
    using System;

    /// <summary>
    /// Implementers handle logging and filtering based on logging levels.
    /// </summary>
    internal interface LogWriter
    {
        bool IsDebugEnabled { get; }
        bool IsInfoEnabled { get; }
        bool IsWarnEnabled { get; }
        bool IsErrorEnabled { get; }
        bool IsFatalEnabled { get; }

        void Log(LoggingLevel level, object obj);
        void Log(LoggingLevel level, object obj, Exception exception);
        void Log(LoggingLevel level, LogWriterOutputProvider messageProvider);
        void LogFormat(LoggingLevel level, IFormatProvider formatProvider, string format, params object[] args);
        void LogFormat(LoggingLevel level, string format, params object[] args);

        void Debug(object obj);
        void Debug(object obj, Exception exception);
        void Debug(LogWriterOutputProvider messageProvider);
        void DebugFormat(IFormatProvider formatProvider, string format, params object[] args);
        void DebugFormat(string format, params object[] args);

        void Info(object obj);
        void Info(object obj, Exception exception);
        void Info(LogWriterOutputProvider messageProvider);
        void InfoFormat(IFormatProvider formatProvider, string format, params object[] args);
        void InfoFormat(string format, params object[] args);

        void Warn(object obj);
        void Warn(object obj, Exception exception);
        void Warn(LogWriterOutputProvider messageProvider);
        void WarnFormat(IFormatProvider formatProvider, string format, params object[] args);
        void WarnFormat(string format, params object[] args);

        void Error(object obj);
        void Error(object obj, Exception exception);
        void Error(LogWriterOutputProvider messageProvider);
        void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args);
        void ErrorFormat(string format, params object[] args);

        void Fatal(object obj);
        void Fatal(object obj, Exception exception);
        void Fatal(LogWriterOutputProvider messageProvider);
        void FatalFormat(IFormatProvider formatProvider, string format, params object[] args);
        void FatalFormat(string format, params object[] args);
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Logging\LogWriterFactory.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Logging
{
    internal interface LogWriterFactory
    {
        LogWriter Get(string name);

        void Shutdown();
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Logging\LogWriterOutputProvider.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Logging
{
    /// <summary>
    /// Delegate to provide the log output if the log level is enabled
    /// </summary>
    /// <returns></returns>
    internal delegate object LogWriterOutputProvider();
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Logging\TopshelfConsoleTraceListener.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Logging
{
    using System;
    using System.Diagnostics;

    internal class TopshelfConsoleTraceListener :
        TraceListener
    {
        public override void Write(string message)
        {
            Console.Write(message);
        }

        public override void WriteLine(string message)
        {
            Console.WriteLine(message);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id,
            string message)
        {
            WriteLine(message);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id,
            string format, params object[] args)
        {
            WriteLine(string.Format(format, args));
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Logging\TraceHostLoggerConfigurator.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Logging
{
    using System;

    [Serializable]
    internal class TraceHostLoggerConfigurator :
        HostLoggerConfigurator
    {
        public LogWriterFactory CreateLogWriterFactory()
        {
            return new TraceLogWriterFactory();
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Logging\TraceLogWriter.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Logging
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    internal class TraceLogWriter :
        LogWriter
    {
        readonly LoggingLevel _level;
        readonly TraceSource _source;

        public TraceLogWriter(TraceSource source)
        {
            _source = source;
            _level = LoggingLevel.FromSourceLevels(source.Switch.Level);
        }

        public bool IsDebugEnabled
        {
            get { return _level >= LoggingLevel.Debug; }
        }

        public bool IsInfoEnabled
        {
            get { return _level >= LoggingLevel.Info; }
        }

        public bool IsWarnEnabled
        {
            get { return _level >= LoggingLevel.Warn; }
        }

        public bool IsErrorEnabled
        {
            get { return _level >= LoggingLevel.Error; }
        }

        public bool IsFatalEnabled
        {
            get { return _level >= LoggingLevel.Fatal; }
        }

        public void LogFormat(LoggingLevel level, string format, params object[] args)
        {
            if (_level < level)
                return;

            LogInternal(level, string.Format(format, args), null);
        }

        /// <summary>
        /// Logs a debug message.
        /// 
        /// </summary>
        /// <param name="message">The message to log</param>
        public void Debug(object message)
        {
            if (!IsDebugEnabled)
                return;
            Log(LoggingLevel.Debug, message, null);
        }

        /// <summary>
        /// Logs a debug message.
        /// 
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="message">The message to log</param>
        public void Debug(object message, Exception exception)
        {
            if (!IsDebugEnabled)
                return;
            Log(LoggingLevel.Debug, message, exception);
        }

        public void Debug(LogWriterOutputProvider messageProvider)
        {
            if (!IsDebugEnabled)
                return;

            object obj = messageProvider();

            LogInternal(LoggingLevel.Debug, obj, null);
        }

        /// <summary>
        /// Logs a debug message.
        /// 
        /// </summary>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void DebugFormat(string format, params object[] args)
        {
            if (!IsDebugEnabled)
                return;
            LogInternal(LoggingLevel.Debug, string.Format(CultureInfo.CurrentCulture, format, args), null);
        }

        /// <summary>
        /// Logs a debug message.
        /// 
        /// </summary>
        /// <param name="formatProvider">The format provider to use</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!IsDebugEnabled)
                return;
            LogInternal(LoggingLevel.Debug, string.Format(formatProvider, format, args), null);
        }

        /// <summary>
        /// Logs an info message.
        /// 
        /// </summary>
        /// <param name="message">The message to log</param>
        public void Info(object message)
        {
            if (!IsInfoEnabled)
                return;
            Log(LoggingLevel.Info, message, null);
        }

        /// <summary>
        /// Logs an info message.
        /// 
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="message">The message to log</param>
        public void Info(object message, Exception exception)
        {
            if (!IsInfoEnabled)
                return;
            Log(LoggingLevel.Info, message, exception);
        }

        public void Info(LogWriterOutputProvider messageProvider)
        {
            if (!IsInfoEnabled)
                return;

            object obj = messageProvider();

            LogInternal(LoggingLevel.Info, obj, null);
        }

        /// <summary>
        /// Logs an info message.
        /// 
        /// </summary>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void InfoFormat(string format, params object[] args)
        {
            if (!IsInfoEnabled)
                return;
            LogInternal(LoggingLevel.Info, string.Format(CultureInfo.CurrentCulture, format, args), null);
        }

        /// <summary>
        /// Logs an info message.
        /// 
        /// </summary>
        /// <param name="formatProvider">The format provider to use</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!IsInfoEnabled)
                return;
            LogInternal(LoggingLevel.Info, string.Format(formatProvider, format, args), null);
        }

        /// <summary>
        /// Logs a warn message.
        /// 
        /// </summary>
        /// <param name="message">The message to log</param>
        public void Warn(object message)
        {
            if (!IsWarnEnabled)
                return;
            Log(LoggingLevel.Warn, message, null);
        }

        /// <summary>
        /// Logs a warn message.
        /// 
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="message">The message to log</param>
        public void Warn(object message, Exception exception)
        {
            if (!IsWarnEnabled)
                return;
            Log(LoggingLevel.Warn, message, exception);
        }

        public void Warn(LogWriterOutputProvider messageProvider)
        {
            if (!IsWarnEnabled)
                return;

            object obj = messageProvider();

            LogInternal(LoggingLevel.Warn, obj, null);
        }

        /// <summary>
        /// Logs a warn message.
        /// 
        /// </summary>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void WarnFormat(string format, params object[] args)
        {
            if (!IsWarnEnabled)
                return;
            LogInternal(LoggingLevel.Warn, string.Format(CultureInfo.CurrentCulture, format, args), null);
        }

        /// <summary>
        /// Logs a warn message.
        /// 
        /// </summary>
        /// <param name="formatProvider">The format provider to use</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!IsWarnEnabled)
                return;
            LogInternal(LoggingLevel.Warn, string.Format(formatProvider, format, args), null);
        }

        /// <summary>
        /// Logs an error message.
        /// 
        /// </summary>
        /// <param name="message">The message to log</param>
        public void Error(object message)
        {
            if (!IsErrorEnabled)
                return;
            Log(LoggingLevel.Error, message, null);
        }

        /// <summary>
        /// Logs an error message.
        /// 
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="message">The message to log</param>
        public void Error(object message, Exception exception)
        {
            if (!IsErrorEnabled)
                return;
            Log(LoggingLevel.Error, message, exception);
        }

        public void Error(LogWriterOutputProvider messageProvider)
        {
            if (!IsErrorEnabled)
                return;

            object obj = messageProvider();

            LogInternal(LoggingLevel.Error, obj, null);
        }

        /// <summary>
        /// Logs an error message.
        /// 
        /// </summary>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void ErrorFormat(string format, params object[] args)
        {
            if (!IsErrorEnabled)
                return;
            LogInternal(LoggingLevel.Error, string.Format(CultureInfo.CurrentCulture, format, args), null);
        }

        /// <summary>
        /// Logs an error message.
        /// 
        /// </summary>
        /// <param name="formatProvider">The format provider to use</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!IsErrorEnabled)
                return;
            LogInternal(LoggingLevel.Error, string.Format(formatProvider, format, args), null);
        }

        /// <summary>
        /// Logs a fatal message.
        /// 
        /// </summary>
        /// <param name="message">The message to log</param>
        public void Fatal(object message)
        {
            if (!IsFatalEnabled)
                return;
            Log(LoggingLevel.Fatal, message, null);
        }

        /// <summary>
        /// Logs a fatal message.
        /// 
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="message">The message to log</param>
        public void Fatal(object message, Exception exception)
        {
            if (!IsFatalEnabled)
                return;
            Log(LoggingLevel.Fatal, message, exception);
        }

        public void Fatal(LogWriterOutputProvider messageProvider)
        {
            if (!IsFatalEnabled)
                return;

            object obj = messageProvider();

            LogInternal(LoggingLevel.Fatal, obj, null);
        }

        /// <summary>
        /// Logs a fatal message.
        /// 
        /// </summary>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void FatalFormat(string format, params object[] args)
        {
            if (!IsFatalEnabled)
                return;
            LogInternal(LoggingLevel.Fatal, string.Format(CultureInfo.CurrentCulture, format, args), null);
        }

        /// <summary>
        /// Logs a fatal message.
        /// 
        /// </summary>
        /// <param name="formatProvider">The format provider to use</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!IsFatalEnabled)
                return;
            LogInternal(LoggingLevel.Fatal, string.Format(formatProvider, format, args), null);
        }

        public void Log(LoggingLevel level, object obj)
        {
            if (_level < level)
                return;

            LogInternal(level, obj, null);
        }

        public void Log(LoggingLevel level, object obj, Exception exception)
        {
            if (_level < level)
                return;

            LogInternal(level, obj, exception);
        }

        public void Log(LoggingLevel level, LogWriterOutputProvider messageProvider)
        {
            if (_level < level)
                return;

            object obj = messageProvider();

            LogInternal(level, obj, null);
        }

        public void LogFormat(LoggingLevel level, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (_level < level)
                return;

            LogInternal(level, string.Format(formatProvider, format, args), null);
        }

        /// <summary>
        /// Logs a debug message.
        /// 
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void DebugFormat(Exception exception, string format, params object[] args)
        {
            if (!IsDebugEnabled)
                return;
            LogInternal(LoggingLevel.Debug, string.Format(CultureInfo.CurrentCulture, format, args), exception);
        }

        /// <summary>
        /// Logs a debug message.
        /// 
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="formatProvider">The format provider to use</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void DebugFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!IsDebugEnabled)
                return;
            LogInternal(LoggingLevel.Debug, string.Format(formatProvider, format, args), exception);
        }

        /// <summary>
        /// Logs an info message.
        /// 
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void InfoFormat(Exception exception, string format, params object[] args)
        {
            if (!IsInfoEnabled)
                return;
            LogInternal(LoggingLevel.Info, string.Format(CultureInfo.CurrentCulture, format, args), exception);
        }

        /// <summary>
        /// Logs an info message.
        /// 
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="formatProvider">The format provider to use</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void InfoFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!IsInfoEnabled)
                return;
            LogInternal(LoggingLevel.Info, string.Format(formatProvider, format, args), exception);
        }

        /// <summary>
        /// Logs a warn message.
        /// 
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void WarnFormat(Exception exception, string format, params object[] args)
        {
            if (!IsWarnEnabled)
                return;
            LogInternal(LoggingLevel.Warn, string.Format(CultureInfo.CurrentCulture, format, args), exception);
        }

        /// <summary>
        /// Logs a warn message.
        /// 
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="formatProvider">The format provider to use</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void WarnFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!IsWarnEnabled)
                return;
            LogInternal(LoggingLevel.Warn, string.Format(formatProvider, format, args), exception);
        }

        /// <summary>
        /// Logs an error message.
        /// 
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void ErrorFormat(Exception exception, string format, params object[] args)
        {
            if (!IsErrorEnabled)
                return;
            LogInternal(LoggingLevel.Error, string.Format(CultureInfo.CurrentCulture, format, args), exception);
        }

        /// <summary>
        /// Logs an error message.
        /// 
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="formatProvider">The format provider to use</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void ErrorFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!IsErrorEnabled)
                return;
            LogInternal(LoggingLevel.Error, string.Format(formatProvider, format, args), exception);
        }

        /// <summary>
        /// Logs a fatal message.
        /// 
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void FatalFormat(Exception exception, string format, params object[] args)
        {
            if (!IsFatalEnabled)
                return;
            LogInternal(LoggingLevel.Fatal, string.Format(CultureInfo.CurrentCulture, format, args), exception);
        }

        /// <summary>
        /// Logs a fatal message.
        /// 
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="formatProvider">The format provider to use</param>
        /// <param name="format">Format string for the message to log</param>
        /// <param name="args">Format arguments for the message to log</param>
        public void FatalFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!IsFatalEnabled)
                return;
            LogInternal(LoggingLevel.Fatal, string.Format(formatProvider, format, args), exception);
        }

        void LogInternal(LoggingLevel level, object obj, Exception exception)
        {
            string message = obj == null
                                 ? ""
                                 : obj.ToString();

            if (exception == null)
                _source.TraceEvent(level.TraceEventType, 0, message);
            else
                _source.TraceData(level.TraceEventType, 0, (object)message, (object)exception);
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Logging\TraceLogWriterFactory.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Logging
{
    using System;
    using System.Diagnostics;
    using Internals.Caching;

    internal class TraceLogWriterFactory :
        LogWriterFactory
    {
        readonly Cache<string, TraceLogWriter> _logs;
        readonly Cache<string, TraceSource> _sources;
        TraceListener _listener;
        readonly TraceSource _defaultSource;

        public TraceLogWriterFactory()
        {
            _logs = new DictionaryCache<string, TraceLogWriter>(CreateTraceLog);
            _sources = new DictionaryCache<string, TraceSource>(CreateTraceSource);

            _defaultSource = new TraceSource("Default", SourceLevels.Information);
            
            _listener = AddDefaultConsoleTraceListener(_defaultSource);

            _sources.Get("Topshelf");
        }

        public LogWriter Get(string name)
        {
            return _logs[name];
        }

        public void Shutdown()
        {
            Trace.Flush();

            if (_listener != null)
            {
                Trace.Listeners.Remove(_listener);

                _listener.Close();
                (_listener as IDisposable).Dispose();
                _listener = null;
            }
        }

        static TraceListener AddDefaultConsoleTraceListener(TraceSource source)
        {
            var listener = new TopshelfConsoleTraceListener();
            listener.Name = "Topshelf";

            source.Listeners.Add(listener);

            return listener;
        }

        TraceLogWriter CreateTraceLog(string name)
        {
            return new TraceLogWriter(_sources[name]);
        }

        TraceSource CreateTraceSource(string name)
        {
            LoggingLevel logLevel = LoggingLevel.Info;
            SourceLevels sourceLevel = logLevel.SourceLevel;
            var source = new TraceSource(name, sourceLevel);
            if (IsSourceConfigured(source))
            {
                return source;
            }

            ConfigureTraceSource(source, name, sourceLevel);

            return source;
        }

        void ConfigureTraceSource(TraceSource source, string name, SourceLevels sourceLevel)
        {
            var defaultSource = _defaultSource;

            for (string parentName = ShortenName(name);
                 !string.IsNullOrEmpty(parentName);
                 parentName = ShortenName(parentName))
            {
                var parentSource = _sources.Get(parentName, key => new TraceSource(key, sourceLevel));
                if (IsSourceConfigured(parentSource))
                {
                    defaultSource = parentSource;
                    break;
                }
            }

            source.Switch = defaultSource.Switch;
            source.Listeners.Clear();
            foreach (TraceListener listener in defaultSource.Listeners)
                source.Listeners.Add(listener);
        }

        static bool IsSourceConfigured(TraceSource source)
        {
            return source.Listeners.Count != 1
                   || !(source.Listeners[0] is DefaultTraceListener)
                   || source.Listeners[0].Name != "Default";
        }

        static string ShortenName(string name)
        {
            int length = name.LastIndexOf('.');

            return length != -1
                       ? name.Substring(0, length)
                       : null;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Runtime\EventCallbackList.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Runtime
{
    using System;
    using System.Collections.Generic;

    internal class EventCallbackList<T>
    {
        readonly IList<Action<T>> _callbacks;

        public EventCallbackList()
        {
            _callbacks = new List<Action<T>>();
        }

        public void Add(Action<T> callback)
        {
            _callbacks.Add(callback);
        }

        public void Notify(T data)
        {
            foreach (var callback in _callbacks)
            {
                callback(data);
            }
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Runtime\HostEnvironment.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Runtime
{
    using System;

    /// <summary>
    /// Abstracts the environment in which the host in running (different OS versions, platforms,
    /// bitness, etc.)
    /// </summary>
    internal interface HostEnvironment
    {
        string CommandLine { get; }

        /// <summary>
        /// Determines if the service is running as an administrator
        /// </summary>
        bool IsAdministrator { get; }

        /// <summary>
        /// Determines if the process is running as a service
        /// </summary>
        bool IsRunningAsAService { get; }

        /// <summary>
        /// Determines if the service is installed
        /// </summary>
        /// <param name="serviceName">The name of the service as it is registered</param>
        /// <returns>True if the service is installed, otherwise false</returns>
        bool IsServiceInstalled(string serviceName);

        /// <summary>
        /// Start the service using operating system controls
        /// </summary>
        /// <param name="serviceName">The name of the service</param>
        void StartService(string serviceName);

        /// <summary>
        /// Stop the service using operating system controls
        /// </summary>
        /// <param name="serviceName"></param>
        void StopService(string serviceName);

        /// <summary>
        /// Install the service using the settings provided
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="beforeInstall"> </param>
        /// <param name="afterInstall"> </param>
        void InstallService(InstallHostSettings settings, Action beforeInstall, Action afterInstall);
        
        /// <summary>
        /// Uninstall the service using the settings provided
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="beforeUninstall"></param>
        /// <param name="afterUninstall"></param>
        void UninstallService(HostSettings settings, Action beforeUninstall, Action afterUninstall);

        /// <summary>
        /// Restarts the service as an administrator which has permission to modify the service configuration
        /// </summary>
        /// <returns>True if the child process was executed, otherwise false</returns>
        bool RunAsAdministrator();

        /// <summary>
        /// Create a service host appropriate for the host environment
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="serviceHandle"></param>
        /// <returns></returns>
        Host CreateServiceHost(HostSettings settings, ServiceHandle serviceHandle);
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Runtime\HostSettings.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Runtime
{
    /// <summary>
    ///   The settings that have been configured for the operating system service
    /// </summary>
    internal interface HostSettings
    {
        /// <summary>
        ///   The name of the service
        /// </summary>
        string Name { get; }

        /// <summary>
        ///   The name of the service as it should be displayed in the service control manager
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        ///   The description of the service that is displayed in the service control manager
        /// </summary>
        string Description { get; }

        /// <summary>
        ///   The service instance name that should be used when the service is registered
        /// </summary>
        string InstanceName { get; }

        /// <summary>
        ///   Returns the Windows service name, including the instance name, which is registered with the SCM Example: myservice$bob
        /// </summary>
        /// <returns> </returns>
        string ServiceName { get; }

        /// <summary>
        ///   True if the service supports pause and continue
        /// </summary>
        bool CanPauseAndContinue { get; }

        /// <summary>
        ///   True if the service can handle the shutdown event
        /// </summary>
        bool CanShutdown { get; }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Runtime\HostStartMode.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Runtime
{
    internal enum HostStartMode
    {
        Automatic = 0,
        Manual = 1,
        Disabled = 2,
        AutomaticDelayed = 3,
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Runtime\InstallHostSettings.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Runtime
{
    using System.ServiceProcess;

    internal interface InstallHostSettings :
        HostSettings
    {
        ServiceAccount Account { get; }
        
        string Username { get; }
        string Password { get;}

        string[] Dependencies { get; }
        
        HostStartMode StartMode { get; }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Runtime\ServiceEvents.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Runtime
{
    internal interface ServiceEvents
    {
        void BeforeStart(HostControl hostControl);
        void AfterStart(HostControl hostControl);
        void BeforeStop(HostControl hostControl);
        void AfterStop(HostControl hostControl);
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Runtime\ServiceEventsImpl.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Runtime
{
    using System;

    internal class ServiceEventsImpl :
        ServiceEvents
    {
        readonly EventCallbackList<HostStartedContext> _afterStart;
        readonly EventCallbackList<HostStoppedContext> _afterStop;
        readonly EventCallbackList<HostStartContext> _beforeStart;
        readonly EventCallbackList<HostStopContext> _beforeStop;

        public ServiceEventsImpl()
        {
            _afterStart = new EventCallbackList<HostStartedContext>();
            _afterStop = new EventCallbackList<HostStoppedContext>();
            _beforeStart = new EventCallbackList<HostStartContext>();
            _beforeStop = new EventCallbackList<HostStopContext>();
        }

        public void BeforeStart(HostControl hostControl)
        {
            var context = new HostStartContextImpl(hostControl);

            _beforeStart.Notify(context);
        }

        public void AfterStart(HostControl hostControl)
        {
            var context = new HostStartedContextImpl(hostControl);

            _afterStart.Notify(context);
        }

        public void BeforeStop(HostControl hostControl)
        {
            var context = new HostStopContextImpl(hostControl);

            _beforeStop.Notify(context);
        }

        public void AfterStop(HostControl hostControl)
        {
            var context = new HostStoppedContextImpl(hostControl);

            _afterStop.Notify(context);
        }

        public void AddBeforeStart(Action<HostStartContext> callback)
        {
            _beforeStart.Add(callback);
        }

        public void AddAfterStart(Action<HostStartedContext> callback)
        {
            _afterStart.Add(callback);
        }

        public void AddBeforeStop(Action<HostStopContext> callback)
        {
            _beforeStop.Add(callback);
        }

        public void AddAfterStop(Action<HostStoppedContext> callback)
        {
            _afterStop.Add(callback);
        }

        abstract class ContextImpl
        {
            readonly HostControl _hostControl;

            public ContextImpl(HostControl hostControl)
            {
                _hostControl = hostControl;
            }

            public void RequestAdditionalTime(TimeSpan timeRemaining)
            {
                _hostControl.RequestAdditionalTime(timeRemaining);
            }

            public void Stop()
            {
                _hostControl.Stop();
            }

            public void Restart()
            {
                _hostControl.Restart();
            }
        }

        class HostStartContextImpl :
            ContextImpl,
            HostStartContext
        {
            public HostStartContextImpl(HostControl hostControl)
                : base(hostControl)
            {
            }

            public void CancelStart()
            {
            }
        }

        class HostStartedContextImpl :
            ContextImpl,
            HostStartedContext
        {
            public HostStartedContextImpl(HostControl hostControl)
                : base(hostControl)
            {
            }
        }

        class HostStopContextImpl :
            ContextImpl,
            HostStopContext
        {
            public HostStopContextImpl(HostControl hostControl)
                : base(hostControl)
            {
            }
        }

        class HostStoppedContextImpl :
            ContextImpl,
            HostStoppedContext
        {
            public HostStoppedContextImpl(HostControl hostControl)
                : base(hostControl)
            {
            }
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Runtime\ServiceFactory.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Runtime
{
    internal delegate T ServiceFactory<T>(HostSettings settings)
        where T : class;
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Runtime\ServiceHandle.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Runtime
{
    using System;

    /// <summary>
    /// A handle to a service being hosted by the Host
    /// </summary>
    internal interface ServiceHandle :
        IDisposable
    {
        /// <summary>
        /// Start the service
        /// </summary>
        /// <param name="hostControl"></param>
        /// <returns>True if the service was started, otherwise false</returns>
        bool Start(HostControl hostControl);

        /// <summary>
        /// Pause the service
        /// </summary>
        /// <param name="hostControl"></param>
        /// <returns>True if the service was paused, otherwise false</returns>
        bool Pause(HostControl hostControl);

        /// <summary>
        /// Continue the service from a paused state
        /// </summary>
        /// <param name="hostControl"></param>
        /// <returns>True if the service was able to continue, otherwise false</returns>
        bool Continue(HostControl hostControl);

        /// <summary>
        /// Stop the service
        /// </summary>
        /// <param name="hostControl"></param>
        /// <returns>True if the service was stopped, or false if the service cannot be stopped at this time</returns>
        bool Stop(HostControl hostControl);

        /// <summary>
        /// Handle the shutdown event
        /// </summary>
        /// <param name="hostControl"></param>
        void Shutdown(HostControl hostControl);
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Runtime\Windows\HostInstaller.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Runtime.Windows
{
    using System.Collections;
    using System.Configuration.Install;
    using Logging;
    using Microsoft.Win32;

    internal class HostInstaller :
        Installer
    {
        static readonly LogWriter _log = HostLogger.Get<HostInstaller>();

        readonly string _arguments;
        readonly Installer[] _installers;
        readonly HostSettings _settings;

        public HostInstaller(HostSettings settings, string arguments, Installer[] installers)
        {
            _installers = installers;
            _arguments = arguments;
            _settings = settings;
        }

        public override void Install(IDictionary stateSaver)
        {
            Installers.AddRange(_installers);

            if (_log.IsInfoEnabled)
                _log.InfoFormat("Installing {0} service", _settings.DisplayName);

            base.Install(stateSaver);

            if (_log.IsDebugEnabled)
                _log.Debug("Opening Registry");

            using (RegistryKey system = Registry.LocalMachine.OpenSubKey("System"))
            using (RegistryKey currentControlSet = system.OpenSubKey("CurrentControlSet"))
            using (RegistryKey services = currentControlSet.OpenSubKey("Services"))
            using (RegistryKey service = services.OpenSubKey(_settings.ServiceName, true))
            {
                service.SetValue("Description", _settings.Description);

                var imagePath = (string)service.GetValue("ImagePath");

                _log.DebugFormat("Service path: {0}", imagePath);

                imagePath += _arguments;

                _log.DebugFormat("Image path: {0}", imagePath);

                service.SetValue("ImagePath", imagePath);
            }

            if (_log.IsDebugEnabled)
                _log.Debug("Closing Registry");
        }

        public override void Uninstall(IDictionary savedState)
        {
            Installers.AddRange(_installers);
            if (_log.IsInfoEnabled)
                _log.InfoFormat("Uninstalling {0} service", _settings.Name);

            base.Uninstall(savedState);
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Runtime\Windows\HostServiceInstaller.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Runtime.Windows
{
    using System;
    using System.Collections;
    using System.Configuration.Install;
    using System.IO;
    using System.Reflection;
    using System.ServiceProcess;

    internal class HostServiceInstaller :
        IDisposable
    {
        readonly Installer _installer;
        readonly TransactedInstaller _transactedInstaller;

        public HostServiceInstaller(InstallHostSettings settings)
        {
            _installer = CreateInstaller(settings);

            _transactedInstaller = CreateTransactedInstaller(_installer);
        }

        public HostServiceInstaller(HostSettings settings)
        {
            _installer = CreateInstaller(settings);

            _transactedInstaller = CreateTransactedInstaller(_installer);
        }

        public void Dispose()
        {
            try
            {
                _transactedInstaller.Dispose();
            }
            finally
            {
                _installer.Dispose();
            }
        }

        public void InstallService(Action<InstallEventArgs> beforeInstall, Action<InstallEventArgs> afterInstall)
        {
            if (beforeInstall != null)
                _installer.BeforeInstall += (sender, args) => beforeInstall(args);
            if (afterInstall != null)
                _installer.AfterInstall += (sender, args) => afterInstall(args);

            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            _transactedInstaller.Install(new Hashtable());
        }

        public void UninstallService(Action<InstallEventArgs> beforeUninstall, Action<InstallEventArgs> afterUninstall)
        {
            if (beforeUninstall != null)
                _installer.BeforeUninstall += (sender, args) => beforeUninstall(args);
            if (afterUninstall != null)
                _installer.AfterUninstall += (sender, args) => afterUninstall(args);

            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            _transactedInstaller.Uninstall(null);
        }

        static Installer CreateInstaller(InstallHostSettings settings)
        {
            var installers = new Installer[]
                {
                    ConfigureServiceInstaller(settings, settings.Dependencies, settings.StartMode),
                    ConfigureServiceProcessInstaller(settings.Account, settings.Username, settings.Password)
                };

            return CreateHostInstaller(settings, installers);
        }

        Installer CreateInstaller(HostSettings settings)
        {
            var installers = new Installer[]
                {
                    ConfigureServiceInstaller(settings, new string[] {}, HostStartMode.Automatic),
                    ConfigureServiceProcessInstaller(ServiceAccount.LocalService, "", ""),
                };

            return CreateHostInstaller(settings, installers);
        }

        static Installer CreateHostInstaller(HostSettings settings, Installer[] installers)
        {
            string arguments = " ";

            if (!string.IsNullOrEmpty(settings.InstanceName))
                arguments += string.Format(" -instance \"{0}\"", settings.InstanceName);

            if (!string.IsNullOrEmpty(settings.DisplayName))
                arguments += string.Format(" -displayname \"{0}\"", settings.DisplayName);

            if (!string.IsNullOrEmpty(settings.Name))
                arguments += string.Format(" -servicename \"{0}\"", settings.Name);

            return new HostInstaller(settings, arguments, installers);
        }

        static TransactedInstaller CreateTransactedInstaller(Installer installer)
        {
            var transactedInstaller = new TransactedInstaller();

            transactedInstaller.Installers.Add(installer);

            Assembly assembly = Assembly.GetEntryAssembly();

            if (assembly == null)
                throw new TopshelfException("Assembly.GetEntryAssembly() is null for some reason.");

            string path = string.Format("/assemblypath={0}", assembly.Location);
            string[] commandLine = {path};

            var context = new InstallContext(null, commandLine);
            transactedInstaller.Context = context;

            return transactedInstaller;
        }

        static ServiceInstaller ConfigureServiceInstaller(HostSettings settings, string[] dependencies,
            HostStartMode startMode)
        {
            var installer = new ServiceInstaller
                {
                    ServiceName = settings.ServiceName,
                    Description = settings.Description,
                    DisplayName = settings.DisplayName,
                    ServicesDependedOn = dependencies
                };

            SetStartMode(installer, startMode);

            return installer;
        }

        static void SetStartMode(ServiceInstaller installer, HostStartMode startMode)
        {
            switch (startMode)
            {
                case HostStartMode.Automatic:
                    installer.StartType = ServiceStartMode.Automatic;
                    break;

                case HostStartMode.Manual:
                    installer.StartType = ServiceStartMode.Manual;
                    break;

                case HostStartMode.Disabled:
                    installer.StartType = ServiceStartMode.Disabled;
                    break;

                case HostStartMode.AutomaticDelayed:
                    installer.StartType = ServiceStartMode.Automatic;
#if !NET35
                    installer.DelayedAutoStart = true;
#endif
                    break;
            }
        }

        static ServiceProcessInstaller ConfigureServiceProcessInstaller(ServiceAccount account, string username,
            string password)
        {
            var installer = new ServiceProcessInstaller
                {
                    Username = username,
                    Password = password,
                    Account = account,
                };

            return installer;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Runtime\Windows\Kernel32.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Runtime.Windows
{
    using System;
    using System.Runtime.InteropServices;

    static class Kernel32
    {
        public static uint TH32CS_SNAPPROCESS = 2;

        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESSENTRY32
        {
            public uint dwSize;
            public uint cntUsage;
            public uint th32ProcessID;
            public IntPtr th32DefaultHeapID;
            public uint th32ModuleID;
            public uint cntThreads;
            public uint th32ParentProcessID;
            public int pcPriClassBase;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szExeFile;
        } ;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

        [DllImport("kernel32.dll")]
        public static extern bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("kernel32.dll")]
        public static extern bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Runtime\Windows\NativeMethods.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Runtime.Windows
{
    using System;
    using System.Runtime.InteropServices;

    internal class NativeMethods
    {
        [Flags]
        internal enum SCM_ACCESS : uint
        {
            /// <summary>
            /// Required to connect to the service control manager.
            /// </summary>
            SC_MANAGER_CONNECT = 0x00001,

            /// <summary>
            /// Required to call the CreateService function to create a service
            /// object and add it to the database.
            /// </summary>
            SC_MANAGER_CREATE_SERVICE = 0x00002,

            /// <summary>
            /// Required to call the EnumServicesStatusEx function to list the 
            /// services that are in the database.
            /// </summary>
            SC_MANAGER_ENUMERATE_SERVICE = 0x00004,

            /// <summary>
            /// Required to call the LockServiceDatabase function to acquire a 
            /// lock on the database.
            /// </summary>
            SC_MANAGER_LOCK = 0x00008,

            /// <summary>
            /// Required to call the QueryServiceLockStatus function to retrieve 
            /// the lock status information for the database.
            /// </summary>
            SC_MANAGER_QUERY_LOCK_STATUS = 0x00010,

            /// <summary>
            /// Required to call the NotifyBootConfigStatus function.
            /// </summary>
            SC_MANAGER_MODIFY_BOOT_CONFIG = 0x00020,

            /// <summary>
            /// Includes STANDARD_RIGHTS_REQUIRED, in addition to all access 
            /// rights in this table.
            /// </summary>
            SC_MANAGER_ALL_ACCESS = ACCESS_MASK.STANDARD_RIGHTS_REQUIRED |
                SC_MANAGER_CONNECT |
                SC_MANAGER_CREATE_SERVICE |
                SC_MANAGER_ENUMERATE_SERVICE |
                SC_MANAGER_LOCK |
                SC_MANAGER_QUERY_LOCK_STATUS |
                SC_MANAGER_MODIFY_BOOT_CONFIG,

            GENERIC_READ = ACCESS_MASK.STANDARD_RIGHTS_READ |
                SC_MANAGER_ENUMERATE_SERVICE |
                SC_MANAGER_QUERY_LOCK_STATUS,

            GENERIC_WRITE = ACCESS_MASK.STANDARD_RIGHTS_WRITE |
                SC_MANAGER_CREATE_SERVICE |
                SC_MANAGER_MODIFY_BOOT_CONFIG,

            GENERIC_EXECUTE = ACCESS_MASK.STANDARD_RIGHTS_EXECUTE |
                SC_MANAGER_CONNECT | SC_MANAGER_LOCK,

            GENERIC_ALL = SC_MANAGER_ALL_ACCESS,
        }

        [Flags]
        internal enum ACCESS_MASK : uint
        {
            DELETE = 0x00010000,
            READ_CONTROL = 0x00020000,
            WRITE_DAC = 0x00040000,
            WRITE_OWNER = 0x00080000,
            SYNCHRONIZE = 0x00100000,

            STANDARD_RIGHTS_REQUIRED = 0x000f0000,

            STANDARD_RIGHTS_READ = 0x00020000,
            STANDARD_RIGHTS_WRITE = 0x00020000,
            STANDARD_RIGHTS_EXECUTE = 0x00020000,

            STANDARD_RIGHTS_ALL = 0x001f0000,

            SPECIFIC_RIGHTS_ALL = 0x0000ffff,

            ACCESS_SYSTEM_SECURITY = 0x01000000,

            MAXIMUM_ALLOWED = 0x02000000,

            GENERIC_READ = 0x80000000,
            GENERIC_WRITE = 0x40000000,
            GENERIC_EXECUTE = 0x20000000,
            GENERIC_ALL = 0x10000000,

            DESKTOP_READOBJECTS = 0x00000001,
            DESKTOP_CREATEWINDOW = 0x00000002,
            DESKTOP_CREATEMENU = 0x00000004,
            DESKTOP_HOOKCONTROL = 0x00000008,
            DESKTOP_JOURNALRECORD = 0x00000010,
            DESKTOP_JOURNALPLAYBACK = 0x00000020,
            DESKTOP_ENUMERATE = 0x00000040,
            DESKTOP_WRITEOBJECTS = 0x00000080,
            DESKTOP_SWITCHDESKTOP = 0x00000100,

            WINSTA_ENUMDESKTOPS = 0x00000001,
            WINSTA_READATTRIBUTES = 0x00000002,
            WINSTA_ACCESSCLIPBOARD = 0x00000004,
            WINSTA_CREATEDESKTOP = 0x00000008,
            WINSTA_WRITEATTRIBUTES = 0x00000010,
            WINSTA_ACCESSGLOBALATOMS = 0x00000020,
            WINSTA_EXITWINDOWS = 0x00000040,
            WINSTA_ENUMERATE = 0x00000100,
            WINSTA_READSCREEN = 0x00000200,

            WINSTA_ALL_ACCESS = 0x0000037f
        }

        [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode,
            SetLastError = true)]
        public static extern IntPtr OpenSCManager(string machineName, string databaseName, uint dwAccess);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseServiceHandle(IntPtr hSCObject);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, uint dwDesiredAccess);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool ChangeServiceConfig2(IntPtr serviceHandle, uint infoLevel,
            IntPtr lpInfo);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct SERVICE_FAILURE_ACTIONS
        {
            public int dwResetPeriod;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpRebootMsg;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpCommand;
            public int cActions;
            public IntPtr actions;
        }

        internal enum SC_ACTION_TYPE
        {
            None = 0,
            RestartService = 1,
            RebootComputer = 2,
            RunCommand = 3
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct SC_ACTION
        {
            public int Type;
            public int Delay;
        }

        public const int SERVICE_CONFIG_FAILURE_ACTIONS = 2;
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Runtime\Windows\RestartServiceRecoveryAction.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Runtime.Windows
{
    internal class RestartServiceRecoveryAction :
        ServiceRecoveryAction
    {
        public RestartServiceRecoveryAction(int delay)
            : base(delay)
        {
        }

        public override NativeMethods.SC_ACTION GetAction()
        {
            return new NativeMethods.SC_ACTION
                {
                    Delay = Delay,
                    Type = (int)NativeMethods.SC_ACTION_TYPE.RestartService,
                };
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Runtime\Windows\RestartSystemRecoveryAction.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Runtime.Windows
{
    internal class RestartSystemRecoveryAction :
        ServiceRecoveryAction
    {
        public RestartSystemRecoveryAction(int delay, string restartMessage)
            : base(delay)
        {
            RestartMessage = restartMessage;
        }

        public string RestartMessage { get; private set; }

        public override NativeMethods.SC_ACTION GetAction()
        {
            return new NativeMethods.SC_ACTION
                {
                    Delay = Delay,
                    Type = (int)NativeMethods.SC_ACTION_TYPE.RebootComputer,
                };
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Runtime\Windows\RunProgramRecoveryAction.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Runtime.Windows
{
    internal class RunProgramRecoveryAction :
        ServiceRecoveryAction
    {
        public RunProgramRecoveryAction(int delay, string command)
            : base(delay)
        {
            Command = command;
        }

        public string Command { get; private set; }

        public override NativeMethods.SC_ACTION GetAction()
        {
            return new NativeMethods.SC_ACTION
                {
                    Delay = Delay,
                    Type = (int)NativeMethods.SC_ACTION_TYPE.RunCommand,
                };
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Runtime\Windows\ServiceRecoveryAction.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Runtime.Windows
{
    internal abstract class ServiceRecoveryAction
    {
        protected ServiceRecoveryAction(int delay)
        {
            Delay = delay;
        }

        public int Delay { get; private set; }

        public abstract NativeMethods.SC_ACTION GetAction();
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Runtime\Windows\ServiceRecoveryOptions.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Runtime.Windows
{
    using System.Collections.Generic;

    internal class ServiceRecoveryOptions
    {
        readonly IList<ServiceRecoveryAction> _actions;

        public ServiceRecoveryOptions()
        {
            _actions = new List<ServiceRecoveryAction>();
        }

        public int ResetPeriod { get; set; }

        public IEnumerable<ServiceRecoveryAction> Actions
        {
            get { return _actions; }
        }

        public void AddAction(ServiceRecoveryAction serviceRecoveryAction)
        {
            _actions.Add(serviceRecoveryAction);
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Runtime\Windows\WindowsHostEnvironment.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Runtime.Windows
{
    using System;
    using System.ComponentModel;
    using System.Configuration.Install;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.ServiceProcess;
    using System.Threading;
    using Logging;

    internal class WindowsHostEnvironment :
        HostEnvironment
    {
        readonly LogWriter _log = HostLogger.Get(typeof(WindowsHostEnvironment));

        public bool IsServiceInstalled(string serviceName)
        {
            return ServiceController.GetServices()
                .Any(service => string.CompareOrdinal(service.ServiceName, serviceName) == 0);
        }

        public void StartService(string serviceName)
        {
            using (var sc = new ServiceController(serviceName))
            {
                if (sc.Status == ServiceControllerStatus.Running)
                {
                    _log.InfoFormat("The {0} service is already running.", serviceName);
                    return;
                }

                if (sc.Status == ServiceControllerStatus.StartPending)
                {
                    _log.InfoFormat("The {0} service is already starting.", serviceName);
                    return;
                }

                sc.Start();
                while (sc.Status == ServiceControllerStatus.Stopped || sc.Status == ServiceControllerStatus.StartPending)
                {
                    Thread.Sleep(500);
                    sc.Refresh();
                }
            }
        }

        public void StopService(string serviceName)
        {
            using (var sc = new ServiceController(serviceName))
            {
                if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    _log.InfoFormat("The {0} service is not running.", serviceName);
                    return;
                }

                if (sc.Status == ServiceControllerStatus.StopPending)
                {
                    _log.InfoFormat("The {0} service is already stopping.", serviceName);
                    return;
                }

                sc.Stop();
                while (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StopPending)
                {
                    Thread.Sleep(500);
                    sc.Refresh();
                }
            }
        }

        public string CommandLine
        {
            get { return CommandLineParser.CommandLine.GetUnparsedCommandLine(); }
        }

        public bool IsAdministrator
        {
            get
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();

                if (null != identity)
                {
                    var principal = new WindowsPrincipal(identity);

                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }

                return false;
            }
        }

        public bool IsRunningAsAService
        {
            get
            {
                try
                {
                    Process process = GetParent(Process.GetCurrentProcess());
                    if (process != null && process.ProcessName == "services")
                    {
                        _log.Debug("Started by the Windows services process");
                        return true;
                    }
                }
                catch (InvalidOperationException)
                {
                    // again, mono seems to fail with this, let's just return false okay?
                }
                return false;
            }
        }

        public bool RunAsAdministrator()
        {
            if (Environment.OSVersion.Version.Major == 6)
            {
                string commandLine = CommandLine.Replace("--sudo", "");

                var startInfo = new ProcessStartInfo(Assembly.GetEntryAssembly().Location, commandLine)
                    {
                        Verb = "runas",
                        UseShellExecute = true,
                        CreateNoWindow = true,
                    };

                try
                {
                    HostLogger.Shutdown();

                    Process process = Process.Start(startInfo);
                    process.WaitForExit();

                    return true;
                }
                catch (Win32Exception ex)
                {
                    _log.Debug("Process Start Exception", ex);
                }
            }

            return false;
        }

        public Host CreateServiceHost(HostSettings settings, ServiceHandle serviceHandle)
        {
            return new WindowsServiceHost(this, settings, serviceHandle);
        }

        public void InstallService(InstallHostSettings settings, Action beforeInstall, Action afterInstall)
        {
            using (var installer = new HostServiceInstaller(settings))
            {
                Action<InstallEventArgs> before = x =>
                    {
                        if (beforeInstall != null)
                            beforeInstall();
                    };

                Action<InstallEventArgs> after = x =>
                    {
                        if (afterInstall != null)
                            afterInstall();
                    };

                installer.InstallService(before, after);
            }
        }

        public void UninstallService(HostSettings settings, Action beforeUninstall, Action afterUninstall)
        {
            using (var installer = new HostServiceInstaller(settings))
            {
                Action<InstallEventArgs> before = x =>
                    {
                        if (beforeUninstall != null)
                            beforeUninstall();
                    };

                Action<InstallEventArgs> after = x =>
                    {
                        if (afterUninstall != null)
                            afterUninstall();
                    };

                installer.UninstallService(before, after);
            }
        }


        Process GetParent(Process child)
        {
            if (child == null)
                throw new ArgumentNullException("child");

            try
            {
                int parentPid = 0;

                IntPtr hnd = Kernel32.CreateToolhelp32Snapshot(Kernel32.TH32CS_SNAPPROCESS, 0);

                if (hnd == IntPtr.Zero)
                    return null;

                var processInfo = new Kernel32.PROCESSENTRY32
                    {
                        dwSize = (uint)Marshal.SizeOf(typeof(Kernel32.PROCESSENTRY32))
                    };

                if (Kernel32.Process32First(hnd, ref processInfo) == false)
                    return null;

                do
                {
                    if (child.Id == processInfo.th32ProcessID)
                        parentPid = (int)processInfo.th32ParentProcessID;
                }
                while (parentPid == 0 && Kernel32.Process32Next(hnd, ref processInfo));

                if (parentPid > 0)
                    return Process.GetProcessById(parentPid);
            }
            catch (Exception ex)
            {
                _log.Error("Unable to get parent process (ignored)", ex);
            }
            return null;
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Runtime\Windows\WindowsHostEnvironmentBuilder.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Runtime.Windows
{
    using Builders;

    internal class WindowsHostEnvironmentBuilder :
        EnvironmentBuilder
    {
        public HostEnvironment Build()
        {
            return new WindowsHostEnvironment();
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Runtime\Windows\WindowsHostSettings.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Runtime.Windows
{
    using System;

    [Serializable]
    internal class WindowsHostSettings :
        HostSettings
    {
        public const string InstanceSeparator = "$";
        string _description;
        string _displayName;

        /// <summary>
        ///   Creates a new WindowsServiceDescription using empty strings for the properties. The class is required to have names by the consumers.
        /// </summary>
        public WindowsHostSettings()
            : this(string.Empty, string.Empty)
        {
        }

        /// <summary>
        ///   Creates a new WindowsServiceDescription instance using the passed parameters.
        /// </summary>
        /// <param name="name"> </param>
        /// <param name="instanceName"> </param>
        public WindowsHostSettings(string name, string instanceName)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (instanceName == null)
                throw new ArgumentNullException("instanceName");

            Name = name;
            InstanceName = instanceName;

            _displayName = "";
            _description = "";
        }

        public string Name { get; set; }

        public string DisplayName
        {
            get
            {
                string displayName = string.IsNullOrEmpty(_displayName)
                                         ? Name
                                         : _displayName;

                if (!string.IsNullOrEmpty(InstanceName))
                    return string.Format("{0} (Instance: {1})", displayName, InstanceName);

                return displayName;
            }
            set { _displayName = value; }
        }


        public string Description
        {
            get
            {
                return string.IsNullOrEmpty(_description)
                           ? DisplayName
                           : _description;
            }
            set { _description = value; }
        }


        public string InstanceName { get; set; }

        public string ServiceName
        {
            get
            {
                return string.IsNullOrEmpty(InstanceName)
                           ? Name
                           : Name + InstanceSeparator + InstanceName;
            }
        }

        public bool CanPauseAndContinue { get; set; }

        public bool CanShutdown { get; set; }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Runtime\Windows\WindowsServiceHost.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Runtime.Windows
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.ServiceProcess;
    using System.Threading;
    using Logging;

    internal class WindowsServiceHost :
        ServiceBase,
        Host,
        HostControl
    {
        static readonly LogWriter _log = HostLogger.Get<WindowsServiceHost>();
        readonly HostEnvironment _environment;
        readonly ServiceHandle _serviceHandle;
        readonly HostSettings _settings;
        int _deadThread;

        public WindowsServiceHost(HostEnvironment environment, HostSettings settings, ServiceHandle serviceHandle)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");
            if (serviceHandle == null)
                throw new ArgumentNullException("serviceHandle");

            _settings = settings;
            _serviceHandle = serviceHandle;
            _environment = environment;

            CanPauseAndContinue = settings.CanPauseAndContinue;
            CanShutdown = settings.CanShutdown;
        }

        public TopshelfExitCode Run()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            AppDomain.CurrentDomain.UnhandledException += CatchUnhandledException;

            _log.Info("Starting as a Windows service");

            if (!_environment.IsServiceInstalled(_settings.ServiceName))
            {
                string message = string.Format("The {0} service has not been installed yet. Please run '{1} install'.",
                    _settings, Assembly.GetEntryAssembly().GetName());
                _log.Fatal(message);

                ExitCode = (int)TopshelfExitCode.ServiceNotInstalled;
                throw new TopshelfException(message);
            }

            _log.Debug("[Topshelf] Starting up as a windows service application");

            Run(this);

            ExitCode = (int)TopshelfExitCode.Ok;
            return TopshelfExitCode.Ok;
        }

        void HostControl.RequestAdditionalTime(TimeSpan timeRemaining)
        {
            _log.DebugFormat("Requesting additional time: {0}", timeRemaining);

            RequestAdditionalTime((int)timeRemaining.TotalMilliseconds);
        }

        void HostControl.Restart()
        {
            _log.Fatal("Restart is not yet implemented");

            throw new NotImplementedException("This is not done yet, so I'm trying");
        }

        void HostControl.Stop()
        {
            if (CanStop)
            {
                _log.Debug("Stop requested by hosted service");
                Stop();
            }
            else
            {
                _log.Debug("Stop requested by hosted service, but service cannot be stopped at this time");
                throw new ServiceControlException("The service cannot be stopped at this time");
            }
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                _log.Info("[Topshelf] Starting");

                _log.DebugFormat("[Topshelf] Arguments: {0}", string.Join(",", args));

                _serviceHandle.Start(this);
            }
            catch (Exception ex)
            {
                _log.Fatal(ex);
                throw;
            }
        }

        protected override void OnStop()
        {
            try
            {
                _log.Info("[Topshelf] Stopping");

                _serviceHandle.Stop(this);
            }
            catch (Exception ex)
            {
                _log.Fatal("The service did not shut down gracefully", ex);
                throw;
            }
            finally
            {
                _log.Info("[Topshelf] Stopped");
            }
        }

        protected override void OnPause()
        {
            try
            {
                _log.Info("[Topshelf] Pausing service");

                _serviceHandle.Pause(this);
            }
            catch (Exception ex)
            {
                _log.Fatal("The service did not shut down gracefully", ex);
                throw;
            }
            finally
            {
                _log.Info("[Topshelf] Paused");
            }
        }

        protected override void OnContinue()
        {
            try
            {
                _log.Info("[Topshelf] Pausing service");

                _serviceHandle.Continue(this);
            }
            catch (Exception ex)
            {
                _log.Fatal("The service did not shut down gracefully", ex);
                throw;
            }
            finally
            {
                _log.Info("[Topshelf] Paused");
            }
        }

        protected override void OnShutdown()
        {
            try
            {
                _log.Info("[Topshelf] Service is being shutdown");

                _serviceHandle.Shutdown(this);
            }
            catch (Exception ex)
            {
                _log.Fatal("The service did not shut down gracefully", ex);
                throw;
            }
            finally
            {
                _log.Info("[Topshelf] Paused");
            }
        }

        void CatchUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _log.Error("The service threw an unhandled exception", (Exception)e.ExceptionObject);

            Stop();

            int deadThreadId = Interlocked.Increment(ref _deadThread);
            Thread.CurrentThread.IsBackground = true;
            Thread.CurrentThread.Name = "Unhandled Exception " + deadThreadId.ToString();
            while (true)
                Thread.Sleep(TimeSpan.FromHours(1));
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Runtime\Windows\WindowsServiceRecoveryController.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Runtime.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    internal class WindowsServiceRecoveryController
    {
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public void SetServiceRecoveryOptions(HostSettings settings, ServiceRecoveryOptions options)
        {
            IntPtr scmHandle = IntPtr.Zero;
            IntPtr serviceHandle = IntPtr.Zero;
            IntPtr lpsaActions = IntPtr.Zero;
            IntPtr lpInfo = IntPtr.Zero;
            try
            {
                List<NativeMethods.SC_ACTION> actions = options.Actions.Select(x => x.GetAction()).ToList();
                if (actions.Count == 0)
                    throw new TopshelfException("Must be at least one failure action configured");

                scmHandle = NativeMethods.OpenSCManager(null, null, (int)NativeMethods.SCM_ACCESS.SC_MANAGER_ALL_ACCESS);
                if (scmHandle == IntPtr.Zero)
                    throw new TopshelfException("Failed to open service control manager");

                serviceHandle = NativeMethods.OpenService(scmHandle, settings.ServiceName,
                    (int)NativeMethods.SCM_ACCESS.SC_MANAGER_ALL_ACCESS);
                if (serviceHandle == IntPtr.Zero)
                    throw new TopshelfException("Failed to open service: " + settings.ServiceName);

                int actionSize = Marshal.SizeOf(typeof(NativeMethods.SC_ACTION));
                lpsaActions = Marshal.AllocHGlobal(actionSize*actions.Count + 1);
                if (lpsaActions == IntPtr.Zero)
                    throw new TopshelfException("Unable to allocate memory for service recovery actions");

                IntPtr nextAction = lpsaActions;
                for (int i = 0; i < actions.Count; i++)
                {
                    Marshal.StructureToPtr(actions[i], nextAction, false);
                    nextAction = (IntPtr)(nextAction.ToInt64() + actionSize);
                }

                var finalAction = new NativeMethods.SC_ACTION();
                finalAction.Type = (int)NativeMethods.SC_ACTION_TYPE.None;
                finalAction.Delay = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;

                Marshal.StructureToPtr(finalAction, nextAction, false);

                string rebootMessage = options.Actions.Where(x => x.GetType() == typeof(RestartSystemRecoveryAction))
                                           .OfType<RestartSystemRecoveryAction>().Select(x => x.RestartMessage).
                                           FirstOrDefault() ?? "";

                string runProgramCommand = options.Actions.Where(x => x.GetType() == typeof(RunProgramRecoveryAction))
                                               .OfType<RunProgramRecoveryAction>().Select(x => x.Command).
                                               FirstOrDefault() ?? "";


                var failureActions = new NativeMethods.SERVICE_FAILURE_ACTIONS();
                failureActions.dwResetPeriod =
                    (int)TimeSpan.FromDays(options.ResetPeriod).TotalSeconds;
                failureActions.lpRebootMsg = rebootMessage;
                failureActions.lpCommand = runProgramCommand;
                failureActions.cActions = actions.Count + 1;
                failureActions.actions = lpsaActions;

                lpInfo = Marshal.AllocHGlobal(Marshal.SizeOf(failureActions));
                if (lpInfo == IntPtr.Zero)
                    throw new TopshelfException("Failed to allocate memory for failure actions");

                Marshal.StructureToPtr(failureActions, lpInfo, false);

                if (!NativeMethods.ChangeServiceConfig2(serviceHandle,
                    NativeMethods.SERVICE_CONFIG_FAILURE_ACTIONS, lpInfo))
                {
                    throw new TopshelfException("Failed to change service recovery options");
                }
            }
            finally
            {
                if (lpInfo != IntPtr.Zero)
                    Marshal.FreeHGlobal(lpInfo);
                if (lpsaActions != IntPtr.Zero)
                    Marshal.FreeHGlobal(lpsaActions);
                if (serviceHandle != IntPtr.Zero)
                    NativeMethods.CloseServiceHandle(serviceHandle);
                if (scmHandle != IntPtr.Zero)
                    NativeMethods.CloseServiceHandle(scmHandle);
            }
        }
    }
}


//------------------------------------------------------------------------------
// File: D:\Development\OSS\Topshelf\src\Topshelf\Runtime\Windows\WindowsUserAccessControl.cs
//------------------------------------------------------------------------------
// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Runtime.Windows
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Security.Principal;
    using Topshelf.Logging;

    internal static class WindowsUserAccessControl
    {
        static readonly LogWriter _log = HostLogger.Get(typeof(WindowsUserAccessControl));

       

     
    }
}
