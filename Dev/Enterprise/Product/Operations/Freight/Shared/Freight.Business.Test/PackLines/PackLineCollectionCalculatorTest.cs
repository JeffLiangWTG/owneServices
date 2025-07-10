using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.Freight.Business.Testing
{
	sealed class PackLineCollectionCalculatorTest : BaseFreightTest
	{
		public void TestCtor()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer container = consol.Containers.AddNew();
			CommonShipment shipment1 = consol.Shipments.AddNew();
			CommonShipment shipment2 = consol.Shipments.AddNew();
			CommonShipment shipment3 = consol.Shipments.AddNew();
			CommonShipment shipment4 = consol.Shipments.AddNew();
			shipment2.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			shipment3.JS_JS_ColoadMasterShipment = shipment2.PK;
			shipment4.JS_JS_ColoadMasterShipment = shipment2.PK;

			PackLine packline1 = shipment1.OuterPackLines.AddNew();
			PackLine packline2 = shipment2.OuterPackLines.AddNew();
			PackLine packline3 = shipment3.OuterPackLines.AddNew();
			PackLine packline4 = shipment4.OuterPackLines.AddNew();

			PackLineCollectionCalculator packLineHelper = new PackLineCollectionCalculator(container.PackLines, PackLineCollectionCalculator.FilterMode.NoFilter);
			AssertContainsExactElementsInAnyOrder("Should contain all packlines", new PackLine[] { packline1, packline2, packline3, packline4 }, packLineHelper.PackLineCollection);

			packLineHelper = new PackLineCollectionCalculator(container.PackLines, PackLineCollectionCalculator.FilterMode.RemoveColoadMasterPackLines);
			AssertContainsExactElementsInAnyOrder("Should not contain ColoadMaster PackLines", new PackLine[] { packline1, packline3, packline4 }, packLineHelper.PackLineCollection);
		}

		public void TestTotalPackages()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer container = consol.Containers.AddNew();
			CommonShipment shipment1 = consol.Shipments.AddNew();
			CommonShipment shipment2 = consol.Shipments.AddNew();
			CommonShipment shipment3 = consol.Shipments.AddNew();
			CommonShipment shipment4 = consol.Shipments.AddNew();
			shipment2.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			shipment3.JS_JS_ColoadMasterShipment = shipment2.PK;
			shipment4.JS_JS_ColoadMasterShipment = shipment2.PK;

			PackLine packline1 = shipment1.OuterPackLines.AddNew();
			PackLine packline2 = shipment2.OuterPackLines.AddNew();
			PackLine packline3 = shipment3.OuterPackLines.AddNew();
			PackLine packline4 = shipment4.OuterPackLines.AddNew();

			packline1.JL_PackageCount = 1;
			packline2.JL_PackageCount = 10;
			packline3.JL_PackageCount = 100;
			packline4.JL_PackageCount = 1000;

			PackLineCollectionCalculator packLineHelper = new PackLineCollectionCalculator(container.PackLines, PackLineCollectionCalculator.FilterMode.RemoveColoadMasterPackLines);
			AssertEquals("Total Packages should be 11", 1101, packLineHelper.TotalPackages);
		}

		public void TestTotalPackagesByShipment()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer container = consol.Containers.AddNew();
			CommonShipment shipment1 = consol.Shipments.AddNew();
			CommonShipment shipment2 = consol.Shipments.AddNew();
			CommonShipment shipment3 = consol.Shipments.AddNew();
			CommonShipment shipment4 = consol.Shipments.AddNew();

			PackLine packline1 = shipment1.OuterPackLines.AddNew();
			PackLine packline2 = shipment3.OuterPackLines.AddNew();
			PackLine packline3 = shipment3.OuterPackLines.AddNew();
			PackLine packline4 = shipment3.OuterPackLines.AddNew();

			packline1.JL_PackageCount = 2;
			packline2.JL_PackageCount = 3;
			packline3.JL_PackageCount = 6;
			packline4.JL_PackageCount = 9;

			PackLineCollectionCalculator packLineHelper = new PackLineCollectionCalculator(container.PackLines, PackLineCollectionCalculator.FilterMode.RemoveColoadMasterPackLines);
			AssertEquals("Total packages by shipment", 18m, packLineHelper.TotalPackagesByShipment(shipment3));
			AssertEquals("Total packages by shipment", 2m, packLineHelper.TotalPackagesByShipment(shipment1));
			AssertEquals(ZDecimal.Zero, packLineHelper.TotalPackagesByShipment(null));
		}

		public void TestTotalWeight()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer container = consol.Containers.AddNew();
			CommonShipment shipment1 = consol.Shipments.AddNew();
			CommonShipment shipment2 = consol.Shipments.AddNew();
			CommonShipment shipment3 = consol.Shipments.AddNew();
			CommonShipment shipment4 = consol.Shipments.AddNew();
			shipment2.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			shipment3.JS_JS_ColoadMasterShipment = shipment2.PK;
			shipment4.JS_JS_ColoadMasterShipment = shipment2.PK;

			PackLine packline1 = shipment1.OuterPackLines.AddNew();
			PackLine packline2 = shipment2.OuterPackLines.AddNew();
			PackLine packline3 = shipment3.OuterPackLines.AddNew();
			PackLine packline4 = shipment4.OuterPackLines.AddNew();

			packline1.JL_ActualWeight = 1;
			packline2.JL_ActualWeight = 10;
			packline3.JL_ActualWeight = 100;
			packline4.JL_ActualWeight = 1000;

			PackLineCollectionCalculator packLineHelper = new PackLineCollectionCalculator(container.PackLines, PackLineCollectionCalculator.FilterMode.RemoveColoadMasterPackLines);
			AssertEquals("Total weight should be 11", 1101m, packLineHelper.TotalWeight);
		}

		public void TestTotalWeightByShipment()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer container = consol.Containers.AddNew();
			CommonShipment shipment1 = consol.Shipments.AddNew();
			CommonShipment shipment2 = consol.Shipments.AddNew();
			CommonShipment shipment3 = consol.Shipments.AddNew();
			CommonShipment shipment4 = consol.Shipments.AddNew();

			PackLine packline1 = shipment1.OuterPackLines.AddNew();
			PackLine packline2 = shipment3.OuterPackLines.AddNew();
			PackLine packline3 = shipment3.OuterPackLines.AddNew();
			PackLine packline4 = shipment3.OuterPackLines.AddNew();

			packline1.JL_ActualWeight = 1;
			packline2.JL_ActualWeight = 10;
			packline3.JL_ActualWeight = 100;
			packline4.JL_ActualWeight = 2000;
			packline4.JL_ActualWeightUQ = Constants.Weight.Grams;

			PackLineCollectionCalculator packLineHelper = new PackLineCollectionCalculator(container.PackLines, PackLineCollectionCalculator.FilterMode.RemoveColoadMasterPackLines);
			AssertEquals("Total weight by shipment should be 112", 112m, packLineHelper.TotalWeightByShipment(shipment3));
			AssertEquals(ZDecimal.Zero, packLineHelper.TotalWeightByShipment(null));
		}

		public void TestTotalVolume()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer container = consol.Containers.AddNew();
			CommonShipment shipment1 = consol.Shipments.AddNew();
			CommonShipment shipment2 = consol.Shipments.AddNew();
			CommonShipment shipment3 = consol.Shipments.AddNew();
			CommonShipment shipment4 = consol.Shipments.AddNew();
			shipment2.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			shipment3.JS_JS_ColoadMasterShipment = shipment2.PK;
			shipment4.JS_JS_ColoadMasterShipment = shipment2.PK;

			PackLine packline1 = shipment1.OuterPackLines.AddNew();
			PackLine packline2 = shipment2.OuterPackLines.AddNew();
			PackLine packline3 = shipment3.OuterPackLines.AddNew();
			PackLine packline4 = shipment4.OuterPackLines.AddNew();

			packline1.JL_ActualVolume = 1;
			packline2.JL_ActualVolume = 10;
			packline3.JL_ActualVolume = 100;
			packline4.JL_ActualVolume = 1000;

			PackLineCollectionCalculator packLineHelper = new PackLineCollectionCalculator(container.PackLines, PackLineCollectionCalculator.FilterMode.RemoveColoadMasterPackLines);
			AssertEquals("Total Volume should be 11", 1101m, packLineHelper.TotalVolume);
		}

		public void TestTotalVolumeByShipment()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer container = consol.Containers.AddNew();
			CommonShipment shipment1 = consol.Shipments.AddNew();
			CommonShipment shipment2 = consol.Shipments.AddNew();
			CommonShipment shipment3 = consol.Shipments.AddNew();
			CommonShipment shipment4 = consol.Shipments.AddNew();

			PackLine packline1 = shipment1.OuterPackLines.AddNew();
			PackLine packline2 = shipment3.OuterPackLines.AddNew();
			PackLine packline3 = shipment3.OuterPackLines.AddNew();
			PackLine packline4 = shipment3.OuterPackLines.AddNew();

			packline1.JL_ActualVolume = 1;
			packline2.JL_ActualVolume = 10;
			packline3.JL_ActualVolume = 100;
			packline4.JL_ActualVolume = 2000;
			packline4.JL_ActualVolumeUQ = Constants.Volume.CubicDecimetres;

			PackLineCollectionCalculator packLineHelper = new PackLineCollectionCalculator(container.PackLines, PackLineCollectionCalculator.FilterMode.RemoveColoadMasterPackLines);
			AssertEquals("Total volume by shipment should be 112", 112m, packLineHelper.TotalVolumeByShipment(shipment3));
			AssertEquals(ZDecimal.Zero, packLineHelper.TotalVolumeByShipment(null));
		}

		public void TestTotalLoadingMeters()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.OuterPackLines.AddNew().JL_LoadingMeters = 1.024;
			shipment.OuterPackLines.AddNew().JL_LoadingMeters = 2.048;
			shipment.OuterPackLines.AddNew().JL_LoadingMeters = 4.096;

			PackLineCollectionCalculator packLineHelper = new PackLineCollectionCalculator(shipment.OuterPackLines, PackLineCollectionCalculator.FilterMode.NoFilter);
			AssertEquals(7.168m, packLineHelper.TotalLoadingMeters);
		}
	}
}
