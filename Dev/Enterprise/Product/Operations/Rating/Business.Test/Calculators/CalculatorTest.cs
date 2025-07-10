using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WiseRates.Api.Model;
using static Enterprise.Core.Constants;
using static Enterprise.Rating.Business.Calculator;
using Constants = Enterprise.Core.Constants;
using MeasureInfo = Enterprise.MasterFiles.Business.MeasureInfo;
using MeasureType = Enterprise.Rating.Integration.MeasureType;
using RefContainer = Enterprise.MasterFiles.Business.RefContainer;

namespace Enterprise.Rating.Business.Testing
{
	internal class CalculatorInternalTest : TestCaseWithFactory
	{
		public void TestCheckOrCreateItems_GivenRatingHeaderIsFormDeleteWithMissingRateLineItem_ThenShouldNotCreateMissingRateLineItem()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateEntry = costing.AddRateEntryWithCMBRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "BAF", 1m, QuantityUnit.KG, "", "", "", ("-50", 5m), ("+50", 10m), ("+150", 15m));

			// Simulate old rates that doesn't have BreaksPer RateLineItem
			var breaksPerRateLineItem = rateEntry.RateLines[0].RateLineItems.Cast<RateLineItem>().Single(rateLineItem => rateLineItem.TM_Type == Items.BreaksPer);
			breaksPerRateLineItem.Delete();

			Factory.Save();

			var loadedCosting = new BusinessObjectFactory().Load<Costing>(costing.PK);
			var loadedRateEntry = (RateEntry)loadedCosting.EntryCollections[RatingConstants.RateCategory.AIR].LoadedCollection.Single();
			var loadedRateLine = loadedRateEntry.ChildRateLines.Single();

			loadedCosting.IsFormDelete = true;

			var rateLineItems = loadedRateLine.Calculator.CheckOrCreateItems_ForTest();

