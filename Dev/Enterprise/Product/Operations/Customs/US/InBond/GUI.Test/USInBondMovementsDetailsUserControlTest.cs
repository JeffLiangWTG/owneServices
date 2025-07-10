using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.InBond.GUI.Testing
{
	sealed class USInBondMovementsDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestPartAttributeCaptions()
		{
			var org = Factory.New<OrgHeader>();
			org.MiscServ.OM_IMPartAttrib1Name = "VIN1";
			org.MiscServ.OM_IMPartAttrib2Name = "VIN2";
			org.MiscServ.OM_IMPartAttrib3Name = "VIN3";
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = org.MainAddress.PK;
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			var commodity = container.Commodities.AddNew();
			using (var form = new USInBondForm(header))
			{
				form.Show();
				var tabControl = ((IFormPlugInsProvider)form).TopLevelTabControl;
				tabControl.SelectedTab = form.MovementDetailsTabPage;
				var foundControls = form.MovementDetailsTabPage.Controls.Find("MoveDetailContainerCommoditiesGrid", true);
				var commoditiesGrid = foundControls.Length > 0 ? (ZGrid)foundControls[0] : null;
				AssertEquals("BY_PartAttrib1 caption", "VIN1", commoditiesGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_PartAttrib1).Caption);
				AssertEquals("BY_PartAttrib2 caption", "VIN2", commoditiesGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_PartAttrib2).Caption);
				AssertEquals("BY_PartAttrib3 caption", "VIN3", commoditiesGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_PartAttrib3).Caption);
				foundControls = tabControl.Controls.Find("MainTabPage", true);
				var mainTabPage = foundControls.Length > 0 ? (ZTabPage)foundControls[0] : null;
				tabControl.SelectedTab = mainTabPage;
				header.ImporterOrgPK = ZGuid.Empty;
				tabControl.SelectedTab = form.MovementDetailsTabPage;
				foundControls = form.MovementDetailsTabPage.Controls.Find("MoveDetailContainerCommoditiesGrid", true);
				commoditiesGrid = foundControls.Length > 0 ? (ZGrid)foundControls[0] : null;
				AssertEquals("BY_PartAttrib1 caption", "Part Attrib. 1", commoditiesGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_PartAttrib1).Caption);
				AssertEquals("BY_PartAttrib2 caption", "Part Attrib. 2", commoditiesGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_PartAttrib2).Caption);
				AssertEquals("BY_PartAttrib3 caption", "Part Attrib. 3", commoditiesGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_PartAttrib3).Caption);
			}
		}

		public void TestInventorySelection()
		{
			var importer = Factory.New<OrgHeader>();
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var helper = new WhsDataTestHelper(Factory);
			helper.GetNewWhsWarehouse(importer.MainAddress.PK, true, "HO2");
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			header.BH_FTZMove = true;
			var bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "XJ5-32423";
			var bill2 = header.Bills.AddNew();
			bill2.B0_MasterBillNumber = "XJ5-79565";
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
			var moveDetail1 = moveHeader.MovementDetails.AddNew(bill1.PK);
			var container1 = moveDetail1.Containers.AddNew();
			var commodity1 = container1.Commodities.AddNew();
			commodity1.BY_PartNumber = "PART1";
			commodity1.BY_WarehouseEntryNumber = "XJ5-32423";
			var moveDetail2 = moveHeader.MovementDetails.AddNew(bill2.PK);
			var container2 = moveDetail2.Containers.AddNew();
			var commodity2 = container2.Commodities.AddNew();
			commodity2.BY_PartNumber = "PART1";
			commodity2.BY_WarehouseEntryNumber = "XJ5-79565";
			using (var form = new USInBondForm(header))
			{
				form.Show();
				var tabControl = ((IFormPlugInsProvider)form).TopLevelTabControl;
				tabControl.SelectedTab = form.MovementDetailsTabPage;
				var foundControls = form.MovementDetailsTabPage.Controls.Find("MovementDetailsGrid", true);
				var movementDetailsGrid = foundControls.Length > 0 ? (ZGrid)foundControls[0] : null;
				AssertNotNull("MovementDetailsGrid", movementDetailsGrid);
				foundControls = form.MovementDetailsTabPage.Controls.Find("MoveDetailContainersGrid", true);
				var moveDetailContainersGrid = foundControls.Length > 0 ? (ZGrid)foundControls[0] : null;
				AssertNotNull("MoveDetailContainersGrid", moveDetailContainersGrid);
				foundControls = form.MovementDetailsTabPage.Controls.Find("InventorySelectionButton", true);
				var inventorySelectionButton = foundControls.Length > 0 ? (ZButton)foundControls[0] : null;
				AssertNotNull("InventorySelectionButton", inventorySelectionButton);
				movementDetailsGrid.Focus();
				movementDetailsGrid.ListManager.Position = 1;
				AssertEquals(moveDetail2, movementDetailsGrid.ListManager.GetCurrent());
				moveDetailContainersGrid.ListManager.Position = 0;
				AssertEquals(container2, moveDetailContainersGrid.ListManager.GetCurrent());
				movementDetailsGrid.ListManager.Position = 0;
				AssertEquals(moveDetail1, movementDetailsGrid.ListManager.GetCurrent());
				inventorySelectionButton.PerformClick();
				var selectionHeader = (ContainerInventorySelectionHeader)ZFormModaliser.LastIBusinessShownOnDialogForTest;
				var filters = selectionHeader.GetFilterDefaults();
				var filter = filters["Customs Entry Key:Property"];
				AssertEquals("XJ5-32423", filter.Value);
				AssertEquals(true, inventorySelectionButton.Visible);
			}
		}

		public void TestInventorySelectionWhenMoveHeaderDeleted()
		{
			var importer = Factory.New<OrgHeader>();
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			importer.OH_Code = "TSTORG";
			var helper = new WhsDataTestHelper(Factory);
			helper.GetNewWhsWarehouse(importer.MainAddress.PK, true, "HO2");
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			header.BH_FTZMove = true;
			var bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "XJ5-32423";
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
			var moveDetail1 = moveHeader.MovementDetails.AddNew(bill1.PK);
			var container1 = moveDetail1.Containers.AddNew();
			var commodity1 = container1.Commodities.AddNew();
			commodity1.BY_PartNumber = "PART1";
			commodity1.BY_WarehouseEntryNumber = "XJ5-32423";
			Factory.Save();
			using (var form = new USInBondForm(header))
			{
				form.Show();
				var tabControl = ((IFormPlugInsProvider)form).TopLevelTabControl;
				tabControl.SelectedTab = form.MovementDetailsTabPage;
				var foundControls = form.MovementDetailsTabPage.Controls.Find("MovementDetailsGrid", true);
				var movementDetailsGrid = foundControls.Length > 0 ? (ZGrid)foundControls[0] : null;
				AssertNotNull("MovementDetailsGrid", movementDetailsGrid);
				foundControls = form.MovementDetailsTabPage.Controls.Find("MoveDetailContainersGrid", true);
				var moveDetailContainersGrid = foundControls.Length > 0 ? (ZGrid)foundControls[0] : null;
				AssertNotNull("MoveDetailContainersGrid", moveDetailContainersGrid);
				foundControls = form.MovementDetailsTabPage.Controls.Find("InventorySelectionButton", true);
				var inventorySelectionButton = foundControls.Length > 0 ? (ZButton)foundControls[0] : null;
				AssertNotNull("InventorySelectionButton", inventorySelectionButton);
				movementDetailsGrid.Focus();
				movementDetailsGrid.ListManager.Position = 0;
				moveDetailContainersGrid.ListManager.Position = 0;
				AssertEquals(moveDetail1, movementDetailsGrid.ListManager.GetCurrent());
				moveDetail1.B9_BM = Guid.Empty;
				AssertNoExceptionThrown(() =>
				{
					inventorySelectionButton.PerformClick();
				});
				AssertEquals(false, inventorySelectionButton.Visible);
			}
		}

		public void TestBillColumnCaption()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				var header = Factory.New<CusInBondHeader>();
				header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselNonContainer;
				var moveHeader = header.MovementHeaders.AddNew();
				var bill = header.Bills.AddNew();
				var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
				using (var form = new USInBondForm(header))
				{
					form.Show();
					var tabControl = ((IFormPlugInsProvider)form).TopLevelTabControl;
					tabControl.SelectedTab = form.MovementDetailsTabPage;
					var foundControls = form.MovementDetailsTabPage.Controls.Find("MovementDetailsGrid", true);
					var movementDetailsGrid = foundControls.Length > 0 ? (ZGrid)foundControls[0] : null;
					AssertNotNull("MovementDetailsGrid", movementDetailsGrid);
					AssertEquals("Bill", movementDetailsGrid.GetColumnCaption(CusInBondMoveDetail.Schema.B9_B0));
					header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
					AssertEquals("Bill", movementDetailsGrid.GetColumnCaption(CusInBondMoveDetail.Schema.B9_B0));
					header.BH_ImportTransportMode = TransportModeCodes.Codes.RailContainer;
					AssertEquals("Master Bill", movementDetailsGrid.GetColumnCaption(CusInBondMoveDetail.Schema.B9_B0));
				}
			}
		}
	}
}
