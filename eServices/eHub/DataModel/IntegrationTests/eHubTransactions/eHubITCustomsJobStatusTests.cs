using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CargoWise.eHub.DataModel.eHubTransactions;

namespace CargoWise.eHub.DataModel.IntegrationTests.eHubTransactions
{
	[TestFixture]
	public class eHubITCustomsJobStatusTests : eHubTransactionsTestBase
	{
		[Test]
		public void TestAddJob()
		{
			var now = new DateTime(1989, 11, 10);
			var trackingID = new Guid();
			var eHubTestClient =
				new eHubClient() { CC_ID = "TTTSSSTTT", CC_OwnerCategory = "WiseTech", CC_SystemCategory = "eHub" };
			var eHubTestClientSystem = new eHubClientSystem() { EH_ID = "AAABBB", EH_URL = "", EH_InsertUTC = now, EH_LastUpdateUTC = now };
			var eHubTestJob = new eHubITCustomsJobStatus()
			{
				eHubClientSystem = eHubTestClientSystem,
				eHubClient = eHubTestClient,
				IT_FileName = "AAAABBBBCCCCDDDD",
				IT_JobID = "111",
				IT_LastStatus = "ABC",
				IT_NotifiedInvalidProfile = false,
				IT_PollingStartUTC = now,
				IT_MessageTrackingID = trackingID,
				IT_ReferenceID = "RRR",
				IT_ProdInd = true,
				IT_MessageType = "foo",
				IT_DeclarationContent = "dGVzdA=="
			};

			var expectedJob = new eHubITCustomsJobStatus()
			{
				IT_EH_ClientSystem = eHubTestClientSystem.EH_PK,
				IT_CC_Sender = eHubTestClient.CC_PK,
				IT_FileName = "AAAABBBBCCCCDDDD",
				IT_JobID = "111",
				IT_LastStatus = "ABC",
				IT_NotifiedInvalidProfile = false,
				IT_PollingStartUTC = now,
				IT_MessageTrackingID = trackingID,
				IT_ReferenceID = "RRR",
				IT_ProdInd = true,
				IT_FileLastModifiedUTC = null,
				IT_MessageType = "foo",
				IT_DeclarationContent = "dGVzdA=="
			};
			using (var context = ContextFactory())
			{
				using (var transaction = context.BeginTransaction())
				{
					context.eHubClients.Add(eHubTestClient);
					context.eHubClientSystems.Add(eHubTestClientSystem);
					context.eHubITCustomsJobStatuses.Add(eHubTestJob);
					context.SaveChanges();
					var reloadedJob = context.eHubITCustomsJobStatuses.AsNoTracking().FirstOrDefault();
					CompareEntities(reloadedJob, expectedJob, memberToIgnore);
				}
			}
		}

		List<string> memberToIgnore = new List<string>() { "IT_PK", "eHubClient", "eHubClientSystem", "IT_Version" };
	}
}