using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.GUI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.GUI.Testing
{
	[TestedType(typeof(OrderLineForm))]
	public class OrderLineFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			var order = Factory.New<Order>();
			order.JD_OrderNumber = "ORDER-01";
			order.JD_OrderNumberSplit = 1;

			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_LineNo = 2;
			orderLine.JO_LineSplitNumber = 3;
			orderLine.JO_LineReference = string.Empty;

			using (var registry = OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new OrderLineForm(orderLine))
			{
				AssertEquals("Order: ORDER-01, Line: 2 (Order Split: 1, Line Split: 3)", form.FormCaption);

				orderLine.JO_LineReference = "LINE-REF-1";

				AssertEquals("Should not change as registry not enabled", "Order: ORDER-01, Line: 2 (Order Split: 1, Line Split: 3)", form.FormCaption);
			}

			orderLine.JO_LineReference = string.Empty;

			using (var registry = OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new OrderLineForm(orderLine))
			{
				AssertEquals("Registry is enabled but line reference is empty", "Order: ORDER-01, Line: 2 (Order Split: 1, Line Split: 3)", form.FormCaption);

				orderLine.JO_LineReference = "LINE-REF-1";

				AssertEquals("Registry is enabled and line reference is not empty", "Order: ORDER-01, Line Reference: LINE-REF-1", form.FormCaption);
			}
		}

		[ExpectNoExceptions]
		public void TestLoadForm()
		{
			var order = Factory.New<Order>();
			var line = order.OrderLines.AddNew();

			using (var form = new OrderLineForm(line))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("Must be editable on the order line form", true, form.OrderLine.Order.OrderLineDeliveriesEditable);
			}
		}

		[RequiresSTA]
		public void TestDescriptionAndCustomFieldsDefaultCaptions()
		{
			var org = Factory.New<OrgHeader>();
			var order = Factory.New<Order>();
			var line = order.OrderLines.AddNew();
			order.BuyerPK = org.PK;

			using (var form = new OrderLineForm(line))
			{
				form.Show();
				AssertEquals("Part Attrib. 1", form.Controls.Find("PartAttrib1TextBox", true)[0].GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Part Attrib. 2", form.Controls.Find("PartAttrib2TextBox", true)[0].GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Part Attrib. 3", form.Controls.Find("PartAttrib3TextBox", true)[0].GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Part Attrib. 3", form.Controls.Find("PartAttrib3TextBox", true)[0].GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Description", form.Controls.Find("JO_DescriptionBoundTextBox", true)[0].GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestSerialNumberVisibility()
		{
			var org = Factory.New<OrgHeader>();
			var order = Factory.New<Order>();
			var line = order.OrderLines.AddNew();
			order.BuyerPK = org.PK;

			using (var form = new OrderLineForm(line))
			{
				form.Show();
				var serialNumberTextBox = form.Controls.Find("SerialNumberTextBox", true)[0];
				AssertEquals("Serial Number text box only visible when registry item in use", true, serialNumberTextBox.Visible);
				AssertEquals("Serial Number", serialNumberTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestCustomFieldCaptions()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MiscServ.OM_IMPartAttrib1Name = "Zubs";
			org.MiscServ.OM_IMPartAttrib2Name = "Rakhsh";
			org.MiscServ.OM_IMPartAttrib3Name = "Zayd";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var order = Factory.New<Order>();
			var line = order.OrderLines.AddNew();
			order.BuyerPK = org.PK;

			using (var form = new OrderLineForm(line))
			{
				form.Show();
				AssertEquals("Zubs", form.Controls.Find("PartAttrib1TextBox", true)[0].GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Rakhsh", form.Controls.Find("PartAttrib2TextBox", true)[0].GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Zayd", form.Controls.Find("PartAttrib3TextBox", true)[0].GetExtension<ILabelCaptionRenderer>().Caption);
			}

			order.BuyerPK = org2.PK;

			using (var form = new OrderLineForm(line))
			{
				form.Show();
				AssertEquals("Part Attrib. 1", form.Controls.Find("PartAttrib1TextBox", true)[0].GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Part Attrib. 2", form.Controls.Find("PartAttrib2TextBox", true)[0].GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Part Attrib. 3", form.Controls.Find("PartAttrib3TextBox", true)[0].GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestHSCodeCaptions()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var order = Factory.New<Order>();
			var line = order.OrderLines.AddNew();
			order.BuyerPK = org.PK;

			using (var form = new OrderLineForm(line))
			{
				form.Show();
				AssertEquals("H.S. Code", form.Controls.Find("HarmonisedCodeFindBox", true)[0].GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestCommodityCodeCaptions()
		{
			var org = Factory.New<OrgHeader>();
			var order = Factory.New<Order>();
			var line = order.OrderLines.AddNew();
			order.BuyerPK = org.PK;

			using (var form = new OrderLineForm(line))
			{
				form.Show();
				AssertEquals("Commodity", form.Controls.Find("JO_RH_NKCommodityCodeFindBox", true)[0].GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestActionsMenuItemsNotAvailableInViewMode()
		{
			var order = Factory.New<Order>();
			var orderLine = order.OrderLines.AddNew();
			var form = new OrderLineForm(orderLine);
			ActionsMenuItemsHelperTest.AssertActionsMenuItemsNotAvailableInViewMode(form);
		}

		public void TestOrderLineTabControl()
		{
			var org = Factory.New<OrgHeader>();
			var order = Factory.New<Order>();
			var line = order.OrderLines.AddNew();
			order.BuyerPK = org.PK;

			using (var form = new OrderLineForm(line))
			{
				form.Show();
				foreach (var controlName in new[] { "CustomFieldsUserControl", "WorkflowTabPage" })
				{
					AssertNotNull(form.Controls.Find(controlName, true).FirstOrDefault());
				}
			}
		}

		public void TestShippingToleranceTab_OnlyVisibleWithRegistry()
		{
			var org = Factory.New<OrgHeader>();
			var order = Factory.New<Order>();
			var line = order.OrderLines.AddNew();
			order.BuyerPK = org.PK;

			using (OrdersDataRegistry.Instance.EnableOrderLineShippingTolerance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new OrderLineForm(line))
			{
				form.Show();
				AssertNull(form.Controls.Find("ShippingToleranceTabPage", true).OfType<ZTabPage>().FirstOrDefault());
			}

			using (OrdersDataRegistry.Instance.EnableOrderLineShippingTolerance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new OrderLineForm(line))
			{
				form.Show();
				AssertNotNull(form.Controls.Find("ShippingToleranceTabPage", true).OfType<ZTabPage>().FirstOrDefault());
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var order = Factory.New<Order>();
			var line = order.OrderLines.AddNew();
			line.HasChanges = false;
			return new OrderLineForm(line)
			{ ControllerID = ControllerIDs.OrderLine };
		}

		#endregion
	}
}
