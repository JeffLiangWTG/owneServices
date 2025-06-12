using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.eHub.DataModel.Comparer;
using CargoWise.eHub.DataModel.IntegrationTests.eHubTransactions;
using NUnit.Framework;

namespace CargoWise.eHub.DataModel.IntegrationTests.eHubArchiveOnline
{
	[TestFixture]
	public class eHubArchiveOnlineSchemaComparisionTests : eHubTransactionsTestBase
	{
		[Test]
		public void TestEHubArchiveOnlineAndEHubTransactionsSchemas()
		{
			using (var eHubTransactionsContext = ContextFactory())
			using (var eHubArchiveOnlineViewContext = EHubArchiveOnlineViewContextFactory())
			{
				var comparer = new SqlDbComparer(eHubTransactionsContext, eHubArchiveOnlineViewContext);

				var results = comparer.CompareTablesBothExisting();

				Assert.Multiple(() =>
				{
					foreach (var result in results)
					{
						var resultDifferences = result.Differences;
						Assert.AreEqual(0, resultDifferences.Count, "Differences: {0}", string.Join("\r\n", resultDifferences));
					}
				});
			}
		}
	}
}