			AssertContainsExactElementsInAnyOrder
			(
				"GIVEN RatingHeader.IsFormDelete = true with missing RateLineItem, WHEN CheckOrCreateItems THEN should not create missing RateLineItem",
				new string[] { Items.UseInclusiveBreaks, Items.HigherChargeableLowerRate, Items.UseAccumulated },
				rateLineItems.Select(rateLineItem => rateLineItem.TM_Type)
			);
		}

		public void TestCheckOrCreateItems_ShouldNotAddNullItem()
		{
			var clientRate = Factory.New<ClientRate>();
			var rateLine = clientRate.AddRateEntry("ORG").RateLines.AddNew();
			rateLine.TL_RateCalculator = UnitCalculator.Code;

			var rateLineItems = rateLine.Calculator.CheckOrCreateItems_ForTest();
			AssertEquals(1, rateLineItems.Count());

			rateLine.ReadOnly = true;
			rateLine.RateLineItems.RemoveAndDeleteAll();

			rateLineItems = rateLine.Calculator.CheckOrCreateItems_ForTest();
			AssertEquals(0, rateLineItems.Count());
		}

		public void TestRounding()
		{
			var clientRate = Factory.New<ClientRate>();
			var rateLine = clientRate.AddRateEntry("ORG").AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);

			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			parameters.ChargeableAmount = new Quantity(125.25m, "KG");

			AssertEquals(125.25m, rateLine.Calculator.ChargeableAmount(parameters).Amount);

			rateLine.TL_Rounding = RatingRoundingTypes.UpTo1;
			AssertEquals(126m, rateLine.Calculator.ChargeableAmount(parameters).Amount);

			rateLine.TL_Rounding = RatingRoundingTypes.UpToHalf;
			AssertEquals(125.5m, rateLine.Calculator.ChargeableAmount(parameters).Amount);

			rateLine.TL_Rounding = RatingRoundingTypes.Bankers;
			AssertEquals(125m, rateLine.Calculator.ChargeableAmount(parameters).Amount);

			rateLine.TL_Rounding = RatingRoundingTypes.Custom;
			rateLine.TL_RoundingFactor = 0.1m;
			AssertEquals(125.3m, rateLine.Calculator.ChargeableAmount(parameters).Amount);

			parameters.ChargeableAmount = new Quantity(5000m, "KG");
			rateLine.TL_Rounding = RatingRoundingTypes.Custom;
			rateLine.TL_RoundingFactor = 0.1m;
			AssertEquals(5000m, rateLine.Calculator.ChargeableAmount(parameters).Amount);

			rateLine.TL_RoundingFactor = 1m;
			AssertEquals(5000m, rateLine.Calculator.ChargeableAmount(parameters).Amount);

			parameters.ChargeableAmount = new Quantity(4999.06m, "KG");
			rateLine.TL_RoundingFactor = 0.1m;
			AssertEquals(4999.1m, rateLine.Calculator.ChargeableAmount(parameters).Amount);

			parameters.ChargeableAmount = new Quantity(125.25m, "KG");
			rateLine.TL_WeightVolumeMultiple = 100m;
			rateLine.TL_Rounding = RatingRoundingTypes.NoRounding;
			AssertEquals(125.25m, rateLine.Calculator.ChargeableAmount(parameters).Amount);

			rateLine.TL_Rounding = RatingRoundingTypes.UpTo1;
			AssertEquals(126m, rateLine.Calculator.ChargeableAmount(parameters).Amount);

			RatingDataRegistry.Instance.RoundingUsesWeightVolumeMultiple.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			rateLine.TL_Rounding = RatingRoundingTypes.UpTo1;
			AssertEquals(200m, rateLine.Calculator.ChargeableAmount(parameters).Amount);

			rateLine.TL_Rounding = RatingRoundingTypes.UpToHalf;
			AssertEquals(150m, rateLine.Calculator.ChargeableAmount(parameters).Amount);

			rateLine.TL_Rounding = RatingRoundingTypes.Bankers;
			AssertEquals(100m, rateLine.Calculator.ChargeableAmount(parameters).Amount);

			rateLine.TL_Rounding = RatingRoundingTypes.Custom;
			rateLine.TL_RoundingFactor = 0.2;
			AssertEquals(140m, rateLine.Calculator.ChargeableAmount(parameters).Amount);

			rateLine.TL_Rounding = RatingRoundingTypes.NoRounding;
			rateLine.TL_WeightVolumeMultiple = 0m;
			criteria.SetTime(new TimeInfo(1, 1, 15));
			rateLine.TL_WeightVolume = QuantityUnit.HR;
			AssertEquals(25.25m, rateLine.Calculator.ChargeableAmount(parameters).Amount);
			rateLine.TL_WeightVolume = QuantityUnit.DY;
			AssertEquals(2m, rateLine.Calculator.ChargeableAmount(parameters).Amount);
			rateLine.TL_WeightVolume = QuantityUnit.WK;
			AssertEquals(1m, rateLine.Calculator.ChargeableAmount(parameters).Amount);

			parameters.ChargeableAmount = new Quantity(0.3m, "KG");
			rateLine.TL_WeightVolume = "KG";

			rateLine.TL_Rounding = RatingRoundingTypes.UpTo1IfLessThanOne;
			AssertEquals(1m, rateLine.Calculator.ChargeableAmount(parameters).Amount);

			parameters.ChargeableAmount = new Quantity(1.3m, "KG");
			AssertEquals(1.3m, rateLine.Calculator.ChargeableAmount(parameters).Amount);

			RatingDataRegistry.Instance.RoundingUsesWeightVolumeMultiple.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var systemRoundings = new DefaultRoundingsCollection();
			DefaultRoundings systemRounding = systemRoundings.AddNew();
			systemRounding.Code = RatingConstants.RateCategory.ORG;
			systemRounding.RoundingType = RatingRoundingTypes.Custom;
			systemRounding.RoundingFactor = 0.5m;
			using (DataRegistryRating.Instance.DefaultRounding.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, systemRoundings))
			{
				rateLine.TL_Rounding = RatingRoundingTypes.DefaultFromRegistry;
				AssertEquals(1.5m, rateLine.Calculator.ChargeableAmount(parameters).Amount);
			}

			systemRounding.RoundingType = RatingRoundingTypes.Bankers;
			using (DataRegistryRating.Instance.DefaultRounding.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, systemRoundings))
			{
				AssertEquals(1m, rateLine.Calculator.ChargeableAmount(parameters).Amount);
			}
		}

		public void TestRoundingCalculations()
		{
			var testData = new Dictionary<ZString, (decimal inputValue, decimal expectedValue)[]>()
				{
					{ RatingRoundingTypes.NoRounding, new [] { (0.49m, 0.49m), (0.50m, 0.50m), (0.51m, 0.51m) } },
					{ RatingRoundingTypes.Chargeable, new [] { (0.49m, 0.49m), (0.50m, 0.50m), (0.51m, 0.51m) } },
					{
						RatingRoundingTypes.Bankers,
						new []
						{
							(0.49m, 0), (0.50m, 0m), (0.51m, 1m),
							(1.49m, 1m), (1.50m, 2m), (1.51m, 2m),
							(2.49m, 2m), (2.50m, 2m), (2.51m, 3m),
						}
					},
					{
						RatingRoundingTypes.UpTo1,
						new []
						{
							(0.49m, 1m), (0.50m, 1m), (0.51m, 1m),
							(1.49m, 2m), (1.50m, 2m), (1.51m, 2m),
						}
					},
					{
						RatingRoundingTypes.UpToHalf,
						new []
						{
							(0.49m, 0.5m), (0.50m, 0.50m), (0.51m, 1m),
							(1.49m, 1.5m), (1.50m, 1.50m), (1.51m, 2m),
						}
					},
					{
						RatingRoundingTypes.UpTo1IfLessThanOne,
						new []
						{
							(0.49m, 1m), (0.50m, 1m), (0.51m, 1m),
							(1.49m, 1.49m), (1.50m, 1.50m), (1.51m, 1.51m),
						}
					},
					{
						RatingRoundingTypes.Custom, // with rounding factor 0.72
						new []
						{
							(0.49m, 0.72m), (0.50m, 0.72m), (0.51m, 0.72m),
							(1.49m, 2.16m), (2.16m, 2.16m), (2.160001m, 2.88m),
						}
					},
				};

			var clientRate = Factory.New<ClientRate>();
			var rateLine = clientRate.AddRateEntry("ORG").AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);

			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			var ratingRoundingTypeList = new RatingRoundingTypeList();
			var roundingTypes = ratingRoundingTypeList.GetAllCodes();

			AssertContainsExactElementsInAnyOrder(roundingTypes, testData.Keys);

			rateLine.TL_RoundingFactor = 0.72m;

			foreach (var roundingType in roundingTypes)
			{
				foreach (var testCase in testData[roundingType])
				{
					rateLine.TL_Rounding = roundingType;
					parameters.ChargeableAmount = new Quantity(testCase.inputValue, "KG");
					var message = $"Rounding Type = {roundingType}, rounds {testCase.inputValue}";
					AssertEquals(message, testCase.expectedValue, rateLine.Calculator.ChargeableAmount(parameters).Amount);
				}
			}
		}

		public void TestDefaultRounding_ProductWarehouse()
		{
			AssertDefaultRounding(RatingConstants.RateCategory.WHS);
		}

		public void TestDefaultRounding_TransitWarehouse()
		{
			AssertDefaultRounding(RatingConstants.RateCategory.TRW);
		}

		public void TestDefaultRounding_TransitWarehouseTransportationUnit()
		{
			AssertDefaultRounding(RatingConstants.RateCategory.TWU);
		}

		void AssertDefaultRounding(string ratingCategory)
		{
			var clientRate = Helper.NewClientRate(Factory.NewWithValidTestData<OrgHeader>());
			var airRateLine = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR).AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);

			AssertEquals("Default from registry - No Rounding", airRateLine.Lookups.Roundings["DEF"].Description);

			var parameters = new AutoRatingCalculatorParametersForTesting(new TestRatingCriteria());
			parameters.ChargeableAmount = new Quantity(125.25m, "KG");

			Factory.Save();
			AssertEquals(125.25m, airRateLine.Calculator.ChargeableAmount(parameters).Amount);

			var whsLine = clientRate.AddRateEntry(ratingCategory).AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);

			if (ratingCategory == RatingConstants.RateCategory.WHS)
			{
				AssertEquals("Default from registry - Round Up to Nearest unit", whsLine.Lookups.Roundings["DEF"].Description);
				Factory.Save();
				AssertEquals(126m, whsLine.Calculator.ChargeableAmount(parameters).Amount);
			}
			else
			{
				AssertEquals("Default from registry - No Rounding", airRateLine.Lookups.Roundings["DEF"].Description);
				Factory.Save();
				AssertEquals(125.25m, airRateLine.Calculator.ChargeableAmount(parameters).Amount);
			}
		}

		public void TestDecimalPlaces()
		{
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			var clientRateEntry = clientRate.AddRateEntry("FCL", "SEA", "AUSYD", "NZAKL");
			var clientRateLine = clientRateEntry.RateLines.AddNew();
			clientRateLine.TL_RateCalculator = UnitCalculator.Code;

			var costing = Factory.New<Costing>();
			costing.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			var costingRateEntry = costing.AddRateEntry("FCL", "SEA", "AUSYD", "NZAKL");
			var costingRateLine = costingRateEntry.RateLines.AddNew();
			costingRateLine.TL_RateCalculator = UnitCalculator.Code;

			var newNumberOfDecimalsAllowed = new SellRatesDecimalsCollection
					{
						new SellRatesDecimals { Code = "FCL", Decimals = "3" }
					};

			using (DataRegistryRating.Instance.SellRatesDecimals.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newNumberOfDecimalsAllowed))
			{
				AssertEquals(3, clientRateLine.Calculator.DecimalPlaces);
				AssertEquals(4, costingRateLine.Calculator.DecimalPlaces);
			}
		}

		#region PacksWeight

		public void TestBreakAmount_PacksWeight()
		{
			var product = Helper.NewOrgSupplierPart(Helper.NewOrgHeader());
			Helper.SetProductWeightAndVolume(product, weight: 2m, weightUQ: Weight.Kilograms, volume: 1m, volumeUQ: Volume.CubicMetres);
			Helper.AddPartUnit(product, package: PkgUnit.Unit, parentPackage: PkgUnit.Pallet, quantityInParent: 2.2m);

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("WHS", "ALL", "AU", "US");
			var rateLine = rateEntry.AddRateLine("DDOC", CombinedCalculator.Code, QuantityUnit.KG);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
			var calculator = rateLine.GetCalculator<CombinedCalculator>();
			calculator["-8"] = (ZDecimal)5m;
			calculator["+8"] = (ZDecimal)10m;
			calculator["+9"] = (ZDecimal)15m;

			Factory.Save();

			var criteria = new TestRatingCriteria();
			criteria.LocalClient = clientRate.Header;
			criteria.ConsumerType = JobInvoicingConsumerTypes.WarehouseInwards;
			criteria.RateTypeToUse = RateType.Warehouse;
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			criteria.RateableMeasures = measures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false,
				optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			measures.AddWarehouseDocketNormalLine((100, null), (0, null), 1, ZGuid.Empty, product.PK, ProductAttributesMeasure.Empty, ZString.Empty, ZString.Empty, (ZString)PkgUnit.Pallet);

			var parameters = new AutoRatingCalculatorParametersWithoutFilter(criteria, new FreightAutoRater(new RatingContext()));
			parameters.AddLineMeasureMatch(MeasureType.Weight, rateLine, 0);

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var result = rateLine.Calculator.ChargeableAmountForBreakSearch_ForTest(parameters);
				AssertEquals
				(
					"GIVEN RateLine UnitFactor is PacksWeight, WHEN ChargeableAmountForBreakSearch THEN should get the product PacksWeight and not rounded",
					4.4m,
					result.Amount
				);
			}

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var result = rateLine.Calculator.ChargeableAmountForBreakSearch_ForTest(parameters);
				AssertEquals
				(
					"GIVEN registry is disable AND RateLine UnitFactor is PacksWeight, WHEN ChargeableAmountForBreakSearch THEN should get the total",
					100m,
					result.Amount
				);
			}
		}

		public void TestBreakAmount_PacksWeight_NonConvertiblePackType()
		{
			var product = Helper.NewOrgSupplierPart(Helper.NewOrgHeader());
			Helper.SetProductWeightAndVolume(product, weight: 2m, weightUQ: Weight.Kilograms, volume: 1m, volumeUQ: Volume.CubicMetres);

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("WHS", "ALL", "AU", "US");
			var rateLine = rateEntry.AddRateLine("DDOC", CombinedCalculator.Code, QuantityUnit.KG);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
			var calculator = rateLine.GetCalculator<CombinedCalculator>();
			calculator["-8"] = (ZDecimal)5m;
			calculator["+8"] = (ZDecimal)10m;

			Factory.Save();

			var criteria = new TestRatingCriteria();
			criteria.LocalClient = clientRate.Header;
			criteria.ConsumerType = JobInvoicingConsumerTypes.WarehouseInwards;
			criteria.RateTypeToUse = RateType.Warehouse;
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			criteria.RateableMeasures = measures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false,
				optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);

			var parameters = new AutoRatingCalculatorParametersWithoutFilter(criteria, new FreightAutoRater(new RatingContext()));
			measures.AddWarehouseDocketNormalLine((100, null), (0, null), 1, ZGuid.Empty, product.PK, ProductAttributesMeasure.Empty, ZString.Empty, ZString.Empty, PkgUnit.Coil);
			parameters.AddLineMeasureMatch(MeasureType.Weight, rateLine, 0);
			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertExceptionThrown<CalculationException>
				(
					"GIVEN non convertable packType THEN should throw exception",
					"Cannot convert pack type 'COI' for packs weight break search.",
					() => rateLine.Calculator.ChargeableAmountForBreakSearch_ForTest(parameters)
				);
			}
		}

		public void TestBreakAmount_PacksWeight_ProductHasZeroWeight()
		{
			var product = Helper.NewOrgSupplierPart(Helper.NewOrgHeader());
			Helper.SetProductWeightAndVolume(product, weight: 0m, weightUQ: Weight.Kilograms, volume: 1m, volumeUQ: Volume.CubicMetres);
			Helper.AddPartUnit(product, package: PkgUnit.Unit, parentPackage: PkgUnit.Pallet, quantityInParent: 2m);

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry("WHS", "ALL", "AU", "US");
			var rateLine = rateEntry.AddRateLine("DDOC", CombinedCalculator.Code, QuantityUnit.KG);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
			var calculator = rateLine.GetCalculator<CombinedCalculator>();
			calculator["-8"] = (ZDecimal)5m;
			calculator["+8"] = (ZDecimal)10m;

			Factory.Save();

			var criteria = new TestRatingCriteria();
			criteria.LocalClient = clientRate.Header;
			criteria.ConsumerType = JobInvoicingConsumerTypes.WarehouseInwards;
			criteria.RateTypeToUse = RateType.Warehouse;
			var measures = new RateableMeasureSet(AdapterType.Shipment);
			criteria.RateableMeasures = measures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false,
				optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			measures.AddWarehouseDocketNormalLine((100, null), (0, null), 1, ZGuid.Empty, product.PK, ProductAttributesMeasure.Empty, ZString.Empty, ZString.Empty, PkgUnit.Pallet);

			var parameters = new AutoRatingCalculatorParametersWithoutFilter(criteria, new FreightAutoRater(new RatingContext()));
			parameters.AddLineMeasureMatch(MeasureType.Weight, rateLine, 0);

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var result = rateLine.Calculator.ChargeableAmountForBreakSearch_ForTest(parameters);
				AssertEquals
				(
					"GIVEN RateLine UnitFactor is PacksWeight, WHEN ChargeableAmountForBreakSearch THEN should get the product PacksWeight and not rounded",
					0m,
					result.Amount
				);
			}
		}

		#endregion

		public void TestBreakAmount_WeightBreakOverrideIsNotSpecified_ShouldRoundChargeableAmount()
		{
			var rate = Factory.New<ClientRate>();
			var entry = rate.AddRateEntry("FCL", "SEA", "AUSYD", "NZAKL");
			var line = entry.RateLines.AddNew();

			line.TL_AC = Env.Registry.FreightChargeCode;
			line.TL_RateCalculator = UnitCalculator.Code;
			line.TL_WeightVolume = "KG";
			line.TL_Rounding = RatingRoundingTypes.UpTo1;

			var parameters = new AutoRatingCalculatorParametersForTesting(new TestRatingCriteria());
			parameters.ChargeableAmount = new Quantity(44.65m, "KG");

			var result = line.Calculator.ChargeableAmountForBreakSearch_ForTest(parameters);
			AssertEquals(45m, result.Amount);
		}

		[ExpectNoExceptions]
		public void TestCalculate_RateLineWithOneAndHigherChargeableLowerRateRuleIsSet_ShouldNotThrowException()
		{
			var rate = Factory.New<ClientRate>();
			var entry = rate.AddRateEntry("FCL", "SEA", "AUSYD", "NZAKL");
			var line = entry.RateLines.AddNew();

			line.TL_RateCalculator = CombinedCalculator.Code;
			line.TL_WeightVolume = "KG";
			line.TL_Rounding = RatingRoundingTypes.UpTo1;
			line.TL_AC = Env.Registry.FreightChargeCode;

			line.RateLineItems.RemoveAndDeleteAll();
			line.Calculator.AddRateLineItem("-", 45m, 0m, 10m);
			((CombinedCalculator)line.Calculator).UseHigherChargeableLowerRateRule = true;

			var parameters = new AutoRatingCalculatorParametersForTesting(new TestRatingCriteria());
			parameters.ChargeableAmount = new Quantity(40.0m, "KG");

			var result = line.Calculator.Calculate(parameters).results.Single();
			AssertEquals("Calculated amount as expected", 10m, result.PaymentBases.Calculate().amount);
		}

		public void TestCalculate_RateLineWithUnitCalculatorAndWeightVolumeIsTE()
		{
			var rate = Factory.New<ClientRate>();
			var entry = rate.AddRateEntry("FCL", "SEA", "AUSYD", "NZAKL");
			entry.RateLines.RemoveAndDeleteAll();

			var line = entry.AddRateLine("FRT", UnitCalculator.Code, "TE");
			line.TL_WeightVolume = "TE";
			line.GetCalculator<UnitCalculator>().PerUnit = 100m;

			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			parameters.ChargeableAmount = new Quantity(4m, "TE");

			var result = line.Calculator.Calculate(parameters).results.Single();
			var autoRateInfo = new AutoRateInfo(result, parameters, Factory);
			AssertEquals("Calculated amount as expected", 400m, autoRateInfo.Amount);
			AssertEquals("FRT: 4 Tea Chest(s) @ USD 100.00/TE", autoRateInfo.SingleLineDescription);

			criteria.PackageInformation = new List<PackageInformation>();
			criteria.PackageInformation.Add(new PackageInformation(ZGuid.NewZGuid().ToGuid(), "Book Box", 3, 2m, Core.Constants.Volume.CubicMetres, ""));
			criteria.PackageInformation.Add(new PackageInformation(ZGuid.NewZGuid().ToGuid(), "Bike", 1, 4m, Core.Constants.Volume.CubicMetres, ""));

			result = line.Calculator.Calculate(parameters).results.Single();
			autoRateInfo = new AutoRateInfo(result, parameters, Factory);
			AssertEquals("Calculated amount as expected", 400m, autoRateInfo.Amount);
			AssertEquals("FRT: 4 Tea Chest(s) - 3 Book Box (15.680003 Tea Chest volume) + 1 Bike (31.360005 Tea Chest volume) @ USD 100.00/TE", autoRateInfo.SingleLineDescription);
		}

		public void TestShowErrorWhileCalculatorNotMapped()
		{
			var costing = Factory.New<Costing>();
			var entry = costing.AddRateEntry("FCL");
			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = CombinedCalculator.Code;

			ErrorReporter.Clear();
			try
			{
				line.Calculator.Decimal1 = 1;
				AssertEquals(ErrorReporter.LastMessageReported, "Property: [Decimal1] should not be accessed from this calculator. Calculator: [Enterprise.Rating.Business.CombinedCalculator]. MapToAttribute:[Bool1;Bool2;Bool3;Bool6;String3]. TL_RateCalculator:[CMB].");// just to check whether error reported
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestCallForPricingExceptionTriggered()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AU", "");
			var line = rateEntry.AddRateLine("ODOC", CombinedCalculator.Code, "KG");
			var items = line.RateLineItems;
			items.RemoveAndDeleteAll();

			var plus25Item = items.AddNew();
			plus25Item.TM_Type = Calculator.Items.Operator.Plus;
			plus25Item.TM_Break = 25m;
			plus25Item.TM_Value = 18m;
			var plus50Item = items.AddNew();
			plus50Item.TM_Type = Calculator.Items.Operator.Plus;
			plus50Item.TM_Break = 50m;
			plus50Item.TM_CallForPricing = true;
			var plus100Item = items.AddNew();
			plus100Item.TM_Type = Calculator.Items.Operator.Plus;
			plus100Item.TM_Break = 100m;

			RateLineItem breakItem = null;
			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			AssertNoExceptionThrown(() => breakItem = (RateLineItem)line.Calculator.GetBreakItem(items.Cast<IRateLineItem>(), parameters, new Quantity(10m, "KG")));
			AssertEquals("Post-condition, not a call for pricing item", false, breakItem.TM_CallForPricing);

			var message = "When calculation returns an item with call for pricing flagged, the exception should be thrown";
			AssertExceptionThrown<AutoRater.CallForPriceException>(message, () => breakItem = (RateLineItem)line.Calculator.GetBreakItem(items.Cast<IRateLineItem>(), parameters, new Quantity(55m, "KG")));

			AssertNoExceptionThrown("only the line with call for pricing should trigger exception", () => breakItem = (RateLineItem)line.Calculator.GetBreakItem(items.Cast<IRateLineItem>(), parameters, new Quantity(200m, "KG")));
		}

		public void TestGetBreakItem()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AU", "");
			var line = rateEntry.AddRateLine("ODOC", CombinedCalculator.Code, QuantityUnit.KG);
			var items = line.RateLineItems;
			items.RemoveAndDeleteAll();

			var calculator = line.Calculator;
			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			AssertNull("No rate line items to check", calculator.GetBreakItem(items.Cast<IRateLineItem>(), parameters, new Quantity(10m, "KG")));

			var minus25Item = calculator.AddRateLineItem(Calculator.Items.Operator.Minus, 25m, 20m, 0m);
			var plus25Item = calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 25m, 18m, 0m);
			var plus50Item = calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 50m, 17.5m, 0m);
			var plus100Item = calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 100m, 16.75m, 0m);

			calculator.UseInclusiveBreaks = false;

			AssertItemsEqual(minus25Item, (RateLineItem)calculator.GetBreakItem(items.Cast<IRateLineItem>(), parameters, new Quantity(0m, "KG")));
			AssertItemsEqual(minus25Item, (RateLineItem)calculator.GetBreakItem(items.Cast<IRateLineItem>(), parameters, new Quantity(24.9m, "KG")));
			AssertItemsEqual(plus25Item, (RateLineItem)calculator.GetBreakItem(items.Cast<IRateLineItem>(), parameters, new Quantity(25.1m, "KG")));
			AssertItemsEqual(plus25Item, (RateLineItem)calculator.GetBreakItem(items.Cast<IRateLineItem>(), parameters, new Quantity(26m, "KG")));
			AssertItemsEqual(plus50Item, (RateLineItem)calculator.GetBreakItem(items.Cast<IRateLineItem>(), parameters, new Quantity(51m, "KG")));
			AssertItemsEqual(plus100Item, (RateLineItem)calculator.GetBreakItem(items.Cast<IRateLineItem>(), parameters, new Quantity(110m, "KG")));

			AssertItemsEqual(plus25Item, (RateLineItem)calculator.GetBreakItem(items.Cast<IRateLineItem>(), parameters, new Quantity(25m, "KG")));
			AssertItemsEqual(plus50Item, (RateLineItem)calculator.GetBreakItem(items.Cast<IRateLineItem>(), parameters, new Quantity(50m, "KG")));
			AssertItemsEqual(plus100Item, (RateLineItem)calculator.GetBreakItem(items.Cast<IRateLineItem>(), parameters, new Quantity(100m, "KG")));

			calculator.UseInclusiveBreaks = true;

			if (calculator.UseInclusiveBreaks)
			{
				AssertItemsEqual(minus25Item, (RateLineItem)calculator.GetBreakItem(items.Cast<IRateLineItem>(), parameters, new Quantity(25m, "KG")));
				AssertItemsEqual(plus25Item, (RateLineItem)calculator.GetBreakItem(items.Cast<IRateLineItem>(), parameters, new Quantity(50m, "KG")));
				AssertItemsEqual(plus50Item, (RateLineItem)calculator.GetBreakItem(items.Cast<IRateLineItem>(), parameters, new Quantity(100m, "KG")));

				items.Remove(minus25Item);
				AssertItemsEqual(plus25Item, (RateLineItem)calculator.GetBreakItem(items.Cast<IRateLineItem>(), parameters, new Quantity(10m, "KG")));
				AssertItemsEqual(plus25Item, (RateLineItem)calculator.GetBreakItem(items.Cast<IRateLineItem>(), parameters, new Quantity(30m, "KG")));
			}
		}

		protected void AssertItemsEqual(RateLineItem item1, RateLineItem item2)
		{
			CombineAssertions(delegate
			{
				AssertEquals(item1.PK, item2.PK);
				AssertEquals(item1.TM_Type, item2.TM_Type);
				AssertEquals(item1.TM_Value, item2.TM_Value);
				AssertEquals(item1.TM_Break, item2.TM_Break);
			});
		}

		TestHelper Helper
		{
			get { return fHelper ?? (fHelper = new TestHelper(Factory)); }
		}

		TestHelper fHelper;
	}

	public abstract class CalculatorTest : TestCaseWithFactory
	{
		public abstract void TestCheckOrCreateItems();

		public abstract void TestMapping();

		public abstract void TestQuotationLines();

		public virtual void TestDocLineAmount() => Assert("Allow calculators that haven't implemented GetDocLineAmount to pass for now", true);

		protected void AssertQuotationLineList(DocLineAmount docLineAmount, string[] expectedQuotationLineList, string message = default)
			=> AssertContainsExactElementsInAnyOrder
			(
				message,
				expectedQuotationLineList,
				docLineAmount.GetQuotationLineList(Line).Select(quotationLine => quotationLine.ToString())
			);

		public void TestQuotationLinesWM()
		{
			DocumentsDataRegistry.Instance.FreightChargesConversionFactorDisplayOption.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "W/M");

			try
			{
				TestQuotationLinesWMCore();
			}
			finally
			{
				DocumentsDataRegistry.Instance.FreightChargesConversionFactorDisplayOption.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "CON");
			}
		}

		protected CalculationResult Calculate(AutoRatingCalculatorParameters parameters)
		{
			return TestCalculator.Calculate(parameters).results.Single();
		}

		protected virtual void TestQuotationLinesWMCore()
		{
			TestQuotationLines();
		}

		public virtual void TestList1()
		{
			AssertEquals("List1 Type", typeof(CodeDescriptionPairList), TestCalculator.List1.GetType());
			AssertEquals("List1 Count", 0, TestCalculator.List1.Count);
		}

		public virtual void TestList2()
		{
			AssertEquals("List2 Type", typeof(CodeDescriptionPairList), TestCalculator.List2.GetType());
			AssertEquals("List2 Count", 0, TestCalculator.List2.Count);
		}

		public virtual void TestList3()
		{
			AssertEquals("List3 Type", typeof(CodeDescriptionPairList), TestCalculator.List3.GetType());
			AssertEquals("List3 Count", 0, TestCalculator.List3.Count);
		}

		public virtual void TestList4()
		{
			AssertEquals("List4 Type", typeof(CodeDescriptionPairList), TestCalculator.List4.GetType());
			AssertEquals("List4 Count", 0, TestCalculator.List4.Count);
		}

		public virtual void TestGetCloneCode()
		{
			AssertGetCloneCode(ZString.Empty);
		}

		protected void AssertGetCloneCode(ZString expected)
		{
			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var line = rateEntry.RateLines.AddNew();

			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			var calculator = (CompanyTariffOrCostBasedCalculator)line.Calculator;

			AssertEquals(expected, TestCalculator.GetCloneCode(calculator));
		}

		public virtual void TestGetCloneLineItems()
		{
			var info = GetType().GetMethod("TestGetCloneCode", BindingFlags.Public | BindingFlags.Instance);
			Assert("TestGetCloneLineItems() should be overriden if you override TestGetCloneCode()", info.DeclaringType == typeof(CalculatorTest));
		}

		public void TestGetQuotationLinesForIterator()
		{
			var testableCalculatorList = new string[] { CartageCalculator.Code, CartageZoneDistanceCalculator.Code, CombinedCalculator.Code, TimeCalculator.Code, ValueRangeCalculator.Code };
			if (!testableCalculatorList.Contains(CalculatorCode))
			{
				Assert("This test applies to BaseCombined and ValueRange calculators", true);
				return;
			}

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AU", "");

			var rateLine1 = rateEntry.AddRateLine("OBILL", CalculatorCode, Core.Constants.PkgUnit.Unit);

			rateLine1.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 3000, 7);
			rateLine1.Calculator.AddRateLineItem(Calculator.Items.Operator.Minus, 800, 4);
			rateLine1.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 500, 3);
			rateLine1.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 2500, 6);
			rateLine1.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 1500, 5);
			rateLine1.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 4000, 8);

			var parameter = Calculator.GetQuotationLinesParam.IncludeNotes;
			parameter |= Calculator.GetQuotationLinesParam.AlternativeFormat;

			var quotationLines = rateLine1.Calculator.GetQuotationLines(parameter, rateEntry);

			AssertEquals("4.00", quotationLines[1].Amount);
			AssertEquals("3.00", quotationLines[2].Amount);
			AssertEquals("7.00", quotationLines[3].Amount);
			AssertEquals("5.00", quotationLines[4].Amount);
			AssertEquals("6.00", quotationLines[5].Amount);
			AssertEquals("8.00", quotationLines[6].Amount);
		}

		#region Costs Comparer Charges Summary

		public virtual void TestGetCostsComparerChargesSummary()
		{
			AssertNull(TestCalculator.GetCostsComparerChargesSummary(new List<RateLine>()));
		}

		protected void AssertChargesSummaryItem(ChargesSummaryItem item, ZString type, ZDecimal value)
		{
			AssertEquals(type, item.Type);
			AssertEquals(value, item.Value);
		}

		protected void AssertChargesSummaryItem(ChargesSummaryItem item, ZString type, ZDecimal @break, ZDecimal value, ZDecimal flatAmount)
		{
			AssertEquals(type, item.Type);
			AssertEquals(@break, item.Break);
			AssertEquals(value, item.Value);
			AssertEquals(flatAmount, item.FlatAmount);
		}

		#endregion

		#region PricePerSingleChargeable

		public virtual void TestPricePerSingleChargeable()
		{
			AssertEquals("Should be empty by default", 0, TestCalculator.PricePerSingleChargeable.Count);
		}

		#endregion

		#region Implementation

		protected JobCharge CreateCostCharge(string chargeCode = null, decimal amount = 0, string currency = "AUD", string container = null, string commodity = null, string containerNumber = null)
		{
			var charge = Factory.NewWithValidTestData<JobCharge>();

			if (string.IsNullOrEmpty(chargeCode))
			{
				charge.JR_AC = ChargeCode.PK;
			}
			else
			{
				charge.JR_AC = Helper.ChargeCodes[chargeCode].PK;
			}

			if (!string.IsNullOrEmpty(container))
			{
				var attrib = charge.JobChargeAttributes.AddNew();
				attrib.EC_Name = JobChargeAttribTypeList.Codes.ContainerCode;
				attrib.EC_Value = container;
			}

			if (!string.IsNullOrEmpty(commodity))
			{
				var attrib = charge.JobChargeAttributes.AddNew();
				attrib.EC_Name = JobChargeAttribTypeList.Codes.Commodity;
				attrib.EC_Value = commodity;
			}

			if (!string.IsNullOrEmpty(containerNumber))
			{
				var attrib = charge.JobChargeAttributes.AddNew();
				attrib.EC_Name = JobChargeAttribTypeList.Codes.ContainerNumber;
				attrib.EC_Value = containerNumber;
			}

			charge.JR_OSCostAmt = amount;
			charge.JR_RX_NKCostCurrency = currency;
			return charge;
		}

		protected CalculationResult AssertCalculation(RatingCriteria criteria, ZDecimal expectedAmount, ZString expectedDescription, string message = default)
		{
			return AssertCalculation(new AutoRatingCalculatorParametersForTesting(criteria), expectedAmount, expectedDescription, message: message);
		}

		protected virtual CalculationResult AssertCalculation(AutoRatingCalculatorParameters parameters, ZDecimal expectedAmount, ZString expectedDescription, string message = default)
		{
			var (results, error) = TestCalculator.Calculate(parameters);
			AssertGreaterThan(message, results.Count(), 0);
			AssertNullOrEmpty(message, error);

			var result = results.Single();
			var autoRateInfo = new AutoRateInfo(result, parameters, Factory);
			var calculatorType = CalculatorType.ToString();

			CombineAssertions(message, delegate
			{
				AssertEquals(calculatorType + " description:", expectedDescription, result.Description);
				AssertEquals(calculatorType + " rounded amount:", expectedAmount, Utilities.Round(autoRateInfo.Amount, result.Currency?.Decimals ?? 2));
			});

			return result;
		}

		protected void AssertCalculation(AutoRatingCalculatorParameters parameters, ZString expectedError)
		{
			var (results, error) = TestCalculator.Calculate(parameters);
			AssertEquals(0, results.Count());
			AssertEquals("The error message should match the expected value", expectedError, error);
		}

		protected void ClearUsedByRateLines(IEnumerable<AutoRateInfo> collection)
		{
			foreach (var info in collection)
			{
				info.PercentageLinesApplied.Clear();
			}
		}

		protected void TestMapping(string itemType, string mapTo)
		{
			InitialiseTestCalculator();
			if (mapTo.StartsWith("Decimal"))
			{
				Line.RateLineItems.FindByTM_Type(itemType).TM_RelevantValue = 12.45M;
				AssertEquals(mapTo, 12.45M, CalculatorType.GetProperty(mapTo).GetValue(TestCalculator, null));

				CalculatorType.GetProperty(mapTo).SetValue(TestCalculator, (ZDecimal)67.89M, null);
				AssertEquals(itemType, 67.89M, Line.RateLineItems.FindByTM_Type(itemType).TM_RelevantValue);
			}
			else if (mapTo.StartsWith("String"))
			{
				Line.RateLineItems.FindByTM_Type(itemType).TM_Text = "ABC";
				AssertEquals(mapTo, "ABC", CalculatorType.GetProperty(mapTo).GetValue(TestCalculator, null));

				CalculatorType.GetProperty(mapTo).SetValue(TestCalculator, (ZString)"DEF", null);
				AssertEquals(itemType, "DEF", Line.RateLineItems.FindByTM_Type(itemType).TM_Text);
			}
			else if (mapTo.StartsWith("Bool"))
			{
				Line.RateLineItems.FindByTM_Type(itemType).TM_Text = "Y";
				AssertEquals(mapTo, ZBool.True, CalculatorType.GetProperty(mapTo).GetValue(TestCalculator, null));

				CalculatorType.GetProperty(mapTo).SetValue(TestCalculator, ZBool.False, null);
				AssertEquals(itemType, "N", Line.RateLineItems.FindByTM_Type(itemType).TM_Text);
			}
			else if (mapTo.StartsWith("Int"))
			{
				Line.RateLineItems.FindByTM_Type(itemType).TM_RelevantValue = 12;
				AssertEquals(mapTo, 12, CalculatorType.GetProperty(mapTo).GetValue(TestCalculator, null));

				CalculatorType.GetProperty(mapTo).SetValue(TestCalculator, (ZInt)67, null);
				AssertEquals(itemType, 67m, Line.RateLineItems.FindByTM_Type(itemType).TM_RelevantValue);
			}
			else
			{
				Fail("Incorrect Mapping");
			}
		}

		protected abstract Type CalculatorType { get; }
		protected abstract string CalculatorCode { get; }

		protected RateEntry Entry
		{
			get
			{
				if (fEntry == null)
				{
					var rate = Factory.New<ClientRate>();
					rate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
					fEntry = rate.AddRateEntry("ORG");
				}

				return fEntry;
			}
		}

		RateEntry fEntry;

		protected RateLine Line
		{
			get
			{
				if (fLine == null)
				{
					fLine = Entry.RateLines.AddNew();
				}

				return fLine;
			}
		}

		RateLine fLine;

		protected void InitialiseTestCalculator()
		{
			if (TestCalculator == null)
			{
				Fail();
			}
		}

		protected Calculator TestCalculator
		{
			get
			{
				if (fTestCalculator == null)
				{
					Line.TL_AC = ChargeCode.PK;
					Line.TL_RateDesc = "Test Rate";

					fTestCalculator = Line.Calculator;
					AssertEquals(CalculatorType, fTestCalculator.GetType());
				}
				return fTestCalculator;
			}
		}

		Calculator fTestCalculator;

		protected AccChargeCode ChargeCode
		{
			get { return fChargeCode ?? (fChargeCode = Helper.ChargeCodes.New("CCC", "Test Charge", CalculatorCode, "ORG")); }
		}

		AccChargeCode fChargeCode;

		protected TestRatingCriteria Criteria
		{
			get { return fCriteria ?? (fCriteria = new TestRatingCriteria()); }
			set { fCriteria = value; }
		}

		TestRatingCriteria fCriteria;

		protected OrgHeader NewClient
		{
			get { return fNewClient ?? (fNewClient = Helper.NewOrgHeader(1)); }
		}

		OrgHeader fNewClient;

		protected TestHelper Helper
		{
			get { return fHelper ?? (fHelper = new TestHelper(Factory)); }
		}

		TestHelper fHelper;

		#endregion
	}

	#region Test Extension Methods

	public static class CalculatorTestExtensionMethods
	{
		public static bool IsTestCalculator(this Type type)
		{
			return type.IsSubclassOf(typeof(Calculator)) && type.GetCustomAttributes(typeof(TestOnlyCalculatorAttribute), true).Any();
		}

		public static RateLineItem AddRateLineItem(this Calculator calculator, ZString itemType, ZDecimal breakAmount, ZDecimal relevantValue, Action<RateLineItem> firstSetters = null, decimal? breakHour = null, decimal? breakHourRate = null)
		{
			var rateLineItem = calculator.RateLineBizO.RateLineItems.AddNew();
			rateLineItem.TM_Type = itemType;
			rateLineItem.TM_Break = breakAmount;
			rateLineItem.TM_RelevantValue = relevantValue;

			if (breakHour.HasValue)
			{
				rateLineItem.TM_BreakHour = breakHour.Value;
			}

			if (breakHourRate.HasValue)
			{
				rateLineItem.TM_BreakHourRate = breakHourRate.Value;
			}

			firstSetters?.Invoke(rateLineItem);

			return rateLineItem;
		}

		public static RateLineItem AddRateLineItem(this Calculator calculator, ZString itemType, ZDecimal breakAmount, ZDecimal relevantValue, ZDecimal flatAmount)
		{
			return AddRateLineItem(calculator, itemType, breakAmount, relevantValue, x => x.TM_FlatAmount = flatAmount);
		}

		public static RateLineItem AddRateLineItem(this Calculator calculator, ZString itemType, ZDecimal breakAmount, ZDecimal relevantValue, ZString breakUnit)
		{
			return AddRateLineItem(calculator, itemType, breakAmount, relevantValue, x => x.TM_BreakWeightVolume = breakUnit);
		}

		public static RateLineItem AddRateLineItemWithZone(this Calculator calculator, ZString itemType, ZDecimal breakAmount, ZDecimal relevantValue, ZGuid transportZonePK, ZDecimal flat = default, ZString unit = default)
		{
			var rateLineItem = calculator.RateLineBizO.RateLineItems.AddNew();
			rateLineItem.TM_TZ_DomesticZone = transportZonePK;
			rateLineItem.TM_Type = itemType;
			rateLineItem.TM_Break = breakAmount;
			rateLineItem.TM_RelevantValue = relevantValue;
			rateLineItem.TM_FlatAmount = flat;
			rateLineItem.TM_BreakWeightVolume = unit;

			return rateLineItem;
		}

		public static RateLineItem AddRateLineItemWithZone(this Calculator calculator, ZString itemType, ZDecimal breakAmount, ZDecimal relevantValue, ZString aciZoneName)
		{
			return AddRateLineItem(calculator, itemType, breakAmount, relevantValue, x => x.TM_F1Zone = aciZoneName);
		}

		public static QuotationLineList GetQuotationLines(this Calculator calculator, RateEntry parentEntry)
		{
			return calculator.GetQuotationLines(Calculator.GetQuotationLinesParam.None, parentEntry);
		}

		public static QuotationLineList GetQuotationLines(this CompanyTariffOrCostBasedCalculator calculator, BusinessObjectFactory factory)
		{
			return calculator.GetQuotationLines();
		}
	}

	public static class CalculationStepTestExtensions
	{
		public static void AssertPerUnit(this CalculationStep step, ZDecimal unitCount, ZDecimal unitPrice, ZDecimal flatAmount)
		{
			Assertion.AssertEquals(true, step.IsPerUnit);
			Assertion.AssertEquals(unitCount, step.UnitCount);
			Assertion.AssertEquals(unitPrice, step.UnitPrice);
			Assertion.AssertEquals(flatAmount, step.Flat);
		}
	}

	#endregion

	sealed class CalculatorConcreteTest : TestCaseWithFactory
	{
		public void TestDocLineAmount()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var rateEntry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);
			var rateLine10 = rateEntry1.AddFlatRateLine("FRT", 10, "AUD");
			var rateLine11 = rateEntry1.AddFlatRateLine("BAF", 11, "AUD");
			var rateLine12 = rateEntry1.AddFlatRateLine("CAF", 12, "USD");

			var rateEntry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG);
			var rateLine20 = rateEntry2.AddFlatRateLine("ODOC", 20, "AUD");
			var rateLine21 = rateEntry2.AddFlatRateLine("OFUMI", 21, "AUD");
			var rateLine22 = rateEntry2.AddFlatRateLine("OCARD", 22, "USD");

			var docLineAmount = rateLine10.Calculator.GetDocLineAmount()
				+ rateLine11.Calculator.GetDocLineAmount()
				+ rateLine12.Calculator.GetDocLineAmount()
				+ rateLine20.Calculator.GetDocLineAmount()
				+ rateLine21.Calculator.GetDocLineAmount()
				+ rateLine22.Calculator.GetDocLineAmount();

			AssertContainsExactElementsInAnyOrder
		   (
				"QuotationLineList",
				new[] { "|AUD|62.00|", "|USD|34.00|" },
				docLineAmount.GetQuotationLineList(Line).Select(quotationLine => quotationLine.ToString())
			);
		}

		public void TestGetBreakItemByAmount_ThrowsIfDuplicateBreaksWithDifferentValues()
		{
			var line = Helper.NewClientRate(null).AddRateEntry("ORG").AddRateLine("ODOC", CombinedCalculator.Code, QuantityUnit.KG);
			var item1 = line.RateLineItems.AddNew();
			var item2 = line.RateLineItems.AddNew();
			var item3 = line.RateLineItems.AddNew();
			var item4 = line.RateLineItems.AddNew();
			var item5 = line.RateLineItems.AddNew();
			item1.TM_Type = Calculator.Items.Operator.Plus;
			item1.TM_Break = 100;
			item1.TM_Value = 9;
			item2.TM_Type = Calculator.Items.Operator.Plus;
			item2.TM_Break = 100;
			item2.TM_Value = 8;
			item3.TM_Type = Calculator.Items.Operator.Plus;
			item3.TM_Break = 200;
			item3.TM_Value = 5;
			item4.TM_Type = Calculator.Items.Operator.Plus;
			item4.TM_Break = 300;
			item4.TM_Value = 3;
			item5.TM_Type = Calculator.Items.Operator.Plus;
			item5.TM_Break = 300;
			item5.TM_Value = 2;
			AssertExceptionThrown<CalculationException>("duplicate lowest breaks with different values cannot be calculated",
				"calculator has duplicate breaks of 100 with different values", () =>
				Calculator.GetBreakItemByAmount(new Quantity(110, QuantityUnit.KG), line.RateLineItems.Cast<RateLineItem>(), true));

			AssertExceptionThrown<CalculationException>("duplicate lowest breaks at the start with different values cannot be calculated",
				"calculator has duplicate breaks of 100 with different values", () =>
				Calculator.GetBreakItemByAmount(new Quantity(0, QuantityUnit.KG), line.RateLineItems.Cast<RateLineItem>(), true));

			AssertExceptionThrown<CalculationException>("duplicate highest breaks with different values cannot be calculated",
				"calculator has duplicate breaks of 300 with different values", () =>
				Calculator.GetBreakItemByAmount(new Quantity(350, QuantityUnit.KG), line.RateLineItems.Cast<RateLineItem>(), true));

			AssertEquals("no exception as long as matching the break is unique", item3, Calculator.GetBreakItemByAmount(new Quantity(250, QuantityUnit.KG), line.RateLineItems.Cast<RateLineItem>(), true));

			item2.TM_Value = item1.TM_Value;
			AssertEquals("duplicate breaks with the same value are OK - last one is returned", item2, Calculator.GetBreakItemByAmount(new Quantity(110, QuantityUnit.KG), line.RateLineItems.Cast<RateLineItem>(), true));
		}

		public void TestIEquatable()
		{
			var line1 = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("ORG").AddRateLine("ODOC", CombinedCalculator.Code, QuantityUnit.KG);
			var line2 = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("ORG").AddRateLine("ODOC", CombinedCalculator.Code, QuantityUnit.M3);
			line1.TL_RateCalculator = CombinedCalculator.Code;
			line2.TL_RateCalculator = CombinedCalculator.Code;
			line1.TL_WeightVolume = "KG";
			line2.TL_WeightVolume = "M3";
			Assert(!line1.Calculator.Equals(line2.Calculator));
			Assert(!line2.Calculator.Equals(line1.Calculator));

			line2.TL_WeightVolume = "KG";
			Assert(line1.Calculator.Equals(line2.Calculator));
			Assert(line2.Calculator.Equals(line1.Calculator));

			line1.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)100m;
			line1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)10m;
			line2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)10m;
			Assert(!line1.Calculator.Equals(line2.Calculator));
			Assert(!line2.Calculator.Equals(line1.Calculator));

			line2.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)100m;
			Assert(line1.Calculator.Equals(line2.Calculator));
			Assert(line2.Calculator.Equals(line1.Calculator));
		}

		public void TestAutoRateDescriptionMacroExpansion()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = clientRate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = UnitCalculator.Code;
			line.TL_WeightVolume = "KG";
			line.TL_RateDesc = "A {B} C {Dee} }{X";

			var criteria = new TestRatingCriteria();

			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			AssertEquals("A {B} C {Dee} }{X", line.Calculator.AutoRateDescription(parameters));

			var handler = new Mock<Converter<string, string>>();

			criteria.HandleExpandMacro = handler.Object;

			handler.Setup(m => m("b")).Returns((Converter<string, string>)null);
			handler.Setup(m => m("dee")).Returns("D");

			criteria.HandleExpandMacro = handler.Object;

			AssertEquals("A {B} C D }{X", line.Calculator.AutoRateDescription(parameters));
		}

		public void TestConditions()
		{
			var chargeCode1 = Factory.New<AccChargeCode>();
			chargeCode1.AC_Code = "XYZ";
			chargeCode1.AC_Desc = "Charge Code 1";

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);

			var line = entry.RateLines.AddNew();
			line.TL_AC = chargeCode1.PK;
			line.TL_RateCalculator = UnitCalculator.Code;
			((UnitCalculator)line.Calculator).PerUnit = 5m;
			line.TL_Condition = RateLineConditions.ForwardingAndBrokerage;
			line.ChargeInformationNoteText = "Please note conditions may apply here";

			var list = line.Calculator.GetQuotationLines(entry);
			AssertContains("*conditions apply", list[1].ToString());

			line.TL_Condition = RateLineConditions.OwnCFS;
			list = line.Calculator.GetQuotationLines(entry);
			AssertContains("*conditions apply", list[1].ToString());

			list = line.Calculator.GetQuotationLines(Calculator.GetQuotationLinesParam.IncludeNotes, entry);
			AssertNotContains("*conditions apply", list[1].ToString());
			AssertContains("Please note conditions may apply here", list[1].ToString());
		}

		public void TestEquipmentType()
		{
			var chargeCode1 = Factory.New<AccChargeCode>();
			chargeCode1.AC_Code = "XYZ";
			chargeCode1.AC_Desc = "Charge Code 1";

			var chargeCode2 = Factory.New<AccChargeCode>();
			chargeCode2.AC_Code = "ABC";
			chargeCode2.AC_Desc = "Charge Code 2";

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);

			var line1 = entry.RateLines.AddNew();
			line1.TL_AC = chargeCode1.PK;
			line1.TL_RateCalculator = CartageCalculator.Code;
			line1.Calculator.EquipmentType = Constants.LCLAIREquipmentNeeded.Premise;

			var line2 = entry.RateLines.AddNew();
			line2.TL_AC = chargeCode2.PK;
			line2.TL_RateCalculator = FlatCalculator.Code;

			var list1 = line1.Calculator.GetQuotationLines(entry);
			AssertNotContains("Only a single cartage charge on this line, so no equipment included", "Premise Supplies Lift", list1[0].ToString());

			line2.TL_RateCalculator = CartageCalculator.Code;
			line2.Calculator.EquipmentType = Constants.LCLAIREquipmentNeeded.HandHaulier;

			list1 = line1.Calculator.GetQuotationLines(entry);
			AssertContains("Multiple cartage charges - so equpiment included", "Premise Supplies Lift", list1[0].ToString());
		}

		public void TestPart()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());

			var part = Helper.NewOrgSupplierPart(rate.Header);

			var entry = rate.AddRateEntry("WHS", "ALL", "", "");

			var line1 = entry.AddRateLine("OCART", UnitCalculator.Code, QuantityUnit.M3);
			line1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1m;
			line1.TL_WeightVolume = "M3";

			var line2 = entry.AddRateLine("OCART", UnitCalculator.Code, QuantityUnit.M3);
			line2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)3m;
			line2.TL_WeightVolume = "M3";
			line2.TL_OP_ProductNumber = part.PK;

			Factory.Save();

			const string expectedWithout = "Pick Up Cartage *|AUD|1.00|per M3\r\n";
			const string expectedWith = @"Pick Up Cartage * 
