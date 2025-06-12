using CargoWise.eHub.DataModel.eHubTransactions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoWise.eHub.DataModel.IntegrationTests.eHubTransactions
{
	[TestFixture]
	public class eHubUSCustomsRegistryTests : eHubTransactionsTestBase
	{
		[Test]
		public void TestAddUSCustomsRegistry()
		{
			var testGuid = Guid.NewGuid();
			var ehubClient = new eHubClient
			{
				CC_PK = testGuid,
				CC_ID = "TTTSSSTTT",
				CC_OwnerCategory = "WiseTech",
				CC_SystemCategory = "eHub"
			};

			var ehubTestUSCustomsRegistry = new eHubUSCustomsRegistry
			{
				ER_ApplicationCode = "ABC",
				ER_CC_Client = testGuid,
				ER_Name = "ABC 1",
				ER_Value = "XYZ",
				ER_IsProduction = true
			};

			var expectedUSCustomsRegistry = new eHubUSCustomsRegistry
			{
				ER_ApplicationCode = "ABC",
				ER_CC_Client = testGuid,
				ER_Name = "ABC 1",
				ER_Value = "XYZ",
				ER_IsProduction = true
			};

			using (var context = ContextFactory())
			{
				using (var transaction = context.BeginTransaction())
				{
					context.eHubClients.Add(ehubClient);
					context.eHubUSCustomsRegistries.Add(ehubTestUSCustomsRegistry);
					context.SaveChanges();
					var reloadedUSCustomsRegistry = context.eHubUSCustomsRegistries.AsNoTracking().FirstOrDefault();
					CompareEntities(reloadedUSCustomsRegistry, expectedUSCustomsRegistry, memberToIgnore);
				}
			}
		}

		List<string> memberToIgnore = new List<string>() { "ER_PK", "eHubClient" };
	}
}
