using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	[HttpContextEnabledTest]
	sealed class TrackingWhsInventoryCollectionFetchStrategyTest : TestCaseWithFactory
	{
		#region TestAddFetchHints

		public void TestAddFetchHints()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var whs1 = helper.CreateWarehouse("WH1", "A", 2, 1);
			var whs2 = helper.CreateWarehouse("WH2", "A");
			var org1 = helper.CreateClient("CLIENT1");
			var org2 = helper.CreateClient("CLIENT2");
			var osp1 = helper.CreateProduct(org1, "P1");
			var osp2 = helper.CreateProduct(org2, "P2");
			helper.CreateProductClientRelationShip(org1, osp2);

			var receive1 = CreateWhsReceiveWithInventory(org1, whs1, "R1", osp1, 10m, whs1.DefaultLocation, ZDateTimeOffset.Today);
			var receive2 = CreateWhsReceiveWithInventory(org2, whs2, "R2", osp2, 10m, whs2.DefaultLocation, ZDateTimeOffset.Today);
			var inventory1 = receive1.Inventory[0];
			var inventory2 = CreateWhsInventory(receive1, osp2, 10m, whs1.DefaultLocation);
			var inventory3 = receive2.Inventory[0];
			var inventory4 = CreateWhsInventory(receive1, osp1, 5m, whs1.DefaultLocation);

			var order1 = TrackingHelper.Get(Factory.NewWithValidTestData<WhsOrder>()).WhsOrder;
			var order2 = TrackingHelper.Get(Factory.NewWithValidTestData<WhsOrder>()).WhsOrder;
			var orderLine1 = TrackingHelper.Get(Factory.NewWithValidTestData<WhsOrderLine>()).WhsOrderLine;
			var orderLine2 = TrackingHelper.Get(Factory.NewWithValidTestData<WhsOrderLine>()).WhsOrderLine;
			orderLine1.WE_WD = order1.PK;
			orderLine2.WE_WD = order2.PK;

			var sn1 = Factory.New<StmNote>();
			var sn2 = Factory.New<StmNote>();

			orderLine1.ReserveStockIfAbleTo(inventory1);
			orderLine2.ReserveStockIfAbleTo(inventory2);
			orderLine2.ReserveStockIfAbleTo(inventory3);

			sn1.ST_ParentID = inventory1.WI_WE_InDocketLine;
			sn1.ST_Table = inventory1.InDocketLine.TableName;

			sn2.ST_ParentID = inventory3.WI_WE_InDocketLine;
			sn2.ST_Table = inventory3.InDocketLine.TableName;

			Factory.Save();

			var viewFactory = new BusinessObjectFactory();

			AssertEquals("Precondition: Should be zero db hit count before loading inventories", 0, viewFactory.DatabaseLoadCount);

			var summaries = new TrackingInventorySummaryCollection(viewFactory);

			var expectedLoadDBHits = new Dictionary<string, int>()
			{
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 2 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 0 },
				{ OrgHeaderSchema.Constants.TableName, 0 },
				{ OrgPartRelationSchema.Constants.TableName, 0 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
			};
			using (AssertDbHitsWithUsefulQueryInformation(expectedLoadDBHits, viewFactory))
			{
				summaries.Load();
				summaries.OfType<TrackingInventorySummary>().LoadInventories();
				AssertEquals(3, summaries.Count);
			}

			var expectedDBHits = new Dictionary<string, int>()
			{
				{ OrgHeaderSchema.Constants.TableName, 2 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 2 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, viewFactory))
			{
				var orderedSummaries = summaries.Cast<TrackingInventorySummary>().OrderByDescending(i => i.ProductCode).ThenBy(i => i.WarehouseName); // Ensure deterministic order
				foreach (var summary in orderedSummaries)
				{
					foreach (TrackingWhsInventory inventory in summary.Inventories)
					{
						AssertNotNull(inventory.SupplierPart);
						AssertNotNull(inventory.Location);
						AssertNotNull(inventory.Warehouse);
						AssertNotNull(inventory.InDocketLine);
						AssertNotNull(inventory.InDocketLine.Notes);
						AssertNotNull(inventory.SupplierPart.RelatedOrganisations[0].Organisation.MiscServ);
					}
				}
			}
		}

		#endregion

		#region CreateWhsReceiveWithInventory

		TrackingWhsReceive CreateWhsReceiveWithInventory(OrgHeader client, WhsWarehouse whs, string externalReference, OrgSupplierPart part, decimal units, WhsLocation location, ZDateTimeOffset? arrivalDate = null)
		{
			var receive = TrackingHelper.Get(Factory.NewWithValidTestData<WhsReceive>());
			receive.WhsReceive.WD_OH_Client = client.PK;
			receive.WhsReceive.WD_WW_Whs = whs.PK;
			receive.WhsReceive.WD_ExternalReference = externalReference;
			receive.WhsReceive.WD_ArrivalDate = arrivalDate ?? ZDateTimeOffset.Empty;

			CreateWhsInventory(receive, part, units, location);

			return receive;
		}

		#endregion

		#region CreateWhsInventory

		TrackingWhsInventory CreateWhsInventory(TrackingWhsReceive receive, OrgSupplierPart part, decimal units, WhsLocation location)
		{
			var inventory = receive.Lines.AddNew().Inventory[0];
			inventory.WI_OP = part.PK;
			inventory.WI_InDocketLineUnits = units;
			inventory.WI_WL = location.PK;

			return inventory;
		}

		#endregion

		#region TestAddFetchHints_PeformanceOfDBHits

		public void TestAddFetchHints_PeformanceOfDBHits()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var whs1 = helper.CreateWarehouse("WH1", "A", 2, 1);
			var whs2 = helper.CreateWarehouse("WH2", "A");
			var org1 = helper.CreateClient("CLIENT1");
			var org2 = helper.CreateClient("CLIENT2");
			var osp1 = helper.CreateProduct(org1, "P1");
			var osp2 = helper.CreateProduct(org2, "P2");
			helper.CreateProductClientRelationShip(org1, osp2);

			var receive1 = CreateWhsReceiveWithInventory(org1, whs1, "R1", osp1, 10m, whs1.DefaultLocation, ZDateTimeOffset.Today);
			var receive2 = CreateWhsReceiveWithInventory(org2, whs2, "R2", osp2, 10m, whs2.DefaultLocation, ZDateTimeOffset.Today);
			var inventory1 = receive1.Inventory[0];
			var inventory2 = CreateWhsInventory(receive1, osp2, 10m, whs1.DefaultLocation);
			var inventory3 = receive2.WhsReceive.Inventory[0];
			var inventory4 = CreateWhsInventory(receive1, osp1, 5m, whs1.DefaultLocation);

			var order1 = TrackingHelper.Get(Factory.NewWithValidTestData<WhsOrder>()).WhsOrder;
			var order2 = TrackingHelper.Get(Factory.NewWithValidTestData<WhsOrder>()).WhsOrder;
			var orderLine1 = TrackingHelper.Get(Factory.NewWithValidTestData<WhsOrderLine>()).WhsOrderLine;
			var orderLine2 = TrackingHelper.Get(Factory.NewWithValidTestData<WhsOrderLine>()).WhsOrderLine;
			orderLine1.WE_WD = order1.PK;
			orderLine2.WE_WD = order2.PK;

			var sn1 = Factory.New<StmNote>();
			var sn2 = Factory.New<StmNote>();

			orderLine1.ReserveStockIfAbleTo(inventory1);
			orderLine2.ReserveStockIfAbleTo(inventory2);
			orderLine2.ReserveStockIfAbleTo(inventory3);

			sn1.ST_ParentID = inventory1.WI_WE_InDocketLine;
			sn1.ST_Table = inventory1.InDocketLine.TableName;

			sn2.ST_ParentID = inventory3.WI_WE_InDocketLine;
			sn2.ST_Table = inventory3.InDocketLine.TableName;

			Factory.Save();

			var expectedLoadDBHits = new Dictionary<string, int>()
			{
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 2 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 0 },
				{ OrgHeaderSchema.Constants.TableName, 0 },
				{ OrgPartRelationSchema.Constants.TableName, 0 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
			};

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };

			AssertEquals("Precondition: Should be zero db hit count before loading inventories", 0, otherFactory.DatabaseLoadCount);
			var summariesInOtherFactory = new TrackingInventorySummaryCollection(otherFactory);
			using (AssertDbHitsWithUsefulQueryInformation(expectedLoadDBHits, otherFactory))
			{
				summariesInOtherFactory.Load();
				summariesInOtherFactory.OfType<TrackingInventorySummary>().LoadInventories();
				AssertEquals(3, summariesInOtherFactory.Count);
			}

			var expectedDBHits = new Dictionary<string, int>()
			{
				{ OrgHeaderSchema.Constants.TableName, 2 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ StmNoteSchema.Constants.TableName, 2 },
				{ WhsDocketSchema.Constants.TableName, 2 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, otherFactory))
			{
				var orderedSummaries = summariesInOtherFactory.Cast<TrackingInventorySummary>().OrderByDescending(i => i.ProductCode).ThenBy(i => i.WarehouseName);
				foreach (var summary in orderedSummaries)
				{
					foreach (TrackingWhsInventory inventory in summary.Inventories)
					{
						AssertNotNull(inventory.SupplierPart);
						AssertNotNull(inventory.Location);
						AssertNotNull(inventory.Warehouse);
						AssertNotNull(inventory.InDocketLine);
						AssertNotNull(inventory.InDocketLine.Notes);
						AssertNotNull(inventory.SupplierPart.RelatedOrganisations[0].Organisation.MiscServ);
						AssertNotNull(inventory.HasEDocsOrNotesAttached);
					}
				}
			}
		}

		#endregion

		#region TestAddFetchHintsUseTVP

		public void TestAddFetchHintsUseTVP()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			var year = ZDate.Today.Year;
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1);
			helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var viewFactory = new BusinessObjectFactory();
			var summaries = new TrackingInventorySummaryCollection(viewFactory);
			summaries.Load();
			using (TestConnection.TrackExecutedCommands())
			{
				using (var settings = TestEntityFrameworkSettings.Get())
				{
					settings.TVPRule = new TVPRule("0");

					summaries.OfType<TrackingInventorySummary>().LoadInventories();
					viewFactory.ExecuteAllFetchHints();
					var commands = TestConnection.ExecutedCommands;

					var stmNoteQueryCommand = commands.SingleOrDefault(c => c.IndexOf("FROM dbo.StmNote\r\n\tWHERE (ST_ParentID in", StringComparison.OrdinalIgnoreCase) >= 0);
					AssertContains("stmNote query should use TVP.", "ST_ParentID in (SELECT Value FROM", stmNoteQueryCommand);
					var pickLineQueryCommand = commands.SingleOrDefault(c => c.IndexOf("FROM dbo.WhsPickLine\r\n\tWHERE (WZ_WE_InventoryLine in", StringComparison.OrdinalIgnoreCase) >= 0);
					AssertContains("pickLine query should use TVP.", "WZ_WE_InventoryLine in (SELECT Value FROM", pickLineQueryCommand);
					var ratingDocketLineQueryCommand = commands.SingleOrDefault(c => c.IndexOf("SELECT WE_WD FROM dbo.WhsDocketLine WHERE (WE_PK in", StringComparison.OrdinalIgnoreCase) >= 0);
					AssertContains("ratingDocketLine query should use TVP.", "(WE_PK in (SELECT Value FROM", ratingDocketLineQueryCommand);
					var docketQueryCommand = commands.SingleOrDefault(c => c.IndexOf("FROM dbo.WhsDocket\r\n\tWHERE (WD_PK in", StringComparison.OrdinalIgnoreCase) >= 0);
					AssertContains("docketQuery query should use TVP.", "(WD_PK in (SELECT Value FROM", docketQueryCommand);
				}
			}
		}

		#endregion

		#region Test Setup

		bool oldIsWeb;

		protected override void SetUp()
		{
			base.SetUp();
			oldIsWeb = Globals.IsWeb;
			Globals.IsWeb = true;
		}

		protected override void TearDown()
		{
			Globals.IsWeb = oldIsWeb;
			base.TearDown();
		}

		#endregion
	}
}
