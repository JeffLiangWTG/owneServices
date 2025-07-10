using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.GUI.Testing
{
	[TestedType(typeof(HVLVConsignmentModuleButtonGrid))]
	class HVLVConsignmentModuleButtonGridTest : ZModuleButtonGridTestBase
	{
		public void TestButtonVisibility()
		{
			using (var buttonGrid = new HVLVConsignmentModuleButtonGrid())
			{
				CombineAssertions("All buttons should be hidden", () =>
				{
					AssertEquals("Show Attach Button", false, buttonGrid.ShowAttachButton);
					AssertEquals("Show Detach Button", false, buttonGrid.ShowDetachButton);
					AssertEquals("Show Edit Button", false, buttonGrid.ShowEditButton);
					AssertEquals("Show New Button", false, buttonGrid.ShowNewButton);
				});
			}
		}

		public void TestAttachButton_AttachConsignmentSuccessfully()
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = "HVL";
			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment = header.Consignments.AddNew();
			consignment.HVC_IsActive = true;
			consignment.HVC_Status = "BKD";
			consignment.HVC_ConsigneeName = "name";
			consignment.HVC_ConsigneeAddress1 = "address";
			consignment.HVC_ConsigneeCity = "city";
			consignment.HVC_ConsigneePostcode = "3053";
			consignment.HVC_ShipperName = "name";
			consignment.HVC_ShipperAddress1 = "address";
			consignment.HVC_ShipperCity = "city";
			consignment.HVC_RN_NKConsigneeCountryCode = "CN";

			var item = consignment.Items.AddNew();
			item.HVI_ManifestedVolume = 10;
			item.HVI_ManifestedWeight = 10;
			factory.Save();
			header.Consignments.Remove(consignment);
			factory.Save();

			var shipmentToAttach = factory.NewWithValidTestData<ForwardingShipment>();
			shipmentToAttach.JS_ShipmentType = "HVL";
			var headerToAttach = shipmentToAttach.GetOrCreateHVLVConsignmentHeader();

			using (var form = new ConsignmentUserControlTestForm(headerToAttach))
			{
				form.Show();
				var buttonGrid = (ZModuleButtonGrid)form.Controls.Find("consignmentsGrid", true).SingleOrDefault();
				AssertEquals(0, form.ConsignmentsGrid.List.Count);
				var attachButton = buttonGrid.AttachButtonForTest;
				attachButton.PerformClick();

				var modulePopUp = buttonGrid.LastShownAttachPopupForTesting;
				AssertNotNull(modulePopUp);
				AssertEquals(3, modulePopUp.Module_ForTest.FilterBusinessObject.ActiveModuleFilters.Count);
				var shipmentFilter = (ModuleGuidFilter)modulePopUp.Module_ForTest.FilterBusinessObject.ActiveModuleFilters.FirstOrDefault(x => x.Code == "Shipments");
				shipmentFilter.Property = shipment.PK;
				modulePopUp.Module_ForTest.PerformSearch_ForTest();
				AssertEquals(1, modulePopUp.Module_ForTest.DisplayGrid.ListManager.Count);

				modulePopUp.FocusFirstRecord();

				modulePopUp.ExposedOKButtonForTesting.PerformClick();
				AssertEquals(1, form.ConsignmentsGrid.List.Count);
				modulePopUp.Close();

				form.FireSaveButton();

				var reloadedConsignment = new BusinessObjectFactory().Load<HVLVConsignment>(consignment.PK);
				AssertEquals(headerToAttach.PK, reloadedConsignment.HVC_HCH_Header);
				AssertEquals(true, reloadedConsignment.HVC_IsActive);
				AssertEquals("BKD", reloadedConsignment.HVC_Status);
				AssertEquals(10m, reloadedConsignment.HVC_ManifestedVolume);
				AssertEquals(10m, reloadedConsignment.HVC_ManifestedWeight);
				AssertEquals(1, (int)reloadedConsignment.HVC_ItemCount);
				AssertEquals(shipmentToAttach.PK, item.HVI_JS_LoadedOnShipment);
			}
		}

		public void TestDetachButton_DetachConsignmentSuccessfully()
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = "HVL";
			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment = header.Consignments.AddNew();
			consignment.HVC_IsActive = true;
			consignment.HVC_Status = "BKD";
			consignment.HVC_ConsigneeName = "name";
			consignment.HVC_ConsigneeAddress1 = "address";
			consignment.HVC_ConsigneeCity = "city";
			consignment.HVC_ConsigneePostcode = "3053";
			consignment.HVC_ShipperName = "name";
			consignment.HVC_ShipperAddress1 = "address";
			consignment.HVC_ShipperCity = "city";
			consignment.HVC_RN_NKConsigneeCountryCode = "CN";
			factory.Save();

			using (var form = new ConsignmentUserControlTestForm(header))
			{
				form.Show();
				var buttonGrid = (ZModuleButtonGrid)form.Controls.Find("consignmentsGrid", true).SingleOrDefault();
				AssertEquals(1, form.ConsignmentsGrid.List.Count);
				buttonGrid.SelectFirstRowIfOnlyRowInGrid();
				var detachedButton = buttonGrid.DetachButtonForTest;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				detachedButton.PerformClick();
				AssertEquals("Question Are you sure you want to detach the selected Consignment(s)?\r\nAll unsaved changes in detached items will be canceled.", UnitTestUserNotification.Instance.LastMessage.ToString());

				AssertEquals(0, form.ConsignmentsGrid.List.Count);

				var result = form.FireSaveButton();

				var reloadedConsignment = new BusinessObjectFactory().Load<HVLVConsignment>(consignment.PK);
				AssertEquals(header.PK, reloadedConsignment.HVC_HCH_Header);
				AssertEquals(false, reloadedConsignment.HVC_IsActive);
				AssertEquals("DTC", reloadedConsignment.HVC_Status);
			}
		}

		public void TestDetachButton_ShowErrorMessage()
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0000001";
			shipment.JS_ShipmentType = "HVL";
			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = header.Consignments.AddNew();
			consignment.HVC_IsActive = true;
			consignment.HVC_Status = "HLD";
			consignment.HVC_ConsignmentId = "C0000001";
			factory.Save();

			using (var form = new ConsignmentUserControlTestForm(header))
			{
				form.Show();
				var buttonGrid = (ZModuleButtonGrid)form.Controls.Find("consignmentsGrid", true).SingleOrDefault();
				AssertEquals(1, form.ConsignmentsGrid.List.Count);
				buttonGrid.SelectFirstRowIfOnlyRowInGrid();
				var detachedButton = buttonGrid.DetachButtonForTest;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				detachedButton.PerformClick();
				AssertEquals("Error Unable to detach Consignment C0000001 from Shipment S0000001 since Consignment Status = Customs Held at Destination. ", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}
	}
}
