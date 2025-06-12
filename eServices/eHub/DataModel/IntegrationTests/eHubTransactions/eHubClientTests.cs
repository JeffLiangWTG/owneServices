using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CargoWise.eHub.DataModel.eHubTransactions;

namespace CargoWise.eHub.DataModel.IntegrationTests.eHubTransactions
{
	[TestFixture]
	public class eHubClientTests : eHubTransactionsTestBase
	{
		[Test]
		public void TestAddClient()
		{
			var eHubTestClient = new eHubClient() { CC_ID = "TTTSSSTTT", CC_OwnerCategory = "WiseTech", CC_SystemCategory = "eHub" };
			var expectedClient = new eHubClient() { CC_ID = "TTTSSSTTT", CC_OwnerCategory = "WiseTech", CC_SystemCategory = "eHub" };
			using (var context = ContextFactory())
			{
				using (var transaction = context.BeginTransaction())
				{
					context.eHubClients.Add(eHubTestClient);
					context.SaveChanges();
					var reloadedClient = context.eHubClients.AsNoTracking().Where(c=>c.CC_ID == "TTTSSSTTT").FirstOrDefault();
					CompareEntities(reloadedClient, expectedClient, memberToIgnore);
				}
			}
		}

		List<string> memberToIgnore = new List<string>() { "CC_PK", "eHubAsyncPollingRegistrations", "eHubCertificates", "eHubInboxMessage_Recipients", "eHubInboxMessage_Senders",
			"eHubInboxMessageArchive_Recipients", "eHubInboxMessageArchive_Senders", "eHubInboxMessageDisplay_Recipients", "eHubInboxMessageDisplay_Senders", "eHubITCustomsJobStatuses",
			"eHubOutboxMessage_Recipients", "eHubOutboxMessage_Senders", "eHubOutboxMessageArchive_Recipients", "eHubOutboxMessageArchive_Senders", "eHubOutboxMessageDisplay_Recipients",
			"eHubOutboxMessageDisplay_Senders", "eHubSubscriptionAutoSubscribes", "eHubSubscriptionBroadcasters_Senders", "eHubSubscriptionBroadcasters_Recipients", "eHubTransformationSet_BillOthers", "eHubTransformationSet_Recipients",
			"eHubTransformationSet_Senders", "eHubClientAuthorisation_Senders", "eHubClientAuthorisation_Recipients", "eHubMonitors", "eHubUSCustomsRegistry_Clients"};
	}
}
