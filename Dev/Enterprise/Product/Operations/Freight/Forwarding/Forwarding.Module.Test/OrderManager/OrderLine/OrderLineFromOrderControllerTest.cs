using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Module.Testing
{
	[TestedType(typeof(OrderLineFromOrderController))]
	public class OrderLineFromOrderControllerTest : OrderLineControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.OrderLineFromOrder;
		}
	}
}
