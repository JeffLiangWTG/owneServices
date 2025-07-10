using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Schema;
using OrgHeaderDO = CargoWise.Database.TestFramework.ObjectModel.OrgHeader;
using WhsLocationDO = CargoWise.Database.TestFramework.ObjectModel.WhsLocation;
using WhsWarehouseDO = CargoWise.Database.TestFramework.ObjectModel.WhsWarehouse;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	class WhsWarehouseValidationTestCase : WhsBusinessObjectValidationTestCase
	{
		#region TestCheckWW_UseGS1PrefixFallback

		public void TestCheckWW_UseGS1PrefixFallback()
		{
			var org = Factory.New<OrgHeader>();
			Whs.WW_OA_WarehouseAddress = org.MainAddress.PK;
			AssertNoErrors("Precondition", Whs.WW_UseGS1PrefixFallbackInfo);

			Whs.WW_UseGS1PrefixFallback = true;
			AssertHasError(Whs.WW_UseGS1PrefixFallbackInfo, "Cannot use a GS1 prefix as it does not exist for this address.");

			Whs.WW_UseGS1PrefixFallback = false;
			AssertNoErrors(Whs.WW_UseGS1PrefixFallbackInfo);

			var customsCode = Whs.WarehouseAddress.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GS1, "1111111");
			Whs.WW_UseGS1PrefixFallback = true;
			AssertNoErrors(Whs.WW_UseGS1PrefixFallbackInfo);

			Whs.WW_OA_WarehouseAddress = ZGuid.NewZGuid();
			AssertHasError(Whs.WW_UseGS1PrefixFallbackInfo, "Cannot use a GS1 prefix as it does not exist for this address.");

			Whs.WW_OA_WarehouseAddress = org.MainAddress.PK;
			AssertNoErrors(Whs.WW_UseGS1PrefixFallbackInfo);

			customsCode.OK_OA_PremisesAddress = ZGuid.NewZGuid();
			Whs.WW_UseGS1PrefixFallback = true;
			AssertHasError(Whs.WW_UseGS1PrefixFallbackInfo, "Cannot use a GS1 prefix as it does not exist for this address.");

			Whs.WW_UseGS1PrefixFallback = false;
			AssertNoErrors(Whs.WW_UseGS1PrefixFallbackInfo);
		}

		#endregion

		#region TestCheckWW_LocationComponentDelimiter

		public void TestCheckWW_LocationComponentDelimiter()
		{
			Whs.WW_LocationComponentDelimiter = "/";
			Assert("Valid Location Delimter should not have errors", !Whs.WW_LocationComponentDelimiterInfo.HasErrors());

			Whs.WW_LocationComponentDelimiter = "";
			Assert("Blank Location Delimter should have errors", Whs.WW_LocationComponentDelimiterInfo.HasErrors());

			Whs.WW_LocationComponentDelimiter = " ";
			Assert("Space Location Delimter should have errors", Whs.WW_LocationComponentDelimiterInfo.HasErrors());
		}

		#endregion

		#region TestCheckWW_LocationColumnsAlphaOk

		public void TestCheckWW_LocationColumnsAlphaOk()
		{
			Row.WR_Columns = 10;
			Whs.WW_LocationColumnsAlpha = ZBool.True;
			Assert("Alpha Columns can be used", !Whs.WW_LocationColumnsAlphaInfo.HasErrors());
		}

		#endregion

		#region TestCheckWW_LocationColumnsAlphaNotOk

		public void TestCheckWW_LocationColumnsAlphaNotOk()
		{
			Row.WR_Columns = 27;
			Whs.WW_LocationColumnsAlpha = ZBool.True;
			Assert("Alpha Columns cannot be used", Whs.WW_LocationColumnsAlphaInfo.HasErrors());
		}

		#endregion

		#region TestCheckWW_LocationColumnsNumbers

		public void TestCheckWW_LocationColumnsNumbers()
		{
			Row.WR_Columns = 999;
			Whs.WW_LocationColumnsAlpha = ZBool.False;
			Assert("Number Columns used", !Whs.WW_LocationColumnsAlphaInfo.HasErrors());
		}

		#endregion

		#region TestCheckWW_LocationLevelsAlphaOk

		public void TestCheckWW_LocationLevelsAlphaOk()
		{
			Row.WR_Levels = 10;
			Whs.WW_LocationLevelsAlpha = ZBool.True;
			Assert("Alpha Levels can be used", !Whs.WW_LocationLevelsAlphaInfo.HasErrors());
		}

		#endregion

		#region TestCheckWW_LocationLevelsAlphaNotOk

		public void TestCheckWW_LocationLevelsAlphaNotOk()
		{
			Row.WR_Levels = 27;
			Whs.WW_LocationLevelsAlpha = ZBool.True;
			Assert("Alpha Levels cannot be used", Whs.WW_LocationLevelsAlphaInfo.HasErrors());
		}

		#endregion

		#region TestCheckWW_LocationLevelsNumbers

		public void TestCheckWW_LocationLevelsNumbers()
		{
			Row.WR_Levels = 999;
			Whs.WW_LocationLevelsAlpha = ZBool.False;
			Assert("Number Levels used", !Whs.WW_LocationLevelsAlphaInfo.HasErrors());
		}

		#endregion

		#region TestCheckWW_LocationTraysAlphaOk

		public void TestCheckWW_LocationTraysAlphaOk()
		{
			Row.WR_Trays = 10;
			Whs.WW_LocationTraysAlpha = ZBool.True;
			Assert("Alpha Trays can be used", !Whs.WW_LocationTraysAlphaInfo.HasErrors());
		}

		#endregion

		#region TestCheckWW_LocationTraysAlphaNotOk

		public void TestCheckWW_LocationTraysAlphaNotOk()
		{
			Row.WR_Trays = 27;
			Whs.WW_LocationTraysAlpha = ZBool.True;
			Assert("Alpha Trays cannot be used", Whs.WW_LocationTraysAlphaInfo.HasErrors());
		}

		#endregion

		#region TestCheckWW_LocationTraysNumbers

		public void TestCheckWW_LocationTraysNumbers()
		{
			Row.WR_Trays = 99;
			Whs.WW_LocationTraysAlpha = ZBool.False;
			Assert("Number Trays used", !Whs.WW_LocationTraysAlphaInfo.HasErrors());
		}

		#endregion

		#region TestCheckWW_WarehouseName

		public void TestCheckWW_WarehouseName()
		{
			Whs.WW_WarehouseName = "AA";
			AssertEquals("Valid", false, Whs.WW_WarehouseNameInfo.HasErrors());
			Whs.WW_WarehouseName = "";
			AssertHasError(Whs.WW_WarehouseNameInfo, "Please enter a Warehouse Name");
		}

		#endregion

		#region TestCheckWW_WarehouseNameForDuplicates

		public void TestCheckWW_WarehouseNameForDuplicates()
		{
			Whs.WW_WarehouseName = "AA";

			WhsWarehouse whs2 = Factory.New<WhsWarehouse>();
			whs2.WW_WarehouseName = "AB";
			AssertEquals("Should be ok", false, whs2.WW_WarehouseNameInfo.HasErrors());

			whs2.WW_WarehouseName = "AA";
			AssertHasErrors(whs2.WW_WarehouseNameInfo);
		}

		#endregion

		#region TestCheckWW_WarehouseCode

		public void TestCheckWW_WarehouseCode()
		{
			Whs.WW_WarehouseCode = "AA";
			AssertEquals("Valid", false, Whs.WW_WarehouseCodeInfo.HasErrors());
			Whs.WW_WarehouseCode = "";
			AssertHasError(Whs.WW_WarehouseCodeInfo, "Please enter a Warehouse Code");
		}

		#endregion

		#region TestCheckWW_WarehouseCodeForDuplicates

		public void TestCheckWW_WarehouseCodeForDuplicates()
		{
			Whs.WW_WarehouseCode = "AA";

			WhsWarehouse whs2 = Factory.New<WhsWarehouse>();
			whs2.WW_WarehouseCode = "AB";
			AssertEquals("Should be ok", false, whs2.WW_WarehouseCodeInfo.HasErrors());

			whs2.WW_WarehouseCode = "AA";
			AssertHasErrors(whs2.WW_WarehouseCodeInfo);
		}

		#endregion

		#region TestCheckWW_WarehouseType

		public void TestCheckWW_WarehouseType()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			AssertNoErrors("Precondition", warehouse.WW_WarehouseTypeInfo);

			warehouse.WW_WarehouseType = "";
			AssertHasError(warehouse.WW_WarehouseTypeInfo, "Please enter a Warehouse Type.");

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			AssertNoErrors(warehouse.WW_WarehouseTypeInfo);

			warehouse.WW_WarehouseType = "XXX";
			AssertHasError(warehouse.WW_WarehouseTypeInfo, "Enter a valid Warehouse Type.");

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			AssertNoErrors(warehouse.WW_WarehouseTypeInfo);

			warehouse.WW_WarehouseType = "123";
			AssertHasError(warehouse.WW_WarehouseTypeInfo, "Enter a valid Warehouse Type.");

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			AssertNoErrors(warehouse.WW_WarehouseTypeInfo);

			warehouse.WW_WarehouseType = "";
			AssertHasError(warehouse.WW_WarehouseTypeInfo, "Please enter a Warehouse Type.");

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			AssertNoErrors(warehouse.WW_WarehouseTypeInfo);
		}

		#endregion

		#region TestCheckWW_WarehouseType_CannotBeChangedToOrFromTransitIfReferencedAlready

		public void TestCheckWW_WarehouseType_CannotBeChangedToOrFromTransitIfReferencedAlready()
		{
			var client = Helper.CreateClient();
			// specifically create rows on each warehouse to test Validation ignores Rows & Areas
			var warehouseWithProductTransactions = Helper.CreateWarehouse("WHS", "A");
			var warehouseWithTransitTransactions = Helper.CreateWarehouse("TRA", "A");
			var warehouseWithNoTransactions = Helper.CreateWarehouse("NON", "A");
			warehouseWithTransitTransactions.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			Factory.Save();

			// add transit & product transactions that reference the warehouse
			var sqlWarehouseWithProductTransactions = WhsWarehouseDO.ShallowLoadFromDB(TestConnection, w => w.PK == warehouseWithProductTransactions.PK)[0];
			var sqlWarehouseWithTransitTransactions = WhsWarehouseDO.ShallowLoadFromDB(TestConnection, w => w.PK == warehouseWithTransitTransactions.PK)[0];
			var sqlWarehouseWithTransitTransactionsLoc = WhsLocationDO.ShallowLoadFromDB(TestConnection, w => w.PK == warehouseWithTransitTransactions.DefaultLocation.PK)[0];
			var sqlClient = OrgHeaderDO.ShallowLoadFromDB(TestConnection, c => c.PK == client.PK)[0];

			new CargoWise.Database.TestFramework.ObjectModel.JobStorage("ID123", sqlWarehouseWithProductTransactions, sqlClient, DateTime.Now, DateTime.Now).Insert(TestConnection);
			new CargoWise.Database.TestFramework.ObjectModel.WhsItemReceiveTransportationUnit(sqlWarehouseWithTransitTransactions, "R1", sqlWarehouseWithTransitTransactionsLoc, "car123").Insert(TestConnection);

			AssertNoErrors("Precondition", warehouseWithProductTransactions.WW_WarehouseTypeInfo);
			AssertNoErrors("Precondition", warehouseWithTransitTransactions.WW_WarehouseTypeInfo);
			AssertNoErrors("Precondition", warehouseWithNoTransactions.WW_WarehouseTypeInfo);

			// Warehouse with Product Warehouse Transactions should not be allowed to change to a Transit Warehouse
			var errorMsg = "Cannot change Warehouse Type as this Warehouse is already in use.";
			warehouseWithProductTransactions.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			AssertHasError(warehouseWithProductTransactions.WW_WarehouseTypeInfo, errorMsg);

			warehouseWithProductTransactions.WW_WarehouseType = WarehouseTypes.Codes.Product;
			AssertNoErrors(warehouseWithProductTransactions.WW_WarehouseTypeInfo);

			warehouseWithProductTransactions.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			AssertNoErrors(warehouseWithProductTransactions.WW_WarehouseTypeInfo);

			// Warehouse with Transit Warehouse Transactions should not be allowed to change to a Product Warehouse
			warehouseWithTransitTransactions.WW_WarehouseType = WarehouseTypes.Codes.Product;
			AssertHasError(warehouseWithTransitTransactions.WW_WarehouseTypeInfo, errorMsg);

			warehouseWithTransitTransactions.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			AssertNoErrors(warehouseWithTransitTransactions.WW_WarehouseTypeInfo);

			warehouseWithTransitTransactions.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			AssertHasError(warehouseWithTransitTransactions.WW_WarehouseTypeInfo, errorMsg);

			warehouseWithTransitTransactions.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			AssertNoErrors(warehouseWithTransitTransactions.WW_WarehouseTypeInfo);

			// Warehouse with no transactions should have no error no matter which Warehouse Type is set
			warehouseWithNoTransactions.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			AssertNoErrors(warehouseWithNoTransactions.WW_WarehouseTypeInfo);

			warehouseWithNoTransactions.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			AssertNoErrors(warehouseWithNoTransactions.WW_WarehouseTypeInfo);

			warehouseWithNoTransactions.WW_WarehouseType = WarehouseTypes.Codes.Product;
			AssertNoErrors(warehouseWithNoTransactions.WW_WarehouseTypeInfo);
		}

		#endregion

		#region TestCheckWW_WarehouseType_CanBeVirtualWarehouse

		public void TestCheckWW_WarehouseType_CanBeVirtualWarehouse()
		{
			var warehouse = Helper.CreateWarehouse("WHS", "A");
			AssertEquals("Precondition:", WarehouseTypes.Codes.Product, warehouse.WW_WarehouseType);

			warehouse.WW_IsVirtualWarehouse = true;
			AssertNoError("Product Warehouse can be virtual warehouse.", warehouse.WW_WarehouseTypeInfo, "This type of warehouse cannot be virtual warehouse.");

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			AssertEquals("Precondition:", true, warehouse.WW_IsVirtualWarehouse);
			AssertHasError(warehouse.WW_WarehouseTypeInfo, "This type of warehouse cannot be virtual warehouse.");

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			AssertEquals("Precondition:", true, warehouse.WW_IsVirtualWarehouse);
			AssertNoError("Product Warehouse can be virtual warehouse.", warehouse.WW_WarehouseTypeInfo, "This type of warehouse cannot be virtual warehouse.");

			warehouse.WW_IsVirtualWarehouse = false;
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			AssertNoError("Container Yard cannot be virtual warehouse.", warehouse.WW_WarehouseTypeInfo, "This type of warehouse cannot be virtual warehouse.");
		}

		#endregion

		#region TestCheckWW_WarehouseType_CannotBeChangedToContainerYardIfReferencedAlready

		public void TestCheckWW_WarehouseType_CannotBeChangedToContainerYardIfReferencedAlready_WarehouseHasTransaction_Product()
		{
			TestCheckWW_WarehouseType_CannotBeChangedToContainerYardIfReferencedAlready_WarehouseHasTransaction_Core(WarehouseTypes.Codes.Product);
		}

		public void TestCheckWW_WarehouseType_CannotBeChangedToContainerYardIfReferencedAlready_WarehouseHasTransaction_Transit()
		{
			TestCheckWW_WarehouseType_CannotBeChangedToContainerYardIfReferencedAlready_WarehouseHasTransaction_Core(WarehouseTypes.Codes.Transit);
		}

		public void TestCheckWW_WarehouseType_CannotBeChangedToContainerYardIfReferencedAlready_WarehouseHasTransaction_FreeTradeZone()
		{
			TestCheckWW_WarehouseType_CannotBeChangedToContainerYardIfReferencedAlready_WarehouseHasTransaction_Core(WarehouseTypes.Codes.FreeTradeZone);
		}

		public void TestCheckWW_WarehouseType_CannotBeChangedToContainerYardIfReferencedAlready_WarehouseHasTransaction_Core(string originalWarehouseType)
		{
			var client = Helper.CreateClient();
			var warehouseWithTransactions = Helper.CreateWarehouse("WHS", "A");
			warehouseWithTransactions.WW_WarehouseType = originalWarehouseType;
			Factory.Save();

			var sqlWarehouseWithTransactions = WhsWarehouseDO.ShallowLoadFromDB(TestConnection, w => w.PK == warehouseWithTransactions.PK)[0];

			if (originalWarehouseType == WarehouseTypes.Codes.Transit)
			{
				var sqlWarehouseWithTransactionsLoc = WhsLocationDO.ShallowLoadFromDB(TestConnection, w => w.PK == warehouseWithTransactions.DefaultLocation.PK)[0];
				new CargoWise.Database.TestFramework.ObjectModel.WhsItemReceiveTransportationUnit(sqlWarehouseWithTransactions, "R1", sqlWarehouseWithTransactionsLoc, "car123").Insert(TestConnection);
			}
			else
			{
				var sqlClient = OrgHeaderDO.ShallowLoadFromDB(TestConnection, c => c.PK == client.PK)[0];
				new CargoWise.Database.TestFramework.ObjectModel.JobStorage("ID123", sqlWarehouseWithTransactions, sqlClient, DateTime.Now, DateTime.Now).Insert(TestConnection);
			}
			AssertNoErrors("Precondition", warehouseWithTransactions.WW_WarehouseTypeInfo);

			warehouseWithTransactions.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			AssertHasError(warehouseWithTransactions.WW_WarehouseTypeInfo, "Cannot change Warehouse Type as this Warehouse is already in use.");
		}

		public void TestCheckWW_WarehouseType_CannotBeChangedToContainerYardIfReferencedAlready_WarehouseHasNoTransaction_Product()
		{
			TestCheckWW_WarehouseType_CannotBeChangedToContainerYardIfReferencedAlready_WarehouseHasNoTransaction_Core(WarehouseTypes.Codes.Product);
		}

		public void TestCheckWW_WarehouseType_CannotBeChangedToContainerYardIfReferencedAlready_WarehouseHasNoTransaction_Transit()
		{
			TestCheckWW_WarehouseType_CannotBeChangedToContainerYardIfReferencedAlready_WarehouseHasNoTransaction_Core(WarehouseTypes.Codes.Transit);
		}

		public void TestCheckWW_WarehouseType_CannotBeChangedToContainerYardIfReferencedAlready_WarehouseHasNoTransaction_FreeTradeZone()
		{
			TestCheckWW_WarehouseType_CannotBeChangedToContainerYardIfReferencedAlready_WarehouseHasNoTransaction_Core(WarehouseTypes.Codes.FreeTradeZone);
		}

		public void TestCheckWW_WarehouseType_CannotBeChangedToContainerYardIfReferencedAlready_WarehouseHasNoTransaction_Core(string originalWarehouseType)
		{
			var warehouseWithNoTransactions = Helper.CreateWarehouse("NON", "A");
			warehouseWithNoTransactions.WW_WarehouseType = originalWarehouseType;
			Factory.Save();

			AssertNoErrors("Precondition", warehouseWithNoTransactions.WW_WarehouseTypeInfo);

			warehouseWithNoTransactions.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			AssertNoErrors(warehouseWithNoTransactions.WW_WarehouseTypeInfo);
		}

		public void TestCheckWW_WarehouseType_NewRecordCanChangedToContainerYard()
		{
			var warehouse = Helper.CreateWarehouse("WHS", "A");
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			AssertEquals("Precondition", false, warehouse.IsInDatabase);
			AssertNoErrors("Precondition", warehouse.WW_WarehouseTypeInfo);

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			AssertEquals("Precondition", false, warehouse.IsInDatabase);
			AssertNoErrors("Precondition", warehouse.WW_WarehouseTypeInfo);
		}

		public void TestCheckWW_WarehouseType_InactiveRecordCanChangedToContainerYard()
		{
			var warehouse = Helper.CreateWarehouse("WHS", "A");
			Factory.Save();

			AssertEquals("Precondition", true, warehouse.IsInDatabase);
			AssertEquals("Precondition", WarehouseTypes.Codes.Product, warehouse.WW_WarehouseType);
			AssertNoErrors("Precondition", warehouse.WW_WarehouseTypeInfo);

			warehouse.WW_IsActive = false;
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			AssertNoErrors("Precondition", warehouse.WW_WarehouseTypeInfo);
		}

		#endregion

		#region TestCheckWW_WarehouseType_CannotBeChangedToOrFromFTZIfReferencedByUnFinalisedPickWithCustomsOrder

		public void TestCheckWW_WarehouseType_CannotBeChangedToOrFromFTZIfReferencedByUnFinalisedPickWithCustomsOrder_CUS()
		{
			TestCheckWW_WarehouseType_CannotBeChangedToOrFromFTZIfReferencedByUnFinalisedPickWithCustomsOrderCore("CUS");
		}

		public void TestCheckWW_WarehouseType_CannotBeChangedToOrFromFTZIfReferencedByUnFinalisedPickWithCustomsOrder_CPS()
		{
			TestCheckWW_WarehouseType_CannotBeChangedToOrFromFTZIfReferencedByUnFinalisedPickWithCustomsOrderCore("CPS");
		}

		void TestCheckWW_WarehouseType_CannotBeChangedToOrFromFTZIfReferencedByUnFinalisedPickWithCustomsOrderCore(string subType)
		{
			var whs = Helper.CreateWarehouse("WHS", "A");
			var whsFTZ = Helper.CreateFTZWarehouse("FTZ", "A", countrycode: Enterprise.Core.Constants.CountryCodes.PuertoRico);

			var transactionsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var client = transactionsHelper.CreateClient("Client");
			var product = transactionsHelper.CreateProduct(client, "P1");

			// create unfinalised non-customs orders/picks, and finalised customs orders/picks
			CreatePickAndOrder(transactionsHelper, client, whs, "ORD", "O1", product.PK, finaliseOrder: true, finalisePick: true);
			CreatePickAndOrder(transactionsHelper, client, whs, subType, "O2", product.PK, finaliseOrder: false, finalisePick: false);
			CreatePickAndOrder(transactionsHelper, client, whs, "ORD", "O3", product.PK, finaliseOrder: false, finalisePick: false);
			CreatePickAndOrder(transactionsHelper, client, whsFTZ, subType, "O4", product.PK, finaliseOrder: true, finalisePick: true);
			CreatePickAndOrder(transactionsHelper, client, whsFTZ, "ORD", "O5", product.PK, finaliseOrder: false, finalisePick: false);

			using (PermitServiceTestHelper.MockServiceToGetPermits())
			{
				Factory.Save();
			}

			// preconditions
			AssertWarehouseTypeChangedHasNoErrors(whs, WarehouseTypes.Codes.FreeTradeZone);
			AssertWarehouseTypeChangedHasNoErrors(whs, WarehouseTypes.Codes.Product);
			AssertWarehouseTypeChangedHasNoErrors(whsFTZ, WarehouseTypes.Codes.Product);
			AssertWarehouseTypeChangedHasNoErrors(whsFTZ, WarehouseTypes.Codes.FreeTradeZone);

			// create unfinalised customs orders/picks
			CreatePickAndOrder(transactionsHelper, client, whs, subType, "O6", product.PK, finaliseOrder: false, finalisePick: false);
			CreatePickAndOrder(transactionsHelper, client, whsFTZ, subType, "O7", product.PK, finaliseOrder: false, finalisePick: false);

			using (PermitServiceTestHelper.MockServiceToGetPermits())
			{
				Factory.Save();
			}

			AssertWarehouseTypeChangedHasNoErrors(whs, WarehouseTypes.Codes.FreeTradeZone);
			whs.WarehouseAddress.OA_RN_NKCountryCode = "US";
			AssertWarehouseTypeChangedHasError(whs, WarehouseTypes.Codes.FreeTradeZone, "Cannot change the Warehouse Type to/from FTZ in US because there are un-finalized Picks or Orders with no pick in the Warehouse.");
			AssertWarehouseTypeChangedHasError(whsFTZ, WarehouseTypes.Codes.Product, "Cannot change the Warehouse Type to/from FTZ in PR because there are un-finalized Picks or Orders with no pick in the Warehouse.");
		}

		void AssertWarehouseTypeChangedHasNoErrors(WhsWarehouse whs, ZString changeToType)
		{
			whs.WW_WarehouseType = changeToType;
			AssertNoErrors(whs.WW_WarehouseTypeInfo);
		}

		void AssertWarehouseTypeChangedHasError(WhsWarehouse whs, ZString changeToType, string errorMessage)
		{
			whs.WW_WarehouseType = changeToType;
			AssertHasError(whs.WW_WarehouseTypeInfo, errorMessage);
		}

		void CreatePickAndOrder(IWhsTransactionTestHelper transactionsHelper, ZGuid client, WhsWarehouse whs, ZString orderType, ZString orderNo, ZGuid product, bool finaliseOrder, bool finalisePick)
		{
			var order = transactionsHelper.CreateWhsOrder(client, whs.PK, orderNo, Notify);
			var orderLine = transactionsHelper.CreateWhsOrderLine(order, product, 1m, "", "0001", "", "", "", 0m, 0m, "", 0m, "", "", 0m, ZGuid.Empty, "");

			var pick = transactionsHelper.CreateWhsPick(new[] { order });
			transactionsHelper.SetOrderType(order, orderType, orderType == "CUS");

			if (finaliseOrder)
			{
				transactionsHelper.FinaliseDocketWithoutUserConfirmation(order);
			}

			if (finalisePick)
			{
				transactionsHelper.FinalisePick(pick);
			}
		}

		#endregion

		#region TestCheckWW_WarehouseType_WithInvalidUNDGState

		public void TestCheckWW_WarehouseType_ProductWarehouse_ToContainerYard_WithInvalidUNDGState()
		{
			TestCheckWW_WarehouseType_ToContainerYard_WithInvalidUNDGStateCore(WarehouseTypes.Codes.Product);
		}

		public void TestCheckWW_WarehouseType_TransitWarehouse_ToContainerYard_WithInvalidUNDGState()
		{
			TestCheckWW_WarehouseType_ToContainerYard_WithInvalidUNDGStateCore(WarehouseTypes.Codes.Transit);
		}

		public void TestCheckWW_WarehouseType_FreeTradeZone_ToContainerYard_WithInvalidUNDGState()
		{
			TestCheckWW_WarehouseType_ToContainerYard_WithInvalidUNDGStateCore(WarehouseTypes.Codes.FreeTradeZone);
		}

		public void TestCheckWW_WarehouseType_ToContainerYard_WithInvalidUNDGStateCore(string startingWarehouseType)
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.WW_WarehouseType = startingWarehouseType;

			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			warehouse.WW_DGThresholdPercentage = 20;
			Helper.CreateWhsUNDGLimit(warehouse, "0004a");

			AssertEquals("Precondition", startingWarehouseType, warehouse.WW_WarehouseType);
			AssertNoWarnings("Precondition", warehouse.WW_WarehouseTypeInfo);

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;

			AssertEquals("Warehouse Type should not change.", startingWarehouseType, warehouse.WW_WarehouseType);
			AssertHasWarning(warehouse.WW_WarehouseTypeInfo, "Attempted to change to a Warehouse Type which does not support UNDG Thresholds. Disable Threshold Limits and clear Warning Percentage to change Warehouse Type.");

			warehouse.WW_WarehouseType = startingWarehouseType;
			AssertNoWarnings("Remove Warning on valid type change.", warehouse.WW_WarehouseTypeInfo);
		}

		#endregion

		#region TestCheckWW_IsDangerousGoodsManagementEnabled

		public void TestCheckWW_IsDangerousGoodsManagementEnabled()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;

			AssertNoWarnings("Precondition", warehouse.WW_IsDangerousGoodsManagementEnabledInfo);

			warehouse.WW_DGThresholdPercentage = 20;
			warehouse.WW_IsDangerousGoodsManagementEnabled = false;
			AssertHasWarning(warehouse.WW_IsDangerousGoodsManagementEnabledInfo, "If DG Limits are disabled then entered Dangerous Goods in the grid will have no limit applied.");

			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			warehouse.WW_DGThresholdPercentage = 0;
			AssertNoWarnings(warehouse.WW_IsDangerousGoodsManagementEnabledInfo);

			Helper.CreateWhsUNDGLimit(warehouse, "0004a");
			warehouse.WW_IsDangerousGoodsManagementEnabled = false;
			AssertHasWarning(warehouse.WW_IsDangerousGoodsManagementEnabledInfo, "If DG Limits are disabled then entered Dangerous Goods in the grid will have no limit applied.");
		}

		#endregion

		#region TestCheckWW_OA_WarehouseAddress

		public void TestCheckWW_OA_WarehouseAddress()
		{
			OrgHeader org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			org.CustomsCodes.RemoveAll();

			Whs.WW_OA_WarehouseAddress = org.MainAddress.PK;
			AssertNoWarning(Whs.WW_OA_WarehouseAddressInfo, WhsWarehouseValidation.NoControlledPremisesIDWarningMessage);

			Helper.EnableWarehouseForBond(Whs, true);
			Whs.Validation.ValidateWW_OA_WarehouseAddress();
			AssertHasWarning(Whs.WW_OA_WarehouseAddressInfo, WhsWarehouseValidation.NoControlledPremisesIDWarningMessage);
		}

		#endregion

		#region TestCheckWW_OA_WarehouseAddress_NotFinalisedOrderOrPick

		public void TestCheckWW_OA_WarehouseAddress_NotFinalisedOrderOrPick_CUS()
		{
			TestCheckWW_OA_WarehouseAddress_NotFinalisedOrderOrPickCore("CUS");
		}

		public void TestCheckWW_OA_WarehouseAddress_NotFinalisedOrderOrPick_CPS()
		{
			TestCheckWW_OA_WarehouseAddress_NotFinalisedOrderOrPickCore("CPS");
		}

		void TestCheckWW_OA_WarehouseAddress_NotFinalisedOrderOrPickCore(string subType)
		{
			var org = Helper.CreateClient("O1", "O1");
			var whs = Helper.CreateWarehouse("W1", "A");
			var whsFTZ = Helper.CreateFTZWarehouse("W2", "A", countrycode: Enterprise.Core.Constants.CountryCodes.UnitedStates);
			var ftzAddress = whs.WW_OA_WarehouseAddress;
			Factory.Save();

			var transactionsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var client = transactionsHelper.CreateClient("Client");
			var product = transactionsHelper.CreateProduct(client, "P1");

			// create unfinalised non-customs orders/picks, and finalised customs orders/picks
			CreatePickAndOrder(transactionsHelper, client, whs, "ORD", "O1", product.PK, finaliseOrder: true, finalisePick: true);
			CreatePickAndOrder(transactionsHelper, client, whs, subType, "O2", product.PK, finaliseOrder: false, finalisePick: false);
			CreatePickAndOrder(transactionsHelper, client, whs, "ORD", "O3", product.PK, finaliseOrder: false, finalisePick: false);
			CreatePickAndOrder(transactionsHelper, client, whsFTZ, subType, "O4", product.PK, finaliseOrder: true, finalisePick: true);
			CreatePickAndOrder(transactionsHelper, client, whsFTZ, "ORD", "O5", product.PK, finaliseOrder: false, finalisePick: false);

			using (PermitServiceTestHelper.MockServiceToGetPermits())
			{
				Factory.Save();
			}

			// preconditions
			AssertWarehouseAddressChangedHasNoErrors(whs, org.MainAddress.PK);
			AssertWarehouseAddressChangedHasNoErrors(whsFTZ, org.MainAddress.PK);

			// create unfinalised customs orders/picks
			CreatePickAndOrder(transactionsHelper, client, whs, subType, "O6", product.PK, finaliseOrder: false, finalisePick: false);
			CreatePickAndOrder(transactionsHelper, client, whsFTZ, subType, "O7", product.PK, finaliseOrder: false, finalisePick: false);

			using (PermitServiceTestHelper.MockServiceToGetPermits())
			{
				Factory.Save();
			}

			AssertWarehouseAddressChangedHasNoErrors(whs, ftzAddress);
			AssertWarehouseAddressChangedHasNoErrors(whsFTZ, ftzAddress);
			whsFTZ.WarehouseAddress.OA_RN_NKCountryCode = "US";
			AssertWarehouseAddressChangedHasError(whsFTZ, ftzAddress);

			org.MainAddress.OA_RN_NKCountryCode = "US";
			whsFTZ.WarehouseAddress.OA_RN_NKCountryCode = "AU";
			AssertWarehouseAddressChangedHasError(whsFTZ, ftzAddress);
		}

			void AssertWarehouseAddressChangedHasNoErrors(WhsWarehouse whs, ZGuid warehouseAddress)
		{
			whs.WW_OA_WarehouseAddress = warehouseAddress;
			AssertNoErrors(whs.WW_OA_WarehouseAddressInfo);
		}

		void AssertWarehouseAddressChangedHasError(WhsWarehouse whs, ZGuid warehouseAddress)
		{
			whs.WW_OA_WarehouseAddress = warehouseAddress;
			AssertHasError(whs.WW_OA_WarehouseAddressInfo, "Cannot change the Warehouse Address to/from a country that requires Permits (US, PR) because there are un-finalized Picks or Orders with no pick in the Warehouse.");
		}

		#endregion

		#region TestCheckWW_OA_WarehouseAddress_OnlyOneCYDPerOrgAddress

		public void TestCheckWW_OA_WarehouseAddress_OnlyOneCYDPerOrgAddress()
		{
			var company = GlbCompany.GetCurrentCompany(Factory);
			var whs1CYD = Helper.CreateCYDWarehouse("W1");

			whs1CYD.Validation.ValidateWW_OA_WarehouseAddress();
			AssertNoErrors(whs1CYD.WW_OA_WarehouseAddressInfo);

			var whs2CYD = Helper.CreateCYDWarehouse("W2");
			var branch2 = company.Branches.AddNew();
			whs2CYD.WW_GB_RelatedCompanyBranch = branch2.PK;
			whs2CYD.WW_OA_WarehouseAddress = whs1CYD.WW_OA_WarehouseAddress;
			whs2CYD.Validation.ValidateWW_OA_WarehouseAddress();

			AssertHasError(whs2CYD.WW_OA_WarehouseAddressInfo, "Address must be Unique per Container Yard Branch.");
		}

		public void TestCheckWW_OA_WarehouseAddress_IsUniquePerBranch_ForCYDType()
		{
			var company = GlbCompany.GetCurrentCompany(Factory);
			var whs1CYD = Helper.CreateCYDWarehouse("W1");
			var branch1 = company.Branches.AddNew();
			whs1CYD.WW_GB_RelatedCompanyBranch = branch1.PK;
			Factory.Save();

			whs1CYD.Validation.ValidateWW_OA_WarehouseAddress();
			AssertNoErrors(whs1CYD.WW_GB_RelatedCompanyBranchInfo);
			AssertNoErrors(whs1CYD.WW_OA_WarehouseAddressInfo);

			var whs2CYD = Helper.CreateCYDWarehouse("W2");
			whs2CYD.WW_GB_RelatedCompanyBranch = branch1.PK;

			whs2CYD.Validation.ValidateWW_OA_WarehouseAddress();
			AssertHasError(whs2CYD.WW_GB_RelatedCompanyBranchInfo, "Warehouse Branch must be Unique per Warehouse and Warehouse Type.");

			var whs3CYD = Helper.CreateCYDWarehouse("W3");
			var branch3 = company.Branches.AddNew();
			whs3CYD.WW_GB_RelatedCompanyBranch = branch3.PK;
			whs3CYD.WW_OA_WarehouseAddress = whs1CYD.WW_OA_WarehouseAddress;
			whs3CYD.Validation.ValidateWW_OA_WarehouseAddress();

			AssertNoErrors(whs1CYD.WW_GB_RelatedCompanyBranchInfo);
			AssertHasError(whs3CYD.WW_OA_WarehouseAddressInfo, "Address must be Unique per Container Yard Branch.");
		}
		#endregion

		#region TestCheckWW_WarehouseType_FTZ_NotFinalisedOrderOrPick

		public void TestCheckWW_WarehouseType_FTZ_NotFinalisedOrderOrPick_CUS()
		{
			TestCheckWW_WarehouseType_FTZ_NotFinalisedOrderOrPickCore("CUS");
		}

		public void TestCheckWW_WarehouseType_FTZ_NotFinalisedOrderOrPick_CPS()
		{
			TestCheckWW_WarehouseType_FTZ_NotFinalisedOrderOrPickCore("CPS");
		}

		void TestCheckWW_WarehouseType_FTZ_NotFinalisedOrderOrPickCore(string subType)
		{
			var whsFTZ = Helper.CreateFTZWarehouse("FTZ", "A");
			var transactionsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var client = transactionsHelper.CreateClient("Client");
			var product = transactionsHelper.CreateProduct(client, "P1");
			CreatePickAndOrder(transactionsHelper, client, whsFTZ, subType, "O8", product.PK, finaliseOrder: false, finalisePick: false);
			Factory.Save();

			whsFTZ.WarehouseAddress.OA_RN_NKCountryCode = "US";
			Factory.Save();

			AssertWarehouseTypeChangedHasError(whsFTZ, WarehouseTypes.Codes.Product, "Cannot change the Warehouse Type to/from FTZ in US because there are un-finalized Picks or Orders with no pick in the Warehouse.");
		}

		#endregion

		#region TestCheckWW_WarehouseType_FTZ_CancelOrder

		public void TestCheckWW_WarehouseType_FTZ_CancelOrder_CUS()
		{
			TestCheckWW_WarehouseType_FTZ_CancelOrderCore("CUS");
		}

		public void TestCheckWW_WarehouseType_FTZ_CancelOrder_CPS()
		{
			TestCheckWW_WarehouseType_FTZ_CancelOrderCore("CPS");
		}

		void TestCheckWW_WarehouseType_FTZ_CancelOrderCore(string orderType)
		{
			var whsFTZ = Helper.CreateFTZWarehouseInUS("FTZ", "A");
			var transactionsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var client = transactionsHelper.CreateClient("Client");
			var product = transactionsHelper.CreateProduct(client, "P1");
			var orderPK = transactionsHelper.CreateWhsOrder(client, whsFTZ.PK, "O1", Notify);
			transactionsHelper.CreateWhsOrderLine(orderPK, product.PK, 1m);
			transactionsHelper.SetOrderType(orderPK, orderType);
			Factory.Save();

			AssertWarehouseTypeChangedHasError(whsFTZ, WarehouseTypes.Codes.Product, "Cannot change the Warehouse Type to/from FTZ in US because there are un-finalized Picks or Orders with no pick in the Warehouse.");
			AssertWarehouseTypeChangedHasNoErrors(whsFTZ, WarehouseTypes.Codes.FreeTradeZone); // back to original value

			var order = Factory.Load<IWhsOrder>(orderPK);
			order.WD_DocketStatus = "CAN";
			Factory.Save();

			AssertWarehouseTypeChangedHasNoErrors(whsFTZ, WarehouseTypes.Codes.Product);
		}

		#endregion

		#region TestCheckWW_OC_DGContact

		public void TestCheckWW_OC_DGContact()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			AssertNoNotifications("Shouldn't be mandatory", warehouse.WW_OC_DGContactInfo);

			var contact1 = warehouse.WarehouseAddress.Header.Contacts.AddNew();
			var otherOrg = Helper.CreateClient("OTH");
			var contact2 = otherOrg.Contacts.AddNew();

			warehouse.WW_OC_DGContact = ZGuid.NewZGuid();
			AssertHasError(warehouse.WW_OC_DGContactInfo, "Enter a valid DG Contact.");

			warehouse.WW_OC_DGContact = contact2.PK;
			AssertHasError(warehouse.WW_OC_DGContactInfo, "Enter a valid DG Contact.");

			warehouse.WW_OC_DGContact = contact1.PK;
			AssertNoNotifications(warehouse.WW_OC_DGContactInfo);
		}

		#endregion

		#region TestCheckWW_DGContactPhoneType

		public void TestCheckWW_DGContactPhoneType()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.WW_DGContactPhoneType = "";
			AssertNoNotifications("Shouldn't be mandatory", warehouse.WW_DGContactPhoneTypeInfo);

			warehouse.WW_DGContactPhoneType = PhoneTypeList.Codes.HOM;
			AssertHasError(warehouse.WW_DGContactPhoneTypeInfo, "Enter a valid DG Contact.");
			warehouse.WW_DGContactPhoneType = "";

			var contact = warehouse.WarehouseAddress.Header.Contacts.AddNew();
			warehouse.WW_OC_DGContact = contact.PK;
			warehouse.Validation.ValidateWW_DGContactPhoneType();
			AssertHasError(warehouse.WW_DGContactPhoneTypeInfo, "Please enter a Phone Type.");

			warehouse.WW_DGContactPhoneType = PhoneTypeList.Codes.HOM;
			AssertNoNotifications(warehouse.WW_DGContactPhoneTypeInfo);
		}

		#endregion

		#region TestCheckWW_IsActive

		public void TestCheckWW_IsActive()
		{
			var whs1 = Helper.CreateWarehouse("1", "A", 1, 1);
			var whs2 = Helper.CreateWarehouse("2", "A", 1, 1);
			whs1.WW_IsActive = false;
			AssertNoErrors("Should not have error because no stock", whs1.WW_IsActiveInfo);

			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var clientPK = helper.CreateClient("1");
			var part = helper.CreateProduct(clientPK, "1");
			helper.CreateStock(whs2.PK, clientPK, "Whs2", part.PK, 10m);
			Factory.Save();
			whs1.Validation.ValidateWW_IsActive();
			AssertNoErrors("Should not have error because stock is only in Whs2", whs1.WW_IsActiveInfo);

			whs1.WW_IsActive = true;
			helper.CreateStock(whs1.PK, clientPK, "Whs1", part.PK, 9m);
			Factory.Save();
			whs1.WW_IsActive = false;
			AssertHasError("Should have error because stock is now also in Whs1", whs1.WW_IsActiveInfo, WhsWarehouseValidation.CannotDeactivateAWarehouseWithSOHError);

			whs1.WW_IsActive = true;
			var adjustment = helper.CreateWhsAdjustment(clientPK, whs1.PK, "1", Notify);
			helper.CreateWhsAdjustmentLine(adjustment.PK, part.PK, -9m, whs1.FindLocation("A").PK);
			helper.FinaliseDocket(adjustment.PK);
			Factory.Save();
			whs1.WW_IsActive = false;
			AssertNoError("Should not have error because stock has been adjusted out", whs1.WW_IsActiveInfo, WhsWarehouseValidation.CannotDeactivateAWarehouseWithSOHError);
		}

		#endregion

		#region TestCheckWW_IsActive_Staged

		public void TestCheckWW_IsActive_Staged()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var clientPk = helper.CreateClient("ORG1");
			var warehouse = Helper.CreateWarehouse("1", "A");
			var product = helper.CreateProduct(clientPk, "P1");
			warehouse.WW_IsActive = true;
			AssertNoError("Should be no errors if warehouse is active.", warehouse.WW_IsActiveInfo, WhsWarehouseValidation.CannotDeactivateAWarehouseWithSOHError);
			warehouse.WW_IsActive = false;
			AssertNoError("Should be no errors if warehouse is not in database.", warehouse.WW_IsActiveInfo, WhsWarehouseValidation.CannotDeactivateAWarehouseWithSOHError);
			warehouse.WW_IsActive = true;
			helper.CreateStock(warehouse.PK, clientPk, product.PK, 10m);
			Factory.Save();
			warehouse.WW_IsActive = false;
			AssertHasError("Should have this error if there are stock on hand.", warehouse.WW_IsActiveInfo, WhsWarehouseValidation.CannotDeactivateAWarehouseWithSOHError);

			warehouse.WW_IsActive = true;
			var orderPk = helper.CreateWhsOrder(clientPk, warehouse.PK, "O1", Notify);
			helper.CreateWhsOrderLine(orderPk, product.PK, 10m);
			var pickPk = helper.CreateWhsPick(new[] { orderPk });
			var pickLine = helper.GetPickLines(pickPk).Single();
			var transferLine = helper.PickAndMakeInTransitTransfer(pickLine, ZDateTime.Today);
			helper.FinaliseDocketLine(transferLine.PK);
			AssertEquals("Precondition: Inventory is Staged.", "STA", transferLine.WE_CurrentInventoryStatus);
			Factory.Save();

			warehouse.WW_IsActive = false;
			AssertHasError("Should have this error if stock is Staged.", warehouse.WW_IsActiveInfo, WhsWarehouseValidation.CannotDeactivateAWarehouseWithSOHError);

			warehouse.WW_IsActive = true;
			helper.FinaliseDocket(orderPk);
			helper.FinalisePick(pickPk);
			Factory.Save();

			warehouse.WW_IsActive = false;
			AssertNoError("Should not have this error if the stock is no longer staged (i.e. all picked).", warehouse.WW_IsActiveInfo, WhsWarehouseValidation.CannotDeactivateAWarehouseWithSOHError);
		}

		#endregion

		#region TestCheckWW_IsActive_CannotBeChangedIfPickedUnfinalisedOrderdExists

		public void TestCheckWW_IsActive_CannotBeChangedIfPickedUnfinalisedOrderdExists()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var clientPk = helper.CreateClient("ORG1");
			var warehouse = Helper.CreateWarehouse("1", "A");
			var product = helper.CreateProduct(clientPk, "P1");
			warehouse.WW_IsActive = true;
			AssertNoError("Should be no errors if warehouse is active.", warehouse.WW_IsActiveInfo, WhsWarehouseValidation.CannotDeactivateAWarehouseWithSOHError);
			warehouse.WW_IsActive = false;
			AssertNoError("Should be no errors if warehouse is not in database.", warehouse.WW_IsActiveInfo, WhsWarehouseValidation.CannotDeactivateAWarehouseWithSOHError);
			warehouse.WW_IsActive = true;
			helper.CreateStock(warehouse.PK, clientPk, product.PK, 10m);
			Factory.Save();
			warehouse.WW_IsActive = false;
			AssertHasError("Should have this error if there are stock on hand.", warehouse.WW_IsActiveInfo, WhsWarehouseValidation.CannotDeactivateAWarehouseWithSOHError);

			warehouse.WW_IsActive = true;
			var orderPk = helper.CreateWhsOrder(clientPk, warehouse.PK, "O1", Notify);
			helper.CreateWhsOrderLine(orderPk, product.PK, 10m);
			var pickPk = helper.CreateWhsPick(new[] { orderPk });
			var pickLine = helper.GetPickLines(pickPk).Single();
			helper.PickAndMakeInTransitTransfer(pickLine, ZDateTime.Today);
			Factory.Save();
			warehouse.WW_IsActive = false;
			AssertHasError("Should have this error if stock is in transit.", warehouse.WW_IsActiveInfo, WhsWarehouseValidation.CannotDeactivateAWarehouseWithSOHError);

			warehouse.WW_IsActive = true;
			helper.FinaliseDocket(orderPk);
			helper.FinalisePick(pickPk);
			Factory.Save();
			warehouse.WW_IsActive = false;
			AssertNoError("Should not have this error if order is finalised and there is no stock on hand or in transit.", warehouse.WW_IsActiveInfo, WhsWarehouseValidation.CannotDeactivateAWarehouseWithSOHError);
		}

		#endregion

		#region TestCheckWW_IsVirtualWarehouse

		public void TestCheckWW_IsVirtualWarehouse()
		{
			var whs1 = Helper.CreateWarehouse("1", "A", 1, 1);

			whs1.WW_IsVirtualWarehouse = true;
			AssertNoErrors(whs1.WW_IsVirtualWarehouseInfo);

			whs1.WW_IsVirtualWarehouse = false;
			AssertNoErrors(whs1.WW_IsVirtualWarehouseInfo);
		}

		#endregion

		#region TestCheckWW_IsVirtualWarehouse_WithInwardProcessingLocations

		public void TestCheckWW_IsVirtualWarehouse_WithInwardProcessingLocations()
			=> TestCheckWW_IsVirtualWarehouse_WithInwardProcessingLocations(isInTheDatabase: false);

		public void TestCheckWW_IsVirtualWarehouse_WithInwardProcessingLocations_Saved()
			=> TestCheckWW_IsVirtualWarehouse_WithInwardProcessingLocations(isInTheDatabase: true);

		void TestCheckWW_IsVirtualWarehouse_WithInwardProcessingLocations(bool isInTheDatabase)
		{
			var warehouse = Helper.CreateWarehouse("1", "A", 1, 1);
			warehouse.WW_IsVirtualWarehouse = true;

			var inwardProcessingArea = Helper.CreateArea(warehouse, "IPR", AreaTypes.Codes.InwardProcessing);

			if (isInTheDatabase)
			{
				Factory.Save();
			}

			warehouse.WW_IsVirtualWarehouse = false;
			AssertHasError(warehouse.WW_IsVirtualWarehouseInfo, "Warehouses with Inward Processing Areas must be marked as Virtual.");

			warehouse.WW_IsVirtualWarehouse = true;
			AssertNoErrors(warehouse.WW_IsVirtualWarehouseInfo);

			inwardProcessingArea.WA_AreaType = AreaTypes.Codes.FreeStore;
			warehouse.WW_IsVirtualWarehouse = false;
			AssertNoErrors(warehouse.WW_IsVirtualWarehouseInfo);
		}

		#endregion

		#region TestCheckWW_GB_RelatedCompanyBranch

		public void TestCheckWW_GB_RelatedCompanyBranch()
		{
			var branch = Factory.New<GlbBranch>();

			Whs.WW_GB_RelatedCompanyBranch = branch.PK;
			AssertHasErrors(Whs.WW_GB_RelatedCompanyBranchInfo);

			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			Whs.Validation.ValidateWW_GB_RelatedCompanyBranch();
			AssertNoErrors(Whs.WW_GB_RelatedCompanyBranchInfo);

			branch.GB_IsActive = false;
			Whs.Validation.ValidateWW_GB_RelatedCompanyBranch();
			AssertHasErrors(Whs.WW_GB_RelatedCompanyBranchInfo);
		}

		#endregion

		#region TestCheckWW_GB_RelatedCompanyBranch_IsUniquePerWarehouseType

		public void TestCheckWW_GB_RelatedCompanyBranch_IsUniquePerWarehouseType()
		{
			var company = GlbCompany.GetCurrentCompany(Factory);
			var branch1 = company.Branches.AddNew();
			var warehouse1 = Factory.New<WhsWarehouse>();
			warehouse1.WW_GB_RelatedCompanyBranch = branch1.PK;
			AssertNoErrors(warehouse1.WW_GB_RelatedCompanyBranchInfo);

			var branch2 = company.Branches.AddNew();
			var warehouse2 = Factory.New<WhsWarehouse>();
			warehouse2.WW_GB_RelatedCompanyBranch = branch2.PK;
			AssertNoErrors(warehouse2.WW_GB_RelatedCompanyBranchInfo);

			warehouse2.WW_GB_RelatedCompanyBranch = branch1.PK;
			AssertHasError(warehouse2.WW_GB_RelatedCompanyBranchInfo, "Warehouse Branch must be Unique per Warehouse and Warehouse Type.");
			AssertHasError(warehouse2.WW_WarehouseTypeInfo, "Warehouse Branch must be Unique per Warehouse and Warehouse Type.");

			warehouse2.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			AssertNoErrors(warehouse2.WW_GB_RelatedCompanyBranchInfo);
			AssertNoErrors(warehouse2.WW_WarehouseTypeInfo);

			warehouse2.WW_WarehouseType = WarehouseTypes.Codes.Product;
			AssertHasError(warehouse2.WW_GB_RelatedCompanyBranchInfo, "Warehouse Branch must be Unique per Warehouse and Warehouse Type.");
			AssertHasError(warehouse2.WW_WarehouseTypeInfo, "Warehouse Branch must be Unique per Warehouse and Warehouse Type.");

			warehouse2.WW_IsActive = false;
			AssertNoErrors(warehouse2.WW_GB_RelatedCompanyBranchInfo);
			AssertNoErrors(warehouse2.WW_WarehouseTypeInfo);

			warehouse2.WW_IsActive = true;
			AssertHasError(warehouse2.WW_GB_RelatedCompanyBranchInfo, "Warehouse Branch must be Unique per Warehouse and Warehouse Type.");
			AssertHasError(warehouse2.WW_WarehouseTypeInfo, "Warehouse Branch must be Unique per Warehouse and Warehouse Type.");

			warehouse2.WW_GB_RelatedCompanyBranch = branch2.PK;
			AssertNoErrors(warehouse2.WW_GB_RelatedCompanyBranchInfo);
			AssertNoErrors(warehouse2.WW_WarehouseTypeInfo);

			warehouse2.WW_GB_RelatedCompanyBranch = branch1.PK;
			AssertHasError(warehouse2.WW_GB_RelatedCompanyBranchInfo, "Warehouse Branch must be Unique per Warehouse and Warehouse Type.");
			AssertHasError(warehouse2.WW_WarehouseTypeInfo, "Warehouse Branch must be Unique per Warehouse and Warehouse Type.");

			warehouse2.WW_IsVirtualWarehouse = true;
			AssertNoErrors(warehouse2.WW_GB_RelatedCompanyBranchInfo);
			AssertNoErrors(warehouse2.WW_WarehouseTypeInfo);
		}

		#endregion

		#region TestCheckWW_GG_ReleaseGroup

		public void TestCheckWW_GG_ReleaseGroup()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			var inactiveReleaseGroup = Helper.CreateReleaseGroup("RG2", "RG2");
			inactiveReleaseGroup.GG_IsActive = false;

			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			AssertNoErrors("Precondition.", warehouse.WW_GG_ReleaseGroupInfo);

			warehouse.WW_GG_ReleaseGroup = releaseGroup.PK;
			AssertNoErrors("Precondition.", warehouse.WW_GG_ReleaseGroupInfo);

			warehouse.WW_GG_ReleaseGroup = inactiveReleaseGroup.PK;
			AssertHasError("Should have an error.", warehouse.WW_GG_ReleaseGroupInfo, "This Release Group is inactive - it may not be used.");

			warehouse.WW_GG_ReleaseGroup = releaseGroup.PK;
			AssertNoErrors("Should have cleared the error.", warehouse.WW_GG_ReleaseGroupInfo);
		}

		public void TestCheckWW_GG_ReleaseGroup_IsNonProductWarehouse()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			var inactiveReleaseGroup = Helper.CreateReleaseGroup("RG2", "RG2");
			inactiveReleaseGroup.GG_IsActive = false;

			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			warehouse.WW_GG_ReleaseGroup = releaseGroup.PK;
			AssertHasError("Should have an error.", warehouse.WW_GG_ReleaseGroupInfo, "This type of Warehouse does not support Task Management.");

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			warehouse.WW_GG_ReleaseGroup = releaseGroup.PK;
			AssertNoErrors("Should have cleared the error.", warehouse.WW_GG_ReleaseGroupInfo);
		}

		public void TestCheckWW_GG_ReleaseGroup_IsVirtualWarehouse()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			var inactiveReleaseGroup = Helper.CreateReleaseGroup("RG2", "RG2");
			inactiveReleaseGroup.GG_IsActive = false;

			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			warehouse.WW_GG_ReleaseGroup = releaseGroup.PK;
			warehouse.WW_IsVirtualWarehouse = true;
			AssertHasError("Should have an error.", warehouse.WW_GG_ReleaseGroupInfo, "Virtual Warehouses do not support Task Management.");

			warehouse.WW_IsVirtualWarehouse = false;
			AssertNoErrors("Should have cleared the error.", warehouse.WW_GG_ReleaseGroupInfo);
		}

		public void TestCheckWW_GG_ReleaseGroup_CannotChangeWithReadyForPlanningStatusJob_WhsDocket()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var releaseGroup1 = Helper.CreateReleaseGroup("RG1", "RG1");
			var releaseGroup2 = Helper.CreateReleaseGroup("RG2", "RG2");

			var warehouse = data.Whs1;
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			warehouse.WW_GG_ReleaseGroup = releaseGroup2.PK;
			AssertNoErrors("Precondition.", warehouse.WW_GG_ReleaseGroupInfo);

			warehouse.WW_GG_ReleaseGroup = releaseGroup1.PK;
			AssertNoErrors("Precondition.", warehouse.WW_GG_ReleaseGroupInfo);

			var receivePK = iHelper.CreateWhsReceive(data.Org1.PK, warehouse.PK, "R1", Notify);
			iHelper.CreateWhsReceiveInventoryLine(receivePK, data.Part1.PK, 10m, warehouse.DefaultInboundDockDoorLocation.PK);
			Factory.Save();

			var receive = Factory.Load<IWhsReceive>(receivePK);
			Factory.Save();

			receive.WD_TaskPlanningStatus = "RFP";
			Factory.Save();

			warehouse.WW_GG_ReleaseGroup = releaseGroup2.PK;
			AssertHasError("Should have an error.", warehouse.WW_GG_ReleaseGroupInfo, "Cannot change the Release Group as the Warehouse has Jobs currently using Task Management.");
		}

		public void TestCheckWW_GG_ReleaseGroup_CannotChangeWithPlannedStatusJob_WhsDocket()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var releaseGroup1 = Helper.CreateReleaseGroup("RG1", "RG1");
			var releaseGroup2 = Helper.CreateReleaseGroup("RG2", "RG2");

			var warehouse = data.Whs1;
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			warehouse.WW_GG_ReleaseGroup = releaseGroup2.PK;
			AssertNoErrors("Precondition.", warehouse.WW_GG_ReleaseGroupInfo);

			warehouse.WW_GG_ReleaseGroup = releaseGroup1.PK;
			AssertNoErrors("Precondition.", warehouse.WW_GG_ReleaseGroupInfo);

			var receivePK = iHelper.CreateWhsReceive(data.Org1.PK, warehouse.PK, "R1", Notify);
			iHelper.CreateWhsReceiveInventoryLine(receivePK, data.Part1.PK, 10m, warehouse.DefaultInboundDockDoorLocation.PK);
			Factory.Save();

			var receive = Factory.Load<IWhsReceive>(receivePK);
			Factory.Save();

			receive.WD_TaskPlanningStatus = "PLA";
			Factory.Save();

			warehouse.WW_GG_ReleaseGroup = releaseGroup2.PK;
			AssertHasError("Should have an error.", warehouse.WW_GG_ReleaseGroupInfo, "Cannot change the Release Group as the Warehouse has Jobs currently using Task Management.");
		}

		public void TestCheckWW_GG_ReleaseGroup_CannotChangeWithReadyForPlanningStatusJob_WhsPick()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var releaseGroup1 = Helper.CreateReleaseGroup("RG1", "RG1");
			var releaseGroup2 = Helper.CreateReleaseGroup("RG2", "RG2");

			var warehouse = data.Whs1;
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			warehouse.WW_GG_ReleaseGroup = releaseGroup2.PK;
			AssertNoErrors("Precondition.", warehouse.WW_GG_ReleaseGroupInfo);

			warehouse.WW_GG_ReleaseGroup = releaseGroup1.PK;
			AssertNoErrors("Precondition.", warehouse.WW_GG_ReleaseGroupInfo);
			Factory.Save();

			var order = iHelper.CreateWhsOrder(data.Org1.PK, warehouse.PK, "O1", Notify);
			var orderLine = iHelper.CreateWhsOrderLine(order, data.Part1.PK, 10m);

			var pickPK = iHelper.CreateWhsPick(new[] { order });
			var pick = Factory.Load<IWhsPick>(pickPK);
			Factory.Save();

			pick.WP_TaskPlanningStatus = "RFP";
			Factory.Save();

			warehouse.WW_GG_ReleaseGroup = releaseGroup2.PK;
			AssertHasError("Should have an error.", warehouse.WW_GG_ReleaseGroupInfo, "Cannot change the Release Group as the Warehouse has Jobs currently using Task Management.");
		}

		public void TestCheckWW_GG_ReleaseGroup_CannotChangeWithPlannedStatusJob_WhsPick()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var releaseGroup1 = Helper.CreateReleaseGroup("RG1", "RG1");
			var releaseGroup2 = Helper.CreateReleaseGroup("RG2", "RG2");

			var warehouse = data.Whs1;
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			warehouse.WW_GG_ReleaseGroup = releaseGroup2.PK;
			AssertNoErrors("Precondition.", warehouse.WW_GG_ReleaseGroupInfo);

			warehouse.WW_GG_ReleaseGroup = releaseGroup1.PK;
			AssertNoErrors("Precondition.", warehouse.WW_GG_ReleaseGroupInfo);
			Factory.Save();

			var order = iHelper.CreateWhsOrder(data.Org1.PK, warehouse.PK, "O1", Notify);
			var orderLine = iHelper.CreateWhsOrderLine(order, data.Part1.PK, 10m);

			var pickPK = iHelper.CreateWhsPick(new[] { order });
			var pick = Factory.Load<IWhsPick>(pickPK);
			Factory.Save();

			pick.WP_TaskPlanningStatus = "PLA";
			Factory.Save();

			warehouse.WW_GG_ReleaseGroup = releaseGroup2.PK;
			AssertHasError("Should have an error.", warehouse.WW_GG_ReleaseGroupInfo, "Cannot change the Release Group as the Warehouse has Jobs currently using Task Management.");
		}

		public void TestCheckWW_GG_ReleaseGroup_CannotChangeWithReadyForPlanningStatusJob_WhsCycleCountLocation()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var releaseGroup1 = Helper.CreateReleaseGroup("RG1", "RG1");
			var releaseGroup2 = Helper.CreateReleaseGroup("RG2", "RG2");

			var warehouse = data.Whs1;
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			warehouse.WW_GG_ReleaseGroup = releaseGroup2.PK;
			AssertNoErrors("Precondition.", warehouse.WW_GG_ReleaseGroupInfo);

			warehouse.WW_GG_ReleaseGroup = releaseGroup1.PK;
			AssertNoErrors("Precondition.", warehouse.WW_GG_ReleaseGroupInfo);
			Factory.Save();

			var cycleCountLocationPK = iHelper.CreateWhsCycleCountLocation(warehouse.Rows[0].Locations[0].PK, "PWA", "RFP");
			Factory.Save();

			warehouse.WW_GG_ReleaseGroup = releaseGroup2.PK;
			AssertHasError("Should have an error.", warehouse.WW_GG_ReleaseGroupInfo, "Cannot change the Release Group as the Warehouse has Jobs currently using Task Management.");
		}

		public void TestCheckWW_GG_ReleaseGroup_CannotChangeWithPlannedStatusJob_WhsCycleCountLocation()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var releaseGroup1 = Helper.CreateReleaseGroup("RG1", "RG1");
			var releaseGroup2 = Helper.CreateReleaseGroup("RG2", "RG2");

			var warehouse = data.Whs1;
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			warehouse.WW_GG_ReleaseGroup = releaseGroup2.PK;
			AssertNoErrors("Precondition.", warehouse.WW_GG_ReleaseGroupInfo);

			warehouse.WW_GG_ReleaseGroup = releaseGroup1.PK;
			AssertNoErrors("Precondition.", warehouse.WW_GG_ReleaseGroupInfo);
			Factory.Save();

			var cycleCountLocationPK = iHelper.CreateWhsCycleCountLocation(warehouse.Rows[0].Locations[0].PK, "PWA", "PLA");
			Factory.Save();

			warehouse.WW_GG_ReleaseGroup = releaseGroup2.PK;
			AssertHasError("Should have an error.", warehouse.WW_GG_ReleaseGroupInfo, "Cannot change the Release Group as the Warehouse has Jobs currently using Task Management.");
		}

		public void TestCheckWW_GG_ReleaseGroup_CannotChangeWithReadyForPlanningStatusJob_WhsLoad()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var releaseGroup1 = Helper.CreateReleaseGroup("RG1", "RG1");
			var releaseGroup2 = Helper.CreateReleaseGroup("RG2", "RG2");

			var warehouse = data.Whs1;
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			warehouse.WW_GG_ReleaseGroup = releaseGroup2.PK;
			AssertNoErrors("Precondition.", warehouse.WW_GG_ReleaseGroupInfo);

			warehouse.WW_GG_ReleaseGroup = releaseGroup1.PK;
			AssertNoErrors("Precondition.", warehouse.WW_GG_ReleaseGroupInfo);
			Factory.Save();

			var loadPK = iHelper.CreateWhsLoad(data.Org1.PK, data.Whs1.DefaultInboundDockDoorLocation.PK, "RFP");
			Factory.Save();

			warehouse.WW_GG_ReleaseGroup = releaseGroup2.PK;
			AssertHasError("Should have an error.", warehouse.WW_GG_ReleaseGroupInfo, "Cannot change the Release Group as the Warehouse has Jobs currently using Task Management.");
		}

		public void TestCheckWW_GG_ReleaseGroup_CannotChangeWithPlannedStatusJob_WhsLoad()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 2, 1);
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var releaseGroup1 = Helper.CreateReleaseGroup("RG1", "RG1");
			var releaseGroup2 = Helper.CreateReleaseGroup("RG2", "RG2");

			var warehouse = data.Whs1;
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			warehouse.WW_GG_ReleaseGroup = releaseGroup2.PK;
			AssertNoErrors("Precondition.", warehouse.WW_GG_ReleaseGroupInfo);

			warehouse.WW_GG_ReleaseGroup = releaseGroup1.PK;
			AssertNoErrors("Precondition.", warehouse.WW_GG_ReleaseGroupInfo);
			Factory.Save();

			var loadPK = iHelper.CreateWhsLoad(data.Org1.PK, data.Whs1.DefaultInboundDockDoorLocation.PK, "PLA");
			Factory.Save();

			warehouse.WW_GG_ReleaseGroup = releaseGroup2.PK;
			AssertHasError("Should have an error.", warehouse.WW_GG_ReleaseGroupInfo, "Cannot change the Release Group as the Warehouse has Jobs currently using Task Management.");
		}

		#endregion

		#region TestCheckWW_DefaultInboundDockDoor

		public void TestCheckWW_DefaultInboundDockDoor()
		{
			var warehouse = Helper.CreateWarehouse("W1", "A", 1, 1, shouldPreGenerateDDL: false);
			AssertEquals(ZGuid.Empty, warehouse.WW_DefaultInboundDockDoor);
			AssertNoErrors(warehouse.WW_DefaultInboundDockDoorInfo);

			Factory.Save();
			Assert("Default DDL should be set during saving", !warehouse.WW_DefaultInboundDockDoor.IsEmpty);
			AssertNoErrors(warehouse.WW_DefaultInboundDockDoorInfo);

			warehouse.WW_DefaultInboundDockDoor = ZGuid.Empty;
			AssertHasError("There should be error if default dock door gets cleared.", warehouse.WW_DefaultInboundDockDoorInfo, WhsWarehouseValidation.DefaultDockDoorLocationMissingError(WhsLocationViewValidation.Inbound));

			warehouse.WW_IsActive = false;
			warehouse.Validation.ValidateWW_DefaultInboundDockDoor();
			AssertNoErrors("For inactive warehouses we don't care about default dock door.", warehouse.WW_DefaultInboundDockDoorInfo);

			warehouse.WW_IsActive = true;
			warehouse.WW_IsVirtualWarehouse = true;
			warehouse.Validation.ValidateWW_DefaultInboundDockDoor();
			AssertNoErrors("For virtual warehouses we don't care about default dock door.", warehouse.WW_DefaultInboundDockDoorInfo);

			warehouse.WW_IsVirtualWarehouse = false;
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			warehouse.Validation.ValidateWW_DefaultInboundDockDoor();
			AssertNoErrors("Should be no error for transit warehouse.", warehouse.WW_DefaultInboundDockDoorInfo);

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			warehouse.Validation.ValidateWW_DefaultInboundDockDoor();
			AssertHasError("There should be error if default dock door gets cleared.", warehouse.WW_DefaultInboundDockDoorInfo, WhsWarehouseValidation.DefaultDockDoorLocationMissingError(WhsLocationViewValidation.Inbound));

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			warehouse.Validation.ValidateWW_DefaultInboundDockDoor();
			AssertHasError("There should be error if default dock door gets cleared.", warehouse.WW_DefaultInboundDockDoorInfo, WhsWarehouseValidation.DefaultDockDoorLocationMissingError(WhsLocationViewValidation.Inbound));

			warehouse.WW_DefaultInboundDockDoor = ZGuid.NewZGuid();
			warehouse.Validation.ValidateWW_DefaultInboundDockDoor();
			AssertHasError("There should be error if WW_DefaultInboundDockDoor is not pointing to a valid location.", warehouse.WW_DefaultInboundDockDoorInfo, WhsWarehouseValidation.DefaultDockDoorLocationMissingError(WhsLocationViewValidation.Inbound));
		}

		public void TestCheckWW_DefaultInboundDockDoor_WrongLocationStatus()
		{
			var warehouse = Helper.CreateWarehouse("W1", "A", 1, 1);

			Factory.Save();
			Assert("Precondition: Default DDL should be set during saving", !warehouse.WW_DefaultInboundDockDoor.IsEmpty);
			AssertNoErrors(warehouse.WW_DefaultInboundDockDoorInfo);

			// now let's point it to some dockdoor location with a different location status to normal.
			var locationA1 = warehouse.FindLocation("A");
			locationA1.WLV_WLT_LocationType = warehouse.DefaultInboundDockDoorLocation.WLV_WLT_LocationType;

			foreach (var locationStatus in new LocationStatus().ToArray().Select(l => l.Code).Where(c => c != LocationStatus.Codes.Normal))
			{
				locationA1.WLV_LocationStatus = locationStatus;
				warehouse.WW_DefaultInboundDockDoor = locationA1.PK;
				AssertHasError("There should be error because location does not have a Status of Normal.", warehouse.WW_DefaultInboundDockDoorInfo, "Default inbound Dock Door Location should have a Location Status of NOR.");

				locationA1.WLV_LocationStatus = LocationStatus.Codes.Normal;
				warehouse.Validation.ValidateWW_DefaultInboundDockDoor();
				AssertNoErrors(warehouse.WW_DefaultInboundDockDoorInfo);
			}
		}

		public void TestCheckWW_DefaultInboundDockDoor_WrongLocationType()
		{
			var warehouse = Helper.CreateWarehouse("W1", "A", 1, 1, shouldPreGenerateDDL: false);
			AssertEquals(ZGuid.Empty, warehouse.WW_DefaultInboundDockDoor);
			AssertNoErrors(warehouse.WW_DefaultInboundDockDoorInfo);

			Factory.Save();
			Assert("Default DDL should be set during saving", !warehouse.WW_DefaultInboundDockDoor.IsEmpty);
			AssertNoErrors(warehouse.WW_DefaultInboundDockDoorInfo);

			// now let's point it to some non-dockdoor location
			var locationA1 = warehouse.FindLocation("A");
			warehouse.WW_DefaultInboundDockDoor = locationA1.PK;
			warehouse.Validation.ValidateWW_DefaultInboundDockDoor();
			AssertHasError("There should be error because location is not dock door.", warehouse.WW_DefaultInboundDockDoorInfo, WhsWarehouseValidation.DefaultDockDoorLocationWrongTypeError(WhsLocationViewValidation.Inbound));

			var ddlLocationType = Helper.CreateLocationType("123", "DOC Test", false, 0, LocationClasses.Codes.DDL);
			locationA1.WLV_WLT_LocationType = ddlLocationType.PK;
			Assert("Precondition:", locationA1.IsDockDoorLocation);

			warehouse.Validation.ValidateWW_DefaultInboundDockDoor();
			AssertNoErrors("Should be no error as now default DDL has correct type.", warehouse.WW_DefaultInboundDockDoorInfo);
		}

		public void TestCheckWW_DefaultInboundDockDoor_WrongWarehouse()
		{
			var whs1 = Helper.CreateWarehouse("W1", "A", 1, 1, shouldPreGenerateDDL: false);
			var whs2 = Helper.CreateWarehouse("W2", "B", 1, 1, shouldPreGenerateDDL: false);

			Factory.Save();

			Assert("Default DDL should be set during saving", !whs1.WW_DefaultInboundDockDoor.IsEmpty);
			Assert("Default DDL should be set during saving", !whs2.WW_DefaultInboundDockDoor.IsEmpty);
			AssertNoErrors(whs1.WW_DefaultInboundDockDoorInfo);
			AssertNoErrors(whs2.WW_DefaultInboundDockDoorInfo);

			var ddlLocationType = Helper.CreateLocationType("123", "DOC Test", false, 0, LocationClasses.Codes.DDL);
			var locationA1 = whs1.FindLocation("A");
			locationA1.WLV_WLT_LocationType = ddlLocationType.PK;
			var locationB1 = whs2.FindLocation("B");
			locationB1.WLV_WLT_LocationType = ddlLocationType.PK;

			whs1.WW_DefaultInboundDockDoor = locationB1.PK;
			AssertHasError(whs1.WW_DefaultInboundDockDoorInfo, WhsWarehouseValidation.DefaultDockDoorLocationWrongWarehouse(WhsLocationViewValidation.Inbound));

			whs1.WW_DefaultInboundDockDoor = locationA1.PK;
			AssertNoErrors(whs1.WW_DefaultInboundDockDoorInfo);
		}

		#endregion

		#region TestCheckWW_DefaultOutboundDockDoor

		public void TestCheckWW_DefaultOutboundDockDoor()
		{
			var warehouse = Helper.CreateWarehouse("W1", "A", 1, 1, shouldPreGenerateDDL: false);
			AssertEquals(ZGuid.Empty, warehouse.WW_DefaultOutboundDockDoor);
			AssertNoErrors(warehouse.WW_DefaultOutboundDockDoorInfo);

			Factory.Save();
			Assert("Default DDL should be set during saving", !warehouse.WW_DefaultOutboundDockDoor.IsEmpty);
			AssertNoErrors(warehouse.WW_DefaultOutboundDockDoorInfo);

			warehouse.WW_DefaultOutboundDockDoor = ZGuid.Empty;
			AssertHasError("There should be error if default dock door gets cleared.", warehouse.WW_DefaultOutboundDockDoorInfo, WhsWarehouseValidation.DefaultDockDoorLocationMissingError(WhsLocationViewValidation.Outbound));

			warehouse.WW_IsActive = false;
			warehouse.Validation.ValidateWW_DefaultOutboundDockDoor();
			AssertNoErrors("For inactive warehouses we don't care about default dock door.", warehouse.WW_DefaultOutboundDockDoorInfo);

			warehouse.WW_IsActive = true;
			warehouse.WW_IsVirtualWarehouse = true;
			warehouse.Validation.ValidateWW_DefaultOutboundDockDoor();
			AssertNoErrors("For virtual warehouses we don't care about default dock door.", warehouse.WW_DefaultOutboundDockDoorInfo);

			warehouse.WW_IsVirtualWarehouse = false;
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			warehouse.Validation.ValidateWW_DefaultOutboundDockDoor();
			AssertNoErrors("Should be no error for transit warehouse.", warehouse.WW_DefaultOutboundDockDoorInfo);

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			warehouse.Validation.ValidateWW_DefaultOutboundDockDoor();
			AssertHasError("There should be error if default dock door gets cleared.", warehouse.WW_DefaultOutboundDockDoorInfo, WhsWarehouseValidation.DefaultDockDoorLocationMissingError(WhsLocationViewValidation.Outbound));

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			warehouse.Validation.ValidateWW_DefaultOutboundDockDoor();
			AssertHasError("There should be error if default dock door gets cleared.", warehouse.WW_DefaultOutboundDockDoorInfo, WhsWarehouseValidation.DefaultDockDoorLocationMissingError(WhsLocationViewValidation.Outbound));

			warehouse.WW_DefaultOutboundDockDoor = ZGuid.NewZGuid();
			warehouse.Validation.ValidateWW_DefaultOutboundDockDoor();
			AssertHasError("There should be error if WW_DefaultOutboundDockDoor is not pointing to a valid location.", warehouse.WW_DefaultOutboundDockDoorInfo, WhsWarehouseValidation.DefaultDockDoorLocationMissingError(WhsLocationViewValidation.Outbound));
		}

		public void TestCheckWW_DefaultOutboundDockDoor_WrongLocationStatus()
		{
			var warehouse = Helper.CreateWarehouse("W1", "A", 1, 1);

			Factory.Save();
			Assert("Precondition: Default DDL should be set during saving", !warehouse.WW_DefaultOutboundDockDoor.IsEmpty);
			AssertNoErrors(warehouse.WW_DefaultOutboundDockDoorInfo);

			// now let's point it to some dockdoor location with a different location status to normal.
			var locationA1 = warehouse.FindLocation("A");
			locationA1.WLV_WLT_LocationType = warehouse.DefaultOutboundDockDoorLocation.WLV_WLT_LocationType;

			foreach (var locationStatus in new LocationStatus().ToArray().Select(l => l.Code).Where(c => c != LocationStatus.Codes.Normal))
			{
				locationA1.WLV_LocationStatus = locationStatus;
				warehouse.WW_DefaultOutboundDockDoor = locationA1.PK;
				AssertHasError("There should be error because location does not have a Status of Normal.", warehouse.WW_DefaultOutboundDockDoorInfo, "Default outbound Dock Door Location should have a Location Status of NOR.");

				locationA1.WLV_LocationStatus = LocationStatus.Codes.Normal;
				warehouse.Validation.ValidateWW_DefaultOutboundDockDoor();
				AssertNoErrors(warehouse.WW_DefaultOutboundDockDoorInfo);
			}
		}

		public void TestCheckWW_DefaultOutboundDockDoor_WrongLocationType()
		{
			var warehouse = Helper.CreateWarehouse("W1", "A", 1, 1, shouldPreGenerateDDL: false);
			AssertEquals(ZGuid.Empty, warehouse.WW_DefaultOutboundDockDoor);
			AssertNoErrors(warehouse.WW_DefaultOutboundDockDoorInfo);

			Factory.Save();
			Assert("Default DDL should be set during saving", !warehouse.WW_DefaultOutboundDockDoor.IsEmpty);
			AssertNoErrors(warehouse.WW_DefaultOutboundDockDoorInfo);

			// now let's point it to some non-dockdoor location
			var locationA1 = warehouse.FindLocation("A");
			warehouse.WW_DefaultOutboundDockDoor = locationA1.PK;
			warehouse.Validation.ValidateWW_DefaultOutboundDockDoor();
			AssertHasError("There should be error because location is not dock door.", warehouse.WW_DefaultOutboundDockDoorInfo, WhsWarehouseValidation.DefaultDockDoorLocationWrongTypeError(WhsLocationViewValidation.Outbound));

			var ddlLocationType = Helper.CreateLocationType("123", "DOC Test", false, 0, LocationClasses.Codes.DDL);
			locationA1.WLV_WLT_LocationType = ddlLocationType.PK;
			Assert("Precondition:", locationA1.IsDockDoorLocation);

			warehouse.Validation.ValidateWW_DefaultOutboundDockDoor();
			AssertNoErrors("Should be no error as now default DDL has correct type.", warehouse.WW_DefaultOutboundDockDoorInfo);
		}

		public void TestCheckWW_DefaultOutboundDockDoor_WrongWarehouse()
		{
			var whs1 = Helper.CreateWarehouse("W1", "A", 1, 1, shouldPreGenerateDDL: false);
			var whs2 = Helper.CreateWarehouse("W2", "B", 1, 1, shouldPreGenerateDDL: false);

			Factory.Save();

			Assert("Default DDL should be set during saving", !whs1.WW_DefaultOutboundDockDoor.IsEmpty);
			Assert("Default DDL should be set during saving", !whs2.WW_DefaultOutboundDockDoor.IsEmpty);
			AssertNoErrors(whs1.WW_DefaultOutboundDockDoorInfo);
			AssertNoErrors(whs2.WW_DefaultOutboundDockDoorInfo);

			var ddlLocationType = Helper.CreateLocationType("123", "DOC Test", false, 0, LocationClasses.Codes.DDL);
			var locationA1 = whs1.FindLocation("A");
			locationA1.WLV_WLT_LocationType = ddlLocationType.PK;
			var locationB1 = whs2.FindLocation("B");
			locationB1.WLV_WLT_LocationType = ddlLocationType.PK;

			whs1.WW_DefaultOutboundDockDoor = locationB1.PK;
			AssertHasError(whs1.WW_DefaultOutboundDockDoorInfo, WhsWarehouseValidation.DefaultDockDoorLocationWrongWarehouse(WhsLocationViewValidation.Outbound));

			whs1.WW_DefaultOutboundDockDoor = locationA1.PK;
			AssertNoErrors(whs1.WW_DefaultOutboundDockDoorInfo);
		}

		#endregion

		#region TestCheckWW_WLT_DefaultLocationType

		public void TestCheckWW_WLT_DefaultLocationType()
		{
			var expectedMessage = "This Location Type is not supported for this type of Warehouse.";

			var productWarehouse = Helper.CreateWarehouse("WHS", "A");
			var transitWarehouse = Helper.CreateTRWWarehouse("TRW", "A");
			var ftzWarehouse = Helper.CreateFTZWarehouse("FTZ", "A");
			var containerYardWarehouse = Helper.CreateWarehouse("CYD", "A");
			containerYardWarehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;

			var tclLocationType = Helper.CreateLocationType("TCL", LocationClasses.Codes.TCL);
			var pstLocationType = Helper.CreateLocationType("PST", "PST", false, 0, LocationClasses.Codes.PST);
			var fixLocationType = Helper.CreateLocationType("LC2", "LC2 Test", false, 1, LocationClasses.Codes.FIX);
			var conLocationType = Helper.CreateLocationType("CON", "CON", false, 0, LocationClasses.Codes.CON);

			AssertNoErrors("Precondition: location type has no errors", productWarehouse.WW_WLT_DefaultLocationTypeInfo);
			AssertNoErrors("Precondition: location type has no errors", transitWarehouse.WW_WLT_DefaultLocationTypeInfo);
			AssertNoErrors("Precondition: location type has no errors", ftzWarehouse.WW_WLT_DefaultLocationTypeInfo);
			AssertNoErrors("Precondition: location type has no errors", containerYardWarehouse.WW_WLT_DefaultLocationTypeInfo);
			Factory.Save();

			productWarehouse.WW_WLT_DefaultLocationType = tclLocationType.PK;
			ftzWarehouse.WW_WLT_DefaultLocationType = tclLocationType.PK;
			containerYardWarehouse.WW_WLT_DefaultLocationType = tclLocationType.PK;
			transitWarehouse.WW_WLT_DefaultLocationType = tclLocationType.PK;
			AssertHasError(productWarehouse.WW_WLT_DefaultLocationTypeInfo, expectedMessage);
			AssertHasError(ftzWarehouse.WW_WLT_DefaultLocationTypeInfo, expectedMessage);
			AssertNoError(containerYardWarehouse.WW_WLT_DefaultLocationTypeInfo, expectedMessage);
			AssertNoError(transitWarehouse.WW_WLT_DefaultLocationTypeInfo, expectedMessage);

			productWarehouse.WW_WLT_DefaultLocationType = pstLocationType.PK;
			ftzWarehouse.WW_WLT_DefaultLocationType = pstLocationType.PK;
			containerYardWarehouse.WW_WLT_DefaultLocationType = pstLocationType.PK;
			transitWarehouse.WW_WLT_DefaultLocationType = pstLocationType.PK;
			AssertNoError(productWarehouse.WW_WLT_DefaultLocationTypeInfo, expectedMessage);
			AssertNoError(ftzWarehouse.WW_WLT_DefaultLocationTypeInfo, expectedMessage);
			AssertHasError(containerYardWarehouse.WW_WLT_DefaultLocationTypeInfo, expectedMessage);
			AssertHasError(transitWarehouse.WW_WLT_DefaultLocationTypeInfo, expectedMessage);

			productWarehouse.WW_WLT_DefaultLocationType = fixLocationType.PK;
			ftzWarehouse.WW_WLT_DefaultLocationType = fixLocationType.PK;
			containerYardWarehouse.WW_WLT_DefaultLocationType = fixLocationType.PK;
			AssertNoError(productWarehouse.WW_WLT_DefaultLocationTypeInfo, expectedMessage);
			AssertNoError(ftzWarehouse.WW_WLT_DefaultLocationTypeInfo, expectedMessage);
			AssertHasError(containerYardWarehouse.WW_WLT_DefaultLocationTypeInfo, expectedMessage);

			productWarehouse.WW_WLT_DefaultLocationType = conLocationType.PK;
			ftzWarehouse.WW_WLT_DefaultLocationType = conLocationType.PK;
			containerYardWarehouse.WW_WLT_DefaultLocationType = conLocationType.PK;
			transitWarehouse.WW_WLT_DefaultLocationType = conLocationType.PK;
			AssertNoError(productWarehouse.WW_WLT_DefaultLocationTypeInfo, expectedMessage);
			AssertNoError(ftzWarehouse.WW_WLT_DefaultLocationTypeInfo, expectedMessage);
			AssertHasError(containerYardWarehouse.WW_WLT_DefaultLocationTypeInfo, expectedMessage);
			AssertHasError(transitWarehouse.WW_WLT_DefaultLocationTypeInfo, expectedMessage);
		}

		#endregion

		#region TestCheckWW_FTZIsDetailedTrackingEnabled

		public void TestCheckWW_FTZIsDetailedTrackingEnabled()
		{
			var expectedMessage = "Detailed Tracking only available for Warehouse in Country/Region US or PR and Type 'FTZ'.";
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Australia;

			warehouse.WW_FTZIsDetailedTrackingEnabled = false;
			AssertNoErrors(warehouse.WW_FTZIsDetailedTrackingEnabledInfo);

			warehouse.WW_FTZIsDetailedTrackingEnabled = true;
			AssertHasError(warehouse.WW_FTZIsDetailedTrackingEnabledInfo, expectedMessage);

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			warehouse.WW_FTZIsDetailedTrackingEnabled = true;
			AssertHasError(warehouse.WW_FTZIsDetailedTrackingEnabledInfo, expectedMessage);

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			warehouse.WW_FTZIsDetailedTrackingEnabled = true;
			AssertHasError(warehouse.WW_FTZIsDetailedTrackingEnabledInfo, expectedMessage);

			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedStates;
			warehouse.WW_FTZIsDetailedTrackingEnabled = true;
			AssertNoErrors(warehouse.WW_FTZIsDetailedTrackingEnabledInfo);

			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.PuertoRico;
			warehouse.WW_FTZIsDetailedTrackingEnabled = true;
			AssertNoErrors(warehouse.WW_FTZIsDetailedTrackingEnabledInfo);
		}

		#endregion

		#region TestCheckWW_FTZDetailedTrackingMethod

		public void TestCheckWW_FTZDetailedTrackingMethod()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			AssertNoErrors("Precondition", warehouse.WW_FTZDetailedTrackingMethodInfo);

			warehouse.WW_FTZDetailedTrackingMethod = "";
			AssertHasError(warehouse.WW_FTZDetailedTrackingMethodInfo, "Please enter a Detailed Tracking Method.");

			warehouse.WW_FTZDetailedTrackingMethod = DetailedTrackingMethod.Codes.UIN;
			AssertNoErrors(warehouse.WW_FTZDetailedTrackingMethodInfo);

			warehouse.WW_FTZDetailedTrackingMethod = "XXX";
			AssertHasError(warehouse.WW_FTZDetailedTrackingMethodInfo, "Enter a valid Detailed Tracking Method.");

			warehouse.WW_FTZDetailedTrackingMethod = DetailedTrackingMethod.Codes.LOT;
			AssertNoErrors(warehouse.WW_FTZDetailedTrackingMethodInfo);
		}

		#endregion

		#region TestCheckWW_NumberOfCycleCountLocationsToAutoAssign

		public void TestCheckWW_NumberOfCycleCountLocationsToAutoAssign()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			AssertNoErrors("Precondition", warehouse.WW_NumberOfCycleCountLocationsToAutoAssignInfo);

			warehouse.WW_NumberOfCycleCountLocationsToAutoAssign = 0;
			AssertHasError(warehouse.WW_NumberOfCycleCountLocationsToAutoAssignInfo, "Number of Cycle Count Locations to Auto Assign cannot be zero.");

			warehouse.WW_NumberOfCycleCountLocationsToAutoAssign = 3;
			AssertNoErrors(warehouse.WW_NumberOfCycleCountLocationsToAutoAssignInfo);
		}

		#endregion

		#region TestCheckIsFixedWidthLocation

		public void TestCheckIsFixedWidthLocation_ForPrefixDuplicates()
		{
			AssertCheckIsFixedWidthLocation_ForPrefixDuplicates(validationRegistryValue: true);
		}

		public void TestCheckIsFixedWidthLocation_ForPrefixDuplicates_RegistryOff()
		{
			AssertCheckIsFixedWidthLocation_ForPrefixDuplicates(validationRegistryValue: false);
		}

		void AssertCheckIsFixedWidthLocation_ForPrefixDuplicates(bool validationRegistryValue)
		{
			using (WarehouseDataRegistry.Instance.EnableRowNamePrefixValidationForFixedWidthWarehouses.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, validationRegistryValue))
			{
				var whs1 = Helper.CreateWarehouse("WHR");
				var row11 = whs1.Rows.AddNew();
				row11.WR_Name = "AA";
				var row21 = whs1.Rows.AddNew();
				row21.WR_Name = "BB";

				var whs2 = Helper.CreateWarehouse("WHP");
				var row12 = whs2.Rows.AddNew();
				row12.WR_Name = "AB";
				var row22 = whs2.Rows.AddNew();
				row22.WR_Name = "CC";
				var row32 = whs2.Rows.AddNew();
				row32.WR_Name = "CCRow";
				var row42 = whs2.Rows.AddNew();
				row42.WR_Name = "ABRow";
				Factory.Save();

				whs1.IsFixedWidthLocation = true;
				AssertNoErrors(whs1.IsFixedWidthLocationInfo);

				whs2.WW_LocationColumnsFixedWidth = 1;
				whs2.WW_LocationLevelsFixedWidth = 1;
				whs2.WW_LocationTraysFixedWidth = 1;
				whs2.IsFixedWidthLocation = true;
				if (validationRegistryValue)
				{
					AssertHasError(whs2.IsFixedWidthLocationInfo, "Warehouse contains Row Names that are prefixes of other Row Names. Rows must not contain another Row's Name within their name to ensure that Fixed Width Locations can be uniquely addressed.");
				}
				else
				{
					AssertNoErrors(whs2.IsFixedWidthLocationInfo);
				}
			}
		}

		#endregion

		#region TestCheckFixedWidthLocationValues

		public void TestCheckFixedWidthLocation_Columns()
		{
			TestCheckFixedWidthLocationCore(
				(whs, fixedWidth) => whs.WW_LocationColumnsFixedWidth = fixedWidth,
				3,
				(whs) => whs.WW_LocationColumnsFixedWidthInfo,
				"Location Columns Fixed Width cannot be greater than 3.");
		}

		public void TestCheckFixedWidthLocation_Levels()
		{
			TestCheckFixedWidthLocationCore(
				(whs, fixedWidth) => whs.WW_LocationLevelsFixedWidth = fixedWidth,
				3,
				(whs) => whs.WW_LocationLevelsFixedWidthInfo,
				"Location Levels Fixed Width cannot be greater than 3.");
		}

		public void TestCheckFixedWidthLocation_Trays()
		{
			TestCheckFixedWidthLocationCore(
				(whs, fixedWidth) => whs.WW_LocationTraysFixedWidth = fixedWidth,
				2,
				(whs) => whs.WW_LocationTraysFixedWidthInfo,
				"Location Trays Fixed Width cannot be greater than 2.");
		}

		public void TestCheckFixedWidthLocationCore(
			Action<WhsWarehouse, ZByte> fixedWidthAssigner,
			ZByte maxLength,
			Func<WhsWarehouse, ZPropertyInfo> getPropertyInfo,
			string expectedError)
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			AssertNoErrors("Precondition", getPropertyInfo(warehouse));

			warehouse.IsFixedWidthLocation = true;
			fixedWidthAssigner(warehouse, (ZByte)(maxLength + 1));
			AssertHasError(getPropertyInfo(warehouse), expectedError);

			fixedWidthAssigner(warehouse, maxLength);
			AssertNoErrors(getPropertyInfo(warehouse));
		}

		public void TestCheckFixedWidthLocation_AlphaColumns()
		{
			TestCheckFixedWidthAlphaLocationCore(
				(whs) => whs.WW_LocationColumnsAlpha = true,
				(whs, fixedWidth) => whs.WW_LocationColumnsFixedWidth = fixedWidth,
				(whs) => whs.WW_LocationColumnsFixedWidthInfo,
				"Location Columns Fixed Width cannot be greater than 1 for Alpha Columns.");
		}

		public void TestCheckFixedWidthLocation_AlphaLevels()
		{
			TestCheckFixedWidthAlphaLocationCore(
				(whs) => whs.WW_LocationLevelsAlpha = true,
				(whs, fixedWidth) => whs.WW_LocationLevelsFixedWidth = fixedWidth,
				(whs) => whs.WW_LocationLevelsFixedWidthInfo,
				"Location Levels Fixed Width cannot be greater than 1 for Alpha Levels.");
		}

		public void TestCheckFixedWidthLocation_AlphaTrays()
		{
			TestCheckFixedWidthAlphaLocationCore(
				(whs) => whs.WW_LocationTraysAlpha = true,
				(whs, fixedWidth) => whs.WW_LocationTraysFixedWidth = fixedWidth,
				(whs) => whs.WW_LocationTraysFixedWidthInfo,
				"Location Trays Fixed Width cannot be greater than 1 for Alpha Trays.");
		}

		void TestCheckFixedWidthAlphaLocationCore(
			Action<WhsWarehouse> alphaColumnAssigner,
			Action<WhsWarehouse, ZByte> fixedWidthAssigner,
			Func<WhsWarehouse, ZPropertyInfo> getPropertyInfo,
			string expectedError)
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			alphaColumnAssigner(warehouse);
			AssertNoErrors("Precondition", getPropertyInfo(warehouse));

			warehouse.IsFixedWidthLocation = true;
			fixedWidthAssigner(warehouse, 2);
			AssertHasError(getPropertyInfo(warehouse), expectedError);

			fixedWidthAssigner(warehouse, 1);
			AssertNoErrors(getPropertyInfo(warehouse));
		}

		public void TestCheckFixedWidthLocation_WithExistingRows_Columns_InitialFixedWidthIsThree()
		{
			TestCheckFixedWidthLocation_WithExistingRowsCore(
				(row, warehouse) =>
				{
					row.WR_Columns = 100;
					warehouse.WW_LocationColumnsFixedWidth = 3;
					warehouse.WW_LocationLevelsFixedWidth = 1;
					warehouse.WW_LocationTraysFixedWidth = 1;
					warehouse.WW_LocationsHaveLeadingZeros = true;
				},
				warehouse => warehouse.WW_LocationColumnsFixedWidth = 2,
				(warehouse) => warehouse.WW_LocationColumnsFixedWidthInfo,
				"You cannot set the fixed width to less than 3 as there is currently one or more rows in this warehouse which contain more than 99 Columns.");
		}

		public void TestCheckFixedWidthLocation_WithExistingRows_Columns_InitialFixedWidthIsTwo()
		{
			TestCheckFixedWidthLocation_WithExistingRowsCore(
				(row, warehouse) =>
				{
					row.WR_Columns = 10;
					warehouse.WW_LocationColumnsFixedWidth = 2;
					warehouse.WW_LocationLevelsFixedWidth = 1;
					warehouse.WW_LocationTraysFixedWidth = 1;
					warehouse.WW_LocationsHaveLeadingZeros = true;
				},
				warehouse => warehouse.WW_LocationColumnsFixedWidth = 1,
				(warehouse) => warehouse.WW_LocationColumnsFixedWidthInfo,
				"You cannot set the fixed width to less than 2 as there is currently one or more rows in this warehouse which contain more than 9 Columns.");
		}

		public void TestCheckFixedWidthLocation_WithExistingRows_Levels_InitialFixedWidthIsThree()
		{
			TestCheckFixedWidthLocation_WithExistingRowsCore(
				(row, warehouse) =>
				{
					row.WR_Levels = 100;
					warehouse.WW_LocationColumnsFixedWidth = 1;
					warehouse.WW_LocationLevelsFixedWidth = 3;
					warehouse.WW_LocationTraysFixedWidth = 1;
					warehouse.WW_LocationsHaveLeadingZeros = true;
				},
				warehouse => warehouse.WW_LocationLevelsFixedWidth = 2,
				(warehouse) => warehouse.WW_LocationLevelsFixedWidthInfo,
				"You cannot set the fixed width to less than 3 as there is currently one or more rows in this warehouse which contain more than 99 Levels.");
		}

		public void TestCheckFixedWidthLocation_WithExistingRows_Levels_InitialFixedWidthIsTwo()
		{
			TestCheckFixedWidthLocation_WithExistingRowsCore(
				(row, warehouse) =>
				{
					row.WR_Levels = 10;
					warehouse.WW_LocationColumnsFixedWidth = 1;
					warehouse.WW_LocationLevelsFixedWidth = 2;
					warehouse.WW_LocationTraysFixedWidth = 1;
					warehouse.WW_LocationsHaveLeadingZeros = true;
				},
				warehouse => warehouse.WW_LocationLevelsFixedWidth = 1,
				(warehouse) => warehouse.WW_LocationLevelsFixedWidthInfo,
				"You cannot set the fixed width to less than 2 as there is currently one or more rows in this warehouse which contain more than 9 Levels.");
		}

		public void TestCheckFixedWidthLocation_WithExistingRows_Trays()
		{
			TestCheckFixedWidthLocation_WithExistingRowsCore(
				(row, warehouse) =>
				{
					row.WR_Trays = 10;
					warehouse.WW_LocationColumnsFixedWidth = 1;
					warehouse.WW_LocationLevelsFixedWidth = 1;
					warehouse.WW_LocationTraysFixedWidth = 2;
					warehouse.WW_LocationsHaveLeadingZeros = true;
				},
				warehouse => warehouse.WW_LocationTraysFixedWidth = 1,
				(warehouse) => warehouse.WW_LocationTraysFixedWidthInfo,
				"You cannot set the fixed width to less than 2 as there is currently one or more rows in this warehouse which contain more than 9 Trays.");
		}

		void TestCheckFixedWidthLocation_WithExistingRowsCore(
			Action<WhsRow, WhsWarehouse> warehouseAndRowConfigSetter,
			Action<WhsWarehouse> fixedWidthPropertySet,
			Func<WhsWarehouse, ZPropertyInfo> getPropertyInfo,
			string expectedErrorMessage)
		{
			// Suspending validation here to prevent row validation setting the calculated property IsFixedWidthLocation
			// false before these tests use the Db columns to set it correctly to true
			// In production row validation is on a different factory from warehouse validation and this collision of validations issue should never occur.
			Factory.SuspendValidation();
			var warehouse = Helper.CreateWarehouse("WHER", "A");
			var row = warehouse.Rows.Single(r => r.WR_Name == "A");
			Factory.ResumeValidation();
			warehouseAndRowConfigSetter(row, warehouse);
			AssertNoErrors("Precondition", getPropertyInfo(warehouse));

			fixedWidthPropertySet(warehouse);
			AssertHasError(getPropertyInfo(warehouse), expectedErrorMessage);
		}

		public void TestCheckFixedWidthLocation_NoLocationFixedWidthParameterConfigured()
		{
			var warehouse = Helper.CreateWarehouse("WH1", "A");
			AssertEquals("Precondition", ZByte.Zero, warehouse.WW_LocationColumnsFixedWidth);
			AssertEquals("Precondition", ZByte.Zero, warehouse.WW_LocationLevelsFixedWidth);
			AssertEquals("Precondition", ZByte.Zero, warehouse.WW_LocationTraysFixedWidth);

			warehouse.IsFixedWidthLocation = true;
			warehouse.Validation.ValidateWW_LocationColumnsFixedWidth();
			AssertHasError(warehouse.WW_LocationColumnsFixedWidthInfo, "Columns Fixed Width cannot be 0 when Location Fixed Width is enabled.");

			warehouse.Validation.ValidateWW_LocationLevelsFixedWidth();
			AssertHasError(warehouse.WW_LocationLevelsFixedWidthInfo, "Levels Fixed Width cannot be 0 when Location Fixed Width is enabled.");

			warehouse.Validation.ValidateWW_LocationTraysFixedWidth();
			AssertHasError(warehouse.WW_LocationTraysFixedWidthInfo, "Trays Fixed Width cannot be 0 when Location Fixed Width is enabled.");
		}

		public void TestCheckFixedWidthLocation_LocationColumnsFixedWidthNotEnabled_Columns()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			AssertNoErrors("Precondition", warehouse.WW_LocationColumnsFixedWidthInfo);
			AssertEquals("Precondition", ZByte.Zero, warehouse.WW_LocationTraysFixedWidth);
			AssertEquals("Precondition", false, warehouse.IsFixedWidthLocation);

			warehouse.WW_LocationColumnsFixedWidth = 1;
			AssertHasError(warehouse.WW_LocationColumnsFixedWidthInfo, "You cannot set the fixed width if Location Fixed Width is not enabled.");
		}

		public void TestCheckFixedWidthLocation_LocationColumnsFixedWidthNotEnabled_Levels()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			AssertNoErrors("Precondition", warehouse.WW_LocationColumnsFixedWidthInfo);
			AssertEquals("Precondition", ZByte.Zero, warehouse.WW_LocationTraysFixedWidth);
			AssertEquals("Precondition", false, warehouse.IsFixedWidthLocation);

			warehouse.WW_LocationLevelsFixedWidth = 1;
			AssertHasError(warehouse.WW_LocationLevelsFixedWidthInfo, "You cannot set the fixed width if Location Fixed Width is not enabled.");
		}

		public void TestCheckFixedWidthLocation_LocationColumnsFixedWidthNotEnabled_Trays()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			AssertNoErrors("Precondition", warehouse.WW_LocationColumnsFixedWidthInfo);
			AssertEquals("Precondition", ZByte.Zero, warehouse.WW_LocationTraysFixedWidth);
			AssertEquals("Precondition", false, warehouse.IsFixedWidthLocation);

			warehouse.WW_LocationTraysFixedWidth = 1;
			AssertHasError(warehouse.WW_LocationTraysFixedWidthInfo, "You cannot set the fixed width if Location Fixed Width is not enabled.");
		}

		public void TestCheckFixedWidthLocation_LocationsHaveLeadingZeros()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			AssertNoErrors("Precondition", warehouse.WW_LocationsHaveLeadingZerosInfo);
			AssertEquals("Precondition", false, warehouse.WW_LocationsHaveLeadingZeros);
			AssertEquals("Precondition", false, warehouse.IsFixedWidthLocation);

			warehouse.IsFixedWidthLocation = true;
			warehouse.WW_LocationsHaveLeadingZeros = false;
			AssertHasError(warehouse.WW_LocationsHaveLeadingZerosInfo, "Locations must have leading zeros if Location Fixed Width is enabled.");

			warehouse.WW_LocationsHaveLeadingZeros = true;
			AssertNoErrors(warehouse.WW_LocationsHaveLeadingZerosInfo);
		}

		#endregion

		#region TestFKsToNotValidateForCancelledRecords

		public void TestShouldValidateFKToCancelledRecord()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var validation = new TestWhsWarehouseValidation(warehouse);

			var list = new string[]
			{
				WhsWarehouseSchema.Constants.WW_DefaultInboundDockDoor,
				WhsWarehouseSchema.Constants.WW_DefaultOutboundDockDoor
			};

			foreach (var propertyInfo in warehouse.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(p => p.IsPersistent))
			{
				if (list.Contains(propertyInfo.Name))
				{
					AssertEquals("NK/FK which cannot be cancelled.", false, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
				}
				else
				{
					AssertEquals("All other properties should just return base condition of true.", true, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
				}
			}
		}

		#endregion

		#region TestCheckWW_UsesVehicleBookingIntegration

		public void TestCheckWW_UsesVehicleBookingIntegration()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			AssertNoErrors(warehouse.WW_UsesVehicleBookingIntegrationInfo);

			CreateOrgCommunityInfo(warehouse, "ABC");
			warehouse.WW_UsesVehicleBookingIntegration = true;
			AssertHasError(warehouse.WW_UsesVehicleBookingIntegrationInfo, "Active organizations in pre-arrival instruction and release order must have VBS community code set before VBS integration can be enabled.");
		}

		public void TestCheckWW_UsesVehicleBookingIntegration_WithValidCommunityCode()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			AssertNoErrors(warehouse.WW_UsesVehicleBookingIntegrationInfo);

			CreateOrgCommunityInfo(warehouse, "CC1");
			warehouse.WW_UsesVehicleBookingIntegration = true;
			AssertNoErrors(warehouse.WW_UsesVehicleBookingIntegrationInfo);
		}

		void CreateOrgCommunityInfo(WhsWarehouse warehouse, string codeType)
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_Code = "ABC01";

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = header.PK;
			orgAddress.OA_Code = "Test Address";

			var receiveAdvice = (BusinessObject)Factory.New<ICYDReceiveAdvice>();
			receiveAdvice[CYDReceiveAdviceSchema.YRA_FromDate] = ZDate.Today.AddDays(-1);
			receiveAdvice[CYDReceiveAdviceSchema.YRA_ToDate] = ZDate.Today.AddDays(2);
			receiveAdvice[CYDReceiveAdviceSchema.YRA_JobNumber] = "YRA12345";
			receiveAdvice[CYDReceiveAdviceSchema.YRA_AcceptanceNumber] = "YRAN";
			receiveAdvice[CYDReceiveAdviceSchema.YRA_Mode] = "FCL";
			receiveAdvice[CYDReceiveAdviceSchema.YRA_WW_Yard] = warehouse.PK;

			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_ParentID = receiveAdvice.PK;
			jobDocAddress.E2_OA_Address = orgAddress.PK;
			jobDocAddress.E2_AddressType = "BKD";
			jobDocAddress.E2_ParentTableCode = "YRA";

			var orgCusCode = Factory.NewWithValidTestData<OrgCusCode>();
			orgCusCode.OK_RN_NKCodeCountry = ZString.Empty;
			orgCusCode.OK_OH = header.PK;
			orgCusCode.OK_OA_PremisesAddress = orgAddress.PK;
			orgCusCode.OK_CodeType = codeType;
			orgCusCode.OK_CustomsRegNo = "TEST";

			Factory.Save();
		}

		#endregion

		#region TestWhsWarehouseValidation

		class TestWhsWarehouseValidation : WhsWarehouseValidation
		{
			public TestWhsWarehouseValidation(WhsWarehouse parent)
				: base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecordExposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}

		#endregion

		#region Implementation

		WhsRow Row => row ?? (row = Whs.Rows.AddNew());

		WhsWarehouse Whs => whs ?? (whs = Factory.New<WhsWarehouse>());

		WhsRow row;
		WhsWarehouse whs;

		#endregion
	}
}
