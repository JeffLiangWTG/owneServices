using System.Collections;
using CargoWise.Application;
using NUnit.Framework;
using ObjectHandle = CargoWise.Application.ObjectHandle;

namespace Enterprise.Warehouse.Invoicing.Business.Test
{
	public abstract class PeriodicInvoicingStrategyTest(string storageType) : TestCase
	{
		public void TestStrategyIsNotNull()
		{
			var strategy = GetStrategy();
			AssertNotNull("The strategy is not null.", strategy);
		}

		public void TestStrategyIsSingleton()
		{
			var strategy = GetStrategy();
			var strategy2 = GetStrategy();
			AssertSame("The strategy is a singleton.", strategy, strategy2);
		}

		object GetStrategy()
		{
			var providers = (Hashtable)ObjectFactory.Get("PeriodicInvoicingStrategyFactory");
			var objectHandle = (ObjectHandle)providers[storageType];
			return objectHandle?.GetObject() as IPeriodicInvoicingStrategy;
		}
	}
}
