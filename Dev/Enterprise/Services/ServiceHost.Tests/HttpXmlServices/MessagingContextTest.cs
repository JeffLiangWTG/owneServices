using System;
using System.IO;
using System.Threading;
using System.Web;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Services.ServiceHost.Tests
{
	class MessagingContextTest : TestCaseWithFactory
	{
		IDisposable SetNewHttpContext()
		{
			var context = new HttpContext(new HttpRequest(string.Empty, "http://test.com", string.Empty), new HttpResponse(new StringWriter()));
			var currentContext = HttpContext.Current;

			return new DisposableAction(() => HttpContext.Current = context, () => HttpContext.Current = currentContext);
		}

		IMessagingContext MessagingContext => ObjectFactory.Get<IMessagingContext>();

		EDICommunicationPartyConfig CreateConfiguration()
		{
			var party = Factory.NewWithValidTestData<EDICommunicationParty>();
			var inboundConfig = Factory.New<EDICommunicationPartyConfig>();
			inboundConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;

			party.Configs.Add(inboundConfig);

			Factory.Save();

			return inboundConfig;
		}

		public void TestCurrentConfigurationIsNullByDefault()
		{
			AssertNull(MessagingContext.CurrentInboundConfig);
		}

		public void TestMessagingContextWithHttpContext()
		{
			using (SetNewHttpContext())
			{
				var inboundConfig = CreateConfiguration();

				MessagingContext.CurrentInboundConfig = inboundConfig;

				AssertEquals(inboundConfig.PK, MessagingContext.CurrentInboundConfig.PK);

				using (SetNewHttpContext())
				{
					AssertNull(MessagingContext.CurrentInboundConfig);
				}
			}
		}

		public void TestMessagingWithoutHttpContext()
		{
			var inboundConfig = CreateConfiguration();

			MessagingContext.CurrentInboundConfig = inboundConfig;

			AssertEquals(inboundConfig.PK, MessagingContext.CurrentInboundConfig.PK);

			using (ExecutionContext.SuppressFlow())
			{
				var thread = new Thread(() =>
				{
					AssertNull(MessagingContext.CurrentInboundConfig);
				});

				thread.Start();
				thread.Join();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			HttpContext.Current = null;
		}
	}
}
