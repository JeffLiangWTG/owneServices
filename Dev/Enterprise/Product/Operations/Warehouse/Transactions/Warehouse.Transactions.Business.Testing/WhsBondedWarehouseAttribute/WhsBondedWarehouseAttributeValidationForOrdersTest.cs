using System;
using System.Linq;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsBondedWarehouseAttributeValidationForOrdersTest : WhsBondedWarehouseAttributeValidationTest
	{
		#region TestOrderLine_CheckWB_EntryKey

		#region TestOrderLine_CheckWB_EntryKey_OrderSubTypeCUS

		public void TestOrderLine_CheckWB_EntryKey_OrderSubTypeCUS_CountryUS()
		{
			TestOrderLine_CheckWB_EntryKey(OrderType.Codes.Customs, Core.Constants.CountryCodes.UnitedStates, expectErrorWhenEmpty: true);
		}

		public void TestOrderLine_CheckWB_EntryKey_OrderSubTypeCUS_CountryPR()
		{
			TestOrderLine_CheckWB_EntryKey(OrderType.Codes.Customs, Core.Constants.CountryCodes.PuertoRico, expectErrorWhenEmpty: true);
		}

		public void TestOrderLine_CheckWB_EntryKey_OrderSubTypeCUS_CountryAU()
		{
			TestOrderLine_CheckWB_EntryKey(OrderType.Codes.Customs, Core.Constants.CountryCodes.Australia, expectErrorWhenEmpty: true);
		}

		#endregion

		#region TestOrderLine_CheckWB_EntryKey_OrderSubTypeORD

		public void TestOrderLine_CheckWB_EntryKey_OrderSubTypeORD_CountryUS()
		{
			TestOrderLine_CheckWB_EntryKey(OrderType.Codes.Order, Core.Constants.CountryCodes.UnitedStates, expectErrorWhenEmpty: false);
		}

		public void TestOrderLine_CheckWB_EntryKey_OrderSubTypeORD_CountryPR()
		{
			TestOrderLine_CheckWB_EntryKey(OrderType.Codes.Order, Core.Constants.CountryCodes.PuertoRico, expectErrorWhenEmpty: false);
		}

		public void TestOrderLine_CheckWB_EntryKey_OrderSubTypeORD_CountryAU()
		{
			TestOrderLine_CheckWB_EntryKey(OrderType.Codes.Order, Core.Constants.CountryCodes.Australia, expectErrorWhenEmpty: false);
		}

		#endregion

		#region TestOrderLine_CheckWB_EntryKey

		void TestOrderLine_CheckWB_EntryKey(string orderSubType, string warehouseCountryCode, bool expectErrorWhenEmpty)
		{
			const string errorMsg = "Entry Number is mandatory for Customs Jobs.";

			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;
			whs.WarehouseAddress.OA_RL_NKRelatedPortCode = Helper.GetCountryUNLOCO(warehouseCountryCode).RL_Code;

			Helper.EnableWarehouseForBond(whs, orderSubType == OrderType.Codes.Customs);
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs, "R1", data.Part1, 100m);

			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = orderSubType;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var customsData = orderLine.CustomsData;
			customsData.Validation.ValidateWB_EntryKey();
			if (expectErrorWhenEmpty)
			{
				AssertHasError("Entry key must be mandatory.", customsData.WB_EntryKeyInfo, errorMsg);

				customsData.WB_EntryKey = "123-1";
				AssertNoErrors("Entry key is not empty therefore must not have an error.", customsData.WB_EntryKeyInfo);
			}
			else
			{
				AssertEquals("Although Entry key is empty, Entry key is not mandatory.", false, customsData.WB_EntryKeyInfo.HasError(errorMsg));
			}
		}

		#endregion

		#region TestOrderLine_CheckWB_EntryKey_CustomsRelaxValidation

		public void TestOrderLine_CheckWB_EntryKey_CustomsRelaxValidation()
		{
			const string errorMsg = "Entry Number is mandatory for Customs Jobs.";

			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;

			Helper.EnableWarehouseForBond(whs, true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs, "R1", data.Part1, 100m);

			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var customsData = orderLine.CustomsData;
			using (WarehouseDataRegistry.Instance.AllowOrderToBeFinalisedWithoutCustomsClearance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				customsData.Validation.ValidateWB_EntryKey();
				AssertNoError("Although Entry key is empty, Entry key is not mandatory Customs allows it.", customsData.WB_EntryKeyInfo, errorMsg);
			}

			customsData.Validation.ValidateWB_EntryKey();
			AssertHasError("Entry key must be mandatory.", customsData.WB_EntryKeyInfo, errorMsg);
		}

		#endregion

		#region TestOrderLine_CheckEntryKey_OrderSubTypeCPS_HasNoPick_NoError_WhenEmpty

		public void TestOrderLine_CheckEntryKey_OrderSubTypeCPS_HasNoPick_NoError_WhenEmpty_CountryUS()
		{
			TestOrderLine_CheckEntryKey_OrderSubTypeCPS_HasNoPick_NoError_WhenEmptyCore(Core.Constants.CountryCodes.UnitedStates, expectedErrorWhenEmpty: false);
		}

		public void TestOrderLine_CheckEntryKey_OrderSubTypeCPS_HasNoPick_NoError_WhenEmpty_CountryPR()
		{
			TestOrderLine_CheckEntryKey_OrderSubTypeCPS_HasNoPick_NoError_WhenEmptyCore(Core.Constants.CountryCodes.PuertoRico, expectedErrorWhenEmpty: false);
		}

		public void TestOrderLine_CheckEntryKey_OrderSubTypeCPS_HasNoPick_NoError_WhenEmpty_CountryAU()
		{
			TestOrderLine_CheckEntryKey_OrderSubTypeCPS_HasNoPick_NoError_WhenEmptyCore(Core.Constants.CountryCodes.Australia, expectedErrorWhenEmpty: true);
		}

		void TestOrderLine_CheckEntryKey_OrderSubTypeCPS_HasNoPick_NoError_WhenEmptyCore(string warehouseCountryCode, bool expectedErrorWhenEmpty)
		{
			const string errorMsg = "Entry Number is mandatory for Customs Jobs.";

			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;
			whs.WarehouseAddress.OA_RL_NKRelatedPortCode = Helper.GetCountryUNLOCO(warehouseCountryCode).RL_Code;
			whs.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;

			Helper.CreateWhsReceiveWithInventory(data.Org1, whs, "R1", data.Part1, 100m);
			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var customsData = orderLine.CustomsData;
			AssertEquals("Pre-condition: WB_EntryKey is empty.", "", customsData.WB_EntryKey);

			customsData.Validation.ValidateWB_EntryKey();
			if (expectedErrorWhenEmpty)
			{
				AssertHasError("Entry key must be mandatory.", customsData.WB_EntryKeyInfo, errorMsg);

				customsData.WB_EntryKey = "123-1";
				AssertNoErrors("Entry key is not empty therefore must not have an error.", customsData.WB_EntryKeyInfo);
			}
			else
			{
				AssertNoErrors("Entry key is not empty therefore must not have an error.", customsData.WB_EntryKeyInfo);
			}
		}

		#endregion

		#region TestOrderLine_CheckWB_EntryKey_OrderSubTypeCPS

		//	US
		public void TestOrderLine_CheckWB_EntryKey_OrderSubTypeCPS_CountryUS_Domestic()
		{
			TestOrderLine_CheckWB_EntryKey_OrderSubTypeCPSCore(Core.Constants.CountryCodes.UnitedStates, ZoneStatusList.Codes.Domestic, expectErrorBeforeSavePick: false, expectErrorAfterSavePick: false);
		}

		public void TestOrderLine_CheckWB_EntryKey_OrderSubTypeCPS_CountryUS_ZoneRestricted()
		{
			TestOrderLine_CheckWB_EntryKey_OrderSubTypeCPSCore(Core.Constants.CountryCodes.UnitedStates, ZoneStatusList.Codes.ZoneRestricted, expectErrorBeforeSavePick: false, expectErrorAfterSavePick: true);
		}

		public void TestOrderLine_CheckWB_EntryKey_OrderSubTypeCPS_CountryUS_PrivilegedForeign()
		{
			TestOrderLine_CheckWB_EntryKey_OrderSubTypeCPSCore(Core.Constants.CountryCodes.UnitedStates, ZoneStatusList.Codes.PrivilegedForeign, expectErrorBeforeSavePick: false, expectErrorAfterSavePick: true);
		}

		public void TestOrderLine_CheckWB_EntryKey_OrderSubTypeCPS_CountryUS_NonPrivilegedForeign()
		{
			TestOrderLine_CheckWB_EntryKey_OrderSubTypeCPSCore(Core.Constants.CountryCodes.UnitedStates, ZoneStatusList.Codes.NonPrivilegedForeign, expectErrorBeforeSavePick: false, expectErrorAfterSavePick: true);
		}

		//	PR
		public void TestOrderLine_CheckWB_EntryKey_OrderSubTypeCPS_CountryPR_Domestic()
		{
			TestOrderLine_CheckWB_EntryKey_OrderSubTypeCPSCore(Core.Constants.CountryCodes.PuertoRico, ZoneStatusList.Codes.Domestic, expectErrorBeforeSavePick: false, expectErrorAfterSavePick: false);
		}

		public void TestOrderLine_CheckWB_EntryKey_OrderSubTypeCPS_CountryPR_ZoneRestricted()
		{
			TestOrderLine_CheckWB_EntryKey_OrderSubTypeCPSCore(Core.Constants.CountryCodes.PuertoRico, ZoneStatusList.Codes.ZoneRestricted, expectErrorBeforeSavePick: false, expectErrorAfterSavePick: true);
		}

		public void TestOrderLine_CheckWB_EntryKey_OrderSubTypeCPS_CountryPR_PrivilegedForeign()
		{
			TestOrderLine_CheckWB_EntryKey_OrderSubTypeCPSCore(Core.Constants.CountryCodes.PuertoRico, ZoneStatusList.Codes.PrivilegedForeign, expectErrorBeforeSavePick: false, expectErrorAfterSavePick: true);
		}

		public void TestOrderLine_CheckWB_EntryKey_OrderSubTypeCPS_CountryPR_NonPrivilegedForeign()
		{
			TestOrderLine_CheckWB_EntryKey_OrderSubTypeCPSCore(Core.Constants.CountryCodes.PuertoRico, ZoneStatusList.Codes.NonPrivilegedForeign, expectErrorBeforeSavePick: false, expectErrorAfterSavePick: true);
		}

		//	AU
		public void TestOrderLine_CheckWB_EntryKey_OrderSubTypeCPS_CountryAU_Domestic()
		{
			TestOrderLine_CheckWB_EntryKey_OrderSubTypeCPSCore(Core.Constants.CountryCodes.Australia, ZoneStatusList.Codes.Domestic, expectErrorBeforeSavePick: true, expectErrorAfterSavePick: true);
		}

		public void TestOrderLine_CheckWB_EntryKey_OrderSubTypeCPS_CountryAU_ZoneRestricted()
		{
			TestOrderLine_CheckWB_EntryKey_OrderSubTypeCPSCore(Core.Constants.CountryCodes.Australia, ZoneStatusList.Codes.ZoneRestricted, expectErrorBeforeSavePick: true, expectErrorAfterSavePick: true);
		}

		public void TestOrderLine_CheckWB_EntryKey_OrderSubTypeCPS_CountryAU_PrivilegedForeign()
		{
			TestOrderLine_CheckWB_EntryKey_OrderSubTypeCPSCore(Core.Constants.CountryCodes.Australia, ZoneStatusList.Codes.PrivilegedForeign, expectErrorBeforeSavePick: true, expectErrorAfterSavePick: true);
		}

		public void TestOrderLine_CheckWB_EntryKey_OrderSubTypeCPS_CountryAU_NonPrivilegedForeign()
		{
			TestOrderLine_CheckWB_EntryKey_OrderSubTypeCPSCore(Core.Constants.CountryCodes.Australia, ZoneStatusList.Codes.NonPrivilegedForeign, expectErrorBeforeSavePick: true, expectErrorAfterSavePick: true);
		}

		void TestOrderLine_CheckWB_EntryKey_OrderSubTypeCPSCore(string warehouseCountryCode, string zoneStatus, bool expectErrorBeforeSavePick, bool expectErrorAfterSavePick)
		{
			const string errorMsg = "Entry Number is mandatory for Customs Jobs.";
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;
			whs.WarehouseAddress.OA_RL_NKRelatedPortCode = Helper.GetCountryUNLOCO(warehouseCountryCode).RL_Code;
			whs.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;

			Helper.CreateWhsReceiveWithInventory(data.Org1, whs, "R1", data.Part1, 100m);
			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			using (PermitServiceTestHelper.MockServiceToGetPermits())
			{
				var pick = Factory.New<WhsPick>();
				pick.WP_PickNo = "P1";
				pick.WP_WW_Whs = order.WD_WW_Whs;
				pick.WP_WL_DockDoor = order.Warehouse.WW_DefaultOutboundDockDoor;
				pick.Orders.Add(order);
				pick.WP_PickOption = WhsPickOption.Codes.Manual;
				AssertEquals("Precondition", true, order.IsAttachedToPick);
				AssertEquals("Precondition: Pick is not saved.", false, pick.IsInDatabase);

				var customsData = orderLine.CustomsData;
				AssertEquals("Precondition", "", customsData.WB_EntryKey);
				customsData.Validation.ValidateWB_EntryKey();
				if (expectErrorBeforeSavePick)
				{
					AssertHasError("Entry key must be mandatory.", customsData.WB_EntryKeyInfo, errorMsg);
				}
				else
				{
					AssertEquals("If warehouse is FTZ and in US/PR, Entry key is not mandatory until after get permits.", false, customsData.WB_EntryKeyInfo.HasError(errorMsg));
				}

				Factory.Save();     // save pick to trigger getting permits

				customsData.WB_ZoneStatus = zoneStatus;
				customsData.WB_EntryKey = "";
				if (expectErrorAfterSavePick)
				{
					AssertHasError($"If warehouse is FTZ and in US/PR, has a pick and zone status is {zoneStatus}, Entry key must be mandatory after get permits.", customsData.WB_EntryKeyInfo, errorMsg);

					customsData.WB_EntryKey = "123-1";
					AssertNoErrors("Entry key is not empty therefore must not have an error.", customsData.WB_EntryKeyInfo);
				}
				else
				{
					AssertEquals("Although Entry key is empty, Entry key is not mandatory.", false, customsData.WB_EntryKeyInfo.HasError(errorMsg));
				}
			}
		}

		#endregion

		#endregion

		#region TestCheckWB_EntryKeyValidation

		public void TestCheckWB_EntryKeyIsNotBasedOnBondedEntryKey()
		{
			const string errorMsg = "Entry Number is mandatory for Customs Jobs.";
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.WE_BondedEntryKey = "EntryKey";
			Factory.Save();

			var bond = Factory.New<WhsBondedWarehouseAttribute>();
			bond.SetParent(orderLine);
			bond.WB_EntryKey = "";
			bond.Validation.ValidateWB_EntryKey();
			AssertEquals("Precondition", false, orderLine.WE_BondedEntryKey.IsEmpty);
			AssertHasError("There was an issue- checking the wrong field (WE_BondedEntryKey) instead of WB_EntryKey.", bond.WB_EntryKeyInfo, errorMsg);

			bond.WB_EntryKey = "A123";
			bond.Validation.ValidateWB_EntryKey();
			AssertNoError("Entry Key should have a value.", bond.WB_EntryKeyInfo, errorMsg);
		}

		#endregion

		#region Implementation

		protected override WhsDocketLine GetDocketLineParent(TestDataSimpleEnvironment data, WhsWarehouse whsOverride = null)
		{
			var whs = whsOverride ?? data.Whs1;
			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = "CUS";
			data.Whs1.DefaultLocation.WLV_WA_PickingArea = whs.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded).PK;

			return Helper.CreateWhsOrderLine(order, data.Part1, 10m);
		}

		protected override WhsBondedWarehouseAttribute GetNewBusinessObject()
		{
			var result = base.GetNewBusinessObject();
			var docket = Factory.New<WhsOrder>();
			docket.WD_DocketSubType = OrderType.Codes.Customs;
			result.WB_ParentID = docket.Lines.AddNew().PK;
			return result;
		}

		#endregion
	}
}
