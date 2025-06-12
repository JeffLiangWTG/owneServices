using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.eHub.DataModel.Accessors;
using CargoWise.eHub.DataModel.eHubTransactions;
using NUnit.Framework;

namespace CargoWise.eHub.DataModel.IntegrationTests.eHubTransactions
{
	[TestFixture]
	public class eHubAsyncPollingRegistrationTests :eHubTransactionsTestBase
	{
		[Test]
		public void TestAddCertificateOwnedByClient()
		{
			var datetime = new DateTime(1989, 11, 10);
			var eHubClient = new eHubClient() { CC_ID = "TTTSSSTTT", CC_OwnerCategory = "WiseTech", CC_SystemCategory = "eHub" };
			var eHubRegistrationType = new eHubRegistrationType(){ RT_ID = "ACAS_BRProtocol", RT_RegistrantType = "AsyncPolling" };
			var eHubClientSystem = new eHubClientSystem() { EH_ID = "WTLNPN", EH_URL = "", EH_InsertUTC = datetime, EH_LastUpdateUTC = datetime };
			var eHubAsyncPollingRegistration = 
				new eHubAsyncPollingRegistration() 
				{ 
					PR_PK = new Guid("00000000-0000-0000-0000-000000000000"),
					PR_CC = eHubClient.CC_PK, 
					PR_EH = eHubClientSystem.EH_PK, 
					PR_RT = eHubRegistrationType.RT_PK, 
					PR_XML = "<AAA>DDD</AAA>", 
					PR_Text = "NPN"
				};
			using (var context = ContextFactory())
			{
				context.Configuration.ProxyCreationEnabled = false;

				using (var transaction = context.BeginTransaction())
				{
					context.eHubClients.Add(eHubClient);
					context.eHubRegistrationTypes.Add(eHubRegistrationType);
					context.eHubClientSystems.Add(eHubClientSystem);
					context.eHubAsyncPollingRegistrations.Add(eHubAsyncPollingRegistration);
					context.SaveChanges();
					var reloadedAsycPollingRegistration = 
						context.eHubAsyncPollingRegistrations.Include("eHubClient").Include("eHubClientSystem").Include("eHubRegistrationType").AsNoTracking().FirstOrDefault();
					CompareEntities(reloadedAsycPollingRegistration, eHubAsyncPollingRegistration);
				}
			}
		}
	}
}