-  for PROD1 (###1)|AUD|3.00|per M3";

			AssertMultilineASCIIEquals("without part", expectedWithout, Render(line1.Calculator.GetQuotationLines(entry)));
			AssertMultilineASCIIEquals("with part", expectedWith, Render(line2.Calculator.GetQuotationLines(entry)));
		}

		// TODO remove this test in 2 years time with WI00617632
		public void TestMinimumAttribute()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "XYZ";
			chargeCode.AC_Desc = "Charge Code 1";

			var rate = Factory.New<ClientRate>();
			var entry = rate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_AC = chargeCode.PK;
			line.TL_RateCalculator = CalculatorForTest.Code;
			var calc = (CalculatorForTest)line.Calculator;
			var calcParams = new AutoRatingCalculatorParametersForTesting(new TestRatingCriteria());

			calc.ExpectedAmount = 1.0M;
			var result = calc.Calculate(calcParams).results.Single();
			AssertCollectionContains(
				JobChargeAttribTypeList.Codes.MinimumRateUsed,
				result.Attributes.Attributes.Select(a => a.Code)
			);

			calc.ExpectedAmount = 5.0M;
			result = calc.Calculate(calcParams).results.Single();
			AssertCollectionNotContains(
				JobChargeAttribTypeList.Codes.MinimumRateUsed,
				result.Attributes.Attributes.Select(a => a.Code)
			);
		}

		public void TestCalculator_EntryHasFMCTariffID_FMCTariffIDGetsPopulatedAsChargeAttribute()
		{
			// Setup
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "XYZ";
			chargeCode.AC_Desc = "Charge Code 1";

			var rate = Factory.New<CompanyTariff>();
			var entry = rate.AddRateEntry("ORG");

			var line = entry.RateLines.AddNew();
			line.TL_AC = chargeCode.PK;
			line.TL_RateCalculator = FlatCalculator.Code;
			line.InitializeCalculator();
			((FlatCalculator)line.Calculator).BaseRate = 100;
			var calc = (FlatCalculator)line.Calculator;
			var calcParams = new AutoRatingCalculatorParametersForTesting(new TestRatingCriteria());

			entry.TI_FMCTariffID = "1111";
			var result = calc.Calculate(calcParams).results.Single();
			AssertCollectionContains(
				"Expected FMCTariffID attribute when set on entry",
				new RateAttribute(JobChargeAttribTypeList.Codes.FMCTariffID, "1111"),
				result.Attributes.Attributes
			);

			entry.TI_FMCTariffID = "";
			result = calc.Calculate(calcParams).results.Single();
			AssertCollectionNotContains(
				"FMCTariffID attribute should not exist when not set on entry",
				new RateAttribute(JobChargeAttribTypeList.Codes.FMCTariffID, null),
				result.Attributes.Attributes
			);
		}

		public void TestCalculate_EntryHasContainer_ContainerGetsPopulatedAsChargeAttribute()
		{
			Entry.TI_RC = Helper.Containers["20GP"].PK;

			var calc = (CalculatorForTest)Line.Calculator;
			var calcParams = new AutoRatingCalculatorParametersForTesting(new TestRatingCriteria());

			calc.ExpectedAmount = 1.0M;
			var result = calc.Calculate(calcParams).results.Single();

			var expectedAttributes = new[]
			{
				new RateAttribute(JobChargeAttribTypeList.Codes.ContainerCode, "20GP")
			};

			AssertCollectionContains(
				"Expected container code '20GP' to be added as a charge attribute.",
				new RateAttribute(JobChargeAttribTypeList.Codes.ContainerCode, "20GP"),
				result.Attributes.Attributes
			);
		}

		public void TestCalculate_EntryHasCommodity_CommodityGetsPopulatedAsChargeAttribute()
		{
			Entry.TI_RH_NKCommodityCode = "HAZ";

			var calc = (CalculatorForTest)Line.Calculator;
			var calcParams = new AutoRatingCalculatorParametersForTesting(new TestRatingCriteria());

			calc.ExpectedAmount = 1.0M;
			var result = calc.Calculate(calcParams).results.Single();

			AssertCollectionContains(
				new RateAttribute(JobChargeAttribTypeList.Codes.Commodity, "HAZ"),
				result.Attributes.Attributes
			);
		}

		public void TestCalculate_LineHasNoCurrency_FailCalculation()
		{
			Line.TL_RX_NKCurrency = ZString.Empty;

			var calc = (CalculatorForTest)Line.Calculator;
			var calcParams = new AutoRatingCalculatorParametersForTesting(new TestRatingCriteria());

			var (result, error) = calc.Calculate(calcParams);

			AssertEquals("Expected calculation result to be empty when the rate has no currency.", 0, result.Count());
			AssertEquals("Expected error message for missing currency in rate.", "the rate has no currency", error);
		}

		public void TestCalculate_Autorating_CalculatorHasRestrictedItem_ShouldThrowException()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR);
			var rateLine = rateEntry.AddRateLine(Helper.ChargeCodes["FRT"], CombinedCalculator.Code, QuantityUnit.KG, "AUD");
			var cmbCalculator = rateLine.GetCalculator<CombinedCalculator>();
			cmbCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 5m, 100);
			var restrictedItem = cmbCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 5m, 0);
			restrictedItem.TM_CallForPricing = true;
			restrictedItem.TM_Text = "Call for Price";

			var criteria = new TestRatingCriteria();
			criteria.IsManualCostSelectMode = false;
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			parameters.ChargeableAmount = new Quantity(10, QuantityUnit.KG);

			AssertExceptionThrown(
				"Expected exception for restricted calculation with proper message",
				typeof(AutoRater.CallForPriceException),
				() => rateLine.Calculator.Calculate(parameters)
			);
		}

		public void TestCalculate_ManualSelect_CalculatorHasRestrictedItem_ShouldReturnEmptyResultsWithError()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR);
			var rateLine = rateEntry.AddRateLine(Helper.ChargeCodes["FRT"], CombinedCalculator.Code, QuantityUnit.KG, "AUD");
			var cmbCalculator = rateLine.GetCalculator<CombinedCalculator>();
			cmbCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 5m, 100);
			var restrictedItem = cmbCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 5m, 0);
			restrictedItem.TM_CallForPricing = true;
			restrictedItem.TM_Text = "Call for Price";

			var criteria = new TestRatingCriteria();
			criteria.IsManualCostSelectMode = true;
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			parameters.ChargeableAmount = new Quantity(10, QuantityUnit.KG);

			var (results, error) = rateLine.Calculator.Calculate(parameters);

			AssertEquals("Results should be empty when a restricted item requires manual cost selection.", 0, results.Count());
			AssertEquals("Error message should match the expected text.", "Greater than 5 Kilogram(s) Call for Price", error);
		}

		public void TestFreightChargesConversionFactorDisplayOption_ProductWarehouse()
		{
			AssertFreightChargesConversionFactorDisplayOption(RatingConstants.RateCategory.WHS);
		}

		public void TestFreightChargesConversionFactorDisplayOption_TransitWarehouse()
		{
			AssertFreightChargesConversionFactorDisplayOption(RatingConstants.RateCategory.TRW);
		}

		public void TestFreightChargesConversionFactorDisplayOption_TransitWarehouseTransportationUnit()
		{
			AssertFreightChargesConversionFactorDisplayOption(RatingConstants.RateCategory.TWU);
		}

		void AssertFreightChargesConversionFactorDisplayOption(string ratingCategory)
		{
			var chargeCode1 = Factory.New<AccChargeCode>();
			chargeCode1.AC_Code = "XYZ";
			chargeCode1.AC_Desc = "Charge Code 1";
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = clientRate.AddRateEntry(ratingCategory);
			entry.TI_RX_NKCurrency = "";

			var line = entry.RateLines.AddNew();
			line.TL_AC = chargeCode1.PK;
			line.TL_RateCalculator = UnitCalculator.Code;
			line.TL_WeightVolume = "KG";
			line.RateLineItems[0].TM_RelevantValue = 50m;

			var list1 = line.Calculator.GetQuotationLines(entry);
			AssertEquals("Charge Code 1||50.00|per KG", list1[0].ToString());

			DocumentsDataRegistry.Instance.FreightChargesConversionFactorDisplayOption.SetValue(Enterprise.ZArchitecture.Environment.EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "W/M");

			try
			{
				var list2 = line.Calculator.GetQuotationLines(entry);
				AssertEquals("Charge Code 1||50.00|per W/M", list2[0].ToString());
			}
			finally
			{
				DocumentsDataRegistry.Instance.FreightChargesConversionFactorDisplayOption.SetValue(Enterprise.ZArchitecture.Environment.EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CON");
			}
		}

		public void TestContractNumberLines()
		{
			const string existingLine = "International Freight|||\r\n";
			const string contractLine = "Contract Number: Blaticus|||\r\n";

			var tests = new[]
			{
				new
				{
					Message = "all good",
					SetContractNumber = true,
					OtherLines = true,
					Flags = Calculator.GetQuotationLinesParam.IncludeContractNumbers,
					Expected = existingLine + contractLine,
				},
				new
				{
					Message = "contract numbers not requested",
					SetContractNumber = true,
					OtherLines = true,
					Flags = Calculator.GetQuotationLinesParam.None,
					Expected = existingLine,
				},
				new
				{
					Message = "no 'other' quotation lines",
					SetContractNumber = true,
					OtherLines = false,
					Flags = Calculator.GetQuotationLinesParam.IncludeContractNumbers,
					Expected = "",
				},
				new
				{
					Message = "no contract number to show",
					SetContractNumber = false,
					OtherLines = true,
					Flags = Calculator.GetQuotationLinesParam.IncludeContractNumbers,
					Expected = existingLine,
				},
			};

			var getQuotationLines = new Mock<Converter<GetQuotationLinesParam, QuotationLineList>>();

			var tariff = Factory.New<CompanyTariff>();
			var entry = tariff.AddRateEntry("FCL");
			var line = entry.AddRateLine("FRT", CalculatorForTest.Code, QuantityUnit.KG);
			line.TL_RateCalculator = CalculatorForTest.Code;
			var calculator = (CalculatorForTest)line.Calculator;
			calculator.GetQuotationLinesInternalOverride = getQuotationLines.Object;

			foreach (var test in tests)
			{
				entry.TI_ContractNumber = test.SetContractNumber ? "Blaticus" : string.Empty;

				var list = new QuotationLineList();

				if (test.OtherLines)
				{
					list.Add(QuotationLine.Header(line, Calculator.RateDescriptionFlags(Calculator.GetQuotationLinesParam.None)));
				}

				getQuotationLines.Setup((m => m(test.Flags))).Returns(list);

				AssertMultilineASCIIEquals(test.Message, test.Expected, Render(calculator.GetQuotationLines(test.Flags, entry)));
			}
		}

		public void TestNoteText()
		{
			const string existingLine = "International Freight|||\r\n";
			const string noteLine = "Note: Blaticus|||\r\n";

			var tests = new[]
			{
				new
				{
					Message = "all good",
					SetNoteText = true,
					OtherLines = true,
					Flags = Calculator.GetQuotationLinesParam.IncludeNotes,
					Expected = existingLine + noteLine,
				},
				new
				{
					Message = "note text not requested",
					SetNoteText = true,
					OtherLines = true,
					Flags = Calculator.GetQuotationLinesParam.None,
					Expected = existingLine,
				},
				new
				{
					Message = "no 'other' quotation lines",
					SetNoteText = true,
					OtherLines = false,
					Flags = Calculator.GetQuotationLinesParam.IncludeNotes,
					Expected = "",
				},
				new
				{
					Message = "no note text to show",
					SetNoteText = false,
					OtherLines = true,
					Flags = Calculator.GetQuotationLinesParam.IncludeNotes,
					Expected = existingLine,
				},
			};

			var getQuotationLines = new Mock<Converter<GetQuotationLinesParam, QuotationLineList>>();

			var tariff = Factory.New<CompanyTariff>();
			var entry = tariff.AddRateEntry("FCL");
			var line = entry.AddRateLine("FRT", CalculatorForTest.Code, QuantityUnit.CN);
			var calculator = (CalculatorForTest)line.Calculator;
			calculator.GetQuotationLinesInternalOverride = getQuotationLines.Object;

			foreach (var test in tests)
			{
				line.ChargeInformationNoteText = test.SetNoteText ? "Blaticus" : string.Empty;

				var list = new QuotationLineList();

				if (test.OtherLines)
				{
					list.Add(QuotationLine.Header(line, RateDescriptionFlags(GetQuotationLinesParam.None)));
				}

				getQuotationLines.Setup(m => m(test.Flags)).Returns(list);
				AssertMultilineASCIIEquals(test.Message, test.Expected, Render(calculator.GetQuotationLines(test.Flags, entry)));
			}
		}

		public void TestRateDescription()
		{
			var expectedRateLine = QuotationLineType.Mandatory | QuotationLineType.UseRateLineDescription;
			var expectedChargeCode = QuotationLineType.Mandatory | QuotationLineType.UseChargeDescription;

			AssertEquals(expectedRateLine, RateDescriptionFlags(GetQuotationLinesParam.None | GetQuotationLinesParam.ShowEquipmentType));
			AssertEquals(expectedChargeCode, RateDescriptionFlags(GetQuotationLinesParam.UseChargeDescription | GetQuotationLinesParam.ShowEquipmentType));
		}

		public void TestCalculationLog()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "", "");
			entry.TI_RH_NKCommodityCode = "COM";

			var container = Factory.New<RefContainer>();
			container.RC_Code = "CNT";
			entry.TI_RC = container.PK;

			var line = entry.RateLines.AddNew();
			line.TL_AC = Env.Registry.FreightChargeCode;

			var ratingCriteria = new TestRatingCriteria();
			ratingCriteria.RateableMeasures.SetQuantity(MeasureType.Weight, 100m, Constants.Weight.Pounds);

			var calcParams = new AutoRatingCalculatorParametersForTesting(ratingCriteria);
			calcParams.ChargeableAmount = new Quantity(200m, Constants.Weight.Pounds);

			line.TL_RateCalculator = CalculatorForCalculationLogTest.Code;
			line.TL_WeightVolume = "LB";
			var calculator = (CalculatorForCalculationLogTest)line.Calculator;
			var result = calculator.Calculate(calcParams).results.Single();

			var calculationLog = result.FreightChargeCodeCalculationLog;
			AssertEquals("Weight unit comes from calculator parameters when not specified on rate line", "LB", calculationLog.Unit);
			AssertEquals("Weight comes from calculator parameters", 100m, calculationLog.Weight);
			AssertEquals("Chargeable comes from calculator parameters", 200m, calculationLog.Chargeable);
			AssertEquals("ChargeableUnit comes from calculator parameters", Constants.Weight.Pounds, calculationLog.ChargeableUnit);

			line.TL_RX_NKCurrency = "AUD";
			line.TL_WeightVolume = "KG";

			result = calculator.Calculate(calcParams).results.Single();

			calculationLog = result.FreightChargeCodeCalculationLog;
			AssertEquals(false, calculationLog.IsCosting);
			AssertEquals(CalculatorForCalculationLogTest.Code, calculationLog.CalculatorCode);
			AssertEquals("FRT", calculationLog.ChargeCode);
			AssertEquals("AUD", calculationLog.Currency);
			AssertEquals("KG", calculationLog.Unit);
			AssertEquals("Weight from calculator parameters converted to rate line's weight unit", 45.359237m, calculationLog.Weight);

			AssertEquals("Chargeable comes from calculator parameters converted", 90.718474m, calculationLog.Chargeable);
			AssertEquals("ChargeableUnit comes from calculator parameters", Constants.Weight.Kilograms, calculationLog.ChargeableUnit);

			AssertEquals("COM", calculationLog.CommodityCode);
			AssertEquals(Core.Constants.RateMode.LCL, calculationLog.RateMode);
			AssertEquals("CNT", calculationLog.ContainerCode);

			AssertEquals(0m, calculationLog.Minimum);
			AssertEquals(128m, calculationLog.Maximum);     //comes from CalculatorForCalculationLogTest
			AssertEquals(16m, calculationLog.BaseRate);

			AssertEquals(1, calculationLog.Steps.Count);

			var calculationStep = calculationLog.Steps[0];
			AssertEquals(true, calculationStep.IsPerUnit);
			AssertEquals(90.718474m, calculationStep.UnitCount);
			AssertEquals(4m, calculationStep.UnitPrice);
			AssertEquals(8m, calculationStep.Flat);
			AssertEquals(370.873896m, calculationStep.Result);

			calculator.ForceCalculationResult = 10m;
			result = calculator.Calculate(calcParams).results.Single();
			calculationLog = result.FreightChargeCodeCalculationLog;
			AssertEquals(32m, calculationLog.Minimum);
			AssertEquals(0m, calculationLog.Maximum);

			calculator.ForceCalculationResult = 200m;
			result = calculator.Calculate(calcParams).results.Single();
			calculationLog = result.FreightChargeCodeCalculationLog;
			AssertEquals(0m, calculationLog.Minimum);
			AssertEquals(128m, calculationLog.Maximum);

			// We don't want calculation logs for InnerPacks populated as we don't want AWB (which uses FreightChargeCodeCalculationLog)
			// to populate wrong weight
			((RateLine)calculator.Line).TL_UnitFactor = UnitFactorList.Codes.InnerPack;
			result = calculator.Calculate(calcParams).results.Single();
			AssertNull(result.FreightChargeCodeCalculationLog);
		}

		public void TestCalculationLog_IsCosting()
		{
			var freightChargeCode = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);
			AssertEquals("Precondition: freight charge code", "FRT", freightChargeCode.AC_Code);

			var costingLine = Helper.NewCosting(Helper.NewOrgHeader()).AddRateEntry("ORG").AddRateLine("FRT", FlatCalculator.Code);
			var result = costingLine.Calculator.Calculate(new AutoRatingCalculatorParametersForTesting(new TestRatingCriteria())).results.Single();
			AssertEquals("Calculation log for costing", true, result.FreightChargeCodeCalculationLog.IsCosting);

			var clientRateLine = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("ORG").AddRateLine("FRT", FlatCalculator.Code);
			result = clientRateLine.Calculator.Calculate(new AutoRatingCalculatorParametersForTesting(new TestRatingCriteria())).results.Single();
			AssertEquals("Calculation log for non-costing", false, result.FreightChargeCodeCalculationLog.IsCosting);
		}

		public void TestCalculationLog_WeightVolumeIsCN()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var entry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "NZAKL");

			var line = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN, "AUD");
			line.TL_Rounding = RatingRoundingTypes.Chargeable;

			var ratingCriteria = new TestRatingCriteria();
			ratingCriteria.RateableMeasures.SetQuantity(MeasureType.Weight, 100m, Constants.Weight.Pounds);

			var calcParams = new AutoRatingCalculatorParametersForTesting(ratingCriteria);

			AssertEquals("Precondition", RatingRoundingTypes.Chargeable, line.TL_Rounding);
			AssertEquals("Precondition", QuantityUnit.CN, line.TL_WeightVolume);

			line.TL_RateCalculator = CalculatorForCalculationLogTest.Code;
			var calculator = (CalculatorForCalculationLogTest)line.Calculator;
			var result = calculator.Calculate(calcParams).results.Single();

			AssertEquals("Chargeable left empty", 0m, result.FreightChargeCodeCalculationLog.Chargeable);
			AssertEquals("ChargeableUnit left empty", "", result.FreightChargeCodeCalculationLog.ChargeableUnit);
		}
		public void TestCalculationLog_CatchFailedUnitConversion()
		{
			const string invalidUnit = "WW";
			const string validUnit = Constants.Weight.Pounds;

			var entry = Factory.NewWithValidTestData<RateEntry>();
			var line = entry.RateLines.AddNew();
			line.TL_AC = Env.Registry.FreightChargeCode;
			line.TL_Rounding = RatingRoundingTypes.Chargeable;
			line.TL_RX_NKCurrency = "AUD";
			line.TL_WeightVolume = invalidUnit;
			var ratingCriteria = new TestRatingCriteria();
			ratingCriteria.RateableMeasures.SetQuantity(MeasureType.Weight, 100m, validUnit);
			var calcParams = new AutoRatingCalculatorParametersForTesting(ratingCriteria);
			line.TL_RateCalculator = CalculatorForCalculationLogTest.Code;
			var calculator = (CalculatorForCalculationLogTest)line.Calculator;

			AssertNoExceptionThrown(() =>
			{
				var (calcRes, error) = calculator.Calculate(calcParams);
				AssertEquals(string.Format("Cannot convert {0} to {1}, The conversion logic for unit {1} is not implemented", validUnit, invalidUnit), error);
			});

			line.TL_WeightVolume = validUnit;

			var secondRatingCriteria = new TestRatingCriteria();
			secondRatingCriteria.RateableMeasures.SetQuantity(MeasureType.Weight, 100m, invalidUnit);
			var secondCalcParams = new AutoRatingCalculatorParametersForTesting(secondRatingCriteria);

			AssertNoExceptionThrown(() =>
			{
				var (calcRes, error) = calculator.Calculate(secondCalcParams);
				AssertEquals(string.Format("Cannot convert {0} to {1}, The conversion logic for unit {0} is not implemented", invalidUnit, validUnit), error);
			});
		}

		public void TestCalculationLog_WithContainerNumber()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "AU", "");
			entry.TI_RH_NKCommodityCode = "COM";

			var container = Factory.New<RefContainer>();
			container.RC_Code = "CNT";
			entry.TI_RC = container.PK;

			var line = entry.RateLines.AddNew();
			line.TL_AC = Env.Registry.FreightChargeCode;
			line.TL_WeightVolume = RatingConstants.Units.CN;

			var ratingCriteria = new TestRatingCriteria();
			var measures = ratingCriteria.RateableMeasures;
			measures.AddContainerWithNumber(ZGuid.Empty, "CONT00001", new MeasureInfo.ContainerInfo(100m, Constants.Weight.Pounds, 15m, Constants.Volume.CubicMetres, 1, 1, "CONT00001"));
			for (var i = 0; i < 9; i++)
			{
				// Why was this previously setting the container number to a ZGuid?
				measures.AddContainerWithNumber(ZGuid.Empty, ZString.Empty, new MeasureInfo.ContainerInfo(10m, Constants.Weight.Pounds, 15m, Constants.Volume.CubicMetres, 1, 1, ZString.Empty));
			}

			var calcParams = new AutoRatingCalculatorParametersForTesting(ratingCriteria);
			calcParams.ChargeableAmount = new Quantity(200m, Constants.Weight.Pounds);

			line.TL_RateCalculator = CalculatorForCalculationLogTest.Code;
			var calculator = (CalculatorForCalculationLogTest)line.Calculator;
			var result = calculator.Calculate(calcParams).results.Single();

			var calculationLog = result.FreightChargeCodeCalculationLog;

			CombineAssertions(() =>
			{
				AssertEquals(16m, calculationLog.BaseRate);
				AssertEquals("CLT", calculationLog.CalculatorCode);

				AssertEquals(0m, calculationLog.Chargeable);
				AssertEquals("", calculationLog.ChargeableUnit);

				AssertEquals("FRT", calculationLog.ChargeCode);
				AssertEquals("COM", calculationLog.CommodityCode);
				AssertEquals("CNT", calculationLog.ContainerCode);

				AssertEquals("AUD", calculationLog.Currency);
				AssertEquals(0m, calculationLog.Maximum);
				AssertEquals("Minimum", 0m, calculationLog.Minimum);      //comes from CalculatorForCalculationLogTest

				AssertEquals(Core.Constants.RateMode.LCL, calculationLog.RateMode);
				AssertEquals("Result", 48m, calculationLog.Result);

				AssertEquals("CN", calculationLog.Unit);
				AssertEquals(0m, calculationLog.Weight);

				AssertEquals("Steps", 2, calculationLog.Steps.Count); // due to overriden CalculateInternal in CalculatorForCalculationLogTest

				AssertEquals("UNT", calculationLog.Steps[0].CalculationType);
				AssertEquals("Flat", 0m, calculationLog.Steps[0].Flat);
				AssertEquals(false, calculationLog.Steps[0].IsPercentage);
				AssertEquals(true, calculationLog.Steps[0].IsPerUnit);
				AssertEquals("Percentage", 0m, calculationLog.Steps[0].Percentage);
				AssertEquals("Steps[0].Result", 4m, calculationLog.Steps[0].Result);
				AssertEquals(1m, calculationLog.Steps[0].UnitCount);
				AssertEquals("Steps[0].UnitPrice", 4m, calculationLog.Steps[0].UnitPrice);

				AssertEquals("UNT", calculationLog.Steps[1].CalculationType);
				AssertEquals(8m, calculationLog.Steps[1].Flat);
				AssertEquals(false, calculationLog.Steps[1].IsPercentage);
				AssertEquals(true, calculationLog.Steps[1].IsPerUnit);
				AssertEquals(0m, calculationLog.Steps[1].Percentage);
				AssertEquals(44m, calculationLog.Steps[1].Result);
				AssertEquals(9m, calculationLog.Steps[1].UnitCount);
				AssertEquals(4m, calculationLog.Steps[1].UnitPrice);
			});
		}

		public void TestCalculationStepExtensionMethods()
		{
			var step = new CalculationStep();
			AssertExceptionThrown(typeof(AssertionFailedError), () => step.AssertPerUnit(0m, 0m, 0m));

			step.CalculationType = CalculationStep.Constants.CalculationType.PerUnit;
			step.AssertPerUnit(0m, 0m, 0m);

			step.UnitCount = 10m;
			step.UnitPrice = 20m;
			step.Flat = 30m;
			step.AssertPerUnit(10m, 20m, 30m);

			AssertExceptionThrown(typeof(AssertionFailedError), () => step.AssertPerUnit(0m, 20m, 30m));
			AssertExceptionThrown(typeof(AssertionFailedError), () => step.AssertPerUnit(10m, 0m, 30m));
			AssertExceptionThrown(typeof(AssertionFailedError), () => step.AssertPerUnit(10m, 20m, 0m));
		}

		#region CloneLineItemsUpdatingRateValues

		public void TestCloneLineItemsUpdatingRateValues_FixCalculationOrder()
		{
			var ctbCalculator = CreateCTBCalculator
			(
				percentage: 10m,
				baseRate: 20m,
				perUnit: 30m,
				minimum: 40m,
				perUnitPercentage: 50m,
				calculationOrder: Calculator.Items.IncreaseFirst
			);

			var clonedRateLine = GetClonedRateLine
			(
				ctbCalculator,
				setupCalculator: (rateLine) =>
				{
					CreateSlideRateLineItem(rateLine, Calculator.Items.Operator.BAS, 100m);
					CreateSlideRateLineItem(rateLine, Calculator.Items.Operator.MIN, 200m);
					CreateSlideRateLineItem(rateLine, Calculator.Items.Operator.Minus, 10m, 100m);
					CreateSlideRateLineItem(rateLine, Calculator.Items.Operator.Plus, 20m, 100m);
				}
			);

			var clonedRateLineItemsCollection = clonedRateLine.RateLineItems;
			var clonedRateLineItems = clonedRateLineItemsCollection.Cast<RateLineItem>().ToArray();
			CombineAssertions("Fix should be applied first", () =>
			{
				AssertEquals("Base: (100+20) * 1.1", 132m, clonedRateLineItemsCollection.FindByTM_Type(Calculator.Items.Operator.BAS).TM_Value);
				AssertEquals("Min: (200+40) * 1.1", 264m, clonedRateLineItemsCollection.FindByTM_Type(Calculator.Items.Operator.MIN).TM_Value);
				AssertEquals("-100: (10+30)*1.5", 60m, clonedRateLineItems.SingleOrDefault(rateLineItem => rateLineItem.RateOperatorIsMinus() && rateLineItem.TM_Break == 100)?.TM_Value);
				AssertEquals("+100: (20+30)*1.5", 75m, clonedRateLineItems.SingleOrDefault(rateLineItem => rateLineItem.RateOperatorIsPlus() && rateLineItem.TM_Break == 100)?.TM_Value);
			});
		}

		public void TestCloneLineItemsUpdatingRateValues_PercentageCalculationOrder()
		{
			var ctbCalculator = CreateCTBCalculator
			(
				percentage: 10m,
				baseRate: 20m,
				perUnit: 30m,
				minimum: 40m,
				perUnitPercentage: 50m,
				calculationOrder: Calculator.Items.PercentFirst
			);

			var clonedRateLine = GetClonedRateLine
			(
				ctbCalculator,
				setupCalculator: (rateLine) =>
				{
					CreateSlideRateLineItem(rateLine, Calculator.Items.Operator.BAS, 100m);
					CreateSlideRateLineItem(rateLine, Calculator.Items.Operator.MIN, 200m);
					CreateSlideRateLineItem(rateLine, Calculator.Items.Operator.Minus, 10m, 100m);
					CreateSlideRateLineItem(rateLine, Calculator.Items.Operator.Plus, 20m, 100m);
				}
			);

			var clonedRateLineItemsCollection = clonedRateLine.RateLineItems;
			var clonedRateLineItems = clonedRateLineItemsCollection.Cast<RateLineItem>().ToArray();
			CombineAssertions("Percentage should be applied first", () =>
			{
				AssertEquals("Base: (100 * 1.10) + 20", 130m, clonedRateLineItemsCollection.FindByTM_Type(Calculator.Items.Operator.BAS).TM_Value);
				AssertEquals("Min: 200 * 1.1 + 40", 260m, clonedRateLineItemsCollection.FindByTM_Type(Calculator.Items.Operator.MIN).TM_Value);
				AssertEquals("-100: (10 * 1.5) + 30", 45m, clonedRateLineItems.SingleOrDefault(rateLineItem => rateLineItem.RateOperatorIsMinus() && rateLineItem.TM_Break == 100)?.TM_Value);
				AssertEquals("+100: (20 * 1.5) + 30", 60m, clonedRateLineItems.SingleOrDefault(rateLineItem => rateLineItem.RateOperatorIsPlus() && rateLineItem.TM_Break == 100)?.TM_Value);
			});
		}

		public void TestCloneLineItemsUpdatingRateValues_CombinedCalculatorAndCostBasedCalculator()
		{
			var ctbCalculator = CreateCTBCalculator
			(
				percentage: 10m,
				baseRate: 20m,
				perUnit: 30m,
				minimum: 40m,
				perUnitPercentage: 50m,
				calculationOrder: Calculator.Items.IncreaseFirst
			);

			var rateLine = GetNewCombinedCalculatorRateLine();
			var clientRateEntry = (RateEntry)rateLine.ParentRateEntry;
			var combinedCalculator = rateLine.GetCalculator<CombinedCalculator>();
			combinedCalculator.BaseRate = 100m;
			combinedCalculator.Minimum = 200m;
			combinedCalculator.PerUnit = 300m;

			var clonedRateLine1 = rateLine.Clone(clientRateEntry.RateLines);
			rateLine.Calculator.CloneLineItemsUpdatingRateValues(ctbCalculator, clonedRateLine1, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			var clonedRateLineItemsCollection1 = clonedRateLine1.RateLineItems;
			CombineAssertions("Fix should be applied first", () =>
			{
				AssertEquals("Base: (100+20) * 1.1", 132m, clonedRateLineItemsCollection1.FindByTM_Type(Calculator.Items.Operator.BAS).TM_Value);
				AssertEquals("Min: (200+40) * 1.1", 264m, clonedRateLineItemsCollection1.FindByTM_Type(Calculator.Items.Operator.MIN).TM_Value);
				AssertEquals("PerUnit: (300+30)*1.5", 495m, clonedRateLineItemsCollection1.FindByTM_Type(Calculator.Items.Operator.UNT).TM_Value);
			});

			ctbCalculator.CalculationOrder = Calculator.Items.PercentFirst;
			var clonedRateLine2 = rateLine.Clone(clientRateEntry.RateLines);
			rateLine.Calculator.CloneLineItemsUpdatingRateValues(ctbCalculator, clonedRateLine2, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			var clonedRateLineItemsCollection2 = clonedRateLine2.RateLineItems;
			CombineAssertions("Percent should be applied first", () =>
			{
				AssertEquals("Base: (100 * 1.10) + 20", 130m, clonedRateLineItemsCollection2.FindByTM_Type(Calculator.Items.Operator.BAS).TM_Value);
				AssertEquals("Min: 200 * 1.1 + 40", 260m, clonedRateLineItemsCollection2.FindByTM_Type(Calculator.Items.Operator.MIN).TM_Value);
				AssertEquals("PerUnit: (300 * 1.5) + 30", 480m, clonedRateLineItemsCollection2.FindByTM_Type(Calculator.Items.Operator.UNT).TM_Value);
			});
		}

		RateLine GetNewCombinedCalculatorRateLine()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "BBB";
			chargeCode.AC_Desc = "Charge Code BBB";

			var clientRate = Factory.New<ClientRate>();
			var clientRateEntry = clientRate.AddRateEntry("LCL", "SEA", "AUSYD", "USLAX", "", "");

			var rateLine = clientRateEntry.AddRateLine(chargeCode.AC_Code, CombinedCalculator.Code, QuantityUnit.KG);

			return rateLine;
		}

		RateLine GetClonedRateLine(CompanyTariffOrCostBasedCalculator companyTariffBasedCalculator, Action<RateLine> setupCalculator)
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "BBB";
			chargeCode.AC_Desc = "Charge Code BBB";

			var clientRate = Factory.New<ClientRate>();
			var clientRateEntry = clientRate.AddRateEntry("LCL", "SEA", "AUSYD", "USLAX", "", "");

			var rateLine = clientRateEntry.RateLines.AddNew();
			rateLine.TL_AC = chargeCode.PK;
			rateLine.TL_RateCalculator = CartageCalculator.Code;
			rateLine.InitializeCalculator();
			rateLine.RateLineItems.FindByTM_Type(CartageCalculator.Items.EquipmentType).TM_Text = Constants.FCLEquipmentNeeded.SideLoader;

			setupCalculator(rateLine);

			var clonedRateLine = rateLine.Clone(clientRateEntry.RateLines);
			rateLine.Calculator.CloneLineItemsUpdatingRateValues(companyTariffBasedCalculator, clonedRateLine, RateLineItem.RateTypeToUpdate.StandardAndAgent);

			AssertEquals("Cloned rateLineItems count should match rateLineItems", rateLine.RateLineItems.Count, clonedRateLine.RateLineItems.Count);

			return clonedRateLine;
		}

		CompanyTariffOrCostBasedCalculator CreateCTBCalculator(ZDecimal percentage, ZDecimal baseRate, ZDecimal perUnitPercentage, ZDecimal perUnit, ZDecimal minimum, string calculationOrder)
		{
			var chargecode = Factory.New<AccChargeCode>();
			chargecode.AC_Code = "AAA";
			chargecode.AC_Desc = "Charge Code AAA";

			var tariff = Factory.New<CompanyTariff>();
			tariff.TH_GlobalRateLevel = 1;
			tariff.TH_GlobalRateDescription = "Company Tariff Level 1";

			var tariffEntry = tariff.AddRateEntry("LCL", "SEA", "AUSYD", "USLAX", "", "");
			var companyTariffRateLine = tariffEntry.RateLines.AddNew();
			companyTariffRateLine.TL_AC = chargecode.PK;

			companyTariffRateLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			companyTariffRateLine.InitializeCalculator();

			companyTariffRateLine.RateLineItems.FindByTM_Type(Calculator.Items.CalculationOrder).TM_Text = calculationOrder;
			companyTariffRateLine.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER).TM_Value = percentage;
			companyTariffRateLine.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS).TM_Value = baseRate;
			companyTariffRateLine.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PRU).TM_Value = perUnitPercentage;
			companyTariffRateLine.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT).TM_Value = perUnit;
			companyTariffRateLine.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MIN).TM_Value = minimum;

			return (CompanyTariffOrCostBasedCalculator)companyTariffRateLine.Calculator;
		}

		static RateLineItem CreateSlideRateLineItem(RateLine rateLine, string type, decimal value, decimal? breakValue = null)
		{
			var rateLineItem = rateLine.RateLineItems.AddNew();
			rateLineItem.TM_Type = type;
			if (breakValue != null)
			{
				rateLineItem.TM_Break = breakValue.Value;
			}
			rateLineItem.TM_Value = value;

			return rateLineItem;
		}

		public void TestGetCloneLineItems()
		{
			var chargeCode1 = Factory.New<AccChargeCode>();
			chargeCode1.AC_Code = "DDD";
			chargeCode1.AC_Desc = "Charge Code DDD";

			var entry1 = Factory.New<ClientRate>().EntryCollections[RatingConstants.RateCategory.LCL].LazyLoadingCollection.AddNew();
			entry1.TI_RateCategory = RatingConstants.RateCategory.LCL;

			var companyTariffRateLine = entry1.RateLines.AddNew();
			companyTariffRateLine.TL_AC = chargeCode1.PK;
			companyTariffRateLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			var ctbCalculator = (CompanyTariffOrCostBasedCalculator)companyTariffRateLine.Calculator;
			companyTariffRateLine.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS).TM_Value = 5.555m;
			companyTariffRateLine.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MIN).TM_Value = 4.444m;
			companyTariffRateLine.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT).TM_Value = 3.333m;

			var chargeCode2 = Factory.New<AccChargeCode>();
			chargeCode2.AC_Code = "CCC";
			chargeCode2.AC_Desc = "Charge Code CCC";

			var entry = Factory.New<ClientRate>().EntryCollections[RatingConstants.RateCategory.LCL].LazyLoadingCollection.AddNew();
			entry.TI_RateCategory = RatingConstants.RateCategory.LCL;

			var line = entry.RateLines.AddNew();
			line.TL_AC = chargeCode2.PK;
			line.TL_RateCalculator = CartageCalculator.Code;
			line.InitializeCalculator();
			line.RateLineItems.FindByTM_Type(CartageCalculator.Items.EquipmentType).TM_Text = Constants.FCLEquipmentNeeded.SideLoader;

			var lineItem1 = line.RateLineItems.AddNew();
			lineItem1.TM_Type = Calculator.Items.Operator.BAS;
			lineItem1.TM_Value = 5m;
			var lineItem2 = line.RateLineItems.AddNew();
			lineItem2.TM_Type = Calculator.Items.Operator.UNT;
			lineItem2.TM_Value = 7m;
			var lineItem3 = line.RateLineItems.AddNew();
			lineItem3.TM_Type = Calculator.Items.Operator.MIN;
			lineItem3.TM_Value = 6m;

			var clonedRateLine = line.Clone(entry.RateLines);
			line.Calculator.CloneLineItemsUpdatingRateValues(ctbCalculator, clonedRateLine, RateLineItem.RateTypeToUpdate.Relevant);
			AssertEquals(10.555m, clonedRateLine.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS).TM_Value);
			AssertEquals("Min should be 10.444 = 6 + 4.444", 10.444m, clonedRateLine.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MIN).TM_Value);
			AssertEquals(10.333m, clonedRateLine.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT).TM_Value);
		}

		public void TestGetCloneLineItems_IncludeAgentRates()
		{
			var chargeCode1 = Factory.New<AccChargeCode>();
			chargeCode1.AC_Code = "DDD";
			chargeCode1.AC_Desc = "Charge Code DDD";

			var entry1 = Factory.New<ClientRate>().EntryCollections[RatingConstants.RateCategory.LCL].LazyLoadingCollection.AddNew();

			var companyTariffRateLine = entry1.RateLines.AddNew();
			companyTariffRateLine.TL_AC = chargeCode1.PK;
			companyTariffRateLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;

			var ctbCalculator = (CompanyTariffOrCostBasedCalculator)companyTariffRateLine.Calculator;

			companyTariffRateLine.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS).TM_Value = 5.555m;
			companyTariffRateLine.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT).TM_Value = 3.333m;

			var chargeCode2 = Factory.New<AccChargeCode>();
			chargeCode2.AC_Code = "CCC";
			chargeCode2.AC_Desc = "Charge Code CCC";

			var entry = Factory.New<ClientRate>().EntryCollections[RatingConstants.RateCategory.LCL].LazyLoadingCollection.AddNew();
			entry.TI_RateCategory = RatingConstants.RateCategory.LCL;

			var line = entry.RateLines.AddNew();
			line.TL_AC = chargeCode2.PK;
			line.TL_RateCalculator = CartageCalculator.Code;
			line.InitializeCalculator();
			line.RateLineItems.FindByTM_Type(CartageCalculator.Items.EquipmentType).TM_Text = Constants.FCLEquipmentNeeded.SideLoader;

			var lineItem1 = line.RateLineItems.AddNew();
			lineItem1.TM_Type = Calculator.Items.Operator.BAS;
			lineItem1.TM_Value = 5m;
			lineItem1.TM_AgentDeclaredRate = 15m;
			var lineItem2 = line.RateLineItems.AddNew();
			lineItem2.TM_Type = Calculator.Items.Operator.UNT;
			lineItem2.TM_Value = 7m;
			lineItem2.TM_AgentDeclaredRate = 17m;

			var clonedRateLine = line.Clone(entry.RateLines);
			line.Calculator.CloneLineItemsUpdatingRateValues(ctbCalculator, clonedRateLine, RateLineItem.RateTypeToUpdate.StandardAndAgent);

			AssertEquals("5m + 5.555m", 10.555m, clonedRateLine.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS).TM_Value);
			AssertEquals("7m + 3.333m", 10.333m, clonedRateLine.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT).TM_Value);

			AssertEquals("15m + 5.555m", 20.555m, clonedRateLine.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS).TM_AgentDeclaredRate);
			AssertEquals("17m + 3.333m", 20.333m, clonedRateLine.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT).TM_AgentDeclaredRate);
		}

		public void TestGetCloneLineItems_AgentRatesWithWeightBreak()
		{
			var chargeCode1 = Factory.New<AccChargeCode>();
			chargeCode1.AC_Code = "AAA";
			chargeCode1.AC_Desc = "Charge Code AAA";

			var tariff = Factory.New<CompanyTariff>();
			tariff.TH_GlobalRateLevel = 1;
			tariff.TH_GlobalRateDescription = "Company Tariff Level 1";

			var tariffEntry = tariff.AddRateEntry("LCL", "SEA", "AUSYD", "USLAX", "", "");
			var companyTariffRateLine = tariffEntry.RateLines.AddNew();
			companyTariffRateLine.TL_AC = chargeCode1.PK;

			companyTariffRateLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			companyTariffRateLine.InitializeCalculator();

			companyTariffRateLine.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS).TM_Value = 50m;
			companyTariffRateLine.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MIN).TM_Value = 10m;

			var minusItem = companyTariffRateLine.RateLineItems.AddNew();
			minusItem.TM_Type = Calculator.Items.Operator.Minus;
			minusItem.TM_Break = 50m;
			minusItem.TM_Value = 12m;

			var plusItem = companyTariffRateLine.RateLineItems.AddNew();
			plusItem.TM_Type = Calculator.Items.Operator.Plus;
			plusItem.TM_Break = 50m;
			plusItem.TM_Value = 5m;

			var chargeCode2 = Factory.New<AccChargeCode>();
			chargeCode2.AC_Code = "BBB";
			chargeCode2.AC_Desc = "Charge Code BBB";

			var clientRate = Factory.New<ClientRate>();
			var clientRateEntry = clientRate.AddRateEntry("LCL", "SEA", "AUSYD", "USLAX", "", "");

			var line = clientRateEntry.RateLines.AddNew();
			line.TL_AC = chargeCode2.PK;
			line.TL_RateCalculator = CartageCalculator.Code;
			line.InitializeCalculator();
			line.RateLineItems.FindByTM_Type(CartageCalculator.Items.EquipmentType).TM_Text = Constants.FCLEquipmentNeeded.SideLoader;

			var lineItem1 = line.RateLineItems.AddNew();
			lineItem1.TM_Type = Calculator.Items.Operator.BAS;
			lineItem1.TM_Value = 200m;
			lineItem1.TM_AgentDeclaredRate = 215m;
			var lineItem2 = line.RateLineItems.AddNew();
			lineItem2.TM_Type = Calculator.Items.Operator.MIN;
			lineItem2.TM_Value = 60m;
			lineItem2.TM_AgentDeclaredRate = 80m;
			var lineItem3 = line.RateLineItems.AddNew();
			lineItem3.TM_Break = 50m;
			lineItem3.TM_Type = Calculator.Items.Operator.Minus;
			lineItem3.TM_Value = 30m;
			lineItem3.TM_AgentDeclaredRate = 35m;
			var lineItem4 = line.RateLineItems.AddNew();
			lineItem4.TM_Break = 50m;
			lineItem4.TM_Type = Calculator.Items.Operator.Plus;
			lineItem4.TM_Value = 25m;
			lineItem4.TM_AgentDeclaredRate = 30m;
			var lineItem5 = line.RateLineItems.AddNew();
			lineItem5.TM_Break = 100m;
			lineItem5.TM_Type = Calculator.Items.Operator.Plus;
			lineItem5.TM_Value = 24m;
			lineItem5.TM_AgentDeclaredRate = 28m;

			var clonedRateLine = line.Clone(clientRateEntry.RateLines);
			var ctbCalculator = (CompanyTariffOrCostBasedCalculator)companyTariffRateLine.Calculator;
			line.Calculator.CloneLineItemsUpdatingRateValues(ctbCalculator, clonedRateLine, RateLineItem.RateTypeToUpdate.StandardAndAgent);

			AssertEquals(line.RateLineItems.Count, clonedRateLine.RateLineItems.Count);

			var clonedBaseItem = clonedRateLine.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS);
			var clonedMinItem = clonedRateLine.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MIN);
			var clonedMinusItem = clonedRateLine.RateLineItems.FindByTM_Type(Calculator.Items.Operator.Minus);

			var clones = clonedRateLine.RateLineItems.Cast<RateLineItem>().ToArray();
			var clonedPlusItem1 = clones.FirstOrDefault(x => x.RateOperatorIsPlus() && x.TM_Break == 50);
			var clonedPlusItem2 = clones.FirstOrDefault(x => x.RateOperatorIsPlus() && x.TM_Break == 100);

			CombineAssertions(() =>
			{
				AssertEquals("200m + 50m", 250m, clonedBaseItem.TM_Value);
				AssertEquals("60m + 10m", 70m, clonedMinItem.TM_Value);
				AssertEquals("30m + 12m", 42m, clonedMinusItem.TM_Value);
				AssertEquals("25m + 5m", 30m, clonedPlusItem1.TM_Value);
				AssertEquals("24m + 5m", 29m, clonedPlusItem2.TM_Value);

				AssertEquals("215m + 50m", 265m, clonedBaseItem.TM_AgentDeclaredRate);
				AssertEquals("80m + 10m", 90m, clonedMinItem.TM_AgentDeclaredRate);
				AssertEquals("35m + 12m", 47m, clonedMinusItem.TM_AgentDeclaredRate);
				AssertEquals("30m + 5m", 35m, clonedPlusItem1.TM_AgentDeclaredRate);
				AssertEquals("28m + 5m", 33m, clonedPlusItem2.TM_AgentDeclaredRate);
			});
		}

		#endregion

		public void TestGetAdditionalDescription()
		{
			var createFactory = new BusinessObjectFactory();

			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "CLIENT";

			var consignor = createFactory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CONSIGNOR";
			consignor.OH_FullName = "CONSIGNOR COMPANY";

			var consignee = createFactory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CONSIGNEE";
			consignee.OH_FullName = "CONSIGNEE COMPANY";

			var provider = createFactory.NewWithValidTestData<OrgHeader>();
			provider.OH_Code = "PROVIDER";
			provider.OH_FullName = "PROVIDER COMPANY";

			var warehouse1 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IWhsWarehouse)));
			warehouse1[WhsWarehouseSchema.WW_WarehouseCode] = "WR1";
			warehouse1[WhsWarehouseSchema.WW_WarehouseName] = "Warehouse1";

			var pickup = createFactory.NewWithValidTestData<OrgHeader>().MainAddress;
			pickup.Header.OH_Code = "PICKUP";
			pickup.OA_City = "Sydney";
			pickup.OA_State = "NSW";

			var delivery = createFactory.NewWithValidTestData<OrgHeader>().MainAddress;
			delivery.Header.OH_Code = "DELIVERY";
			delivery.OA_City = "Los Angeles";
			delivery.OA_State = "California";

			var chargeCode1 = Factory.New<AccChargeCode>();
			chargeCode1.AC_Code = "AAA";
			chargeCode1.AC_Desc = "Charge Code AAA";

			var quote = Helper.NewQuote(client);

			var entry1 = quote.AddRateEntry("FCL");

			var line1 = entry1.RateLines.AddNew();
			line1.TL_AC = chargeCode1.PK;
			line1.TL_RateCalculator = FlatCalculator.Code;
			line1.InitializeCalculator();
			((FlatCalculator)line1.Calculator).BaseRate = 100;

			entry1.TI_OH_Consignee = consignee.PK;
			entry1.TI_OH_Consignor = consignor.PK;
			entry1.TI_OH_TransportProvider = provider.PK;
			entry1.TI_PL_NKCarrierServiceLevel = "D2D";
			entry1.TI_CartagePickupAddressPostCode = "2000";
			entry1.TI_CartageDeliveryAddressPostCode = "3000";

			createFactory.Save();

			var expectedLine = @"Charge Code AAA 
