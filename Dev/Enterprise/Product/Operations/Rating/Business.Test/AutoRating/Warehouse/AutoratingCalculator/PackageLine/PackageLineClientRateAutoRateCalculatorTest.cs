using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	internal class PackageLineClientRateAutoRateCalculatorTest : PackageLineAutoRateCalculatorTest
	{
		#region Unit Calculator

		public void TestUnitCalculator_RateEntryWithWarehousePK()
		{
			var warehouse1 = Helper.NewWarehouse();

			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.WHS, Constants.RateMode.ALL, "AU", "US", "WHSOUT", 1m, QuantityUnit.KG);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.PackageLine;
			rateEntry.TI_WW_Warehouse = warehouse1.PK;

			Factory.Save();

			Criteria = GetWarehouseCriteria(clientRate.Header);
			var measures = Criteria.RateableMeasures;
			measures.CreateWarehousePackages((rateableMeasureSet) =>
			{
				measures.AddWarehouseOuterPackage(volume: 40m, weight: 50m, packageCount: 60m, packageType: PkgUnit.Basket, docketReference: "ABC", warehousePK: warehouse1.PK.ToGuid());
			});

			AssertAutorate
			(
				CostSell.Revenue,
				expected: new[]
				{
					new SimpleArInfo { Amount = 50m, InvoiceLineDesc = "Warehouse Out Charge ABC", CalculationSingleLineDescription = "WHSOUT: 50 Kilogram(s) @ AUD 1.00/KG (for warehouse package/s: 60x BSK)" }
				},
				assertionMessage: "GIVEN rateLine with PackageLine UnitFactor and UNT calculator WHEN autorate THEN should be calculated per package"
			);
		}

		public void TestUnitCalculator_ActualPercentage80()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.WHS, Constants.RateMode.ALL, "AU", "US", "WHSOUT", 1m, QuantityUnit.KG);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.PackageLine;
			rateEntry.RateLines[0].TL_ActualPercentage = 80;

			Factory.Save();

			Criteria = GetWarehouseCriteria(clientRate.Header);
			var measures = Criteria.RateableMeasures;
			measures.CreateWarehousePackages((rateableMeasureSet) =>
			{
				measures.AddWarehouseOuterPackage(volume: 40m, weight: 50m, packageCount: 60m, docketReference: "ABC", packageType: PkgUnit.Basket);
			});

			AssertAutorate
			(
				CostSell.Revenue,
				expected: new[]
				{
					// 80% X 50 KG = 40KG
					// 20% (40M3 / .006M3/KG) = 1334KG
					new SimpleArInfo { Amount = 1374, InvoiceLineDesc = "Warehouse Out Charge ABC", CalculationSingleLineDescription = "WHSOUT: 1374 Kilogram(s) @ AUD 1.00/KG (for warehouse package/s: 60x BSK)" }
				},
				assertionMessage: "GIVEN rateLine with PackageLine UnitFactor and UNT calculator WHEN autorate THEN should be calculated per package"
			);
		}

		public void TestUnitCalculator_SingleOuterPackage()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.WHS, Constants.RateMode.ALL, "AU", "US", "WHSOUT", 1m, QuantityUnit.KG);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.PackageLine;

			Factory.Save();

			Criteria = GetWarehouseCriteria(clientRate.Header);
			var measures = Criteria.RateableMeasures;
			measures.CreateWarehousePackages((rateableMeasureSet) =>
			{
				measures.AddWarehouseOuterPackage(volume: 40m, weight: 50m, packageCount: 60m, packageType: PkgUnit.Basket, docketReference: "ABC");
			});

			AssertAutorate
			(
				CostSell.Revenue,
				expected: new[]
				{
					new SimpleArInfo { Amount = 50m, InvoiceLineDesc = "Warehouse Out Charge ABC", CalculationSingleLineDescription = "WHSOUT: 50 Kilogram(s) @ AUD 1.00/KG (for warehouse package/s: 60x BSK)" }
				},
				assertionMessage: "GIVEN rateLine with PackageLine UnitFactor and UNT calculator WHEN autorate THEN should be calculated per package"
			);
		}

		public void TestUnitCalculator_Weight()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.WHS, Constants.RateMode.ALL, "AU", "US", "WHSOUT", 1m, QuantityUnit.KG);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.PackageLine;

			Factory.Save();

			Criteria = GetWarehouseCriteria(clientRate.Header);
			var measures = Criteria.RateableMeasures;
			measures.CreateWarehousePackages((rateableMeasureSet) =>
			{
				measures.AddWarehouseOuterPackage(volume: 40m, weight: 50m, packageCount: 60m, packageType: PkgUnit.Basket, docketReference: "ABC");
				measures.AddWarehouseOuterPackage(volume: 40m, weight: 10m, packageCount: 20m, packageType: PkgUnit.Bundle, docketReference: "ABC");
			});

			AssertAutorate
			(
				CostSell.Revenue,
				expected: new[]
				{
					new SimpleArInfo { Amount = 60m, InvoiceLineDesc = "Warehouse Out Charge ABC", CalculationSingleLineDescription = "WHSOUT: 10 Kilogram(s) @ AUD 1.00/KG (for warehouse package/s: 20x BND)\r\nWHSOUT: 50 Kilogram(s) @ AUD 1.00/KG (for warehouse package/s: 60x BSK)" }
				},
				assertionMessage: "GIVEN rateLine with PackageLine UnitFactor and UNT calculator WHEN autorate THEN should be calculated per package"
			);
		}

		public void TestUnitCalculator_WeightConversion()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.WHS, Constants.RateMode.ALL, "AU", "US", "WHSOUT", 1m, Weight.Pounds);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.PackageLine;

			Factory.Save();

			Criteria = GetWarehouseCriteria(clientRate.Header);
			var measures = Criteria.RateableMeasures;
			measures.CreateWarehousePackages((rateableMeasureSet) =>
			{
				measures.AddWarehouseOuterPackage(volume: 40m, weight: 50m, packageCount: 60m, packageType: PkgUnit.Basket, docketReference: "ABC");
				measures.AddWarehouseOuterPackage(volume: 40m, weight: 10m, packageCount: 20m, packageType: PkgUnit.Bundle, docketReference: "ABC");
			});

			AssertAutorate
			(
				CostSell.Revenue,
				expected: new[]
				{
					new SimpleArInfo { Amount = 134m, InvoiceLineDesc = "Warehouse Out Charge ABC", CalculationSingleLineDescription = "WHSOUT: 111 Pound(s) @ AUD 1.00/LB (for warehouse package/s: 60x BSK)\r\nWHSOUT: 23 Pound(s) @ AUD 1.00/LB (for warehouse package/s: 20x BND)" }
				},
				assertionMessage: "GIVEN rateLine with PackageLine UnitFactor and UNT calculator WHEN autorate THEN should be calculated per package"
			);
		}

		public void TestUnitCalculator_Volume()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.WHS, Constants.RateMode.ALL, "AU", "US", "WHSOUT", 1m, QuantityUnit.M3);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.PackageLine;

			Factory.Save();

			Criteria = GetWarehouseCriteria(clientRate.Header);
			var measures = Criteria.RateableMeasures;
			measures.CreateWarehousePackages((rateableMeasureSet) =>
			{
				measures.AddWarehouseOuterPackage(volume: 40m, weight: 50m, packageCount: 60m, packageType: PkgUnit.Basket, docketReference: "ABC");
				measures.AddWarehouseOuterPackage(volume: 30m, weight: 40m, packageCount: 50m, packageType: PkgUnit.Bundle, docketReference: "ABC");
			});

			AssertAutorate
			(
				CostSell.Revenue,
				expected: new[]
				{
					new SimpleArInfo { Amount = 70m, InvoiceLineDesc = "Warehouse Out Charge ABC", CalculationSingleLineDescription = "WHSOUT: 30 Cubic Meter(s) @ AUD 1.00/M3 (for warehouse package/s: 50x BND)\r\nWHSOUT: 40 Cubic Meter(s) @ AUD 1.00/M3 (for warehouse package/s: 60x BSK)" }
				},
				assertionMessage: "GIVEN rateLine with PackageLine UnitFactor and UNT calculator WHEN autorate THEN should be calculated per package"
			);
		}

		public void TestUnitCalculator_VolumeConversion()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.WHS, Constants.RateMode.ALL, "AU", "US", "WHSOUT", 1m, Volume.CubicFeet);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.PackageLine;

			Factory.Save();

			Criteria = GetWarehouseCriteria(clientRate.Header);
			var measures = Criteria.RateableMeasures;
			measures.CreateWarehousePackages((rateableMeasureSet) =>
			{
				measures.AddWarehouseOuterPackage(volume: 40m, weight: 50m, packageCount: 60m, packageType: PkgUnit.Basket, docketReference: "ABC");
				measures.AddWarehouseOuterPackage(volume: 30m, weight: 40m, packageCount: 50m, packageType: PkgUnit.Bundle, docketReference: "ABC");
			});

			AssertAutorate
			(
				CostSell.Revenue,
				expected: new[]
				{
					new SimpleArInfo { Amount = 2473m, InvoiceLineDesc = "Warehouse Out Charge ABC", CalculationSingleLineDescription = "WHSOUT: 1060 Cubic Feet @ AUD 1.00/CF (for warehouse package/s: 50x BND)\r\nWHSOUT: 1413 Cubic Feet @ AUD 1.00/CF (for warehouse package/s: 60x BSK)" }
				},
				assertionMessage: "GIVEN rateLine with PackageLine UnitFactor and UNT calculator WHEN autorate THEN should be calculated per package"
			);
		}

		public void TestUnitCalculator_PackageType()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.WHS, Constants.RateMode.ALL, "AU", "US", "WHSOUT", 1m, PkgUnit.Bag);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.PackageLine;

			Factory.Save();

			Criteria = GetWarehouseCriteria(clientRate.Header);
			var measures = Criteria.RateableMeasures;
			measures.CreateWarehousePackages((rateableMeasureSet) =>
			{
				measures.AddWarehouseOuterPackage(volume: 40m, weight: 50m, packageCount: 60m, packageType: PkgUnit.Basket, docketReference: "ABC");
				measures.AddWarehouseOuterPackage(volume: 30m, weight: 40m, packageCount: 50m, packageType: PkgUnit.Bag, docketReference: "ABC");
				measures.AddWarehouseOuterPackage(volume: 40m, weight: 50m, packageCount: 60m, packageType: PkgUnit.Bag, docketReference: "ABC");
			});

			AssertAutorate
			(
				CostSell.Revenue,
				expected: new[]
				{
					new SimpleArInfo { Amount = 110m, InvoiceLineDesc = "Warehouse Out Charge ABC", CalculationSingleLineDescription = "WHSOUT: 0 Bag(s) @ AUD 1.00/Bag (for warehouse package/s: 60x BSK)\r\nWHSOUT: 50 Bag(s) @ AUD 1.00/Bag (for warehouse package/s: 50x BAG)\r\nWHSOUT: 60 Bag(s) @ AUD 1.00/Bag (for warehouse package/s: 60x BAG)" }
				},
				assertionMessage: "GIVEN rateLine with PackageLine UnitFactor and UNT calculator WHEN autorate THEN should be calculated per package"
			);
		}

		public void TestUnitCalculator_NoMatchingUnit()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.WHS, Constants.RateMode.ALL, "AU", "US", "WHSOUT", 1m, PkgUnit.Coil);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.PackageLine;

			Factory.Save();

			Criteria = GetWarehouseCriteria(clientRate.Header);
			var measures = Criteria.RateableMeasures;
			measures.CreateWarehousePackages((rateableMeasureSet) =>
			{
				measures.AddWarehouseOuterPackage(volume: 40m, weight: 50m, packageCount: 60m, packageType: PkgUnit.Basket, docketReference: "ABC");
				measures.AddWarehouseOuterPackage(volume: 30m, weight: 40m, packageCount: 50m, packageType: PkgUnit.Bag, docketReference: "ABC");
				measures.AddWarehouseOuterPackage(volume: 40m, weight: 50m, packageCount: 60m, packageType: PkgUnit.Bag, docketReference: "ABC");
			});

			AssertAutorate
			(
				CostSell.Revenue,
				expected: new[]
				{
					new SimpleArInfo { Amount = 0m, InvoiceLineDesc = "Warehouse Out Charge ABC", CalculationSingleLineDescription = "WHSOUT: 0 Coil(s) @ AUD 1.00/Coil (for warehouse package/s: 50x BAG)\r\nWHSOUT: 0 Coil(s) @ AUD 1.00/Coil (for warehouse package/s: 60x BAG)\r\nWHSOUT: 0 Coil(s) @ AUD 1.00/Coil (for warehouse package/s: 60x BSK)" }
				},
				assertionMessage: "GIVEN rateLine with PackageLine UnitFactor and UNT calculator WHEN autorate THEN should be calculated per package"
			);
		}

		#endregion

		#region Implementation

		protected override RatingHeader RatingHeader => ratingHeader ?? (ratingHeader = Helper.NewClientRate(NewClient));
		RatingHeader ratingHeader;

		protected override CostSell CostSell => CostSell.Revenue;

		protected override void Save() => Factory.Save();

		protected override OrgHeader LocalClient => NewClient;

		#endregion
	}
}
