using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingInventorySummaryCollection))]
	[SetGlobalsIsWeb]
	sealed class TrackingInventorySummaryCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TrackingInventorySummaryCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals("AllowNew", false, Collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			AssertEquals("AllowRemove", false, Collection.AllowRemove);
		}

		public void TestLoad()
		{
			SetupTestData();
			var summaries = new TrackingInventorySummaryCollection(Factory);
			AssertEquals("Collection should be empty initially", 0, summaries.Count);

			summaries.Load();
			AssertEquals("Collection should contain 7 unique combinations of Client, Warehouse and Product.", 7, summaries.Count);

			var filter = new ZQuery(WhsInventoryViewSchema.WI_OH_Client, Client1.PK);
			filter.AddToFilter(WhsInventoryViewSchema.WI_OP, Part1.PK);

			summaries.Load(filter);
			AssertEquals("Should match 2 items", 2, summaries.Count);
			summaries.OfType<TrackingInventorySummary>().LoadInventories();
			AssertEquals(true, summaries.OfType<TrackingInventorySummary>().Any(s => s.WI_TotalUnits == 55m && s.Inventories.Count > 1));
			AssertEquals(true, summaries.OfType<TrackingInventorySummary>().Any(s => s.WI_TotalUnits == 10m && s.Inventories.Count == 1));

			filter = new ZQuery(WhsInventoryViewSchema.WI_OH_Client, Client2.PK);
			filter.AddToFilter(WhsInventoryViewSchema.WI_OP, Part3.PK);
			summaries.Load(filter);
			AssertEquals("Should match 2 items", 2, summaries.Count);

			var summary1 = summaries[0];
			var summary2 = summaries[1];
			AssertEquals("The key properties of two Summaries are identical", false,
				summary1.WI_OH_Client.Equals(summary2.WI_OH_Client) &&
				summary1.WI_WW_Whs.Equals(summary2.WI_WW_Whs) &&
				summary1.WI_OP.Equals(summary2.WI_OP) &&
				summary1.WI_UnitsUQ.Equals(summary2.WI_UnitsUQ) &&
				summary1.WI_ClientUQ.Equals(summary2.WI_ClientUQ));

			var whs2Summary = summaries.OfType<TrackingInventorySummary>().Single(s => s.WI_WW_Whs == Warehouse2.PK);
			AssertEquals("Second Warehouse", Warehouse2.PK, whs2Summary.Warehouse.PK);
			AssertEquals("WI_TotalUnits should be correct", 45m, whs2Summary.WI_TotalUnits);
			AssertEquals("CommittedToTransactionQuantity should be correct", 17m, whs2Summary.WI_CommittedUnits);
			AssertEquals("WI_AvailableToPickQuantity should be correct", 28m, whs2Summary.WI_AvailableUnits);
			AssertEquals("WI_CrossDockQuantity should be correct", 0m, whs2Summary.WI_CrossDockQuantity);
		}

		public void TestLoadInTransitLines()
		{
			var nonSupportUser = WarehouseHelper.CreateGlbStaff("NS", "notsupport");
			Factory.Save();

			using (Env.SetTemporaryUserContext(nonSupportUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var data = new TestDataSimpleEnvironment(Factory);

				var receive1 = WarehouseHelper.CreateWhsReceive(data.Org1, data.Whs1, "1", new TestNotificationBuffer());
				var line1 = WarehouseHelper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 3m);
				var line2 = WarehouseHelper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 2m);
				var inventory = receive1.Inventory[0];
				receive1.AllocateLocationsWithMock();
				receive1.FinaliseDocket();
				Factory.Save();

				var transfer = WarehouseHelper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
				var transferLine1 = WarehouseHelper.CreateWhsTransferLine(transfer, data.Part1, 1m, inventory.LocationString, "");
				AssertEquals("Precondition: Inventory should Available.", "AVL", transferLine1.WE_CurrentInventoryStatus);

				transferLine1.PickedTime = ZDateTimeOffset.Now;
				AssertEquals("Precondition: Inventory should In Transit.", "INT", transferLine1.WE_CurrentInventoryStatus);
				Factory.Save();

				var summaries = new TrackingInventorySummaryCollection(Factory);
				summaries.Load();
				summaries.OfType<TrackingInventorySummary>().LoadInventories();

				var expectedLinePKs = new ZGuid[] { line1.PK, line2.PK, transferLine1.PK };
				AssertContainsExactElementsInAnyOrder("Collection should include in transit line", expectedLinePKs, summaries[0].Inventories.GetPKs());

				var filter = new ZQuery(WhsInventoryViewSchema.WI_OH_Client, data.Org1.PK);
				filter.AddToFilter(WhsInventoryViewSchema.WI_OP, data.Part1.PK);
				summaries = new TrackingInventorySummaryCollection(Factory);
				summaries.Load(filter);
				summaries.OfType<TrackingInventorySummary>().LoadInventories();
				AssertContainsExactElementsInAnyOrder("Collection should include in transit line", expectedLinePKs, summaries[0].Inventories.GetPKs());
			}
		}

		public void TestLoadMaxRows()
		{
			SetupTestData();
			var summaries = new TrackingInventorySummaryCollection(Factory);
			summaries.Load(new ZQuery { MaximumRows = 7 });
			summaries.OfType<TrackingInventorySummary>().LoadInventories();

			AssertEquals("Collection should contain 7 unique combinations of Client, Warehouse and Product", 7, summaries.Count);
			AssertEquals("Collection should be composed of 11 inventories even though 7 MaximumRows was specified in query", 11, summaries.OfType<TrackingInventorySummary>().Aggregate(0, (totalCount, next) => totalCount + next.Inventories.Count));
		}

		public void TestLoadRemovesOrderBy()
		{
			var query = new ZQuery { OrderBy = WhsTrackingInventorySummaryItemViewSchema.WI_ArrivalDate.Name };
			var summaries = new TrackingInventorySummaryCollection(Factory);
			summaries.Load(query);
			AssertEquals(string.Empty, query.OrderBy);
		}

		public void TestLoadWithIModuleManualSort()
		{
			SetupTestData();
			var ascending = new TrackingInventorySummaryCollection(Factory);
			ascending.Load(new ZQuery { OrderBy = $"{WhsTrackingInventorySummaryItemViewSchema.WI_ArrivalDate.Name}{OrderByClause.Ascending}" });

			var descending = new TrackingInventorySummaryCollection(Factory);
			descending.Load(new ZQuery { OrderBy = $"{WhsTrackingInventorySummaryItemViewSchema.WI_ArrivalDate.Name}{OrderByClause.Descending}" });

			CombineAssertions(() =>
			{
				for (var i = 0; i < ascending.Count; i++)
				{
					AssertEquals(ascending[i].WI_ArrivalDate, descending[descending.Count - 1 - i].WI_ArrivalDate);
				}

				AssertEquals(ascending.Count, descending.Count);
			});
		}

		public void TestFetchHints()
		{
			SetupTestData();
			var newFactory = new BusinessObjectFactory();

			var summaries = new TrackingInventorySummaryCollection(newFactory);
			summaries.Load(new ZQuery { MaximumRows = 7 });

			var expectedDBHits = new Dictionary<string, int>()
			{
				{ OrgHeaderSchema.Constants.TableName, 0 },
				{ OrgPartRelationSchema.Constants.TableName, 0 },
				{ OrgSupplierPartSchema.Constants.TableName, 0 },
				{ WhsWarehouseSchema.Constants.TableName, 0 },
				{ WhsDocketSchema.Constants.TableName, 0 },
				{ WhsDocketLineSchema.Constants.TableName, 0 },
				{ WhsInventoryViewSchema.Constants.TableName, 0 },
				{ WhsLocationViewSchema.Constants.TableName, 0 },
				{ WhsPickLineSchema.Constants.TableName, 0 },
				{ StmNoteSchema.Constants.TableName, 0 },
				{ RefPacksSchema.Constants.TableName, 0 }
			};
			AssertDbHits(expectedDBHits, newFactory);

			summaries.FetchStrategy.FetchForView(summaries.ToArray(), new[] { new TableColumn("ClientQuantity", TrackingInventorySummary.Schema.WI_ClientQuantity), new TableColumn("Product", TrackingInventorySummary.Schema.ProductCode), new TableColumn("Warehouse", TrackingInventorySummary.Schema.WarehouseName) });
			expectedDBHits = new Dictionary<string, int>()
			{
				{ OrgHeaderSchema.Constants.TableName, 0 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 0 },
				{ WhsDocketSchema.Constants.TableName, 2 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 0 },
				{ WhsPickLineSchema.Constants.TableName, 0 },
				{ StmNoteSchema.Constants.TableName, 0 },
				{ RefPacksSchema.Constants.TableName, 0 }
			};
			AssertDbHits(expectedDBHits, newFactory);

			foreach (TrackingInventorySummary summary in summaries)
			{
				_ = summary.HasProductImage;
				_ = summary.ProductCode;
				_ = summary.ProductDescription;
				_ = summary.TotalVolume;
				_ = summary.TotalWeight;
				_ = summary.WarehouseName;
				_ = summary.WI_ArrivalDate;
				_ = summary.WI_AvailableUnits;
				_ = summary.WI_ClientQuantity;
				_ = summary.WI_ClientUQ;
				_ = summary.WI_CommittedUnits;
				_ = summary.WI_CrossDockQuantity;
				_ = summary.WI_Currency;
				_ = summary.WI_TotalUnits;
				_ = summary.WI_TotalValue;
				_ = summary.WI_UnitsUQ;

				foreach (TrackingWhsInventory inventory in summary.Inventories)
				{
					_ = inventory.ReceiptReference;
					_ = inventory.WI_OP_PartNum;
					_ = inventory.WI_ArrivalDateOrETA;
					_ = inventory.StatusDesc;
					_ = inventory.WI_AvailableToPickQuantity;
					_ = inventory.InternalsProxy.CommittedToTransactionQuantity;
					_ = inventory.WI_CrossDockQuantity;
					_ = inventory.WI_TotalUnits;
					_ = inventory.WI_TotalValue;
					_ = inventory.WI_Currency;
					_ = inventory.WI_PartAttrib1;
					_ = inventory.WI_PartAttrib2;
					_ = inventory.WI_PartAttrib3;
					_ = inventory.WI_SerialNumber;
					_ = inventory.WI_ExpiryDate;
					_ = inventory.WI_PackingDate;
					_ = inventory.SupplierPart.OP_LastCost;
				}
			}

			expectedDBHits = new Dictionary<string, int>()
			{
				{ OrgHeaderSchema.Constants.TableName, 0 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 3 },
				{ WhsDocketLineSchema.Constants.TableName, 2 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ StmNoteSchema.Constants.TableName, 0 },
				{ RefPacksSchema.Constants.TableName, 0 }
			};
			AssertDbHits(expectedDBHits, newFactory);
		}

		public void TestDocketLineStorageMainFetchForViewHints()
		{
			SetupTestData();
			var newFactory = new BusinessObjectFactory();
			var summaries = new TrackingInventorySummaryCollection(newFactory);
			summaries.Load(new ZQuery { MaximumRows = 7 });
			summaries.FetchStrategy.FetchForView(summaries.ToArray(), new[] { new TableColumn("ArrivalDate", TrackingInventorySummary.Schema.WI_ArrivalDate), new TableColumn("Currency", TrackingInventorySummary.Schema.WI_Currency) });

			var inventories = summaries
				.OfType<TrackingInventorySummary>()
				.SelectMany(s => s.Inventories)
				.OfType<TrackingWhsInventory>();

			foreach (var inventory in inventories)
			{
				_ = inventory.HasEDocsOrNotesAttached;
			}

			var expectedDBHits = new Dictionary<string, int>()
			{
				{ StorageMainSchema.Constants.TableName, 1 }
			};
			var docketLines = inventories.Select(i => i.InDocketLine).Distinct();
			var docFactory = docketLines.FirstOrDefault()?.DocFactory;
			AssertNotNull(docFactory);
			Assert(docketLines.All(l => l.DocFactory == docFactory));
			AssertDbHits(expectedDBHits, docFactory);
		}

		public void TestHasProductImageFetchForViewHints()
		{
			SetupTestData();
			var inventoryFactory = new BusinessObjectFactory();
			var summaries = new TrackingInventorySummaryCollection(inventoryFactory);
			summaries.Load(new ZQuery { MaximumRows = 7 });
			summaries.FetchStrategy.FetchForView(summaries.ToArray(), new[] { new TableColumn("ProductImage", TrackingInventorySummary.Schema.HasProductImage), new TableColumn("Currency", TrackingInventorySummary.Schema.WI_Currency) });

			foreach (TrackingInventorySummary summary in summaries)
			{
				_ = summary.HasProductImage;
			}

			var parts = summaries
				.OfType<TrackingInventorySummary>()
				.Select(s => s.SupplierPart);

			Assert(parts.Any(p => p.PK == Part1.PK));
			Assert(parts.Any(p => p.PK == Part2.PK));
			Assert(parts.Any(p => p.PK == Part3.PK));
			Assert(parts.Any(p => p.PK == Part4.PK));

			var expectedDocFactory = parts.First().DocManagerInfo().MasterFactory;
			Assert(parts.All(p => p.DocManagerInfo().MasterFactory == expectedDocFactory));

			var expectedDBHits = new Dictionary<string, int>()
			{
				{ StorageMainSchema.Constants.TableName, 1 }
			};
			AssertDbHits(expectedDBHits, (BusinessObjectFactory)expectedDocFactory);
		}

		#region Implementation

		WhsWarehouse Warehouse1 => warehouse1 ?? (warehouse1 = WarehouseHelper.CreateWarehouse("1", "A1", 3, 2));
		WhsWarehouse warehouse1;

		WhsWarehouse Warehouse2 => warehouse2 ?? (warehouse2 = WarehouseHelper.CreateWarehouse("2", "A2", 3, 2));
		WhsWarehouse warehouse2;

		OrgSupplierPart Part1 => part1 ?? (part1 = WarehouseHelper.CreateProduct(Client1, "1"));
		OrgSupplierPart part1;

		OrgSupplierPart Part2 => part2 ?? (part2 = WarehouseHelper.CreateProduct(Client1, "2"));
		OrgSupplierPart part2;

		OrgSupplierPart Part3 => part3 ?? (part3 = WarehouseHelper.CreateProduct(Client2, "3"));
		OrgSupplierPart part3;

		OrgSupplierPart Part4 => part4 ?? (part4 = WarehouseHelper.CreateProduct(Client2, "4"));
		OrgSupplierPart part4;

		OrgHeader Client1 => Helper.TestOrg;

		OrgHeader Client2 => client2 ?? (client2 = WarehouseHelper.CreateClient("CLIENT2", "Client 2"));
		OrgHeader client2;

		protected override BusinessObject GetNewElementToAddToTheCollection() => TrackingInventorySummaryProvider.GetEmptySummary(Factory);

		void SetupTestData()
		{
			var notify = new TestNotificationBuffer();

			var docket1 = WarehouseHelper.CreateWhsReceive(Client1.PK, Warehouse1.PK, "111", notify);
			var docket2 = WarehouseHelper.CreateWhsReceive(Client1.PK, Warehouse1.PK, "112", notify);
			var docket3 = WarehouseHelper.CreateWhsReceive(Client1.PK, Warehouse2.PK, "123", notify);
			var docket4 = WarehouseHelper.CreateWhsReceive(Client1.PK, Warehouse2.PK, "124", notify);
			var docket5 = WarehouseHelper.CreateWhsReceive(Client1.PK, Warehouse1.PK, "115", notify);
			var docket6 = WarehouseHelper.CreateWhsReceive(Client2.PK, Warehouse1.PK, "211", notify);
			var docket7 = WarehouseHelper.CreateWhsReceive(Client2.PK, Warehouse2.PK, "222", notify);
			var docket8 = WarehouseHelper.CreateWhsReceive(Client2.PK, Warehouse1.PK, "213", notify);
			var docket9 = WarehouseHelper.CreateWhsReceive(Client2.PK, Warehouse2.PK, "214", notify);
			var docket10 = WarehouseHelper.CreateWhsReceive(Client1.PK, Warehouse1.PK, "111a", notify);
			var docket11 = WarehouseHelper.CreateWhsReceive(Client2.PK, Warehouse2.PK, "214a", notify);

			var line1 = WarehouseHelper.CreateWhsReceiveInventoryLine(docket1, Part1.PK, 10);
			var line2 = WarehouseHelper.CreateWhsReceiveInventoryLine(docket2, Part2.PK, 10);
			var line3 = WarehouseHelper.CreateWhsReceiveInventoryLine(docket3, Part1.PK, 10);
			var line4 = WarehouseHelper.CreateWhsReceiveInventoryLine(docket4, Part2.PK, 10);
			var line5 = WarehouseHelper.CreateWhsReceiveInventoryLine(docket5, Part1.PK, 10);
			var line6 = WarehouseHelper.CreateWhsReceiveInventoryLine(docket6, Part3.PK, 10);
			var line7 = WarehouseHelper.CreateWhsReceiveInventoryLine(docket7, Part4.PK, 10);
			var line8 = WarehouseHelper.CreateWhsReceiveInventoryLine(docket8, Part3.PK, 10);
			var line9 = WarehouseHelper.CreateWhsReceiveInventoryLine(docket9, Part3.PK, 10);
			var line10 = WarehouseHelper.CreateWhsReceiveInventoryLine(docket10, Part1.PK, 35);
			var line11 = WarehouseHelper.CreateWhsReceiveInventoryLine(docket11, Part3.PK, 35);

			docket1.AllocateLocationsWithMock();
			docket2.AllocateLocationsWithMock();
			docket3.AllocateLocationsWithMock();
			docket4.AllocateLocationsWithMock();
			docket5.AllocateLocationsWithMock();
			docket6.AllocateLocationsWithMock();
			docket7.AllocateLocationsWithMock();
			docket8.AllocateLocationsWithMock();
			docket9.AllocateLocationsWithMock();
			docket10.AllocateLocationsWithMock();
			docket11.AllocateLocationsWithMock();

			docket1.FinaliseDocket();
			docket2.FinaliseDocket();
			docket3.FinaliseDocket();
			docket4.FinaliseDocket();
			docket5.FinaliseDocket();
			docket6.FinaliseDocket();
			docket7.FinaliseDocket();
			// dont finalise docket 8 to test putaway status
			docket9.FinaliseDocket();
			docket10.FinaliseDocket();
			docket11.FinaliseDocket();

			// manually fudge the inventory data for more complex filter tests
			// docket8 has not been finalised so its inventory status = 'Putaway'
			docket5.Inventory[0].Location.WLV_LocationStatus = LocationStatus.Codes.Damaged;
			docket6.Inventory[0].Location.WLV_LocationStatus = LocationStatus.Codes.Held;
			docket7.Inventory[0].WI_InventoryStatus = InventoryStatus.Codes.Held;
			docket7.Inventory[0].InDocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Held;
			docket7.Inventory[0].InDocketLine.WE_WHC_NKCurrentInventoryHeldCode = "HOLD!!";
			// arrival date
			var newArrivalDateForDocket1Inventory1 = docket1.Inventory[0].WI_ArrivalDate.AddDays(-7);
			docket1.Inventory[0].WI_ArrivalDate = newArrivalDateForDocket1Inventory1;
			docket1.Inventory[0].InDocketLine.WE_AdjustmentArrivalDate = newArrivalDateForDocket1Inventory1;

			var newArrivalDateForDocket2Inventory1 = docket2.Inventory[0].WI_ArrivalDate.AddDays(-7);
			docket2.Inventory[0].WI_ArrivalDate = newArrivalDateForDocket2Inventory1;
			docket2.Inventory[0].InDocketLine.WE_AdjustmentArrivalDate = newArrivalDateForDocket2Inventory1;

			// setup attribute stuff
			docket1.Inventory[0].WI_PartAttrib1 = "PA1";
			docket1.Inventory[0].InDocketLine.WE_PartAttrib1 = "PA1";
			var packingDateForDocket1 = ZDate.Today.AddMonths(-2);
			docket1.Inventory[0].WI_PackingDate = packingDateForDocket1;
			docket1.Inventory[0].InDocketLine.WE_PackingDate = packingDateForDocket1;

			docket2.Inventory[0].WI_PartAttrib2 = "PA2";
			docket2.Inventory[0].InDocketLine.WE_PartAttrib2 = "PA2";

			docket3.Inventory[0].WI_PartAttrib3 = "PA3";
			docket3.Inventory[0].InDocketLine.WE_PartAttrib3 = "PA3";

			docket4.Inventory[0].WI_PartAttrib1 = "PA1";
			docket4.Inventory[0].InDocketLine.WE_PartAttrib1 = "PA1";
			docket4.Inventory[0].WI_PartAttrib2 = "PA2";
			docket4.Inventory[0].InDocketLine.WE_PartAttrib2 = "PA2";

			docket5.Inventory[0].WI_PartAttrib1 = "PA1";
			docket5.Inventory[0].InDocketLine.WE_PartAttrib1 = "PA1";
			docket5.Inventory[0].WI_PartAttrib3 = "PA3";
			docket5.Inventory[0].InDocketLine.WE_PartAttrib3 = "PA3";

			docket6.Inventory[0].WI_PartAttrib1 = "PA1";
			docket6.Inventory[0].InDocketLine.WE_PartAttrib1 = "PA1";
			docket6.Inventory[0].WI_PartAttrib2 = "PA2";
			docket6.Inventory[0].InDocketLine.WE_PartAttrib2 = "PA2";
			docket6.Inventory[0].WI_PartAttrib3 = "PA3";
			docket6.Inventory[0].InDocketLine.WE_PartAttrib3 = "PA3";
			docket6.Inventory[0].WI_SerialNumber = "PA3";
			docket6.Inventory[0].InDocketLine.WE_SerialNumber = "SN1";
			docket6.Inventory[0].WI_ExpiryDate = ZDate.Today.AddMonths(1);
			docket6.Inventory[0].InDocketLine.WE_ExpiryDate = ZDate.Today.AddMonths(1);
			docket6.Inventory[0].WI_PackingDate = ZDate.Today.AddMonths(-1);
			docket6.Inventory[0].InDocketLine.WE_PackingDate = ZDate.Today.AddMonths(-1);

			docket7.Inventory[0].WI_ExpiryDate = ZDate.Today.AddMonths(2);
			docket7.Inventory[0].InDocketLine.WE_ExpiryDate = ZDate.Today.AddMonths(2);

			docket9.Inventory[0].WI_PackingDate = ZDate.Today.AddMonths(-2); // this should be ignored because units are zero
			docket9.Inventory[0].InDocketLine.WE_PackingDate = ZDate.Today.AddMonths(-2); // this should be ignored because units are zero

			docket10.Inventory[0].WI_PartAttrib1 = "PA1"; // this needs to be set to test the view aggregation
			docket10.Inventory[0].InDocketLine.WE_PartAttrib1 = "PA1"; // this needs to be set to test the view aggregation

			Factory.Save();

			var order = WarehouseHelper.CreateWhsOrder(Client2, Warehouse2, "OR1", notify);
			WarehouseHelper.CreateWhsOrderLine(order, Part3, 17m);
			var pick = WarehouseHelper.CreatePickByAttachingOrders(order);

			Factory.Save();
		}

		protected override TrackingInventorySummaryCollection GetCollectionToTest() => new TrackingInventorySummaryCollection(Factory);

		WhsTestHelperFunctions WarehouseHelper => warehouseHelper ?? (warehouseHelper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions warehouseHelper;

		ZWebTestHelper Helper
		{
			get
			{
				if (helper == null)
				{
					helper = new ZWebTestHelper(Factory);
					Factory.Save();
				}
				return helper;
			}
		}
		ZWebTestHelper helper;

		#endregion
	}
}
