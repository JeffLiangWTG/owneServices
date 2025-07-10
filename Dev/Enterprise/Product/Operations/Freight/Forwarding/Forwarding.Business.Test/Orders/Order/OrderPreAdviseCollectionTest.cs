using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(JobOrderHeaderLookups.OrderPreAdviseCollection))]
	sealed class OrderPreAdviseCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var parent = Factory.NewWithValidTestData<Order>();
			return new JobOrderHeaderLookups.OrderPreAdviseCollection(Factory, parent);
		}
	}
}
