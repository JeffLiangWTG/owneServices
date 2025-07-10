using System;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class CustomsDataGridHelperTest : WhsTestCaseWithFactory
	{
		#region TestShowHideCustomsData

		public void TestShowHideCustomsData()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var dynamicWorkOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1);

			using (var form = new ZForm(order))
			{
				var grid = new ZGrid();
				grid.DataSource = order;
				grid.DataMember = "Lines";

				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("CustomsTariffLookup", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("CustomsTariffItem", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("CustomsTariffDesc", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("WE_BondedEntryKey", 80)); // inwards
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("CustomsData+WB_EntryKey", 80));
				grid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo("CustomsData+WB_EntryLineNo", 80, 2));
				grid.ColumnStyles.Add(new ZDateEditColumnStyleInfo("CustomsData+WB_EntryDate", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("CustomsData+WB_DeclarationReference", 80));
				grid.ColumnStyles.Add(new ZDateEditColumnStyleInfo("CustomsData+WB_CustomsDeadline", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("CustomsData+WB_InwardStyle", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("CustomsData+WB_InwardProcedure", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("CustomsData+WB_RN_NKCountryOfOrigin", 80));
				grid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo("CustomsData+WB_CustomsQty", 80, 2));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("CustomsData+WB_CustomsUnitOfQty", 80));
				grid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo("CustomsData+WB_ValueForDuty", 80, 2));
				grid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo("CustomsData+WB_TILV", 80, 2));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("CustomsData+WB_AddInfo", 80));
				grid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo("CustomsData+WB_CustomsSecondQuantity", 80, 2));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("CustomsData+WB_CustomsSecondUnitQty", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("CustomsData+WB_Tariff", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("CustomsData+WB_PrimaryPreference", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("WE_PackageGroupId", 80));
				grid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo("CustomsData+WB_CustomsThirdQuantity", 80, 2));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("CustomsData+WB_CustomsThirdUnitQty", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("CustomsData+ManufacturerCode", 80));
				grid.ColumnStyles.Add(new ZAddressDropEditColumnStyleInfo() { ColumnName = "CustomsData+WB_OA_ManufacturerAddress" });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("CustomsData+WB_IsMainInwardsProcessedItem", 80));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("CustomsData+WB_IsSecondaryInwardsProcessedItem", 80));

				form.Controls.Add(grid);
				form.Show();

				CustomsDataGridHelper.ShowHideCustomsData(grid, order, new[] { "WE_PackageGroupId" });
				AssertColumnsAvailability(grid, areColumnsAvailable: false, areUSColumnsAvailable: false, isBondedEntryKeyVisible: false);

				data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "SGSIN";
				Helper.EnableWarehouseForBond(data.Whs1, true);
				order.WD_DocketSubType = OrderType.Codes.Customs;
				CustomsDataGridHelper.ShowHideCustomsData(grid, order, new[] { "WE_PackageGroupId" });
				AssertColumnsAvailability(grid, areColumnsAvailable: true, areUSColumnsAvailable: false, isBondedEntryKeyVisible: true);

				data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUSYD";
				order.WD_DocketSubType = "";
				order.WD_DocketSubType = OrderType.Codes.Customs;
				CustomsDataGridHelper.ShowHideCustomsData(grid, order, new[] { "WE_PackageGroupId" });
				AssertColumnsAvailability(grid, areColumnsAvailable: true, areUSColumnsAvailable: false, isBondedEntryKeyVisible: true);

				data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
				CustomsDataGridHelper.ShowHideCustomsData(grid, order, new[] { "WE_PackageGroupId" });
				AssertColumnsAvailability(grid, areColumnsAvailable: true, areUSColumnsAvailable: true, isBondedEntryKeyVisible: true);

				receive.WD_DocketSubType = OrderType.Codes.Customs;
				CustomsDataGridHelper.ShowHideCustomsData(grid, receive, new[] { "WE_PackageGroupId" });
				AssertColumnsAvailability(grid, areColumnsAvailable: true, areUSColumnsAvailable: true, isBondedEntryKeyVisible: false);

				var iprArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
				data.Whs1.WW_IsVirtualWarehouse = true;
				workOrder.WD_IsInwardsProcessingJob = true;
				CustomsDataGridHelper.ShowHideCustomsData(grid, workOrder, new[] { "WE_PackageGroupId" });
				AssertColumnsAvailability(grid, areColumnsAvailable: false, areUSColumnsAvailable: false, isBondedEntryKeyVisible: false);

				dynamicWorkOrder.WD_IsInwardsProcessingJob = true;
				CustomsDataGridHelper.ShowHideCustomsData(grid, dynamicWorkOrder, new[] { "WE_PackageGroupId" });
				AssertColumnsAvailability(grid, areColumnsAvailable: false, areUSColumnsAvailable: false, isBondedEntryKeyVisible: true);
			}
		}

		void AssertColumnsAvailability(ZGrid grid, bool areColumnsAvailable, bool areUSColumnsAvailable, bool isBondedEntryKeyVisible)
		{
			var columnStyles = grid.ColumnStyles.Cast<ZGridColumnInfo>();

			// Bonded
			AssertEquals(!areColumnsAvailable, columnStyles.Single(i => i.ColumnName == "CustomsTariffLookup").IsUnavailable);
			AssertEquals(!areColumnsAvailable, columnStyles.Single(i => i.ColumnName == "CustomsTariffItem").IsUnavailable);
			AssertEquals(!areColumnsAvailable, columnStyles.Single(i => i.ColumnName == "CustomsTariffDesc").IsUnavailable);
			AssertEquals(!areColumnsAvailable, columnStyles.Single(i => i.ColumnName == "CustomsData+WB_EntryKey").IsUnavailable);
			AssertEquals(!areColumnsAvailable, columnStyles.Single(i => i.ColumnName == "CustomsData+WB_EntryLineNo").IsUnavailable);
			AssertEquals(!areColumnsAvailable, columnStyles.Single(i => i.ColumnName == "CustomsData+WB_EntryDate").IsUnavailable);
			AssertEquals(!areColumnsAvailable, columnStyles.Single(i => i.ColumnName == "CustomsData+WB_DeclarationReference").IsUnavailable);
			AssertEquals(!areColumnsAvailable, columnStyles.Single(i => i.ColumnName == "CustomsData+WB_CustomsDeadline").IsUnavailable);
			AssertEquals(!areColumnsAvailable, columnStyles.Single(i => i.ColumnName == "CustomsData+WB_InwardStyle").IsUnavailable);
			AssertEquals(!areColumnsAvailable, columnStyles.Single(i => i.ColumnName == "CustomsData+WB_InwardProcedure").IsUnavailable);
			AssertEquals(!areColumnsAvailable, columnStyles.Single(i => i.ColumnName == "CustomsData+WB_RN_NKCountryOfOrigin").IsUnavailable);
			AssertEquals(!areColumnsAvailable, columnStyles.Single(i => i.ColumnName == "CustomsData+WB_CustomsQty").IsUnavailable);
			AssertEquals(!areColumnsAvailable, columnStyles.Single(i => i.ColumnName == "CustomsData+WB_CustomsUnitOfQty").IsUnavailable);
			AssertEquals(!areColumnsAvailable, columnStyles.Single(i => i.ColumnName == "CustomsData+WB_ValueForDuty").IsUnavailable);
			AssertEquals(!areColumnsAvailable, columnStyles.Single(i => i.ColumnName == "CustomsData+WB_TILV").IsUnavailable);
			AssertEquals(!areColumnsAvailable, columnStyles.Single(i => i.ColumnName == "CustomsData+WB_AddInfo").IsUnavailable);
			AssertEquals(!areColumnsAvailable, columnStyles.Single(i => i.ColumnName == "CustomsData+WB_CustomsSecondQuantity").IsUnavailable);
			AssertEquals(!areColumnsAvailable, columnStyles.Single(i => i.ColumnName == "CustomsData+WB_CustomsSecondUnitQty").IsUnavailable);
			AssertEquals(!areColumnsAvailable, columnStyles.Single(i => i.ColumnName == "CustomsData+WB_Tariff").IsUnavailable);
			AssertEquals(!areColumnsAvailable, columnStyles.Single(i => i.ColumnName == "CustomsData+WB_PrimaryPreference").IsUnavailable);
			AssertEquals(!areColumnsAvailable, columnStyles.Single(i => i.ColumnName == "CustomsData+WB_CustomsThirdQuantity").IsUnavailable);
			AssertEquals(!areColumnsAvailable, columnStyles.Single(i => i.ColumnName == "CustomsData+WB_CustomsThirdUnitQty").IsUnavailable);
			AssertEquals(!areColumnsAvailable, columnStyles.Single(i => i.ColumnName == "CustomsData+ManufacturerCode").IsUnavailable);
			AssertEquals(!areColumnsAvailable, columnStyles.Single(i => i.ColumnName == "CustomsData+WB_OA_ManufacturerAddress").IsUnavailable);
			AssertEquals(!areColumnsAvailable, columnStyles.Single(i => i.ColumnName == "CustomsData+WB_IsMainInwardsProcessedItem").IsUnavailable);
			AssertEquals(!areColumnsAvailable, columnStyles.Single(i => i.ColumnName == "CustomsData+WB_IsSecondaryInwardsProcessedItem").IsUnavailable);

			// US
			AssertEquals(!areUSColumnsAvailable, columnStyles.Single(i => i.ColumnName == "WE_PackageGroupId").IsUnavailable);

			// Order
			AssertEquals(!isBondedEntryKeyVisible, columnStyles.Single(i => i.ColumnName == "WE_BondedEntryKey").IsUnavailable);
		}

		#endregion

		#region TestShowHideCustomsData_USFTZ

		public void TestShowHideCustomsData_USFTZ()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = OrderType.Codes.Customs;
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_DocketSubType = OrderType.Codes.Customs;

			using (var form = new ZForm(order))
			{
				var grid = new ZGrid();
				grid.DataSource = order;
				grid.DataMember = "Lines";

				grid.ColumnStyles.Add(new ZGuidDropEditColumnStyleInfo() { ColumnName = "CustomsData+WB_ZoneStatus" });
				grid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo("CustomsData+WB_IsFromAnotherFTZWhs", 80));
				grid.ColumnStyles.Add(new ZGuidDropEditColumnStyleInfo() { ColumnName = "CustomsData+WB_OutwardType" });

				form.Controls.Add(grid);
				form.Show();

				AssertEquals("Precondition", false, data.Whs1.IsFTZBondedEnabledAndUSJurisdiction());
				AssertUSFTZColumns(receive, grid, expectedZoneStatusIsAvailable: false, expectedIsFromAnotherFTZWhsIsAvailable: false, expectedOutwardTypeIsAvailable: false);

				data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUSYD";
				AssertEquals("Precondition", false, data.Whs1.IsFTZBondedEnabledAndUSJurisdiction());
				AssertUSFTZColumns(receive, grid, expectedZoneStatusIsAvailable: false, expectedIsFromAnotherFTZWhsIsAvailable: false, expectedOutwardTypeIsAvailable: false);

				data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
				AssertEquals("Precondition", false, data.Whs1.IsFTZBondedEnabledAndUSJurisdiction());
				AssertUSFTZColumns(receive, grid, expectedZoneStatusIsAvailable: false, expectedIsFromAnotherFTZWhsIsAvailable: false, expectedOutwardTypeIsAvailable: false);

				AssertEquals("Precondition", false, data.Whs1.IsFTZBondedEnabledAndUSJurisdiction());
				AssertUSFTZColumns(order, grid, expectedZoneStatusIsAvailable: false, expectedIsFromAnotherFTZWhsIsAvailable: false, expectedOutwardTypeIsAvailable: false);

				data.Whs1.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
				AssertEquals("Precondition", true, data.Whs1.IsFTZBondedEnabledAndUSJurisdiction());
				AssertUSFTZColumns(order, grid, expectedZoneStatusIsAvailable: true, expectedIsFromAnotherFTZWhsIsAvailable: true, expectedOutwardTypeIsAvailable: true);
				AssertUSFTZColumns(receive, grid, expectedZoneStatusIsAvailable: true, expectedIsFromAnotherFTZWhsIsAvailable: true, expectedOutwardTypeIsAvailable: false);
			}
		}

		static void AssertUSFTZColumns(WhsDocket docket, ZGrid grid, bool expectedZoneStatusIsAvailable, bool expectedIsFromAnotherFTZWhsIsAvailable, bool expectedOutwardTypeIsAvailable)
		{
			CustomsDataGridHelper.ShowHideCustomsData(grid, docket, Array.Empty<string>());
			var columnStyles = grid.ColumnStyles.Cast<ZGridColumnInfo>();
			AssertEquals(!expectedZoneStatusIsAvailable, columnStyles.Single(i => i.ColumnName == "CustomsData+WB_ZoneStatus").IsUnavailable);
			AssertEquals(!expectedIsFromAnotherFTZWhsIsAvailable, columnStyles.Single(i => i.ColumnName == "CustomsData+WB_IsFromAnotherFTZWhs").IsUnavailable);
			AssertEquals(!expectedOutwardTypeIsAvailable, columnStyles.Single(i => i.ColumnName == "CustomsData+WB_OutwardType").IsUnavailable);
		}

		#endregion

	}
}
