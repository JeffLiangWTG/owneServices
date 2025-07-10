using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.GUI.Testing
{
	[TestedType(typeof(OrdersInvoicesListForm))]
	public class OrdersInvoicesListFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			Order order = Factory.New<Order>();
			return new OrdersInvoicesListForm(new QueryUserFindboxEventArgs(DummyModuleIDs.Dummy, order.InvoiceList));
		}
	}
}
