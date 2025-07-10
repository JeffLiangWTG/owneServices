using System;
using System.Linq;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(WarehouseOperatorTransactionsOperationalActionMethodProvider))]
	class WarehouseOperatorTransactionsOperationalActionMethodProviderTest : OperationalActionMethodProviderTest
	{
		protected override ActionMethodProviderID ID => ActionMethodProviderIDs.WarehouseOperatorTransactions;

		public void TestNewMethods()
		{
			var provider = new WarehouseOperatorTransactionsOperationalActionMethodProvider();
			var expectedMethodTypes = new Type[]
			{
				typeof(ExWarehouseOperationalActionMethod),
				typeof(ExportNonBelnOperationalActionMethod),
				typeof(ExportBelnOperationalActionMethod),
				typeof(ExbondForUnderReceiptsOperationalActionMethod),
				typeof(ClearExpiredStockOperationalActionMethod),
			};
			AssertContainsExactElementsInAnyOrder(expectedMethodTypes, provider.NewMethods(null).Select(x => x.GetType()));
		}
	}
}
