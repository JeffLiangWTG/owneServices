using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CargoWise.eHub.DataModel.eHubTransactions;

namespace CargoWise.eHub.DataModel.IntegrationTests.eHubTransactions
{
	[TestFixture]
	public class eHubInterfaceCounterTests : eHubTransactionsTestBase
	{

		[Test]
		public void TestAddNewCounter()
		{
			var time = new DateTime(1989, 11, 10);
			var eHubTransformationSet = new eHubTransformationSet() { TS_Name = "AAABBBCCC" };
			var eHubInterfaceCounter = new eHubInterfaceCounter() { eHubTransformationSet = eHubTransformationSet, CT_Name = "AAABBB", CT_LastUpdateUTC = time };
			var expectedeHubInterfaceCounter = new eHubInterfaceCounter() { eHubTransformationSet = eHubTransformationSet, CT_Name = "AAABBB", CT_LastUpdateUTC = time };
			using (var context = ContextFactory())
			{
				context.Configuration.ProxyCreationEnabled = false;

				using (var transaction = context.BeginTransaction())
				{
					context.eHubTransformationSets.Add(eHubTransformationSet);
					context.eHubInterfaceCounters.Add(eHubInterfaceCounter);
					context.SaveChanges();
					var reloadedCounter = context.eHubInterfaceCounters.Include("eHubTransformationSet").AsNoTracking().FirstOrDefault(c => c.CT_Name == "AAABBB");
					CompareEntities(reloadedCounter, expectedeHubInterfaceCounter);
				}
			}
		}
	}
}

