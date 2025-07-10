using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	internal class WarehousePackageLineCartageZoneDistanceCalculatorTest : RatingTestCase
	{
		#region Non Cumulative

		public void TestNonCumulative()
		{
			RateLine.TL_WeightVolume = QuantityUnit.KG;
			Calculator.IsAccumulated = false;
			Calculator.AddRateLineItemWithZone(Business.Calculator.Items.Operator.Minus, 55, 5m, ZoneAU.PK);
			Calculator.AddRateLineItemWithZone(Business.Calculator.Items.Operator.Plus, 55, 10m, ZoneAU.PK);
			Calculator.AddRateLineItemWithZone(Business.Calculator.Items.Operator.Plus, 100, 15m, ZoneAU.PK);

			var criteria = GetWarehouseCriteria(RateLine.Header.Header);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehousePackages((rateableMeasureSet) =>
			{
				measures.AddWarehouseOuterPackage(volume: 40m, weight: 50m, packageCount: 200m, packageType: PkgUnit.Basket, docketReference: "ABC", warehousePK: Warehouse.PK.ToGuid());
				measures.AddWarehouseOuterPackage(volume: 50m, weight: 60m, packageCount: 300m, packageType: PkgUnit.Box, docketReference: "ABC", warehousePK: Warehouse.PK.ToGuid());
			});
			Factory.Save();

			AssertAutorate
			(
				criteria,
				new[]
				{
					new SimpleArInfo
					{
						Amount = 850m,
						InvoiceLineDesc = "Warehouse Charge ABC",
						CalculationSingleLineDescription = @"WHSCHG: 50 Kilogram(s) @ AUD 5.00/KG (for warehouse package/s: 200x BSK)
WHSCHG: 60 Kilogram(s) @ AUD 10.00/KG (for warehouse package/s: 300x BOX)"
					}
				}
			);
		}

		public void TestNonCumulative_ActualPercentage80()
		{
			RateLine.TL_WeightVolume = QuantityUnit.KG;
			RateLine.TL_ActualPercentage = 80;
			Calculator.IsAccumulated = false;
			Calculator.AddRateLineItemWithZone(Business.Calculator.Items.Operator.Minus, 1400, 5m, ZoneAU.PK);
			Calculator.AddRateLineItemWithZone(Business.Calculator.Items.Operator.Plus, 1400, 10m, ZoneAU.PK);
			Calculator.AddRateLineItemWithZone(Business.Calculator.Items.Operator.Plus, 2000, 15m, ZoneAU.PK);

			var criteria = GetWarehouseCriteria(RateLine.Header.Header);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehousePackages((rateableMeasureSet) =>
			{
				measures.AddWarehouseOuterPackage(volume: 40m, weight: 50m, packageCount: 60m, packageType: PkgUnit.Basket, docketReference: "ABC", warehousePK: Warehouse.PK.ToGuid());
				measures.AddWarehouseOuterPackage(volume: 50m, weight: 60m, packageCount: 70m, packageType: PkgUnit.Box, docketReference: "ABC", warehousePK: Warehouse.PK.ToGuid());
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
						Amount = 24020m,
						InvoiceLineDesc = "Warehouse Charge ABC",
						CalculationSingleLineDescription = @"WHSCHG: 1374 Kilogram(s) @ AUD 5.00/KG (for warehouse package/s: 60x BSK)
WHSCHG: 1715 Kilogram(s) @ AUD 10.00/KG (for warehouse package/s: 70x BOX)" }
				}
			);
		}

		public void TestNonCumulative_InclusiveBreaks()
		{
			RateLine.TL_WeightVolume = QuantityUnit.KG;
			Calculator.IsAccumulated = false;
			Calculator.UseInclusiveBreaks = true;
			Calculator.AddRateLineItemWithZone(Business.Calculator.Items.Operator.Minus, 25, 30m, ZoneAU.PK);
			Calculator.AddRateLineItemWithZone(Business.Calculator.Items.Operator.Plus, 25, 20m, ZoneAU.PK);
			Calculator.AddRateLineItemWithZone(Business.Calculator.Items.Operator.Plus, 50, 10m, ZoneAU.PK);

			var criteria = GetWarehouseCriteria(RateLine.Header.Header);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehousePackages((rateableMeasureSet) =>
			{
				measures.AddWarehouseOuterPackage(volume: 40m, weight: 25m, packageCount: 60m, packageType: PkgUnit.Basket, docketReference: "ABC", warehousePK: Warehouse.PK.ToGuid());
				measures.AddWarehouseOuterPackage(volume: 50m, weight: 50m, packageCount: 70m, packageType: PkgUnit.Box, docketReference: "ABC", warehousePK: Warehouse.PK.ToGuid());
			});
			Factory.Save();

			AssertAutorate
			(
				criteria,
				new[]
				{
					new SimpleArInfo
					{
						Amount = 1750m,
						InvoiceLineDesc = "Warehouse Charge ABC",
						CalculationSingleLineDescription = @"WHSCHG: 25 Kilogram(s) @ AUD 30.00/KG (for warehouse package/s: 60x BSK)
WHSCHG: 50 Kilogram(s) @ AUD 20.00/KG (for warehouse package/s: 70x BOX)" }
				}
			);
		}

		public void TestNonCumulative_HigherBreakLowerRate()
		{
			RateLine.TL_WeightVolume = QuantityUnit.KG;
			Calculator.IsAccumulated = false;
			Calculator.UseHigherChargeableLowerRateRule = true;
			Calculator.AddRateLineItemWithZone(Business.Calculator.Items.Operator.Minus, 45, 30m, ZoneAU.PK);
			Calculator.AddRateLineItemWithZone(Business.Calculator.Items.Operator.Plus, 45, 20m, ZoneAU.PK);
			Calculator.AddRateLineItemWithZone(Business.Calculator.Items.Operator.Plus, 200, 10m, ZoneAU.PK);

			var criteria = GetWarehouseCriteria(RateLine.Header.Header);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehousePackages((rateableMeasureSet) =>
			{
				measures.AddWarehouseOuterPackage(volume: 40m, weight: 150m, packageCount: 60m, packageType: PkgUnit.Basket, docketReference: "ABC", warehousePK: Warehouse.PK.ToGuid());
				measures.AddWarehouseOuterPackage(volume: 50m, weight: 60m, packageCount: 70m, packageType: PkgUnit.Box, docketReference: "ABC", warehousePK: Warehouse.PK.ToGuid());
			});
			Factory.Save();

			AssertAutorate
			(
				criteria,
				new[]
				{
					new SimpleArInfo
					{
						Amount = 3200m,
						InvoiceLineDesc = "Warehouse Charge ABC",
						CalculationSingleLineDescription = @"WHSCHG: 200 Kilogram(s) (HBLR is applied) @ AUD 10.00/KG (for warehouse package/s: 60x BSK)
WHSCHG: 60 Kilogram(s) @ AUD 20.00/KG (for warehouse package/s: 70x BOX)" }
				}
			);
		}

		public void TestNonCumulative_BreakUnit_PerWeight()
		{
			RateLine.TL_WeightVolume = PkgUnit.Box;
			Calculator.IsAccumulated = false;
			var rateLineItem = Calculator.AddRateLineItemWithZone(Business.Calculator.Items.Operator.Minus, 5m, 5m, ZoneAU.PK);
			rateLineItem.TM_BreakWeightVolume = QuantityUnit.KG;
			Calculator.AddRateLineItemWithZone(Business.Calculator.Items.Operator.Plus, 5, 10m, ZoneAU.PK);
			Calculator.AddRateLineItemWithZone(Business.Calculator.Items.Operator.Plus, 20, 15m, ZoneAU.PK);

			var criteria = GetWarehouseCriteria(RateLine.Header.Header);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehousePackages((rateableMeasureSet) =>
			{
				measures.AddWarehouseOuterPackage(volume: 40m, weight: 40m, packageCount: 10m, packageType: PkgUnit.Box, docketReference: "ABC", warehousePK: Warehouse.PK.ToGuid());
				measures.AddWarehouseOuterPackage(volume: 50m, weight: 120m, packageCount: 20m, packageType: PkgUnit.Box, docketReference: "ABC", warehousePK: Warehouse.PK.ToGuid());
				measures.AddWarehouseOuterPackage(volume: 60m, weight: 200m, packageCount: 300m, packageType: PkgUnit.Package, docketReference: "ABC", warehousePK: Warehouse.PK.ToGuid());
				measures.AddWarehouseOuterPackage(volume: 70m, weight: 300m, packageCount: 400m, packageType: PkgUnit.Box, docketReference: "ABC", warehousePK: Guid.NewGuid());
			});
			Factory.Save();

			AssertAutorate
			(
				criteria,
				new[]
				{
					new SimpleArInfo
					{
						Amount = 250m,
						InvoiceLineDesc = "Warehouse Charge ABC",
						CalculationSingleLineDescription = @"WHSCHG: 10 Box(s) @ AUD 5.00/Box (for warehouse package/s: 10x BOX)
WHSCHG: 20 Box(s) @ AUD 10.00/Box (for warehouse package/s: 20x BOX)"
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
			var rateLineItem = Calculator.AddRateLineItemWithZone(Business.Calculator.Items.Operator.Minus, 5m, 5m, ZoneAU.PK);
			rateLineItem.TM_BreakWeightVolume = QuantityUnit.KG;
			Calculator.AddRateLineItemWithZone(Business.Calculator.Items.Operator.Plus, 5, 10m, ZoneAU.PK);
			Calculator.AddRateLineItemWithZone(Business.Calculator.Items.Operator.Plus, 20, 15m, ZoneAU.PK);

			var criteria = GetWarehouseCriteria(RateLine.Header.Header);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehousePackages((rateableMeasureSet) =>
			{
				measures.AddWarehouseOuterPackage(volume: 2m, weight: 40m, packageCount: 10m, packageType: PkgUnit.Box, docketReference: "ABC", warehousePK: Warehouse.PK.ToGuid());
				measures.AddWarehouseOuterPackage(volume: 5m, weight: 120m, packageCount: 20m, packageType: PkgUnit.Box, docketReference: "ABC", warehousePK: Warehouse.PK.ToGuid());
			});
			Factory.Save();

			AssertAutorate
			(
				criteria,
				new[]
				{
					new SimpleArInfo
					{
						Amount = 250m,
						InvoiceLineDesc = "Warehouse Charge ABC",
						CalculationSingleLineDescription = @"WHSCHG: 10 Box(s) @ AUD 5.00/Box (for warehouse package/s: 10x BOX)
WHSCHG: 20 Box(s) @ AUD 10.00/Box (for warehouse package/s: 20x BOX)"
					}
				}
			);
		}

		#endregion

		#region Cumulative

		public void TestCumulative()
		{
			RateLine.TL_WeightVolume = QuantityUnit.KG;
			Calculator.IsAccumulated = true;
			Calculator.AddRateLineItemWithZone(Business.Calculator.Items.Operator.Minus, 5, 5m, ZoneAU.PK);
			Calculator.AddRateLineItemWithZone(Business.Calculator.Items.Operator.Plus, 5, 10m, ZoneAU.PK);
			Calculator.AddRateLineItemWithZone(Business.Calculator.Items.Operator.Plus, 20, 15m, ZoneAU.PK);

			var criteria = GetWarehouseCriteria(RateLine.Header.Header);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehousePackages((rateableMeasureSet) =>
			{
				measures.AddWarehouseOuterPackage(volume: 40m, weight: 50m, packageCount: 60m, packageType: PkgUnit.Basket, docketReference: "ABC", warehousePK: Warehouse.PK.ToGuid());
				measures.AddWarehouseOuterPackage(volume: 50m, weight: 60m, packageCount: 70m, packageType: PkgUnit.Box, docketReference: "ABC", warehousePK: Warehouse.PK.ToGuid());
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
			RateLine.TL_ActualPercentage = 80;
			Calculator.IsAccumulated = true;
			Calculator.AddRateLineItemWithZone(Business.Calculator.Items.Operator.Minus, 5, 5m, ZoneAU.PK);
			Calculator.AddRateLineItemWithZone(Business.Calculator.Items.Operator.Plus, 5, 10m, ZoneAU.PK);
			Calculator.AddRateLineItemWithZone(Business.Calculator.Items.Operator.Plus, 20, 15m, ZoneAU.PK);

			var criteria = GetWarehouseCriteria(RateLine.Header.Header);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehousePackages((rateableMeasureSet) =>
			{
				measures.AddWarehouseOuterPackage(volume: 40m, weight: 50m, packageCount: 60m, packageType: PkgUnit.Basket, docketReference: "ABC", warehousePK: Warehouse.PK.ToGuid());
				measures.AddWarehouseOuterPackage(volume: 50m, weight: 60m, packageCount: 70m, packageType: PkgUnit.Box, docketReference: "ABC", warehousePK: Warehouse.PK.ToGuid());
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

		public void TestCumulative_InclusiveBreaks()
		{
			RateLine.TL_WeightVolume = QuantityUnit.KG;
			Calculator.IsAccumulated = true;
			Calculator.UseInclusiveBreaks = true;
			Calculator.AddRateLineItemWithZone(Business.Calculator.Items.Operator.Minus, 25, 30m, ZoneAU.PK);
			Calculator.AddRateLineItemWithZone(Business.Calculator.Items.Operator.Plus, 25, 20m, ZoneAU.PK);
			Calculator.AddRateLineItemWithZone(Business.Calculator.Items.Operator.Plus, 50, 10m, ZoneAU.PK);

			var criteria = GetWarehouseCriteria(RateLine.Header.Header);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehousePackages((rateableMeasureSet) =>
			{
				measures.AddWarehouseOuterPackage(volume: 40m, weight: 25m, packageCount: 60m, packageType: PkgUnit.Basket, docketReference: "ABC", warehousePK: Warehouse.PK.ToGuid());
				measures.AddWarehouseOuterPackage(volume: 50m, weight: 50m, packageCount: 70m, packageType: PkgUnit.Box, docketReference: "ABC", warehousePK: Warehouse.PK.ToGuid());
			});
			Factory.Save();

			AssertAutorate
			(
				criteria,
				new[]
				{
					new SimpleArInfo
					{
						Amount = 2000m,
						InvoiceLineDesc = "Warehouse Charge ABC",
						CalculationSingleLineDescription = @"WHSCHG: 25 Kilogram(s) @ AUD 30.00/KG (for warehouse package/s: 60x BSK)
WHSCHG: 25 Kilogram(s) @ AUD 30.00/KG + 25 Kilogram(s) @ AUD 20.00/KG (for warehouse package/s: 70x BOX)" }
				}
			);
		}

		public void TestCumulative_HigherBreakLowerRate()
		{
			RateLine.TL_WeightVolume = QuantityUnit.KG;
			Calculator.IsAccumulated = true;
			Calculator.UseHigherChargeableLowerRateRule = true;
			Calculator.AddRateLineItemWithZone(Business.Calculator.Items.Operator.Minus, 45, 30m, ZoneAU.PK);
			Calculator.AddRateLineItemWithZone(Business.Calculator.Items.Operator.Plus, 45, 20m, ZoneAU.PK);
			Calculator.AddRateLineItemWithZone(Business.Calculator.Items.Operator.Plus, 200, 10m, ZoneAU.PK);

			var criteria = GetWarehouseCriteria(RateLine.Header.Header);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehousePackages((rateableMeasureSet) =>
			{
				measures.AddWarehouseOuterPackage(volume: 40m, weight: 150m, packageCount: 60m, packageType: PkgUnit.Basket, docketReference: "ABC", warehousePK: Warehouse.PK.ToGuid());
				measures.AddWarehouseOuterPackage(volume: 50m, weight: 60m, packageCount: 70m, packageType: PkgUnit.Box, docketReference: "ABC", warehousePK: Warehouse.PK.ToGuid());
			});
			Factory.Save();

			AssertAutorate
			(
				criteria,
				new[]
				{
					new SimpleArInfo
					{
						Amount = 5100m,
						InvoiceLineDesc = "Warehouse Charge ABC",
						CalculationSingleLineDescription = @"WHSCHG: 45 Kilogram(s) @ AUD 30.00/KG + 105 Kilogram(s) @ AUD 20.00/KG (for warehouse package/s: 60x BSK)
WHSCHG: 45 Kilogram(s) @ AUD 30.00/KG + 15 Kilogram(s) @ AUD 20.00/KG (for warehouse package/s: 70x BOX)" }
				},
				"Higher break lower rate doesn't apply to cumulative"
			);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			InsertChargeCode(Factory, "WHSCHG", "Warehouse Charge", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSOutwards);
		}

		protected string CalculatorCode => CartageZoneDistanceCalculator.Code;

		BusinessObject Warehouse
		{
			get
			{
				if (warehouse == null)
				{
					warehouse = Helper.NewWarehouse();
					var warehouseOrgHeader = Helper.NewOrgHeader();
					var warehouseAddress = warehouseOrgHeader.MainAddress;
					warehouseAddress.OA_Address1 = "123 Fake Street";
					warehouseAddress.OA_City = "Sydney";
					warehouseAddress.OA_State = "NSW";
					warehouseAddress.OA_PostCode = "2000";
					warehouseAddress.OA_RL_NKRelatedPortCode = "AUSYD";
					warehouse[WhsWarehouseSchema.WW_OA_WarehouseAddress] = warehouseOrgHeader.MainAddress.PK;
				}

				return warehouse;
			}
		}
		BusinessObject warehouse;

		RateTransportZone ZoneAU
		{
			get
			{
				if (zoneAU == null)
				{
					var fromPostCode = Factory.LoadTop1<RefPostCode>(new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, "2000"));
					var zoneSetAU = Helper.CreateRateTransportZoneSet(NewClient, CountryCodes.Australia);
					zoneAU = zoneSetAU.CreateRateTransportZoneForTest("AU Zone");
					zoneAU.CreateRateTransportZoneItemForTest(fromPostCode);
				}
				return zoneAU;
			}
		}
		RateTransportZone zoneAU;

		RateLine RateLine
		{
			get
			{
				if (rateLine == null)
				{
					var ratingHeader = Helper.NewClientRate(NewClient);
					var rateEntry = ratingHeader.AddRateEntry(RatingConstants.RateCategory.WHS, "ALL", "AU", "US");
					rateEntry.TI_WW_Warehouse = Warehouse.PK;

					rateLine = rateEntry.AddRateLine("WHSCHG", CalculatorCode, QuantityUnit.KG);
					rateLine.TL_UnitFactor = UnitFactorList.Codes.PackageLine;
				}
				return rateLine;
			}
		}
		RateLine rateLine;

		CartageZoneDistanceCalculator Calculator => RateLine.GetCalculator<CartageZoneDistanceCalculator>();

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
