﻿namespace NServiceBus.AcceptanceTests.Basic
{
    using System;
    using System.Collections.Generic;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NServiceBus.MessageMutator;
    using NUnit.Framework;

    public class When_mutating : NServiceBusAcceptanceTest
    {

        static Context context = new Context();

        [Test]
        public void Context_should_be_populated()
        {
            Scenario.Define(context)
                    .WithEndpoint<Sender>(b => b.Given((bus, c) =>
                    {
                        bus.Send(new StartMessage());
                    }))
                    .WithEndpoint<Receiver>()
                    .Done(c => c.WasCalled)
                    .Run(TimeSpan.FromHours(1));
            Assert.True(context.WasCalled, "The message handler should be called");
        }

        public class Context : ScenarioContext
        {
            public bool WasCalled { get; set; }
        }

        public class Sender : EndpointConfigurationBuilder
        {
            public Sender()
            {
                EndpointSetup<DefaultServer>()
                    .AddMapping<StartMessage>(typeof(Receiver));
            }
        }

        public class Receiver : EndpointConfigurationBuilder
        {
            public Receiver()
            {
                EndpointSetup<DefaultServer>(
                    b => b.RegisterComponents(r => r.ConfigureComponent<Mutator>(DependencyLifecycle.InstancePerCall)));
            }

            public class StartMessageHandler : IHandleMessages<StartMessage>
            {
                IBus bus;

                public StartMessageHandler(IBus bus)
                {
                    this.bus = bus;
                }

                public void Handle(StartMessage message)
                {
                    bus.SendLocal(new LoopMessage());
                }
            }
            public class LoopMessageHandler : IHandleMessages<LoopMessage>
            {
                public void Handle(LoopMessage message)
                {
                    context.WasCalled=true;
                }
            }

            public class Mutator:
                IMutateIncomingMessages,
                IMutateIncomingTransportMessages,
                IMutateOutgoingMessages,
                IMutateOutgoingTransportMessages
            {
                public void MutateIncoming(MutateIncomingMessageContext context)
                {
                    Assert.IsNotEmpty(context.Headers);
                    Assert.IsNotNull(context.Message);
                }

                public void MutateIncoming(MutateIncomingTransportMessageContext context)
                {
                    Assert.IsNotEmpty(context.Headers);
                    Assert.IsNotNull(context.Body);
                }

                public void MutateOutgoing(MutateOutgoingMessageContext context)
                {
                    Assert.IsNotEmpty(context.OutgoingHeaders);
                    Assert.IsNotNull(context.OutgoingMessage);
                    IReadOnlyDictionary<string, string> incomingHeaders;
                    context.TryGetIncomingHeaders(out incomingHeaders);
                    object incomingmessage;
                    context.TryGetIncomingMessage(out incomingmessage);
                    Assert.IsNotEmpty(incomingHeaders);
                    Assert.IsNotNull(incomingmessage);
                }

                public void MutateOutgoing(MutateOutgoingTransportMessageContext context)
                {
                    Assert.IsNotEmpty(context.OutgoingHeaders);
                    Assert.IsNotNull(context.OutgoingBody);
                    IReadOnlyDictionary<string, string> incomingHeaders;
                    context.TryGetIncomingHeaders(out incomingHeaders);
                    object incomingmessage;
                    context.TryGetIncomingMessage(out incomingmessage);
                    Assert.IsNotEmpty(incomingHeaders);
                    Assert.IsNotNull(incomingmessage);

                }
            }
        }

        public class StartMessage : IMessage
        {
        }
        public class LoopMessage : IMessage
        {
        }
    }

}
