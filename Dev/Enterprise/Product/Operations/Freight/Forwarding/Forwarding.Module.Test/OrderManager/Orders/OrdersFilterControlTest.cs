using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Module;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Module.Testing
{
	public class OrdersFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestWorkflowFilterStripIsInherited()
		{
			using (ZForm form = new ZForm())
			{
				OrdersFilterControl control = new OrdersFilterControl(new OrderCollection(Factory), new OrdersFilterBusinessObject());
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				control.AddNewFilterStrip();
				Assert("Must return WorkflowFilterStripWithRoutingSupport so that workflow filter strips may be selected", control.LastFilterStripType.IsSubclassOf(typeof(WorkflowFilterStripWithRoutingSupport)));
			}
		}
	}
}
