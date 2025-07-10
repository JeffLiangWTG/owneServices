using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(Order))]
	sealed class OrderRelatableActivityTest : RelatableActivityTestCase<Order>
	{
		protected override Order GetNewActivity()
		{
			return Factory.NewWithValidTestData<Order>();
		}
	}
}