-  From Postcode 2000 for CONSIGNOR COMPANY To Postcode 3000 for CONSIGNEE COMPANY
-  Carrier Service Level: D2D|USD|100.00|";

			var list1 = line1.Calculator.GetQuotationLines(entry1);
			AssertEquals(expectedLine, list1[0].ToString());

			var originZone = Factory.NewWithValidTestData<RateTransportZone>();
			var destinationZone = Factory.NewWithValidTestData<RateTransportZone>();

			originZone.TZ_ZoneName = "Origin Zone";
			destinationZone.TZ_ZoneName = "Destination Zone";

			entry1.TI_TZ_OriginZone = originZone.PK;
			entry1.TI_TZ_DestinationZone = destinationZone.PK;

			var originSuburb = Factory.NewWithValidTestData<RefCityTown>();
			var destinationSuburb = Factory.NewWithValidTestData<RefCityTown>();

			originSuburb.R9_InternationalName = "ORIGIN SUBURB";
			destinationSuburb.R9_InternationalName = "DESTINATION SUBURB";

			entry1.OriginSuburbPK = originSuburb.PK;
			entry1.DestinationSuburbPK = destinationSuburb.PK;

			createFactory.Save();

			expectedLine = @"Charge Code AAA 
-  From Origin Zone ORIGIN SUBURB 2000 for CONSIGNOR COMPANY To Destination Zone DESTINATION SUBURB 3000 for CONSIGNEE COMPANY
-  Carrier Service Level: D2D|USD|100.00|";

			list1 = line1.Calculator.GetQuotationLines(entry1);
			AssertEquals(expectedLine, list1[0].ToString());

			var entry2 = quote.AddRateEntry("ORG");

			var line2 = entry2.RateLines.AddNew();
			line2.TL_AC = chargeCode1.PK;
			line2.TL_RateCalculator = FlatCalculator.Code;
			line2.InitializeCalculator();
			((FlatCalculator)line2.Calculator).BaseRate = 200;

			entry2.TI_RS_NKServiceLevel_NI = "STD";
			entry2.TI_RH_NKCommodityCode = "GEN";

			expectedLine = @"Charge Code AAA|AUD|200.00|";

			var list2 = line2.Calculator.GetQuotationLines(entry2);
			AssertEquals(expectedLine, list2[0].ToString());

			var entry3 = quote.AddRateEntry("WHS");

			var line3 = entry3.RateLines.AddNew();
			line3.TL_AC = chargeCode1.PK;
			line3.TL_RateCalculator = FlatCalculator.Code;
			line3.InitializeCalculator();
			((FlatCalculator)line3.Calculator).BaseRate = 300;

			entry3.TI_WW_Warehouse = warehouse1.PK;
			entry3.TI_OA_CartagePickupAddressOverride = pickup.PK;
			entry3.TI_OA_CartageDeliveryAddressOverride = delivery.PK;
			pickup.OA_PostCode = "2000";
			delivery.OA_PostCode = "3000";

			expectedLine = @"Charge Code AAA 
