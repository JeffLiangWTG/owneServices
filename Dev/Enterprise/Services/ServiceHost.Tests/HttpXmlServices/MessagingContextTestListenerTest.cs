using System;
using System.Web;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Services.ServiceHost.Tests
{
	class MessagingContextTestListenerTest : TestCaseWithFactory
	{
		public void TestMessagingContextConfigIsReset()
		{
			var messagingContext = ObjectFactory.Get<IMessagingContext>();
			messagingContext.CurrentInboundConfig = Factory.New<EDICommunicationPartyConfig>();
			new MessagingContextTestListener().BeforeEachTest(DateTime.Now);
			AssertEquals("Messaging Context inbound config reset by test listener", null, messagingContext.CurrentInboundConfig);
		}

		protected override void SetUp()
		{
			base.SetUp();
			HttpContext.Current = null;
		}
	}
}
