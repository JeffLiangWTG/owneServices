using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsStocktake))]
	public sealed class WhsStocktakeTest : WhsBusinessObjectTestCase
	{
		#region TestSaveAndDeleteBusinessObject

		[ExpectNoExceptions]
		public override void TestSaveAndDeleteBusinessObject()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			stocktake.WS_WL_Location = data.Whs1.DefaultLocation.PK;
			Factory.Save();

			stocktake.Delete();
			Factory.Save();
		}

		#endregion

		#region TestCreateAdjustmentIfItIsDeletedForStocktake

		public void TestCreateAdjustmentIfItIsDeletedForStocktake()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0m,
				5m, 1, StocktakeLineStatus.Codes.Open);
			var line2 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 0m,
				1m, 1, StocktakeLineStatus.Codes.Open);
			AssertNull("Adjustment is not created yet.", stocktake.UnfinalizedAdjustmentForTesting);
			stocktake.CloseLines(new WhsStocktakeLine[] { line1 });
			AssertNotNull("An Adjustment should be created.", stocktake.UnfinalizedAdjustmentForTesting);

			stocktake.UnfinalizedAdjustmentForTesting.Delete();

			AssertNoExceptionThrown(() => { stocktake.CloseLines(new WhsStocktakeLine[] { line2 }); });
		}

		#endregion

		#region Test Saving

		#region TestOnFactorySaving

		public void TestOnFactorySaving()
		{
			OrgHeader org = Helper.CreateClient();
			WhsWarehouse whs = Helper.CreateWarehouse("1");
			Stocktake.WS_WW_Whs = whs.PK;
			Stocktake.WS_OH_Client = org.PK;

			AssertEquals("Precondition", false, Stocktake.IsInDatabase);
			AssertEquals("Precondition", ZString.Empty, Stocktake.WS_StocktakeNumber);

			Factory.Save();

			AssertEquals("Stocktake not saved", true, Stocktake.IsInDatabase);
			AssertEquals("WS_StocktakeNumber is not set", "SK00000001", Stocktake.WS_StocktakeNumber);
		}

		#endregion

		#region TestNumberFountainsAreNotAccessedMoreThanOnceOnSaving

		public void TestNumberFountainsAreNotAccessedMoreThanOnceOnSaving()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// save the first stocktake

			var stocktake1 = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			AssertEquals("Precondition - Stocktake ID should be empty.", true, stocktake1.WS_StocktakeNumber.IsEmpty);
			Factory.Save();
			AssertEquals("WS_StocktakeNumber is set to a valid value", "SK00000001", stocktake1.WS_StocktakeNumber);

			// save a second stocktake

			var stocktake2 = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			AssertEquals("Precondition - Stocktake ID should be empty.", true, stocktake2.WS_StocktakeNumber.IsEmpty);
			Factory.Save();
			AssertEquals("WS_StocktakeNumber is set to a valid value", "SK00000002", stocktake2.WS_StocktakeNumber);
		}

		#endregion

		#region TestOnFactorySavedFailed

		public void TestOnFactorySavedFailed()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);

			stocktake.WS_WL_Location = ZGuid.Invalid;

			AssertExceptionThrown<ZSaveException>(() => Factory.Save());

			AssertEquals("WS_StocktakeNumber should have been cleared on save failure.", true, stocktake.WS_StocktakeNumber.IsEmpty);
		}

		#endregion

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			Stocktake.WS_StocktakeNumber = "SK00001001";
			AssertEquals("Warehouse Stocktake SK00001001", Stocktake.HumanReadableName);
		}

		#endregion

		#region Test Notes

		public void TestNoteContextsForRelatedNotes()
		{
			WhsStocktake stocktake = Factory.New<WhsStocktake>();

			Assert("Should always be 'Warehouse' module",
				(stocktake.GetNoteContextsForRelatedNotes().Module & StmNoteContextModule.W) != 0);
			Assert("Should always be 'Internal' direction",
				(stocktake.GetNoteContextsForRelatedNotes().Direction & StmNoteContextDirection.I) != 0);
			Assert("Should always be 'Stocktake' freight mode",
				(stocktake.GetNoteContextsForRelatedNotes().FreightMode & StmNoteContextFreightMode.S) != 0);

			OrgHeader client = Helper.CreateClient();
			WhsWarehouse warehouse = Helper.CreateWarehouse("Warehouse");
			CreateTestNoteColection(client);
			CreateTestNoteColection(warehouse);

			stocktake.WS_OH_Client = client.PK;
			stocktake.WS_WW_Whs = warehouse.PK;
			AssertEquals("Visible Notes Count", 8, stocktake.Notes.VisibleNotes.Count);
		}

		void CreateTestNoteColection(BusinessObject businessObject)
		{
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.A, StmNoteContextDirection.A,
				StmNoteContextFreightMode.A);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.A,
				StmNoteContextFreightMode.A);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.R,
				StmNoteContextFreightMode.A);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.R,
				StmNoteContextFreightMode.R);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.O,
				StmNoteContextFreightMode.A);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.O,
				StmNoteContextFreightMode.O);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.O,
				StmNoteContextFreightMode.R);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.O,
				StmNoteContextFreightMode.T);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.I,
				StmNoteContextFreightMode.A);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.I,
				StmNoteContextFreightMode.T);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.I,
				StmNoteContextFreightMode.D);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.I,
				StmNoteContextFreightMode.S);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.W, StmNoteContextDirection.I,
				StmNoteContextFreightMode.P);

			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.A, StmNoteContextDirection.I,
				StmNoteContextFreightMode.A);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.A, StmNoteContextDirection.O,
				StmNoteContextFreightMode.A);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.A, StmNoteContextDirection.A,
				StmNoteContextFreightMode.S);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.A, StmNoteContextDirection.A,
				StmNoteContextFreightMode.R);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.F, StmNoteContextDirection.I,
				StmNoteContextFreightMode.A);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.F, StmNoteContextDirection.A,
				StmNoteContextFreightMode.P);
			GetStmNote(businessObject.GetNotes().AddNew(), StmNoteContextModule.F, StmNoteContextDirection.I,
				StmNoteContextFreightMode.P);
		}

		static StmNote GetStmNote(StmNote note, StmNoteContextModule module, StmNoteContextDirection direction,
			StmNoteContextFreightMode freightMode)
		{
			note.ST_NoteContextModule = module.ToString();
			note.ST_NoteContextDirection = direction.ToString();
			note.ST_NoteContextFreightMode = freightMode.ToString();
			return note;
		}

		#endregion

		#region Related Entities

		#region TestClosingLines

		public void TestClosingLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeLine1 =
				Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, "", true);
			var stocktakeLine2 =
				Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part2, data.Whs1.DefaultLocation, "", true);
			AssertEquals(0, stocktake.ClosingLines.Count());

			bool stocktakeLineWasClosed = false;
			stocktakeLine1.WU_StatusInfo.ValueChanged += delegate
			{
				stocktakeLineWasClosed = stocktakeLine1.IsClosed;
				AssertContainsExactElementsInAnyOrder(new[] { stocktakeLine1 }, stocktake.ClosingLines);
			};

			stocktake.CloseLines(new[] { stocktakeLine1 });
			AssertEquals("Should have closed the line and also prove that the assertion of Closing Lines was done.",
				true, stocktakeLineWasClosed);
		}

		#endregion

		#region TestLines

		public void TestLines()
		{
			WhsStocktakeLineCollection origCollection = Stocktake.Lines;
			Stocktake.Lines.AddNew().WU_Status = StocktakeLineStatus.Codes.Open;
			Stocktake.Lines.AddNew().WU_Status = StocktakeLineStatus.Codes.Closed;

			AssertEquals("All lines should be returned.", 2, Stocktake.Lines.Count);
		}

		public void TestLines_CountChanged_CalledCheckAndUpdateStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.New);
			Factory.Save();

			stocktake.Load();
			AssertEquals(StocktakeStatus.Codes.Loaded, stocktake.WS_StocktakeStatus);
			AssertEquals(2, stocktake.Lines.Count);

			var originalLines = stocktake.Lines.ToArray();
			var manuallyAddedLine = stocktake.Lines.AddNew();

			stocktake.CloseLines(originalLines);
			AssertEquals(StocktakeStatus.Codes.Loaded, stocktake.WS_StocktakeStatus);

			stocktake.Lines.Delete(manuallyAddedLine);
			AssertEquals(StocktakeStatus.Codes.Finalised, stocktake.WS_StocktakeStatus);
		}

		#endregion

		#endregion

		#region Properties

		#region TestWS_ABCAnalysisCategory

		public void TestWS_ABCAnalysisCategory()
		{
			Stocktake.WS_ABCAnalysisCategory = "B";
			AssertEquals("B", Stocktake.WS_ABCAnalysisCategory);
		}

		#endregion

		#region TestWS_CountEmptyLocationsCategory

		public void TestWS_CountEmptyLocationsCategory()
		{
			Stocktake.WS_CountEmptyLocationsCategory = "X";
			AssertEquals("X", Stocktake.WS_CountEmptyLocationsCategory);
		}

		#endregion

		#region TestClientAddress

		public void TestClientAddress()
		{
			AssertEquals("Precondition", null, Stocktake.ClientAddress);
			OrgHeader client = Factory.New<OrgHeader>();
			OrgAddress address = client.MainAddress;
			address.OA_Address1 = "Address Line 1";
			address.OA_Address2 = "Address Line 2";
			address.OA_City = "SYDNEY";
			address.OA_PostCode = "2000";

			Stocktake.WS_OH_Client = client.PK;

			AssertEquals(client.MainAddress, Stocktake.ClientAddress);
		}

		#endregion

		#region TestClientChangeKeepsMatchedProducts

		public void TestClientChangeKeepsMatchedProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			Helper.CreateWhsStocktakeProductFilter(stocktake, data.Part1);
			Helper.CreateWhsStocktakeProductFilter(stocktake, data.Part2);

			AssertEquals("Precondition: there are 2 product filters", 2, stocktake.ProductFilterCollection.Count);

			var client2 = Helper.CreateClient("C2");
			Helper.CreateProductClientRelationShip(client2, data.Part1, OrgPartRelation.RelationshipTypes.Supplier);
			Helper.CreateProductClientRelationShip(client2, data.Part2, OrgPartRelation.RelationshipTypes.Owner);
			stocktake.WS_OH_Client = client2.PK;

			AssertEquals("There should be 1 product filter left", 1, stocktake.ProductFilterCollection.Count);
			AssertEquals("The only product left should be Part2, because it has OWN relationship to client2",
				data.Part2, stocktake.ProductFilterCollection[0].Product);

			var client3 = Helper.CreateClient("C3");
			Helper.CreateProductClientRelationShip(client3, data.Part2, OrgPartRelation.RelationshipTypes.Both);
			stocktake.WS_OH_Client = client3.PK;
			AssertEquals("There should be 1 product filter left", 1, stocktake.ProductFilterCollection.Count);
			AssertEquals("The only product left should be Part2, because it has BTH relationship to client3",
				data.Part2, stocktake.ProductFilterCollection[0].Product);
		}

		#endregion

		#region TestClientChangeWipesOutProductFilterWhenClientCompletelyDifferent

		public void TestClientChangeWipesOutProductFilterWhenClientCompletelyDifferent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			Helper.CreateWhsStocktakeProductFilter(stocktake, data.Part1);
			Helper.CreateWhsStocktakeProductFilter(stocktake, data.Part2);

			AssertEquals("Precondition: there are 2 product filters", 2, stocktake.ProductFilterCollection.Count);

			var client2 = Helper.CreateClient("C2"); // client has no relationships with data.Part1 and data.Part2
			stocktake.WS_OH_Client = client2.PK;

			AssertEquals("There should be 0 product filter left because client2 has no links to Part1 and Part2", 0,
				stocktake.ProductFilterCollection.Count);
		}

		#endregion

		#region TestSelectedArea

		public void TestSelectedArea()
		{
			AssertEquals("Precondition", null, Stocktake.SelectedArea);

			WhsWarehouse whs = Helper.CreateWarehouse("1");
			WhsArea area = Helper.CreateArea(whs, "AREA1", "FRE");
			Stocktake.WS_WA_Area = area.PK;

			AssertEquals(area, Stocktake.SelectedArea);
		}

		#endregion

		#region TestSelectedRow

		public void TestSelectedRow()
		{
			AssertEquals("Precondition", null, Stocktake.SelectedRow);

			WhsWarehouse whs = Helper.CreateWarehouse("1");
			WhsRow row = Helper.CreateRow(whs, "ROW1");
			Stocktake.WS_WR_Row = row.PK;

			AssertEquals(row, Stocktake.SelectedRow);
		}

		#endregion

		#region TestReadOnlyWS_RH_NKCommodityCodeInfo

		public void TestReadOnlyWS_RH_NKCommodityCodeInfo()
		{
			AssertEquals(CodeLists.StocktakeStatus.Codes.New, Stocktake.WS_StocktakeStatus);
			AssertEquals(false, Stocktake.WS_RH_NKCommodityCodeInfo.ReadOnly);

			Stocktake.WS_StocktakeStatus = CodeLists.StocktakeStatus.Codes.Loaded;
			AssertEquals(true, Stocktake.WS_RH_NKCommodityCodeInfo.ReadOnly);

			Stocktake.WS_StocktakeStatus = CodeLists.StocktakeStatus.Codes.Finalised;
			AssertEquals(true, Stocktake.WS_RH_NKCommodityCodeInfo.ReadOnly);
		}

		#endregion

		#region TestIsBondedWarehouse

		public void TestIsBondedWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			AssertEquals(false, stocktake.IsBondedWarehouse);

			Helper.EnableWarehouseForBond(data.Whs1, true);
			AssertEquals(true, stocktake.IsBondedWarehouse);

			stocktake.WS_WW_Whs = ZGuid.Empty;
			AssertEquals(false, stocktake.IsBondedWarehouse);
		}

		#endregion

		#region TestIsFinalised

		public void TestIsFinalised()
		{
			Stocktake.WS_StocktakeStatus = CodeLists.StocktakeStatus.Codes.New;
			AssertEquals("IsFinalised must return false when status = New", false, Stocktake.IsFinalised);

			Stocktake.WS_StocktakeStatus = CodeLists.StocktakeStatus.Codes.Loaded;
			AssertEquals("IsFinalised must return false when status = Loaded", false, Stocktake.IsFinalised);

			Stocktake.WS_StocktakeStatus = CodeLists.StocktakeStatus.Codes.Finalised;
			AssertEquals("IsFinalised must return true when status = Finalised", true, Stocktake.IsFinalised);

			Stocktake.WS_StocktakeStatus = ZString.Empty;
			AssertEquals("IsFinalised must return false when status is empty", false, Stocktake.IsFinalised);
		}

		#endregion

		#region TestIsLoaded

		public void TestIsLoaded()
		{
			Stocktake.WS_StocktakeStatus = CodeLists.StocktakeStatus.Codes.New;
			AssertEquals("IsLoaded must return false when status = New", false, Stocktake.IsLoaded);

			Stocktake.WS_StocktakeStatus = CodeLists.StocktakeStatus.Codes.Loaded;
			AssertEquals("IsLoaded must return true when status = Loaded", true, Stocktake.IsLoaded);

			Stocktake.WS_StocktakeStatus = CodeLists.StocktakeStatus.Codes.Finalised;
			AssertEquals("IsLoaded must return false when status = Finalised", false, Stocktake.IsLoaded);

			Stocktake.WS_StocktakeStatus = ZString.Empty;
			AssertEquals("IsLoaded must return false when status is empty", false, Stocktake.IsLoaded);
		}

		#endregion

		#region TestFilterMustBeReadonly

		public void TestFilterMustBeReadonly()
		{
			Stocktake.WS_StocktakeStatus = CodeLists.StocktakeStatus.Codes.New;
			AssertEquals("FilterMustBeReadonly must return false when status = New", false,
				Stocktake.FilterMustBeReadonly);

			Stocktake.WS_StocktakeStatus = CodeLists.StocktakeStatus.Codes.Loaded;
			AssertEquals("FilterMustBeReadonly must return true when status = Loaded", true,
				Stocktake.FilterMustBeReadonly);

			Stocktake.WS_StocktakeStatus = CodeLists.StocktakeStatus.Codes.Finalised;
			AssertEquals("FilterMustBeReadonly must return true when status = Finalised", true,
				Stocktake.FilterMustBeReadonly);

			Stocktake.WS_StocktakeStatus = ZString.Empty;
			AssertEquals("FilterMustBeReadonly must return false when status is empty", false,
				Stocktake.FilterMustBeReadonly);
		}

		#endregion

		#region TestCurrentCountColumnNumber

		public void TestCurrentCountColumnNumber()
		{
			// Setup test data
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory);

			var stocktakeWithoutLines =
				Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1); // Stocktaketake doesn't have lines
			AssertEquals(new ZByte(1), stocktakeWithoutLines.CurrentCountColumnNumber);

			var stocktakeWithCurrentCountColumnNumberOne = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			Helper.CreateWhsStocktakeLine(stocktakeWithCurrentCountColumnNumberOne, data.Org1, data.Part1,
				data.Whs1.DefaultLocation, 1, 1, StocktakeLineStatus.Codes.Open); // Open line With CountOne
			AssertEquals(new ZByte(1), stocktakeWithCurrentCountColumnNumberOne.CurrentCountColumnNumber);

			var stocktakeWithCurrentCountColumnNumberTwo = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			Helper.CreateWhsStocktakeLine(stocktakeWithCurrentCountColumnNumberTwo, data.Org1, data.Part1,
				data.Whs1.DefaultLocation, 1, 2, StocktakeLineStatus.Codes.Open); // Open line With CountTwo
			AssertEquals(new ZByte(2), stocktakeWithCurrentCountColumnNumberTwo.CurrentCountColumnNumber);

			var stocktakeWithCurrentCountColumnNumberThree =
				Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			Helper.CreateWhsStocktakeLine(stocktakeWithCurrentCountColumnNumberThree, data.Org1, data.Part1,
				data.Whs1.DefaultLocation, 1, 3, StocktakeLineStatus.Codes.Open); // Open line With CountThree
			AssertEquals(new ZByte(3), stocktakeWithCurrentCountColumnNumberThree.CurrentCountColumnNumber);

			var stocktakeWithCurrentCountOneAndClosedLine = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			Helper.CreateWhsStocktakeLine(stocktakeWithCurrentCountOneAndClosedLine, data.Org1, data.Part1,
				data.Whs1.DefaultLocation, 1, 1, StocktakeLineStatus.Codes.Closed); // Closed line With CountOne
			AssertEquals(new ZByte(1), stocktakeWithCurrentCountOneAndClosedLine.CurrentCountColumnNumber);

			var stocktakeWithCurrentCountTwoAndClosedLine = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			Helper.CreateWhsStocktakeLine(stocktakeWithCurrentCountTwoAndClosedLine, data.Org1, data.Part1,
				data.Whs1.DefaultLocation, 1, 2, StocktakeLineStatus.Codes.Closed); // Closed line With CountTwo
			AssertEquals(new ZByte(2), stocktakeWithCurrentCountTwoAndClosedLine.CurrentCountColumnNumber);

			var stocktakeWithCurrentCountThreeAndClosedLine =
				Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			Helper.CreateWhsStocktakeLine(stocktakeWithCurrentCountThreeAndClosedLine, data.Org1, data.Part1,
				data.Whs1.DefaultLocation, 1, 3, StocktakeLineStatus.Codes.Closed); // Closed line With CountThree
			AssertEquals(new ZByte(3), stocktakeWithCurrentCountThreeAndClosedLine.CurrentCountColumnNumber);
		}

		#endregion

		#region TestMaximumNumberOfColumns

		public void TestMaximumNumberOfColumns()
		{
			AssertEquals("Maximum number of count columns should be three.", 3, WhsStocktake.MaximumNumberOfColumns);
		}

		#endregion

		#region Location

		#region TestLocation

		public void TestLocation()
		{
			// create environment
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Factory.Save();
			// create stocktake
			var stocktake = Factory.New<WhsStocktake>();

			// Stocktake without warehouse

			stocktake.LocationString = "";
			AssertNull(stocktake.Location);
			AssertEquals("FK should be empty", ZGuid.Empty, stocktake.WS_WL_Location);
			AssertNull(stocktake.Warehouse);
			AssertEquals("", stocktake.LocationString);
			AssertNull(stocktake.Warehouse);

			stocktake.LocationString = "A-2-1";
			AssertNull(stocktake.Location);
			AssertEquals("Location Guid should be empty", ZGuid.Empty, stocktake.WS_WL_Location);
			AssertEquals("A-2-1", stocktake.LocationString);
			AssertNull(stocktake.Warehouse);

			stocktake.LocationString = "AAAA";
			AssertNull(stocktake.Location);
			AssertEquals("Location Guid should be empty", ZGuid.Empty, stocktake.WS_WL_Location);
			AssertEquals("AAAA", stocktake.LocationString);
			AssertNull(stocktake.Warehouse);

			// Stocktake with warehouse

			stocktake.WS_WW_Whs = data.Whs1.PK;

			stocktake.LocationString = "";
			AssertNull(stocktake.Location);
			AssertEquals("FK should be empty", ZGuid.Empty, stocktake.WS_WL_Location);
			AssertEquals("", stocktake.LocationString);
			AssertEquals(data.Whs1, stocktake.Warehouse);

			stocktake.LocationString = "A-2";
			AssertEquals(data.Whs1.FindLocation("A-2-1"), stocktake.Location);
			AssertEquals("Location Guid should be A-2-1 location's guid.", data.Whs1.FindLocation("A-2-1").PK,
				stocktake.WS_WL_Location);
			AssertEquals("A-2-1", stocktake.LocationString);
			AssertEquals(data.Whs1, stocktake.Warehouse);

			stocktake.LocationString = "AAAA";
			AssertNull(stocktake.Location);
			AssertEquals("Location Guid should be empty", ZGuid.Empty, stocktake.WS_WL_Location);
			AssertEquals("AAAA", stocktake.LocationString);
			AssertEquals(data.Whs1, stocktake.Warehouse);
		}

		public void TestLocation_FixedWidthLocation()
		{
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("1", 3, 3, 2);
			Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 2);
			Factory.Save();

			var stocktake = Factory.New<WhsStocktake>();

			stocktake.WS_WW_Whs = warehouse.PK;

			stocktake.LocationString = "";
			AssertNull(stocktake.Location);
			AssertEquals("FK should be empty", ZGuid.Empty, stocktake.WS_WL_Location);
			AssertEquals("", stocktake.LocationString);
			AssertEquals(warehouse, stocktake.Warehouse);

			var location1 = warehouse.FindLocation("A002001");
			stocktake.LocationString = "A002";
			AssertEquals(location1, stocktake.Location);
			AssertEquals("Location Guid should be A002001 location's guid.", location1.PK, stocktake.WS_WL_Location);
			AssertEquals("A-002-001", stocktake.LocationString);
			AssertEquals(warehouse, stocktake.Warehouse);

			stocktake.LocationString = "AAAA";
			AssertNull(stocktake.Location);
			AssertEquals("Location Guid should be empty", ZGuid.Empty, stocktake.WS_WL_Location);
			AssertEquals("AAAA", stocktake.LocationString);
			AssertEquals(warehouse, stocktake.Warehouse);

			stocktake.LocationString = "A002001";
			AssertEquals(location1, stocktake.Location);
			AssertEquals("Location Guid should be A002001 location's guid.", location1.PK, stocktake.WS_WL_Location);
			AssertEquals("A-002-001", stocktake.LocationString);
			AssertEquals(warehouse, stocktake.Warehouse);

			stocktake.LocationString = "A-2-1";
			AssertNull(stocktake.Location);
			AssertEquals("Location Guid should be empty", ZGuid.Empty, stocktake.WS_WL_Location);
			AssertEquals("A-2-1", stocktake.LocationString);
			AssertEquals(warehouse, stocktake.Warehouse);

			stocktake.LocationString = "A-002-001";
			AssertEquals(location1, stocktake.Location);
			AssertEquals("Location Guid should be A002001 location's guid.", location1.PK, stocktake.WS_WL_Location);
			AssertEquals("A-002-001", stocktake.LocationString);
			AssertEquals(warehouse, stocktake.Warehouse);

			stocktake.LocationString = "";
			AssertNull(stocktake.Location);
			AssertEquals("Location Guid should be empty", ZGuid.Empty, stocktake.WS_WL_Location);
			AssertEquals("", stocktake.LocationString);
			AssertEquals(warehouse, stocktake.Warehouse);

			stocktake.LocationString = "A-002";
			AssertEquals(location1, stocktake.Location);
			AssertEquals("Location Guid should be A002001 location's guid.", location1.PK, stocktake.WS_WL_Location);
			AssertEquals("A-002-001", stocktake.LocationString);
			AssertEquals(warehouse, stocktake.Warehouse);
		}

		#endregion

		#region TestLocationAndWarehouse

		public void TestLocationWithChangingWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var warehouse = Helper.CreateWarehouse("TWS", "B", 1, 1);
			Factory.Save();

			var stocktake = Factory.New<WhsStocktake>();

			stocktake.LocationString = "";
			AssertNoErrors(stocktake.LocationStringInfo);

			stocktake.LocationString = "C";
			AssertHasError(stocktake.LocationStringInfo, "A valid warehouse has not been selected for this Stocktake.");

			stocktake.WS_WW_Whs = data.Whs1.PK;
			stocktake.LocationString = "A";
			AssertNoErrors(stocktake.LocationStringInfo);

			stocktake.WS_WW_Whs = warehouse.PK;
			stocktake.LocationString = "A"; // To trigger the validation again.
			AssertHasError(stocktake.LocationStringInfo, "Please enter a valid Location Row.");

			stocktake.LocationString = "B";
			AssertNoErrors(stocktake.LocationStringInfo);

			stocktake.LocationString = "";
			AssertNoErrors(stocktake.LocationStringInfo);
		}

		#endregion

		#endregion

		#region TestCountryCode

		public void TestCountryCode()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			data.Whs1.WarehouseAddress.OA_RN_NKCountryCode = "AU";
			AssertEquals("AU", stocktake.CountryCode);

			data.Whs1.WarehouseAddress.OA_RN_NKCountryCode = "US";
			AssertEquals("US", stocktake.CountryCode);

			data.Whs1.WarehouseAddress.OA_RN_NKCountryCode = "";
			AssertEquals("", stocktake.CountryCode);

			stocktake.WS_WW_Whs = ZGuid.Empty;
			AssertEquals("", stocktake.CountryCode);
		}

		#endregion

		#endregion

		#region CloseLines

		#region TestCloseLines

		public void TestCloseLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			var lineWithValidationErrors = stocktake.Lines.AddNew(); // Without a product and a location
			var lineWithoutValidationErrors = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1,
				data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);

			AssertEquals(false, stocktake.CloseLines(new[] { lineWithValidationErrors, lineWithoutValidationErrors }));
			AssertEquals(true, lineWithValidationErrors.HasErrors);
			AssertEquals(false, lineWithoutValidationErrors.HasErrors);
			AssertEquals(StocktakeLineStatus.Codes.Open, lineWithValidationErrors.WU_Status);
			AssertEquals(StocktakeLineStatus.Codes.Closed, lineWithoutValidationErrors.WU_Status);

			lineWithValidationErrors.WU_OH_Client = data.Org1.PK;
			lineWithValidationErrors.WU_OP = data.Part1.PK;
			lineWithValidationErrors.WU_WL = data.Whs1.DefaultLocation.PK;
			lineWithValidationErrors.WU_InventoryStatus = InventoryStatus.Codes.Available;

			AssertEquals(true, stocktake.CloseLines(new[] { lineWithValidationErrors }));
			AssertEquals(false, lineWithValidationErrors.HasErrors);
			AssertEquals(StocktakeLineStatus.Codes.Closed, lineWithValidationErrors.WU_Status);
		}

		#endregion

		#region TestCloseLines_CreatesNewAdjustmentIfNoAdjustmentExists

		public void TestCloseLines_CreatesNewAdjustmentIfNoAdjustmentExists()
		{
			var warehouse = Helper.CreateWarehouse("Test Warehouse", "TestRow1", 1, 1);
			var organization = Helper.CreateClient();
			var part = Helper.CreateProduct(organization, "TestProd1");

			Stocktake.WS_WW_Whs = warehouse.PK;
			Stocktake.WS_OH_Client = organization.PK;
			Stocktake.WS_StocktakeNumber = "T00000003";

			Stocktake.Lines.AddNew().WU_OP = part.PK;
			Stocktake.Lines[0].WU_WL = warehouse.Rows[0].Locations[0].PK;
			Stocktake.Lines[0].WU_LastCount = 5;
			Stocktake.Lines[0].WU_SystemUnits = 10;
			Stocktake.Lines[0].WU_OH_Client = organization.PK;

			Stocktake.CloseLines(new WhsStocktakeLine[] { Stocktake.Lines[0] });
			AssertEquals("Stocktake should have one Adjustment", 1, Stocktake.Adjustments.Count);

			var adjustment = Stocktake.Adjustments[0];
			AssertEquals("Stocktake should have one Adjustment Line", 1, adjustment.Lines.Count);
			AssertEquals("Comment is auto filled if stocktake line comment was empty", "Auto Stocktake Adjustment",
				adjustment.Lines[0].WE_LineComment);
			AssertEquals("Reason code for line is auto filled", "STA", adjustment.Lines[0].WE_ReasonCode);
		}

		#endregion

		#region TestCloseLines_CreatesAdjustmentLinesWithAppropriateStatusAndHeldCodes

		public void TestCloseLines_CreatesAdjustmentLinesWithAppropriateStatusAndHeldCodes()
		{
			var abcCode = Helper.CreateInventoryHeldCode("ABC", "ABC");
			Factory.Save();

			var data = new TestDataSimpleEnvironment(Factory, 1, 3);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, locations[0],
				StocktakeLineStatus.Codes.Open, StocktakeInventoryStatus.Codes.Damaged, 5, 10);
			Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, locations[1],
				StocktakeLineStatus.Codes.Open, StocktakeInventoryStatus.Codes.Held, 5, 10);
			Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, locations[2],
				StocktakeLineStatus.Codes.Open, StocktakeInventoryStatus.Codes.Available, 5, 10);
			Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, locations[2],
				StocktakeLineStatus.Codes.Open, StocktakeInventoryStatus.Codes.Held, 5, 0);
			stocktake.CloseLines(stocktake.Lines.ToArray());
			AssertEquals("Stocktake should have one Adjustment", 1, stocktake.Adjustments.Count);

			var adjustment = Stocktake.Adjustments[0];
			AssertEquals("Stocktake should have 4 Adjustment Lines", 4, adjustment.Lines.Count);

			AssertNotNull(adjustment.Lines.Single(l =>
				l.WE_OriginalInventoryStatus == InventoryStatus.Codes.Available &&
				l.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty));
			var adjustOutHeldLine = (WhsAdjustmentLine)adjustment.Lines.Single(l =>
				l.WE_OriginalInventoryStatus == InventoryStatus.Codes.Held &&
				l.WE_WHC_NKOriginalInventoryHeldCode == ZString.Empty && l.WE_TransactionQuantity == -5);
			AssertNotNull(adjustOutHeldLine);
			AssertNotNull(adjustment.Lines.Single(l =>
				l.WE_OriginalInventoryStatus == InventoryStatus.Codes.Held &&
				l.WE_WHC_NKOriginalInventoryHeldCode == InventoryHoldCodes.Codes.Damaged));
			AssertNotNull(adjustment.Lines.Single(l =>
				l.WE_OriginalInventoryStatus == InventoryStatus.Codes.Held &&
				l.WE_WHC_NKOriginalInventoryHeldCode == InventoryHoldCodes.Codes.Held &&
				l.WE_TransactionQuantity == 5));

			// Test adjustment line with empty held code functions as desired
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventoryABC_Held = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, locations[1]);
			var inventoryHEL_Held = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, locations[1]);
			var inventoryDAM = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, locations[1]);
			var inventoryAVL = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, locations[1]);
			inventoryABC_Held.OriginalInventoryHeldCode = "ABC";
			inventoryHEL_Held.OriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			inventoryDAM.OriginalInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var strategy = new WhsInventoryCommitter<WhsAdjustmentLine>(adjustOutHeldLine);
			strategy.CommitInventory();
			AssertEquals(5m, strategy.TotalQtyCommitted);
		}

		#endregion

		#region TestCloseLines_AdjustmentExternalReferences

		public void TestCloseLines_AdjustmentExternalReferences()
		{
			var warehouse = Helper.CreateWarehouse("Test Warehouse", "TestRow1", 2, 1);
			var organization = Helper.CreateClient();
			var part = Helper.CreateProduct(organization, "TestProd1");
			Factory.Save();

			var stocktake = GetNewBusinessObject();
			stocktake.WS_WW_Whs = warehouse.PK;
			stocktake.WS_OH_Client = organization.PK;
			stocktake.WS_StocktakeNumber = "00000001";

			stocktake.Lines.AddNew().WU_OP = part.PK;
			stocktake.Lines[0].WU_WL = warehouse.FindLocation("TestRow1-1").PK;
			stocktake.Lines[0].WU_LastCount = 5;
			stocktake.Lines[0].WU_SystemUnits = 10;

			stocktake.CloseLines(new WhsStocktakeLine[] { stocktake.Lines[0] });
			AssertEquals("Stocktake Adjustment's external reference is invalid",
				"STOCKTAKE " + stocktake.WS_StocktakeNumber + "-1",
				stocktake.Adjustments[0].WD_ExternalReference.ToString());
			stocktake.Adjustments[0].WD_DocketStatus = DocketStatus.Codes.Finalised;
			stocktake.Adjustments[0].WD_FinalisedDate = ZDateTimeOffset.Now;

			stocktake.Lines.AddNew().WU_OP = part.PK;
			stocktake.Lines[1].WU_WL = warehouse.FindLocation("TestRow1-2").PK;
			stocktake.Lines[1].WU_LastCount = 3;
			stocktake.Lines[1].WU_SystemUnits = 5;
			stocktake.CloseLines(new WhsStocktakeLine[] { stocktake.Lines[1] });
			AssertEquals("Stocktake Adjustment's external reference is invalid",
				"STOCKTAKE " + stocktake.WS_StocktakeNumber + "-2",
				stocktake.Adjustments[1].WD_ExternalReference.ToString());
		}

		#endregion

		#region TestCloseLines_ReusesCachedUnfinalisedAdjustment

		public void TestCloseLines_ReusesCachedUnfinalisedAdjustment()
		{
			var warehouse = Helper.CreateWarehouse("Test Warehouse", "TestRow1", 2, 1);
			var organization = Helper.CreateClient();
			var part = Helper.CreateProduct(organization, "TestProd1");
			Factory.Save();

			Stocktake.WS_WW_Whs = warehouse.PK;
			Stocktake.WS_OH_Client = organization.PK;
			Stocktake.WS_StocktakeNumber = "T00000002";

			Stocktake.Lines.AddNew().WU_OP = part.PK;
			Stocktake.Lines[0].WU_WL = warehouse.FindLocation("TestRow1-1").PK;
			Stocktake.Lines[0].WU_LastCount = 5;
			Stocktake.Lines[0].WU_SystemUnits = 10;
			Stocktake.Lines[0].WU_OH_Client = organization.PK;
			Stocktake.CloseLines(new WhsStocktakeLine[] { Stocktake.Lines[0] });

			Stocktake.Lines.AddNew().WU_OP = part.PK;
			Stocktake.Lines[1].WU_WL = warehouse.FindLocation("TestRow1-2").PK;
			Stocktake.Lines[1].WU_LastCount = 3;
			Stocktake.Lines[1].WU_SystemUnits = 5;
			Stocktake.Lines[1].WU_OH_Client = organization.PK;
			AssertEquals("Stocktake first Adjustment is not Finalized or Cancelled", false,
				Stocktake.Adjustments[0].IsFinalisedOrCancelled);
			Stocktake.CloseLines(new WhsStocktakeLine[] { Stocktake.Lines[1] });

			AssertEquals("Stocktake should have only one Adjustment", 1, Stocktake.Adjustments.Count);
			AssertEquals("Stocktake Adjustment should have two lines", 2, Stocktake.Adjustments[0].Lines.Count);
		}

		#endregion

		#region TestCloseLines_FindUnfinalizedAdjustment

		public void TestCloseLines_FindUnfinalizedAdjustment()
		{
			var warehouse = Helper.CreateWarehouse("Test Warehouse", "TestRow1", 6, 1);
			var locations = warehouse.Rows.Single(r => r.WR_Name == "TestRow1").Locations;
			var organization1 = Helper.CreateClient();
			var organization2 = Helper.CreateClient();
			var organization3 = Helper.CreateClient();

			var part = Helper.CreateProduct(organization1, "TestProd1");
			Helper.CreateProductClientRelationShip(organization2, part);
			Helper.CreateProductClientRelationShip(organization3, part);

			var stocktake = GetNewBusinessObject();
			stocktake.WS_WW_Whs = warehouse.PK;
			stocktake.WS_OH_Client = organization1.PK;
			stocktake.WS_StocktakeNumber = "T00000001";

			stocktake.Lines.AddNew().WU_OP = part.PK;
			stocktake.Lines[0].WU_WL = locations[0].PK;
			stocktake.Lines[0].WU_LastCount = 8;
			stocktake.Lines[0].WU_SystemUnits = 10;
			stocktake.Lines[0].WU_OH_Client = organization1.PK;
			stocktake.CloseLines(new WhsStocktakeLine[] { stocktake.Lines[0] });

			stocktake.Lines.AddNew().WU_OP = part.PK;
			stocktake.Lines[1].WU_WL = locations[1].PK;
			stocktake.Lines[1].WU_LastCount = 6;
			stocktake.Lines[1].WU_SystemUnits = 8;
			stocktake.WS_OH_Client = organization2.PK;
			stocktake.Lines[1].WU_OH_Client = organization2.PK;
			stocktake.CloseLines(new WhsStocktakeLine[] { stocktake.Lines[1] });

			AssertEquals("Stocktake should have two Adjustments", 2, stocktake.Adjustments.Count);
			AssertEquals("First Stocktake Adjustment should have only one line", 1,
				stocktake.Adjustments[0].Lines.Count);

			stocktake.Lines.AddNew().WU_OP = part.PK;
			stocktake.Lines[2].WU_WL = locations[2].PK;
			stocktake.Lines[2].WU_LastCount = 5;
			stocktake.Lines[2].WU_SystemUnits = 6;
			stocktake.WS_OH_Client = organization1.PK;
			stocktake.Lines[2].WU_OH_Client = organization1.PK;
			stocktake.CloseLines(new WhsStocktakeLine[] { stocktake.Lines[2] });

			AssertEquals("Stocktake should still have only two Adjustments", 2, stocktake.Adjustments.Count);
			AssertEquals("First Stocktake Adjustment should have two lines now", 2,
				stocktake.Adjustments[0].Lines.Count);

			stocktake.Lines.AddNew().WU_OP = part.PK;
			stocktake.Lines[3].WU_WL = locations[3].PK;
			stocktake.Lines[3].WU_LastCount = 4;
			stocktake.Lines[3].WU_SystemUnits = 5;
			stocktake.WS_OH_Client = organization3.PK;
			stocktake.Lines[3].WU_OH_Client = organization3.PK;
			stocktake.CloseLines(new WhsStocktakeLine[] { stocktake.Lines[3] });
			AssertEquals("Stocktake should have three Adjustments", 3, stocktake.Adjustments.Count);
			stocktake.Adjustments[2].WD_DocketStatus = DocketStatus.Codes.Finalised;
			stocktake.Adjustments[2].WD_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals("Stocktake third Adjustment should be now Finalised", true,
				stocktake.Adjustments[2].IsFinalised);

			stocktake.Lines.AddNew().WU_OP = part.PK;
			stocktake.Lines[4].WU_WL = locations[4].PK;
			stocktake.Lines[4].WU_LastCount = 2;
			stocktake.Lines[4].WU_SystemUnits = 4;
			stocktake.WS_OH_Client = organization1.PK;
			stocktake.Lines[4].WU_OH_Client = organization1.PK;
			stocktake.CloseLines(new WhsStocktakeLine[] { stocktake.Lines[4] });
			AssertEquals("Stocktake should still have three Adjustments", 3, stocktake.Adjustments.Count);
			AssertEquals("Stocktake first Adjustment should have three lines now", 3,
				stocktake.Adjustments[0].Lines.Count);

			stocktake.Adjustments[0].WD_DocketStatus = DocketStatus.Codes.Finalised;
			stocktake.Adjustments[0].WD_FinalisedDate = ZDateTimeOffset.Now;
			stocktake.Lines.AddNew().WU_OP = part.PK;
			stocktake.Lines[5].WU_WL = locations[5].PK;
			stocktake.Lines[5].WU_LastCount = 1;
			stocktake.Lines[5].WU_SystemUnits = 2;
			stocktake.WS_OH_Client = organization1.PK;
			stocktake.Lines[5].WU_OH_Client = organization1.PK;
			stocktake.CloseLines(new WhsStocktakeLine[] { stocktake.Lines[5] });
			AssertEquals("Stocktake should now have four Adjustments", 4, stocktake.Adjustments.Count);
			AssertEquals("Stocktake first Adjustment should still have only three lines", 3,
				stocktake.Adjustments[0].Lines.Count);
		}

		#endregion

		#region TestCloseLines_CreatesNewAdjustmentIfPreviousAdjustmentFinalised

		public void TestCloseLines_CreatesNewAdjustmentIfPreviousAdjustmentFinalised()
		{
			var warehouse = Helper.CreateWarehouse("Test Warehouse", "TestRow1", 2, 1);
			var organization = Helper.CreateClient();
			var part = Helper.CreateProduct(organization, "TestProd1");
			Factory.Save();

			var stocktake = GetNewBusinessObject();
			stocktake.WS_WW_Whs = warehouse.PK;
			stocktake.WS_OH_Client = organization.PK;
			stocktake.WS_StocktakeNumber = "T00000004";

			stocktake.Lines.AddNew().WU_OP = part.PK;
			stocktake.Lines[0].WU_WL = warehouse.FindLocation("TestRow1-1").PK;
			stocktake.Lines[0].WU_LastCount = 5;
			stocktake.Lines[0].WU_SystemUnits = 10;
			stocktake.Lines[0].WU_OH_Client = organization.PK;
			stocktake.CloseLines(new WhsStocktakeLine[] { stocktake.Lines[0] });

			stocktake.Lines.AddNew().WU_OP = part.PK;
			stocktake.Lines[1].WU_WL = warehouse.FindLocation("TestRow1-2").PK;
			stocktake.Lines[1].WU_LastCount = 3;
			stocktake.Lines[1].WU_SystemUnits = 5;
			stocktake.Lines[1].WU_OH_Client = organization.PK;
			stocktake.Adjustments[0].WD_DocketStatus = DocketStatus.Codes.Finalised;
			stocktake.Adjustments[0].WD_FinalisedDate = ZDateTimeOffset.Now;
			stocktake.CloseLines(new WhsStocktakeLine[] { stocktake.Lines[1] });

			AssertEquals("Stocktake should have two Adjustment", 2, stocktake.Adjustments.Count);
		}

		#endregion

		#region TestCloseLines_CreatesOneAdjustmentPerClient

		public void TestCloseLines_CreatesOneAdjustmentPerClient()
		{
			var warehouse = Helper.CreateWarehouse("Test Warehouse", "TestRow1", 2, 1);
			var organization = Helper.CreateClient();
			var part = Helper.CreateProduct(organization, "TestProd1");
			Factory.Save();

			Stocktake.WS_WW_Whs = warehouse.PK;
			Stocktake.WS_OH_Client = organization.PK;
			Stocktake.WS_StocktakeNumber = "T00000005";

			Stocktake.Lines.AddNew().WU_OP = part.PK;
			Stocktake.Lines[0].WU_WL = warehouse.FindLocation("TestRow1-1").PK;
			Stocktake.Lines[0].WU_LastCount = 5;
			Stocktake.Lines[0].WU_SystemUnits = 10;
			Stocktake.Lines[0].WU_OH_Client = organization.PK;
			Stocktake.CloseLines(new WhsStocktakeLine[] { Stocktake.Lines[0] });

			Stocktake.Lines.AddNew().WU_OP = part.PK;
			Stocktake.Lines[1].WU_WL = warehouse.FindLocation("TestRow1-2").PK;
			Stocktake.Lines[1].WU_LastCount = 3;
			Stocktake.Lines[1].WU_SystemUnits = 5;
			organization = Helper.CreateClient();
			Helper.CreateProductClientRelationShip(organization, part);

			Stocktake.WS_OH_Client = organization.PK;
			Stocktake.Lines[1].WU_OH_Client = organization.PK;
			Stocktake.CloseLines(new WhsStocktakeLine[] { Stocktake.Lines[1] });

			AssertEquals("Stocktake should have two Adjustments", 2, Stocktake.Adjustments.Count);
			AssertEquals(
				"CloseLines should create second Adjustment when the first Adjustment is for a different client", true,
				Stocktake.Adjustments[1] != null);
		}

		#endregion

		#region TestCloseLines_CreateAdjustment_PackTypeAndProductStockKeepingUnitAllEmpty

		public void TestCloseLines_CreateAdjustment_PackTypeAndProductStockKeepingUnitAllEmpty()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			data.Part1.OP_StockKeepingUnit = "";

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 10m,
				5m, 1, StocktakeLineStatus.Codes.Open);

			AssertEquals("Precondition: Product.StockKeepingUnit should be empty", ZString.Empty,
				data.Part1.OP_StockKeepingUnit);
			AssertEquals("Precondition: PackType in Line should be empty", ZString.Empty, line.WU_F3_NKPackType);

			stocktake.CloseLines(new WhsStocktakeLine[] { line });

			var adjustment = stocktake.Adjustments.Cast<WhsAdjustment>().Single();
			AssertEquals($"The PackType in Line should be '{Constants.PkgUnit.Unit}'", Constants.PkgUnit.Unit,
				adjustment.Lines.Single().WE_F3_NKPackType);
		}

		#endregion

		#region TestCloseLines_CreateAdjustment_PackTypeIsEmpty

		public void TestCloseLines_CreateAdjustment_PackTypeIsEmpty()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			data.Part1.OP_StockKeepingUnit = Constants.PkgUnit.Pallet;

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, 10m,
				5m, 1, StocktakeLineStatus.Codes.Open);
			line.WU_F3_NKPackType = "";

			AssertEquals("Precondition: Product.StockKeepingUnit should not be empty", Constants.PkgUnit.Pallet,
				data.Part1.OP_StockKeepingUnit);
			AssertEquals("Precondition: PackType in Line should be empty", ZString.Empty, line.WU_F3_NKPackType);

			stocktake.CloseLines(new WhsStocktakeLine[] { line });

			var adjustment = stocktake.Adjustments.Cast<WhsAdjustment>().Single();
			AssertEquals("The PackType in Line should use Product.StockKeepingUnit", Constants.PkgUnit.Pallet,
				adjustment.Lines.Single().WE_F3_NKPackType);
		}

		#endregion

		#region TestCloseLines_UpdatesFinalisedStatus

		public void TestCloseLines_UpdatesFinalisedStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation,
				StocktakeLineStatus.Codes.Open, StocktakeInventoryStatus.Codes.Available);
			Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part2, data.Whs1.DefaultLocation,
				StocktakeLineStatus.Codes.Open, StocktakeInventoryStatus.Codes.Available);
			Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation,
				StocktakeLineStatus.Codes.Open, StocktakeInventoryStatus.Codes.Damaged);
			//close last two lines and leave the first one open
			stocktake.CloseLines(new WhsStocktakeLine[] { stocktake.Lines[1], stocktake.Lines[2] });
			AssertEquals("Two of three lines are closed - stocktake status should not be Finalised", false,
				stocktake.IsFinalised);
			//all lines are closed now
			stocktake.CloseLines(new WhsStocktakeLine[] { stocktake.Lines[0] });
			AssertEquals("All lines are closed - stocktake status must be Finalised", true, stocktake.IsFinalised);
		}

		#endregion

		#region TestCloseLines_DateClosed

		[TestDate(2012, 1, 1)]
		public void TestCloseLines_DateClosed()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			var line1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation,
				StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			var line2 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part2, data.Whs1.DefaultLocation,
				StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);

			stocktake.CloseLines(new WhsStocktakeLine[] { line1 });
			AssertEquals("Since line1 has been closed now it should have closed line date set to today.", ZDateTime.Now,
				line1.WU_DateClosed);
			AssertEquals("Since line2 hasn't been closed, it should not have a closed datetime.", ZDateTime.Empty,
				line2.WU_DateClosed.Date);
		}

		#endregion

		#region TestCloseLines_AttributeNeutralProducts

		#region TestCloseLines_AttributeNeutralProducts

		public void TestCloseLines_AttributeNeutralProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			data.Part1.RelatedOrganisations[0].OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;
			// data.Part2 is an attribute specified product

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, true);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			var lineWithCountOne = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1,
				data.Whs1.DefaultLocation, 1m, 3m, 1, StocktakeLineStatus.Codes.Open, "A1", "A2", "A3");
			lineWithCountOne.WU_SerialNumber = WhsStocktake.AttributeNeutral;
			var lineWithCountTwo = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1,
				data.Whs1.DefaultLocation, 1m, 3m, 2, StocktakeLineStatus.Codes.Open, "A4", "A5", "A6");
			lineWithCountTwo.WU_SerialNumber = WhsStocktake.AttributeNeutral;
			var lineWithCountThree = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1,
				data.Whs1.DefaultLocation, 1m, 3m, 3, StocktakeLineStatus.Codes.Open, "A7", "A8", "A9");
			lineWithCountThree.WU_SerialNumber = WhsStocktake.AttributeNeutral;

			stocktake.CloseLines(new WhsStocktakeLine[] { lineWithCountOne, lineWithCountTwo, lineWithCountThree });
			AssertEquals("Pre-condition", 1, stocktake.Adjustments.Count);
			var sortedAdjustmentLines = stocktake.Adjustments[0].Lines.ToList();
			sortedAdjustmentLines.Sort((x, y) =>
				x.WE_PartAttrib1.CompareTo(y.WE_PartAttrib1) + x.WE_PartAttrib2.CompareTo(y.WE_PartAttrib2) +
				x.WE_PartAttrib3.CompareTo(y.WE_PartAttrib3));

			AssertEquals("Stocktake Adjustment should have six lines.", 6, stocktake.Adjustments[0].Lines.Count);

			AssertProductAttributes("A1", "A2", "A3", "", sortedAdjustmentLines[0].WE_PartAttrib1,
				sortedAdjustmentLines[0].WE_PartAttrib2, sortedAdjustmentLines[0].WE_PartAttrib3,
				sortedAdjustmentLines[0].WE_SerialNumber);
			AssertProductAttributes("A1", "A2", "A3", "", sortedAdjustmentLines[1].WE_PartAttrib1,
				sortedAdjustmentLines[1].WE_PartAttrib2, sortedAdjustmentLines[1].WE_PartAttrib3,
				sortedAdjustmentLines[1].WE_SerialNumber);
			AssertProductAttributes("A4", "A5", "A6", "", sortedAdjustmentLines[2].WE_PartAttrib1,
				sortedAdjustmentLines[2].WE_PartAttrib2, sortedAdjustmentLines[2].WE_PartAttrib3,
				sortedAdjustmentLines[2].WE_SerialNumber);
			AssertProductAttributes("A4", "A5", "A6", "", sortedAdjustmentLines[3].WE_PartAttrib1,
				sortedAdjustmentLines[3].WE_PartAttrib2, sortedAdjustmentLines[3].WE_PartAttrib3,
				sortedAdjustmentLines[3].WE_SerialNumber);
			AssertProductAttributes("A7", "A8", "A9", "", sortedAdjustmentLines[4].WE_PartAttrib1,
				sortedAdjustmentLines[4].WE_PartAttrib2, sortedAdjustmentLines[4].WE_PartAttrib3,
				sortedAdjustmentLines[4].WE_SerialNumber);
			AssertProductAttributes("A7", "A8", "A9", "", sortedAdjustmentLines[5].WE_PartAttrib1,
				sortedAdjustmentLines[5].WE_PartAttrib2, sortedAdjustmentLines[5].WE_PartAttrib3,
				sortedAdjustmentLines[5].WE_SerialNumber);
		}

		#endregion

		#region TestCloseLines_AttributeNeutralPartAttributeNonSerial

		public void TestCloseLines_AttributeNeutralPartAttributeNonSerial()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			data.Part1.RelatedOrganisations[0].OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			// data.Part2 is an attribute specified product

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, true, useSerialNumber: false);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			var lineWithCountOne = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1,
				data.Whs1.DefaultLocation, 1m, 3m, 1, StocktakeLineStatus.Codes.Open, "", "A1", "A2");
			var lineWithCountTwo = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1,
				data.Whs1.DefaultLocation, 1m, 3m, 2, StocktakeLineStatus.Codes.Open, "", "A3", "A4");
			var lineWithCountThree = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1,
				data.Whs1.DefaultLocation, 1m, 3m, 3, StocktakeLineStatus.Codes.Open, "", "A5", "A6");

			stocktake.CloseLines(new[] { lineWithCountOne, lineWithCountTwo, lineWithCountThree });
			AssertEquals("Pre-condition", 1, stocktake.Adjustments.Count);
			AssertEquals("Stocktake Adjustment should have three lines if Org isn't using Serial #s.", 3,
				stocktake.Adjustments[0].Lines.Count);

			stocktake.Adjustments[0].Lines
				.ApplySort(WhsDocketLineSchema.Constants.WE_PartAttrib2, ListSortDirection.Ascending);
			AssertProductAttributes("", "A1", "A2", "", stocktake.Adjustments[0].Lines[0].WE_PartAttrib1,
				stocktake.Adjustments[0].Lines[0].WE_PartAttrib2, stocktake.Adjustments[0].Lines[0].WE_PartAttrib3,
				stocktake.Adjustments[0].Lines[0].WE_SerialNumber);
			AssertProductAttributes("", "A3", "A4", "", stocktake.Adjustments[0].Lines[1].WE_PartAttrib1,
				stocktake.Adjustments[0].Lines[1].WE_PartAttrib2, stocktake.Adjustments[0].Lines[1].WE_PartAttrib3,
				stocktake.Adjustments[0].Lines[1].WE_SerialNumber);
			AssertProductAttributes("", "A5", "A6", "", stocktake.Adjustments[0].Lines[2].WE_PartAttrib1,
				stocktake.Adjustments[0].Lines[2].WE_PartAttrib2, stocktake.Adjustments[0].Lines[2].WE_PartAttrib3,
				stocktake.Adjustments[0].Lines[2].WE_SerialNumber);
		}

		#endregion

		void AssertProductAttributes(string expA1, string expA2, string expA3, string expSer, string a1, string a2,
			string a3, string ser)
		{
			AssertEquals("Attribute 1 should be same in adjustment line.", expA1, a1);
			AssertEquals("Attribute 2 should be same in adjustment line.", expA2, a2);
			AssertEquals("Attribute 3 should be same in adjustment line.", expA3, a3);
			AssertEquals("Serial Attribute should be same in adjustment line.", expSer, ser);
		}

		#endregion

		#region TestCloseLines_AttributeNeutralProducts_ForManuallyEnteredLines

		public void TestCloseLines_AttributeNeutralProducts_ForManuallyEnteredLines_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var relation = data.Part1.RelatedOrganisations.Cast<OrgPartRelation>().Single();
			relation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			relation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "");
			receiveLine1.WE_SerialNumber = "SN1";
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			stocktake.Load(Notify);
			var loadedStocktakeLine = stocktake.Lines.Single();
			var manualStocktakeLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1,
				data.Whs1.DefaultLocation, 1m, 1, StocktakeLineStatus.Codes.Open, true);
			manualStocktakeLine.WU_SerialNumber = "SN2";

			stocktake.CloseLines(new[] { loadedStocktakeLine, manualStocktakeLine });
			var adjustment = stocktake.Adjustments.Cast<WhsAdjustment>().Single();
			AssertEquals("Both loaded and manually added stocktake lines should generate adjustment lines.", 2,
				adjustment.Lines.Count);
			adjustment.Lines.Single(l =>
				l.WE_PartAttrib1 == "" && l.WE_PartAttrib2 == "" && l.WE_PartAttrib3 == "" && l.WE_SerialNumber == "");
			adjustment.Lines.Single(l =>
				l.WE_PartAttrib1 == "" && l.WE_PartAttrib2 == "" && l.WE_PartAttrib3 == "" &&
				l.WE_SerialNumber == "SN2");
		}

		#endregion

		#region TestCloseLines_WithPerPackageQtyAndPackageGroupID

		public void TestCloseLines_WithPerPackageQtyAndPackageGroupID()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeLine =
				Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, "", true);
			stocktakeLine.WU_LastCount = 10m;
			stocktakeLine.WU_PackageGroupId = "ABC";
			stocktakeLine.WU_PerPackageQty = 2m;
			AssertEquals(true, stocktake.CloseLines(new[] { stocktakeLine }));

			var adjustment = stocktake.Adjustments[0];
			var adjustmentLine = adjustment.Lines[0];
			AssertEquals(10m, adjustmentLine.WE_TransactionQuantity);
			AssertEquals("ABC", adjustmentLine.WE_PackageGroupId);
			AssertEquals(2m, adjustmentLine.WE_PerPackageQty);
		}

		#endregion

		#region TestCloseLines_WithNoInventory

		public void TestCloseLines_WithNoInventory()
		{
			var whs = Helper.CreateWarehouse("AAAA");
			var row = Helper.CreateRow(whs, "A");
			var org = Helper.CreateClient();
			var prod = Helper.CreateProduct(org, "AAA");
			CloseLine(whs, org, prod, 10, 0);
		}

		#endregion

		#region TestCloseLines_LessInventory

		public void TestCloseLines_LessInventory()
		{
			var whs = Helper.CreateWarehouse("AAAA");
			var row = Helper.CreateRow(whs, "A");
			var org = Helper.CreateClient();
			var prod = Helper.CreateProduct(org, "AAA");
			CloseLine(whs, org, prod, 200, 100);
		}

		#endregion

		#region TestCloseLines_MoreInventory

		public void TestCloseLines_MoreInventory()
		{
			var whs = Helper.CreateWarehouse("AAAA");
			var row = Helper.CreateRow(whs, "A");
			var org = Helper.CreateClient();
			var prod = Helper.CreateProduct(org, "AAA");
			CloseLine(whs, org, prod, 100, 200);
		}

		#endregion

		#region TestCloseLines_NewCountColumn

		public void TestCloseLines_NewCountColumn()
		{
			// Setup test data

			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locations[0],
				string.Empty);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, locations[1],
				string.Empty);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, locations[2],
				string.Empty);

			// Create stocktake and two lines

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			//stocktake.WS_NumberOfLastCounts = 1; // First new count column is used

			var lineWithNewCountOne = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, locations[0], 5,
				1, StocktakeLineStatus.Codes.Open);
			var lineWithNewCountTwo = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, locations[1], 4,
				2, StocktakeLineStatus.Codes.Open);
			var lineWithNewCountThree = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, locations[2], 3,
				3, StocktakeLineStatus.Codes.Open);
			lineWithNewCountOne.WU_InventoryStatus = InventoryStatus.Codes.Available;
			lineWithNewCountTwo.WU_InventoryStatus = InventoryStatus.Codes.Available;
			lineWithNewCountThree.WU_InventoryStatus = InventoryStatus.Codes.Available;

			lineWithNewCountOne.WU_SystemUnits = 10;
			lineWithNewCountTwo.WU_SystemUnits = 10;
			lineWithNewCountThree.WU_SystemUnits = 10;
			lineWithNewCountOne.WU_LastCount = 5;

			// Test adjustments

			Factory.Save(); // Save changes

			stocktake.CloseLines(new WhsStocktakeLine[] { lineWithNewCountOne });

			AssertNotNull("Adjustment should be created", stocktake.UnfinalizedAdjustmentForTesting);
			var adjustmentLineForNewCountOne = stocktake.UnfinalizedAdjustmentForTesting.Lines[0];
			AssertEquals("Adjustment line units should be calculated against the new count one column.", (decimal)-5,
				adjustmentLineForNewCountOne.WE_TransactionQuantity);
			AssertEquals("Line status must be closed", true, lineWithNewCountOne.IsClosed);
			adjustmentLineForNewCountOne.RunPreSaveValidation();

			lineWithNewCountTwo.WU_Count2 = 4;
			lineWithNewCountTwo.WU_TotalCounts = 2;

			Factory.Save(); // Save changes

			stocktake.CloseLines(new WhsStocktakeLine[] { lineWithNewCountTwo });

			var adjustmentLineForNewCountTwo = stocktake.UnfinalizedAdjustmentForTesting.Lines[1];
			AssertEquals("Adjustment line units should be calculated against the new count two column.", (decimal)-6,
				adjustmentLineForNewCountTwo.WE_TransactionQuantity);
			AssertEquals("Line status must be closed", true, lineWithNewCountTwo.IsClosed);
			adjustmentLineForNewCountTwo.RunPreSaveValidation();

			lineWithNewCountThree.WU_Count3 = 3;
			lineWithNewCountThree.WU_TotalCounts = 3;

			Factory.Save(); // Save changes

			stocktake.CloseLines(new WhsStocktakeLine[] { lineWithNewCountThree });

			var adjustmentLineForNewCountThree = stocktake.UnfinalizedAdjustmentForTesting.Lines[2];
			AssertEquals("Adjustment line units should be calculated against the new count three column.", (decimal)-7,
				adjustmentLineForNewCountThree.WE_TransactionQuantity);
			AssertEquals("Line status must be closed", true, lineWithNewCountThree.IsClosed);
		}

		#endregion

		#region TestCloseLines_PackTypesArePassedToAdjustment

		public void TestCloseLines_PackTypesArePassedToAdjustment()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_StockKeepingUnit = Constants.PkgUnit.Box;
			Helper.CreateProductUnit(data.Part2, Constants.PkgUnit.Box, Constants.PkgUnit.Pallet, 20m);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var stocktakeLine1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1,
				data.Whs1.DefaultLocation, 10m, 3m, ZDateTime.Today, 1, StocktakeLineStatus.Codes.Open);
			var stocktakeLine2 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part2,
				data.Whs1.DefaultLocation, 10m, 13m, ZDateTime.Today, 1, StocktakeLineStatus.Codes.Open);
			stocktakeLine2.WU_F3_NKPackType = Constants.PkgUnit.Pallet;
			stocktake.CloseLines(stocktake.Lines.ToArray());
			AssertEquals("Precondition", true, stocktakeLine1.IsClosed);
			AssertEquals("Precondition", true, stocktakeLine2.IsClosed);

			var adjustment = stocktake.Adjustments.Cast<WhsAdjustment>().Single();
			AssertEquals("1 adjustment line should be created for each stocktake line.", 2, adjustment.Lines.Count);
			var adjustmentLine1 =
				adjustment.Lines.Single(l => l.WE_OP == data.Part1.PK && l.WE_TransactionQuantity == -7m);
			var adjustmentLine2 =
				adjustment.Lines.Single(l => l.WE_OP == data.Part2.PK && l.WE_TransactionQuantity == 3m);
			AssertEquals("If stocktake line have no PackType, then Stock Keeping Unit of the product should be used.",
				Constants.PkgUnit.Box, adjustmentLine1.WE_F3_NKPackType);
			AssertEquals("If stocktake line have PackType specified, it should be passed to the Adjustment.",
				Constants.PkgUnit.Pallet, adjustmentLine2.WE_F3_NKPackType);
		}

		#endregion

		#region TestCloseLines_LocationOverflowProcessConcurrentIncreaseAndDecrease

		public void TestCloseLines_LocationOverflowProcessConcurrentIncreaseAndDecrease()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			data.Whs1.FindLocation("A").WLV_MaxQuantity = 100m;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m,
				data.Whs1.FindLocation("A-1-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 30m,
				data.Whs1.FindLocation("A-1-1"), "");
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Whs1.FindLocation("A"));
			stocktake.WS_WW_Whs = data.Whs1.PK;
			Factory.Save();

			var line1 = stocktake.Lines.AddNew();
			var line2 = stocktake.Lines.AddNew();

			line1.WU_OP = data.Part1.PK;
			line1.WU_WL = data.Whs1.FindLocation("A").PK;
			line1.WU_SystemUnits = 30m;
			line1.WU_LastCount = 10m;

			line2.WU_OP = data.Part2.PK;
			line2.WU_WL = data.Whs1.FindLocation("A").PK;
			line2.WU_SystemUnits = 30m;
			line2.WU_LastCount = 75m;

			stocktake.CloseLines(new[] { line1, line2 });
			AssertEquals("Precondition: stocktake has one adjustment", 1, stocktake.Adjustments.Count);
			stocktake.Adjustments[0].RunPreSaveValidation(); //creates picklines and allocates stock
			AssertNoExceptionThrown(
				"Should be able to process concurrent increase and decrease ajustments that do not overflow location.",
				Factory.Save);
		}

		#endregion

		#region CloseLine

		void CloseLine(WhsWarehouse whs, OrgHeader org, OrgSupplierPart prod, ZDecimal count, ZDecimal invUnits)
		{
			whs.Rows[0].WR_Columns = 1;
			whs.Rows[0].WR_Levels = 1;
			Factory.Save();

			var stocktake = GetNewBusinessObject();
			stocktake.WS_WW_Whs = whs.PK;
			stocktake.WS_OH_Client = org.PK;

			var line = stocktake.Lines.AddNew();
			line.WU_WL = whs.Rows[0].Locations[0].PK;
			line.WU_OP = prod.PK;
			line.WU_LastCount = count;
			line.WU_SystemUnits = invUnits;

			var lines = new[] { line };

			// create some inventory
			if (invUnits > 0)
			{
				var inv = Factory.New<WhsInventoryView>();
				inv.WI_OH_Client = org.PK;
				inv.WI_OP = prod.PK;
				inv.WI_TotalUnits = invUnits;
				inv.WI_WL = whs.Rows[0].Locations[0].PK;
			}

			// close the line
			stocktake.CloseLines(lines);

			Assert("Adjustment should be created", stocktake.UnfinalizedAdjustmentForTesting != null);
			var adjLine = stocktake.UnfinalizedAdjustmentForTesting.Lines[0];
			AssertEquals("Adjustment line units", count - invUnits, adjLine.WE_TransactionQuantity);
			Assert("Line status must be closed", line.IsClosed);
		}

		#endregion

		#endregion

		#region Test Adjustments

		#region TestAdjustments

		public void TestAdjustments()
		{
			Stocktake.WS_StocktakeNumber = "T000001";
			AssertEquals("Stocktake should have 0 adjustments", 0, Stocktake.Adjustments.Count);

			var adjustment = Factory.New<WhsAdjustment>();
			adjustment.WD_ExternalReference = "STOCKTAKE T000001-A";
			AssertEquals("Stocktake should have 1 adjustment", 1, Stocktake.Adjustments.Count);
		}

		#endregion

		#region LoadAdjustments

		public void TestLoadAdjustments()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);

			stocktake.WS_StocktakeNumber = "T00000007";
			stocktake.Lines.AddNew();
			stocktake.Lines[0].WU_OH_Client = data.Org1.PK;
			stocktake.Lines[0].WU_WL = data.Whs1.DefaultLocation.PK;
			stocktake.Lines[0].WU_OP = Helper.CreateProduct(data.Org1, "Test Prod1").PK;
			stocktake.Lines[0].WU_SystemUnits = 10;
			stocktake.Lines[0].WU_LastCount = 15;

			stocktake.CloseLines(new WhsStocktakeLine[] { stocktake.Lines[0] });
			AssertEquals("Stocktake Adjustment should be created", 1, stocktake.Adjustments.Count);
			AssertEquals("Stocktake Adjustment should have one line", 1, stocktake.Adjustments[0].Lines.Count);

			stocktake = Factory.New<WhsStocktake>();
			stocktake.WS_StocktakeNumber = "T00000007";
			AssertEquals("Stocktake should load one Adjustment", 1, stocktake.Adjustments.Count);
			AssertEquals("Stocktake Adjustment should have one line", 1, stocktake.Adjustments[0].Lines.Count);
		}

		#endregion

		#region TestLoadAdjustmentsWithInventoryStatus

		public void TestLoadAdjustmentsWithInventoryStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);

			var lineWithStatusAVL = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1,
				data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, StocktakeInventoryStatus.Codes.Available);
			var lineWithStatusHEL = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1,
				data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, StocktakeInventoryStatus.Codes.Held);
			var lineWithStatusDMG = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1,
				data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, StocktakeInventoryStatus.Codes.Damaged);
			lineWithStatusAVL.WU_TotalCounts = 1;
			lineWithStatusHEL.WU_TotalCounts = 1;
			lineWithStatusDMG.WU_TotalCounts = 1;
			lineWithStatusAVL.CurrentCount = 1;
			lineWithStatusHEL.CurrentCount = 1;
			lineWithStatusDMG.CurrentCount = 1;

			stocktake.CloseLines(new WhsStocktakeLine[] { stocktake.Lines[0], stocktake.Lines[1], stocktake.Lines[2] });
			AssertEquals("Pre-condition", 1, stocktake.Adjustments.Count);
			AssertEquals("Stocktake Adjustment should have three lines", 3, stocktake.Adjustments[0].Lines.Count);

			AssertEquals(InventoryStatus.Codes.Available, stocktake.Adjustments[0].Lines[0].WE_OriginalInventoryStatus);
			AssertEquals(InventoryStatus.Codes.Available, stocktake.Adjustments[0].Lines[0].WE_CurrentInventoryStatus);
			AssertEquals(InventoryStatus.Codes.Held, stocktake.Adjustments[0].Lines[1].WE_OriginalInventoryStatus);
			AssertEquals(InventoryStatus.Codes.Held, stocktake.Adjustments[0].Lines[1].WE_CurrentInventoryStatus);
			AssertEquals(InventoryStatus.Codes.Held, stocktake.Adjustments[0].Lines[2].WE_OriginalInventoryStatus);
			AssertEquals(InventoryStatus.Codes.Held, stocktake.Adjustments[0].Lines[2].WE_CurrentInventoryStatus);
			AssertEquals(InventoryHoldCodes.Codes.Damaged,
				stocktake.Adjustments[0].Lines[2].WE_WHC_NKOriginalInventoryHeldCode);
		}

		#endregion

		#endregion

		#region Load

		#region TestLoad

		public void TestLoad()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 4);
			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var part4 = Helper.CreateProduct(data.Org1, "P4");

			var receive_Finalised = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			CreateWhsReceiveInventoryLine(receive_Finalised, data.Part1, 10m, data.Whs1.DefaultLocation,
				CodeLists.InventoryStatus.Codes.Putaway);
			CreateWhsReceiveInventoryLine(receive_Finalised, data.Part2, 10m, data.Whs1.DefaultLocation,
				CodeLists.InventoryStatus.Codes.Putaway);
			CreateWhsReceiveInventoryLine(receive_Finalised, data.Part2, 20m, data.Whs1.DefaultLocation,
				CodeLists.InventoryStatus.Codes.Putaway);
			CreateWhsReceiveInventoryLine(receive_Finalised, part3, 10m, data.Whs1.DefaultLocation,
				CodeLists.InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Held);
			CreateWhsReceiveInventoryLine(receive_Finalised, part3, 20m, data.Whs1.DefaultLocation,
				CodeLists.InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
			receive_Finalised.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive_Finalised);

			var receive_UnFinalised = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			CreateWhsReceiveInventoryLine(receive_UnFinalised, part4, 10m, data.Whs1.DefaultLocation,
				CodeLists.InventoryStatus.Codes.Putaway);
			CreateWhsReceiveInventoryLine(receive_UnFinalised, part4, 20m, data.Whs1.DefaultLocation,
				CodeLists.InventoryStatus.Codes.Putaway, InventoryHoldCodes.Codes.Damaged);
			CreateWhsReceiveInventoryLine(receive_UnFinalised, part4, 30m, data.Whs1.DefaultLocation,
				CodeLists.InventoryStatus.Codes.Putaway, InventoryHoldCodes.Codes.Damaged);
			Factory.Save();

			// committ some inventory to make sure Available Units are calculated correctly
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);

			Factory.Save();

			// create and load stocktake
			Stocktake.WS_OH_Client = data.Org1.PK;
			Stocktake.WS_WW_Whs = data.Whs1.PK;
			Stocktake.Load();

			AssertEquals("Stocktake line count", 4, Stocktake.Lines.Count);
			AssertEquals("Stocktakeline1 system units", 10.0m,
				FindLine(Stocktake, data.Part1, CodeLists.StocktakeInventoryStatus.Codes.Available)
					.WU_SystemUnits); // During the finalisation of the docket Putaway status is changed to Available
			AssertEquals("Stocktakeline2 system units", 30.0m,
				FindLine(Stocktake, data.Part2, CodeLists.StocktakeInventoryStatus.Codes.Available)
					.WU_SystemUnits); // During the finalisation of the docket Putaway status is changed to Available
			AssertEquals("Stocktakeline3 system units", 10.0m,
				FindLine(Stocktake, part3, CodeLists.StocktakeInventoryStatus.Codes.Held).WU_SystemUnits);
			AssertEquals("Stocktakeline3 system units", 20.0m,
				FindLine(Stocktake, part3, CodeLists.StocktakeInventoryStatus.Codes.Damaged).WU_SystemUnits);

			var inventory = receive_Finalised.Inventory[FindInv(receive_Finalised.Inventory, data.Part2)];
			inventory.WI_TotalUnits = 15m;
			inventory.WI_InDocketLineUnits = 15m;
			inventory.InDocketLine.WE_TransactionQuantity = inventory.WI_InDocketLineUnits;

			// ensure system units retain past snapshot
			AssertEquals("Stocktakeline1 system units", 10.0m,
				FindLine(Stocktake, data.Part1, CodeLists.StocktakeInventoryStatus.Codes.Available)
					.WU_SystemUnits); // During the finalisation of the docket Putaway status is changed to Available
			AssertEquals("Stocktakeline2 system units", 30.0m,
				FindLine(Stocktake, data.Part2, CodeLists.StocktakeInventoryStatus.Codes.Available)
					.WU_SystemUnits); // During the finalisation of the docket Putaway status is changed to Available
			AssertEquals("Stocktakeline3 system units", 10.0m,
				FindLine(Stocktake, part3, CodeLists.StocktakeInventoryStatus.Codes.Held).WU_SystemUnits);
			AssertEquals("Stocktakeline3 system units", 20.0m,
				FindLine(Stocktake, part3, CodeLists.StocktakeInventoryStatus.Codes.Damaged).WU_SystemUnits);

			var commodity1 = Factory.NewWithValidTestData<RefCommodityCode>();
			var commodity2 = Factory.NewWithValidTestData<RefCommodityCode>();
			data.Part1.OP_RH_NKCommodityCode = commodity1.RH_Code;
			data.Part2.OP_RH_NKCommodityCode = commodity2.RH_Code;

			Factory.Save();

			// create and load stocktake by CommodityCode
			Stocktake = GetNewBusinessObject();
			Stocktake.WS_WW_Whs = data.Whs1.PK;
			Stocktake.WS_OH_Client = data.Org1.PK;
			Stocktake.WS_RH_NKCommodityCode = commodity1.RH_Code;
			Stocktake.Load();

			AssertEquals("StocktakeLine1 system units", 10.0m,
				FindLine(Stocktake, data.Part1, CodeLists.InventoryStatus.Codes.Available).WU_SystemUnits);
		}

		#endregion

		#region TestLoadWithABCCategory

		public void TestLoadWithABCCategory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 4);
			var part3 = Helper.CreateProduct(data.Org1, "P5");
			var paramsByWhsAndClient = Factory.New<WhsProductParamsByWhsAndClient>();
			paramsByWhsAndClient.W3_WW = data.Whs1.PK;
			paramsByWhsAndClient.W3_OH = data.Org1.PK;
			paramsByWhsAndClient.W3_OP = part3.PK;

			var analysisDate = ZDateTimeOffset.Today;

			var abcCategory1 = Factory.New<WhsABCCategory>();
			abcCategory1.WJ_OP_Product = paramsByWhsAndClient.W3_OP;
			abcCategory1.WJ_OH_Client = paramsByWhsAndClient.W3_OH;
			abcCategory1.WJ_WW_Warehouse = paramsByWhsAndClient.W3_WW;
			abcCategory1.WJ_Category = "XXX";
			abcCategory1.WJ_AnalysisDateFrom = analysisDate.AddDays(-10);
			abcCategory1.WJ_AnalysisDateTo = analysisDate.AddDays(-10);

			var abcCategory2 = Factory.New<WhsABCCategory>();
			abcCategory2.WJ_OP_Product = paramsByWhsAndClient.W3_OP;
			abcCategory2.WJ_OH_Client = paramsByWhsAndClient.W3_OH;
			abcCategory2.WJ_WW_Warehouse = Factory.NewWithValidTestData<WhsWarehouse>().PK;
			abcCategory2.WJ_Category = "DEF";
			abcCategory2.WJ_AnalysisDateFrom = analysisDate;
			abcCategory2.WJ_AnalysisDateTo = analysisDate;

			var abcCategory3 = Factory.New<WhsABCCategory>();
			abcCategory3.WJ_OP_Product = paramsByWhsAndClient.W3_OP;
			abcCategory3.WJ_OH_Client = paramsByWhsAndClient.W3_OH;
			abcCategory3.WJ_WW_Warehouse = paramsByWhsAndClient.W3_WW;
			abcCategory3.WJ_Category = "ABC";
			abcCategory3.WJ_AnalysisDateFrom = analysisDate;
			abcCategory3.WJ_AnalysisDateTo = analysisDate;

			var receive_Finalised = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			CreateWhsReceiveInventoryLine(receive_Finalised, data.Part1, 10m, data.Whs1.DefaultLocation,
				InventoryStatus.Codes.Putaway);
			CreateWhsReceiveInventoryLine(receive_Finalised, data.Part2, 20m, data.Whs1.DefaultLocation,
				InventoryStatus.Codes.Putaway);
			CreateWhsReceiveInventoryLine(receive_Finalised, part3, 30m, data.Whs1.DefaultLocation,
				InventoryStatus.Codes.Putaway);
			receive_Finalised.FinaliseDocket();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);

			Factory.Save();

			Stocktake.WS_OH_Client = data.Org1.PK;
			Stocktake.WS_WW_Whs = data.Whs1.PK;
			Stocktake.WS_ABCAnalysisCategory = "ABC";
			Stocktake.Load();

			AssertEquals("Only Line with ABC Category was loaded", 1, Stocktake.Lines.Count);
			AssertEquals("StocktakeLine1 system units", 30.0m,
				FindLine(Stocktake, part3, CodeLists.InventoryStatus.Codes.Available).WU_SystemUnits);
		}

		#endregion

		#region TestLoadEmptyLocation

		public void TestLoadEmptyLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 4);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m); // Fill one location
			var emptyWhs = Helper.CreateWarehouse("EmptyWareHouse", "Row", 4, 5);

			var packingStationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			AddPackingLocation(data.Whs1, "CC", packingStationLocationType.PK);
			AddPackingLocation(emptyWhs, "CC", packingStationLocationType.PK);

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Consolidation", false, 0, LocationClasses.Codes.CON);
			AddPackingLocation(data.Whs1, "DD", packingConsolidationLocationType.PK);
			AddPackingLocation(emptyWhs, "DD", packingConsolidationLocationType.PK);
			Factory.Save();

			var totalLocations = 4 * 5; // cols * levels (dockdoor + packing station is excluded)
			AssertStocktakeLines(emptyWhs, 0, CountEmptyLocationCategory.Codes.ExcludeEmptyLocations); // Default
			AssertStocktakeLines(emptyWhs, totalLocations, CountEmptyLocationCategory.Codes.IncludeEmptyLocations);
			AssertStocktakeLines(emptyWhs, totalLocations, CountEmptyLocationCategory.Codes.OnlyCountEmptyLocations);

			var numberOfOccupiedLocation = 1;
			totalLocations = 3 * 4; // cols * levels (dockdoor + packing station is excluded)
			AssertStocktakeLines(data.Whs1, numberOfOccupiedLocation,
				CountEmptyLocationCategory.Codes.ExcludeEmptyLocations); // Default
			AssertStocktakeLines(data.Whs1, totalLocations, CountEmptyLocationCategory.Codes.IncludeEmptyLocations);
			AssertStocktakeLines(data.Whs1, totalLocations - numberOfOccupiedLocation,
				CountEmptyLocationCategory.Codes.OnlyCountEmptyLocations);

			data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations[2].WLV_LocationStatus = LocationStatus.Codes.Void;
			Factory.Save();
			AssertStocktakeLines(data.Whs1, totalLocations - numberOfOccupiedLocation - 1,
				CountEmptyLocationCategory.Codes.OnlyCountEmptyLocations);

			void AddPackingLocation(WhsWarehouse warehouse, string code, ZGuid locationTypePK)
			{
				var packingLocation = Helper.CreateRowAndGenerateLocations(warehouse, code, 1, 1).Locations[0];
				packingLocation.WLV_WLT_LocationType = locationTypePK;
			}
		}

		void AssertStocktakeLines(WhsWarehouse whs, int expectedLines, string countEmptyLocationsCategory)
		{
			var whsStocktake = Factory.New<WhsStocktake>();
			whsStocktake.WS_WW_Whs = whs.PK;
			whsStocktake.WS_CountEmptyLocationsCategory = countEmptyLocationsCategory;
			whsStocktake.Load();
			AssertEquals(expectedLines, whsStocktake.Lines.Count);
		}

		#endregion

		#region TestLoadWithPendingReceive_LocationNotContainsOtherInventory

		public void TestLoadWithPendingReceive_LocationNotContainsOtherInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			Factory.Save();

			AssertEquals("Precondition: Receive should not be finalised.", false, receive.IsFinalised);

			AssertStocktakeLines(data.Whs1, 0, CountEmptyLocationCategory.Codes.ExcludeEmptyLocations);
			// Locations: Inventory Status
			// A	    : EMP
			AssertStocktakeLines(data.Whs1, 1, CountEmptyLocationCategory.Codes.IncludeEmptyLocations);
			AssertStocktakeLines(data.Whs1, 1, CountEmptyLocationCategory.Codes.OnlyCountEmptyLocations);
		}

		#endregion

		#region TestLoadWithInTransitInventory_LocationContainsOtherInventory

		public void TestLoadWithInTransitInventory_LocationContainsOtherInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var destLocation = data.Whs1.FindLocation("A-2");
			var receive_Finalised = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			CreateWhsReceiveInventoryLine(receive_Finalised, data.Part1, 10m, data.Whs1.DefaultLocation,
				CodeLists.InventoryStatus.Codes.Putaway);
			CreateWhsReceiveInventoryLine(receive_Finalised, data.Part1, 20m, destLocation,
				CodeLists.InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Held);
			CreateWhsReceiveInventoryLine(receive_Finalised, data.Part2, 10m, data.Whs1.DefaultLocation,
				CodeLists.InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Held);
			CreateWhsReceiveInventoryLine(receive_Finalised, data.Part2, 20m, data.Whs1.DefaultLocation,
				CodeLists.InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
			receive_Finalised.FinaliseDocket();

			Factory.Save();

			AssertIsFinalisedPrecondition(receive_Finalised);

			// create and load stocktake
			var whsStocktake = Factory.New<WhsStocktake>();
			whsStocktake.WS_OH_Client = data.Org1.PK;
			whsStocktake.WS_WW_Whs = data.Whs1.PK;
			whsStocktake.Load();

			AssertEquals("Stocktake line count", 4, whsStocktake.Lines.Count);
			AssertEquals("Part1 Available inventory", 10.0m,
				FindLine(whsStocktake, data.Part1, CodeLists.StocktakeInventoryStatus.Codes.Available).WU_SystemUnits);
			AssertEquals("Part1 Held inventory", 20.0m,
				FindLine(whsStocktake, data.Part1, CodeLists.StocktakeInventoryStatus.Codes.Held).WU_SystemUnits);
			AssertEquals("Part2 Held inventory", 10.0m,
				FindLine(whsStocktake, data.Part2, CodeLists.StocktakeInventoryStatus.Codes.Held).WU_SystemUnits);
			AssertEquals("Part2 Damaged inventory", 20.0m,
				FindLine(whsStocktake, data.Part2, CodeLists.StocktakeInventoryStatus.Codes.Damaged).WU_SystemUnits);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine =
				Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, data.Whs1.DefaultLocation, destLocation);
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Precondition: In-Transit Inventory", InventoryStatus.Codes.InTransit,
				transferLine.WE_CurrentInventoryStatus);

			// create and load stocktake
			whsStocktake = Factory.New<WhsStocktake>();
			whsStocktake.WS_OH_Client = data.Org1.PK;
			whsStocktake.WS_WW_Whs = data.Whs1.PK;
			whsStocktake.Load();

			AssertEquals("Stocktake line count", 4, whsStocktake.Lines.Count);
			AssertEquals("Part1 Available inventory", 5.0m,
				FindLine(whsStocktake, data.Part1, CodeLists.StocktakeInventoryStatus.Codes.Available).WU_SystemUnits);
			AssertEquals("Part1 Held inventory", 20.0m,
				FindLine(whsStocktake, data.Part1, CodeLists.StocktakeInventoryStatus.Codes.Held).WU_SystemUnits);
			AssertEquals("Part2 Held inventory", 10.0m,
				FindLine(whsStocktake, data.Part2, CodeLists.StocktakeInventoryStatus.Codes.Held).WU_SystemUnits);
			AssertEquals("Part2 Damaged inventory", 20.0m,
				FindLine(whsStocktake, data.Part2, CodeLists.StocktakeInventoryStatus.Codes.Damaged).WU_SystemUnits);

			// Locations	       : Inventory Status
			// A-1 with Part1(AVL) : AVL
			// A-1 with Part2(HLD) : HLD
			// A-1 with Part2(DAM) : DAM
			// A-2                 : AVL
			AssertStocktakeLines(data.Whs1, 4, CountEmptyLocationCategory.Codes.IncludeEmptyLocations);
			AssertStocktakeLines(data.Whs1, 0, CountEmptyLocationCategory.Codes.OnlyCountEmptyLocations);
		}

		#endregion

		#region TestLoadWithInTransitInventory_LocationNotContainsOtherInventory

		public void TestLoadWithInTransitInventory_LocationNotContainsOtherInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "REC", data.Part1, 10m,
				data.Whs1.DefaultLocation, "");
			Factory.Save();

			// create and load stocktake
			var whsStocktake = Factory.New<WhsStocktake>();
			whsStocktake.WS_OH_Client = data.Org1.PK;
			whsStocktake.WS_WW_Whs = data.Whs1.PK;
			whsStocktake.Load();

			AssertEquals("Stocktake line count", 1, whsStocktake.Lines.Count);
			AssertEquals("Available inventory", 10.0m,
				FindLine(whsStocktake, data.Part1, CodeLists.StocktakeInventoryStatus.Codes.Available).WU_SystemUnits);

			var destLocation = data.Whs1.FindLocation("A-2");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine =
				Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, data.Whs1.DefaultLocation, destLocation);
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals(InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);

			AssertStocktakeLines(data.Whs1, 0, CountEmptyLocationCategory.Codes.ExcludeEmptyLocations);

			// Locations: Inventory Status
			// A-1	    : EMP
			// A-2	    : EMP
			AssertStocktakeLines(data.Whs1, 2, CountEmptyLocationCategory.Codes.IncludeEmptyLocations);
			AssertStocktakeLines(data.Whs1, 2, CountEmptyLocationCategory.Codes.OnlyCountEmptyLocations);
		}

		#endregion

		#region TestLoadWithUnfinalisedTransferButTransferLineFinalised

		public void TestLoadWithUnfinalisedTransferButTransferLineFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive_Finalised = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			CreateWhsReceiveInventoryLine(receive_Finalised, data.Part1, 10m, data.Whs1.DefaultLocation,
				CodeLists.InventoryStatus.Codes.Putaway);
			CreateWhsReceiveInventoryLine(receive_Finalised, data.Part2, 10m, data.Whs1.DefaultLocation,
				CodeLists.InventoryStatus.Codes.Putaway);
			receive_Finalised.FinaliseDocket();

			Factory.Save();

			AssertIsFinalisedPrecondition(receive_Finalised);

			// create and load stocktake
			var whsStocktake = Factory.New<WhsStocktake>();
			whsStocktake.WS_OH_Client = data.Org1.PK;
			whsStocktake.WS_WW_Whs = data.Whs1.PK;
			whsStocktake.Load();

			AssertEquals("Stocktake line count", 2, whsStocktake.Lines.Count);
			AssertEquals("Available inventory", 10.0m,
				FindLine(whsStocktake, data.Part1, CodeLists.StocktakeInventoryStatus.Codes.Available).WU_SystemUnits);
			AssertEquals("Available inventory", 10.0m,
				FindLine(whsStocktake, data.Part2, CodeLists.StocktakeInventoryStatus.Codes.Available).WU_SystemUnits);

			var destLocation = data.Whs1.FindLocation("A-2");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 =
				Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, data.Whs1.DefaultLocation, destLocation);
			var transferLine2 =
				Helper.CreateWhsTransferLine(transfer, data.Part2, 10m, data.Whs1.DefaultLocation, destLocation);
			transferLine1.FinaliseDocketLine();
			transferLine2.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			AssertEquals("Transfer is not finalised", false, transfer.IsFinalised);
			AssertEquals("TransferLine1 is finalised", true, transferLine1.IsFinalised);
			AssertEquals("TransferLine2 is not finalised", false, transferLine2.IsFinalised);

			// create and load stocktake
			whsStocktake = Factory.New<WhsStocktake>();
			whsStocktake.WS_OH_Client = data.Org1.PK;
			whsStocktake.WS_WW_Whs = data.Whs1.PK;
			whsStocktake.Load();

			AssertEquals("Stocktake line count", 2, whsStocktake.Lines.Count);

			var lineForPart1 = FindLine(whsStocktake, data.Part1, CodeLists.StocktakeInventoryStatus.Codes.Available);
			AssertEquals("Available inventory", 10.0m, lineForPart1.WU_SystemUnits);
			AssertEquals("Part1 already be transferred to location 'A-2'", "A-2", lineForPart1.LocationString);

			var lineForPart2 = FindLine(whsStocktake, data.Part2, CodeLists.StocktakeInventoryStatus.Codes.Available);
			AssertEquals("Available inventory", 10.0m, lineForPart2.WU_SystemUnits);
			AssertEquals("Part2 still in location 'A-1'", "A-1", lineForPart2.LocationString);

			// Locations(Product): Inventory Status
			// A-1(Part2)	     : AVL
			// A-2(Part1)	     : AVL
			AssertStocktakeLines(data.Whs1, 2, CountEmptyLocationCategory.Codes.IncludeEmptyLocations);
			AssertStocktakeLines(data.Whs1, 0, CountEmptyLocationCategory.Codes.OnlyCountEmptyLocations);
		}

		#endregion

		#region TestLoad_ByCycle

		public void TestLoad_ByCycle()
		{
			var data = new TestDataForInventory(Factory);

			data.CreateMultiWarehouseClientProductInventory();
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", Helper.CreateProduct(data.Org1, "P3"),
				100m);
			CargoWise.Database.TestFramework.ObjectModel.WhsDocketLine.DeleteInDB(Db.Connection, data.Line217.PK.ToGuid());
			Factory.Save();

			var param = data.Product1.ParamsByWhsAndClient.AddNew();
			param.W3_WW = data.Whs1.PK;
			param.W3_OH = data.Org1.PK;
			param.W3_StockTakeCycle = "MON";

			param = data.Product1.ParamsByWhsAndClient.AddNew();
			param.W3_WW = data.Whs1.PK;
			param.W3_OH = data.Org2.PK;
			param.W3_StockTakeCycle = "TUE";

			param = data.Product1.ParamsByWhsAndClient.AddNew();
			param.W3_WW = data.Whs2.PK;
			param.W3_OH = data.Org1.PK;
			param.W3_StockTakeCycle = "TUE";

			param = data.Product2.ParamsByWhsAndClient.AddNew();
			param.W3_WW = data.Whs1.PK;
			param.W3_OH = data.Org1.PK;
			param.W3_StockTakeCycle = "TUE";

			param = data.Product2.ParamsByWhsAndClient.AddNew();
			param.W3_WW = data.Whs1.PK;
			param.W3_OH = data.Org2.PK;
			param.W3_StockTakeCycle = "WED";

			Factory.Save();

			Stocktake.WS_WW_Whs = data.Whs1.PK;
			Stocktake.WS_OH_Client = data.Org1.PK;
			Stocktake.Load();
			AssertEquals("Stocktake should load all records for Whs1 and Org1,", 3, Stocktake.Lines.Count);

			Stocktake = GetNewBusinessObject();
			Stocktake.WS_WW_Whs = data.Whs1.PK;
			Stocktake.WS_OH_Client = data.Org2.PK;
			Stocktake.Load();
			AssertEquals("Stocktake should load all records for Whs1 and Org2,", 7, Stocktake.Lines.Count);

			Stocktake = GetNewBusinessObject();
			Stocktake.WS_WW_Whs = data.Whs1.PK;
			Stocktake.WS_OH_Client = data.Org1.PK;
			Stocktake.WS_StocktakeCycle = "XXX";
			Stocktake.Load();
			AssertEquals("Stocktake should load no records,", 0, Stocktake.Lines.Count);

			Stocktake = GetNewBusinessObject();
			Stocktake.WS_WW_Whs = data.Whs1.PK;
			Stocktake.WS_OH_Client = data.Org1.PK;
			Stocktake.WS_StocktakeCycle = "MON";
			Stocktake.Load();
			AssertEquals("Stocktake should load 1 record,", 1, Stocktake.Lines.Count);

			Stocktake = GetNewBusinessObject();
			Stocktake.WS_WW_Whs = data.Whs1.PK;
			Stocktake.WS_OH_Client = data.Org1.PK;
			Stocktake.WS_StocktakeCycle = "TUE";
			Stocktake.Load();
			AssertEquals("Stocktake should load 1 records,", 1, Stocktake.Lines.Count);

			Stocktake = GetNewBusinessObject();
			Stocktake.WS_WW_Whs = data.Whs1.PK;
			Stocktake.WS_OH_Client = data.Org2.PK;
			Stocktake.WS_StocktakeCycle = "TUE";
			Stocktake.Load();
			AssertEquals("Stocktake should load 1 record,", 1, Stocktake.Lines.Count);

			Stocktake = GetNewBusinessObject();
			Stocktake.WS_WW_Whs = data.Whs1.PK;
			Stocktake.WS_OH_Client = data.Org2.PK;
			Stocktake.WS_StocktakeCycle = "WED";
			Stocktake.Load();
			AssertEquals("Stocktake should load 6 records,", 6, Stocktake.Lines.Count);
		}

		#endregion

		#region TestLoad_InventoryData

		[TestDate(2012, 1, 1, 01, 00, 00)]
		public void TestLoad_InventoryData()
		{
			var now = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory, 4, 4);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: false);

			// Setup receive and inventory line

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation,
				"PLT1", now.AddDays(-3), now.AddDays(-2), "A1", "A2", "A3", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save(); // To save receipt to the database

			// Create and load stocktake
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			stocktake.Load();

			AssertEquals("One stocktake line should be created.", 1, stocktake.Lines.Count);

			var line = stocktake.Lines[0];

			// Check whether the stocktake line contains same data as inventory line

			AssertEquals(inventory.WI_OH_Client, line.WU_OH_Client);
			AssertEquals(inventory.WI_WL, line.WU_WL);
			AssertEquals(inventory.WI_OP, line.WU_OP);
			AssertEquals(inventory.WI_PalletID, line.WU_PalletID);
			AssertEquals(inventory.WI_TotalUnits, line.WU_SystemUnits);
			AssertEquals(inventory.WI_PartAttrib1, line.WU_PartAttrib1);
			AssertEquals(inventory.WI_PartAttrib2, line.WU_PartAttrib2);
			AssertEquals(inventory.WI_PartAttrib3, line.WU_PartAttrib3);
			AssertEquals("", line.WU_SerialNumber);
			AssertEquals(inventory.WI_PackingDate, line.WU_PackingDate);
			AssertEquals(inventory.WI_ExpiryDate, line.WU_ExpiryDate);
		}

		#region TestLoad_InventoryData_WithSerialNumber

		[TestDate(2012, 1, 1, 01, 00, 00)]
		public void TestLoad_InventoryData_WithSerialNumber()
		{
			var now = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory, 4, 4);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			// Setup receive and inventory line

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation,
				"PLT1", now.AddDays(3), now.AddDays(-2), "A1", "A2", "A3", "");
			inventory.WI_SerialNumber = "SER";
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save(); // To save receipt to the database

			// Create and load stocktake
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			stocktake.Load();

			AssertEquals("One stocktake line should be created.", 1, stocktake.Lines.Count);

			var line = stocktake.Lines[0];

			// Check whether the stocktake line contains same data as inventory line

			AssertEquals(inventory.WI_OH_Client, line.WU_OH_Client);
			AssertEquals(inventory.WI_WL, line.WU_WL);
			AssertEquals(inventory.WI_OP, line.WU_OP);
			AssertEquals(inventory.WI_PalletID, line.WU_PalletID);
			AssertEquals(inventory.WI_TotalUnits, line.WU_SystemUnits);
			AssertEquals(inventory.WI_PartAttrib1, line.WU_PartAttrib1);
			AssertEquals(inventory.WI_PartAttrib2, line.WU_PartAttrib2);
			AssertEquals(inventory.WI_PartAttrib3, line.WU_PartAttrib3);
			AssertEquals(inventory.WI_SerialNumber, line.WU_SerialNumber);
			AssertEquals(inventory.WI_PackingDate, line.WU_PackingDate);
			AssertEquals(inventory.WI_ExpiryDate, line.WU_ExpiryDate);
		}

		#endregion

		#endregion

		#region TestLoad_WithPackageGroupIdAndPerPackageQty

		public void TestLoad_WithPackageGroupIdAndPerPackageQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.RemoveAreasOfTypeFromWarehouse(data.Whs1, "FRE");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, "123-1", "ABC", 2m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, "123-1", "ABC", 2m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, "123-2", "ABC", 2m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m, "123-3", "ABC", 1m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, "123-3", "XYZ", 5m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			stocktake.Load();

			AssertEquals("Should load all inventory in the warehouse.", 4, stocktake.Lines.Count);
			var lines = stocktake.Lines.ToList();
			AssertStocktakeLineExists(lines, data.Part1, 20m, "123-1", "ABC", 2m);
			AssertStocktakeLineExists(lines, data.Part1, 10m, "123-2", "ABC", 2m);
			AssertStocktakeLineExists(lines, data.Part2, 15m, "123-3", "ABC", 1m);
			AssertStocktakeLineExists(lines, data.Part1, 10m, "123-3", "XYZ", 5m);
		}

		void AssertStocktakeLineExists(List<WhsStocktakeLine> lines, OrgSupplierPart part, ZDecimal systemUnits,
			ZString entryKey, ZString packageGroupID, ZDecimal perPackageQty)
		{
			var stocktakeLine = lines.SingleOrDefault(s =>
				s.WU_OP == part.PK
				&& s.WU_SystemUnits == systemUnits
				&& s.WU_BondedEntryKey == entryKey
				&& s.WU_PackageGroupId == packageGroupID
				&& s.WU_PerPackageQty == perPackageQty);

			AssertNotNull(string.Format(
				"Could not find stocktakeLine Part: {0}, Units: {1}, BEK: {2}, ID: {3}, PerPackQty: {4}",
				part.OP_PartNum, systemUnits, entryKey, packageGroupID, perPackageQty), stocktakeLine);
			lines.Remove(stocktakeLine);
		}

		#endregion

		#region TestLoadWithAttibuteNeutralProducts

		#region TestLoadWithAttibuteNeutralProducts_WithPartAttribute1UsedForSerialNumberProducts

		[TestDate(2012, 04, 05)]
		public void TestLoadWithAttibuteNeutralProducts_WithPartAttribute1UsedForSerialNumberProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var location = data.Whs1.FindLocation("A");

			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			data.Part1.RelatedOrganisations[0].OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			// data.Part2 is attribute specified

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, true, useSerialNumber: false);

			var receive_Finalised = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var line = CreateWhsReceiveInventoryLine(receive_Finalised, data.Part1, 1m, location,
				InventoryStatus.Codes.Putaway, ZDate.Today, ZDate.Today.AddDays(-1), "A1", "A2", "A3");
			var samePartAttribute1AndSameOtherAttributes = CreateWhsReceiveInventoryLine(receive_Finalised, data.Part1,
				1m, location, InventoryStatus.Codes.Putaway, ZDate.Today, ZDate.Today.AddDays(-1), "A1", "A2", "A3");
			var differentPartAttribute1AndSameOtherAttributes = CreateWhsReceiveInventoryLine(receive_Finalised,
				data.Part1, 1m, location, InventoryStatus.Codes.Putaway, ZDate.Today, ZDate.Today.AddDays(-1), "A2",
				"A2", "A3");
			var differentPartAttribute1AndDifferentOtherAttributes = CreateWhsReceiveInventoryLine(receive_Finalised,
				data.Part1, 1m, location, InventoryStatus.Codes.Putaway, ZDate.Today.AddDays(1),
				ZDate.Today.AddDays(-2), "A1", "A3", "A4");
			var line1WithAttributeSpecfiedProduct = CreateWhsReceiveInventoryLine(receive_Finalised, data.Part2, 1m,
				location, InventoryStatus.Codes.Putaway, ZDate.Today.AddDays(1), ZDate.Today.AddDays(-2), "A1", "A2",
				"A3");
			var line2WithAttributeSpecfiedProduct = CreateWhsReceiveInventoryLine(receive_Finalised, data.Part2, 1m,
				location, InventoryStatus.Codes.Putaway, ZDate.Today.AddDays(1), ZDate.Today.AddDays(-2), "A2", "A2",
				"A3");

			receive_Finalised.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive_Finalised);

			Factory.Save();

			Stocktake.WS_OH_Client = data.Org1.PK;
			Stocktake.WS_WW_Whs = data.Whs1.PK;
			Stocktake.Load();

			AssertEquals("Stocktake line count", 5, Stocktake.Lines.Count);
			var stocktakeLine1 = FindLine(Stocktake, data.Part1, CodeLists.InventoryStatus.Codes.Available, "A1", "A2",
				"A3"); // During the finalisation of the docket Putaway status is changed to Available
			AssertEquals("Stocktakeline1 system units", 2m, stocktakeLine1.WU_SystemUnits);
			AssertEquals("Stocktakeline1 expiry date", ZDate.Today, stocktakeLine1.WU_ExpiryDate);
			AssertEquals("Stocktakeline1 packing date", ZDate.Today.AddDays(-1), stocktakeLine1.WU_PackingDate);
			AssertEquals("Stocktakeline1 partattribute 1", "A1", stocktakeLine1.WU_PartAttrib1);
			AssertEquals("Stocktakeline1 partattribute 2", "A2", stocktakeLine1.WU_PartAttrib2);
			AssertEquals("Stocktakeline1 partattribute 3", "A3", stocktakeLine1.WU_PartAttrib3);

			var stocktakeLine2 = FindLine(Stocktake, data.Part1, CodeLists.InventoryStatus.Codes.Available, "A1", "A3",
				"A4"); // During the finalisation of the docket Putaway status is changed to Available
			AssertEquals("Stocktakeline2 system units", 1m, stocktakeLine2.WU_SystemUnits);
			AssertEquals("Stocktakeline2 expiry date", ZDate.Today.AddDays(1), stocktakeLine2.WU_ExpiryDate);
			AssertEquals("Stocktakeline2 packing date", ZDate.Today.AddDays(-2), stocktakeLine2.WU_PackingDate);
			AssertEquals("Stocktakeline2 partattribute 1", "A1", stocktakeLine2.WU_PartAttrib1);
			AssertEquals("Stocktakeline2 partattribute 2", "A3", stocktakeLine2.WU_PartAttrib2);
			AssertEquals("Stocktakeline2 partattribute 3", "A4", stocktakeLine2.WU_PartAttrib3);

			var stocktakeLine3 = FindLine(Stocktake, data.Part1, CodeLists.InventoryStatus.Codes.Available, "A2", "A2",
				"A3"); // During the finalisation of the docket Putaway status is changed to Available
			AssertEquals("Stocktakeline3 system units", 1m, stocktakeLine3.WU_SystemUnits);
			AssertEquals("Stocktakeline3 expiry date", ZDate.Today, stocktakeLine3.WU_ExpiryDate);
			AssertEquals("Stocktakeline3 packing date", ZDate.Today.AddDays(-1), stocktakeLine3.WU_PackingDate);
			AssertEquals("Stocktakeline3 partattribute 1", "A2", stocktakeLine3.WU_PartAttrib1);
			AssertEquals("Stocktakeline3 partattribute 2", "A2", stocktakeLine3.WU_PartAttrib2);
			AssertEquals("Stocktakeline3 partattribute 3", "A3", stocktakeLine3.WU_PartAttrib3);

			var stocktakeLine4 = FindLine(Stocktake, data.Part2, CodeLists.InventoryStatus.Codes.Available, "A1", "A2",
				"A3"); // During the finalisation of the docket Putaway status is changed to Available
			AssertEquals("Stocktakeline4 system units", 1m, stocktakeLine4.WU_SystemUnits);
			AssertEquals("Stocktakeline4 expiry date", ZDate.Today.AddDays(1), stocktakeLine4.WU_ExpiryDate);
			AssertEquals("Stocktakeline4 packing date", ZDate.Today.AddDays(-2), stocktakeLine4.WU_PackingDate);
			AssertEquals("Stocktakeline4 partattribute 1", "A1", stocktakeLine4.WU_PartAttrib1);
			AssertEquals("Stocktakeline4 partattribute 2", "A2", stocktakeLine4.WU_PartAttrib2);
			AssertEquals("Stocktakeline4 partattribute 3", "A3", stocktakeLine4.WU_PartAttrib3);

			var stocktakeLine5 = FindLine(Stocktake, data.Part2, CodeLists.InventoryStatus.Codes.Available, "A2", "A2",
				"A3"); // During the finalisation of the docket Putaway status is changed to Available
			AssertEquals("Stocktakeline5 system units", 1m, stocktakeLine5.WU_SystemUnits);
			AssertEquals("Stocktakeline5 expiry date", ZDate.Today.AddDays(1), stocktakeLine5.WU_ExpiryDate);
			AssertEquals("Stocktakeline5 packing date", ZDate.Today.AddDays(-2), stocktakeLine5.WU_PackingDate);
			AssertEquals("Stocktakeline5 partattribute 1", "A2", stocktakeLine5.WU_PartAttrib1);
			AssertEquals("Stocktakeline5 partattribute 2", "A2", stocktakeLine5.WU_PartAttrib2);
			AssertEquals("Stocktakeline5 partattribute 3", "A3", stocktakeLine5.WU_PartAttrib3);
		}

		#endregion

		#region TestLoadWithAttibuteNeutralProducts_WithPartAttribute2UsedForSerialNumberProducts

		[TestDate(2012, 04, 05)]
		public void TestLoadWithAttibuteNeutralProducts_WithPartAttribute2UsedForSerialNumberProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var location = data.Whs1.FindLocation("A");

			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			data.Part1.RelatedOrganisations[0].OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute2;

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, true, useSerialNumber: false);

			var receive_Finalised = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var line = CreateWhsReceiveInventoryLine(receive_Finalised, data.Part1, 1m, location,
				InventoryStatus.Codes.Putaway, ZDate.Today, ZDate.Today.AddDays(-1), "A1", "A2", "A3");
			var samePartAttribute1AndSameOtherAttributes = CreateWhsReceiveInventoryLine(receive_Finalised, data.Part1,
				1m, location, InventoryStatus.Codes.Putaway, ZDate.Today, ZDate.Today.AddDays(-1), "A1", "A2", "A3");
			var differentPartAttribute1AndSameOtherAttributes = CreateWhsReceiveInventoryLine(receive_Finalised,
				data.Part1, 1m, location, InventoryStatus.Codes.Putaway, ZDate.Today, ZDate.Today.AddDays(-1), "A1",
				"A1", "A3");
			var differentPartAttribute1AndDifferentOtherAttributes = CreateWhsReceiveInventoryLine(receive_Finalised,
				data.Part1, 1m, location, InventoryStatus.Codes.Putaway, ZDate.Today.AddDays(1),
				ZDate.Today.AddDays(-2), "A1", "A3", "A4");
			var line1WithAttributeSpecfiedProduct = CreateWhsReceiveInventoryLine(receive_Finalised, data.Part2, 1m,
				location, InventoryStatus.Codes.Putaway, ZDate.Today.AddDays(1), ZDate.Today.AddDays(-2), "A1", "A2",
				"A3");
			var line2WithAttributeSpecfiedProduct = CreateWhsReceiveInventoryLine(receive_Finalised, data.Part2, 1m,
				location, InventoryStatus.Codes.Putaway, ZDate.Today.AddDays(1), ZDate.Today.AddDays(-2), "A1", "A4",
				"A3");

			receive_Finalised.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive_Finalised);

			Factory.Save();

			Stocktake.WS_OH_Client = data.Org1.PK;
			Stocktake.WS_WW_Whs = data.Whs1.PK;
			Stocktake.Load();

			AssertEquals("Stocktake line count", 5, Stocktake.Lines.Count);
			var stocktakeLine1 = FindLine(Stocktake, data.Part1, CodeLists.InventoryStatus.Codes.Available, "A1", "A2",
				"A3"); // During the finalisation of the docket Putaway status is changed to Available
			AssertEquals("Stocktakeline1 system units", 2m, stocktakeLine1.WU_SystemUnits);
			AssertEquals("Stocktakeline1 expiry date", ZDate.Today, stocktakeLine1.WU_ExpiryDate);
			AssertEquals("Stocktakeline1 packing date", ZDate.Today.AddDays(-1), stocktakeLine1.WU_PackingDate);
			AssertEquals("Stocktakeline1 partattribute 1", "A1", stocktakeLine1.WU_PartAttrib1);
			AssertEquals("Stocktakeline1 partattribute 2", "A2", stocktakeLine1.WU_PartAttrib2);
			AssertEquals("Stocktakeline1 partattribute 3", "A3", stocktakeLine1.WU_PartAttrib3);

			var stocktakeLine2 = FindLine(Stocktake, data.Part1, CodeLists.InventoryStatus.Codes.Available, "A1", "A3",
				"A4"); // During the finalisation of the docket Putaway status is changed to Available
			AssertEquals("Stocktakeline2 system units", 1m, stocktakeLine2.WU_SystemUnits);
			AssertEquals("Stocktakeline2 expiry date", ZDate.Today.AddDays(1), stocktakeLine2.WU_ExpiryDate);
			AssertEquals("Stocktakeline2 packing date", ZDate.Today.AddDays(-2), stocktakeLine2.WU_PackingDate);
			AssertEquals("Stocktakeline2 partattribute 1", "A1", stocktakeLine2.WU_PartAttrib1);
			AssertEquals("Stocktakeline2 partattribute 2", "A3", stocktakeLine2.WU_PartAttrib2);
			AssertEquals("Stocktakeline2 partattribute 3", "A4", stocktakeLine2.WU_PartAttrib3);

			var stocktakeLine3 = FindLine(Stocktake, data.Part1, CodeLists.InventoryStatus.Codes.Available, "A1", "A1",
				"A3"); // During the finalisation of the docket Putaway status is changed to Available
			AssertEquals("Stocktakeline3 system units", 1m, stocktakeLine3.WU_SystemUnits);
			AssertEquals("Stocktakeline3 expiry date", ZDate.Today, stocktakeLine3.WU_ExpiryDate);
			AssertEquals("Stocktakeline3 packing date", ZDate.Today.AddDays(-1), stocktakeLine3.WU_PackingDate);
			AssertEquals("Stocktakeline3 partattribute 1", "A1", stocktakeLine3.WU_PartAttrib1);
			AssertEquals("Stocktakeline3 partattribute 2", "A1", stocktakeLine3.WU_PartAttrib2);
			AssertEquals("Stocktakeline3 partattribute 3", "A3", stocktakeLine3.WU_PartAttrib3);

			var stocktakeLine4 = FindLine(Stocktake, data.Part2, CodeLists.InventoryStatus.Codes.Available, "A1", "A2",
				"A3"); // During the finalisation of the docket Putaway status is changed to Available
			AssertEquals("Stocktakeline4 system units", 1m, stocktakeLine4.WU_SystemUnits);
			AssertEquals("Stocktakeline4 expiry date", ZDate.Today.AddDays(1), stocktakeLine4.WU_ExpiryDate);
			AssertEquals("Stocktakeline4 packing date", ZDate.Today.AddDays(-2), stocktakeLine4.WU_PackingDate);
			AssertEquals("Stocktakeline4 partattribute 1", "A1", stocktakeLine4.WU_PartAttrib1);
			AssertEquals("Stocktakeline4 partattribute 2", "A2", stocktakeLine4.WU_PartAttrib2);
			AssertEquals("Stocktakeline4 partattribute 3", "A3", stocktakeLine4.WU_PartAttrib3);

			var stocktakeLine5 = FindLine(Stocktake, data.Part2, CodeLists.InventoryStatus.Codes.Available, "A1", "A4",
				"A3"); // During the finalisation of the docket Putaway status is changed to Available
			AssertEquals("Stocktakeline5 system units", 1m, stocktakeLine5.WU_SystemUnits);
			AssertEquals("Stocktakeline5 expiry date", ZDate.Today.AddDays(1), stocktakeLine5.WU_ExpiryDate);
			AssertEquals("Stocktakeline5 packing date", ZDate.Today.AddDays(-2), stocktakeLine5.WU_PackingDate);
			AssertEquals("Stocktakeline5 partattribute 1", "A1", stocktakeLine5.WU_PartAttrib1);
			AssertEquals("Stocktakeline5 partattribute 2", "A4", stocktakeLine5.WU_PartAttrib2);
			AssertEquals("Stocktakeline5 partattribute 3", "A3", stocktakeLine5.WU_PartAttrib3);
		}

		#endregion

		#region TestLoadWithAttibuteNeutralProducts_WithPartAttribute3UsedForSerialNumberProducts

		[TestDate(2012, 04, 05)]
		public void TestLoadWithAttibuteNeutralProducts_WithPartAttribute3UsedForSerialNumberProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var location = data.Whs1.FindLocation("A");

			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			data.Part1.RelatedOrganisations[0].OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute3;

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, true, useSerialNumber: false);

			var receive_Finalised = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var line = CreateWhsReceiveInventoryLine(receive_Finalised, data.Part1, 1m, location,
				InventoryStatus.Codes.Putaway, ZDate.Today, ZDate.Today.AddDays(-1), "A1", "A2", "A3");
			var samePartAttribute1AndSameOtherAttributes = CreateWhsReceiveInventoryLine(receive_Finalised, data.Part1,
				1m, location, InventoryStatus.Codes.Putaway, ZDate.Today, ZDate.Today.AddDays(-1), "A1", "A2", "A3");
			var differentPartAttribute1AndSameOtherAttributes = CreateWhsReceiveInventoryLine(receive_Finalised,
				data.Part1, 1m, location, InventoryStatus.Codes.Putaway, ZDate.Today, ZDate.Today.AddDays(-1), "A1",
				"A2", "A4");
			var differentPartAttribute1AndDifferentOtherAttributes = CreateWhsReceiveInventoryLine(receive_Finalised,
				data.Part1, 1m, location, InventoryStatus.Codes.Putaway, ZDate.Today.AddDays(1),
				ZDate.Today.AddDays(-2), "A1", "A3", "A4");
			var line1WithAttributeSpecfiedProduct = CreateWhsReceiveInventoryLine(receive_Finalised, data.Part2, 1m,
				location, InventoryStatus.Codes.Putaway, ZDate.Today.AddDays(1), ZDate.Today.AddDays(-2), "A1", "A2",
				"A3");
			var line2WithAttributeSpecfiedProduct = CreateWhsReceiveInventoryLine(receive_Finalised, data.Part2, 1m,
				location, InventoryStatus.Codes.Putaway, ZDate.Today.AddDays(1), ZDate.Today.AddDays(-2), "A1", "A4",
				"A3");

			receive_Finalised.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive_Finalised);

			Factory.Save();

			Stocktake.WS_OH_Client = data.Org1.PK;
			Stocktake.WS_WW_Whs = data.Whs1.PK;
			Stocktake.Load();

			AssertEquals("Stocktake line count", 5, Stocktake.Lines.Count);
			var stocktakeLine1 = FindLine(Stocktake, data.Part1, CodeLists.InventoryStatus.Codes.Available, "A1", "A2",
				"A3"); // During the finalisation of the docket Putaway status is changed to Available
			AssertEquals("Stocktakeline1 system units", 2m, stocktakeLine1.WU_SystemUnits);
			AssertEquals("Stocktakeline1 expiry date", ZDate.Today, stocktakeLine1.WU_ExpiryDate);
			AssertEquals("Stocktakeline1 packing date", ZDate.Today.AddDays(-1), stocktakeLine1.WU_PackingDate);
			AssertEquals("Stocktakeline1 partattribute 1", "A1", stocktakeLine1.WU_PartAttrib1);
			AssertEquals("Stocktakeline1 partattribute 2", "A2", stocktakeLine1.WU_PartAttrib2);
			AssertEquals("Stocktakeline1 partattribute 3", "A3", stocktakeLine1.WU_PartAttrib3);

			var stocktakeLine2 = FindLine(Stocktake, data.Part1, CodeLists.InventoryStatus.Codes.Available, "A1", "A3",
				"A4"); // During the finalisation of the docket Putaway status is changed to Available
			AssertEquals("Stocktakeline2 system units", 1m, stocktakeLine2.WU_SystemUnits);
			AssertEquals("Stocktakeline2 expiry date", ZDate.Today.AddDays(1), stocktakeLine2.WU_ExpiryDate);
			AssertEquals("Stocktakeline2 packing date", ZDate.Today.AddDays(-2), stocktakeLine2.WU_PackingDate);
			AssertEquals("Stocktakeline2 partattribute 1", "A1", stocktakeLine2.WU_PartAttrib1);
			AssertEquals("Stocktakeline2 partattribute 2", "A3", stocktakeLine2.WU_PartAttrib2);
			AssertEquals("Stocktakeline2 partattribute 3", "A4", stocktakeLine2.WU_PartAttrib3);

			var stocktakeLine3 = FindLine(Stocktake, data.Part1, CodeLists.InventoryStatus.Codes.Available, "A1", "A2",
				"A4"); // During the finalisation of the docket Putaway status is changed to Available
			AssertEquals("Stocktakeline3 system units", 1m, stocktakeLine3.WU_SystemUnits);
			AssertEquals("Stocktakeline3 expiry date", ZDate.Today, stocktakeLine3.WU_ExpiryDate);
			AssertEquals("Stocktakeline3 packing date", ZDate.Today.AddDays(-1), stocktakeLine3.WU_PackingDate);
			AssertEquals("Stocktakeline3 partattribute 1", "A1", stocktakeLine3.WU_PartAttrib1);
			AssertEquals("Stocktakeline3 partattribute 2", "A2", stocktakeLine3.WU_PartAttrib2);
			AssertEquals("Stocktakeline3 partattribute 3", "A4", stocktakeLine3.WU_PartAttrib3);

			var stocktakeLine4 = FindLine(Stocktake, data.Part2, CodeLists.InventoryStatus.Codes.Available, "A1", "A2",
				"A3"); // During the finalisation of the docket Putaway status is changed to Available
			AssertEquals("Stocktakeline4 system units", 1m, stocktakeLine4.WU_SystemUnits);
			AssertEquals("Stocktakeline4 expiry date", ZDate.Today.AddDays(1), stocktakeLine4.WU_ExpiryDate);
			AssertEquals("Stocktakeline4 packing date", ZDate.Today.AddDays(-2), stocktakeLine4.WU_PackingDate);
			AssertEquals("Stocktakeline4 partattribute 1", "A1", stocktakeLine4.WU_PartAttrib1);
			AssertEquals("Stocktakeline4 partattribute 2", "A2", stocktakeLine4.WU_PartAttrib2);
			AssertEquals("Stocktakeline4 partattribute 3", "A3", stocktakeLine4.WU_PartAttrib3);

			var stocktakeLine5 = FindLine(Stocktake, data.Part2, CodeLists.InventoryStatus.Codes.Available, "A1", "A4",
				"A3"); // During the finalisation of the docket Putaway status is changed to Available
			AssertEquals("Stocktakeline5 system units", 1m, stocktakeLine5.WU_SystemUnits);
			AssertEquals("Stocktakeline5 expiry date", ZDate.Today.AddDays(1), stocktakeLine5.WU_ExpiryDate);
			AssertEquals("Stocktakeline5 packing date", ZDate.Today.AddDays(-2), stocktakeLine5.WU_PackingDate);
			AssertEquals("Stocktakeline5 partattribute 1", "A1", stocktakeLine5.WU_PartAttrib1);
			AssertEquals("Stocktakeline5 partattribute 2", "A4", stocktakeLine5.WU_PartAttrib2);
			AssertEquals("Stocktakeline5 partattribute 3", "A3", stocktakeLine5.WU_PartAttrib3);
		}

		#endregion

		#region TestLoadWithAttibuteNeutralProducts_WithSerialNumberColumnUsed

		[TestDate(2012, 04, 05)]
		public void TestLoadWithAttibuteNeutralProducts_WithSerialNumberColumnUsed()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var location = data.Whs1.FindLocation("A");

			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			data.Part1.RelatedOrganisations[0].OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, true);

			var receive_Finalised = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var line = CreateWhsReceiveInventoryLine(receive_Finalised, data.Part1, 1m, location,
				InventoryStatus.Codes.Putaway, ZDate.Today, ZDate.Today.AddDays(-1), "A1", "A2", "A3");
			line.WI_SerialNumber = "SN1";
			var samePartAttribute1AndSameOtherAttributes = CreateWhsReceiveInventoryLine(receive_Finalised, data.Part1,
				1m, location, InventoryStatus.Codes.Putaway, ZDate.Today, ZDate.Today.AddDays(-1), "A1", "A2", "A3");
			samePartAttribute1AndSameOtherAttributes.WI_SerialNumber = "SN2";
			var differentPartAttribute1AndSameOtherAttributes = CreateWhsReceiveInventoryLine(receive_Finalised,
				data.Part1, 1m, location, InventoryStatus.Codes.Putaway, ZDate.Today, ZDate.Today.AddDays(-1), "A1",
				"A2", "A4");
			differentPartAttribute1AndSameOtherAttributes.WI_SerialNumber = "SN3";
			var differentPartAttribute1AndDifferentOtherAttributes = CreateWhsReceiveInventoryLine(receive_Finalised,
				data.Part1, 1m, location, InventoryStatus.Codes.Putaway, ZDate.Today.AddDays(1),
				ZDate.Today.AddDays(-2), "A1", "A3", "A4");
			differentPartAttribute1AndDifferentOtherAttributes.WI_SerialNumber = "SN4";
			var line1WithAttributeSpecfiedProduct = CreateWhsReceiveInventoryLine(receive_Finalised, data.Part2, 1m,
				location, InventoryStatus.Codes.Putaway, ZDate.Today.AddDays(1), ZDate.Today.AddDays(-2), "A1", "A2",
				"A3");
			line1WithAttributeSpecfiedProduct.WI_SerialNumber = "SN5";
			var line2WithAttributeSpecfiedProduct = CreateWhsReceiveInventoryLine(receive_Finalised, data.Part2, 1m,
				location, InventoryStatus.Codes.Putaway, ZDate.Today.AddDays(1), ZDate.Today.AddDays(-2), "A1", "A2",
				"A3");
			line2WithAttributeSpecfiedProduct.WI_SerialNumber = "SN6";

			receive_Finalised.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive_Finalised);

			Factory.Save();

			Stocktake.WS_OH_Client = data.Org1.PK;
			Stocktake.WS_WW_Whs = data.Whs1.PK;
			Stocktake.Load();

			AssertEquals("Stocktake line count", 5, Stocktake.Lines.Count);
			var stocktakeLine1 = FindLine(Stocktake, data.Part1, CodeLists.InventoryStatus.Codes.Available, "A1", "A2",
				"A3"); // During the finalisation of the docket Putaway status is changed to Available
			AssertEquals("Stocktakeline1 system units", 2m, stocktakeLine1.WU_SystemUnits);
			AssertEquals("Stocktakeline1 expiry date", ZDate.Today, stocktakeLine1.WU_ExpiryDate);
			AssertEquals("Stocktakeline1 packing date", ZDate.Today.AddDays(-1), stocktakeLine1.WU_PackingDate);
			AssertEquals("Stocktakeline1 partattribute 1", "A1", stocktakeLine1.WU_PartAttrib1);
			AssertEquals("Stocktakeline1 partattribute 2", "A2", stocktakeLine1.WU_PartAttrib2);
			AssertEquals("Stocktakeline1 partattribute 3", "A3", stocktakeLine1.WU_PartAttrib3);
			AssertEquals("Stocktakeline1 Serial Number", WhsStocktake.AttributeNeutral, stocktakeLine1.WU_SerialNumber);

			var stocktakeLine2 = FindLine(Stocktake, data.Part1, CodeLists.InventoryStatus.Codes.Available, "A1", "A2",
				"A4"); // During the finalisation of the docket Putaway status is changed to Available
			AssertEquals("Stocktakeline2 system units", 1m, stocktakeLine2.WU_SystemUnits);
			AssertEquals("Stocktakeline2 expiry date", ZDate.Today, stocktakeLine2.WU_ExpiryDate);
			AssertEquals("Stocktakeline2 packing date", ZDate.Today.AddDays(-1), stocktakeLine2.WU_PackingDate);
			AssertEquals("Stocktakeline2 partattribute 1", "A1", stocktakeLine2.WU_PartAttrib1);
			AssertEquals("Stocktakeline2 partattribute 2", "A2", stocktakeLine2.WU_PartAttrib2);
			AssertEquals("Stocktakeline2 partattribute 3", "A4", stocktakeLine2.WU_PartAttrib3);
			AssertEquals("Stocktakeline2 Serial Number", WhsStocktake.AttributeNeutral, stocktakeLine2.WU_SerialNumber);

			var stocktakeLine3 = FindLine(Stocktake, data.Part1, CodeLists.InventoryStatus.Codes.Available, "A1", "A3",
				"A4"); // During the finalisation of the docket Putaway status is changed to Available
			AssertEquals("Stocktakeline3 system units", 1m, stocktakeLine3.WU_SystemUnits);
			AssertEquals("Stocktakeline3 expiry date", ZDate.Today.AddDays(1), stocktakeLine3.WU_ExpiryDate);
			AssertEquals("Stocktakeline3 packing date", ZDate.Today.AddDays(-2), stocktakeLine3.WU_PackingDate);
			AssertEquals("Stocktakeline3 partattribute 1", "A1", stocktakeLine3.WU_PartAttrib1);
			AssertEquals("Stocktakeline3 partattribute 2", "A3", stocktakeLine3.WU_PartAttrib2);
			AssertEquals("Stocktakeline3 partattribute 3", "A4", stocktakeLine3.WU_PartAttrib3);
			AssertEquals("Stocktakeline3 Serial Number", WhsStocktake.AttributeNeutral, stocktakeLine3.WU_SerialNumber);

			var stocktakeLine4 = FindLine(Stocktake, data.Part2, CodeLists.InventoryStatus.Codes.Available, "A1", "A2",
				"A3", "SN5"); // During the finalisation of the docket Putaway status is changed to Available
			AssertEquals("Stocktakeline4 system units", 1m, stocktakeLine4.WU_SystemUnits);
			AssertEquals("Stocktakeline4 expiry date", ZDate.Today.AddDays(1), stocktakeLine4.WU_ExpiryDate);
			AssertEquals("Stocktakeline4 packing date", ZDate.Today.AddDays(-2), stocktakeLine4.WU_PackingDate);
			AssertEquals("Stocktakeline4 partattribute 1", "A1", stocktakeLine4.WU_PartAttrib1);
			AssertEquals("Stocktakeline4 partattribute 2", "A2", stocktakeLine4.WU_PartAttrib2);
			AssertEquals("Stocktakeline4 partattribute 3", "A3", stocktakeLine4.WU_PartAttrib3);
			AssertEquals("Stocktakeline4 Serial Number", "SN5", stocktakeLine4.WU_SerialNumber);

			var stocktakeLine5 = FindLine(Stocktake, data.Part2, CodeLists.InventoryStatus.Codes.Available, "A1", "A2",
				"A3", "SN6"); // During the finalisation of the docket Putaway status is changed to Available
			AssertEquals("Stocktakeline5 system units", 1m, stocktakeLine5.WU_SystemUnits);
			AssertEquals("Stocktakeline5 expiry date", ZDate.Today.AddDays(1), stocktakeLine5.WU_ExpiryDate);
			AssertEquals("Stocktakeline5 packing date", ZDate.Today.AddDays(-2), stocktakeLine5.WU_PackingDate);
			AssertEquals("Stocktakeline5 partattribute 1", "A1", stocktakeLine5.WU_PartAttrib1);
			AssertEquals("Stocktakeline5 partattribute 2", "A2", stocktakeLine5.WU_PartAttrib2);
			AssertEquals("Stocktakeline5 partattribute 3", "A3", stocktakeLine5.WU_PartAttrib3);
			AssertEquals("Stocktakeline5 Serial Number", "SN6", stocktakeLine5.WU_SerialNumber);
		}

		#endregion

		#region TestLoad_Transfers

		public void TestLoad_Transfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m,
				data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 15m, "A-1", "A-2");
			transfer.RunPreSaveValidation(); // to commit stock.
			transferLine2.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine2);

			Factory.Save();

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			stocktake.Load();
			AssertEquals("Should have loaded 2 inventory records to stocktake.", 2, stocktake.Lines.Count);
			AssertHaveStocktakeLineMatchInventory(receive.Inventory[0], stocktake.Lines);
			AssertHaveStocktakeLineMatchInventory(transferLine2.Inventory[0], stocktake.Lines);
		}

		void AssertHaveStocktakeLineMatchInventory(WhsInventoryView expectedInventory,
			WhsStocktakeLineCollection allStocktakeLines)
		{
			var stocktakeLine = allStocktakeLines.Cast<WhsStocktakeLine>().Single(l =>
				l.WU_OP == expectedInventory.WI_OP && l.WU_WL == expectedInventory.WI_WL);
			AssertEquals("WU_OH_Client", expectedInventory.WI_OH_Client, stocktakeLine.WU_OH_Client);
			AssertEquals("WU_OP", expectedInventory.WI_OP, stocktakeLine.WU_OP);
			AssertEquals("WU_PalletID", expectedInventory.WI_PalletID, stocktakeLine.WU_PalletID);
			AssertEquals("WU_PartAttrib1", expectedInventory.WI_PartAttrib1, stocktakeLine.WU_PartAttrib1);
			AssertEquals("WU_PartAttrib2", expectedInventory.WI_PartAttrib2, stocktakeLine.WU_PartAttrib2);
			AssertEquals("WU_PartAttrib3", expectedInventory.WI_PartAttrib3, stocktakeLine.WU_PartAttrib3);
			AssertEquals("WU_ExpiryDate", expectedInventory.WI_ExpiryDate, stocktakeLine.WU_ExpiryDate);
			AssertEquals("WU_PackingDate", expectedInventory.WI_PackingDate, stocktakeLine.WU_PackingDate);
			AssertEquals("WU_SystemUnits", expectedInventory.WI_TotalUnits, stocktakeLine.WU_SystemUnits);
			AssertEquals("WU_WL", expectedInventory.WI_WL, stocktakeLine.WU_WL);
		}

		#endregion

		#endregion

		#region TestLoadByLocation

		public void TestLoadByLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			var locationA1 = data.Whs1.FindLocation("A-1-1");
			var locationA2 = data.Whs1.FindLocation("A-1-2");

			var receiveForOrg1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			CreateWhsReceiveInventoryLine(receiveForOrg1, data.Part1, 1m, locationA1, InventoryStatus.Codes.Putaway);
			CreateWhsReceiveInventoryLine(receiveForOrg1, data.Part2, 2m, locationA1, InventoryStatus.Codes.Putaway);
			CreateWhsReceiveInventoryLine(receiveForOrg1, data.Part1, 3m, locationA2, InventoryStatus.Codes.Putaway);

			receiveForOrg1.FinaliseDocket();

			AssertIsFinalisedPrecondition(receiveForOrg1);

			Factory.Save();

			Stocktake.WS_WW_Whs = data.Whs1.PK;
			Stocktake.WS_OH_Client = data.Org1.PK;
			Stocktake.WS_WL_Location = locationA1.PK;

			Stocktake.Load();

			AssertEquals("Stocktake line count should be 2.", 2, Stocktake.Lines.Count);
			var stocktakeLine1 =
				FindLine(Stocktake, data.Part1,
					InventoryStatus.Codes
						.Available); // During the finalisation of the docket Putaway status is changed to Available
			AssertEquals("stocktakeLine1 system units should be 1.", 1m, stocktakeLine1.WU_SystemUnits);
			AssertEquals("stocktakeLine1 location should be location1.", locationA1.ToLocationString(),
				stocktakeLine1.LocationString);

			var stocktakeLine2 =
				FindLine(Stocktake, data.Part2,
					InventoryStatus.Codes
						.Available); // During the finalisation of the docket Putaway status is changed to Available
			AssertEquals("stocktakeLine2 system units should be 2.", 2m, stocktakeLine2.WU_SystemUnits);
			AssertEquals("stocktakeLine2 location should be location 1.", locationA1.ToLocationString(),
				stocktakeLine2.LocationString);
		}

		#endregion

		#region TestLoadWithoutClient

		public void TestLoadWithoutClient()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client2 = Helper.CreateClient("C2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1);
			Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R1", data.Part1, 1);

			var stocktake = Helper.CreateWhsStocktake(null, data.Whs1); // no client specified
			Factory.Save();

			Assert("stocktake should be loaded", stocktake.Load());
			AssertEquals(2, stocktake.Lines.Count);
			AssertEquals(StocktakeStatus.Codes.Loaded, stocktake.WS_StocktakeStatus);
		}

		#endregion

		#region TestLoadAsksUserConfirmationWhenNumberOfRowsIsBig

		public void TestLoadAsksUserConfirmationWhenNumberOfRowsIsBig()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			var locationA1 = data.Whs1.FindLocation("A-1-1");
			var locationA2 = data.Whs1.FindLocation("A-1-2");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.VIN, "SN");
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			// put 1000 inventories into location1
			for (int i = 0; i < WhsStocktake.StocktakeLineCountLimitToAskConfirmation; i++)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, locationA1, ZDate.Empty, ZDate.Empty,
					i.ToString("00000"), "", "", "");
			}

			// and 1 inventory in location2
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, locationA2, ZDate.Empty, ZDate.Empty, "01001",
				"", "", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			Factory.Save();

			Notify.DefaultResponse = false;
			Assert("There will be 1001 rows to load and user response is No - stocktake lines should not be created",
				!stocktake.Load(Notify));
			AssertEquals(0, stocktake.Lines.Count);
			AssertEquals(StocktakeStatus.Codes.New, stocktake.WS_StocktakeStatus);

			stocktake.WS_WL_Location = locationA1.PK;
			Assert("For 1000 rows user will not be asked a confirmation and stocktake lines should be created",
				stocktake.Load(Notify));
			AssertEquals(1000, stocktake.Lines.Count);
			AssertEquals(StocktakeStatus.Codes.Loaded, stocktake.WS_StocktakeStatus);

			var anotherStockTake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			Notify.DefaultResponse = true;
			Assert("There will be 1001 rows to load and user response is Yes - stocktake lines should be created",
				anotherStockTake.Load(Notify));
			AssertEquals(1001, anotherStockTake.Lines.Count);
			AssertEquals(StocktakeStatus.Codes.Loaded, anotherStockTake.WS_StocktakeStatus);
		}

		#endregion

		#region TestPerformanceOfStocktakeLoad_DBHits

		[StressTest]
		public void TestPerformanceOfStocktakeLoad_DBHits()
		{
			const int numberOfInventoryLocationsToCreate = 100; // Inventory Lines to create

			var data = new TestDataSimpleEnvironment(Factory, numberOfInventoryLocationsToCreate, 1);

			int code = 1;
			foreach (var location in data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations)
			{
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, string.Format("R{0}", code), Notify);
				var product = Helper.CreateProduct(data.Org1, string.Format("PR{0}", code++));
				CreateWhsReceiveInventoryLine(receive, product, 1m, location, InventoryStatus.Codes.Putaway);
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var stocktake = newFactory.New<WhsStocktake>();
			stocktake.WS_WW_Whs = data.Whs1.PK;
			stocktake.WS_OH_Client = data.Org1.PK;

			newFactory.ResetDatabaseLoadCount();
			stocktake.Load();

			AssertEquals(100, stocktake.Lines.Count);

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 2 },
				{ WhsLocationViewSchema.Constants.TableName, 2 },
				{ WhsRowSchema.Constants.TableName, 1 }
			};

			AssertDbHits(expectedDbHits, newFactory);
		}

		#endregion

		WhsInventoryView CreateWhsReceiveInventoryLine(WhsReceive receive, OrgSupplierPart part, ZDecimal units,
			WhsLocation location, ZString inventoryStatus, string heldCode = "")
		{
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, part, units);
			inventory.WI_WL = location.PK;
			inventory.OriginalInventoryStatus = inventoryStatus;
			inventory.OriginalInventoryHeldCode = heldCode;
			return inventory;
		}

		WhsInventoryView CreateWhsReceiveInventoryLine(WhsReceive receive, OrgSupplierPart part, ZDecimal units,
			WhsLocation location, ZString inventoryStatus, ZDate expiryDate, ZDate packingDate, ZString attribute1,
			ZString attribute2, ZString attribute3, string heldCode = "")
		{
			var line = CreateWhsReceiveInventoryLine(receive, part, units, location, inventoryStatus, heldCode);
			Helper.SetInventoryAttributes(line, expiryDate, packingDate, attribute1, attribute2, attribute3, "");
			return line;
		}

		#endregion

		#region TestCreatesLogOnSave

		public void TestCreatesLogOnSave()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			Factory.Save();
			AssertEquals("Warehouse Job Entered event should be created.", 1,
				stocktake.Logs.Find(Helper.GetLogFilter(Events.WarehouseJobEntered.Code)).Length);

			stocktake.WS_StocktakeCycle = "XX";
			Factory.Save();
			AssertEquals("Only one Warehouse Job Entered event should be created.", 1,
				stocktake.Logs.Find(Helper.GetLogFilter(Events.WarehouseJobEntered.Code)).Length);
		}

		#endregion

		#region TestCreatesLogOnFinalised

		public void TestCreatesLogOnFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation,
				StocktakeLineStatus.Codes.Closed, InventoryStatus.Codes.Available);
			Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part2, data.Whs1.DefaultLocation,
				StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			Factory.Save();
			Assert("Preconditon", !stocktake.IsFinalised);
			AssertEquals("One line still open, should not have created 'finalised' log.", 0,
				stocktake.Logs.Find(Helper.GetLogFilter(Events.ItemDocumentJobFinalised.Code)).Length);

			//all lines are closed now
			stocktake.CloseLines(new WhsStocktakeLine[] { stocktake.Lines[1] });
			Factory.Save();
			Assert("Preconditon: Stocktake Finalised", stocktake.IsFinalised);
			AssertEquals("Should have created 'finalised' log.", 1,
				stocktake.Logs.Find(Helper.GetLogFilter(Events.ItemDocumentJobFinalised.Code)).Length);

			stocktake.WS_StocktakeCycle = "XX";
			Factory.Save();
			AssertEquals("Should not have created another 'finalised' log.", 1,
				stocktake.Logs.Find(Helper.GetLogFilter(Events.ItemDocumentJobFinalised.Code)).Length);
		}

		#endregion

		#region TestCreateStocktakeLines

		public void TestCreateStocktakeLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// create inventory
			var receive_Finalised = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			CreateWhsReceiveInventoryLine(receive_Finalised, data.Part1, 10m, data.Whs1.DefaultLocation,
				InventoryStatus.Codes.Putaway);
			CreateWhsReceiveInventoryLine(receive_Finalised, data.Part2, 20m, data.Whs1.DefaultLocation,
				InventoryStatus.Codes.Putaway);
			receive_Finalised.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive_Finalised);

			// create stocktake and lines
			var stocktake = Factory.New<WhsStocktake>();
			stocktake.WS_OH_Client = data.Org1.PK;
			stocktake.WS_WW_Whs = data.Whs1.PK;
			var lines = stocktake.CreateStocktakeLines();
			AssertEquals("Precondition - Two stocktake lines should be created.", 2, lines.Count());

			var lineForPart1 = lines.Single(l => l.WU_OP == data.Part1.PK);
			var lineForPart2 = lines.Single(l => l.WU_OP == data.Part2.PK);
			AssertEquals(10m, lineForPart1.WU_SystemUnits);
			AssertEquals(20m, lineForPart2.WU_SystemUnits);
			AssertEquals(0m, lineForPart1.CurrentCount);
			AssertEquals(0m, lineForPart2.CurrentCount);
		}

		public void TestCreateStocktakeLines_WithCommittedStock()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// create inventory
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part2, -3m, inventory2.Location);
			adjustmentLine.RunPreSaveValidation();
			AssertEquals("Precondition: stock is committed.", 3m, adjustmentLine.CommittedQuantity);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine =
				Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, inventory1.Location, inventory1.Location);
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition: stock is committed.", 1m, transferLine.QtyCommittedIncludingMatchingLines);

			var reservedOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(reservedOrder, data.Part1, 2m);
			var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventory1);
			AssertEquals("Precondition: stock is reserved.", 2m, reservedPickLine.ReservedQuantity);

			var pickedOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m,
				WhsPickOption.Codes.Manual);
			var pick = Helper.CreatePickNew(pickedOrder);
			pick.AutoAllocateItemsWithMock();
			var totalPickLineQuantity = Helper.GetTotalPickLineQuantity(pick);
			AssertEquals("Precondition: stock is picked.", 5m, totalPickLineQuantity);

			Factory.Save();

			// create stocktake and lines
			var stocktake = Factory.New<WhsStocktake>();
			stocktake.WS_OH_Client = data.Org1.PK;
			stocktake.WS_WW_Whs = data.Whs1.PK;
			var lines = stocktake.CreateStocktakeLines();
			AssertEquals("Precondition - Two stocktake lines should be created.", 2, lines.Count());

			var lineForPart1 = lines.Single(l => l.WU_OP == data.Part1.PK);
			var lineForPart2 = lines.Single(l => l.WU_OP == data.Part2.PK);
			AssertEquals("6 units are committed.", 10m, lineForPart1.WU_SystemUnits);
			AssertEquals("3 units are committed.", 10m, lineForPart2.WU_SystemUnits);
		}

		#endregion

		#region TestCreateStocktakeLines_WithInTransitUnits

		public void TestCreateStocktakeLines_WithInTransitUnits()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			// create order and pick
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			pick1.FinaliseAllOrders();
			pick1.FinalisePick();
			AssertIsFinalisedPrecondition(order1);
			AssertIsFinalisedPrecondition(pick1);
			Factory.Save();

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine = Helper.CreateWhsOrderLine(order2, data.Part2, 5m);
			var pick2 = Helper.CreatePickNew(order2);
			AssertEquals("Precondition", false, order2.IsFinalised);

			var availableInventory = pick2.OrderedInventories[0].AvailableInventories[0];
			availableInventory.PickLineQuantity = 2m;

			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			availableInventory.PickLineQuantity = 5m;
			Factory.Save();

			var stocktake12 = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var lines12 = stocktake12.CreateStocktakeLines();
			AssertEquals("Precondition - one stocktake lines should be created.", 1, lines12.Count());
			AssertEquals(15m, lines12.Single(l => l.WU_OP == data.Part1.PK).WU_SystemUnits);

			var stocktake22 = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part2);
			var lines22 = stocktake22.CreateStocktakeLines();
			AssertEquals("Precondition - one stocktake lines should be created.", 1, lines22.Count());
			AssertEquals(18m, lines22.Single(l => l.WU_OP == data.Part2.PK).WU_SystemUnits);
		}

		#endregion

		#region TestCreateStocktakeLines_FiltersDockDoorLocations

		public void TestCreateStocktakeLines_FiltersDockDoorLocations()
		{
			var defaultDDLType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, "DOC"));
			var customDDLType = Helper.CreateLocationType("DO2", LocationClasses.Codes.DDL);

			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoor1 = data.Whs1.DefaultOutboundDockDoorLocation;
			var dockDoor2 = data.Whs1.FindLocation("A-2");
			var dockDoor3 = data.Whs1.FindLocation("A-3");
			dockDoor2.WLV_WLT_LocationType = defaultDDLType.PK;
			dockDoor3.WLV_WLT_LocationType = customDDLType.PK;
			Factory.Save();

			var stocktake = Factory.New<WhsStocktake>();
			stocktake.WS_OH_Client = data.Org1.PK;
			stocktake.WS_WW_Whs = data.Whs1.PK;
			stocktake.WS_CountEmptyLocationsCategory = CountEmptyLocationCategory.Codes.IncludeEmptyLocations;

			// Test empty DDL excluded
			var lines1 = stocktake.CreateStocktakeLines();
			AssertEquals("Precondition - One stocktake line should be created.", 1, lines1.Count());
			AssertEquals("Should *not* include Dock Door Locations.", false,
				lines1.Any(l => l.Location.IsDockDoorLocation));

			var receive1 =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, finalise: false);
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m);
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			// HACK: Create AVL inventory in the dock door
			receive1.Lines[0].WE_WL = dockDoor1.PK;
			receive1.Lines[1].WE_WL = dockDoor2.PK;
			receive1.Lines[2].WE_WL = dockDoor3.PK;
			Factory.Save();

			// Test non empty DDL excluded
			var lines2 = stocktake.CreateStocktakeLines();
			AssertEquals("Precondition - One stocktake line should be created.", 1, lines2.Count());
			AssertEquals("Should *not* include Dock Door Locations.", false,
				lines2.Any(l => l.Location.IsDockDoorLocation));
		}

		#endregion

		#region TestCreateStocktakeLines_FiltersPackingStationLocations

		public void TestCreateStocktakeLines_FiltersPackingStationLocations_PackingConsolidation()
		{
			TestCreateStocktakeLines_FiltersPackingLocationsCore(LocationClasses.Codes.CON, (location) => location.IsPackingConsolidationLocation);
		}

		public void TestCreateStocktakeLines_FiltersPackingStationLocations_PackingStation()
		{
			TestCreateStocktakeLines_FiltersPackingLocationsCore(LocationClasses.Codes.PST, (location) => location.IsPackingStationLocation);
		}

		void TestCreateStocktakeLines_FiltersPackingLocationsCore(string locationClass, Func<WhsLocation, bool> isPackingLocationCheck)
		{
			var packingLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, locationClass);

			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var packingLocation1 = data.Whs1.FindLocation("A-3");
			var packingLocation2 = data.Whs1.FindLocation("A-2");
			packingLocation1.WLV_WLT_LocationType = packingLocationType.PK;
			packingLocation2.WLV_WLT_LocationType = packingLocationType.PK;
			Factory.Save();

			var stocktake = Factory.New<WhsStocktake>();
			stocktake.WS_OH_Client = data.Org1.PK;
			stocktake.WS_WW_Whs = data.Whs1.PK;
			stocktake.WS_CountEmptyLocationsCategory = CountEmptyLocationCategory.Codes.IncludeEmptyLocations;

			// Test empty packing locations excluded
			var lines1 = stocktake.CreateStocktakeLines();
			AssertEquals("Precondition - One stocktake line should be created.", 1, lines1.Count());
			AssertEquals("Should *not* include Packing Station Locations.", false,
				lines1.Any(l => isPackingLocationCheck(l.Location)));

			var receive1 =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, finalise: false);
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m);
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			// HACK: Create AVL inventory in the packing location
			receive1.Lines[0].WE_WL = packingLocation1.PK;
			receive1.Lines[1].WE_WL = packingLocation2.PK;
			Factory.Save();

			// Test non empty packing locations excluded
			var lines2 = stocktake.CreateStocktakeLines();
			AssertEquals("Precondition - One stocktake line should be created.", 1, lines2.Count());
			AssertEquals("Should *not* include Packing Locations.", false,
				lines2.Any(l => isPackingLocationCheck(l.Location)));
		}

		#endregion

		#region TestCreateStocktakeLines_Staged

		public void TestCreateStocktakeLines_Staged()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m,
				data.Whs1.FindLocation("A"), "");
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 15m);
			var pick = Helper.CreatePickNew(order1);

			var pickLine = order1.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit,
				transferLine.WE_CurrentInventoryStatus);
			Factory.Save();

			var stocktake = Factory.New<WhsStocktake>();
			stocktake.WS_OH_Client = data.Org1.PK;
			stocktake.WS_WW_Whs = data.Whs1.PK;
			stocktake.WS_CountEmptyLocationsCategory = CountEmptyLocationCategory.Codes.IncludeEmptyLocations;

			var lines1 = stocktake.CreateStocktakeLines();
			AssertEquals("Precondition - One stocktake line should be created.", 1, lines1.Count());
			var lineForLocation1 = lines1.Single(l => l.WU_WL == data.Whs1.FindLocation("A").PK);
			AssertEquals("Stock should *not* be in source location as it is In-Transit.", 5m,
				lineForLocation1.WU_SystemUnits);

			transferLine.FinaliseDocketLine();
			AssertEquals("Precondition - should be Staged.", InventoryStatus.Codes.Staged,
				transferLine.WE_CurrentInventoryStatus);
			Factory.Save();

			var lines2 = stocktake.CreateStocktakeLines();
			AssertEquals("Precondition - One stocktake line should be created.", 1, lines1.Count());
			lineForLocation1 = lines2.Single(l => l.WU_WL == data.Whs1.FindLocation("A").PK);
			AssertEquals(5m, lineForLocation1.WU_SystemUnits);

			// HACK: Put Staged inventory in another location, Staged inventory is not handled by stocktake
			// Test that we don't solely rely on it being in a dock door location
			CargoWise.Database.TestFramework.ObjectModel.WhsDocketLine
				.UpdateWhere(transferLine.PK.ToGuid())
				.Set(l => l.WE_WL, data.Whs1.FindLocation("A").PK.ToGuid())
				.Post(((IDbConnected)Factory).Connection);

			var lines3 = stocktake.CreateStocktakeLines();
			AssertEquals("Precondition - One stocktake line should be created.", 1, lines3.Count());
			lineForLocation1 = lines3.Single(l => l.WU_WL == data.Whs1.FindLocation("A").PK);
			AssertEquals("Not all stock should *not* be in location A as 15m is Staged.", 5m,
				lineForLocation1.WU_SystemUnits);
		}

		#endregion

		#region TestCreateStocktakeLines_DoesNotCreateForInTransit

		public void TestCreateStocktakeLines_DoesNotCreateForInTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			var orderPickLine = orderLine.PickLines.Single();

			var transferLine = Helper.PickAndMakeInTransitTransfer(orderPickLine, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);
			Factory.Save();

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			AssertEquals("Should ignore InTransit lines.", false, stocktake.CreateStocktakeLines().Any());
		}

		#endregion

		#region TestCreateStocktakeLines_SetsLineNoAccordingToLocationSequence

		public void TestCreateStocktakeLines_SetsLineNoAccordingToLocationSequence()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.Rows.Single(r => r.WR_Name == "A").Delete();
			Helper.CreateRowAndGenerateLocations(data.Whs1, "A", 2, 1, 1, 1);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 2, 1, 1, 3);
			Helper.CreateRowAndGenerateLocations(data.Whs1, "C", 2, 1, 1, 2);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "D", 2, 12, 1, 10);
			row.SortPickPathMethod = SortPathMethods.Codes.LevelThenColumn;
			row.UpdatePathSequenceOnLocations();
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m,
				data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m,
				data.Whs1.FindLocation("A-2"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m,
				data.Whs1.FindLocation("B-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part1, 10m,
				data.Whs1.FindLocation("B-2"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m,
				data.Whs1.FindLocation("C-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R6", data.Part1, 10m,
				data.Whs1.FindLocation("C-2"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R7", data.Part1, 10m,
				data.Whs1.FindLocation("D-1-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R8", data.Part1, 10m,
				data.Whs1.FindLocation("D-2-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R9", data.Part1, 10m,
				data.Whs1.FindLocation("D-1-2"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R10", data.Part1, 10m,
				data.Whs1.FindLocation("D-1-10"), "");
			Factory.Save();

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			stocktake.CreateStocktakeLines();

			var lineA_1 = stocktake.Lines.First(l => l.LocationString == "A-1");
			var lineA_2 = stocktake.Lines.First(l => l.LocationString == "A-2");
			var lineB_1 = stocktake.Lines.First(l => l.LocationString == "B-1");
			var lineB_2 = stocktake.Lines.First(l => l.LocationString == "B-2");
			var lineC_1 = stocktake.Lines.First(l => l.LocationString == "C-1");
			var lineC_2 = stocktake.Lines.First(l => l.LocationString == "C-2");
			var lineD_1_1 = stocktake.Lines.First(l => l.LocationString == "D-1-1");
			var lineD_1_2 = stocktake.Lines.First(l => l.LocationString == "D-1-2");
			var lineD_1_10 = stocktake.Lines.First(l => l.LocationString == "D-1-10");
			var lineD_2_1 = stocktake.Lines.First(l => l.LocationString == "D-2-1");
			AssertSequencesEqual(
				new[]
				{
					lineA_1, lineA_2, lineC_1, lineC_2, lineB_1, lineB_2, lineD_1_1, lineD_2_1, lineD_1_2,
					lineD_1_10
				}, stocktake.Lines.OrderBy(l => l.WU_LineNo));
		}

		#endregion

		#region TestDelete_DeletesAllRelatedJobHeaders

		public void TestDelete_DeletesAllRelatedJobHeaders()
		{
			var stocktake = GetNewBusinessObject();
			var jobHeader1 = Helper.CreateRatingJob(stocktake, "SK");
			var jobHeader2 = Helper.CreateRatingJob(stocktake, "SK");
			AssertEquals("Precondition", false, stocktake.IsDeleted);
			AssertEquals("Precondition", false, jobHeader1.IsDeleted);
			AssertEquals("Precondition", false, jobHeader2.IsDeleted);

			stocktake.Delete();
			AssertEquals(true, stocktake.IsDeleted);
			AssertEquals(true, jobHeader1.IsDeleted);
			AssertEquals(true, jobHeader2.IsDeleted);
		}

		public void TestDelete_WhatHappensIfJobIsInDb()
		{
			var expectedExceptionMessage = "You cannot delete Job in Database.";

			var stocktake = GetNewBusinessObject();
			var jobHeader = Helper.CreateRatingJob(stocktake, "SK");
			AssertEquals("Precondition", false, stocktake.IsDeleted);
			AssertEquals("Precondition", false, jobHeader.IsDeleted);

			Factory.Save();

			var exception = AssertExceptionThrown<InvalidOperationException>(() => stocktake.Delete());

			AssertContains("Exception Message", expectedExceptionMessage, exception.Message);
			AssertEquals(false, stocktake.IsDeleted);
			AssertEquals(false, jobHeader.IsDeleted);
		}

		#endregion

		#region IJobInvoicingPlugIn Members

		public void TestIJobInvoicingPlugIn_DefaultCreditor()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(null,
				jobInvPlugIn.InvoicingSupporter.GetDefaultCreditor(new DefaultCreditorSetting(null, null)));
		}

		public void TestIJobInvoicingPlugIn_OperationsBranch()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(Env.Security.None, jobInvPlugIn.InvoicingSupporter.AuditSecurity);
			Stocktake.WS_WW_Whs = Helper.CreateWarehouse("1").PK;
			Stocktake.Warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
			AssertEquals(GlbBranch.CurrentBranch.PK, jobInvPlugIn.InvoicingSupporter.OperationsBranch.PK);
		}

		public void TestIJobInvoicingPlugIn_AuditSecurity()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(Env.Security.None, jobInvPlugIn.InvoicingSupporter.AuditSecurity);
		}

		public void TestIJobInvoicingPlugIn_JobInvoicingSecurity()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(Env.Security.WhsStocktakeJobInvoicing, jobInvPlugIn.InvoicingSupporter.JobInvoicingSecurity);
		}

		public void TestIJobInvoicingPlugIn_OverriddenDepartmentPK()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(ZGuid.Empty, jobInvPlugIn.InvoicingSupporter.OverriddenDepartmentPK);
		}

		public void TestIJobInvoicingPlugIn_Origin()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(null, jobInvPlugIn.InvoicingSupporter.Origin);
		}

		public void TestIJobInvoicingPlugIn_Destination()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(null, jobInvPlugIn.InvoicingSupporter.Destination);
		}

		public void TestIJobInvoicingPlugIn_TranshipmentPort()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(null, jobInvPlugIn.InvoicingSupporter.GetTranshipmentPort(CostSell.Cost));
			AssertEquals(null, jobInvPlugIn.InvoicingSupporter.GetTranshipmentPort(CostSell.Revenue));
		}

		public void TestIJobInvoicingPlugIn_ConsolRateCurrency()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(null, jobInvPlugIn.InvoicingSupporter.ConsolRateCurrency);
		}

		public void TestIJobInvoicingPlugIn_ActualChargeable()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(0m, jobInvPlugIn.InvoicingSupporter.ActualChargeable);
		}

		public void TestIJobInvoicingPlugIn_ActualChargeableUnit()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(ZString.Empty, jobInvPlugIn.InvoicingSupporter.ActualChargeableUnit);
		}

		public void TestIJobInvoicingPlugIn_TransportMode()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(ZString.Empty, jobInvPlugIn.InvoicingSupporter.TransportMode);
		}

		public void TestIJobInvoicingPlugIn_IsDirectShipment()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(false, jobInvPlugIn.InvoicingSupporter.IsDirectShipment);
		}

		public void TestIJobInvoicingPlugIn_SendingAgent()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(null, jobInvPlugIn.InvoicingSupporter.SendingAgent);
		}

		public void TestIJobInvoicingPlugIn_ContainerMode()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(ZString.Empty, jobInvPlugIn.InvoicingSupporter.ContainerMode);
		}

		public void TestIJobInvoicingPlugIn_IsPlugInReadOnly()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(false, jobInvPlugIn.InvoicingSupporter.IsPlugInReadOnly);
		}

		public void TestIJobInvoicingPlugIn_JobNumber()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			Stocktake.WS_StocktakeNumber = "00001234";
			AssertEquals("00001234", jobInvPlugIn.JobNumber);
		}

		public void TestIJobInvoicingPlugIn_ReceivingAgent()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(null, jobInvPlugIn.InvoicingSupporter.ReceivingAgent);
		}

		public void TestIJobInvoicingPlugIn_IsImport()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(false, jobInvPlugIn.InvoicingSupporter.IsImport);
		}

		public void TestIJobInvoicingPlugIn_ConsolExchangeRate()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(0m, jobInvPlugIn.InvoicingSupporter.ConsolExchangeRate);
		}

		public void TestIJobInvoicingPlugIn_Consignor()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(null, jobInvPlugIn.InvoicingSupporter.Consignor);

			OrgHeader client = Helper.CreateClient();
			Stocktake.WS_OH_Client = client.PK;
			AssertEquals(client, jobInvPlugIn.InvoicingSupporter.Consignor);
		}

		public void TestIJobInvoicingPlugIn_Consignee()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(null, jobInvPlugIn.InvoicingSupporter.Consignee);
		}

		public void TestIJobInvoicingPlugIn_ConsumerType()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(JobInvoicingConsumerTypes.WarehouseStocktake, jobInvPlugIn.InvoicingSupporter.ConsumerType);
		}

		public void TestIJobInvoicingPlugIn_ShipmentNumberOfColoadMaster()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(ZString.Empty, jobInvPlugIn.InvoicingSupporter.ShipmentNumberOfColoadMaster);
		}

		public void TestIJobInvoicingPlugIn_HouseBillNumber()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(ZString.Empty, jobInvPlugIn.InvoicingSupporter.HouseBillNumber);
		}

		public void TestIJobInvoicingPlugIn_MasterBillNumber()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(ZString.Empty, jobInvPlugIn.InvoicingSupporter.MasterBillNumber);
		}

		public void TestIJobInvoicingPlugIn_ETA()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(ZDateTime.Empty, jobInvPlugIn.InvoicingSupporter.ETA);
		}

		public void TestIJobInvoicingPlugIn_ETD()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(ZDateTime.Empty, jobInvPlugIn.InvoicingSupporter.ETD);
		}

		public void TestIJobInvoicingPlugIn_ActualWeight()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(0m, jobInvPlugIn.InvoicingSupporter.ActualWeight);
		}

		public void TestIJobInvoicingPlugIn_ActualWeightUnit()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(ZString.Empty, jobInvPlugIn.InvoicingSupporter.ActualWeightUnit);
		}

		public void TestIJobInvoicingPlugIn_ActualVolume()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(0m, jobInvPlugIn.InvoicingSupporter.ActualVolume);
		}

		public void TestIJobInvoicingPlugIn_ActualVolumeUnit()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(ZString.Empty, jobInvPlugIn.InvoicingSupporter.ActualVolumeUnit);
		}

		public void TestIJobInvoicingPlugIn_CreateAccountingJobOnSavingOfOperationsJob()
		{
			IJobInvoicingPlugIn jobInvPlugIn = Stocktake;
			AssertEquals(true, jobInvPlugIn.InvoicingSupporter.CreateAccountingJobOnSavingOfOperationsJob);
		}

		public void TestIJobInvoicingPlugIn_EditSecurity()
		{
			IJobInvoicingPlugIn testJob = Factory.New<WhsStocktake>();
			AssertEquals("EditSecurityCheckPoint should be Empty", Env.Security.None,
				testJob.InvoicingSupporter.EditSecurityCheckpoint);
			AssertEquals("EditSecuritytMessage should be Empty", ZString.Empty,
				testJob.InvoicingSupporter.EditSecurityMessage);
			AssertEquals("EditSecurityLock should be False", ZBool.False, testJob.InvoicingSupporter.EditSecurityLock);
		}

		public void TestIJobInvoicingPlugIn_DefaultChargeGroup()
		{
			IJobInvoicingPlugIn testJob = Factory.New<WhsStocktake>();
			AssertEquals("DefaultChargeGroup should be Empty", ZString.Empty,
				testJob.InvoicingSupporter.DefaultChargeGroup);
		}

		#endregion

		#region IJobHeaderParent Members

		#region TestIJobHeaderParent_SetJobNumberFieldOnSaving

		public void TestIJobHeaderParent_SetJobNumberFieldOnSaving()
		{
			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				var iJobHeader = (IJobHeaderParent)Stocktake;
				AssertEquals("Precondition", true, Stocktake.WS_StocktakeNumber.IsEmpty);

				iJobHeader.SetJobNumberFieldOnSaving();
				AssertEquals(
					"Stocktake No. must be set, otherwise we rely on the save order between JobHeader and WhsStocktake (to ensure a correct JobHeader number).",
					"SK00000001", Stocktake.WS_StocktakeNumber);

				Stocktake.Factory.Save();
				AssertEquals(
					"Stocktake No. should not have been set by OnSaving() because it was already set by an IJobHeader method.",
					"SK00000001", Stocktake.WS_StocktakeNumber);
			}
		}

		#endregion

		#region TestIJobHeaderParent_AllowInvoiceDeletion

		public void TestIJobHeaderParent_AllowInvoiceDeletion()
		{
			var docket = (IJobHeaderParent)GetNewBusinessObject();
			Assert(docket.AllowInvoiceDeletion);
		}

		#endregion

		#endregion

		#region IDocManagerSupport Members

		public void TestDocManagerInfo()
		{
			DocManagerInfo info = Stocktake.DocManagerInfo;
			AssertEquals(Stocktake, info.BusinessEntity);
			AssertEquals(Core.Constants.DocManagerCodes.WarehouseStocktake, info.DocManagerCode);
		}

		#endregion

		#region IDocumentSupportable Members

		public void TestDocumentSupporter()
		{
			AssertEquals(typeof(WhsStocktakeDocumentSupporter),
				Stocktake.DocumentSupporter.GetType());
		}

		#endregion

		#region ISendEmailSource members

		public void TestEmailSubject()
		{
			AssertEquals("Stocktake - " + Stocktake.WS_StocktakeNumber, ((ISendEmailSource)Stocktake).EmailSubject);
		}

		public void TestGetAddressBookSelection()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var contact = client.Contacts.AddNew();
			Stocktake.WS_OH_Client = client.PK;

			AddressBookSelection selection = ((ISendEmailSource)Stocktake).GetAddressBookSelection();
			AssertEquals(1, selection.Recipients.Count);
			Assert(selection.Recipients.Contains(contact));
		}

		#endregion

		#region TestIMasterAssignerMembers

		public void TestIMasterAssignerMembers()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			stocktake.Lines.AddNew();
			stocktake.Lines.AddNew();

			var masterAssigner = (IMasterStaffAssigner)stocktake;

			AssertEquals(stocktake.Lines, masterAssigner.Lines);
			AssertEquals(stocktake.Factory, masterAssigner.Factory);
			AssertEquals(true, masterAssigner.IsJobAssignable);

			stocktake.WS_StocktakeStatus = StocktakeStatus.Codes.Finalised;
			AssertEquals(stocktake.Lines, masterAssigner.Lines);
			AssertEquals(stocktake.Factory, masterAssigner.Factory);
			AssertEquals(false, masterAssigner.IsJobAssignable);
		}

		#region TestImasterAssignerMembers_CanLinesAssignable

		public void TestImasterAssignerMembers_CanLinesAssignableCount1()
		{
			AssertCanLinesAssignable(1);
		}

		public void TestImasterAssignerMembers_CanLinesAssignableCount2()
		{
			AssertCanLinesAssignable(2);
		}

		public void TestImasterAssignerMembers_CanLinesAssignableCount3()
		{
			AssertCanLinesAssignable(3);
		}

		public void AssertCanLinesAssignable(ZByte countNumber)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.Loaded);
			var stocktakeNew = Helper.CreateWhsStocktake(data.Org1, data.Whs1, StocktakeStatus.Codes.New);

			var openLine1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation,
				1m, countNumber, StocktakeLineStatus.Codes.Open);
			var openLine2 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part2, data.Whs1.DefaultLocation,
				1m, countNumber, StocktakeLineStatus.Codes.Open);
			var closedLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation,
				1m, countNumber, StocktakeLineStatus.Codes.Closed);

			var assigner = (IMasterStaffAssigner)stocktake;

			// All un-assigned lines
			AssertEquals(true, assigner.CanAssignOrUnAssignAnyLines);

			// One assigned and un-assigned line
			openLine1.CurrentCountVerifiedDate = ZDateTime.Now;
			AssertEquals(true, assigner.CanAssignOrUnAssignAnyLines);

			// All un-assigned lines
			openLine2.CurrentCountVerifiedDate = ZDateTime.Now;
			AssertEquals(false, assigner.CanAssignOrUnAssignAnyLines);

			// finalised stocktake
			openLine1.CurrentCountVerifiedDate = ZDateTime.Empty;
			openLine2.CurrentCountVerifiedDate = ZDateTime.Empty;
			openLine1.WU_Status = StocktakeLineStatus.Codes.Closed;
			openLine2.WU_Status = StocktakeLineStatus.Codes.Closed;
			stocktake.WS_StocktakeStatus = StocktakeStatus.Codes.Finalised;
			AssertEquals(false, assigner.CanAssignOrUnAssignAnyLines);

			// New Stocktake
			AssertEquals(false, ((IMasterStaffAssigner)stocktakeNew).CanAssignOrUnAssignAnyLines);
		}

		#endregion

		#endregion

		#region IWhsLogEventParent

		public void TestEventFreeTextReference()
		{
			var logParent = (IWhsLogEventParent)Stocktake;
			AssertEquals(Stocktake.WS_StocktakeNumber, logParent.EventFreeTextReference);
		}

		public void TestEventReferenceParameterType()
		{
			var logParent = (IWhsLogEventParent)Stocktake;
			AssertEquals(Constants.EventReferenceParameterTypes.Stocktake, logParent.EventReferenceParameterType);
		}

		public void TestIWhsLogEventParentWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Stocktake;
			stocktake.WS_WW_Whs = data.Whs1.PK;
			IWhsLogEventParent logParent = stocktake;
			AssertEquals(data.Whs1, logParent.Warehouse);
		}

		#endregion

		#region TestILocationConsumer

		public void TestILocationConsumer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Factory.Save();

			var stocktake = Factory.New<WhsStocktake>();
			var locationPK = data.Whs1.DefaultLocation.PK;
			stocktake.WS_WL_Location = locationPK;
			var locationConsumer = (ILocationConsumer)stocktake;
			AssertEquals("Stocktake", locationConsumer.LocationTypeForMessages);
			AssertEquals(locationPK, locationConsumer.LocationPK);

			locationConsumer.LocationTitle = "Test";
			AssertEquals("Test", locationConsumer.LocationTitle);
		}

		#endregion

		#region TestIsNew

		public void TestIsNew()
		{
			var stocktake = Factory.New<WhsStocktake>();

			stocktake.WS_StocktakeStatus = StocktakeStatus.Codes.New;
			AssertEquals(true, stocktake.IsNew);

			stocktake.WS_StocktakeStatus = StocktakeStatus.Codes.Loaded;
			AssertEquals(false, stocktake.IsNew);

			stocktake.WS_StocktakeStatus = StocktakeStatus.Codes.Finalised;
			AssertEquals(false, stocktake.IsNew);
		}

		#endregion

		#region TestResetLocationIfWarehouseIsChanged

		public void TestResetLocationIfWarehouseIsChanged()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = Helper.CreateWarehouse("WHS");

			var stocktake = Factory.New<WhsStocktake>();
			stocktake.WS_WW_Whs = data.Whs1.PK;
			stocktake.LocationString = "A";

			stocktake.WS_WW_Whs = data.Whs1.PK;
			AssertEquals("A", stocktake.LocationString);
			AssertEquals(data.Whs1.FindLocation("A").PK, stocktake.WS_WL_Location);

			stocktake.WS_WW_Whs = warehouse.PK;
			AssertEquals("", stocktake.LocationString);
			AssertEquals(ZGuid.Empty, stocktake.WS_WL_Location);
		}

		#endregion

		#region TestStocktakeTypeDefaultValue

		public void TestStocktakeTypeDefaultValue()
		{
			var stocktake = Factory.New<WhsStocktake>();
			AssertEquals(WarehouseDataRegistry.Instance.StocktakeTypes.Value.DefaultCode, stocktake.WS_StocktakeType);
		}

		#endregion

		#region Implementation

		WhsStocktakeLine FindLine(WhsStocktake stocktake, OrgSupplierPart part, ZString inventoryStatus)
		{
			return stocktake.Lines.Single(l => l.WU_OP == part.PK && l.WU_InventoryStatus == inventoryStatus);
		}

		WhsStocktakeLine FindLine(WhsStocktake stocktake, OrgSupplierPart part, ZString inventoryStatus, ZString at1,
			ZString at2, ZString at3)
		{
			return stocktake.Lines.Single(l =>
				l.WU_OP == part.PK && l.WU_InventoryStatus == inventoryStatus && l.WU_PartAttrib1 == at1 &&
				l.WU_PartAttrib2 == at2 && l.WU_PartAttrib3 == at3);
		}

		WhsStocktakeLine FindLine(WhsStocktake stocktake, OrgSupplierPart part, ZString inventoryStatus, ZString at1,
			ZString at2, ZString at3, ZString ser)
		{
			return stocktake.Lines.Single(l =>
				l.WU_OP == part.PK && l.WU_InventoryStatus == inventoryStatus && l.WU_PartAttrib1 == at1 &&
				l.WU_PartAttrib2 == at2 && l.WU_PartAttrib3 == at3 && l.WU_SerialNumber == ser);
		}

		int FindInv(WhsInventoryViewCollection inv, OrgSupplierPart part)
		{
			for (int i = 0; i < inv.Count; i++)
			{
				if (inv[i].WI_OP == part.PK)
				{
					return i;
				}
			}

			return -1;
		}

		new WhsStocktake GetNewBusinessObject()
		{
			return (WhsStocktake)base.GetNewBusinessObject();
		}

		WhsStocktake Stocktake
		{
			get { return stocktake ?? (stocktake = GetNewBusinessObject()); }
			set { stocktake = value; }
		}

		WhsStocktake stocktake;

		#endregion

		#region INumberFountainConsumer

		public void TestINumberFountainConsumer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			AssertEquals("WhsStocktake uses correct Fountain.", Env.NumberFountains.WarehouseStocktakeNumber, ((INumberFountainConsumer)stocktake).Fountain);

			stocktake.WS_StocktakeNumber = "SK01";
			AssertEquals("ID refers to correct Field.", "SK01", ((INumberFountainConsumer)stocktake).ID);

			((INumberFountainConsumer)stocktake).ID = "SK02";
			AssertEquals("ID refers to correct Field.", "SK02", stocktake.WS_StocktakeNumber);
		}

		public void TestINumberFountainConsumer_SavingSetsJobID()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);

			AssertEquals("Precondition: Stocktake Number is empty.", "", stocktake.WS_StocktakeNumber);

			Factory.Save();
			AssertNotEquals("Stocktake Number should not be empty.", "", stocktake.WS_StocktakeNumber);
			AssertEquals("Stocktake Number was set correctly.", "SK00000001", stocktake.WS_StocktakeNumber);
		}

		#endregion
	}
}
