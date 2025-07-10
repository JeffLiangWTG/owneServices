using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsBondedWarehouseAttributeValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestCheckWB_CustomsQty

		public void TestCheckWB_CustomsQty()
		{
			var bond = GetNewBusinessObject();
			bond.WB_CustomsQty = 10m;
			AssertNoErrors(bond.WB_CustomsQtyInfo);

			bond.WB_CustomsQty = -10m;
			if (IsNegativeValueAllowedForAdjustments)
			{
				AssertNoErrors(bond.WB_CustomsQtyInfo);
			}
			else
			{
				AssertHasErrors(bond.WB_CustomsQtyInfo);
			}
		}

		#endregion

		#region TestCheckWB_EntryKey

		public void TestCheckWB_EntryKey()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var docketLine = GetDocketLineParent(data);
			Factory.Save();

			var bond = GetNewBusinessObject();
			bond.SetParent(docketLine);
			AssertNoErrors(bond.WB_EntryKeyInfo);

			bond.Validation.ValidateWB_EntryKey();

			if (CheckEntryNumber)
			{
				AssertHasError(bond.WB_EntryKeyInfo, "Entry Number is mandatory for Customs Jobs.");
			}
			else
			{
				AssertNoErrors(bond.WB_EntryKeyInfo);
			}

			using (bond.SuspendUpdatingDocketLineEntryKey())
			{
				bond.WB_EntryKey = "123";
				AssertNoErrors(bond.WB_EntryKeyInfo);
			}

			bond.WB_EntryKey = "";

			if (CheckEntryNumber)
			{
				AssertHasError(bond.WB_EntryKeyInfo, "Entry Number is mandatory for Customs Jobs.");

				using (bond.SuspendValidationTesting())
				{
					bond.WB_EntryKeyInfo.ClearAllNotifications(); // clear notifications manually because they won't be cleared when validation is disabled (job is not customs transaction) 
				}
			}
			else
			{
				AssertNoErrors(bond.WB_EntryKeyInfo);
			}

			docketLine.Docket.WD_DocketSubType = "";
			data.Whs1.DefaultLocation.WLV_WA_PickingArea = data.Whs1.Areas.Single(a => a.WA_AreaType != "BON" && a.WA_AreaType != "DDA").PK;
			Helper.EnableWarehouseForBond(data.Whs1, false);
			bond.Validation.ValidateWB_EntryKey();
			AssertNoErrors(bond.WB_EntryKeyInfo);

			bond.WB_EntryKey = "123";
			AssertNoErrors(bond.WB_EntryKeyInfo);
		}

		protected virtual bool CheckEntryNumber
		{
			get { return true; }
		}

		#endregion

		#region TestCheckWB_ValueForDuty

		public void TestCheckWB_ValueForDuty()
		{
			var bond = GetNewBusinessObject();
			bond.WB_ValueForDuty = 10m;
			AssertNoErrors(bond.WB_ValueForDutyInfo);

			bond.WB_ValueForDuty = -10m;
			if (IsNegativeValueAllowedForAdjustments)
			{
				AssertNoErrors(bond.WB_ValueForDutyInfo);
			}
			else
			{
				AssertHasErrors(bond.WB_ValueForDutyInfo);
			}
		}

		#endregion

		#region TestCheckWB_TILV

		public void TestCheckWB_TILV()
		{
			var bond = GetNewBusinessObject();
			bond.WB_TILV = 10m;
			AssertNoErrors(bond.WB_TILVInfo);

			bond.WB_TILV = -10m;
			if (IsNegativeValueAllowedForAdjustments)
			{
				AssertNoErrors(bond.WB_TILVInfo);
			}
			else
			{
				AssertHasErrors(bond.WB_TILVInfo);
			}
		}

		protected virtual bool IsNegativeValueAllowedForAdjustments
		{
			get { return false; }
		}

		#endregion

		#region TestCheckWB_ZoneStatus

		public void TestCheckWB_ZoneStatus()
		{
			var bond = GetNewBusinessObject();
			bond.WB_ZoneStatus = "";
			AssertNoErrors("ZoneStatus is valid, should not have error", bond.WB_ZoneStatusInfo);
			bond.WB_ZoneStatus = "B";
			AssertHasError(bond.WB_ZoneStatusInfo, "Enter a valid Zone Status.");

			bond.WB_ZoneStatus = "P";
			AssertHasError("Is not US FTZ", bond.WB_ZoneStatusInfo, "Enter a valid Zone Status.");
		}

		public void TestCheckWB_ZoneStatus_USFTZ()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS("WHS");
			var docketLine = GetDocketLineParent(data, whs);
			var docket = docketLine.Docket;
			docket.WD_DocketSubType = ReceiveType.Codes.Customs;

			var bond = docketLine.CustomsData;
			bond.WB_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			AssertNoErrors("ZoneStatus is valid, should not have error", bond.WB_ZoneStatusInfo);

			bond.WB_ZoneStatus = "X";
			AssertHasError(bond.WB_ZoneStatusInfo, "Enter a valid Zone Status.");
		}

		#endregion

		#region TestCheckWB_OutwardType

		public void TestCheckWB_OutwardType()
		{
			var bond = GetNewBusinessObject();
			AssertEquals("Precondition", ZString.Empty, bond.WB_OutwardType);
			AssertNoErrors("OutwardType has default value, should not have error.", bond.WB_OutwardTypeInfo);
			bond.WB_OutwardType = "ABC";
			AssertHasError(bond.WB_OutwardTypeInfo, "Enter a valid Outward Type.");
		}

		public void TestCheckWB_OutwardType_USFTZ()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var docketLine = GetDocketLineParent(data);
			var whs = Helper.CreateFTZWarehouseInUS("WHS");
			var docket = docketLine.Docket;
			docket.WD_WW_Whs = whs.PK;
			docket.WD_DocketSubType = ReceiveType.Codes.Customs;

			var bond = GetNewBusinessObject();
			bond.SetParent(docketLine);
			bond.WB_OutwardType = "ABC";
			AssertHasError(bond.WB_OutwardTypeInfo, "Enter a valid Outward Type.");

			bond.WB_OutwardType = WhsBondedWarehouseAttributeOutwardType.Codes.CNN;
			AssertNoError(bond.WB_OutwardTypeInfo, "Enter a valid Outward Type.");

			whs.WW_WarehouseType = Integration.CodeLists.WarehouseTypes.Codes.FreeTradeZone;
			bond.WB_OutwardType = WhsBondedWarehouseAttributeOutwardType.Codes.CNN;
			AssertNoError(bond.WB_OutwardTypeInfo, "Enter a valid Outward Type.");
		}

		public void TestCheckWB_OutwardType_Mandatory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS("WHS");
			var order = Helper.CreateWhsOrder(data.Org1, whs);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var bond = GetNewBusinessObject();
			bond.SetParent(orderLine);
			AssertEquals("Precondition", true, whs.IsFTZAndUSJurisdiction());

			order.WD_DocketSubType = OrderType.Codes.Customs;
			bond.WB_OutwardType = "";
			AssertHasError(bond.WB_OutwardTypeInfo, "Please enter an Outward Type.");

			bond.WB_OutwardType = WhsBondedWarehouseAttributeOutwardType.Codes.CNN;
			AssertNoError(bond.WB_OutwardTypeInfo, "Please enter an Outward Type.");

			var receive = Helper.CreateWhsReceive(data.Org1, whs);
			bond.SetParent(Helper.CreateWhsReceiveLine(receive, data.Part1, 10m));
			bond.WB_OutwardType = "";
			AssertNoError(bond.WB_OutwardTypeInfo, "Please enter an Outward Type.");
		}

		public void TestCheckWB_OutwardType_OnlyCheckForCustomsJob()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateWarehouse("WHS");
			var order = Helper.CreateWhsOrder(data.Org1, whs);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var bond = GetNewBusinessObject();
			bond.SetParent(orderLine);

			order.WD_DocketSubType = OrderType.Codes.Order;
			bond.WB_OutwardType = "";
			AssertNoError(bond.WB_OutwardTypeInfo, "Please enter an Outward Type.");

			order.WD_DocketSubType = OrderType.Codes.Customs;
			bond.WB_OutwardType = "";
			AssertHasError(bond.WB_OutwardTypeInfo, "Please enter an Outward Type.");

			order.WD_DocketSubType = OrderType.Codes.Customs;
			bond.WB_OutwardType = WhsBondedWarehouseAttributeOutwardType.Codes.CNN;
			AssertNoError(bond.WB_OutwardTypeInfo, "Please enter an Outward Type.");
		}

		#endregion

		#region TestCheckWB_IsFromAnotherFTZWhs

		public void TestCheckWB_IsFromAnotherFTZWhs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			string errorMsg = "Only US FTZ warehouse can select from other US FTZ.";
			var bond = GetNewBusinessObject();
			bond.WB_IsFromAnotherFTZWhs = true;
			AssertHasError(bond.WB_IsFromAnotherFTZWhsInfo, errorMsg);
			bond.WB_IsFromAnotherFTZWhs = false;
			AssertNoError(bond.WB_IsFromAnotherFTZWhsInfo, errorMsg);

			var docketLine = GetDocketLineParent(data);
			var docket = docketLine.Docket;
			docket.WD_OH_Client = data.Org1.PK;
			var whs = Helper.CreateFTZWarehouseInUS("WHS");
			docket.WD_WW_Whs = whs.PK;
			docket.WD_DocketSubType = ReceiveType.Codes.Customs;
			bond.SetParent(docketLine);

			bond.WB_IsFromAnotherFTZWhs = true;
			AssertEquals("Precondition", true, whs.IsFTZAndUSJurisdiction());
			AssertNoError(bond.WB_IsFromAnotherFTZWhsInfo, errorMsg);

			whs.WarehouseAddress.OA_RL_NKRelatedPortCode = Helper.GetCountryUNLOCO(Core.Constants.CountryCodes.Australia).RL_Code;
			AssertEquals("Precondition", false, whs.IsFTZAndUSJurisdiction());
			AssertNoError(bond.WB_IsFromAnotherFTZWhsInfo, errorMsg);

			bond.Validation.ValidateWB_IsFromAnotherFTZWhs();
			AssertHasError(bond.WB_IsFromAnotherFTZWhsInfo, errorMsg);

			whs.WarehouseAddress.OA_RL_NKRelatedPortCode = Helper.GetCountryUNLOCO(Core.Constants.CountryCodes.UnitedStates).RL_Code;
			bond.Validation.ValidateWB_IsFromAnotherFTZWhs();
			AssertNoError(bond.WB_IsFromAnotherFTZWhsInfo, errorMsg);

			whs.WW_WarehouseType = Integration.CodeLists.WarehouseTypes.Codes.Product;
			bond.Validation.ValidateWB_IsFromAnotherFTZWhs();
			AssertEquals("Precondition", false, whs.IsFTZAndUSJurisdiction());
			AssertHasError(bond.WB_IsFromAnotherFTZWhsInfo, errorMsg);
		}

		#endregion

		#region Implementation

		protected virtual WhsDocketLine GetDocketLineParent(TestDataSimpleEnvironment data, WhsWarehouse whsOverride = null)
		{
			var whs = whsOverride ?? data.Whs1;
			var location = whs.DefaultLocation;
			var bondedArea = whs.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			location.WLV_WA_PutawayArea = bondedArea.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, whs);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location);
			inventory.CustomsData.WB_EntryKey = "EntryKey";
			Factory.Save();

			return inventory.InDocketLine;
		}

		protected virtual WhsBondedWarehouseAttribute GetNewBusinessObject()
		{
			return Factory.New<WhsBondedWarehouseAttribute>();
		}

		#endregion
	}
}
