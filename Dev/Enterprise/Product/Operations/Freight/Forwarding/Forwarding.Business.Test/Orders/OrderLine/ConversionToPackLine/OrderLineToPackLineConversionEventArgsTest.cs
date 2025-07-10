using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class OrderLineToPackLineConversionEventArgsTest : TestCaseWithFactory
	{
		public void TestNameShipmentsEventArgs()
		{
			OrderLineToPackLineConversionHelper helper = new OrderLineToPackLineConversionHelper(Factory, Factory.New<ForwardingShipment>(), new List<Order>());
			OrderLineToPackLineConversionEventArgs e = new OrderLineToPackLineConversionEventArgs(helper, true);
			AssertSame("Helper is same", helper, e.Helper);
		}

		public void TestShouldConvertOderLines()
		{
			OrderLineToPackLineConversionHelper helper = new OrderLineToPackLineConversionHelper(Factory, Factory.New<ForwardingShipment>(), new List<Order>());
			OrderLineToPackLineConversionEventArgs e = new OrderLineToPackLineConversionEventArgs(helper, false);
			AssertEquals("ShouldCreatePacklines", false, e.ShouldCreatePacklines);

			e.ShouldCreatePacklines = true;
			AssertEquals("ShouldCreatePacklines", true, e.ShouldCreatePacklines);
		}
	}
}
