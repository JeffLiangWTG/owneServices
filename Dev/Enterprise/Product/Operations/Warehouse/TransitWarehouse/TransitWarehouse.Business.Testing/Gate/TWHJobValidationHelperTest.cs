using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public class TWHJobValidationHelperTest : TestCaseWithFactory
	{
		public void TestGetTotalsOfPackageStates()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var wps1 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked, weight: 25, weightUQ: Weight.Kilograms, volume: 3, volumeUQ: Volume.CubicMetres);

			AssertEquals(((decimal)25, (decimal)3, 1), TWHJobValidationHelper.GetTotalsOfPackageStates(new List<WhsItemPackageState>() { wps1 }));
		}

		public void TestGetTotalsOfPackageStates_WhenMultiplePackages()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var wps1 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked, weight: 25, weightUQ: Weight.Kilograms, volume: 3, volumeUQ: Volume.CubicMetres);
			var wps2 = Helper.CreatePackageState(rcn1, 2, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked, weight: 30, weightUQ: Weight.Kilograms, volume: 3, volumeUQ: Volume.CubicMetres);
			var wps3 = Helper.CreatePackageState(rcn1, 3, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked, weight: 60, weightUQ: Weight.Kilograms, volume: 3, volumeUQ: Volume.CubicMetres);
			var wps4 = Helper.CreatePackageState(rcn1, 4, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked, weight: 10, weightUQ: Weight.Kilograms, volume: 3, volumeUQ: Volume.CubicMetres);

			AssertEquals(((decimal)125, (decimal)12, 10), TWHJobValidationHelper.GetTotalsOfPackageStates(new List<WhsItemPackageState>() { wps1, wps2, wps3, wps4 }));
		}

		public void TestGetTotalsOfPackageStates_WhenVolumeAndWeightIsZero()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var wps1 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked, weight: 0, weightUQ: Weight.Kilograms, volume: 0, volumeUQ: Volume.CubicMetres);
			var wps2 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked, weight: 0, weightUQ: Weight.Kilograms, volume: 0, volumeUQ: Volume.CubicMetres);
			var wps3 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked, weight: 0, weightUQ: Weight.Kilograms, volume: 0, volumeUQ: Volume.CubicMetres);
			var wps4 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked, weight: 0, weightUQ: Weight.Kilograms, volume: 0, volumeUQ: Volume.CubicMetres);

			AssertEquals(((decimal)0, (decimal)0, 4), TWHJobValidationHelper.GetTotalsOfPackageStates(new List<WhsItemPackageState>() { wps1, wps2, wps3, wps4 }));
		}

		public void TestGetTotalsOfPackageStates_WhenPackagesHaveDifferentWeightAndVolumeUnits()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var wps1 = Helper.CreatePackageState(rcn1, 1, "PLT", "P2", TransitWarehouseStatuses.Codes.Booked, weight: 352.74m, weightUQ: Weight.Ounces, volume: 317.832m, volumeUQ: Volume.CubicFeet);
			var wps2 = Helper.CreatePackageState(rcn1, 1, "PLT", "P3", TransitWarehouseStatuses.Codes.Booked, weight: 11.0231m, weightUQ: Weight.Pounds, volume: 1000, volumeUQ: Volume.CubicCentimeters);
			var wps3 = Helper.CreatePackageState(rcn1, 1, "PKG", "P4", TransitWarehouseStatuses.Codes.Booked, weight: 0.02m, weightUQ: Weight.Tonnes, volume: 18.3113m, volumeUQ: Volume.CubicYards);
			var wps4 = Helper.CreatePackageState(rcn1, 1, "PLT", "P5", TransitWarehouseStatuses.Codes.Booked, weight: 1, weightUQ: Weight.Kilograms, volume: 17m, volumeUQ: Volume.CubicMetres);
			var wps5 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked, weight: 3000, weightUQ: Weight.Grams, volume: 427166, volumeUQ: Volume.CubicInches);

			var (totalWeightInKg, totalVolumeInM3, totalPackages) = TWHJobValidationHelper.GetTotalsOfPackageStates(new List<WhsItemPackageState>() { wps1, wps2, wps3, wps4, wps5 });
			AssertEquals(39, totalWeightInKg, (decimal)0.001);
			AssertEquals((decimal)47.001, totalVolumeInM3, (decimal)0.001);
		}

		public void TestGetValidPackageStates()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var wps1 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.AdjustedOut);
			var wps2 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Arrived);
			var wps3 = Helper.CreatePackageState(rcn1, 1, "PKG", "P1", TransitWarehouseStatuses.Codes.ArrivedNotProcessed);
			var wps4 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.ArrivedPacked);
			var wps5 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked);
			var wps6 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Committed);
			var wps7 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Departed);
			var wps8 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Finalized);
			var wps9 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.FreightLoaded);
			var wpsa = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.GatedIn);
			var wpsb = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Packed);
			var wpsc = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Packing);
			var wpsd = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Picked);
			var wpse = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Putaway);
			var wpsf = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.ReadyToPack);
			var wpsg = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Staged);
			var wpsh = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Unpacked);
			var wpsi = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Unpacking);

			var packageStates = new List<WhsItemPackageState>() { wps1, wps2, wps3, wps4, wps5, wps6, wps7, wps8, wps9, wpsa, wpsb, wpsc, wpsd, wpse, wpsf, wpsg, wpsh, wpsi };
			var validPackageStates = TWHJobValidationHelper.GetValidPackageStates(packageStates).ToList();

			AssertCollectionNotContains("Expected wps1 not to be in validPackageStates because has 'AdjustedOut' status", wps1, validPackageStates);
			AssertCollectionNotContains("Expected wps7 not to be in validPackageStates because has 'Departed' status", wps7, validPackageStates);
			AssertCollectionNotContains("Expected wps8 not to be in validPackageStates because has 'Finalized' status", wps8, validPackageStates);
		}

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;
	}
}
