using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Common.Testing
{
	public abstract class DocketLineFromInventoryHelperTest<TBusinessObject, TDocketLine> : WhsTestCaseWithFactory
		where TBusinessObject : WhsDocket
		where TDocketLine : WhsDocketLine
	{
		#region TestNullNotificationSubscriber

		public void TestNullNotificationSubscriber()
		{
			AssertExceptionThrown<ArgumentNullException>(() => GetInventoryHelper(null));
		}

		#endregion

		#region CreateDocketLineFromInventory

		#region TestCreateDocketLineFromInventory

		public void TestCreateDocketLineFromInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);

			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Today.AddDays(+2), ZDate.Today.AddDays(-5), "PA11", "PA21", "PA31", "BEK-11");
			inventory1.WI_WL = locations[0].PK;
			inventory1.WI_PalletID = "P_ID_001";
			inventory1.WI_ArrivalDate = ZDateTimeOffset.Today.AddDays(1);
			Helper.SetInventoryCustomAttributes(inventory1, "CA11", "CA21", "CA31", "CA41", "CA51", "CA61", 11m, 12m, 13m, 14m, 15m,
				ZDateTime.Now.AddDays(11), ZDateTime.Now.AddDays(12), ZDateTime.Now.AddDays(13), ZDateTime.Now.AddDays(14), ZDateTime.Now.AddDays(15), true, true, true, true, true, "TB1");

			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, ZDate.Today.AddDays(+3), ZDate.Today.AddDays(-6), "PA12", "PA22", "PA32", "BEK-12");
			inventory2.WI_WL = locations[1].PK;
			inventory2.WI_PalletID = "P_ID_002";
			inventory2.WI_ArrivalDate = ZDateTimeOffset.Today.AddDays(2);
			Helper.SetInventoryCustomAttributes(inventory2, "CA12", "CA22", "CA32", "CA42", "CA52", "CA62", 21m, 22m, 23m, 24m, 25m,
				ZDateTime.Now.AddDays(21), ZDateTime.Now.AddDays(22), ZDateTime.Now.AddDays(23), ZDateTime.Now.AddDays(24), ZDateTime.Now.AddDays(25), false, false, false, false, false, "TB2");

			var docket = GetNewDocket(data.Org1, data.Whs1);
			docket.WD_WW_Whs = data.Whs1.PK;
			var docketLineFromInventoryHelper = GetInventoryHelper(docket);
			var docketLine1 = docketLineFromInventoryHelper.CreateDocketLineFromInventory(docket.Lines, inventory1);
			var docketLine2 = docketLineFromInventoryHelper.CreateDocketLineFromInventory(docket.Lines, inventory2);
			AssertEquals(2, docket.Lines.Count);
			AssertDocketLineEqualsInventory(inventory1, docketLine1);
			AssertDocketLineEqualsInventory(inventory2, docketLine2);
		}

		#endregion

		#region TestCreateDocketLineFromInventory_WithEmptyClientAndWarehouse

		public void TestCreateDocketLineFromInventory_WithEmptyClientAndWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var otherWhs = Helper.CreateWarehouse("Whs2", "W2", "A");
			var otherClient = Helper.CreateClient("C2");
			Helper.CreateProductClientRelationShip(otherClient, data.Part1);
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(otherClient, otherWhs, "R2", data.Part1, 10m);
			Factory.Save();

			var docket = Factory.New<TBusinessObject>();
			AssertNull("Precondition.", docket.Client);
			AssertNull("Precondition.", docket.Warehouse);

			var docketLineFromInventoryHelper = GetInventoryHelper(docket);
			docketLineFromInventoryHelper.CreateDocketLineFromInventory(docket.Lines, receive1.Inventory[0]);
			AssertEquals("Should *not* populate Client.", null, docket.Client);
			AssertEquals("Should *not* populate Warehouse.", null, docket.Warehouse);

			docketLineFromInventoryHelper.CreateDocketLineFromInventory(docket.Lines, receive2.Inventory[0]);
			AssertEquals("Should *not* override Client.", null, docket.Client);
			AssertEquals("Should *not* override Warehouse.", null, docket.Warehouse);
		}

		#endregion

		#region TestCreateDocketLineFromInventory_WithEmptyClient

		public void TestCreateDocketLineFromInventory_WithEmptyClient()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var otherWhs = Helper.CreateWarehouse("Whs2", "W2", "A");
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var docket = Factory.New<TBusinessObject>();
			docket.WD_WW_Whs = otherWhs.PK;
			AssertNull("Precondition.", docket.Client);

			var docketLineFromInventoryHelper = GetInventoryHelper(docket);
			docketLineFromInventoryHelper.CreateDocketLineFromInventory(docket.Lines, receive1.Inventory[0]);
			AssertEquals("Should *not* populate Client.", null, docket.Client);
			AssertEquals("Should *not* override Warehouse.", otherWhs, docket.Warehouse);
		}

		#endregion

		#region TestCreateDocketLineFromInventory_WithEmptyWarehouse

		public void TestCreateDocketLineFromInventory_WithEmptyWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var otherClient = Helper.CreateClient("C2");
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var docket = Factory.New<TBusinessObject>();
			docket.WD_OH_Client = otherClient.PK;
			AssertNull("Precondition.", docket.Warehouse);

			var docketLineFromInventoryHelper = GetInventoryHelper(docket);
			docketLineFromInventoryHelper.CreateDocketLineFromInventory(docket.Lines, receive1.Inventory[0]);
			AssertEquals("Should *not* populate Warehouse.", null, docket.Warehouse);
			AssertEquals("Should *not* override Client.", otherClient, docket.Client);
		}

		#endregion

		#region TestCreateDocketLineFromInventory_ProductValidationIsRun

		public void TestCreateDocketLineFromInventory_ProductValidationIsRun()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);

			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"), "P_ID_001");

			var docket = GetNewDocket(data.Org1, data.Whs1);
			var docketLineFromInventoryHelper = GetInventoryHelper(docket);
			WhsDocketLine lineAdded = null;
			var productValidationHitCount = 0;
			docket.Lines.CountChanged += (sender, e) =>
			{
				lineAdded = docket.Lines.Single();
				lineAdded.WE_OPInfo.AdditionalValidation += () => productValidationHitCount++;
			};

			var newLine = docketLineFromInventoryHelper.CreateDocketLineFromInventory(docket.Lines, inventory);
			AssertEquals(lineAdded, newLine);
			AssertEquals("Validation for the Product should have run.", 1, productValidationHitCount);
		}

		#endregion

		#endregion

		#region AcceptInventoryLinesFromSearchGrid

		#region TestAcceptInventoryLinesFromSearchGrid

		public void TestAcceptInventoryLinesFromSearchGrid()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = "CUS";

			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Today.AddDays(+2), ZDate.Today.AddDays(-5), "PA11", "PA21", "PA31", "BEK-11");
			inventory1.WI_WL = locations[0].PK;
			inventory1.WI_PalletID = "P_ID_001";
			inventory1.WI_ArrivalDate = ZDateTimeOffset.Today.AddDays(1);
			Helper.SetInventoryCustomAttributes(inventory1, "CA11", "CA21", "CA31", "CA41", "CA51", "CA61", 11m, 12m, 13m, 14m, 15m,
				ZDateTime.Now.AddDays(11), ZDateTime.Now.AddDays(12), ZDateTime.Now.AddDays(13), ZDateTime.Now.AddDays(14), ZDateTime.Now.AddDays(15), true, true, true, true, true, "TB1");
			SetCustomsData(inventory1);

			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, ZDate.Today.AddDays(+3), ZDate.Today.AddDays(-6), "PA12", "PA22", "PA32", "BEK-12");
			inventory2.WI_WL = locations[1].PK;
			inventory2.WI_PalletID = "P_ID_002";
			inventory2.WI_ArrivalDate = ZDateTimeOffset.Today.AddDays(2);
			Helper.SetInventoryCustomAttributes(inventory2, "CA12", "CA22", "CA32", "CA42", "CA52", "CA62", 21m, 22m, 23m, 24m, 25m,
				ZDateTime.Now.AddDays(21), ZDateTime.Now.AddDays(22), ZDateTime.Now.AddDays(23), ZDateTime.Now.AddDays(24), ZDateTime.Now.AddDays(25), false, false, false, false, false, "TB2");

			var docket = GetNewDocket(data.Org1, data.Whs1);

			if (CanBeCustomsJob)
			{
				docket.WD_DocketSubType = "CUS";
				AssertEquals("Precondition: Docket is a Customs Transaction.", true, docket.IsCustomsTransaction);
			}
			else
			{
				AssertEquals("Precondition: Docket is not a Customs Transaction.", false, docket.IsCustomsTransaction);
			}

			var docketLineFromInventoryHelper = GetInventoryHelper(docket);
			docketLineFromInventoryHelper.AcceptInventoryLinesFromSearchGrid(docket.Lines, new[] { inventory1, inventory2 });
			docket.Lines.ApplySort(WhsDocketLineSchema.Constants.WE_TransactionQuantity, ListSortDirection.Ascending);

			AssertEquals("precondition - collection allows new lines", true, ((IBindingList)docket.Lines).AllowNew);
			AssertEquals(2, docket.Lines.Count);
			AssertDocketLineEqualsInventory(inventory1, docket.Lines[0]);
			AssertDocketLineEqualsInventory(inventory2, docket.Lines[1]);
		}

		protected virtual bool CanBeCustomsJob => true;

		void SetCustomsData(WhsInventoryView inventory)
		{
			AssertEquals("Precondition - New columns need testing.", 46, WhsBondedWarehouseAttributeSchema.All.Count - 4); // No need to test SystemCreate and SystemLastEdit columns
			var year = ZDate.Today.Year;
			var customsData = inventory.InDocketLine.CustomsData;
			customsData.WB_EntryLineNo = 1;
			customsData.WB_EntryKey = "DEF";
			customsData.WB_AddInfo = "addinfo";
			customsData.WB_BondedWhsQty = 19;
			customsData.WB_BondedWhsUnitOfQty = "KG";
			customsData.WB_CustomsQty = 3;
			customsData.WB_CustomsUnitOfQty = "LB";
			customsData.WB_DeclarationReference = "dec ref";
			customsData.WB_EntryDate = new ZDateTime(year, 3, 19);
			customsData.WB_IsActive = true;
			customsData.WB_ParentTableCode = "WE";
			customsData.WB_RN_NKCountryOfOrigin = "AU";
			customsData.WB_RX_NKTILVCurrency = "AUD";
			customsData.WB_TILV = 11.5;
			customsData.WB_ValueForDuty = 100;
			customsData.WB_PrimaryPreference = "STANDARD";
			customsData.WB_CustomsSecondQuantity = 99;
			customsData.WB_CustomsSecondUnitQty = "LB";
			customsData.WB_CustomsThirdQuantity = 5;
			customsData.WB_CustomsThirdUnitQty = "KG";
			customsData.WB_CustomsFourthQuantity = 150;
			customsData.WB_CustomsFourthUnitQty = "LB";
			customsData.WB_CustomsFifthQuantity = 8.5;
			customsData.WB_CustomsFifthUnitQty = "KG";
			customsData.WB_Tariff = "01010101";
			customsData.WB_ParentID = inventory.InDocketLine.PK;
			customsData.WB_ZoneStatus = "";
			customsData.WB_IsFromAnotherFTZWhs = false;
			customsData.WB_OutwardType = "";
			customsData.WB_CustomsDeadline = ZDate.Empty;
			customsData.WB_InwardStyle = "";
			customsData.WB_InwardProcedure = "";
			customsData.WB_MatchingKey = "";
			customsData.WB_IsMainInwardsProcessedItem = true;
			customsData.WB_IsSecondaryInwardsProcessedItem = true;
			customsData.WB_Remarks = "Test";
			customsData.WB_RN_NKCountryOfDestination = "IE";
			customsData.WB_AllDutiesAmount = 10m;
			customsData.WB_VATAmount = 2.3m;

			var orgAddress = Factory.New<OrgAddress>();
			customsData.WB_OA_ManufacturerAddress = orgAddress.PK;
		}

		public void TestAcceptInventoryLinesFromSearchGrid_NonCustomsTransaction()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = "CUS";

			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"), "P_ID_001");
			inventory.WI_ArrivalDate = ZDateTimeOffset.Today.AddDays(1);
			SetCustomsData(inventory);

			Helper.EnableWarehouseForBond(data.Whs1, false);
			var docket = GetNewDocket(data.Org1, data.Whs1);
			AssertEquals("Precondition: Docket is not a Customs Transaction.", false, docket.IsCustomsTransaction);

			var docketLineFromInventoryHelper = GetInventoryHelper(docket);
			docketLineFromInventoryHelper.AcceptInventoryLinesFromSearchGrid(docket.Lines, new[] { inventory });
			AssertEquals(1, docket.Lines.Count);
			AssertDocketLineEqualsInventory(inventory, docket.Lines[0]);
		}

		public void TestAcceptInventoryLinesFromSearchGrid_WrongClient()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);

			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"), "P_ID_001");
			inventory.WI_ArrivalDate = ZDateTimeOffset.Today.AddDays(1);

			var client2 = Helper.CreateClient("Client2");
			var docket = GetNewDocket(client2, data.Whs1);
			var docketLineFromInventoryHelper = GetInventoryHelper(docket);
			docketLineFromInventoryHelper.AcceptInventoryLinesFromSearchGrid(docket.Lines, new[] { inventory });
			AssertEquals(1, docket.Lines.Count);
			AssertHasError(docket.Lines[0].WE_OPInfo, "This product is not related to the client. Products must have an Owner Relationship for the client");
		}

		#endregion

		#region TestAcceptInventoryLinesFromSearchGrid_BondedUS

		public void TestAcceptInventoryLinesFromSearchGrid_BondedUS()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = "CUS";
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, "123-1", "ABC", 10m);
			var docket = GetNewDocket(data.Org1, data.Whs1);

			AssertEquals("precondition - collection allows new lines", true, ((IBindingList)docket.Lines).AllowNew);
			var docketLineFromInventoryHelper = GetInventoryHelper(docket);
			docketLineFromInventoryHelper.AcceptInventoryLinesFromSearchGrid(docket.Lines, new[] { inventory });
			AssertDocketLineEqualsInventory(inventory, docket.Lines[0]);
		}

		#endregion

		#region TestAcceptInventoryLinesFromSearchGrid_DoesNotExecute_IfCollectionDoesNotAllowNew

		public void TestAcceptInventoryLinesFromSearchGrid_DoesNotExecute_IfCollectionDoesNotAllowNew()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docket = GetNewDocket(data.Org1, data.Whs1);
			docket.WD_OH_Client = ZGuid.Empty;
			docket.WD_WW_Whs = ZGuid.Empty;
			docket.WD_DocketStatus = DocketStatus.Codes.Entered;

			var docketLineFromInventoryHelper = GetInventoryHelper(docket);

			AssertEquals("precondition - collection does not allow new lines", false, ((IBindingList)docket.Lines).AllowNew);
			docketLineFromInventoryHelper.AcceptInventoryLinesFromSearchGrid(docket.Lines, new BusinessObject[1] { Factory.New<WhsInventoryView>() });
			AssertEquals(true, ((NotificationBuffer)docket.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseCollectionDoesNotAllowNew));

			((NotificationBuffer)docket.NotificationManager.Peek).Clear();
			docket.WD_DocketStatus = docket.WD_DocketType == DocketType.Codes.Order ? DocketStatus.Codes.Picking : DocketStatus.Codes.Finalised;

			AssertEquals("precondition - collection does not allow new lines", false, ((IBindingList)docket.Lines).AllowNew);
			docketLineFromInventoryHelper.AcceptInventoryLinesFromSearchGrid(docket.Lines, new BusinessObject[1] { Factory.New<WhsInventoryView>() });
			AssertEquals(true, ((NotificationBuffer)docket.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseCollectionDoesNotAllowNew));

			((NotificationBuffer)docket.NotificationManager.Peek).Clear();
			docket.WD_DocketStatus = DocketStatus.Codes.Cancelled;

			AssertEquals("precondition - collection does not allow new lines", false, ((IBindingList)docket.Lines).AllowNew);
			docketLineFromInventoryHelper.AcceptInventoryLinesFromSearchGrid(docket.Lines, new BusinessObject[1] { Factory.New<WhsInventoryView>() });
			AssertEquals(true, ((NotificationBuffer)docket.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseCollectionDoesNotAllowNew));
		}

		#endregion

		#region TestAcceptInventoryLinesFromSearchGrid_HandlesNullArray

		[ExpectException(typeof(ArgumentNullException))]
		public void TestAcceptInventoryLinesFromSearchGrid_HandlesNullArray()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docket = GetNewDocket(data.Org1, data.Whs1);
			var docketLineFromInventoryHelper = GetInventoryHelper(docket);
			docketLineFromInventoryHelper.AcceptInventoryLinesFromSearchGrid(docket.Lines, null);
		}

		#endregion

		#endregion

		#region SetDocketLineFromInventory

		public void TestSetDocketLineFromInventory_DoesNotAcceptNull()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var docket = GetNewDocket(data.Org1, data.Whs1);
			var docketLine = GetNewDocketLine(docket, data.Part1, data.Whs1.DefaultLocation);
			var docketLineFromInventoryHelper = GetInventoryHelper(docket);
			AssertExceptionThrown<ArgumentNullException>(() => docketLineFromInventoryHelper.SetDocketLineFromInventory(docketLine, null, ExcludeFromCopy.None));
		}

		public void TestSetDocketLineFromInventory_DoesNotAcceptNonInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var orderLine = Factory.New<WhsOrderLine>();
			var docket = GetNewDocket(data.Org1, data.Whs1);
			var docketLine = GetNewDocketLine(docket, data.Part1, data.Whs1.DefaultLocation);
			var docketLineFromInventoryHelper = GetInventoryHelper(docket);
			AssertExceptionThrown(typeof(ArgumentException), "You should never pass in a Docket Line which is not a Inventory Line.", () => docketLineFromInventoryHelper.SetDocketLineFromInventory(docketLine, orderLine, ExcludeFromCopy.None));
		}

		public void TestSetDocketLineFromInventory_ExcludeCustomAttribs()
		{
			var receiveLine = Factory.New<WhsReceiveLine>();
			Helper.SetDocketLineCustomAttributes(receiveLine, "CA1", "CA2", "CA3", "CA4", "CA5", "CA6", 1.1m, 2.2m, 3.3m, 4.4m, 5.5m, new ZDateTime(2017, 1, 1), new ZDateTime(2017, 1, 2), new ZDateTime(2017, 1, 3), new ZDateTime(2017, 1, 4), new ZDateTime(2017, 1, 5), true, true, true, true, true, "BLOB");

			var docket = Factory.New<TBusinessObject>();
			var docketLine1 = (TDocketLine)docket.Lines.AddNew();
			var docketLine2 = (TDocketLine)docket.Lines.AddNew();
			var docketLineFromInventoryHelper = GetInventoryHelper(docket);
			docketLineFromInventoryHelper.SetDocketLineFromInventory(docketLine1, receiveLine, ExcludeFromCopy.None);
			docketLineFromInventoryHelper.SetDocketLineFromInventory(docketLine2, receiveLine, ExcludeFromCopy.CustomAttribs);
			Helper.AssertDocketLineCustomAttributes(docketLine1, "CA1", "CA2", "CA3", "CA4", "CA5", "CA6", 1.1m, 2.2m, 3.3m, 4.4m, 5.5m, new ZDateTime(2017, 1, 1), new ZDateTime(2017, 1, 2), new ZDateTime(2017, 1, 3), new ZDateTime(2017, 1, 4), new ZDateTime(2017, 1, 5), true, true, true, true, true, "BLOB");
			Helper.AssertDocketLineCustomAttributes(docketLine2, "", "", "", "", "", "", 0m, 0m, 0m, 0m, 0m, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, false, false, false, false, false, "");
		}

		public void TestSetDocketLineFromInventory_ExcludePackType()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receiveLine = Factory.New<WhsReceiveLine>();
			receiveLine.WE_F3_NKPackType = "BOX";

			var docket = Factory.New<TBusinessObject>();
			var docketLine1 = (TDocketLine)docket.Lines.AddNew();
			var docketLine2 = (TDocketLine)docket.Lines.AddNew();
			var docketLineFromInventoryHelper = GetInventoryHelper(docket);
			docketLineFromInventoryHelper.SetDocketLineFromInventory(docketLine1, receiveLine, ExcludeFromCopy.None);
			docketLineFromInventoryHelper.SetDocketLineFromInventory(docketLine2, receiveLine, ExcludeFromCopy.PackType);
			AssertEquals("Pack Type should be copied by default.", "BOX", docketLine1.WE_F3_NKPackType);
			AssertEquals("Pack Type should be not be copied when excluded from defaulting.", "", docketLine2.WE_F3_NKPackType);
		}

		public void TestSetDocketLineFromInventory_Qty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receiveLine = receive.Lines[0];

			var docket = Factory.New<TBusinessObject>();
			var docketLine1 = (TDocketLine)docket.Lines.AddNew();
			var docketLine2 = (TDocketLine)docket.Lines.AddNew();
			var docketLineFromInventoryHelper = GetInventoryHelper(docket);
			docketLineFromInventoryHelper.SetDocketLineFromInventory(docketLine1, receiveLine, ExcludeFromCopy.None);
			docketLineFromInventoryHelper.SetDocketLineFromInventory(docketLine2, receiveLine, ExcludeFromCopy.Qty);
			AssertDocketLinesForSetDocketLineFromInventory(docketLine1, docketLine2, 10m);
		}

		public void TestSetDocketLineFromInventory_SuspendsValidationForProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receiveLine = receive.Lines[0];

			var docket = Factory.New<TBusinessObject>();
			var docketLine = (TDocketLine)docket.Lines.AddNew();
			var isValidationSuspended = false;
			var productValidationHitCount = 0;
			docketLine.WE_OPInfo.ValueChanged += (sender, e) => isValidationSuspended = docketLine.IsValidationSuspended;
			docketLine.WE_OPInfo.AdditionalValidation += () => productValidationHitCount++;
			var docketLineFromInventoryHelper = GetInventoryHelper(docket);
			docketLineFromInventoryHelper.SetDocketLineFromInventory(docketLine, receiveLine, ExcludeFromCopy.None);
			AssertEquals("Product Validation should have been suspended when setting the Product field.", true, isValidationSuspended);
			AssertEquals("Product Validation should have been run afterwards.", 1, productValidationHitCount);
		}

		#endregion

		#region Implementation

		void AssertDocketLineEqualsInventory(WhsInventoryView inventory, WhsDocketLine line)
		{
			AssertEquals("WE_OP", inventory.WI_OP, line.WE_OP);
			AssertNoErrors(line.WE_OPInfo);
			AssertEquals("WE_F3_NKPackType", inventory.WI_F3_NKPackType, line.WE_F3_NKPackType);
			AssertEquals("WE_BondedEntryKey", inventory.WI_BondedEntryKey, line.WE_BondedEntryKey);

			AssertLocations(line, inventory);

			WhsBondedWarehouseAttribute.BreakUpKey(inventory.WI_BondedEntryKey);

			AssertEquals("WE_PackageGroupId", inventory.PackageGroupId, line.WE_PackageGroupId);
			AssertEquals("WE_PerPackageQty", inventory.PerPackageQty, line.WE_PerPackageQty);
			Helper.AssertAttributes(inventory, line);
			Helper.AssertDocketLineCustomAttributes(inventory, line.WE_CustomAttrib1, line.WE_CustomAttrib2, line.WE_CustomAttrib3, line.WE_CustomAttrib4, line.WE_CustomAttrib5, line.WE_CustomAttrib6,
				line.WE_CustomDecimal1, line.WE_CustomDecimal2, line.WE_CustomDecimal3, line.WE_CustomDecimal4, line.WE_CustomDecimal5, line.WE_CustomDate1, line.WE_CustomDate2, line.WE_CustomDate3,
				line.WE_CustomDate4, line.WE_CustomDate5, line.WE_CustomFlag1, line.WE_CustomFlag2, line.WE_CustomFlag3, line.WE_CustomFlag4, line.WE_CustomFlag5, line.WE_CustomTextBlob1);

			if (line.IsCustomsTransaction)
			{
				AssertEquals("CustomsData.WB_ParentID, Should copy from new docketline.", line.PK, line.CustomsData.WB_ParentID);
				AssertDocketLineEqualsInventory_CustomsData(inventory.CustomsData, line.CustomsData);
			}
			else
			{
				AssertEquals((ZShort)0, line.CustomsData.WB_EntryLineNo);
				AssertEquals("", line.CustomsData.WB_EntryKey);
				AssertEquals(0m, line.CustomsData.WB_BondedWhsQty);
			}
		}

		protected virtual void AssertDocketLineEqualsInventory_CustomsData(WhsBondedWarehouseAttribute inventoryCustomsData, WhsBondedWarehouseAttribute docketlineCustomsData)
		{
			AssertEquals("CustomsData.WB_EntryLineNo, Should always copied.", inventoryCustomsData.WB_EntryLineNo, docketlineCustomsData.WB_EntryLineNo);
			AssertEquals("CustomsData.WB_EntryKey, Should always copied.", inventoryCustomsData.WB_EntryKey, docketlineCustomsData.WB_EntryKey);
			AssertEquals("CustomsData.WB_AddInfo", ZString.Empty, docketlineCustomsData.WB_AddInfo);
			AssertEquals("CustomsData.WB_BondedWhsQty", 0m, docketlineCustomsData.WB_BondedWhsQty);
			AssertEquals("CustomsData.WB_BondedWhsUnitOfQty", ZString.Empty, docketlineCustomsData.WB_BondedWhsUnitOfQty);
			AssertEquals("CustomsData.WB_CustomsQty", 0m, docketlineCustomsData.WB_CustomsQty);
			AssertEquals("CustomsData.WB_CustomsUnitOfQty", ZString.Empty, docketlineCustomsData.WB_CustomsUnitOfQty);
			AssertEquals("CustomsData.WB_DeclarationReference", ZString.Empty, docketlineCustomsData.WB_DeclarationReference);
			AssertEquals("CustomsData.WB_EntryDate", ZDateTime.Empty, docketlineCustomsData.WB_EntryDate);
			AssertEquals("CustomsData.WB_IsActive", inventoryCustomsData.WB_IsActive, docketlineCustomsData.WB_IsActive);
			AssertEquals("CustomsData.WB_ParentTableCode, Should not copied.", "WE", docketlineCustomsData.WB_ParentTableCode);
			AssertEquals("CustomsData.WB_RN_NKCountryOfOrigin", ZString.Empty, docketlineCustomsData.WB_RN_NKCountryOfOrigin);
			AssertEquals("CustomsData.WB_RX_NKTILVCurrency", ZString.Empty, docketlineCustomsData.WB_RX_NKTILVCurrency);
			AssertEquals("CustomsData.WB_TILV", 0m, docketlineCustomsData.WB_TILV);
			AssertEquals("CustomsData.WB_ValueForDuty", 0m, docketlineCustomsData.WB_ValueForDuty);
			AssertEquals("CustomsData.WB_WB_InwardsEntry", ZGuid.Empty, docketlineCustomsData.WB_WB_InwardsEntry);
			AssertEquals("CustomsData.WB_PrimaryPreference", ZString.Empty, docketlineCustomsData.WB_PrimaryPreference);
			AssertEquals("CustomsData.WB_CustomsSecondQuantity", 0m, docketlineCustomsData.WB_CustomsSecondQuantity);
			AssertEquals("CustomsData.WB_CustomsSecondUnitQty", ZString.Empty, docketlineCustomsData.WB_CustomsSecondUnitQty);
			AssertEquals("CustomsData.WB_CustomsThirdQuantity", 0m, docketlineCustomsData.WB_CustomsThirdQuantity);
			AssertEquals("CustomsData.WB_CustomsThirdUnitQty", ZString.Empty, docketlineCustomsData.WB_CustomsThirdUnitQty);
			AssertEquals("CustomsData.WB_CustomsFourthQuantity", 0m, docketlineCustomsData.WB_CustomsFourthQuantity);
			AssertEquals("CustomsData.WB_CustomsFourthUnitQty", ZString.Empty, docketlineCustomsData.WB_CustomsFourthUnitQty);
			AssertEquals("CustomsData.WB_CustomsFifthQuantity", 0m, docketlineCustomsData.WB_CustomsFifthQuantity);
			AssertEquals("CustomsData.WB_CustomsFifthUnitQty", ZString.Empty, docketlineCustomsData.WB_CustomsFifthUnitQty);
			AssertEquals("CustomsData.WB_Tariff", ZString.Empty, docketlineCustomsData.WB_Tariff);
			AssertEquals("CustomsData.WB_OA_ManufacturerAddress", ZGuid.Empty, docketlineCustomsData.WB_OA_ManufacturerAddress);
			AssertEquals("CustomsData.WB_IsMainInwardsProcessedItem", false, docketlineCustomsData.WB_IsMainInwardsProcessedItem);
			AssertEquals("CustomsData.WB_IsSecondaryInwardsProcessedItem", false, docketlineCustomsData.WB_IsSecondaryInwardsProcessedItem);
			AssertEquals("CustomsData.WB_Remarks", ZString.Empty, docketlineCustomsData.WB_Remarks);
			AssertEquals("CustomsData.WB_RN_NKCountryOfDestination", ZString.Empty, docketlineCustomsData.WB_RN_NKCountryOfDestination);
		}

		protected virtual void AssertLocations(WhsDocketLine line, WhsInventoryView inventory)
		{
			AssertEquals("WE_WL", inventory.WI_WL, line.WE_WL);
			AssertEquals("WE_PalletID", inventory.WI_PalletID, line.WE_PalletID);
		}

		protected virtual void AssertDocketLinesForSetDocketLineFromInventory(WhsDocketLine docketLineWithoutExclude, WhsDocketLine docketLineWithExclude, decimal qtyOnInventory)
		{
			AssertEquals("Qty should be copied.", qtyOnInventory, docketLineWithoutExclude.WE_TransactionQuantity);
			AssertEquals("Qty should not be copied when excluded.", 0m, docketLineWithExclude.WE_TransactionQuantity);
		}

		protected abstract TBusinessObject GetNewDocket(OrgHeader client, WhsWarehouse warehouse);
		protected abstract TDocketLine GetNewDocketLine(TBusinessObject docket, OrgSupplierPart part, WhsLocation location);
		protected abstract DocketLineFromInventoryHelper<TDocketLine> GetInventoryHelper(WhsDocket docket);

		#endregion
	}
}
