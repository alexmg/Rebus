﻿using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Rebus.Messages;
using Rebus.Transports.Msmq;
using Rhino.Mocks;

namespace Rebus.Tests.Unit
{
    [TestFixture]
    public class TestMessageForwarding : RebusBusUnitTestBase
    {
        [Test]
        public void CanForwardLogicalMessageToAnotherEndpoint()
        {
            // arrange
            activateHandlers.Handle<JustSomeMessage>(msg => bus.Routing.ForwardCurrentMessage("anotherEndpoint"));

            // act
            const string arbitrarykey = "arbitraryKey";
            const string anotherArbitraryKey = "anotherArbitraryKey";

            const string arbitraryValue = "arbitraryValue";
            const string anotherArbitraryValue = "anotherArbitraryValue";

            receiveMessages.Deliver(new Message
                {
                    Headers = new Dictionary<string, string>
                        {
                            {arbitrarykey, arbitraryValue},
                            {anotherArbitraryKey, anotherArbitraryValue}
                        },
                    Messages = new object[] {new JustSomeMessage()}
                });

            Thread.Sleep(0.5.Seconds());

            // assert
            sendMessages
                .AssertWasCalled(s => s.Send(Arg<string>.Is.Equal("anotherEndpoint"),
                                             Arg<TransportMessageToSend>
                                                 .Matches(
                                                     m =>
                                                     m.Headers.ContainsKey(arbitrarykey) &&
                                                     m.Headers[arbitrarykey] == arbitraryValue
                                                     && m.Headers.ContainsKey(anotherArbitraryKey) &&
                                                     m.Headers[anotherArbitraryKey] == anotherArbitraryValue),
                                             Arg<ITransactionContext>.Is.Anything));
        }

        class JustSomeMessage {}
    }
}