-  From Sydney, NSW To Los Angeles, California
-  Warehouse: Warehouse1|AUD|300.00|";

			var list3 = line3.Calculator.GetQuotationLines(entry3);
			AssertEquals(expectedLine, list3[0].ToString());
		}

		public void TestConversionFactorInQuotationLine()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "BBB";
			chargeCode.AC_Desc = "Charge Code BBB";

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);

			var line = entry.AddRateLine(chargeCode, UnitCalculator.Code, Constants.Volume.CubicYards);
			line.ConversionFactor = new ConversionFactor(1000m, Constants.Volume.CubicInches, Constants.Weight.Pounds);

			var lineItem1 = line.RateLineItems.AddNew();
			lineItem1.TM_Value = 200m;

			AssertEquals("CY (1 LB = 1000 CI)", line.Calculator.UnitDescription(true));

			var entry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL);

			var line2 = entry2.AddRateLine(chargeCode, UnitCalculator.Code, Constants.Volume.CubicYards);
			line2.ConversionFactor = new ConversionFactor(1000m, Constants.Volume.CubicInches, Constants.Weight.Pounds);

			var lineItem2 = line2.RateLineItems.AddNew();
			lineItem2.TM_Value = 200m;

			AssertEquals("CY", line2.Calculator.UnitDescription(true));
		}

		public void TestUnitDescriptionServicesFromChargeCode()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "BBB";
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG);

			var line = entry.RateLines.AddNew();
			line.TL_AC = chargeCode.PK;
			line.TL_RateCalculator = UnitCalculator.Code;
			line.InitializeCalculator();
			line.TL_WeightVolume = QuantityUnit.SV;
			line.RateLineItems.AddNew();

			AssertEquals("Origin Service", line.Calculator.UnitDescription(true));

			chargeCode.AC_ChargeSubGroup = Core.Constants.FreightServiceType.Codes.Fumigation;

			AssertEquals("Origin Fumigation", line.Calculator.UnitDescription(true));
		}

		public void TestGetUnitDescription()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL);

			var line = entry.RateLines.AddNew();
			line.TL_AC = Helper.ChargeCodes["FRT"].PK;
			line.TL_RateCalculator = UnitCalculator.Code;
			line.InitializeCalculator();
			line.RateLineItems.AddNew();
			line.TL_WeightVolume = QuantityUnit.LW;

			AssertEquals("Lowest Bill", line.Calculator.UnitDescription(true));

			line.TL_WeightVolume = QuantityUnit.JP;

			AssertEquals("Job Package", line.Calculator.UnitDescription(true));

			line.TL_WeightVolume = QuantityUnit.CP;

			AssertEquals("Job Pallet", line.Calculator.UnitDescription(true));

			line.TL_WeightVolume = QuantityUnit.PL;

			AssertEquals("Pallet", line.Calculator.UnitDescription(true));

			line.TL_WeightVolume = QuantityUnit.WK;

			AssertEquals("Week", line.Calculator.UnitDescription(true));

			line.TL_WeightVolume = Constants.PkgUnit.Dozen;

			AssertEquals("Dozen", line.Calculator.UnitDescription(true));

			line.TL_WeightVolume = Constants.PkgUnit.Unit;

			AssertEquals("Unit", line.Calculator.UnitDescription(true));

			line.TL_WeightVolume = Constants.Weight.Kilograms;

			AssertEquals("Single weight/volume units display just the short code", "KG", line.Calculator.UnitDescription(true));

			line.TL_WeightVolume = Constants.Weight.Pounds;

			AssertEquals("LB", line.Calculator.UnitDescription(true));

			line.TL_WeightVolume = Constants.Weight.LongTons;

			AssertEquals("TL", line.Calculator.UnitDescription(true));

			line.TL_WeightVolume = Constants.Volume.CubicMetres;

			AssertEquals("M3", line.Calculator.UnitDescription(true));

			line.TL_WeightVolume = Constants.Volume.MegaLitre;

			AssertEquals("ML", line.Calculator.UnitDescription(true));
		}

		public void TestContainerOwnershipDescription()
		{
			var rateLine = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("SCO", "SEA", "AUSYD", "USLAX", "STD", "20GP").RateLines[0];
			AssertEquals("20GP Container", rateLine.Calculator.UnitDescription(true, addContainerCode: true));

			rateLine.TL_ContainerOwnership = Constants.ContainerOwnership.Codes.ShipperOwned;
			AssertEquals("20GP Shipper Owned Container", rateLine.Calculator.UnitDescription(true, addContainerCode: true));
		}

		[ExpectNoExceptions]
		public void TestReadOnlyRateLine_CheckOrCreateItems_NoNewItemsCreated()
		{
			var rate = Factory.New<ClientRate>();
			var entry = rate.AddRateEntry("DST");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.RateLines.AddNew();

			line.TL_RateCalculator = CombinedCalculator.Code;
			line.TL_WeightVolume = "FI";
			line.TL_Rounding = RatingRoundingTypes.UpTo1;
			line.TL_AC = Env.Registry.FreightChargeCode;

			line.RateLineItems.RemoveAndDeleteAll();
			line.Calculator.AddRateLineItem("-", 1m, 25m, 0m);
			line.Calculator.AddRateLineItem("+", 1m, 5m, 20m);

			line.ReadOnly = true;

			line.Calculator.PerformPostConstructionActions();
		}

		public void TestCalculate_SetAttribute_TransportProviderPK_ClientRate()
		{
			var container20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var providerOrg = Factory.NewWithValidTestData<OrgHeader>();

			var rate = Factory.New<ClientRate>();
			var entry = rate.AddRateEntry("FCL", "SEA", "AUSYD", "NZAKL", container: container20GP.RC_Code, commodity: "GEN", removeLines: true);
			entry.TI_OH_TransportProvider = providerOrg.PK;
			var rateLine = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);

			var criteria = new TestRatingCriteria();
			criteria.RateableMeasures.AddContainer(container20GP.PK);

			var parameters = new AutoRatingCalculatorParametersForTesting(criteria)
			{
				ChargeableAmount = new Quantity(1, QuantityUnit.CN)
			};

			var result = rateLine.Calculator.Calculate(parameters).results.Single();
			var comparableAttributes = result.Attributes.ComparableAttributes;

			var expectedAttribute = $"{JobChargeAttribTypeList.Codes.TransportProviderPK}|{providerOrg.PK.ToString()}|0";
			var actualComparableAttributes = comparableAttributes
				.Select(attr => $"{attr.Code}|{attr.Value}|{attr.Amount}")
				.ToArray();

			AssertCollectionContains(
				"The expected provider attribute should be present in comparable attributes",
				expectedAttribute,
				actualComparableAttributes
			);

			// Not set if no provider
			entry.TI_OH_TransportProvider = ZGuid.Empty;
			comparableAttributes = rateLine.Calculator.Calculate(parameters).results.Single().Attributes.ComparableAttributes;
			var comparableCodes = comparableAttributes.Select(x => x.Code).ToArray();

			AssertCollectionNotContains(
				"The provider attribute should not be present when no provider is set",
				JobChargeAttribTypeList.Codes.TransportProviderPK,
				comparableCodes
			);
		}

		public void TestCalculate_SetAttributes_Costing()
		{
			var container20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var serviceProvider = Factory.NewWithValidTestData<OrgHeader>();
			var transportProvider = Factory.NewWithValidTestData<OrgHeader>();

			var costing = Helper.NewCosting(serviceProvider);
			var entry = costing.AddRateEntry("FCL", "SEA", "AUSYD", "NZAKL", container: container20GP.RC_Code, commodity: "GEN", removeLines: true);
			entry.TI_OH_TransportProvider = transportProvider.PK;
			var rateLine = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);

			var criteria = new TestRatingCriteria();
			criteria.RateableMeasures.AddContainer(container20GP.PK);

			var parameters = new AutoRatingCalculatorParametersForTesting(criteria)
			{
				ChargeableAmount = new Quantity(1, QuantityUnit.CN)
			};

			var result = rateLine.Calculator.Calculate(parameters).results.Single();
			var comparableAttributes = result.Attributes.ComparableAttributes;

			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.TransportProviderPK, transportProvider.PK.ToString()), comparableAttributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.ServiceProviderPK, serviceProvider.PK.ToString()), comparableAttributes);
		}

		public void TestCalculate_SetAttribute_ContainerType()
		{
			var container20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			var rate = Factory.New<ClientRate>();
			var entry = rate.AddRateEntry("FCL", "SEA", "AUSYD", "NZAKL", container: container20GP.RC_Code, commodity: "GEN");
			entry.RateLines.RemoveAndDeleteAll();
			var rateLine = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);

			var criteria = new TestRatingCriteria();
			criteria.RateableMeasures.AddContainer(container20GP.PK);

			var parameters = new AutoRatingCalculatorParametersForTesting(criteria)
			{
				ChargeableAmount = new Quantity(1, QuantityUnit.CN)
			};

			var result = rateLine.Calculator.Calculate(parameters).results.Single();
			var comparableAttributes = result.Attributes.ComparableAttributes;

			var actualAttributes = comparableAttributes
				.Select(attr => $"{attr.Code}|{attr.Value}")
				.ToArray();

			AssertCollectionContains("The comparable attributes should contain the expected container code and value.", $"{JobChargeAttribTypeList.Codes.ContainerCode}|{container20GP.RC_Code}", actualAttributes);
		}

		public void TestCalculate_SetAttribute_RateId_CW1Rate()
		{
			var rate = Factory.New<ClientRate>();

			var entry = rate.AddRateEntry("FCL");
			entry.RateLines.RemoveAndDeleteAll();

			var rateLine = entry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);

			var criteria = new TestRatingCriteria();

			var parameters = new AutoRatingCalculatorParametersForTesting(criteria)
			{
				ChargeableAmount = new Quantity(1, QuantityUnit.CN)
			};

			var result = rateLine.Calculator.Calculate(parameters).results.Single();
			var comparableAttributes = result.Attributes.Attributes;

			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.RateId, entry.PK.ToString()), comparableAttributes);
		}

		public void TestCalculate_SetAttribute_RateId_WiseRate()
		{
			var rateLine = new WiseLine(Factory, new Charge());
			rateLine.TL_AC = Helper.ChargeCodes["FRT"].PK;
			rateLine.TL_WeightVolume = "CN";
			rateLine.TL_RX_NKCurrency = "USD";
			rateLine.RateCalculatorType = CalculatorType.Unit;
			rateLine.ChildRateLineItems = new[]
			{
				new WiseLineItem(rateLine, Calculator.Items.Operator.UNT, string.Empty, 5m, ZString.Empty, 0m, 0m, null)
			};

			var entry = new WiseEntry(new Rate { Origin = "AUSYD", Destination = "UAIEV", ProviderRateId = "McLaren" }, Factory);
			entry.ChildRateLines = new[] { rateLine };

			rateLine.ParentRateEntry = entry;

			var criteria = new TestRatingCriteria();

			var parameters = new AutoRatingCalculatorParametersForTesting(criteria)
			{
				ChargeableAmount = new Quantity(1, QuantityUnit.CN)
			};

			var result = rateLine.Calculator.Calculate(parameters).results.Single();
			var comparableAttributes = result.Attributes.Attributes;

			var expectedAttribute = $"{JobChargeAttribTypeList.Codes.RateId}|{entry.RateId}";

			var actualAttributes = comparableAttributes
				.Select(attr => $"{attr.Code}|{attr.Value}")
				.ToArray();

			AssertCollectionContains(
				expectedAttribute,
				actualAttributes
			);
		}

		public void TestCalculate_WhenUsingNonSlidingCalculator_ThenNoCalculatorDescriptionAttribute()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "XYZ";
			chargeCode.AC_Desc = "Charge Code 1";

			var rate = Factory.New<CompanyTariff>();
			var entry = rate.AddRateEntry("ORG");

			var line = entry.RateLines.AddNew();
			line.TL_AC = chargeCode.PK;
			line.TL_RateCalculator = FlatCalculator.Code;
			line.InitializeCalculator();
			((FlatCalculator)line.Calculator).BaseRate = 100;
			var calc = (FlatCalculator)line.Calculator;
			var calcParams = new AutoRatingCalculatorParametersForTesting(new TestRatingCriteria());

			var result = calc.Calculate(calcParams).results.Single();

			var actualAttributes = result.Attributes.Attributes.Select(a => a.Code).ToArray();
			var prohibitedCode = JobChargeAttribTypeList.Codes.CalculatorDescription;

			AssertCollectionNotContains(
				"The collection of attributes should not contain the CalculatorDescription code.",
				prohibitedCode,
				actualAttributes
			);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "XYZ";
			chargeCode.AC_Desc = "Test Charge";

			var rate = Factory.New<Costing>();
			Entry = rate.AddRateEntry("ORG");

			Line = Entry.RateLines.AddNew();
			Line.TL_AC = chargeCode.PK;
			Line.TL_RateCalculator = CalculatorForTest.Code;

			AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10);
			Factory.Save();
		}

		RateEntry Entry { get; set; }
		RateLine Line { get; set; }

		string Render(QuotationLineList list)
		{
			var builder = new StringBuilder();

			foreach (var line in list)
			{
				builder.AppendLine(line.ToString());
			}

			return builder.ToString();
		}

		TestHelper Helper
		{
			get { return helper ?? (helper = new TestHelper(Factory)); }
		}
		TestHelper helper;

		#endregion
	}

	public class AutoRatingCalculatorParametersForTesting : AutoRatingCalculatorParametersWithoutFilter
	{
		public AutoRatingCalculatorParametersForTesting(RatingCriteria criteria)
			: base(criteria, new FreightAutoRater(new RatingContext()))
		{
		}

		public AutoRatingCalculatorParametersForTesting(RatingCriteria criteria, FreightAutoRater rater)
			: base(criteria, rater)
		{
		}

		public AutoRatingCalculatorParametersForTesting(TestRatingCriteria criteria, AutoRateInfoCollection testInfos)
			: base(criteria, new FreightAutoRater(new RatingContext()))
		{
			SetResults_ForTest(testInfos);
		}

		public override Quantity GetChargeableAmount(IRateLine line)
		{
			return chargeableAmount ?? base.GetChargeableAmount(line);
		}

		public Quantity ChargeableAmount
		{
			set { chargeableAmount = value; }
		}

		Quantity? chargeableAmount;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		public void SetTime(int days, int hours, int minutes = 0)
		{
			((TestRatingCriteria)Criteria).SetTime(new TimeInfo(days, hours, minutes));
		}

		public override IEnumerable<IRateableContainer> GetChargeableContainers(IRateLine line)
		{
			if (chargeableAmount.HasValue && chargeableAmount.Value.Unit == QuantityUnit.CN)
			{
				var measures = new RateableMeasureSet(AdapterType.Shipment);
				new TestContainers(Factory, ZGuid.Empty, (int)chargeableAmount.Value.Amount)
					.PopulateContainerList(measures);
				return measures.GetAllContainers();
			}

			return base.GetChargeableContainers(line);
		}

		internal void AddExistingChargeForTest(AccChargeCode chargeCode, Money revenueValue, bool isRevenuePosted = false)
		{
			var testFactory = new BusinessObjectFactory();

			var existingCharges = new List<IAutoRatingChargeInfo>();
			existingCharges.AddRange(Criteria.GetExistingCharges());

			var newCharge = testFactory.New<TestCharge>();
			if (revenueValue.Currency.Code == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
			{
				newCharge.JR_LocalSellAmt = revenueValue.Amount;
			}
			else
			{
				newCharge.JR_OSSellAmt = revenueValue.Amount;
				newCharge.JR_RX_NKSellCurrency = revenueValue.Currency.Code;
			}

			newCharge.JR_AC = chargeCode.PK;

			if (isRevenuePosted)
			{
				var arLine = testFactory.New<AccTransactionLines>();
				arLine.AL_LineType = TransactionLineTypes.Revenue;
				newCharge.JR_AL_ARLine = arLine.PK;
				newCharge.JR_Calc_SellRatingBehavior = JobChargeLookups.StopFromAutorating;
			}

			existingCharges.Add(newCharge);
			((TestRatingCriteria)Criteria).SetExistingCharges(existingCharges.ToArray());
		}
	}

	internal sealed class CalculatorReflectionTest : TestCaseWithFactory
	{
		public void TestMapToAndReleatedToAreMappedToSameTypeProperties()
		{
			var allCalculatorTypes = Assembly.GetAssembly(typeof(Calculator))
											 .GetTypes()
											 .Where(type => type.IsClass
															&& !type.IsAbstract
															&& type.IsSubclassOf(typeof(Calculator))
															&& type != typeof(NullCalculator)
															&& !type.IsTestCalculator())
											 .ToArray();

			Assert(allCalculatorTypes.Length > 0);

			foreach (var calculatorType in allCalculatorTypes)
			{
				var calculatorPropertyAttributes = calculatorType.GetCustomAttributes(typeof(CalculatorPropertyAttribute), false)
																 .Cast<CalculatorPropertyAttribute>()
																 .Where(x => !string.IsNullOrEmpty(x.RelatedTo) && !string.IsNullOrEmpty(x.MapTo))
																 .ToArray();

				foreach (var calculatorPropertyAttribute in calculatorPropertyAttributes)
				{
					var releatedToPropertyType = calculatorType.GetProperty(calculatorPropertyAttribute.RelatedTo).PropertyType;
					var mapToPropertyType = calculatorType.GetProperty(calculatorPropertyAttribute.MapTo).PropertyType;

					AssertEquals(mapToPropertyType, releatedToPropertyType);
				}
			}
		}
	}

	public static class CalculatorExtensions
	{
		public static (IEnumerable<CalculationResult> results, string error) Calculate(this Calculator calculator, RatingCriteria criteria)
		{
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			return calculator.Calculate(parameters);
		}
	}
}
