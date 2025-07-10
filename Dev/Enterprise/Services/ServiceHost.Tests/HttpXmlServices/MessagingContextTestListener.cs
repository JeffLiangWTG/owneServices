using System;
using CargoWise.Application;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	public class MessagingContextTestListener : BaseTestListener
	{
		readonly IMessagingContext messagingContext = ObjectFactory.Get<IMessagingContext>();

		public override void BeforeEachTest(DateTime startTime)
		{
			messagingContext.ResetCurrentInboundConfig();
		}
	}
}
