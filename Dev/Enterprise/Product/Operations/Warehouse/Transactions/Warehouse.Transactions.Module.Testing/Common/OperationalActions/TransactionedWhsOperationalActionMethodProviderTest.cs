using System;
using Enterprise.Services.OperationalActions.Support;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public class TransactionedWhsOperationalActionMethodProviderTest : TransactionedTestCase
	{
		public void TestExpectedMethodsforOrders_CreateLoadsFromOrdersActionMethod()
		{
			var provider = OperationalActionMethodProvider.New(ActionMethodProviderIDs.Warehouse);
			AssertCollectionContains("Should exists CreateLoadsFromOrdersActionMethod type",
			typeof(CreateLoadsFromOrdersActionMethod),
			Array.ConvertAll(provider.NewMethods(new OrderOperationalActionSupporter()), m => m.GetType()));
		}
	}
}
