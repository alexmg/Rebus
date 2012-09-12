using System;
using System.Collections;
using System.Collections.Generic;

namespace Rebus.Configuration
{
    /// <summary>
    /// Very simple and independent container adapter that relies on <see cref="SimpleHandlerActivator"/>
    /// to activate handlers.
    /// </summary>
    public class BuiltinContainerAdapter : IContainerAdapter, IDisposable
    {
        readonly SimpleHandlerActivator handlerActivator = new SimpleHandlerActivator();

        public IBus Bus { get; set; }

        public IAdvancedBus AdvancedBus { get; set; }

        /// <summary>
        /// Registers the given handler type. It is assumed that the type registered has a public
        /// default constructor - otherwise, instantiation will fail.
        /// </summary>
        public BuiltinContainerAdapter Register(Type handlerType)
        {
            handlerActivator.Register(handlerType);
            return this;
        }

        /// <summary>
        /// Registers a factory method that is capable of creating a handler instance.
        /// </summary>
        public BuiltinContainerAdapter Register<THandler>(Func<THandler> handlerFactory)
        {
            handlerActivator.Register(handlerFactory);
            return this;
        }

        /// <summary>
        /// Registers a function that can handle messages of the specified type.
        /// </summary>
        public BuiltinContainerAdapter Handle<TMessage>(Action<TMessage> handler)
        {
            handlerActivator.Handle(handler);
            return this;
        }

        public IEnumerable<IHandleMessages<T>> GetHandlerInstancesFor<T>()
        {
            return handlerActivator.GetHandlerInstancesFor<T>();
        }

        public void Release(IEnumerable handlerInstances)
        {
            handlerActivator.Release(handlerInstances);
        }

        public void SaveBusInstances(IBus bus, IAdvancedBus advancedBus)
        {
            Bus = bus;
            AdvancedBus = advancedBus;
        }

        public void Dispose()
        {
            Bus.Dispose();

            if (!ReferenceEquals(Bus, AdvancedBus))
            {
                AdvancedBus.Dispose();
            }
        }
    }
}