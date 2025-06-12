using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CargoWise.eHub.DataModel.eHubTransactions;

namespace CargoWise.eHub.DataModel.IntegrationTests.eHubTransactions
{
	[TestFixture]
	public class eHubClientSystemTests : eHubTransactionsTestBase
	{
		[Test]
		public void TestAddeHubClientSystem()
		{
			var datetime = new DateTime(1989, 11, 10);
			var clientSystem = new eHubClientSystem() { EH_ID = "TTT", EH_URL = "ccc", EH_InsertUTC = datetime, EH_LastUpdateUTC = datetime };
			var expectedClientSystem = new eHubClientSystem() { EH_ID = "TTT", EH_URL = "ccc", EH_InsertUTC = datetime, EH_LastUpdateUTC = datetime };
			using (var context = ContextFactory())
			{
				using (var transaction = context.BeginTransaction())
				{
					context.eHubClientSystems.Add(clientSystem);
					context.SaveChanges();
					var reloadedClientSystem = context.eHubClientSystems.AsNoTracking().Where(c => c.EH_ID == "TTT").FirstOrDefault();
					CompareEntities(reloadedClientSystem, expectedClientSystem, memberToIgnore);
				}
			}
		}

		List<string> memberToIgnore = new List<string>() { "EH_PK" };
	}
}
