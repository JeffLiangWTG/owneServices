using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(OrderLineProcessTaskCollection))]
	sealed class OrderLineProcessTaskCollectionTest : ProcessTaskCollectionTest<OrderLineProcessTaskCollection>
	{
		#region Implementation

		protected override OrderLineProcessTaskCollection GetCollectionToTestCore()
		{
			return new OrderLineProcessTaskCollection(OrderLine);
		}

		OrderLine OrderLine
		{
			get
			{
				if (orderLine == null)
				{
					orderLine = Factory.NewWithValidTestData<OrderLine>();
				}
				return orderLine;
			}
		}
		OrderLine orderLine;

		#endregion
	}
}
