using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Module;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Module.Testing
{
	public class JobShipmentPreplanningFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestWorkflowFilterStripIsInherited()
		{
			using (ZForm form = new ZForm())
			{
				JobShipmentPreplanningFilterControl control =
					new JobShipmentPreplanningFilterControl(new JobShipmentPreplanningCollection(Factory),
																									new JobShipmentPreplanningFilterBusinessObject());
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				control.AddNewFilterStrip();
				AssertEquals("Must return WorkflowFilterStripWithRoutingSupport so that workflow filter strips may be selected", typeof(WorkflowFilterStripWithRoutingSupport), control.LastFilterStripType);
			}
		}
	}
}
