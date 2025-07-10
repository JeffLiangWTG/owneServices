using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsOrderValidationTest : WhsPickableDocketValidationTest<WhsOrder>
	{
		#region TestCheckWD_TransportMode

		public void TestCheckWD_TransportMode()
		{
			var order = GetNewBusinessObject();
			order.WD_TransportMode = "";
			AssertNoErrors(order.WD_TransportModeInfo);

			order.WD_TransportMode = "XXX";
			AssertHasError(order.WD_TransportModeInfo, "Enter a valid " + order.WD_TransportModeInfo.HumanReadableName + ".");

			order.WD_TransportMode = "ROA";
			AssertNoErrors(order.WD_TransportModeInfo);
		}

		#endregion

		#region TestCheckWD_ContainerMode

		public void TestCheckWD_ContainerMode()
		{
			var order = GetNewBusinessObject();
			order.WD_ContainerMode = "";
			AssertNoErrors(order.WD_ContainerModeInfo);

			order.WD_ContainerMode = "XXX";
			AssertHasError(order.WD_ContainerModeInfo, "Enter a valid " + order.WD_ContainerModeInfo.HumanReadableName + ".");

			order.WD_ContainerMode = "LCL";
			AssertNoErrors(order.WD_ContainerModeInfo);

			order.WD_TransportMode = "AIR";
			AssertHasError(order.WD_ContainerModeInfo, "Enter a valid " + order.WD_ContainerModeInfo.HumanReadableName + ".");
		}

		#endregion

		#region TestCheckWD_OH_Client

		public override void TestValidateWD_OH_Client()
		{
			base.TestValidateWD_OH_Client();
			AssertValidateWD_OH_Client();
		}

		public override void TestValidateWD_OH_Client_ForWeb()
		{
			base.TestValidateWD_OH_Client_ForWeb();
			AssertValidateWD_OH_Client();
		}

		void AssertValidateWD_OH_Client()
		{
			var warning = "This client does not have a Customs CP Permit Code";

			Docket.WD_DocketSubType = CodeLists.OrderType.Codes.Customs;
			Docket.WD_OH_Client = ZGuid.Empty;
			AssertNoWarning(Docket.WD_OH_ClientInfo, warning);

			OrgHeader client = Helper.CreateClient();
			Docket.WD_DocketSubType = CodeLists.OrderType.Codes.Order;
			Docket.WD_OH_Client = client.PK;
			AssertNoWarning(Docket.WD_OH_ClientInfo, warning);

			Docket.WD_DocketSubType = CodeLists.OrderType.Codes.Customs;
			Docket.Validation.ValidateWD_OH_Client();
			AssertHasWarning(Docket.WD_OH_ClientInfo, warning);

			OrgCusCode code = client.CustomsCodes.AddNew();
			code.OK_CodeType = OrgCusCode.CodeTypes.CustomsCPPermitCode;
			Docket.Validation.ValidateWD_OH_Client();
			AssertNoWarning(Docket.WD_OH_ClientInfo, warning);
		}

		public void TestValidateWD_OH_Client_CPPermitWarning_NormalWareHouse()
		{
			var whs = Helper.CreateWarehouse("WHS", "A");
			TestAndAssertValidateWD_OH_Client_CPPermitWarning_Core(whs, OrderType.Codes.Order, isUSWarehouseBondedEnabled: false, isFTZWarehouse: false, expectWaring: false);
		}

		public void TestValidateWD_OH_Client_CPPermitWarning_IsCustom()
		{
			var whs = Helper.CreateWarehouse("WHS", "A");
			Helper.EnableWarehouseForBond(whs, true);
			TestAndAssertValidateWD_OH_Client_CPPermitWarning_Core(whs, OrderType.Codes.Customs, isUSWarehouseBondedEnabled: false, isFTZWarehouse: false, expectWaring: true);
		}

		public void TestValidateWD_OH_Client_CPPermitWarning_USWarehouse()
		{
			var whs = Helper.CreateWarehouse("WHS", "A");
			Helper.EnableWarehouseForBond(whs, true);
			whs.WarehouseAddress.OA_RL_NKRelatedPortCode = Helper.GetCountryUNLOCO(Core.Constants.CountryCodes.UnitedStates).RL_Code;
			TestAndAssertValidateWD_OH_Client_CPPermitWarning_Core(whs, OrderType.Codes.Customs, isUSWarehouseBondedEnabled: true, isFTZWarehouse: false, expectWaring: true);
		}

		public void TestValidateWD_OH_Client_CPPermitWarning_USWarehouseFTZ()
		{
			var ftzWhs = Helper.CreateFTZWarehouseInUS();
			TestAndAssertValidateWD_OH_Client_CPPermitWarning_Core(ftzWhs, OrderType.Codes.Customs, isUSWarehouseBondedEnabled: true, isFTZWarehouse: true, expectWaring: false);
		}

		void TestAndAssertValidateWD_OH_Client_CPPermitWarning_Core(WhsWarehouse whs, string orderSubType, bool isUSWarehouseBondedEnabled, bool isFTZWarehouse, bool expectWaring)
		{
			var warning = "This client does not have a Customs CP Permit Code";
			var client = Helper.CreateClient("Client", "C");
			var product = Helper.CreateProduct(client, "P1");
			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(client, whs);
			order.WD_DocketSubType = orderSubType;
			AssertEquals("Precondition", isUSWarehouseBondedEnabled, order.IsWarehouseBondedEnabledAndUSJurisdiction());
			AssertEquals("Precondition", isFTZWarehouse, whs.IsFTZWarehouse);
			AssertEquals("Precondition", false, DoesClientHaveCPCode(client.PK));

			order.Validation.ValidateWD_OH_Client();
			if (expectWaring)
			{
				AssertHasWarning($"Based on IsUSWarehouseBondedEnabled: {isUSWarehouseBondedEnabled}, OrderSubType: {orderSubType}, IsFTZWarehouse: {isFTZWarehouse} and client does not have permit, it should have a warning.", order.WD_OH_ClientInfo, warning);
			}
			else
			{
				AssertEquals($"Based on IsUSWarehouseBondedEnabled: {isUSWarehouseBondedEnabled}, OrderSubType: {orderSubType}, IsFTZWarehouse: {isFTZWarehouse} and client does not have permit, it should *NOT* have a warning.", false, order.WD_OH_ClientInfo.HasWarnings());
			}
		}

		bool DoesClientHaveCPCode(ZGuid clientPK)
		{
			var query = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.CustomsCPPermitCode);
			query.AddToFilter(OrgCusCodeSchema.OK_OH, clientPK);
			return Factory.LoadTop1<OrgCusCode>(query) != null;
		}

		#endregion

		#region TestCheckWD_WW_Warehouse

		public void TestCheckWD_WW_Warehouse_HasCrossDockLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("WH2");

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_WL_CrossDock = data.Whs1.WW_DefaultOutboundDockDoor;
			AssertNoErrors(order.WD_WW_WhsInfo);

			Factory.Save();

			order.WD_WW_Whs = whs2.PK;
			AssertHasError(order.WD_WW_WhsInfo, "The order already has Cross Dock Location, the Cross Dock Location should be removed prior to changing the warehouse.");

			order.WD_WW_Whs = data.Whs1.PK;
			AssertNoErrors(order.WD_WW_WhsInfo);

			Factory.Save();

			order.WD_WL_CrossDock = ZGuid.Empty;
			order.WD_WW_Whs = whs2.PK;
			AssertNoErrors(order.WD_WW_WhsInfo);
		}

		public void TestCheckWD_WW_Warehouse_HasReservedStock()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("WH2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.ReserveStockIfAbleTo(receive.Inventory[0], 10m);

			AssertNoErrors(order.WD_WW_WhsInfo);

			Factory.Save();

			order.WD_WW_Whs = whs2.PK;
			AssertHasError(order.WD_WW_WhsInfo, "This order has Cross-Dock Allocation(s) linked to Warehouse 1, the Cross-Dock Allocation(s) should be removed prior to changing the warehouse.");

			order.WD_WW_Whs = data.Whs1.PK;
			AssertNoErrors(order.WD_WW_WhsInfo);
		}

		#endregion

		#region TestCheckWD_DocketType

		public void TestCheckWD_DocketType()
		{
			Docket.WD_DocketType = "";
			AssertEquals("Docket type validation accepting blank", true, Docket.WD_DocketTypeInfo.HasErrors());

			Docket.WD_DocketType = CodeLists.DocketType.Codes.Order;
			AssertEquals("Docket type validation not accepting valid docket type: " + Docket.WD_DocketType, false, Docket.WD_DocketTypeInfo.HasErrors());

			Docket.WD_DocketType = CodeLists.DocketType.Codes.Adjustment;
			AssertEquals("Docket type validation accepting invalid docket type: " + Docket.WD_DocketType, true, Docket.WD_DocketTypeInfo.HasErrors());

			Docket.WD_DocketType = CodeLists.DocketType.Codes.Receive;
			AssertEquals("Docket type validation accepting invalid docket type: " + Docket.WD_DocketType, true, Docket.WD_DocketTypeInfo.HasErrors());

			Docket.WD_DocketType = CodeLists.DocketType.Codes.Transfer;
			AssertEquals("Docket type validation accepting invalid docket type: " + Docket.WD_DocketType, true, Docket.WD_DocketTypeInfo.HasErrors());

			// Also test a dodgy random type
			Docket.WD_DocketType = ";;;";
			AssertEquals("Docket type validation accepting junk", true, Docket.WD_DocketTypeInfo.HasErrors());
		}

		#endregion

		#region TestCheckWD_DocketSubType

		public void TestCheckWD_DocketSubType_NormalWareHouse()
		{
			var whs = Helper.CreateWarehouse("WHS", "A");
			TestCheckWD_DocketSubTypeWithPermit(whs, OrderType.Codes.Order, isUSWarehouseBondedEnabled: false, isFTZWarehouse: false, expectedErrorMsg: string.Empty);
		}

		public void TestCheckWD_DocketSubType_IsCustom()
		{
			var whs = Helper.CreateWarehouse("WHS", "A");
			Helper.EnableWarehouseForBond(whs, true);
			TestCheckWD_DocketSubTypeWithPermit(whs, OrderType.Codes.Customs, isUSWarehouseBondedEnabled: false, isFTZWarehouse: false, expectedErrorMsg: string.Empty);
		}

		public void TestCheckWD_DocketSubType_USWarehouse()
		{
			var whs = Helper.CreateWarehouse("WHS", "A");
			Helper.EnableWarehouseForBond(whs, true);
			whs.WarehouseAddress.OA_RL_NKRelatedPortCode = Helper.GetCountryUNLOCO(Core.Constants.CountryCodes.UnitedStates).RL_Code;
			TestCheckWD_DocketSubTypeWithPermit(whs, OrderType.Codes.Customs, isUSWarehouseBondedEnabled: true, isFTZWarehouse: false, expectedErrorMsg: string.Empty);
		}

		public void TestCheckWD_DocketSubType_NotFTZWarehouse_CustomsReleaseWithPermit()
		{
			var whs = Helper.CreateWarehouse("WHS", "A");
			Helper.EnableWarehouseForBond(whs, true);
			whs.WarehouseAddress.OA_RL_NKRelatedPortCode = Helper.GetCountryUNLOCO(Core.Constants.CountryCodes.UnitedStates).RL_Code;

			var client = Helper.CreateClient("Client", "C");
			var product = Helper.CreateProduct(client, "P1");
			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(client, whs);
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			AssertHasError(order.WD_DocketSubTypeInfo, "Only orders placed with a US FTZ Warehouse can have an Order Type 'CPS'.");
		}

		public void TestCheckWD_DocketSubType_USWarehouseFTZ_CustomsReleaseWithPermit()
		{
			var ftzWhs = Helper.CreateFTZWarehouseInUS();
			TestCheckWD_DocketSubTypeWithPermit(ftzWhs, OrderType.Codes.CustomsReleaseWithPermit, isUSWarehouseBondedEnabled: true, isFTZWarehouse: true, expectedErrorMsg: string.Empty);
		}

		public void TestCheckWD_DocketSubType_USWarehouseFTZ_CustomsReleaseWithPermit_WarehouseInPuertoRico()
		{
			var ftzWhs = Helper.CreateFTZWarehouse(countrycode: Core.Constants.CountryCodes.PuertoRico);
			TestCheckWD_DocketSubTypeWithPermit(ftzWhs, OrderType.Codes.CustomsReleaseWithPermit, isUSWarehouseBondedEnabled: true, isFTZWarehouse: true, expectedErrorMsg: string.Empty);
		}

		public void TestCheckWD_DocketSubType_USWarehouseFTZ_Order()
		{
			var ftzWhs = Helper.CreateFTZWarehouseInUS();
			TestCheckWD_DocketSubTypeWithPermit(ftzWhs, OrderType.Codes.Order, isUSWarehouseBondedEnabled: true, isFTZWarehouse: true, expectedErrorMsg: "Orders placed with a US FTZ Warehouse must have an Order Type of 'CUS' or 'CPS'.");
		}

		public void TestCheckWD_DocketSubType_USWarehouseFTZ_BackOrder()
		{
			var ftzWhs = Helper.CreateFTZWarehouseInUS();
			TestCheckWD_DocketSubTypeWithPermit(ftzWhs, OrderType.Codes.BackOrder, isUSWarehouseBondedEnabled: true, isFTZWarehouse: true, expectedErrorMsg: "Orders placed with a US FTZ Warehouse must have an Order Type of 'CUS' or 'CPS'.");
		}

		public void TestCheckWD_DocketSubType_USWarehouseFTZ_RepeatOrder()
		{
			var ftzWhs = Helper.CreateFTZWarehouseInUS();
			TestCheckWD_DocketSubTypeWithPermit(ftzWhs, OrderType.Codes.RepeatOrder, isUSWarehouseBondedEnabled: true, isFTZWarehouse: true, expectedErrorMsg: "Orders placed with a US FTZ Warehouse must have an Order Type of 'CUS' or 'CPS'.");
		}

		void TestCheckWD_DocketSubTypeWithPermit(WhsWarehouse whs, string orderSubType, bool isUSWarehouseBondedEnabled, bool isFTZWarehouse, ZString expectedErrorMsg)
		{
			var client = Helper.CreateClient("Client", "C");
			var product = Helper.CreateProduct(client, "P1");
			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(client, whs);
			order.WD_DocketSubType = orderSubType;
			AssertEquals("Precondition", isUSWarehouseBondedEnabled, order.IsWarehouseBondedEnabledAndUSJurisdiction());
			AssertEquals("Precondition", isFTZWarehouse, whs.IsFTZWarehouse);

			if (expectedErrorMsg.IsEmpty)
			{
				AssertNoErrors($"Based on IsUSWarehouseBondedEnabled: {isUSWarehouseBondedEnabled}, OrderSubType: {orderSubType}, IsFTZWarehouse: {isFTZWarehouse} and client does not have permit, it should *NOT* have an error.", order.WD_DocketSubTypeInfo);
			}
			else
			{
				AssertHasError($"Based on IsUSWarehouseBondedEnabled: {isUSWarehouseBondedEnabled}, OrderSubType: {orderSubType}, IsFTZWarehouse: {isFTZWarehouse} and client does not have permit, it should have an error.", order.WD_DocketSubTypeInfo, expectedErrorMsg);
				var whsNotCustoms = Helper.CreateWarehouse("Normal", "N");
				order.WD_WW_Whs = whsNotCustoms.PK;
				AssertNoError("Change to valid warehouse should clear the error.", order.WD_DocketSubTypeInfo, expectedErrorMsg);
			}
		}

		public void TestCheckWD_DocketSubType_IsWarehouseBondedEnabledAndUSJurisdiction_Customs()
		{
			var whs = Helper.CreateFTZWarehouseInUS();
			var client = Helper.CreateClient("Client", "C");
			var product = Helper.CreateProduct(client, "P1");
			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(client, whs);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			AssertEquals("Precondition", true, order.IsWarehouseBondedEnabledAndUSJurisdiction());
			AssertEquals("Precondition", true, whs.IsFTZWarehouse);

			AssertHasWarning(order.WD_DocketSubTypeInfo, "Manual orders placed with a US FTZ Warehouse with an Order Type 'CUS' will not use permits.");
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			AssertNoNotifications(order.WD_DocketSubTypeInfo);
		}

		#endregion

		#region TestCheckWD_DocketTypeOnlyAccept

		public void TestCheckWD_DocketSubTypeOnlyAcceptCustomsReleaseWithPermit()
		{
			TestCheckWD_DocketSubTypeOnlyAcceptCustomsReleaseWithPermitCore(OrderType.Codes.CustomsReleaseWithPermit, "");
		}

		public void TestCheckWD_DocketSubTypeOnlyAcceptCustomsReleaseWithPermit_Order()
		{
			TestCheckWD_DocketSubTypeOnlyAcceptCustomsReleaseWithPermitCore(OrderType.Codes.Order, "Orders placed with a US FTZ Warehouse must have an Order Type of 'CUS' or 'CPS'.");
		}

		public void TestCheckWD_DocketSubTypeOnlyAcceptCustomsReleaseWithPermit_BackOrder()
		{
			TestCheckWD_DocketSubTypeOnlyAcceptCustomsReleaseWithPermitCore(OrderType.Codes.BackOrder, "Orders placed with a US FTZ Warehouse must have an Order Type of 'CUS' or 'CPS'.");
		}

		public void TestCheckWD_DocketSubTypeOnlyAcceptCustomsReleaseWithPermit_RepeatOrder()
		{
			TestCheckWD_DocketSubTypeOnlyAcceptCustomsReleaseWithPermitCore(OrderType.Codes.RepeatOrder, "Orders placed with a US FTZ Warehouse must have an Order Type of 'CUS' or 'CPS'.");
		}

		void TestCheckWD_DocketSubTypeOnlyAcceptCustomsReleaseWithPermitCore(string orderSubType, ZString errorMsg)
		{
			var whs = Helper.CreateFTZWarehouseInUS();
			var client = Helper.CreateClient("Client", "C");
			var product = Helper.CreateProduct(client, "P1");
			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(client, whs);
			order.WD_DocketSubType = orderSubType;
			AssertEquals("Precondition", true, order.IsWarehouseBondedEnabledAndUSJurisdiction());
			AssertEquals("Precondition", true, whs.IsFTZWarehouse);

			if (!errorMsg.IsEmpty)
			{
				AssertHasError($"Only should be able to select 'CPS' type. {orderSubType} is not valid type", order.WD_DocketSubTypeInfo, errorMsg);
			}
			else
			{
				AssertEquals($"Only should be able to select 'CPS' type. {orderSubType} is valid type", false, order.WD_DocketSubTypeInfo.HasErrors());
			}
		}

		#endregion

		#region TestCheckWD_WhsOrderFulfillmentRule

		public void TestCheckWD_WhsOrderFulfillmentRule_IsAValidCode()
		{
			Docket.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.None;
			AssertNoErrors(Docket.WD_WhsOrderFulfillmentRuleInfo);

			Docket.WD_WhsOrderFulfillmentRule = "";
			AssertHasErrors(Docket.WD_WhsOrderFulfillmentRuleInfo);

			Docket.WD_WhsOrderFulfillmentRule = "354";
			AssertHasErrors(Docket.WD_WhsOrderFulfillmentRuleInfo);

			Docket.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			AssertNoErrors(Docket.WD_WhsOrderFulfillmentRuleInfo);
		}

		public void TestCheckWD_WhsOrderFulfillmentRule_WithoutShortfall()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Docket.WD_WW_Whs = data.Whs1.PK;
			Docket.WD_OH_Client = data.Org1.PK;
			Helper.CreateWhsOrderLine(Docket, data.Part1, 10m);
			//shortfall calculation for unpicked orders is a dbonly query
			Factory.Save();

			Docket.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			AssertEquals(false, Docket.WD_WhsOrderFulfillmentRuleInfo.HasWarning(fulfillmentRuleNotMetWarning));

			Docket.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.None;
			AssertEquals(false, Docket.WD_WhsOrderFulfillmentRuleInfo.HasWarning(fulfillmentRuleNotMetWarning));
		}

		public void TestCheckWD_WhsOrderFulfillmentRule_WithShortfall()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Docket.WD_WW_Whs = data.Whs1.PK;
			Docket.WD_OH_Client = data.Org1.PK;
			Helper.CreateWhsOrderLine(Docket, data.Part1, 1000m);
			Factory.Save(); //shortfall calculation for unpicked orders is a dbonly query

			Docket.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			AssertEquals(true, Docket.WD_WhsOrderFulfillmentRuleInfo.HasWarning(fulfillmentRuleNotMetWarning));

			Docket.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.None;
			AssertEquals(false, Docket.WD_WhsOrderFulfillmentRuleInfo.HasWarning(fulfillmentRuleNotMetWarning));
		}

		const string fulfillmentRuleNotMetWarning = "The Fulfillment Rule has not been met.\r\n" +
																												"Pick documentation cannot be printed until the Fulfillment Rule has been satisfied or manually overridden.";

		#endregion

		#region TestCheckWD_RequiredDate

		public void TestCheckWD_RequiredDate()
		{
			Docket.WD_RequiredDate = ZDateTimeOffset.Now;
			Assert("Required Date should have no error", !Docket.WD_RequiredDateInfo.HasErrors());
			Docket.WD_RequiredDate = ZDateTimeOffset.Empty;
			Assert("Required Date should have error", Docket.WD_RequiredDateInfo.HasErrors());
		}

		public void TestCheckWD_RequiredDateRestrictionOnPreDateWithoutFinalisedDateOverride()
		{
			Env.Security.WhsOrderPreDate.IsAllowed = true;
			Docket.WD_RequiredDate = ZDateTimeOffset.Today.AddDays(-8);
			AssertNoErrors(Docket.WD_RequiredDateInfo);

			Env.Security.WhsOrderPreDate.IsAllowed = false;
			Docket.WD_RequiredDate = ZDateTimeOffset.Today.AddDays(-7);
			AssertNoErrors(Docket.WD_RequiredDateInfo);

			Docket.WD_RequiredDate = ZDateTimeOffset.Today.AddDays(-8);
			AssertNoErrors(Docket.WD_RequiredDateInfo);
		}

		public void TestCheckWD_RequiredDateRestrictionOnPreDateWithFinalisedDateOverride()
		{
			WhsWarehouse warehouse = Helper.CreateWarehouse("1");
			Docket.WD_WW_Whs = warehouse.PK;
			warehouse.WW_UseRequiredDateForOutwardsFinalisedDate = true;

			Env.Security.WhsOrderPreDate.IsAllowed = true;
			Docket.WD_RequiredDate = ZDateTimeOffset.Today.AddDays(-8);
			AssertNoErrors(Docket.WD_RequiredDateInfo);

			Env.Security.WhsOrderPreDate.IsAllowed = false;
			Docket.WD_RequiredDate = ZDateTimeOffset.Today.AddDays(-7);
			AssertNoErrors(Docket.WD_RequiredDateInfo);

			Docket.WD_RequiredDate = ZDateTimeOffset.Today.AddDays(-8);
			AssertHasErrors(Docket.WD_RequiredDateInfo);
		}

		#endregion

		#region TestCheckWD_TotalUnits

		public void TestCheckWD_TotalUnits()
		{
			Docket.WD_TotalUnits = 5m;
			AssertNotEquals("Precondition - make sure that WD_TotalUnits is different to Docket.WD_TotalUnitsFromLines", Docket.WD_TotalUnits, Docket.WD_TotalUnitsFromLines);
			AssertHasWarning(Docket.WD_TotalUnitsInfo, "Total Units 5 does not equal the total of all line units 0.");

			var orderLine = Docket.Lines.AddNew();
			orderLine.WE_TransactionQuantity = 5m;
			Docket.Validation.ValidateWD_TotalUnits();
			AssertNoWarnings(Docket.WD_TotalUnitsInfo);

			var client = Helper.CreateClient();
			var product = Helper.CreateProduct(client, "BOWLHAT");

			Docket.WD_OH_Client = client.PK;
			orderLine.WE_OP = product.PK;

			AssertNoWarnings(Docket.WD_TotalUnitsInfo);

			var poke = orderLine.WE_ShortfallQuantityCached;
			AssertHasWarning(Docket.WD_TotalUnitsInfo, "One or more Order Lines are in Shortfall.");
		}

		#endregion

		#region TestCheckSumOfUnitsMetEqualsUnitsSent

		public void TestCheckSumOfUnitsMetEqualsUnitsSent()
		{
			Docket.Delete(); // dodgy, I know.

			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Helper.CreatePickNew(order);

			order.WD_UnitsSent = 4;
			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.Quantity = 4;

			AssertEquals("Units is automatically reduced", 3m, order.WD_UnitsSent);
			AssertHasWarning(order.WD_UnitsSentInfo, "The quantity in the Units Sent field on the Release tab, does not match the sum of the Quantity Met fields on the Order Lines tab.");

			order.WD_UnitsSent = 4;
			AssertEquals("Precondition", 4m, releaseLine.Quantity);
			AssertNoWarnings(order.WD_UnitsSentInfo);
		}

		public void TestCheckSumOfUnitsMetEqualsUnitsSent_PickByBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 70m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 2m);
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			Factory.Save();

			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals("Precondition", 2m, order.WD_UnitsSent);
			AssertEquals("Precondition", 2m, releaseLine.Quantity);

			order.Validation.ValidateWD_UnitsSent();
			AssertNoWarnings(order.WD_UnitsSentInfo);
		}

		#endregion

		#region TestCheckWD_DocketStatus

		public override void TestCheckWD_DocketStatus()
		{
			base.TestCheckWD_DocketStatus();

			Docket.WD_WP = ZGuid.Empty;
			Docket.WD_DocketStatus = "";
			AssertEquals("Docket status validation accepting blank", true, Docket.WD_DocketStatusInfo.HasErrors());

			// here we test each type, but only Receive should be accepted
			for (int i = 0; i < Docket.Statuses.Count; i++)
			{
				Docket.WD_DocketStatus = Docket.Statuses[i].Code;
				if ((Docket.WD_DocketStatus == DocketStatus.Codes.New) ||
						(Docket.WD_DocketStatus == DocketStatus.Codes.Entered) ||
						(Docket.WD_DocketStatus == DocketStatus.Codes.Cancelled) ||
						(Docket.WD_DocketStatus == DocketStatus.Codes.Error) ||
						(Docket.WD_DocketStatus == DocketStatus.Codes.Held) ||
						(Docket.WD_DocketStatus == WhsOrderStatus.Codes.Departed))
				{
					AssertEquals("Docket status validation NOT accepting valid docket status: " + Docket.WD_DocketStatus, false, Docket.WD_DocketStatusInfo.HasErrors());
				}
				else
				{
					AssertEquals("Docket status validation accepting invalid docket status: " + Docket.WD_DocketStatus, true, Docket.WD_DocketStatusInfo.HasErrors());
				}
			}

			// Also test a dodgy random type
			Docket.WD_DocketStatus = ";;;";
			AssertEquals("Docket status validation accepting junk", true, Docket.WD_DocketStatusInfo.HasErrors());
		}

		#endregion

		#region TestCheckWD_UseDirectedPackingConsolidation

		public void TestCheckWD_UseDirectedPackingConsolidation_Disabled()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			order.WD_UseDirectedPackingConsolidation = false;
			AssertNoErrors(order.WD_UseDirectedPackingConsolidationInfo);

			order.WD_WW_Whs = ZGuid.Empty;
			order.WD_UseDirectedPackingConsolidation = false;
			AssertNoErrors(order.WD_UseDirectedPackingConsolidationInfo);
		}

		public void TestCheckWD_UseDirectedPackingConsolidation_Enabled()
		{
			const string ErrorMessage = "Use Directed Packing Consolidation should not be enabled as there are no Packing Consolidation Locations in the Order's Warehouse.";

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			order.WD_UseDirectedPackingConsolidation = true;
			AssertHasError(order.WD_UseDirectedPackingConsolidationInfo, ErrorMessage);

			order.WD_UseDirectedPackingConsolidation = false;
			AssertNoError(order.WD_UseDirectedPackingConsolidationInfo, ErrorMessage);

			var packingConsolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;
			Factory.Save();

			order.WD_UseDirectedPackingConsolidation = true;
			AssertNoError(order.WD_UseDirectedPackingConsolidationInfo, ErrorMessage);
		}

		public void TestCheckWD_UseDirectedPackingConsolidation_Enabled_Cancelled()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.CancelReactivateDocket();
			AssertEquals("Precondition.", true, order.IsCancelled);

			order.WD_UseDirectedPackingConsolidation = true;
			AssertNoErrors(order.WD_UseDirectedPackingConsolidationInfo);
		}

		public void TestCheckWD_UseDirectedPackingConsolidation_Enabled_Finalized()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);

			order.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition.", true, order.IsFinalised);

			order.WD_UseDirectedPackingConsolidation = true;
			AssertNoErrors(order.WD_UseDirectedPackingConsolidationInfo);
		}

		#endregion

		#region TestStagingArea

		public void TestCheckWD_WL_CrossDockHasWarningIfEmptyButLinesAreAllocated()
		{
			var order = Docket;
			AssertEquals("Precondition", 0, order.Lines.Count);
			order.Validation.ValidateWD_WL_CrossDock();
			AssertNoWarnings(order.WD_WL_CrossDockInfo);

			order.Lines.AddNew();
			Helper.CreateReservePickLine(order.Lines[0], Factory.NewWithValidTestData<WhsInventoryView>(), 1m);
			order.Validation.ValidateWD_WL_CrossDock();
			AssertHasWarning("Should have warning because the cross dock location is empty yet lines are allocated", order.WD_WL_CrossDockInfo, "No cross-dock location has been specified for the cross docked lines");

			order.WD_WL_CrossDock = Factory.New<WhsArea>().PK;
			order.Validation.ValidateWD_WL_CrossDock();
			AssertNoWarnings("Should have no warning because the cross dock location is not empty", order.WD_WL_CrossDockInfo);
		}

		public void TestCheckWD_WL_CrossDockHasWarningIfVolumeExceeded()
		{
			Docket.Delete();

			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.FindLocation(WhsWarehouse.DefaultDockDoorRowName);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsOrderLine(order, data.Part1, 2);

			order.WD_WL_CrossDock = location.PK;
			location.WLV_MaxCubic = 2.0f;
			location.WLV_MaxCubicUnit = Core.Constants.Volume.CubicMetres;
			data.Part1.OP_Cubic = 1.2f;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inv = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2);
			inv.WI_WL = ZGuid.Empty; // crossdocked stock usually won't have a location
			receive.RunPreSaveValidation(); // creates inventory docket lines
			var reservedPickLine = Helper.CreateReservePickLine(line1, inv, 2m);

			Factory.Save();
			order.Validation.ValidateWD_WL_CrossDock();
			AssertHasWarning("Should have warning because the cross dock location volume is exceeded", order.WD_WL_CrossDockInfo, "The cross-dock location maximum volume has been exceeded");

			reservedPickLine.ReservedQuantity = 1m;
			Factory.Save();
			order.Validation.ValidateWD_WL_CrossDock();
			AssertNoWarnings("Should not have warning because the cross dock location volume is not exceeded", order.WD_WL_CrossDockInfo);

			// make sure an inventories volume is not counted twice if it is located in the cross dock location
			// and there is also a crossdock link. ie. don't count the inventory + link quantity as it is the same stock
			location.WLV_MaxCubic = 3.0f;
			inv.WI_WL = location.PK;

			Factory.Save();
			order.Validation.ValidateWD_WL_CrossDock();
			AssertNoWarnings("Should not have warning because the cross dock location volume is not exceeded", order.WD_WL_CrossDockInfo);
		}

		public void TestCheckWD_WL_CrossDock_ErrorIfLocationInAnotherWarehouse()
		{
			var warehouse1 = Helper.CreateWarehouse("WHS");
			var warehouse2 = Helper.CreateWarehouse("ABC");
			Factory.Save();

			var locationInWarehouse1 = warehouse1.FindLocation(WhsWarehouse.DefaultDockDoorRowName);
			var locationInWarehouse2 = warehouse2.FindLocation(WhsWarehouse.DefaultDockDoorRowName);
			var expectedErrorMessage = "Enter a valid Cross-Dock Location.";

			var order = Helper.CreateWhsOrder(Helper.CreateClient(), warehouse1);
			AssertNoErrors("Precondition", order.WD_WL_CrossDockInfo);

			order.WD_WL_CrossDock = locationInWarehouse1.PK;
			AssertNoError(order.WD_WL_CrossDockInfo, expectedErrorMessage);

			order.WD_WL_CrossDock = locationInWarehouse2.PK;
			AssertHasError(order.WD_WL_CrossDockInfo, expectedErrorMessage);

			order.WD_WL_CrossDock = locationInWarehouse1.PK;
			AssertNoError(order.WD_WL_CrossDockInfo, expectedErrorMessage);

			order.WD_WW_Whs = warehouse2.PK;
			AssertHasError(order.WD_WL_CrossDockInfo, expectedErrorMessage);

			order.WD_WW_Whs = warehouse1.PK;
			AssertNoError(order.WD_WL_CrossDockInfo, expectedErrorMessage);
		}

		public void TestWD_WL_CrossDockLocationIsDockDoorLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var dockDoorLocation = data.Whs1.FindLocation(WhsWarehouse.DefaultDockDoorRowName);
			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");

			var expectedErrorMessage = "The cross-dock location should have a Dock Door Location Type";
			var order = Helper.CreateWhsOrder(Helper.CreateClient(), data.Whs1);
			AssertNoErrors("Precondition", order.WD_WL_CrossDockInfo);

			order.WD_WL_CrossDock = nonDockDoorLocation.PK;
			AssertHasError(order.WD_WL_CrossDockInfo, expectedErrorMessage);

			order.WD_WL_CrossDock = dockDoorLocation.PK;
			AssertNoError(order.WD_WL_CrossDockInfo, expectedErrorMessage);
		}

		#endregion

		#region TestWD_RS_NKServiceLevel

		// removed until discussion over 'free text' OrgCarrierServiceLevels is resolved

		//public void TestWD_RS_NKServiceLevel()
		//{
		//    RefServiceLevel d2dServiceLevel = Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "D2D");
		//    RefServiceLevel stdServiceLevel = Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "STD");
		//    RefServiceLevel dirServiceLevel = Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "DIR");

		//    OrgHeader transportCo = Factory.New<OrgHeader>();
		//    OrgCarrierServiceLevel transportCoServiceLevel = transportCo.MiscServ.CarrierServiceLevels.AddNew();
		//    transportCoServiceLevel.PL_Code = "D2D";

		//    Docket.TransportCoDocAddress.OrganisationPK = transportCo.PK;
		//    AssertNoErrors("Precondition", Docket.WD_RS_NKServiceLevelInfo);

		//    Docket.WD_RS_NKServiceLevel = dirServiceLevel.PK;
		//    AssertHasError(Docket.WD_RS_NKServiceLevelInfo, "The Service Level is not supported by the selected Transport Company.");

		//    Docket.WD_RS_NKServiceLevel = d2dServiceLevel.PK;
		//    AssertNoErrors(Docket.WD_RS_NKServiceLevelInfo);

		//    Docket.WD_RS_NKServiceLevel = stdServiceLevel.PK;
		//    AssertNoErrors("Standard (STD) Service Level is supported by default.", Docket.WD_RS_NKServiceLevelInfo);

		//    // test missing uses the Warehouse error message
		//    Docket.WD_RS_NKServiceLevel = ZGuid.Missing;
		//    AssertHasError(Docket.WD_RS_NKServiceLevelInfo, "The Service Level is not supported by the selected Transport Company.");

		//    // test invalid guid
		//    Docket.WD_RS_NKServiceLevel = ZGuid.Invalid;
		//    AssertHasError(Docket.WD_RS_NKServiceLevelInfo, "Enter a valid Service Level.");

		//    Docket.TransportCoDocAddress.OrganisationPK = ZGuid.Empty;
		//    Docket.WD_RS_NKServiceLevel = ZGuid.NewZGuid();
		//    AssertNoErrors("If no Transport Co is selected, all Service Levels are valid.", Docket.WD_RS_NKServiceLevelInfo);
		//}

		#endregion

		#region TestConsigneeNameOrPKValidation

		public override void TestConsigneeNameOrPKValidation()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_IsConsignee = true;

			// new order
			var docket = GetNewBusinessObject();
			AssertEquals("Precondition", docket.WD_DocketStatus, DocketStatus.Codes.New);
			docket.ConsigneeDocAddress.E2_AddressOverride = false;
			docket.Validation.ValidateConsigneeNameOrPK();
			AssertHasErrors(docket.ConsigneeNameOrPKInfo);

			docket.ConsigneeDocAddress.OrganisationPK = consignee.PK;
			docket.Validation.ValidateConsigneeNameOrPK();
			AssertNoErrors(docket.ConsigneeNameOrPKInfo);

			docket.ConsigneeDocAddress.E2_AddressOverride = true;
			docket.ConsigneeDocAddress.E2_CompanyName = "Blah";
			docket.Validation.ValidateConsigneeNameOrPK();
			AssertNoErrors(docket.ConsigneeNameOrPKInfo);

			docket.ConsigneeDocAddress.E2_CompanyName = "";
			docket.Validation.ValidateConsigneeNameOrPK();
			AssertHasErrors(docket.ConsigneeNameOrPKInfo);

			// attached to pick order
			var orderATP = GetNewBusinessObject();
			orderATP.WD_WP = ZGuid.NewZGuid();
			orderATP.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
			AssertConsigneeDocAddress(consignee, orderATP);

			// picked order
			var orderPicked = GetNewBusinessObject();
			orderPicked.WD_WP = ZGuid.NewZGuid();
			orderPicked.WD_DocketStatus = DocketStatus.Codes.Picking;
			AssertConsigneeDocAddress(consignee, orderPicked);

			// cancelled order
			var orderCancelled = GetNewBusinessObject();
			orderCancelled.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			AssertConsigneeDocAddress(consignee, orderCancelled);

			// finalised order
			var orderFinalised = GetNewBusinessObject();
			orderFinalised.WD_DocketStatus = WhsOrderStatus.Codes.Loaded;
			orderFinalised.WD_FinalisedDate = ZDateTimeOffset.Now;
			AssertConsigneeDocAddress(consignee, orderFinalised);
		}

		static void AssertConsigneeDocAddress(OrgHeader consignee, WhsOrder order)
		{
			order.ConsigneeDocAddress.E2_AddressOverride = false;
			order.Validation.ValidateConsigneeNameOrPK();
			AssertNoErrors(order.ConsigneeNameOrPKInfo);

			order.ConsigneeDocAddress.OrganisationPK = consignee.PK;
			order.Validation.ValidateConsigneeNameOrPK();
			AssertNoErrors(order.ConsigneeNameOrPKInfo);

			order.ConsigneeDocAddress.E2_AddressOverride = true;
			order.ConsigneeDocAddress.E2_CompanyName = "Blah";
			order.Validation.ValidateConsigneeNameOrPK();
			AssertNoErrors(order.ConsigneeNameOrPKInfo);

			order.ConsigneeDocAddress.E2_CompanyName = "";
			order.Validation.ValidateConsigneeNameOrPK();
			AssertNoErrors(order.ConsigneeNameOrPKInfo);
		}

		#endregion

		#region TestIncoTerms

		[TestDate(2010, 1, 1)]
		public void TestCheckWD_INCO_Applies2010IncoTerms_ForInternational()
		{
			var org = Helper.CreateClient("XZA");
			var docket = GetNewBusinessObject();
			docket.ConsigneePK = org.PK;
			org.MainAddress.OA_RL_NKRelatedPortCode = "USCHI";

			var warehouse = Helper.CreateWarehouse("XXX");
			docket.WD_WW_Whs = warehouse.PK;
			AssertEquals("Precondition", false, docket.IsDomesticFreight);

			docket.WD_RequiredDate = ZDateTimeOffset.Empty;
			foreach (CodeDescriptionPair pair in docket.Lookups.INCOTerms)
			{
				docket.WD_INCO = pair.Code;
				AssertNoWarnings(docket.WD_INCOInfo);
			}

			docket.WD_RequiredDate = new ZDateTimeOffset(2010, 12, 31);
			foreach (CodeDescriptionPair pair in docket.Lookups.INCOTerms)
			{
				docket.WD_INCO = pair.Code;
				AssertNoWarnings(docket.WD_INCOInfo);
			}

			// test using Jan 01

			docket.WD_RequiredDate = new ZDateTimeOffset(2011, 01, 01); // date is on or aafter Jan 1 2011
			var allIncoTerms = docket.Lookups.INCOTerms.Cast<CodeDescriptionPair>();

			// test against invalid INCOterms
			foreach (var incoTerm in allIncoTerms.Where(incoTerm => docket.Validation.IsIncoTermValidUnderIncoTerms2010Rules(incoTerm.Code)))
			{
				docket.WD_INCO = incoTerm.Code;
				AssertNoWarnings(docket.WD_INCOInfo);
			}

			// test against valid INCOterms
			foreach (var pair in allIncoTerms.Where(incoTerm => !docket.Validation.IsIncoTermValidUnderIncoTerms2010Rules(incoTerm.Code)))
			{
				docket.WD_INCO = pair.Code;
				AssertHasWarning(docket.WD_INCOInfo, "This Incoterm is obsolete from 1 January 2011 according to the Incoterms 2010 rules.");
			}
		}

		public void TestCheckWD_INCONotValidateIfReadOnly()
		{
			var docket = GetNewBusinessObject();
			docket.WD_DocketStatus = DocketStatus.Codes.Entered;
			var pick = Factory.New<WhsPick>();
			docket.Lines.AddNew();
			pick.Orders.Add(docket);
			docket.WD_INCO = "123";
			AssertEquals(true, docket.WD_INCOInfo.HasErrors());

			docket.WD_FinalisedDate = ZDateTimeOffset.Now;
			pick.WP_PickStatus = PickStatus.Codes.Finalised;
			Helper.AddOnePostedChargeLine(docket);

			docket.RunPreSaveValidation();
			AssertEquals(false, docket.WD_INCOInfo.HasErrors());
		}

		#endregion

		#region TestCheckWD_INCO

		public void TestValidateWD_INCO()
		{
			var docket = GetNewBusinessObject();
			AssertNoErrors(docket.WD_INCOInfo);

			docket.WD_INCO = "123";
			AssertHasError(docket.WD_INCOInfo, "Enter a valid " + docket.WD_INCOInfo.Description + ".");

			docket.WD_INCO = docket.Lookups.INCOTerms[0].Code;
			AssertNoErrors(docket.WD_INCOInfo);
		}

		#endregion

		#region TestWD_CODPayMethod

		public void TestValidateWD_CODPayMethod()
		{
			var docket = GetNewBusinessObject();
			AssertNoErrors(docket.WD_CODPayMethodInfo);

			docket.WD_CODPayMethod = "123";
			AssertHasError(docket.WD_CODPayMethodInfo, "Enter a valid " + docket.WD_CODPayMethodInfo.HumanReadableName + ".");

			docket.WD_CODPayMethod = docket.Lookups.ShipperCODPaymentTypes[0].Code;
			AssertNoErrors(docket.WD_CODPayMethodInfo);
		}

		#endregion

		#region TestValidateAll_ConsigneeDocAddress

		public void TestValidateAll_ConsigneeDocAddress()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");

			order.ConsigneeAddressPK = data.Org1.MainAddress.PK;
			order.Validation.ValidateAll();
			AssertNoErrors("Precondition", order.ConsigneeDocAddress.E2_CompanyNameInfo);
			AssertNoErrors("Precondition", order.ConsigneeDocAddress.E2_CityInfo);

			// consignee docaddress with override
			order.ConsigneeDocAddress.E2_AddressOverride = true;
			order.ConsigneeDocAddress.E2_CompanyName = "";
			order.ConsigneeDocAddress.E2_City = "";
			order.Validation.ValidateAll();
			AssertHasErrors(order.ConsigneeDocAddress.E2_CompanyNameInfo);
			AssertHasErrors(order.ConsigneeDocAddress.E2_CityInfo);

			order.ConsigneeDocAddress.E2_CompanyName = "ABCD";
			order.ConsigneeDocAddress.E2_City = "SYDNEY";
			order.Validation.ValidateAll();
			AssertNoErrors(order.ConsigneeDocAddress.E2_CompanyNameInfo);
			AssertNoErrors(order.ConsigneeDocAddress.E2_CityInfo);

			// consignee docaddress without override 
			order.ConsigneeDocAddress.E2_AddressOverride = false;
			order.ConsigneeDocAddress.E2_CompanyName = "";
			order.ConsigneeDocAddress.E2_City = "";
			order.Validation.ValidateAll();
			AssertNoErrors(order.ConsigneeDocAddress.E2_CompanyNameInfo);
			AssertNoErrors(order.ConsigneeDocAddress.E2_CityInfo);

			order.ConsigneeDocAddress.E2_CompanyName = "ABCD";
			order.ConsigneeDocAddress.E2_City = "SYDNEY";
			order.Validation.ValidateAll();
			AssertNoErrors(order.ConsigneeDocAddress.E2_CompanyNameInfo);
			AssertNoErrors(order.ConsigneeDocAddress.E2_CityInfo);
		}

		#endregion

		#region TestCheckWD_GS_NKAssignedPacker

		public void TestCheckWD_GS_NKAssignedPacker()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			Assert("Precondition", order.WD_WP.IsEmpty);
			AssertNoErrors(order.WD_GS_NKAssignedPackerInfo);

			order.WD_GS_NKAssignedPacker = "ABC";
			AssertHasError(order.WD_GS_NKAssignedPackerInfo, "You cannot assign a packer if the order is not yet attached to a pick.");

			order.WD_GS_NKAssignedPacker = "";
			AssertNoErrors(order.WD_GS_NKAssignedPackerInfo);

			Helper.CreatePickNew(order);
			Assert("Precondition", !order.WD_WP.IsEmpty);

			order.WD_GS_NKAssignedPacker = "ABC";
			AssertNoErrors(order.WD_GS_NKAssignedPackerInfo);
		}

		#endregion

		#region Implementation

		protected override IEnumerable<string> ValidSubTypeForBondedWarehouse => new[] { OrderType.Codes.Customs, OrderType.Codes.CustomsReleaseWithPermit };
		protected override IEnumerable<string> ValidSubTypeForInwardProcessing => new[] { OrderType.Codes.Customs };

		#endregion
	}
}
