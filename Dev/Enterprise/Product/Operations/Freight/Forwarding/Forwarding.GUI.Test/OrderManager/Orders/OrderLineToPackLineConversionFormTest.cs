using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.GUI.Testing
{
	[TestedType(typeof(OrderLineToPackLineConversionForm))]
	public class OrderLineToPackLineConversionFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			OrderLineToPackLineConversionHelper helper = new OrderLineToPackLineConversionHelper(Factory, Factory.New<ForwardingShipment>(), new List<Order>());
			return new OrderLineToPackLineConversionForm(helper);
		}
	}
}
