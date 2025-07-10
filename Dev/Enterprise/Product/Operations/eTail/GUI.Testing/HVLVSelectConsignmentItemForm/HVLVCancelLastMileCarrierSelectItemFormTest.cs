using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.eTail.GUI.Testing
{
	[TestedType(typeof(HVLVCancelLastMileCarrierSelectItemForm))]
	class HVLVCancelLastMileCarrierSelectItemFormTest : HVLVSelectConsignmentItemFormBaseTest
	{
		public void TestBuildTree_ContainsConsignmentAndItemsThatHaveBeenBooked()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item1 = consignment.Items.AddNew();
			var item2 = consignment.Items.AddNew();
			var item3 = consignment.Items.AddNew();
			item1.Logs.AddNew(AutoEvents.BookingConfirmed, string.Empty, DateTime.Now.AddDays(-1), false);
			item3.Logs.AddNew(AutoEvents.BookingConfirmed, string.Empty, DateTime.Now.AddDays(-1), false);

			using (var form = new HVLVCancelLastMileCarrierSelectItemForm(consignment))
			{
				form.Show();

				var consignmentTreeView = form.Controls.Find("ConsignmentItemTreeView", true)[0] as ZTreeView;

				var treeNodes = consignmentTreeView.Nodes.OfType<TreeNode>();
				AssertContainsExactElementsInAnyOrder(new[] { consignment.HumanReadableName }, treeNodes.Select(node => node.Text));

				var rootConsignmentNode = treeNodes.Single();
				var itemNodes = rootConsignmentNode.Nodes.OfType<ZBusinessObjectTreeNode>();
				AssertContainsExactElementsInAnyOrder(new[] { item1, item3 }, itemNodes.Select(node => node.BizO));
			}
		}

		public void TestItemNodeText_WhenItemHasNoBarCodeOrShipperReference_UsesItemId()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();
			item.HVI_ItemId = "Item 001";
			item.Logs.AddNew(AutoEvents.BookingConfirmed, string.Empty, DateTime.Now.AddDays(-1), false);

			using (var form = new HVLVCancelLastMileCarrierSelectItemForm(consignment))
			{
				form.Show();

				var consignmentTreeView = form.Controls.Find("ConsignmentItemTreeView", true)[0] as ZTreeView;

				var itemNodeText = consignmentTreeView.Nodes[0].Nodes[0].Text;
				AssertEquals("Item should be labelled with item id:", $"Last Mile Carrier Booking for Item ID: {item.HVI_ItemId}", itemNodeText);
			}
		}

		public void TestItemNodeText_WhenItemHasBarcodeOnly_UsesBarcode()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();
			item.HVI_CurrentBarcode = "BARCODEORDER66";
			item.Logs.AddNew(AutoEvents.BookingConfirmed, string.Empty, DateTime.Now.AddDays(-1), false);

			using (var form = new HVLVCancelLastMileCarrierSelectItemForm(consignment))
			{
				form.Show();

				var consignmentTreeView = form.Controls.Find("ConsignmentItemTreeView", true)[0] as ZTreeView;

				var itemNodeText = consignmentTreeView.Nodes[0].Nodes[0].Text;
				AssertEquals("Item should be labelled with barcode:", $"Last Mile Carrier Booking for Item Barcode: {item.HVI_CurrentBarcode}", itemNodeText);
			}
		}

		public void TestItemNodeText_WhenItemShipperReferenceOnly_UsesShipperReference()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();
			item.HVI_ShipperReference = "My specific order of Ketracel White";
			item.Logs.AddNew(AutoEvents.BookingConfirmed, string.Empty, DateTime.Now.AddDays(-1), false);

			using (var form = new HVLVCancelLastMileCarrierSelectItemForm(consignment))
			{
				form.Show();

				var consignmentTreeView = form.Controls.Find("ConsignmentItemTreeView", true)[0] as ZTreeView;

				var itemNodeText = consignmentTreeView.Nodes[0].Nodes[0].Text;
				AssertEquals("Item should be labelled with shipper reference:", $"Last Mile Carrier Booking for Item Shipper Reference: {item.HVI_ShipperReference}", itemNodeText);
			}
		}

		public void TestItemNodeText_WhenItemHasShipperReferenceAndBarcode_UsesShipperReference()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();
			item.HVI_CurrentBarcode = "THISGETSIGNOREDFORSHIPPERREF";
			item.HVI_ShipperReference = "My specific order of Ketracel White";
			item.Logs.AddNew(AutoEvents.BookingConfirmed, string.Empty, DateTime.Now.AddDays(-1), false);

			using (var form = new HVLVCancelLastMileCarrierSelectItemForm(consignment))
			{
				form.Show();

				var consignmentTreeView = form.Controls.Find("ConsignmentItemTreeView", true)[0] as ZTreeView;

				var itemNodeText = consignmentTreeView.Nodes[0].Nodes[0].Text;
				AssertEquals("Item should be labelled with shipper reference:", $"Last Mile Carrier Booking for Item Shipper Reference: {item.HVI_ShipperReference}", itemNodeText);
			}
		}

		#region Implementation

		protected override HVLVSelectConsignmentItemForm GetSelectItemForm(HVLVConsignment consignment) => new HVLVCancelLastMileCarrierSelectItemForm(consignment);

		protected override HVLVItem CreateValidItemForTreeView(HVLVConsignment parentConsignment)
		{
			var item = base.CreateValidItemForTreeView(parentConsignment);
			item.Logs.AddNew(AutoEvents.BookingConfirmed, string.Empty, DateTime.Now.AddDays(-1), false);
			return item;
		}

		protected override ZString ExpectedCaptionResourceString => "Cancel Last Mile Carrier Booking";

		protected override ZString ExpectedOkButtonText => "Cancel Booking";

		#endregion
	}
}
