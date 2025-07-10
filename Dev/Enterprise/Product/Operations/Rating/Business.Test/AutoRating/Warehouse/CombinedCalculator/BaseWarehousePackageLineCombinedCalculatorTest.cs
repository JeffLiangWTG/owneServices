using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	internal abstract class BaseWarehousePackageLineCombinedCalculatorTest<T> : RatingTestCase
		where T : BaseCombinedCalculator
	{
		#region Non Cumulative

		public void TestNonCumulative()
		{
			RateLine.TL_WeightVolume = QuantityUnit.KG;
			Calculator.IsAccumulated = false;
			Calculator["-35"] = (ZDecimal)5m;
			Calculator["+35"] = (ZDecimal)10m;
			Calculator["+50"] = (ZDecimal)15m;

			var criteria = GetWarehouseCriteria(RateLine.Header.Header);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehousePackages((rateableMeasureSet) =>
			{
				measures.AddWarehouseOuterPackage(volume: 10m, weight: 30m, packageCount: 100m, packageType: PkgUnit.Basket, docketReference: "ABC");
				measures.AddWarehouseOuterPackage(volume: 20m, weight: 40m, packageCount: 200m, packageType: PkgUnit.Box, docketReference: "ABC");
			});
			Factory.Save();

			AssertAutorate
			(
				criteria,
				new[]
				{
					new SimpleArInfo
					{
						Amount = 550m,
						InvoiceLineDesc = "Warehouse Charge ABC",
						CalculationSingleLineDescription = @"WHSCHG: 30 Kilogram(s) @ AUD 5.00/KG (for warehouse package/s: 100x BSK)
WHSCHG: 40 Kilogram(s) @ AUD 10.00/KG (for warehouse package/s: 200x BOX)"
					}
				}
			);
		}

		public void TestNonCumulative_ActualPercentage80()
		{
			RateLine.TL_WeightVolume = RatingConstants.Units.M3;
			RateLine.ConversionFactor = new ConversionFactor(250m, Weight.Kilograms, Volume.CubicMetres);
			RateLine.TL_ActualPercentage = 80;
			RateLine.TL_Rounding = RatingRoundingTypes.NoRounding;
			Calculator.IsAccumulated = false;
			Calculator["-2"] = (ZDecimal)5m;
			Calculator["+2"] = (ZDecimal)10m;
			Calculator["+5"] = (ZDecimal)15m;

			var criteria = GetWarehouseCriteria(RateLine.Header.Header);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehousePackages((rateableMeasureSet) =>
			{
				measures.AddWarehouseOuterPackage(volume: 1m, weight: 1000m, packageCount: 60m, packageType: PkgUnit.Basket, docketReference: "ABC");
				measures.AddWarehouseOuterPackage(volume: 7m, weight: 750m, packageCount: 70m, packageType: PkgUnit.Box, docketReference: "ABC");
			});
			Factory.Save();

			AssertAutorate
			(
				criteria,
				new[]
				{
					// Package1: 1M3 & 1000KG => 1000KG / 250KG/M3 = 4M3 => 80% x 1M3 + 20% x 4M3 = 1.6
					// Package2: 7M3 & 750KG => 750KG / 250KG/M3 = 3M3 => 80% x 7M3 + 20% x 3M3 = 6.2
					new SimpleArInfo
					{
						Amount = 113m,
						InvoiceLineDesc = "Warehouse Charge ABC",
						CalculationSingleLineDescription = @"WHSCHG: 1.6 Cubic Meter(s) @ AUD 5.00/M3 (for warehouse package/s: 60x BSK)
WHSCHG: 7 Cubic Meter(s) @ AUD 15.00/M3 (for warehouse package/s: 70x BOX)" }
				}
			);
		}

		public void TestNonCumulative_BreakUnit_PerWeight()
		{
			RateLine.TL_WeightVolume = PkgUnit.Box;
			RateLine.TL_ActualPercentage = 0; // we want chargeable value to be used for breaks search
			RateLine.ConversionFactor = new ConversionFactor(100, QuantityUnit.KG, QuantityUnit.M3);
			Calculator.IsAccumulated = false;
			Calculator.AddRateLineItem(Business.Calculator.Items.Operator.Minus, 50m, 5m, QuantityUnit.KG);
			Calculator.AddRateLineItem(Business.Calculator.Items.Operator.Plus, 50m, 4m);
			Calculator.AddRateLineItem(Business.Calculator.Items.Operator.Plus, 100m, 3m);

			var criteria = GetWarehouseCriteria(RateLine.Header.Header);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehousePackages((rateableMeasureSet) =>
			{
				measures.AddWarehouseOuterPackage(volume: 4m /* 400 KG*/, weight: 300m, packageCount: 10m, packageType: PkgUnit.Box, docketReference: "ABC");
				measures.AddWarehouseOuterPackage(volume: 5m /* 500 KG */, weight: 5000m, packageCount: 20m, packageType: PkgUnit.Box, docketReference: "ABC");
				measures.AddWarehouseOuterPackage(volume: 6m /*600 KG*/, weight: 200m, packageCount: 300m, packageType: PkgUnit.Package, docketReference: "ABC");
			});
			Factory.Save();

			AssertAutorate
			(
				criteria,
				new[]
				{
					// 400 KG / 10 BOX = $5 @ BOX (<50 KG)
					// 5000 KG / 20 BOX = $3 @ BOX (>100 KG)
					new SimpleArInfo
					{
						Amount = 110m,
						InvoiceLineDesc = "Warehouse Charge ABC",
						CalculationSingleLineDescription = @"WHSCHG: 10 Box(s) @ AUD 5.00/Box (for warehouse package/s: 10x BOX)
WHSCHG: 20 Box(s) @ AUD 3.00/Box (for warehouse package/s: 20x BOX)"
					}
				}
			);
		}

		public void TestNonCumulative_BreakUnit_PerWeight_ActualPercentage80()
		{
			RateLine.TL_WeightVolume = PkgUnit.Box;
			RateLine.TL_ActualPercentage = 80;
			RateLine.ConversionFactor = new ConversionFactor(10, "KG", "M3");
			Calculator.IsAccumulated = false;
			Calculator.AddRateLineItem(Business.Calculator.Items.Operator.Minus, 5m, 5m, QuantityUnit.KG);
			Calculator["+5"] = (ZDecimal)10m;
			Calculator["+20"] = (ZDecimal)15m;

			var criteria = GetWarehouseCriteria(RateLine.Header.Header);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehousePackages((rateableMeasureSet) =>
			{
				measures.AddWarehouseOuterPackage(volume: 2m, weight: 40m, packageCount: 10m, packageType: PkgUnit.Box, docketReference: "ABC");
				measures.AddWarehouseOuterPackage(volume: 5m, weight: 120m, packageCount: 20m, packageType: PkgUnit.Box, docketReference: "ABC");
			});
			Factory.Save();

			AssertAutorate
			(
				criteria,
				new[]
				{
					new SimpleArInfo
					{
						Amount = 250,
						InvoiceLineDesc = "Warehouse Charge ABC",
						CalculationSingleLineDescription = @"WHSCHG: 10 Box(s) @ AUD 5.00/Box (for warehouse package/s: 10x BOX)
WHSCHG: 20 Box(s) @ AUD 10.00/Box (for warehouse package/s: 20x BOX)" }
				}
			);
		}

		public void TestNonCumulative_BreakUnit_PerWeight_DifferentToKG()
		{
			RateLine.TL_WeightVolume = Weight.Pounds;
			Calculator.IsAccumulated = false;
			Calculator["-70"] = (ZDecimal)5m;
			Calculator["+70"] = (ZDecimal)10m;
			Calculator["+80"] = (ZDecimal)15m;

			var criteria = GetWarehouseCriteria(RateLine.Header.Header);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehousePackages((rateableMeasureSet) =>
			{
				measures.AddWarehouseOuterPackage(volume: 10m, weight: 30m, packageCount: 100m, packageType: PkgUnit.Basket, docketReference: "ABC");
				measures.AddWarehouseOuterPackage(volume: 20m, weight: 40m, packageCount: 200m, packageType: PkgUnit.Box, docketReference: "ABC");
			});
			Factory.Save();

			AssertAutorate
			(
				criteria,
				new[]
				{
					new SimpleArInfo
					{
						Amount = 1670m,
						InvoiceLineDesc = "Warehouse Charge ABC",
						CalculationSingleLineDescription = @"WHSCHG: 67 Pound(s) @ AUD 5.00/LB (for warehouse package/s: 100x BSK)
WHSCHG: 89 Pound(s) @ AUD 15.00/LB (for warehouse package/s: 200x BOX)"
					}
				}
			);
		}

		public void TestNonCumulative_BreakUnit_Volume()
		{
			RateLine.TL_WeightVolume = PkgUnit.Box;
			Calculator.IsAccumulated = false;
			Calculator.AddRateLineItem(Business.Calculator.Items.Operator.Minus, 5m, 5m, QuantityUnit.M3);
			Calculator["+5"] = (ZDecimal)10m;
			Calculator["+20"] = (ZDecimal)15m;

			var criteria = GetWarehouseCriteria(RateLine.Header.Header);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehousePackages((rateableMeasureSet) =>
			{
				measures.AddWarehouseOuterPackage(volume: 120m, weight: 40m, packageCount: 10m, packageType: PkgUnit.Box, docketReference: "ABC");
				measures.AddWarehouseOuterPackage(volume: 40m, weight: 50m, packageCount: 20m, packageType: PkgUnit.Box, docketReference: "ABC");
			});
			Factory.Save();

			AssertAutorate
			(
				criteria,
				new[]
				{
					new SimpleArInfo
					{
						Amount = 200,
						InvoiceLineDesc = "Warehouse Charge ABC",
						CalculationSingleLineDescription = @"WHSCHG: 10 Box(s) @ AUD 10.00/Box (for warehouse package/s: 10x BOX)
WHSCHG: 20 Box(s) @ AUD 5.00/Box (for warehouse package/s: 20x BOX)" }
				}
			);
		}

		#endregion

		#region Cumulative

		public void TestCumulative()
		{
			RateLine.TL_WeightVolume = QuantityUnit.KG;
			Calculator.IsAccumulated = true;
			Calculator["-5"] = (ZDecimal)5m;
			Calculator["+5"] = (ZDecimal)10m;
			Calculator["+20"] = (ZDecimal)15m;

			var criteria = GetWarehouseCriteria(RateLine.Header.Header);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehousePackages((rateableMeasureSet) =>
			{
				measures.AddWarehouseOuterPackage(volume: 2m, weight: 50m, packageCount: 60m, packageType: PkgUnit.Basket, docketReference: "ABC");
				measures.AddWarehouseOuterPackage(volume: 5m, weight: 60m, packageCount: 70m, packageType: PkgUnit.Box, docketReference: "ABC");
			});
			Factory.Save();

			AssertAutorate
			(
				criteria,
				new[]
				{
					new SimpleArInfo
					{
						Amount = 1400m,
						InvoiceLineDesc = "Warehouse Charge ABC",
						CalculationSingleLineDescription = @"WHSCHG: 5 Kilogram(s) @ AUD 5.00/KG + 15 Kilogram(s) @ AUD 10.00/KG + 30 Kilogram(s) @ AUD 15.00/KG (for warehouse package/s: 60x BSK)
WHSCHG: 5 Kilogram(s) @ AUD 5.00/KG + 15 Kilogram(s) @ AUD 10.00/KG + 40 Kilogram(s) @ AUD 15.00/KG (for warehouse package/s: 70x BOX)" }
				}
			);
		}

		public void TestCumulative_ActualPercentage80()
		{
			RateLine.TL_WeightVolume = QuantityUnit.KG;
			Calculator.IsAccumulated = true;
			RateLine.TL_ActualPercentage = 80;
			Calculator["-5"] = (ZDecimal)5m;
			Calculator["+5"] = (ZDecimal)10m;
			Calculator["+20"] = (ZDecimal)15m;

			var criteria = GetWarehouseCriteria(RateLine.Header.Header);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehousePackages((rateableMeasureSet) =>
			{
				measures.AddWarehouseOuterPackage(volume: 40m, weight: 50m, packageCount: 60m, packageType: PkgUnit.Basket, docketReference: "ABC");
				measures.AddWarehouseOuterPackage(volume: 50m, weight: 60m, packageCount: 70m, packageType: PkgUnit.Box, docketReference: "ABC");
			});
			Factory.Save();

			AssertAutorate
			(
				criteria,
				new[]
				{
					// 80%x50KG + 20%x(40M3/0.006) = 1374KG
					// 80%x60KG + 20%x(50M3/0.006) = 1715KG
					new SimpleArInfo
					{
						Amount = 46085m,
						InvoiceLineDesc = "Warehouse Charge ABC",
						CalculationSingleLineDescription = @"WHSCHG: 5 Kilogram(s) @ AUD 5.00/KG + 15 Kilogram(s) @ AUD 10.00/KG + 1354 Kilogram(s) @ AUD 15.00/KG (for warehouse package/s: 60x BSK)
WHSCHG: 5 Kilogram(s) @ AUD 5.00/KG + 15 Kilogram(s) @ AUD 10.00/KG + 1695 Kilogram(s) @ AUD 15.00/KG (for warehouse package/s: 70x BOX)" }
				}
			);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			InsertChargeCode(Factory, "WHSCHG", "Warehouse Charge", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards);
		}

		protected abstract string CalculatorCode { get; }

		RateLine RateLine
		{
			get
			{
				if (rateLine == null)
				{
					var ratingHeader = Helper.NewClientRate(NewClient);
					var rateEntry = ratingHeader.AddRateEntry(RatingConstants.RateCategory.WHS, "ALL", "AU", "US");

					rateLine = rateEntry.AddRateLine("WHSCHG", CalculatorCode, QuantityUnit.KG);
					rateLine.TL_UnitFactor = UnitFactorList.Codes.PackageLine;
				}
				return rateLine;
			}
		}
		RateLine rateLine;

		T Calculator => RateLine.GetCalculator<T>();

		TestRatingCriteria GetWarehouseCriteria(OrgHeader localClient)
		{
			var criteria = new TestRatingCriteria();
			criteria.LocalClient = localClient;
			criteria.ConsumerType = JobInvoicingConsumerTypes.WarehouseOutwards;
			criteria.RateTypeToUse = RateType.Warehouse;
			criteria.ChargeCodeGroups.Add(ChargeCodeGroupList.Codes.WHSOutwards);
			criteria.Creditors = Creditors.New(OrgWithSource.New(localClient, new List<string>() { "Provider" }));
			criteria.PickupAddress = Helper.NewOrgHeader().MainAddress;
			criteria.DeliveryAddress = Helper.NewOrgHeader().MainAddress;

			return criteria;
		}

		void AssertAutorate(TestRatingCriteria criteria, IEnumerable<SimpleArInfo> expectedResults, string assertionMessage = default)
		{
			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var autoRater = new FreightAutoRater(new RatingContext());
				var results = autoRater.AutoRate(criteria, CostSell.Revenue);
				AssertRatingResults(assertionMessage, expectedResults, results);
			}
		}

		#endregion
	}
}
