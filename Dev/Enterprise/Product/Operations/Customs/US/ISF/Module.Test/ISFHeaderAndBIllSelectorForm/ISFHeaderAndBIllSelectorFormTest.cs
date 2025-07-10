using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.Module.Testing
{
	[TestedType(typeof(ISFHeaderAndBIllSelectorForm))]
	sealed class ISFHeaderAndBIllSelectorFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		public void TestCreateShipment()
		{
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CONS";
			consignor.OH_IsConsignor = true;
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CONSE";
			consignee.OH_IsConsignee = true;
			OrgHeader importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "IMPORTER";
			importer.OH_Code = "IMPZZZ1";
			importer.MainAddress.OA_Address1 = "ADDRESS 1";
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_OH_Importer = importer.PK;
			header.BF_MasterBill = "MB1";
			header.BF_HouseBill = "HB1";
			CusISFBill bill2 = header.ReferenceDatas.AddNew();
			bill2.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			bill2.BB_BillNum = "HB2";
			CusISFBill bill3 = header.ReferenceDatas.AddNew();
			bill3.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			bill3.BB_BillNum = "HB3";
			Factory.Save();
			ISFHeaderRow headerRow = new ISFHeaderRow(header);
			AssertEquals(3, headerRow.Bills.Count);
			using (ISFHeaderAndBIllSelectorForm form = new ISFHeaderAndBIllSelectorForm(headerRow))
			{
				form.Show();
				ZButton createButton = (ZButton)form.Controls["BottomPanel"].Controls["CreateButton"];
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				AssertEquals(true, createButton.Enabled);
				createButton.PerformClick();
				AssertEquals("Should be disable to stop duplicate clicks", false, createButton.Enabled);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				ZForm consolForm = ZFormModaliser.LastFormShownForTest as ZForm;
				AssertContains(string.Format(" With Data From {0}", header.HumanReadableName), consolForm.Text);
				consolForm.Close();
				AssertNotEquals("Previous shipment create was cancelled.\r\nWould you like to continue with the rest?", UnitTestUserNotification.Instance.LastMessage.Text);
				ZForm shipmentForm1 = ZFormModaliser.LastFormShownForTest as ZForm;
				AssertEquals("New Shipment With Data From House Bill of Lading:HB1 (1 of 3 Shipments)", shipmentForm1.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ForwardingShipment shipment1 = shipmentForm1.BusinessEntity as ForwardingShipment;
				shipment1.ConsignorPK = consignor.PK;
				shipment1.ConsigneePK = consignee.PK;
				UpdateShipmentForSaving(shipment1);
				AssertEquals("Should set Shipment State", ChildEditableServiceStates.Shipment, ChildEditableService.GetState(shipment1.Factory));
				shipmentForm1.Close();
				AssertNotEquals("Previous shipment create was cancelled.\r\nWould you like to continue with the rest?", UnitTestUserNotification.Instance.LastMessage.Text);
				ZForm shipmentForm2 = ZFormModaliser.LastFormShownForTest as ZForm;
				AssertNotEquals(shipmentForm1, shipmentForm2);
				AssertEquals("New Shipment With Data From House Bill of Lading:HB2 (2 of 3 Shipments)", shipmentForm2.Text);
				ForwardingShipment shipment2 = shipmentForm2.BusinessEntity as ForwardingShipment;
				UpdateShipmentForSaving(shipment2);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				shipmentForm2.Close();
				AssertEquals("Previous shipment create was cancelled.\r\nWould you like to continue with the rest?", UnitTestUserNotification.Instance.LastMessage.Text);
				ZForm shipmentForm3 = ZFormModaliser.LastFormShownForTest as ZForm;
				AssertEquals(shipmentForm2, shipmentForm3);
				AssertEquals(false, shipmentForm2.Visible);
			}
		}

		public void TestTickAndUntickLine()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_MasterBill = "MB1";
			header.BF_HouseBill = "HB1";
			CusISFLine line1 = header.Lines.AddNew();
			line1.BL_HarmonisedNum = "10.10.1010";
			CusISFLine line2 = header.Lines.AddNew();
			line2.BL_HarmonisedNum = "20.20.2020";
			CusISFLine line3 = header.Lines.AddNew();
			line3.BL_HarmonisedNum = "30.30.3030";
			ISFHeaderRow headerRow = new ISFHeaderRow(header);
			AssertEquals(1, headerRow.Bills.Count);
			ISFBillRow billRow = headerRow.Bills[0];
			using (ISFHeaderAndBIllSelectorForm form = new ISFHeaderAndBIllSelectorForm(headerRow))
			{
				form.Show();
				ZGrid lineGrid = (ZGrid)form.Controls["LineGroupBox"].Controls["LineGrid"];
				lineGrid.SelectAllElements();
				AssertEquals(3, lineGrid.SelectedElements.Length);
				ISFLineRow lineRow1 = (ISFLineRow)lineGrid.SelectedElements[0];
				ISFLineRow lineRow2 = (ISFLineRow)lineGrid.SelectedElements[1];
				ISFLineRow lineRow3 = (ISFLineRow)lineGrid.SelectedElements[2];
				MenuItem tickMenuItem = lineGrid.ContextMenu.MenuItems.FindByText("Tick Copy");
				MenuItem unTickMenuItem = lineGrid.ContextMenu.MenuItems.FindByText("Un-Tick Copy");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				unTickMenuItem.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, lineRow1.ShouldCopy);
				AssertEquals(false, lineRow2.ShouldCopy);
				AssertEquals(false, lineRow3.ShouldCopy);
				tickMenuItem.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, lineRow1.ShouldCopy);
				AssertEquals(true, lineRow2.ShouldCopy);
				AssertEquals(true, lineRow3.ShouldCopy);
				lineGrid.UnSelectAll();
				unTickMenuItem.PerformClick();
				AssertEquals(ISFHeaderAndBIllSelectorForm.SelectAtLeastOneLine, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				tickMenuItem.PerformClick();
				AssertEquals(ISFHeaderAndBIllSelectorForm.SelectAtLeastOneLine, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				lineGrid.Select(1);
				unTickMenuItem.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, lineRow1.ShouldCopy);
				AssertEquals(false, lineRow2.ShouldCopy);
				AssertEquals(true, lineRow3.ShouldCopy);
				tickMenuItem.PerformClick();
				AssertEquals(true, lineRow1.ShouldCopy);
				AssertEquals(true, lineRow2.ShouldCopy);
				AssertEquals(true, lineRow3.ShouldCopy);
			}
		}

		public void TestNoExcepitonOnSecurityRestriction()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "IMPORTER";
			importer.OH_Code = "IMPZZZ1";
			importer.MainAddress.OA_Address1 = "ADDRESS 1";
			var header = Factory.New<CusISFHeader>();
			header.BF_OH_Importer = importer.PK;
			header.BF_MasterBill = "MB1";
			header.BF_HouseBill = "HB1";
			Factory.Save();
			var headerRow = new ISFHeaderRow(header);
			Environment.Env.Security.MaintainConsolNew.IsAllowed = false;
			using (var form = new ISFHeaderAndBIllSelectorForm(headerRow))
			{
				form.Show();
				ZButton createButton = (ZButton)form.Controls["BottomPanel"].Controls["CreateButton"];
				AssertEquals(true, createButton.Enabled);
				AssertExceptionThrown<SecurityAccessDeniedException>("A new form should not have been created.", () => createButton.PerformClick());
			}
		}

		public void TestTickAndUntickContainer()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_MasterBill = "MB1";
			header.BF_HouseBill = "HB1";
			CusISFEquip container1 = header.Equipments.AddNew();
			container1.BE_ContainerNum = "CONT1";
			CusISFEquip container2 = header.Equipments.AddNew();
			container2.BE_ContainerNum = "CONT2";
			CusISFEquip container3 = header.Equipments.AddNew();
			container3.BE_ContainerNum = "CONT3";
			ISFHeaderRow headerRow = new ISFHeaderRow(header);
			using (ISFHeaderAndBIllSelectorForm form = new ISFHeaderAndBIllSelectorForm(headerRow))
			{
				form.Show();
				ZGrid containerGrid = (ZGrid)form.Controls["ContainerGroupBox"].Controls["ContainerGrid"];
				containerGrid.SelectAllElements();
				AssertEquals(3, containerGrid.SelectedElements.Length);
				ISFContainerRow containerRow1 = (ISFContainerRow)containerGrid.SelectedElements[0];
				ISFContainerRow containerRow2 = (ISFContainerRow)containerGrid.SelectedElements[1];
				ISFContainerRow containerRow3 = (ISFContainerRow)containerGrid.SelectedElements[2];
				MenuItem tickMenuItem = containerGrid.ContextMenu.MenuItems.FindByText("Tick Copy");
				MenuItem unTickMenuItem = containerGrid.ContextMenu.MenuItems.FindByText("Un-Tick Copy");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				unTickMenuItem.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, containerRow1.ShouldCopy);
				AssertEquals(false, containerRow2.ShouldCopy);
				AssertEquals(false, containerRow3.ShouldCopy);
				tickMenuItem.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, containerRow1.ShouldCopy);
				AssertEquals(true, containerRow2.ShouldCopy);
				AssertEquals(true, containerRow3.ShouldCopy);
				containerGrid.UnSelectAll();
				unTickMenuItem.PerformClick();
				AssertEquals(ISFHeaderAndBIllSelectorForm.SelectAtLeastOneContainer, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				tickMenuItem.PerformClick();
				AssertEquals(ISFHeaderAndBIllSelectorForm.SelectAtLeastOneContainer, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				containerGrid.Select(1);
				unTickMenuItem.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, containerRow1.ShouldCopy);
				AssertEquals(false, containerRow2.ShouldCopy);
				AssertEquals(true, containerRow3.ShouldCopy);
				tickMenuItem.PerformClick();
				AssertEquals(true, containerRow1.ShouldCopy);
				AssertEquals(true, containerRow2.ShouldCopy);
				AssertEquals(true, containerRow3.ShouldCopy);
			}
		}

		protected override Form GetFormToBashCore()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			CusISFBill bill = header.ReferenceDatas.AddNew();
			bill.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			bill.BB_BillNum = "HB23423";
			ISFHeaderRow headerRow = new ISFHeaderRow(header);
			((IBusinessObjectState)headerRow).ClearHasChangesIncludingChildren();
			return new ISFHeaderAndBIllSelectorForm(headerRow);
		}

		void UpdateShipmentForSaving(ForwardingShipment shipment)
		{
			shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.OriginalReq;
			var job = shipment.ShipmentJobHeader;
			if (job != null)
			{
				job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery()).PK;
			}

			shipment.RunPreSaveValidation();
		}
	}
}
