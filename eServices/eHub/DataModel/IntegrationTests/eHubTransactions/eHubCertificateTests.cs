using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CargoWise.eHub.DataModel.eHubTransactions;

namespace CargoWise.eHub.DataModel.IntegrationTests.eHubTransactions
{
	[TestFixture]
	public class eHubCertificateTests : eHubTransactionsTestBase
	{
		[Test]
		public void TestAddCertificateOwnedByClient()
		{
			var datetime = new DateTime(1989, 11, 10);
			var eHubTestClient = new eHubClient() { CC_ID = "TTTSSSTTT", CC_OwnerCategory = "WiseTech", CC_SystemCategory = "eHub" };
			var certificate = new eHubCertificate() { CE_Category = "Test", CE_ID = "Test", CE_ContainerType = "pem-text", CE_AddedUTC = datetime };
			certificate.eHubClient = eHubTestClient;
			var expectedCertificate = new eHubCertificate() { CE_Category = "Test", CE_ID = "Test", CE_ContainerType = "pem-text", CE_AddedUTC = datetime, CE_CC_Owner = eHubTestClient.CC_PK };
			using (var context = ContextFactory())
			{
				using (var transaction = context.BeginTransaction())
				{
					context.eHubClients.Add(eHubTestClient);
					context.eHubCertificates.Add(certificate);
					context.SaveChanges();
					var reloadedCertificate = context.eHubCertificates.AsNoTracking().FirstOrDefault();
					CompareEntities(reloadedCertificate, expectedCertificate, memberToIgnore);
				}
			}
		}

		[Test]
		public void TestAddCertificateOwnedByClientSystem()
		{
			var datetime = new DateTime(1989, 11, 10);
			var clientSystem = new eHubClientSystem() { EH_ID = "TTT", EH_URL = "ccc", EH_InsertUTC = DateTime.Now, EH_LastUpdateUTC = datetime };
			var certificate = new eHubCertificate() { CE_Category = "Test", CE_ID = "Test", CE_ContainerType = "pem-text", CE_AddedUTC = datetime };
			certificate.eHubClientSystem = clientSystem;
			var expectedCertificate = new eHubCertificate() { CE_Category = "Test", CE_ID = "Test", CE_ContainerType = "pem-text", CE_AddedUTC = datetime, CE_EH_Owner = clientSystem.EH_PK };
			using (var context = ContextFactory())
			{
				using (var transaction = context.BeginTransaction())
				{
					context.eHubClientSystems.Add(clientSystem);
					context.eHubCertificates.Add(certificate);
					context.SaveChanges();
					var reloadedCertificate = context.eHubCertificates.AsNoTracking().FirstOrDefault();
					CompareEntities(reloadedCertificate, expectedCertificate, memberToIgnore);
				}
			}
		}

		List<string> memberToIgnore = new List<string>() { "CE_PK", "eHubClient", "eHubClientSystem" };
	}
}
