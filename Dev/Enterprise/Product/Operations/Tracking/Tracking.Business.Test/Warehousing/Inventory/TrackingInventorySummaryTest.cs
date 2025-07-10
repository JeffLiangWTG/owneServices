using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingInventorySummary))]
	public sealed class TrackingInventorySummaryTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAdditionalFilter()
		{
			var filter = new ZQuery();
			filter.AddToFilter(WhsTrackingInventorySummaryItemViewSchema.PK, ZGuid.NewZGuid());
			var summary = GetEmptySummary(filter);

			AssertEquals(filter, summary.AdditionalFilter);
		}

		public void TestInventories()
		{
			var inventories = Summary.Inventories;
			AssertNotNull(inventories);
			AssertEquals(0, inventories.Count);
			AssertEquals(inventories, Summary.Inventories);
			AssertEquals(ZBool.True, Summary.IsRegisteredEditableChildObject(inventories));
		}

		public void TestLoadInventoriesWithInventoryPKs()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var warehouse1 = helper.CreateWarehouse("W1", "A", 3, 1);
			var warehouse2 = helper.CreateWarehouse("W2", "B", 3, 1);
			var client1 = helper.CreateClient("C1", "Client1");
			var client2 = helper.CreateClient("C2", "Client2");
			var part1 = helper.CreateProduct(client1, "P1");
			var part2 = helper.CreateProduct(client2, "P2");
			Factory.Save();

			var receive1 = helper.CreateWhsReceiveWithInventory(client1, warehouse1, "R1", part1, 50m, warehouse1.FindLocation("A-1"), "", allocateLocations: false, finalise: true);
			var receive2 = helper.CreateWhsReceiveWithInventory(client1, warehouse1, "R2", part1, 100m, warehouse1.FindLocation("A-2"), "", allocateLocations: false, finalise: true);
			helper.CreateWhsReceiveWithInventory(client2, warehouse1, "R3", part2, 150m, warehouse1.FindLocation("A-1"), "", allocateLocations: false, finalise: true);
			helper.CreateWhsReceiveWithInventory(client1, warehouse2, "R4", part1, 200m, warehouse2.FindLocation("B-1"), "", allocateLocations: false, finalise: true);
			helper.CreateWhsReceiveWithInventory(client1, warehouse1, "R5", part1, 250m, warehouse1.FindLocation("A-1"), "Pallet", allocateLocations: false, finalise: true);

			Factory.Save();

			var filter = new ZQuery();
			filter.AddToFilter(WhsTrackingInventorySummaryItemViewSchema.WI_PalletID, string.Empty);
			var summaries = TrackingInventorySummaryProvider.GetSummaries(Factory, filter);
			var summary = summaries.FirstOrDefault(s => s.WI_OH_Client == client1.PK && s.WI_WW_Whs == warehouse1.PK && s.WI_OP == part1.PK);
			AssertNotNull(summary);

			summary.LoadInventories();
			AssertEquals(0, summary.Inventories.Count);

			summary.AddInventoryPK(receive1.Lines.First().PK);
			summary.AddInventoryPK(receive2.Lines.First().PK);
			summary.LoadInventories();
			AssertEquals(2, summary.Inventories.Count);

			var inventories = summary.Inventories.Cast<TrackingWhsInventory>();
			Assert(inventories.Any(i => i.WI_OH_Client == client1.PK && i.WI_WW_Whs == warehouse1.PK && i.WI_OP == part1.PK && i.WI_AvailableToPickQuantity == 50m));
			Assert(inventories.Any(i => i.WI_OH_Client == client1.PK && i.WI_WW_Whs == warehouse1.PK && i.WI_OP == part1.PK && i.WI_AvailableToPickQuantity == 100m));
			Assert(inventories.All(i => i.SupplierPart == summary.SupplierPart));
		}

		[TestDate(2020, 5, 6)]
		public void TestSortLoadedInventories()
		{
			var today = ZDateTimeOffset.Now;
			var helper = new WhsTestHelperFunctions(Factory);
			var warehouse = helper.CreateWarehouse("WHS", "A", 2, 1);
			var client = helper.CreateClient("ORG", "Client");
			var part1 = helper.CreateProduct(client, "P1");
			var part2 = helper.CreateProduct(client, "P2");
			helper.SetClientAllAttributeType(client, false);
			helper.SetProductAllAttributeUse(client, part1, true);
			var location = warehouse.FindLocation("A-1");
			Factory.Save();

			var receive1 = helper.CreateWhsReceive(client.PK, warehouse.PK, "R1", today);
			var receiveLine11 = helper.CreateWhsReceiveLine(receive1, part1, 10m, location, "PLT1");
			var receiveLine12 = helper.CreateWhsReceiveLine(receive1, part1, 10m, location, "PLT2");

			var receive2 = helper.CreateWhsReceive(client.PK, warehouse.PK, "R2", today);
			var receiveLine21 = helper.CreateWhsReceiveLine(receive2, part1, 10m, location, "PLT1");

			var receive3 = helper.CreateWhsReceive(client.PK, warehouse.PK, "R3", today.AddDays(-1));
			var receiveLine31 = helper.CreateWhsReceiveLine(receive3, part1, 10m, location, "PLT5");

			var receive4 = helper.CreateWhsReceive(client.PK, warehouse.PK, "R4", today);
			var receiveLine41 = helper.CreateWhsReceiveLine(receive4, part2, 10m, location, "PLT1", ZDate.Today, ZDate.Today, "A1", "A1", "A1", "");
			var receiveLine42 = helper.CreateWhsReceiveLine(receive4, part2, 10m, location, "PLT1", ZDate.Today, ZDate.Today, "A1", "A2", "A1", "");
			var receiveLine43 = helper.CreateWhsReceiveLine(receive4, part2, 10m, location, "PLT1", ZDate.Today, ZDate.Today, "A1", "A2", "A3", "");
			Factory.Save();

			var filter = new ZQuery();
			filter.AddToFilter(WhsTrackingInventorySummaryItemViewSchema.WI_PalletID, SQLComparisonOperator.StartsWith, "PLT");
			var summaries = TrackingInventorySummaryProvider.GetSummaries(Factory, filter);
			AssertEquals(2, summaries.Count());

			summaries.LoadInventories();
			var part1Summary = summaries.Single(i => i.SupplierPart.PK == part1.PK);
			AssertEquals(4, part1Summary.Inventories.Count);
			AssertEquals("First order by Arrival Date", receiveLine31.PK, part1Summary.Inventories[0].PK);
			AssertEquals("Fallback to order by Reference", receiveLine11.PK, part1Summary.Inventories[1].PK);
			AssertEquals("Fallback to order by Pallet ID", receiveLine12.PK, part1Summary.Inventories[2].PK);
			AssertEquals("Fallback to order by Reference", receiveLine21.PK, part1Summary.Inventories[3].PK);

			var part2Summary = summaries.Single(i => i.SupplierPart.PK == part2.PK);
			AssertEquals(3, part2Summary.Inventories.Count);
			AssertEquals("Order by Part Attribute1", receiveLine41.PK, part2Summary.Inventories[0].PK);
			AssertEquals("Order by Part Attribute2", receiveLine42.PK, part2Summary.Inventories[1].PK);
			AssertEquals("Order by Part Attribute3", receiveLine43.PK, part2Summary.Inventories[2].PK);
		}

		[TestDate(2020, 5, 6)]
		public void TestSortLoadedInventories_SortBySerialNumber()
		{
			var today = ZDateTimeOffset.Now;
			var helper = new WhsTestHelperFunctions(Factory);
			var warehouse = helper.CreateWarehouse("WHS", "A", 2, 1);
			var client = helper.CreateClient("ORG", "Client");
			var part = helper.CreateProduct(client, "P1");
			helper.SetClientAllAttributeType(client, false);
			helper.SetProductAllAttributeUse(client, part, true);
			var location = warehouse.DefaultLocation;
			Factory.Save();

			var receive = helper.CreateWhsReceive(client.PK, warehouse.PK, "R1", today);
			var receiveLine1 = helper.CreateWhsReceiveLine(receive, part.PK, 10m, location.PK, "PLT1", ZDate.Today, ZDate.Today, "A1", "A2", "A3", "SN1", "");
			var receiveLine2 = helper.CreateWhsReceiveLine(receive, part.PK, 10m, location.PK, "PLT1", ZDate.Today, ZDate.Today, "A1", "A2", "A3", "SN3", "");
			var receiveLine3 = helper.CreateWhsReceiveLine(receive, part.PK, 10m, location.PK, "PLT1", ZDate.Today, ZDate.Today, "A1", "A2", "A3", "SN2", "");
			Factory.Save();

			var filter = new ZQuery();
			filter.AddToFilter(WhsTrackingInventorySummaryItemViewSchema.WI_PalletID, SQLComparisonOperator.StartsWith, "PLT");
			var summaries = TrackingInventorySummaryProvider.GetSummaries(Factory, filter);
			AssertEquals(1, summaries.Count());

			summaries.LoadInventories();

			var partSummary = summaries.Single(i => i.SupplierPart.PK == part.PK);
			AssertEquals(3, partSummary.Inventories.Count);
			AssertEquals("Order by Part SerialNumber", receiveLine1.PK, partSummary.Inventories[0].PK);
			AssertEquals("Order by Part SerialNumber", receiveLine3.PK, partSummary.Inventories[1].PK);
			AssertEquals("Order by Part SerialNumber", receiveLine2.PK, partSummary.Inventories[2].PK);
		}

		public void TestWI_WW_Whs()
		{
			var warehousePK = Guid.NewGuid();
			var summary = GetSummary(Tuple.Create<SchemaColumn, object>(WhsTrackingInventorySummaryItemViewSchema.WI_WW_Whs, warehousePK));
			AssertEquals(warehousePK, summary.WI_WW_Whs);
		}
		public void TestWI_OH_Client()
		{
			var clientPK = Guid.NewGuid();
			var summary = GetSummary(Tuple.Create<SchemaColumn, object>(WhsTrackingInventorySummaryItemViewSchema.WI_OH_Client, clientPK));
			AssertEquals(clientPK, summary.WI_OH_Client);
		}

		public void TestWI_OP()
		{
			var partPK = Guid.NewGuid();
			var summary = GetSummary(Tuple.Create<SchemaColumn, object>(WhsTrackingInventorySummaryItemViewSchema.WI_OP, partPK));
			AssertEquals(partPK, summary.WI_OP);
		}

		public void TestWI_UnitsUQ()
		{
			var summary = GetSummary(Tuple.Create<SchemaColumn, object>(WhsTrackingInventorySummaryItemViewSchema.WI_UnitsUQ, "PLT"));
			AssertEquals("PLT", summary.WI_UnitsUQ);
		}

		public void TestWI_ClientUQ()
		{
			var summary = GetSummary(Tuple.Create<SchemaColumn, object>(WhsTrackingInventorySummaryItemViewSchema.WI_ClientUQ, "PLT"));
			AssertEquals("PLT", summary.WI_ClientUQ);
		}

		public void TestWI_ArrivalDate()
		{
			var arrivalDate = new DateTimeOffset(new DateTime(2020, 5, 5));
			var summary = GetSummary(Tuple.Create<SchemaColumn, object>(WhsTrackingInventorySummaryItemViewSchema.WI_ArrivalDate, arrivalDate));
			AssertEquals(arrivalDate, summary.WI_ArrivalDate);
		}

		public void TestWI_TotalValue()
		{
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			part.OP_LastCost = 5.25m;

			var summary = GetSummary(
				Tuple.Create<SchemaColumn, object>(WhsTrackingInventorySummaryItemViewSchema.WI_TotalUnits, 10m),
				Tuple.Create<SchemaColumn, object>(WhsTrackingInventorySummaryItemViewSchema.WI_OP, part.PK.ToGuid()));
			AssertEquals(52.5m, summary.WI_TotalValue);
		}

		public void TestWI_Currency()
		{
			var summary = GetSummary(Tuple.Create<SchemaColumn, object>(WhsTrackingInventorySummaryItemViewSchema.WI_Currency, "USD"));
			AssertEquals("USD", summary.WI_Currency);
		}

		public void TestTotalWeight()
		{
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			part.OP_Weight = 10.5m;

			var summary = GetSummary(
				Tuple.Create<SchemaColumn, object>(WhsTrackingInventorySummaryItemViewSchema.WI_TotalUnits, 10m),
				Tuple.Create<SchemaColumn, object>(WhsTrackingInventorySummaryItemViewSchema.WI_OP, part.PK.ToGuid()));
			AssertEquals(105m, summary.TotalWeight);
		}

		public void TestTotalVolume()
		{
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			part.OP_Cubic = 5.5m;

			var summary = GetSummary(
				Tuple.Create<SchemaColumn, object>(WhsTrackingInventorySummaryItemViewSchema.WI_TotalUnits, 10m),
				Tuple.Create<SchemaColumn, object>(WhsTrackingInventorySummaryItemViewSchema.WI_OP, part.PK.ToGuid()));
			AssertEquals(55m, summary.TotalVolume);
		}

		public void TestWI_TotalUnits()
		{
			var summary = GetSummary(Tuple.Create<SchemaColumn, object>(WhsTrackingInventorySummaryItemViewSchema.WI_TotalUnits, 100m));
			AssertEquals(100m, summary.WI_TotalUnits);
		}

		public void TestWI_CommittedUnits()
		{
			var summary = GetSummary(Tuple.Create<SchemaColumn, object>(WhsTrackingInventorySummaryItemViewSchema.WI_CommittedUnits, 85m));
			AssertEquals(85m, summary.WI_CommittedUnits);
		}

		public void TestWI_AvailableUnits()
		{
			var summary = GetSummary(Tuple.Create<SchemaColumn, object>(WhsTrackingInventorySummaryItemViewSchema.WI_AvailableUnits, 50m));
			AssertEquals(50m, summary.WI_AvailableUnits);
		}

		public void TestWI_CrossDockQuantity()
		{
			var summary = GetSummary(Tuple.Create<SchemaColumn, object>(WhsTrackingInventorySummaryItemViewSchema.WI_CrossDockQuantity, 150m));
			AssertEquals(150m, summary.WI_CrossDockQuantity);
		}

		public void TestWI_ClientQuantity()
		{
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			part.OP_StockKeepingUnit = "BOX";
			var conversion = part.PartUnits.AddNew();
			conversion.OF_PackType = "PLT";
			conversion.OF_QuantityInParent = 11;
			conversion.OF_ParentPackType = "BOX";

			var summary = GetSummary(
				Tuple.Create<SchemaColumn, object>(WhsTrackingInventorySummaryItemViewSchema.WI_TotalUnits, 10m),
				Tuple.Create<SchemaColumn, object>(WhsTrackingInventorySummaryItemViewSchema.WI_ClientUQ, "PLT"),
				Tuple.Create<SchemaColumn, object>(WhsTrackingInventorySummaryItemViewSchema.WI_OP, part.PK.ToGuid()));
			AssertEquals(110m, summary.WI_ClientQuantity);
		}

		public void TestWarehouse()
		{
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseName = "WHS1";
			var summary = GetSummary(Tuple.Create<SchemaColumn, object>(WhsTrackingInventorySummaryItemViewSchema.WI_WW_Whs, warehouse.PK.ToGuid()));
			AssertEquals(warehouse, summary.Warehouse);
			AssertEquals("WarehouseName in English", "WHS1", summary.WarehouseName);

			var resKey = warehouse.WW_WarehouseNameInfo.CustomizableDataResourceStrings.GetMultilingualString(warehouse, "WHS1").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "仓库1"));
				AssertEquals("WarehouseName in Chinese", "仓库1", summary.WarehouseName);
			}
		}

		public void TestSupplierPart()
		{
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			part.OP_PartNum = "FNDR";
			part.OP_Desc = "Fender";
			var summary = GetSummary(Tuple.Create<SchemaColumn, object>(WhsTrackingInventorySummaryItemViewSchema.WI_OP, part.PK.ToGuid()));
			AssertEquals(part, summary.SupplierPart);
			AssertEquals("FNDR", summary.ProductCode);
			AssertEquals("Fender", summary.ProductDescription);
		}

		public void TestProductImage()
		{
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var summary = GetSummary(Tuple.Create<SchemaColumn, object>(WhsTrackingInventorySummaryItemViewSchema.WI_OP, part.PK.ToGuid()));
			using (var resourceRetriever = new EmbeddedResourceRetriever(typeof(StorageDocsTest).Assembly))
			{
				var image = resourceRetriever.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.JPG", "small.jpg");
				var docManager = ((IDocManagerSupport)part).DocManagerInfo;

				docManager.AddFileOrDocument(image, "NNN");
				docManager.AllEDocs[0].IsPublished = true;
				AssertEquals(false, summary.HasProductImage);

				summary = GetSummary(Tuple.Create<SchemaColumn, object>(WhsTrackingInventorySummaryItemViewSchema.WI_OP, part.PK.ToGuid()));
				docManager.AddFileOrDocument(image, Core.Constants.RefDocTypes.ImageFile);
				docManager.AllEDocs[1].IsPublished = true;
				AssertEquals(true, summary.HasProductImage);
			}
		}

		public void TestProductImage_IfNoEdocs()
		{
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var summary = GetSummary(Tuple.Create<SchemaColumn, object>(WhsTrackingInventorySummaryItemViewSchema.WI_OP, part.PK.ToGuid()));
			var masterFactory = ((IDocManagerSupport)part).DocManagerInfo.MasterFactory;

			AssertNull("Precondition", masterFactory.GetStorageMainForPK(part.PK));
			AssertEquals(false, summary.HasProductImage);

			AssertNull("Should not have created StorageMain record if it did not exist", masterFactory.GetStorageMainForPK(part.PK));
		}

		public void TestProductImage_Cache()
		{
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var summary = GetSummary(Tuple.Create<SchemaColumn, object>(WhsTrackingInventorySummaryItemViewSchema.WI_OP, part.PK.ToGuid()));
			var masterFactory = ((IDocManagerSupport)part).DocManagerInfo.MasterFactory;

			var expectedDBHits = new Dictionary<string, int>()
			{
				{ StorageMainSchema.Constants.TableName, 1 }
			};

			using (AssertDbHitsForAllFactories(expectedDBHits))
			{
				AssertEquals(false, summary.HasProductImage);
				AssertEquals(false, summary.HasProductImage);
			}
		}

		public void TestTableName()
		{
			AssertEquals(WhsTrackingInventorySummaryItemViewSchema.Constants.TableName, Summary.TableName);
		}

		public void TestTablePrefix()
		{
			AssertEquals(WhsTrackingInventorySummaryItemViewSchema.Constants.Prefix, Summary.TablePrefix);
		}

		#region Implementation

		TrackingInventorySummary GetSummary(params Tuple<SchemaColumn, object>[] dataValues) => GetSummary(Factory, dataValues);

		public static TrackingInventorySummary GetSummary(BusinessObjectFactory factory, params Tuple<SchemaColumn, object>[] dataValues)
		{
			var table = new DataTable("TrackingInventorySummaryItemView");
			foreach (var column in WhsTrackingInventorySummaryItemViewSchema.All)
			{
				table.Columns.Add(column.Name, column.DotNetType);
			}
			var row = table.NewRow();
			table.Rows.Add(row);

			if (dataValues != null)
			{
				foreach (var dataValue in dataValues)
				{
					row[dataValue.Item1.Name] = dataValue.Item2;
				}
			}
			var data = new DynamicBusinessObject(factory, row);

			return new TrackingInventorySummary(data, ZQuery.NoResultQuery);
		}

		#endregion

		#region Implementation

		TrackingInventorySummary Summary => (TrackingInventorySummary)CachedBusinessObject;

		protected override BusinessObject GetNewBusinessObject() => GetEmptySummary();

		TrackingInventorySummary GetEmptySummary(ZQuery filter = null) => TrackingInventorySummaryProvider.GetEmptySummary(Factory, filter);

		#endregion
	}
}
