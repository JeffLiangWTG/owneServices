using System;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	[TestedType(typeof(OrderDetailsBulkUpdateForm))]
	public class OrderDetailsBulkUpdateFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			OrderDetailsBulkUpdateBusinessObject bO = new OrderDetailsBulkUpdateBusinessObject(Factory);
			return new OrderDetailsBulkUpdateForm(bO);
		}

		public void TestNewControlsNotAdded()
		{
			OrderDetailsBulkUpdateBusinessObject bO = new OrderDetailsBulkUpdateBusinessObject(Factory);
			using (TestOrderDetailsBulkUpdateForm form = new TestOrderDetailsBulkUpdateForm(bO))
			{
				AssertEquals(
					"The number of bound controls on OrderTrackingDates has changed, " +
					"please ensure the values are being copied across appropriately and update the count on this test.",
					18, CountBoundControls(form.OrderTrackingDates));
				AssertEquals(
					"The number of bound controls on OrderPlanningVesselVoyageAndDates has changed, " +
					"please ensure the values are being copied across appropriately and update the count on this test.",
					12, CountBoundControls(form.OrderPlanningVesselVoyageAndDates));
			}
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestBulkUpdateWhileBinding()
		{
			var newOrder = Factory.NewWithValidTestData<Order>();
			newOrder.JD_OrderNumber = "splaty";

			Factory.Save();
			OrderDetailsBulkUpdateBusinessObject bO = new OrderDetailsBulkUpdateBusinessObject(Factory);
			using (TestOrderDetailsBulkUpdateForm form = new TestOrderDetailsBulkUpdateForm(bO))
			{
				form.Show();
				Application.DoEvents();

				OrderToBulkUpdate orderToBulkUpdate = new OrderToBulkUpdate(Factory, bO);
				orderToBulkUpdate.SetOrder(newOrder);

				bO.SelectedOrders.Add(orderToBulkUpdate);
				bO.JD_A_ARV = ZDateTime.Now;
				Factory.Save();
			}
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestOnAttachButton_Click()
		{
			OrderDetailsBulkUpdateBusinessObject bO = new OrderDetailsBulkUpdateBusinessObject(Factory);
			using (TestOrderDetailsBulkUpdateForm form = new TestOrderDetailsBulkUpdateForm(bO))
			{
				form.Show();
				Application.DoEvents();
				try
				{
					form.OnAttachButton_Click(null, null);
					Application.DoEvents();
				}
				finally
				{
					if (form.LastShownAttachPopup != null)
					{
						form.LastShownAttachPopup.Dispose();
					}
				}
			}
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestOnDetachButton_Click()
		{
			OrderDetailsBulkUpdateBusinessObject bO = new OrderDetailsBulkUpdateBusinessObject(Factory);
			bO.SelectedOrders.AddNew();
			using (TestOrderDetailsBulkUpdateForm form = new TestOrderDetailsBulkUpdateForm(bO))
			{
				form.Show();
				Application.DoEvents();
				form.OnDetachButton_Click(null, null);
				Application.DoEvents();
			}
		}

		static int CountBoundControls(Control control)
		{
			int result = 0;
			foreach (Control childControl in control.Controls)
			{
				if (!string.IsNullOrEmpty(childControl.GetBindingMember()))
				{
					result++;
				}
				result += CountBoundControls(childControl);
			}
			return result;
		}

		class TestOrderDetailsBulkUpdateForm : OrderDetailsBulkUpdateForm
		{
			public TestOrderDetailsBulkUpdateForm(OrderDetailsBulkUpdateBusinessObject bO) : base(bO)
			{
			}

			public new OrderTrackingDatesControl OrderTrackingDates
			{
				get { return base.OrderTrackingDates; }
			}

			public new OrderPlanningVesselVoyageAndDatesControl OrderPlanningVesselVoyageAndDates
			{
				get { return base.OrderPlanningVesselVoyageAndDates; }
			}

			public new void OnAttachButton_Click(object sender, EventArgs e)
			{
				base.OnAttachButton_Click(sender, e);
			}

			public new void OnDetachButton_Click(object sender, EventArgs e)
			{
				base.OnDetachButton_Click(sender, e);
			}

			public new EmbeddedModulePopup LastShownAttachPopup
			{
				get { return base.LastShownAttachPopup; }
			}
		}
	}
}
