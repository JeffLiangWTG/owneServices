using System.Linq;
using NUnit.Framework;

namespace CargoWise.eHub.DataModel.IntegrationTests.eHubTransactions
{
	[TestFixture]
    public class eHubTransactionsDatabaseTests : eHubTransactionsTestBase
	{

		[Test]
        public void Database_CompatibilityLevel()
        {
            using (var context = ContextFactory())
            {
                var level = context.SqlQuery<byte>("select compatibility_level from sys.databases where name = 'eHubTransactions'").First();
                Assert.AreEqual(100, level);
            }
        }
	}
}

