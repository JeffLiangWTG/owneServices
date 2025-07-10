using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	public class ProductLineWarehouseTest : WarehouseTest
	{
		#region Company Tariff

		public void TestCompanyTariff_UnitCalculator_RateLineHasUNTUnit()
		{
			var orgHeader = Helper.NewOrgHeader(1);

			Factory.Save();

			var companyTariff = Helper.NewCompanyTariff();
			var rateEntry = companyTariff.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.WHS, Constants.RateMode.ALL, "AU", "US", "WHSIN", 1m, PkgUnit.Unit);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.ProductLine;

			companyTariff.Factory.Save();

			var criteria = GetWarehouseCriteria(orgHeader);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false, optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			measures.AddWarehouseDocketNormalLine(weightInKG: (10m, null), volumeInM3: (10, null), units: 100m, warehousePK: ZGuid.Empty, product1.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);
			measures.AddWarehouseDocketNormalLine(weightInKG: (10m, null), volumeInM3: (10, null), units: 200m, warehousePK: ZGuid.Empty, product2.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);

			AssertAutorate
			(
				assertionMessage: "GIVEN rateLine with ProductLine UnitFactor and UNT calculator WHEN autorate job line with same rate UQ THEN should be calculated per job line",
				criteria,
				CostSell.Revenue,
				expected: new[]
				{
					new SimpleArInfo
					{
						Amount = 300m,
						InvoiceLineDesc = "Warehouse In Charge",
						CalculationSingleLineDescription = @"WHSIN: 100 Unit(s) @ AUD 1.00/Unit (for warehouse line/s: PROD1(BAG))
WHSIN: 200 Unit(s) @ AUD 1.00/Unit (for warehouse line/s: PROD2(BAG))"
					}
				}
			);
		}

		#endregion

		#region Costing

		public void TestCosting_CMBCalculator_RateLineHasUNTUnit()
		{
			var costing = Helper.NewCosting(NewClient);
			var rateEntry = costing.AddRateEntryWithCMBRateLine(RatingConstants.RateCategory.WHS, Constants.RateMode.ALL, "AU", "US", "WHSIN", 1m, PkgUnit.Unit, "", "", "", ("-50", 5m), ("+50", 10m), ("+150", 15m));
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.ProductLine;

			Factory.Save();

			var criteria = GetWarehouseCriteria(costing.Header);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false, optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			measures.AddWarehouseDocketNormalLine(weightInKG: (10m, null), volumeInM3: (10, null), units: 100m, warehousePK: ZGuid.Empty, product1.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);
			measures.AddWarehouseDocketNormalLine(weightInKG: (10m, null), volumeInM3: (10, null), units: 200m, warehousePK: ZGuid.Empty, product1.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);

			AssertAutorate
			(
				assertionMessage: "GIVEN rateLine with ProductLine UnitFactor and CMB calculator WHEN autorate job line with same rate UQ THEN should be calculated per job line",
				criteria,
				CostSell.Cost,
				expected: new[]
				{
					new SimpleArInfo
					{
						Amount = 4000m,
						InvoiceLineDesc = "Warehouse In Charge",
						CalculationSingleLineDescription = @"WHSIN: 100 Unit(s) @ AUD 10.00/Unit (for warehouse line/s: PROD1(BAG))
WHSIN: 200 Unit(s) @ AUD 15.00/Unit (for warehouse line/s: PROD1(BAG))"
					}
				}
			);
		}

		#endregion

		#region CMB Calculator

		public void TestClientRate_CMBCalculator_RateLineHasM3Unit_CumulativeBreaks()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntryWithCMBRateLine(RatingConstants.RateCategory.WHS, Constants.RateMode.ALL, "AU", "US", "WHSIN", 1m, QuantityUnit.M3, "", "", "", ("-50", 5m), ("+50", 10m), ("+150", 15m));
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.ProductLine;
			rateEntry.RateLines[0].Calculator.IsAccumulated = true;

			Factory.Save();

			var criteria = GetWarehouseCriteria(clientRate.Header);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false, optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			measures.AddWarehouseDocketNormalLine(weightInKG: (10m, null), volumeInM3: (350, null), units: 100m, warehousePK: ZGuid.Empty, product1.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);
			measures.AddWarehouseDocketNormalLine(weightInKG: (20m, null), volumeInM3: (400, null), units: 200m, warehousePK: ZGuid.Empty, product2.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);

			AssertAutorate
			(
				assertionMessage: "GIVEN rateLine with ProductLine UnitFactor and CMB calculator WHEN autorate job line with same rate UQ THEN should be calculated per job line",
				criteria,
				CostSell.Revenue,
				expected: new[]
				{
					new SimpleArInfo
					{
						Amount = 9250m,
						InvoiceLineDesc = "Warehouse In Charge",
						CalculationSingleLineDescription = @"WHSIN: 50 Cubic Meter(s) @ AUD 5.00/M3 + 100 Cubic Meter(s) @ AUD 10.00/M3 + 200 Cubic Meter(s) @ AUD 15.00/M3 (for warehouse line/s: PROD1(BAG))
WHSIN: 50 Cubic Meter(s) @ AUD 5.00/M3 + 100 Cubic Meter(s) @ AUD 10.00/M3 + 250 Cubic Meter(s) @ AUD 15.00/M3 (for warehouse line/s: PROD2(BAG))"
					}
				}
			);
		}

		public void TestClientRate_CMBCalculator_RateLineHasM3Unit_ActualPercentage80()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntryWithCMBRateLine(RatingConstants.RateCategory.WHS, Constants.RateMode.ALL, "AU", "US", "WHSIN", 1m, QuantityUnit.M3, "", "", "", ("-50", 5m), ("+50", 10m), ("+150", 15m));
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.ProductLine;
			rateEntry.RateLines[0].TL_ActualPercentage = 80;

			Factory.Save();

			var criteria = GetWarehouseCriteria(clientRate.Header);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false, optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			measures.AddWarehouseDocketNormalLine(weightInKG: (10000m, null), volumeInM3: (30, null), units: 100m, warehousePK: ZGuid.Empty, product1.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);
			measures.AddWarehouseDocketNormalLine(weightInKG: (20000m, null), volumeInM3: (40, null), units: 200m, warehousePK: ZGuid.Empty, product2.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);

			AssertAutorate
			(
				assertionMessage: "GIVEN rateLine with ProductLine UnitFactor and CMB calculator WHEN autorate job line with same rate UQ THEN should be calculated per job line",
				criteria,
				CostSell.Revenue,
				expected: new[]
				{
					new SimpleArInfo
					{
						Amount = 740m,
						InvoiceLineDesc = "Warehouse In Charge",
						CalculationSingleLineDescription = @"WHSIN: 36 Cubic Meter(s) @ AUD 5.00/M3 (for warehouse line/s: PROD1(BAG))
WHSIN: 56 Cubic Meter(s) @ AUD 10.00/M3 (for warehouse line/s: PROD2(BAG))"
					}
				}
			);
		}

		public void TestClientRate_CMBCalculator_RateLineHasUNTUnit()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntryWithCMBRateLine(RatingConstants.RateCategory.WHS, Constants.RateMode.ALL, "AU", "US", "WHSIN", 1m, PkgUnit.Unit, "", "", "", ("-50", 5m), ("+50", 10m), ("+150", 15m));
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.ProductLine;

			Factory.Save();

			var criteria = GetWarehouseCriteria(clientRate.Header);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false, optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			measures.AddWarehouseDocketNormalLine(weightInKG: (10m, null), volumeInM3: (10, null), units: 100m, warehousePK: ZGuid.Empty, product1.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);
			measures.AddWarehouseDocketNormalLine(weightInKG: (10m, null), volumeInM3: (10, null), units: 200m, warehousePK: ZGuid.Empty, product2.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);

			AssertAutorate
			(
				assertionMessage: "GIVEN rateLine with ProductLine UnitFactor and CMB calculator WHEN autorate job line with same rate UQ THEN should be calculated per job line",
				criteria,
				CostSell.Revenue,
				expected: new[]
				{
					new SimpleArInfo
					{
						Amount = 4000m,
						InvoiceLineDesc = "Warehouse In Charge",
						CalculationSingleLineDescription = @"WHSIN: 100 Unit(s) @ AUD 10.00/Unit (for warehouse line/s: PROD1(BAG))
WHSIN: 200 Unit(s) @ AUD 15.00/Unit (for warehouse line/s: PROD2(BAG))"
					}
				}
			);
		}

		public void TestClientRate_CMBCalculator_RateLineHasPalletUnit()
		{
			Helper.AddPartUnit(product1, package: PkgUnit.Unit, parentPackage: PkgUnit.Carton, quantityInParent: 20m);
			Helper.AddPartUnit(product1, package: PkgUnit.Carton, parentPackage: PkgUnit.Pallet, quantityInParent: 500m);

			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntryWithCMBRateLine(RatingConstants.RateCategory.WHS, Constants.RateMode.ALL, "AU", "US", "WHSIN", 1m, PkgUnit.Pallet, "", "", "", ("-0.5", 5m), ("+0.5", 10m), ("+2", 15m));
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.ProductLine;

			Factory.Save();

			var criteria = GetWarehouseCriteria(clientRate.Header);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false, optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			measures.AddWarehouseDocketNormalLine(weightInKG: (10m, null), volumeInM3: (10, null), units: 100m, warehousePK: ZGuid.Empty, product1.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);
			measures.AddWarehouseDocketNormalLine(weightInKG: (10m, null), volumeInM3: (10, null), units: 200m, warehousePK: ZGuid.Empty, product1.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);

			AssertAutorate
			(
				assertionMessage: "GIVEN rateLine with ProductLine UnitFactor and CMB calculator WHEN autorate job line with different rate UQ THEN should be calculated per job line",
				criteria,
				CostSell.Revenue,
				expected: new[]
				{
					new SimpleArInfo
					{
						Amount = 20m,
						InvoiceLineDesc = "Warehouse In Charge",
						CalculationSingleLineDescription = @"WHSIN: 1 Pallet(s) @ AUD 10.00/Pallet (for warehouse line/s: PROD1(BAG))
WHSIN: 1 Pallet(s) @ AUD 10.00/Pallet (for warehouse line/s: PROD1(BAG))"
					}
				}
			);
		}

		public void TestClientRate_CMBCalculator_RateLineHasKGUnit()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntryWithCMBRateLine(RatingConstants.RateCategory.WHS, Constants.RateMode.ALL, "AU", "US", "WHSIN", 1m, QuantityUnit.KG, "", "", "", ("-15", 5m), ("+15", 10m), ("25", 15m));
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.ProductLine;

			Factory.Save();

			var criteria = GetWarehouseCriteria(clientRate.Header);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false, optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			measures.AddWarehouseDocketNormalLine(weightInKG: (10m, null), volumeInM3: (30, null), units: 100m, warehousePK: ZGuid.Empty, product1.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);
			measures.AddWarehouseDocketNormalLine(weightInKG: (20m, null), volumeInM3: (40, null), units: 200m, warehousePK: ZGuid.Empty, product2.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);

			AssertAutorate
			(
				assertionMessage: "GIVEN rateLine with ProductLine UnitFactor and CMB calculator WHEN autorate rate KG UQ THEN should be calculated per job line",
				criteria,
				CostSell.Revenue,
				expected: new[]
				{
					new SimpleArInfo
					{
						Amount = 250m,
						InvoiceLineDesc = "Warehouse In Charge",
						CalculationSingleLineDescription = @"WHSIN: 10 Kilogram(s) @ AUD 5.00/KG (for warehouse line/s: PROD1(BAG))
WHSIN: 20 Kilogram(s) @ AUD 10.00/KG (for warehouse line/s: PROD2(BAG))"
					}
				}
			);
		}

		#endregion

		#region Unit Calculator

		public void TestClientRate_UnitCalculator()
		{
			Helper.AddPartUnit(product1, package: PkgUnit.Unit, parentPackage: PkgUnit.Pail, quantityInParent: 3m); // 1PAI = 3UNT
			Helper.AddPartUnit(product2, package: PkgUnit.Unit, parentPackage: PkgUnit.Pail, quantityInParent: 4m); // 1PAI = 4UNT

			var ratingHeader = CreateRatingHeader(NewClient);
			var rateEntry = ratingHeader.AddRateEntry(RatingConstants.RateCategory.WHS, RateMode.ALL, "AU", "US");
			var rateLine = rateEntry.AddRateLine("WHSIN", UnitCalculator.Code, PkgUnit.Pail);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.ProductLine;
			var calculator = rateLine.GetCalculator<UnitCalculator>();
			calculator.PerUnit = 1m;

			Factory.Save();

			var criteria = GetWarehouseCriteria(NewClient);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false, optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			measures.AddWarehouseDocketNormalLine(weightInKG: (10m, null), volumeInM3: (30, null), units: 100m, warehousePK: ZGuid.Empty, product1.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);
			measures.AddWarehouseDocketNormalLine(weightInKG: (20m, null), volumeInM3: (40, null), units: 200m, warehousePK: ZGuid.Empty, product2.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Pail);

			AssertAutorate
			(
				criteria,
				expected: new[]
				{
					// AddWarehouseDocketNormalLine units is using 'UNT' instead of packType
					// Product1: 100UNT = 34PAI
					// Product2: 200UNT = 50PAI
					new SimpleArInfo
					{
						Amount = 84m,
						InvoiceLineDesc = "Warehouse In Charge",
						CalculationSingleLineDescription = @"WHSIN: 34 Pail(s) @ AUD 1.00/Pail (for warehouse line/s: PROD1(BAG))
WHSIN: 50 Pail(s) @ AUD 1.00/Pail (for warehouse line/s: PROD2(PAI))"
					}
				}
			);
		}

		public void TestClientRate_UnitCalculator_RateLineWithWarehousePK()
		{
			var warehouse1 = Helper.NewWarehouse();
			var warehouse2 = Helper.NewWarehouse();

			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.WHS, Constants.RateMode.ALL, "AU", "US", "WHSIN", 1m, PkgUnit.Unit);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.ProductLine;
			rateEntry.TI_WW_Warehouse = warehouse1.PK;

			Factory.Save();

			var criteria = GetWarehouseCriteria(clientRate.Header);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false, optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			measures.AddWarehouseDocketNormalLine(weightInKG: (10m, null), volumeInM3: (10, null), units: 100m, warehousePK: warehouse1.PK, product1.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);
			measures.AddWarehouseDocketNormalLine(weightInKG: (10m, null), volumeInM3: (10, null), units: 200m, warehousePK: warehouse2.PK, product1.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);

			AssertAutorate
			(
				assertionMessage: "GIVEN rateEntry with WarehousePK WHEN autorate THEN should be calculated per job line and filtered by warehouse",
				criteria,
				CostSell.Revenue,
				expected: new[]
				{
					new SimpleArInfo { Amount = 100m, InvoiceLineDesc = "Warehouse In Charge", CalculationSingleLineDescription = "WHSIN: 100 Unit(s) @ AUD 1.00/Unit (for warehouse line/s: PROD1(BAG))" }
				}
			);
		}

		public void TestClientRate_UnitCalculator_RateLineHasUNTUnit_ActualPercentage80()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.WHS, Constants.RateMode.ALL, "AU", "US", "WHSIN", 1m, QuantityUnit.M3);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.ProductLine;
			rateEntry.RateLines[0].TL_ActualPercentage = 80;

			Factory.Save();

			var criteria = GetWarehouseCriteria(clientRate.Header);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false, optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			measures.AddWarehouseDocketNormalLine(weightInKG: (10000m, null), volumeInM3: (30, null), units: 100m, warehousePK: ZGuid.Empty, product1.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);
			measures.AddWarehouseDocketNormalLine(weightInKG: (20000m, null), volumeInM3: (40, null), units: 200m, warehousePK: ZGuid.Empty, product2.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);

			AssertAutorate
			(
				assertionMessage: "GIVEN rateLine with ProductLine UnitFactor and UNT calculator WHEN autorate job line with same rate UQ THEN should be calculated per job line",
				criteria,
				CostSell.Revenue,
				expected: new[]
				{
					new SimpleArInfo
					{
						Amount = 92m,
						InvoiceLineDesc = "Warehouse In Charge",
						CalculationSingleLineDescription = @"WHSIN: 36 Cubic Meter(s) @ AUD 1.00/M3 (for warehouse line/s: PROD1(BAG))
WHSIN: 56 Cubic Meter(s) @ AUD 1.00/M3 (for warehouse line/s: PROD2(BAG))"
					}
				}
			);
		}

		public void TestClientRate_UnitCalculator_RateLineHasUNTUnit()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.WHS, Constants.RateMode.ALL, "AU", "US", "WHSIN", 1m, PkgUnit.Unit);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.ProductLine;

			Factory.Save();

			var criteria = GetWarehouseCriteria(clientRate.Header);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false, optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			measures.AddWarehouseDocketNormalLine(weightInKG: (10m, null), volumeInM3: (10, null), units: 100m, warehousePK: ZGuid.Empty, product1.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);
			measures.AddWarehouseDocketNormalLine(weightInKG: (10m, null), volumeInM3: (10, null), units: 200m, warehousePK: ZGuid.Empty, product2.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);

			AssertAutorate
			(
				assertionMessage: "GIVEN rateLine with ProductLine UnitFactor and UNT calculator WHEN autorate job line with same rate UQ THEN should be calculated per job line",
				criteria,
				CostSell.Revenue,
				expected: new[]
				{
					new SimpleArInfo
					{
						Amount = 300m,
						InvoiceLineDesc = "Warehouse In Charge",
						CalculationSingleLineDescription = @"WHSIN: 100 Unit(s) @ AUD 1.00/Unit (for warehouse line/s: PROD1(BAG))
WHSIN: 200 Unit(s) @ AUD 1.00/Unit (for warehouse line/s: PROD2(BAG))"
					}
				}
			);
		}

		public void TestClientRate_UnitCalculator_RateLineHasPalletUnit()
		{
			Helper.AddPartUnit(product1, package: PkgUnit.Unit, parentPackage: PkgUnit.Carton, quantityInParent: 20m);
			Helper.AddPartUnit(product1, package: PkgUnit.Carton, parentPackage: PkgUnit.Pallet, quantityInParent: 500m);

			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.WHS, Constants.RateMode.ALL, "AU", "US", "WHSIN", 1m, PkgUnit.Pallet);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.ProductLine;

			Factory.Save();

			var criteria = GetWarehouseCriteria(clientRate.Header);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false, optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			measures.AddWarehouseDocketNormalLine(weightInKG: (10m, null), volumeInM3: (10, null), units: 100m, warehousePK: ZGuid.Empty, product1.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);
			measures.AddWarehouseDocketNormalLine(weightInKG: (10m, null), volumeInM3: (10, null), units: 200m, warehousePK: ZGuid.Empty, product1.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);

			AssertAutorate
			(
				assertionMessage: "GIVEN rateLine with ProductLine UnitFactor and UNT calculator WHEN autorate job line with different rate UQ THEN should be calculated per job line",
				criteria,
				CostSell.Revenue,
				expected: new[]
				{
					new SimpleArInfo
					{
						Amount = 2m,
						InvoiceLineDesc = "Warehouse In Charge",
						CalculationSingleLineDescription = @"WHSIN: 1 Pallet(s) @ AUD 1.00/Pallet (for warehouse line/s: PROD1(BAG))
WHSIN: 1 Pallet(s) @ AUD 1.00/Pallet (for warehouse line/s: PROD1(BAG))"
					}
				}
			);
		}

		public void TestClientRate_UnitCalculator_RateLineHasKGUnit()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.WHS, Constants.RateMode.ALL, "AU", "US", "WHSIN", 1m, QuantityUnit.KG);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.ProductLine;

			Factory.Save();

			var criteria = GetWarehouseCriteria(clientRate.Header);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false, optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			measures.AddWarehouseDocketNormalLine(weightInKG: (10m, null), volumeInM3: (30, null), units: 100m, warehousePK: ZGuid.Empty, product1.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);
			measures.AddWarehouseDocketNormalLine(weightInKG: (20m, null), volumeInM3: (40, null), units: 200m, warehousePK: ZGuid.Empty, product2.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);

			AssertAutorate
			(
				assertionMessage: "GIVEN rateLine with ProductLine UnitFactor and UNT calculator WHEN autorate rate KG UQ THEN should be calculated per job line",
				criteria,
				CostSell.Revenue,
				expected: new[]
				{
					new SimpleArInfo
					{
						Amount = 30m,
						InvoiceLineDesc = "Warehouse In Charge",
						CalculationSingleLineDescription = @"WHSIN: 10 Kilogram(s) @ AUD 1.00/KG (for warehouse line/s: PROD1(BAG))
WHSIN: 20 Kilogram(s) @ AUD 1.00/KG (for warehouse line/s: PROD2(BAG))"
					}
				}
			);
		}

		public void TestClientRate_UnitCalculator_RateLineUnitHasNoProductConversion()
		{
			Helper.AddPartUnit(product1, package: PkgUnit.Unit, parentPackage: PkgUnit.Pallet, quantityInParent: 2m);

			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.WHS, Constants.RateMode.ALL, "AU", "US", "WHSIN", 10m, PkgUnit.Tote);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.ProductLine;

			Factory.Save();

			var criteria = GetWarehouseCriteria(clientRate.Header);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false, optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			measures.AddWarehouseDocketNormalLine(weightInKG: (10m, null), volumeInM3: (10, null), units: 5m, warehousePK: ZGuid.Empty, product1.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);

			AssertAutorate
			(
				assertionMessage: "No conversion from product UQ to rateLine Unit",
				criteria,
				CostSell.Revenue,
				expected: new[]
				{
					new SimpleArInfo { Amount = 0m, InvoiceLineDesc = "Warehouse In Charge", CalculationSingleLineDescription = "WHSIN: Calculation failed due to no UNT to TOT unit conversion present on PROD1 (###1)" }
				}
			);
		}

		#endregion

		public void TestCartageCalculator()
		{
			var ratingHeader = CreateRatingHeader(NewClient);
			var rateEntry = ratingHeader.AddRateEntry(RatingConstants.RateCategory.WHS, RateMode.ALL, "AU", "US");
			var rateLine = rateEntry.AddRateLine("WHSIN", CartageCalculator.Code, Weight.Kilograms);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.ProductLine;
			var calculator = rateLine.GetCalculator<CartageCalculator>();
			calculator["-10"] = (ZDecimal)5m;
			calculator["+10"] = (ZDecimal)10m;
			calculator["+20"] = (ZDecimal)15m;
			Factory.Save();

			var criteria = GetWarehouseCriteria(NewClient);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false, optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			measures.AddWarehouseDocketNormalLine(weightInKG: (15m, null), volumeInM3: (30, null), units: 100m, warehousePK: ZGuid.Empty, product1.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);
			measures.AddWarehouseDocketNormalLine(weightInKG: (25m, null), volumeInM3: (40, null), units: 200m, warehousePK: ZGuid.Empty, product2.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Pail);

			AssertAutorate
			(
				criteria,
				expected: new[]
				{
					new SimpleArInfo
					{
						Amount = 525m,
						InvoiceLineDesc = "Warehouse In Charge",
						CalculationSingleLineDescription = @"WHSIN: 15 Kilogram(s) @ AUD 10.00/KG (for warehouse line/s: PROD1(BAG))
WHSIN: 25 Kilogram(s) @ AUD 15.00/KG (for warehouse line/s: PROD2(PAI))"
					},
				}
			);
		}

		public void TestCartageZoneDistanceCalculator()
		{
			var orgHeader = NewClient;

			var warehouse = Helper.NewWarehouse();
			var warehouseOrgHeader = Helper.NewOrgHeader();
			var warehouseAddress = warehouseOrgHeader.MainAddress;
			warehouseAddress.OA_Address1 = "123 Fake Street";
			warehouseAddress.OA_City = "Sydney";
			warehouseAddress.OA_State = "NSW";
			warehouseAddress.OA_PostCode = "2000";
			warehouseAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			warehouse[WhsWarehouseSchema.WW_OA_WarehouseAddress] = warehouseOrgHeader.MainAddress.PK;

			var fromPostCode = Factory.LoadTop1<RefPostCode>(new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, "2000"));
			var zoneSetAU = Helper.CreateRateTransportZoneSet(orgHeader, CountryCodes.Australia);
			var zoneAU = zoneSetAU.CreateRateTransportZoneForTest("AU Zone");
			zoneAU.CreateRateTransportZoneItemForTest(fromPostCode);

			var ratingHeader = CreateRatingHeader(orgHeader);
			var rateEntry = ratingHeader.AddRateEntry(RatingConstants.RateCategory.WHS, RateMode.ALL, "AU", "US");
			var rateLine = rateEntry.AddRateLine("WHSIN", CartageZoneDistanceCalculator.Code, Weight.Kilograms);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.ProductLine;
			rateLine.ConversionFactor = new ConversionFactor(250m, Weight.Kilograms, Volume.CubicMetres);
			var calculator = rateLine.GetCalculator<CartageZoneDistanceCalculator>();
			calculator.IsAccumulated = false;
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Minus, 5, 5m, zoneAU.PK);
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 5, 10m, zoneAU.PK);
			calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 20, 15m, zoneAU.PK);

			Factory.Save();

			var criteria = GetWarehouseCriteria(orgHeader);
			criteria.PickupAddress = Helper.NewOrgHeader().MainAddress;
			((OrgAddress)criteria.PickupAddress).OA_PostCode = "2000";

			var measures = criteria.RateableMeasures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false, optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			measures.AddWarehouseDocketNormalLine(weightInKG: (10m, null), volumeInM3: (100, null), units: 100m, warehousePK: warehouse.PK.ToGuid(), product1.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);
			measures.AddWarehouseDocketNormalLine(weightInKG: (30m, null), volumeInM3: (200, null), units: 200m, warehousePK: warehouse.PK.ToGuid(), product2.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Pail);

			AssertAutorate
			(
				criteria,
				expected: new[]
				{
					new SimpleArInfo
					{
						Amount = 550m,
						InvoiceLineDesc = "Warehouse In Charge",
						CalculationSingleLineDescription = @"WHSIN: 10 Kilogram(s) @ AUD 10.00/KG (for warehouse line/s: PROD1(BAG))
WHSIN: 30 Kilogram(s) @ AUD 15.00/KG (for warehouse line/s: PROD2(PAI))"
					}
				}
			);
		}

		public void TestFlatPlusPerUnitCalculator()
		{
			var ratingHeader = Helper.NewClientRate(NewClient);
			var rateEntry = ratingHeader.AddRateEntry(RatingConstants.RateCategory.WHS, RateMode.ALL, "AU", "US");
			var rateLine = rateEntry.AddRateLine("WHSIN", FlatPlusPerUnitCalculator.Code, Weight.Kilograms);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.ProductLine;
			var calculator = rateLine.GetCalculator<FlatPlusPerUnitCalculator>();
			calculator.BaseRate = 20m;
			calculator.PerUnit = 10m;

			Factory.Save();

			var criteria = GetWarehouseCriteria(NewClient);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false, optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			measures.AddWarehouseDocketNormalLine(weightInKG: (10m, null), volumeInM3: (30, null), units: 100m, warehousePK: ZGuid.Empty, product1.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);
			measures.AddWarehouseDocketNormalLine(weightInKG: (20m, null), volumeInM3: (40, null), units: 200m, warehousePK: ZGuid.Empty, product2.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Pail);

			AssertAutorate
			(
				criteria,
				expected: new[]
				{
					new SimpleArInfo
					{
						Amount = 340m,
						InvoiceLineDesc = "Warehouse In Charge",
						CalculationSingleLineDescription = @"WHSIN: Base Rate AUD 20.00 (for warehouse line/s: PROD1(BAG)) + 10 Kilogram(s) @ AUD 10.00/KG (for warehouse line/s: PROD1(BAG))
WHSIN: Base Rate AUD 20.00 (for warehouse line/s: PROD2(PAI)) + 20 Kilogram(s) @ AUD 10.00/KG (for warehouse line/s: PROD2(PAI))"
					}
				}
			);
		}

		public void TestNoteCalculator()
		{
			Helper.AddPartUnit(product1, package: PkgUnit.Unit, parentPackage: PkgUnit.Pail, quantityInParent: 2m);

			var ratingHeader = Helper.NewClientRate(NewClient);
			var rateEntry = ratingHeader.AddRateEntry(RatingConstants.RateCategory.WHS, RateMode.ALL, "AU", "US");
			var rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, PkgUnit.Pail);
			rateLine1.TL_UnitFactor = UnitFactorList.Codes.ProductLine;
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 100m;
			var rateLine2 = rateEntry.AddRateLine("FRT", NoteCalculator.Code);
			rateLine2.TL_UnitFactor = UnitFactorList.Codes.ProductLine;
			rateLine2.GetCalculator<NoteCalculator>().ShowOnBillingWithoutPrefix = false;
			var rateLineItem2 = rateLine2.RateLineItems.AddNew();
			rateLineItem2.TM_Text = "charge note";
			rateLineItem2.TM_Value = 50m;

			Factory.Save();

			var criteria = GetWarehouseCriteria(NewClient);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false, optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			measures.AddWarehouseDocketNormalLine(weightInKG: (10m, null), volumeInM3: (10, null), units: 100m, warehousePK: ZGuid.Empty, product1.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);
			measures.AddWarehouseDocketNormalLine(weightInKG: (10m, null), volumeInM3: (10, null), units: 200m, warehousePK: ZGuid.Empty, product2.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Pail);

			AssertAutorate
			(
				criteria,
				expected: new[]
				{
					new SimpleArInfo
					{
						IgnoreNewLineTabSpace = true,
						Amount = 5000m,
						CalculationSingleLineDescription = @"FRT: 50 Pail(s) @ AUD 100.00/Pail (for warehouse line/s: PROD1(BAG))
FRT: Calculation failed due to no UNT to PAI unit conversion present on PROD2 (###2)
FRT: Base Rate AUD 0.00 (for warehouse line/s: PROD1(BAG))
FRT: Base Rate AUD 0.00 (for warehouse line/s: PROD2(PAI))",
					InvoiceLineDesc = @"RATE NOTE: International Freight
charge note $50
RATE NOTE: International Freight
charge note $50"
					}
				}
			);
		}

		public void TestFirstPlusAdditionalCalculator()
		{
			var ratingHeader = CreateRatingHeader(NewClient);
			var rateEntry = ratingHeader.AddRateEntry(RatingConstants.RateCategory.WHS, RateMode.ALL, "AU", "US");
			var rateLine = rateEntry.AddRateLine("WHSIN", FirstPlusAdditionalCalculator.Code, Weight.Kilograms);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.ProductLine;
			var calculator = rateLine.GetCalculator<FirstPlusAdditionalCalculator>();
			calculator.First = 20m;
			calculator.Additional = 10m;

			Factory.Save();

			var criteria = GetWarehouseCriteria(NewClient);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false, optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			measures.AddWarehouseDocketNormalLine(weightInKG: (10m, null), volumeInM3: (30, null), units: 100m, warehousePK: ZGuid.Empty, product1.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);
			measures.AddWarehouseDocketNormalLine(weightInKG: (20m, null), volumeInM3: (40, null), units: 200m, warehousePK: ZGuid.Empty, product2.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Pail);

			AssertAutorate
			(
				criteria,
				expected: new[]
				{
					new SimpleArInfo
					{
						Amount = 320m,
						InvoiceLineDesc = "Warehouse In Charge",
						CalculationSingleLineDescription = @"WHSIN: 1 Kilogram(s) @ AUD 20.00/KG + 19 Kilogram(s) @ AUD 10.00/KG (for warehouse line/s: PROD2(PAI))
WHSIN: 1 Kilogram(s) @ AUD 20.00/KG + 9 Kilogram(s) @ AUD 10.00/KG (for warehouse line/s: PROD1(BAG))"
					}
				}
			);
		}

		public void TestExcludeCompanyTariffsCalculator()
		{
			Helper.AddPartUnit(product1, package: PkgUnit.Unit, parentPackage: PkgUnit.Pail, quantityInParent: 2m);

			var orgHeader = Helper.NewOrgHeader(1);

			var companyTariff = Helper.NewCompanyTariff();
			var companyTariffRateEntry1 = companyTariff.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.WHS, RateMode.ALL, "AU", "US", "WHSIN", 10m, PkgUnit.Bag);
			var rateLine1 = companyTariffRateEntry1.RateLines[0];
			rateLine1.TL_UnitFactor = UnitFactorList.Codes.ProductLine;
			var rateLine2 = companyTariffRateEntry1.AddRateLine("WHSCH2", UnitCalculator.Code, PkgUnit.Pail);
			rateLine2.GetCalculator<UnitCalculator>().PerUnit = 100m;
			rateLine2.TL_UnitFactor = UnitFactorList.Codes.ProductLine;

			companyTariff.Factory.Save();

			var clientRate = Helper.NewClientRate(orgHeader);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS, RateMode.ALL, "AU", "US");
			rateEntry.AddRateLine("WHSIN", ExcludeCompanyTariffsCalculator.Code);
			rateEntry.RateLines[0].TL_UnitFactor = UnitFactorList.Codes.ProductLine;

			Factory.Save();

			var criteria = GetWarehouseCriteria(orgHeader);
			var measures = criteria.RateableMeasures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false, optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			measures.AddWarehouseDocketNormalLine(weightInKG: (10m, null), volumeInM3: (10, null), units: 100m, warehousePK: ZGuid.Empty, product1.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Bag);
			measures.AddWarehouseDocketNormalLine(weightInKG: (10m, null), volumeInM3: (10, null), units: 200m, warehousePK: ZGuid.Empty, product2.PK, attributes: ProductAttributesMeasure.Empty, commodityCode: "", docketReference: "", packType: PkgUnit.Pail);

			AssertAutorate
			(
				criteria,
				expected: new[]
				{
					new SimpleArInfo
					{
						Amount = 5000m,
						InvoiceLineDesc = "Warehouse Charge 2",
						CalculationSingleLineDescription = @"WHSCH2: 50 Pail(s) @ AUD 100.00/Pail (for warehouse line/s: PROD1(BAG))
WHSCH2: Calculation failed due to no UNT to PAI unit conversion present on PROD2 (###2)"
					}
				}
			);
		}

		RatingHeader CreateRatingHeader(OrgHeader orgHeader) => Helper.NewClientRate(orgHeader);

		protected void AssertAutorate(TestRatingCriteria criteria, IEnumerable<SimpleArInfo> expected, string assertionMessage = default)
		{
			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var autoRater = new FreightAutoRater(new RatingContext());
				var results = autoRater.AutoRate(criteria, CostSell.Revenue);
				AssertRatingResults(assertionMessage, expected, results);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			InsertChargeCode(Factory, "WHSCH2", "Warehouse Charge 2", UnitCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);

			product1 = Helper.NewOrgSupplierPart(Helper.NewOrgHeader());
			Helper.SetProductWeightAndVolume(product1, weight: 2m, weightUQ: Weight.Kilograms, volume: 1m, volumeUQ: Volume.CubicMetres);

			product2 = Helper.NewOrgSupplierPart(Helper.NewOrgHeader());
			Helper.SetProductWeightAndVolume(product2, weight: 2m, weightUQ: Weight.Kilograms, volume: 1m, volumeUQ: Volume.CubicMetres);

			Factory.Save();
		}

		OrgSupplierPart product1;

		OrgSupplierPart product2;

		#endregion

		#region Implementation

		protected override JobInvoicingConsumerType CriteriaConsumerType => JobInvoicingConsumerTypes.WarehouseInwards;

		protected override string CriteriaChargeCodeGroup => ChargeCodeGroupList.Codes.WHSInwards;

		#endregion
	}
}
