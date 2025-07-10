using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsLocationCapacityTest : WhsTestCaseWithFactory
	{
		#region TestLocationCapacity

		public void TestLocationCapacity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.DefaultLocation;
			location.WLV_MaxWeight = 20;
			location.WLV_MaxWeightUnit = "KG";
			location.WLV_MaxCubic = 30;
			location.WLV_MaxCubicUnit = "M3";
			location.WLV_MaxQuantity = 40;

			Helper.SetProductWeightAndVolume(data.Part1, 1, "KG", 1, "M3");
			Factory.Save();
			AssertReturnedValues(LoadData(location.PK, ZGuid.Empty), 20m, 30m, 40m, 0);

			// Normal SKU

			var finalisedReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 1, null, "Pallet0001");
			Factory.Save();
			AssertReturnedValues(LoadData(location.PK, ZGuid.Empty), 19m, 29m, 39m, 1);
			// check that if we exclude docket - available capacity will be reverted to original
			AssertReturnedValues(LoadData(location.PK, finalisedReceive.PK), 20m, 30m, 40m, 0);

			// this is to illustrate that pending transactions reduce available capacity
			var notFinalisedReceive =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Now, data.Part1, 2, null, "Pallet0002", true, false);
			Factory.Save();
			AssertReturnedValues(LoadData(location.PK, finalisedReceive.PK), 18m, 28m, 38m, 1);
			AssertReturnedValues(LoadData(location.PK, notFinalisedReceive.PK), 19m, 29m, 39m, 1);
			AssertReturnedValues(LoadData(location.PK, ZGuid.Empty), 17m, 27m, 37m, 2);

			// Weight SKU
			var productWithWeightSKU = Helper.CreateProduct(data.Org1, "Gold");
			productWithWeightSKU.OP_StockKeepingUnit = Constants.Weight.Decitons;
			productWithWeightSKU.OP_Cubic = 0m;
			productWithWeightSKU.OP_Weight = 333m; // this value should be ignored by calculation because SKU is weight

			var finalisedWeightReceive =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", ZDateTimeOffset.Now, productWithWeightSKU,
					0.04m, null, "Pallet0003"); // 0.04 DT = 4 KG
			Factory.Save();
			AssertReturnedValues(LoadData(location.PK, ZGuid.Empty), 13m, 27m, 36.96m, 3);
			AssertReturnedValues(LoadData(location.PK, finalisedWeightReceive.PK), 17m, 27m, 37m, 2);

			var notFinalisedWeightReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", ZDateTimeOffset.Now,
				productWithWeightSKU, 0.04m, null, "Pallet0004", true, false);
			Factory.Save();
			AssertReturnedValues(LoadData(location.PK, ZGuid.Empty), 9m, 27m, 36.92m, 4);
			AssertReturnedValues(LoadData(location.PK, notFinalisedWeightReceive.PK), 13m, 27m, 36.96m, 3);

			// Volume SKU

			var productWithVolumeSKU = Helper.CreateProduct(data.Org1, "Bubbles");
			productWithVolumeSKU.OP_StockKeepingUnit = Constants.Volume.MegaLitre;
			productWithVolumeSKU.OP_Cubic = 555m; // this value should be ignored by calculation because SKU is weight
			productWithVolumeSKU.OP_Weight = 0m;

			var finalisedVolumeReceive =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", ZDateTimeOffset.Now, productWithVolumeSKU,
					0.003m, null, "Pallet0005"); // 0.003 ML (Mega Liters) = 3 M3
			Factory.Save();
			AssertReturnedValues(LoadData(location.PK, ZGuid.Empty), 9m, 24m, 36.917m, 5);
			AssertReturnedValues(LoadData(location.PK, finalisedVolumeReceive.PK), 9m, 27m, 36.92m, 4);

			var notFinalisedVolumeReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R6", ZDateTimeOffset.Now,
				productWithVolumeSKU, 0.003m, null, "Pallet0006", true, false);
			Factory.Save();
			AssertReturnedValues(LoadData(location.PK, ZGuid.Empty), 9m, 21m, 36.914m, 6);
			AssertReturnedValues(LoadData(location.PK, notFinalisedVolumeReceive.PK), 9m, 24m, 36.917m, 5);
		}

		public void TestLocationCapacity_NoCapacities()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.DefaultLocation;
			Factory.Save();

			var connection = ((IDbConnected)Factory).Connection;
			var capacityData = LoadData(location.PK, ZGuid.Empty);
			AssertEquals("AvailableWeight", DBNull.Value,
				connection.ExecuteScalar(
					$"SELECT AvailableWeight FROM dbo.WhsLocationCapacity('{location.PK}', null)"));
			AssertEquals("AvailableVolume", DBNull.Value,
				connection.ExecuteScalar(
					$"SELECT AvailableVolume FROM dbo.WhsLocationCapacity('{location.PK}', null)"));
			AssertEquals("AvailableUnits", DBNull.Value,
				connection.ExecuteScalar($"SELECT AvailableUnits FROM dbo.WhsLocationCapacity('{location.PK}', null)"));
			AssertEquals("PalletCount", 0,
				connection.ExecuteScalar($"SELECT PalletCount FROM dbo.WhsLocationCapacity('{location.PK}', null)"));
		}

		public void TestLocationCapacity_MultiLinesWithTheSamePalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.DefaultLocation;
			location.WLV_MaxWeight = 20;
			location.WLV_MaxWeightUnit = "KG";
			location.WLV_MaxCubic = 30;
			location.WLV_MaxCubicUnit = "M3";
			location.WLV_MaxQuantity = 40;

			Helper.SetProductWeightAndVolume(data.Part1, 1, "KG", 1, "M3");
			Factory.Save();
			AssertReturnedValues(LoadData(location.PK, ZGuid.Empty), 20m, 30m, 40m, 0);

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 1, null, "Pallet0001");
			Factory.Save();
			AssertReturnedValues(LoadData(location.PK, ZGuid.Empty), 19m, 29m, 39m, 1);
			AssertReturnedValues(LoadData(location.PK, receive1.PK), 20m, 30m, 40m, 0);

			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Now, data.Part1, 2, null, "Pallet0001");
			Factory.Save();
			AssertReturnedValues(LoadData(location.PK, receive1.PK), 18m, 28m, 38m, 1);
			AssertReturnedValues(LoadData(location.PK, receive2.PK), 19m, 29m, 39m, 1);
			AssertReturnedValues(LoadData(location.PK, ZGuid.Empty), 17m, 27m, 37m, 1);
		}

		public void TestLocationCapacity_MultiLinesWithTheSamePalletID_DifferentStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.DefaultLocation;
			location.WLV_MaxWeight = 20;
			location.WLV_MaxWeightUnit = "KG";
			location.WLV_MaxCubic = 30;
			location.WLV_MaxCubicUnit = "M3";
			location.WLV_MaxQuantity = 40;

			Helper.SetProductWeightAndVolume(data.Part1, 1, "KG", 1, "M3");
			Factory.Save();
			AssertReturnedValues(LoadData(location.PK, ZGuid.Empty), 20m, 30m, 40m, 0);

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 1, null, "Pallet0001");
			Factory.Save();
			AssertReturnedValues(LoadData(location.PK, ZGuid.Empty), 19m, 29m, 39m, 1);
			AssertReturnedValues(LoadData(location.PK, receive1.PK), 20m, 30m, 40m, 0);

			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Now, data.Part1, 2, null, "Pallet0001", true, false);
			Factory.Save();
			AssertReturnedValues(LoadData(location.PK, receive1.PK), 18m, 28m, 38m, 1);
			AssertReturnedValues(LoadData(location.PK, receive2.PK), 19m, 29m, 39m, 1);
			AssertReturnedValues(LoadData(location.PK, ZGuid.Empty), 17m, 27m, 37m, 1);
		}

		public void TestLocationCapacity_NoStockOnHand()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.DefaultLocation;
			location.WLV_MaxWeight = 20;
			location.WLV_MaxWeightUnit = "KG";
			location.WLV_MaxCubic = 30;
			location.WLV_MaxCubicUnit = "M3";
			location.WLV_MaxQuantity = 40;
			Factory.Save();

			AssertReturnedValues(LoadData(location.PK, ZGuid.Empty), 20m, 30m, 40m, 0);
		}

		public void TestLocationCapacity_StagedInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			dockDoorLocation.WLV_MaxQuantity = 20m;
			Helper.SetProductWeightAndVolume(data.Part1, 1, "KG", 1, "M3");
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var originalInventory = receive.Lines[0];
			Factory.Save();
			AssertReturnedValues(LoadData(dockDoorLocation.PK, ZGuid.Empty), 0m, 0m, 20m, 0); // Precondition

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 8m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			Factory.Save();
			AssertReturnedValues(LoadData(dockDoorLocation.PK, ZGuid.Empty), 0m, 0m, 12m, 0); // In-Transit

			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit,
				transferLine.WE_CurrentInventoryStatus);
			transferLine.FinaliseDocketLine();
			AssertEquals("Precondition - should be Staged.", InventoryStatus.Codes.Staged,
				transferLine.WE_CurrentInventoryStatus);
			Factory.Save();

			AssertReturnedValues(LoadData(dockDoorLocation.PK, ZGuid.Empty), 0m, 0m, 12m, 0); // Staged
		}

		public void TestLocationCapacity_ExcludePFUDocketLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var normalLocation = data.Whs1.DefaultLocation;
			dockDoorLocation.WLV_MaxWeight = 20;
			dockDoorLocation.WLV_MaxWeightUnit = "KG";
			dockDoorLocation.WLV_MaxCubic = 30;
			dockDoorLocation.WLV_MaxCubicUnit = "M3";
			dockDoorLocation.WLV_MaxQuantity = 40;

			Helper.SetProductWeightAndVolume(data.Part1, 1, "KG", 1, "M3");
			Factory.Save();

			AssertReturnedValues(LoadData(dockDoorLocation.PK, ZGuid.Empty), 20m, 30m, 40m, 0);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 7m, dockDoorLocation, "PLT-1");
			var receiveLine = (WhsReceiveLine)inventory.InDocketLine;
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.CreateWhsTransferLine(putawayTransfer, data.Part1, 7m,
				dockDoorLocation.ToLocationString(), "PLT-1", normalLocation.ToLocationString(), "PLT-1");
			transferLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;
			transferLine.RunPreSaveValidation(); // to commit inventory
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();
			AssertEquals("Precondition", 0m, receiveLine.WE_StockOnHand);
			AssertEquals("Precondition", DocketLineStatus.Codes.PickedForUnload, receiveLine.WE_DocketLineStatus);
			AssertEquals("Precondition", 7m, transferLine.WE_StockOnHand);
			AssertEquals("Precondition", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);

			//PFU docket lines won't affect reuslt of LoadData
			AssertReturnedValues(LoadData(dockDoorLocation.PK, ZGuid.Empty), 20m, 30m, 40m, 0);
			AssertReturnedValues(LoadData(dockDoorLocation.PK, receive.PK), 20m, 30m, 40m, 0);
			AssertReturnedValues(LoadData(dockDoorLocation.PK, putawayTransfer.PK), 20m, 30m, 40m, 0);
		}

		public void TestLocationCapacity_ExcludesCancelledReceiveLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.DefaultOutboundDockDoorLocation;
			location.WLV_MaxWeight = 20;
			location.WLV_MaxWeightUnit = "KG";
			location.WLV_MaxCubic = 30;
			location.WLV_MaxCubicUnit = "M3";
			location.WLV_MaxQuantity = 40;

			Helper.SetProductWeightAndVolume(data.Part1, 1, "KG", 1, "M3");
			Factory.Save();

			// Precondition: Initial location capacity
			AssertReturnedValues(LoadData(location.PK, ZGuid.Empty), 20m, 30m, 40m, 0);

			var cancelledReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var cancelledReceiveLine = Helper.CreateWhsReceiveLine(cancelledReceive, data.Part1, 10m, location);
			cancelledReceive.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			cancelledReceiveLine.WE_DocketLineStatus = DocketLineStatus.Codes.Cancelled;
			cancelledReceiveLine.WE_StockOnHand = 0m;
			Factory.Save();

			AssertEquals("Precondition", DocketStatus.Codes.Cancelled, cancelledReceive.WD_DocketStatus);
			AssertEquals("Precondition", DocketLineStatus.Codes.Cancelled, cancelledReceiveLine.WE_DocketLineStatus);

			// Location capacity does not change
			AssertReturnedValues(LoadData(location.PK, ZGuid.Empty), 20m, 30m, 40m, 0);
		}

		#endregion

		#region TestAllWeightsAreConsidered

		public void TestAllWeightsAreConsidered()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 1, "KG", 1, "M3");
			var location = data.Whs1.DefaultLocation;
			location.WLV_MaxWeight = 10;
			data.Part1.OP_Weight = 99; // this value should be ignored

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 2, true, false);

			foreach (var weightUQ in Constants.Weight.Codes)
			{
				data.Part1.OP_StockKeepingUnit = weightUQ;
				location.WLV_MaxWeightUnit = weightUQ;
				Factory.Save();
				// the goal is to check that OP_Weight of the product is ignored when StockKeepingUnit is Weight
				AssertReturnedValues(LoadData(location.PK, ZGuid.Empty), 7, 0, 0, 0);
			}
		}

		#endregion

		#region TestAllVolumesAreConsidered

		public void TestAllVolumesAreConsidered()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 1, "KG", 1, "M3");
			var location = data.Whs1.DefaultLocation;
			location.WLV_MaxCubic = 10;
			data.Part1.OP_Cubic = 99;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 2, true, false);

			foreach (var volumeUQ in Constants.Volume.Codes)
			{
				data.Part1.OP_StockKeepingUnit = volumeUQ;
				location.WLV_MaxCubicUnit = volumeUQ;
				Factory.Save();
				// the goal is to check that OP_Cubic of the product is ignored when StockKeepingUnit is Volume
				AssertReturnedValues(LoadData(location.PK, ZGuid.Empty), 0, 7, 0, 0);
			}
		}

		#endregion

		#region TestCapacitiesExceeded

		public void TestCapacitiesExceeded()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 15, location);
			receive1.FinaliseDocket();
			Factory.Save();

			using (WhsTestHelperFunctions.SuspendTrigger("TG_PreventOverReduceLocationUnitsCapacity",
					   WhsLocationSchema.Constants.TableName)) // Existing bad data is possible
			{
				location.WLV_MaxQuantity = 10m;
				location.WLV_MaxWeight = 10m;
				location.WLV_MaxWeightUnit = Constants.Weight.Kilograms;
				location.WLV_MaxCubic = 10m;
				location.WLV_MaxCubicUnit = Constants.Volume.CubicDecimetres;

				data.Part1.OP_Weight = 1m;
				data.Part1.OP_WeightUQ = Constants.Weight.Kilograms;
				data.Part1.OP_Cubic = 1m;
				data.Part1.OP_CubicUQ = Constants.Volume.CubicDecimetres;
				Factory.Save();
			}

			AssertReturnedValues(LoadData(location.PK, ZGuid.Empty), 0m, 0m, 0m, 0);
		}

		#endregion

		#region Implementation

		DynamicBusinessObject LoadData(ZGuid locationPK, ZGuid docketToExcludePK)
		{
			var rawSql =
				@"SELECT AvailableWeight, AvailableVolume, AvailableUnits, PalletCount FROM dbo.WhsLocationCapacity(@LocationPK, @DocketPK)";
			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(rawSql,
				new[]
				{
					ZSqlParameter.New("@LocationPK", locationPK, WhsLocationViewSchema.PK),
					ZSqlParameter.New("@DocketPK", docketToExcludePK, WhsDocketSchema.PK)
				});
			return collection[0];
		}

		void AssertReturnedValues(DynamicBusinessObject loadedResult, ZDecimal expectedAvailableWeight,
			ZDecimal expectedAvailableVolume, ZDecimal expectedAvailableUnits, int palletCount)
		{
			CombineAssertions(() =>
			{
				AssertEquals("AvailableWeight is wrong", expectedAvailableWeight, loadedResult["AvailableWeight"]);
				AssertEquals("AvailableVolume is wrong", expectedAvailableVolume, loadedResult["AvailableVolume"]);
				AssertEquals("AvailableUnits is wrong", expectedAvailableUnits, loadedResult["AvailableUnits"]);
				AssertEquals("PalletCount is wrong", palletCount, loadedResult["PalletCount"]);
			});
		}

		#endregion
	}
}
