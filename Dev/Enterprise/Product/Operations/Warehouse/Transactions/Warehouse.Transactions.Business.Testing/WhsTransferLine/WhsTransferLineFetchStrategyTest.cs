using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsTransferLineFetchStrategyTest : WhsDocketLineFetchStrategyTest<WhsTransfer, WhsTransferLine>
	{
		#region TestFetchForLoad_InterWhsDest_WithNoDestLocation

		[StressTest]
		public void TestFetchForLoad_InterWhsDest_WithNoDestLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var warehouses = new WhsWarehouse[10];
			for (int i = 0; i < 10; i++)
			{
				warehouses[i] = Helper.CreateWarehouse(string.Format("W{0}", i), "A");
				Helper.CreateWhsReceiveWithInventory(data.Org1, warehouses[i], string.Format("R{0}", i), data.Part1, 100m, warehouses[i].FindLocation("A"), "");
			}

			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", transferType: TransferType.Codes.InterWhsDest);

			for (int i = 0; i < 100; i++)
			{
				var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A", warehouses[i % 10].PK, "");
				transfer.RunPreSaveValidation(); // to generate pick lines
				transferLine.PickedTime = ZDateTimeOffset.Now;

				AssertNotNull("Precondition: Created child line.", transferLine.ChildTransferLine);
			}

			Factory.Save();

			// Child Transfer
			var otherFactory1 = new BusinessObjectFactory { RefreshEnabled = false };
			var childTransferLinesInOtherFactory = otherFactory1.Load<WhsTransferLine>(new ZQuery(WhsDocketLineSchema.WE_WD, transfer.ChildTransfers.First().PK));

			var expectedChildDbHits = new Dictionary<string, int>
			{
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 2 },
				{ WhsDocketLineSchema.Constants.TableName, 3 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 }
			};
			AssertDbHits(expectedChildDbHits, otherFactory1);

			// Parent
			var otherFactory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var transferLinesInOtherFactory = otherFactory2.Load<WhsTransferLine>(new ZQuery(WhsDocketLineSchema.WE_WD, transfer.PK));

			var expectedParentDbHits = new Dictionary<string, int>
			{
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 }
			};
			AssertDbHits(expectedParentDbHits, otherFactory2);

			AssertHasNoUnconsumedFetchHintsOnDocketLineRowOrLocation(otherFactory1);
			AssertHasNoUnconsumedFetchHintsOnDocketLineRowOrLocation(otherFactory2);
		}

		#endregion

		#region TestFetchForLoad_InterWhsSource

		[StressTest]
		public void TestFetchForLoad_InterWhsSource()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var warehouses = new WhsWarehouse[10];
			for (int i = 0; i < 10; i++)
			{
				warehouses[i] = Helper.CreateWarehouse(string.Format("W{0}", i), "A");
			}

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, data.Whs1.FindLocation("A"), "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", transferType: TransferType.Codes.InterWhsSource);

			for (int i = 0; i < 100; i++)
			{
				var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A", warehouses[i % 10].PK, "");
				AssertNoErrors("Precondition: Dest Location cannot be empty. If this is valid we may need extra fetch hints.", transferLine.WE_WLInfo);
				transferLine.LocationString = "A";

				transfer.RunPreSaveValidation(); // to generate pick lines
				transferLine.PickedTime = ZDateTimeOffset.Now;

				AssertNotNull("Precondition: Created child line.", transferLine.ChildTransferLine);
			}

			Factory.Save();

			// Child Transfer
			var otherFactory1 = new BusinessObjectFactory { RefreshEnabled = false };
			var childTransferLinesInOtherFactory = otherFactory1.Load<WhsTransferLine>(new ZQuery(WhsDocketLineSchema.WE_WD, transfer.ChildTransfers.First().PK));

			var expectedChildDbHits = new Dictionary<string, int>
			{
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 }
			};
			AssertDbHits(expectedChildDbHits, otherFactory1);

			// Parent
			var otherFactory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var transferLinesInOtherFactory = otherFactory2.Load<WhsTransferLine>(new ZQuery(WhsDocketLineSchema.WE_WD, transfer.PK));

			var expectedParentDbHits = new Dictionary<string, int>
			{
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 3 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 }
			};
			AssertDbHits(expectedParentDbHits, otherFactory2);

			AssertHasNoUnconsumedFetchHintsOnDocketLineRowOrLocation(otherFactory1);
			AssertHasNoUnconsumedFetchHintsOnDocketLineRowOrLocation(otherFactory2);
		}

		#endregion

		#region Implementation

		void AssertHasNoUnconsumedFetchHintsOnDocketLineRowOrLocation(BusinessObjectFactory factory)
		{
			CombineAssertions(
				() =>
				{
					AssertEquals("Should have no unconsumed fetch hints on WhsDocketLine.", 0, factory.ActiveFetchHintsForTable(WhsDocketLineSchema.Constants.TableName));
					AssertEquals("Should have no unconsumed fetch hints on WhsLocationView.", 0, factory.ActiveFetchHintsForTable(WhsLocationViewSchema.Constants.TableName));
					AssertEquals("Should have no unconsumed fetch hints on WhsRow.", 0, factory.ActiveFetchHintsForTable(WhsRowSchema.Constants.TableName));
				});
		}

		protected override void AdditionalExpectedDbHitsForFetchForLoad(Dictionary<string, int> expectedDbHits)
		{
			base.AdditionalExpectedDbHitsForFetchForLoad(expectedDbHits);
			expectedDbHits.Add(WhsDocketSchema.Constants.TableName, 1);
		}

		protected override void AdditionalExpectedDbHitsForFetchForView(Dictionary<string, int> expectedDbHits)
		{
			base.AdditionalExpectedDbHitsForFetchForView(expectedDbHits);
			expectedDbHits.Add(WhsWarehouseSchema.Constants.TableName, 1);
			expectedDbHits.Add(WhsAreaSchema.Constants.TableName, 1);
		}

		protected override WhsTransfer CreateDocket(OrgHeader client, WhsWarehouse warehouse, string reference)
		{
			return Helper.CreateWhsTransfer(client, warehouse, reference);
		}

		protected override void CreateDocketLine(WhsTransfer transfer, OrgSupplierPart part, WhsLocation location, decimal quantity)
		{
			Helper.CreateWhsTransferLine(transfer, part, quantity, location.ToLocationString(), "");
		}

		#endregion
	}
}
