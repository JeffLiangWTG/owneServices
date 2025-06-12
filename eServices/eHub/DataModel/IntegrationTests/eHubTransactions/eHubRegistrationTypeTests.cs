using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CargoWise.eHub.DataModel.eHubTransactions;

namespace CargoWise.eHub.DataModel.IntegrationTests.eHubTransactions
{
	[TestFixture]
	public class eHubRegistrationTypeTests : eHubTransactionsTestBase
	{
		[Test]
		public void TestAddeHubRegistrationType()
		{
			var registrationType = new eHubRegistrationType() { RT_ID = "TTT", RT_Description = "Test", RT_RegistrantType = "Client" };
			var expectedRegistrationType = new eHubRegistrationType() { RT_ID = "TTT", RT_Description = "Test", RT_RegistrantType = "Client" };
			using (var context = ContextFactory())
			{
				using (var transaction = context.BeginTransaction())
				{
					context.eHubRegistrationTypes.Add(registrationType);
					context.SaveChanges();
					var reloadedRegistrationType = context.eHubRegistrationTypes.AsNoTracking().Where(r => r.RT_ID == "TTT").FirstOrDefault();
					CompareEntities(reloadedRegistrationType, expectedRegistrationType, memberToIgnore);
				}
			}
		}
		[Test]
		public void TestAddeHubRegistrationTypeACAS_BR()
		{
			var registrationType = new eHubRegistrationType() { RT_ID = "ACAS_BR", RT_Description = "BR Air Cargo Advance Screening", RT_RegistrantType = "AsyncPolling" };
			var expectedRegistrationType = new eHubRegistrationType() { RT_ID = "ACAS_BR", RT_Description = "BR Air Cargo Advance Screening", RT_RegistrantType = "AsyncPolling" };
			using (var context = ContextFactory())
			{
				using (var transaction = context.BeginTransaction())
				{
					context.eHubRegistrationTypes.Add(registrationType);
					context.SaveChanges();
					var reloadedRegistrationType = context.eHubRegistrationTypes.AsNoTracking().Where(r => r.RT_ID == "ACAS_BR").FirstOrDefault();
					CompareEntities(reloadedRegistrationType, expectedRegistrationType, memberToIgnore);
				}
			}
		}

		List<string> memberToIgnore = new List<string>() { "RT_PK", "eHubAsyncPollingRegistrations" };
	}
}
