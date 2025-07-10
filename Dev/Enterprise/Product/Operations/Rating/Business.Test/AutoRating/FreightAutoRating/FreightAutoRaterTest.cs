using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Billing.Business;
using Enterprise.Billing.Business.Testing;
using Enterprise.Core;
using Enterprise.Customs.Common.Shared;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WiseRates.Constants;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	/*
	 * ATTENTION
	 * This file is FULL
	 * Do not add fuel to the fire. Please extract stuff out of this file into
	 * a separate test file instead of adding just-one-more test
	 *
	 * Thank you.
	 */
	public class FreightAutoRaterTest : RatingTestCase
	{
		#region Packs Weight

		public void TestPacksWeight()
		{
			var product = Helper.NewOrgSupplierPart(Helper.NewOrgHeader());
			Helper.SetProductWeightAndVolume(product, weight: 2m, weightUQ: Weight.Kilograms, volume: 1m, volumeUQ: Volume.CubicMetres);
			Helper.AddPartUnit(product, package: PkgUnit.Unit, parentPackage: PkgUnit.Pallet, quantityInParent: 2m);

			var clientRate = Helper.NewClientRate(NewClient);
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
			var measures = criteria.RateableMeasures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false,
				optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			measures.AddWarehouseDocketNormalLine((10m, null), (0, null), 10m, ZGuid.Empty, product.PK, ProductAttributesMeasure.Empty, "", "", PkgUnit.Pallet);

			var testAutoRater = new FreightAutoRater(new RatingContext());

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var results = testAutoRater.AutoRate(criteria, CostSell.Revenue);
				AssertRatingResults
				(
					expected: new[]
					{
						new SimpleArInfo
						{
							Amount = 50m,
							InvoiceLineDesc = "Destination Documentation Fee - PROD1 (###1)",
							CalculationSingleLineDescription = "DDOC: 10 Kilogram(s) @ AUD 5.00/KG (for 4 KG/PLT Packs Weight)"
						}
					},
					results
				);
			}
		}

		public void TestPacksWeight_DifferentPackTypeSameProducts()
		{
			var product = Helper.NewOrgSupplierPart(Helper.NewOrgHeader());
			Helper.SetProductWeightAndVolume(product, weight: 2m, weightUQ: Weight.Kilograms, volume: 1m, volumeUQ: Volume.CubicMetres);
			Helper.AddPartUnit(product, package: PkgUnit.Unit, parentPackage: PkgUnit.Pallet, quantityInParent: 2m);
			Helper.AddPartUnit(product, package: PkgUnit.Unit, parentPackage: PkgUnit.Crate, quantityInParent: 3m);

			var clientRate = Helper.NewClientRate(NewClient);
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
			var measures = criteria.RateableMeasures;
			measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false,
				optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			measures.AddWarehouseDocketNormalLine((10m, null), (0, null), 10m, ZGuid.Empty, product.PK, ProductAttributesMeasure.Empty, "", "", PkgUnit.Pallet);
			measures.AddWarehouseDocketNormalLine((20m, null), (0, null), 10m, ZGuid.Empty, product.PK, ProductAttributesMeasure.Empty, "", "", PkgUnit.Crate);

			var testAutoRater = new FreightAutoRater(new RatingContext());

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var results = testAutoRater.AutoRate(criteria, CostSell.Revenue);
				AssertRatingResults
				(
					"GIVEN same product with different PackType WHEN autorate THEN the packType should be calculated separately",
					expected: new[]
					{
					new SimpleArInfo
					{
						Amount = 150m,
						InvoiceLineDesc = "Destination Documentation Fee - PROD1 (###1)",
						CalculationSingleLineDescription = "DDOC: 10 Kilogram(s) @ AUD 5.00/KG (for 4 KG/PLT Packs Weight)\r\nDDOC: 20 Kilogram(s) @ AUD 5.00/KG (for 6 KG/CRT Packs Weight)"
					},
					},
					results
				);
			}
		}

		#endregion

		#region BadClientRatesWithEmptyClientWouldNeverLoadIfExistInDB

		public void TestBadClientRatesWithEmptyClientWouldNeverLoadIsExistInDB()
		{
			var org = Helper.NewOrgHeader();

			var rate = Helper.NewClientRate(null);
			var rateEntry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "KRSEL", "", "");

			var rateLine = rateEntry.RateLines[0];
			rateLine.TL_RateCalculator = UnitCalculator.Code;
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 5m;

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "KRSEL", FreightMode.LSE, null, 1000m, 0m, org);
			var autoRater = new FreightAutoRater(new RatingContext());
			var results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);

			var expected = Enumerable.Empty<SimpleArInfo>();
			AssertRatingResults(expected, results);

			rate.TH_OH = org.PK;

			Factory.Save();

			autoRater = new FreightAutoRater(new RatingContext());

			results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);
			expected = new[]
						{
							new SimpleArInfo
								{
									Amount = 5000m,
									InvoiceLineDesc = "International Freight",
									CalculationSingleLineDescription = "FRT: 1000 Kilogram(s) @ AUD 5.00/KG"
								}
						};

			AssertRatingResults(expected, results);
		}

		#endregion

		#region NoMoreDoubling

		public void TestWithEmptyCommodityOnCompanyTariffAndCTBClientRate()
		{
			var tariff1 = Helper.NewCompanyTariff();
			var tariffEntry = tariff1.AddRateEntry("DST", "AIR", "", "AUSYD", "", "");
			tariffEntry.TI_RH_NKCommodityCode = ZString.Empty;
			tariffEntry.RateLines.RemoveAndDeleteAll();

			var tariffLine1 = tariffEntry.AddRateLine("DDOC", UnitCalculator.Code, QuantityUnit.KG);
			tariffLine1.Calculator[UnitCalculator.Items.Operator.UNT] = (ZDecimal)5m;

			var tariffLine2 = tariffEntry.AddRateLine("DSEC", FlatCalculator.Code);
			tariffLine2.Calculator[FlatCalculator.Items.Operator.BAS] = (ZDecimal)100m;

			tariff1.Factory.Save();

			var org1 = Helper.NewOrgHeader(1);
			var org2 = Helper.NewOrgHeader(1);

			var rate = Helper.NewClientRate(org1);
			var rateEntry = rate.AddRateEntry("DST", "AIR", "", "AUSYD", "", "");
			rateEntry.TI_RH_NKCommodityCode = "ALUM";
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("DDOC", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			rateLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().BaseRate = -25m;

			var rateLine2 = rateEntry.AddRateLine("DSEC", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			rateLine2.GetCalculator<CompanyTariffOrCostBasedCalculator>().BaseRate = -5m;

			Factory.Save();

			var testObject = new AutoRatingObject("USLAX", "AUSYD", FreightMode.LSE, null, 0m, 0m, rate.Header);
			testObject.DebtorOrgs[RatingDebtorOrgTypes.AG] = org2;
			((RateableMeasureSet)testObject.RateableMeasures).SetWeightWithCommodity(100m, Core.Constants.Weight.Kilograms, (ZString)"ALUM");

			var autoRater = new FreightAutoRater(new RatingContext());
			var results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Result Count", 2, results.Count);

			var info = results.First(c => c.ChargeCode.PK == Helper.ChargeCodes["DDOC"].PK);
			var info2 = results.First(c => c.ChargeCode.PK == Helper.ChargeCodes["DSEC"].PK);

			AssertEquals(475m, info.Amount);
			AssertEquals(95m, info2.Amount);
		}

		#endregion

		#region Consignee Charges on Origin Rates

		public void TestConsigneeRatesOnOriginRate()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var otherConsignee = Factory.NewWithValidTestData<OrgHeader>();
			var otherConsignor = Factory.NewWithValidTestData<OrgHeader>();

			var rate = Helper.NewClientRate(org1);
			var orgEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.AIR, "INBOM", "");
			orgEntry.TI_OH_Consignee = consignee.PK;
			orgEntry.TI_OH_Consignor = consignor.PK;
			var line = orgEntry.AddRateLine(TestCAF.AC_Code, FlatCalculator.Code);
			line.Calculator[FlatCalculator.Items.Operator.BAS] = (ZDecimal)500m;

			var orgEntry2 = rate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.AIR, "INBOM", "");
			orgEntry2.TI_OH_Consignor = consignor.PK;
			var line2 = orgEntry2.AddRateLine(TestCAF.AC_Code, FlatCalculator.Code);
			line2.Calculator[FlatCalculator.Items.Operator.BAS] = (ZDecimal)1300m;

			Factory.Save();

			var testObject = new AutoRatingObject("INBOM", "USLAX", FreightMode.LSE, null, 200M, 0M, org1);
			testObject.Consignee = consignee;
			testObject.Consignor = otherConsignor;

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 0, results.Count);

			testObject.Consignor = consignor;
			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 1, results.Count);

			var info = results.First(r => r.ChargeCode.PK == TestCAF.PK);
			AssertEquals("Sell Rate", 500m, info.Amount);

			testObject.Consignee = otherConsignee;
			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 1, results.Count);

			info = results.First(r => r.ChargeCode.PK == TestCAF.PK);
			AssertEquals("Sell Rate", 1300m, info.Amount);
		}

		#endregion

		#region Weight/Volume Duplication

		public void TestAutoRate_SameChargeDifferentUnit_AutoRateBoth()
		{
			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.AIR, "USLAX", GlbBranch.CurrentBranch.GB_RL_NKHomePort);
			rateEntry.TI_RH_NKCommodityCode = "GEN";
			rateEntry.AddUnitRateLine("DDOC", 100, "KG");
			rateEntry.AddUnitRateLine("DDOC", 5, "M3");

			Factory.Save();

			var autoRater = new FreightAutoRater(new RatingContext());
			var autoRatingObject = new AutoRatingObject("USLAX", GlbBranch.CurrentBranch.GB_RL_NKHomePort, FreightMode.LSE, null, 2750M, 1.5M, client);
			var measures = (RateableMeasureSet)autoRatingObject.RateableMeasures;
			var parts = new RateablePartList { WeightUnit = "KG", VolumeUnit = "M3", HasCommodity = true };
			parts.AddPart(new RateablePart { CommodityCode = "GEN", Weight = 100, Volume = 2m });
			parts.AddPart(new RateablePart { CommodityCode = "HAZ", Weight = 30, Volume = 4 }); // Should not be used as commodity doesn't match
			measures.AddPartList(MeasureType.Weight, parts);
			measures.AddPartList(MeasureType.Volume, parts);

			var results = autoRater.AutoRate(new AutoRatingProxy(autoRatingObject), CostSell.Revenue).RateInfoCollection;

			var descriptions = results[0].SingleLineDescription.Split('\n').Select(desc => (string)desc.Trim()).ToArray();
			// 333.333 KG is 2 M3 converted to KG using standard volume to weight conversion factors.
			AssertContainsExactElementsInAnyOrder(new[] { "DDOC: 333.333 Kilogram(s) @ AUD 100.00/KG", "DDOC: 2 Cubic Meter(s) @ AUD 5.00/M3" }, descriptions);
			AssertEquals(33343.30m, results[0].Amount);
		}

		#endregion

		#region No Empty Charge Line

		public void TestMinimumCalcLineShouldBeExcludedIfNotApplied()
		{
			var org = Helper.NewOrgHeader();

			var product = Helper.NewOrgSupplierPart(org);
			var partUnit = product.PartUnits.AddNew();
			partUnit.OF_QuantityInParent = 10m;
			partUnit.OF_PackType = "UNT";
			partUnit.OF_ParentPackType = "BOX";

			var rate = Helper.NewClientRate(org);
			var entry = rate.AddRateEntry("WHS", "ALL", "", "");

			var line1 = entry.AddRateLine("ODOC", WarehousePackCalculator.Code, Constants.PkgUnit.Unit);
			var item1 = line1.RateLineItems.AddNew();
			item1.TM_Type = "BOX";
			item1.TM_Break = 0m;
			item1.TM_Value = 15m;

			var line2 = entry.AddRateLine("ODOC", MinimumCalculator.Code);
			line2.Calculator[MinimumCalculator.Items.MIN] = (ZDecimal)20m;
			line2.Calculator[MinimumCalculator.Items.MinimumType] = (ZString)CalculatorConstants.Text.MIN_ChargeCode;

			Factory.Save();

			var testObject = new AutoRatingObject();
			testObject.ConsumerType = JobInvoicingConsumerTypes.WarehouseInwards;
			testObject.RateTypeToUse = RateType.Warehouse;
			testObject.Measures.SetQuantity(MeasureType.Weight, 0m, Core.Constants.Weight.Kilograms);
			testObject.Measures.SetQuantity(MeasureType.Volume, 0m, Core.Constants.Volume.CubicMetres);
			testObject.DebtorOrgs[RatingDebtorOrgTypes.LC] = org;

			testObject.Measures.CreateWarehouseDocketLines(null, true, false, RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes | RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType);
			// 20 / 10 x 15 = $30
			testObject.Measures.AddWarehouseDocketNormalLine((0m, null), (0, null), 20m, ZGuid.Empty, product.PK, new ProductAttributesMeasure("AA1", "BB1", "CC1"), "", "X00004205", "");
			testObject.Measures.SetQuantityWithWarehouseDocket(MeasureType.Shipment, 1m, ZString.Empty, ZGuid.Empty, "X00004205");

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);

			var expected = new[]
			{
				new SimpleArInfo
					{
						Amount = 30m,
						InvoiceLineDesc = "Origin Documentation Fee X00004205 - PROD1 (###1) AA1 BB1 CC1",
						CalculationSingleLineDescription = "ODOC: 2 Box (Product PROD1) @ AUD 15.00/BOX"
					}
			};
			AssertRatingResults(expected, results);

			line2.Calculator[MinimumCalculator.Items.MinimumType] = (ZString)CalculatorConstants.Text.MIN_Job;
			Factory.Save();
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);
			AssertRatingResults(expected, results);
		}

		#endregion

		#region Conflicting Rates

		[TestDate(2020, 01, 01)]
		public void TestConflictingRates_RatesService()
		{
			Helper.ChargeCodes.New("TSTFUM", "Fumigation Service", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin, FreightServiceType.Codes.Fumigation);

			var costing = Helper.CreateTestRate(TransportModes.Sea, ContainerModes.FCL, "AUSYD", "USLAX", "", "", "", "Boaty", NewClient.OH_Code, "");
			costing.Charges.Add(Helper.CreatePerUnitCharge("TSTFUM", QuantityUnit.SV, "USD", 10));
			costing.Charges.Add(Helper.CreatePerUnitCharge("TSTFUM", QuantityUnit.SV, "AUD", 20));

			var shipment = TestHelper.CreateForwardingShipment(Factory, TransportModes.Sea, ContainerModes.FCL, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", weight: 100m, volume: 10m);
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(10);
			var consol = TestHelper.CreateForwardingConsol(TransportModes.Sea, "AUSYD", "USLAX", TransportProvider1, shipment);
			consol.Transports[0].CreditorPK = TransportProvider1.PK;

			var fumigationService = shipment.DocsAndCartage.Services.AddNew();
			fumigationService.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			fumigationService.ES_ServiceCount = 1;
			fumigationService.ES_Duration = new TimeSpan(2, 30, 0);
			fumigationService.ES_Completed = ZDateTime.Today;

			Factory.Save();

			var costings = new[] { costing };
			var testContext = new MockRatesServiceContext(Helper.CreateSEARatesSearchResponse(costings, costings, carrierCode: "Boaty"));

			var testAutoRater = new FreightAutoRater(testContext);
			var results = testAutoRater.AutoRate(new AutoRatingProxy(shipment.RatingAdapter), CostSell.Cost).RateInfoCollection;
			AssertRatingResults
			(
				expected: new[]
				{
					new SimpleArInfo
					{
							InvoiceLineDesc = @"RATE NOTE: Fumigation Service
	Charge cannot be calculated due to conflicting rates found.",
					},
				},
				results
			);
		}

		[TestDate(2011, 12, 1)]
		public void TestConflictingRates()
		{
			var tariff = Helper.NewCompanyTariff();

			var tariffEntry1 = tariff.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			tariffEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			var tariffLine1 = tariffEntry1.AddRateLine("DDOC", FlatCalculator.Code);
			tariffLine1.Calculator[FlatCalculator.Items.Operator.BAS] = (ZDecimal)500m;

			var tariffEntry2 = tariff.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.SEA, "AUSYD", "USLAX", "", "40GP");
			tariffEntry2.TI_RH_NKCommodityCode = ZString.Empty;
			var tariffLine2 = tariffEntry2.AddRateLine("DDOC", FlatCalculator.Code);
			tariffLine2.Calculator[FlatCalculator.Items.Operator.BAS] = (ZDecimal)500m;

			var org = Helper.NewOrgHeader(1);
			var rate = Helper.NewClientRate(org);

			var rateEntry1 = rate.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			var rateLine1 = rateEntry1.AddRateLine("DDOC", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);

			var rateEntry2 = rate.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.SEA, "AUSYD", "USLAX", "", "40GP");
			rateEntry2.TI_RH_NKCommodityCode = ZString.Empty;
			var rateLine2 = rateEntry2.AddRateLine("DDOC", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);

			tariffEntry1.TI_RateStartDate = new ZDate(2011, 12, 1);
			tariffEntry2.TI_RateStartDate = new ZDate(2011, 12, 1);
			rateEntry1.TI_RateStartDate = new ZDate(2011, 12, 1);
			rateEntry2.TI_RateStartDate = new ZDate(2011, 12, 1);

			tariffEntry1.TI_RateEndDate = ZDate.Empty;
			tariffEntry2.TI_RateEndDate = ZDate.Empty;
			rateEntry1.TI_RateEndDate = ZDate.Empty;
			rateEntry2.TI_RateEndDate = ZDate.Empty;

			tariff.Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.FCL, new TestContainers(Factory, "20GP", 3, "40GP", 2), 0m, 0m, org);

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);

			var expected = new[]
				{
					new SimpleArInfo
						{
							Amount = 500m,
							InvoiceLineDesc = "Destination Documentation Fee",
							CalculationSingleLineDescription = "DDOC: Base Rate USD 500.00",
						}
				};

			AssertRatingResults(expected, results);

			((CompanyTariffOrCostBasedCalculator)rateLine1.Calculator).Percent = 5m;
			((CompanyTariffOrCostBasedCalculator)rateLine2.Calculator).Percent = 5m;
			tariffLine1.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)490m;
			tariffLine2.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)510m;
			tariff.Factory.Save();

			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);

			expected = new[]
			{
				new SimpleArInfo
					{
						InvoiceLineDesc = @"RATE NOTE: Destination Documentation Fee
	Charge cannot be calculated due to conflicting rates found.",
					}
			};

			AssertRatingResults(expected, results);

			tariffLine1.Calculator[FlatCalculator.Items.Operator.BAS] = (ZDecimal)500m;
			tariffLine2.Calculator[FlatCalculator.Items.Operator.BAS] = (ZDecimal)500m;
			tariff.Factory.Save();

			Factory.Save();

			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);

			expected = new[]
			{
				new SimpleArInfo
					{
						Amount = 525m,
						InvoiceLineDesc = "Destination Documentation Fee",
						CalculationDescription = "DDOC: 105.00% of (Base Rate USD 500.00)",
					}
			};

			AssertRatingResults(expected, results);

			((CompanyTariffOrCostBasedCalculator)rateLine1.Calculator).Percent = 6m;
			((CompanyTariffOrCostBasedCalculator)rateLine2.Calculator).Percent = 7m;

			Factory.Save();

			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);

			expected = new[]
				{
					new SimpleArInfo
						{
							InvoiceLineDesc = @"RATE NOTE: Destination Documentation Fee
	Charge cannot be calculated due to conflicting rates found.",
						}
				};

			AssertRatingResults(expected, results);

			AssertNotNull("Expected autorating to return at least one active result", results.RateInfoCollection[0]);

			var expectedCalculationDescription = @"DDOC: Base Rate USD 0.00
Rate Note: Value must be manually specified.

Destination Documentation Fee

Charge located in TESTORG1 client rate with the following details:

Payment Term:		Collect
Mode:			SEA
Charge Code Group:	DST
Start Date:		01 December 2011
Origin:			AUSYD
Destination:		USLAX
Container:		20GP
Currency:		USD
Autorated for:		record
Leg:			AUSYD-USLAX";

			AssertContains("Expected ratingAudit to contain this description", expectedCalculationDescription, results.RateInfoCollection[0].Description);
		}

		#endregion

		#region Company Tariff Incoterms Overrides

		public void TestCompanyTariffIncotermsOverrides()
		{
			//Overseas USLAX side
			var consignor = Helper.NewOrgHeader();
			var agent = Helper.NewOrgHeader();

			//Local side
			var consignee = Helper.NewOrgHeader(1);
			var client = Helper.NewOrgHeader();

			var cnrRate = Helper.NewClientRate(consignor);
			var cnrEntry = cnrRate.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.AIR, "USLAX", GlbBranch.CurrentBranch.GB_RL_NKHomePort);
			var cnrLine = cnrEntry.AddRateLine("DDOC", FlatCalculator.Code);
			cnrLine.Calculator[FlatCalculator.Items.Operator.BAS] = (ZDecimal)800m;

			var tariff = Helper.NewCompanyTariff();
			var tariffEntry = tariff.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.AIR, "USLAX", GlbBranch.CurrentBranch.GB_RL_NKHomePort);
			var tariffLine = tariffEntry.AddRateLine("DDOC", FlatCalculator.Code);
			tariffLine.Calculator[FlatCalculator.Items.Operator.BAS] = (ZDecimal)500m;
			tariff.Factory.Save();

			Factory.Save();

			var autoRater = new FreightAutoRater(new RatingContext());
			var testObject = new AutoRatingObject("USLAX", GlbBranch.CurrentBranch.GB_RL_NKHomePort, FreightMode.LSE, null, 2750M, 1.5M, client);
			testObject.DebtorOrgs[RatingDebtorOrgTypes.AG] = agent;
			testObject.Consignee = consignee;
			testObject.Consignor = consignor;

			testObject.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "EXW"));
			var results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("1 result", 1, results.Count);
			AssertEquals("Consignee charge taken as INCOTERM was EXW, the Consignor charge should not be used", 500m, results[0].Amount);

			testObject.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "CPT"));
			autoRater = new FreightAutoRater(new RatingContext());
			results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("1 result", 1, results.Count);
			AssertEquals("Consignor charge taken as INCOTERM was CPT, so it takes precendence over Consignee as DST charges are paid by the Consignor", 800m, results[0].Amount);
		}

		#endregion

		#region Agent Rates Calculation

		public void TestAgentRatesCalculation()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			testRate.TH_OH = NewClient.PK;

			var aIRRateEntry1 = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "", "");
			aIRRateEntry1.RateLines.RemoveAndDeleteAll();

			var aIRRateLine1b = aIRRateEntry1.AddRateLine(TestBAF.AC_Code, FlatCalculator.Code);

			((FlatCalculator)aIRRateLine1b.Calculator).BaseRate = 50m;
			aIRRateLine1b.ViewAgentRates = true;
			((FlatCalculator)aIRRateLine1b.Calculator).BaseRate = 30m;

			aIRRateLine1b.ViewAgentRates = false;

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 100M, .5M, testRate.Header);

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("ViewAgentRates", false, aIRRateLine1b.ViewAgentRates);
			AssertEquals("Result Count", 1, results.Count);

			var info = results.First(r => r.ChargeCode.PK == TestBAF.PK);
			AssertEquals("Sell Rate", 50M, info.Amount);
			AssertEquals("Agent Sell Rate", 30M, info.AgentAmount);
			AssertEquals("Sell Currency", "AUD", info.Currency);
		}

		#endregion

		#region Percentage Calculator Looped

		public void TestPercentageCalculatorLoopedCalculation()
		{
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var testObject = new AutoRatingObject("AUSYD", "GBLON", FreightMode.LCL, null, 2750M, 1.5M, testRate.Header);

			var oRGEntry = testRate.AddRateEntry("ORG", "LCL", "AUSYD", "GBLON");
			var oRGLine1 = oRGEntry.AddRateLine("OAWB", PercentageCalculator.Code);
			oRGLine1.GetCalculator<PercentageCalculator>().Percent = 10m;
			oRGLine1.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.FreightCharges);
			oRGEntry.AddRateLine("ODOC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 500m;

			var fRTEntry = testRate.AddRateEntry("LCL", "LCL", "AUSYD", "GBLON");
			fRTEntry.RateLines.RemoveAndDeleteAll();
			fRTEntry.TI_RX_NKCurrency = "AUD";
			var fRTLine1 = fRTEntry.AddRateLine("CAF", PercentageCalculator.Code);
			fRTLine1.GetCalculator<PercentageCalculator>().Percent = 5m;
			fRTLine1.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.OriginCharges);
			var fRTLine2 = fRTEntry.AddRateLine("BAF", FlatCalculator.Code);
			fRTLine2.GetCalculator<FlatCalculator>().BaseRate = 200m;

			testRate.Factory.Save();

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			var info1 = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["ODOC"].PK);
			var info2 = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["BAF"].PK);
			var info3 = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["OAWB"].PK);
			var info4 = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["CAF"].PK);

			AssertEquals("ORG ODOC flat amount", 500M, info1.Amount);
			AssertEquals("FRT BAF flat amount", 200M, info2.Amount);
			AssertEquals("ORG OAWB 10% from FRT(200)", 20M, info3.Amount);
			AssertEquals("FRT CAF 5% from ORG(500)", 25M, info4.Amount);
			AssertEquals("Result Count", 4, results.Count);

			// Test results remain the same after a second autorating run
			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			info1 = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["ODOC"].PK);
			info2 = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["BAF"].PK);
			info3 = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["OAWB"].PK);
			info4 = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["CAF"].PK);

			AssertEquals("ORG ODOC flat amount", 500M, info1.Amount);
			AssertEquals("FRT BAF flat amount", 200M, info2.Amount);
			AssertEquals("ORG OAWB 10% from FRT(200)", 20M, info3.Amount);
			AssertEquals("FRT CAF 5% from ORG(500)", 25M, info4.Amount);
			AssertEquals("Result Count", 4, results.Count);

			// Test results change if existing charge is present
			var existingCharge = Factory.NewWithValidTestData<TestCharge>();
			existingCharge.JR_AC = Helper.ChargeCodes["OCART"].PK;
			existingCharge.JR_LocalSellAmt = 500m;

			testObject.SetExistingCharges(new[] { existingCharge });

			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			info1 = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["ODOC"].PK);
			info2 = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["BAF"].PK);
			info3 = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["OAWB"].PK);
			info4 = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["CAF"].PK);

			AssertEquals("ORG ODOC flat amount", 500M, info1.Amount);
			AssertEquals("FRT BAF flat amount", 200M, info2.Amount);
			AssertEquals("ORG OAWB 10% from FRT(200)", 20M, info3.Amount);
			AssertEquals("FRT CAF 5% from ORG ODOC(500) + ORG OCART preexisting (500)", 50M, info4.Amount);
			AssertEquals("Result Count", 4, results.Count);
		}

		public void TestPercentageCalculatorLoopedSimplePercentage()
		{
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1 = testRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "20GP");
			entry1.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 1000m;

			var line1b = entry1.AddRateLine("BAF", PercentageCalculator.Code);
			line1b.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = Helper.ChargeCodes["FRT"].PK;
			line1b.GetCalculator<PercentageCalculator>().Percent = 5m;

			var line1c = entry1.AddRateLine("CAF", PercentageCalculator.Code);
			line1c.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.FreightCharges);
			line1c.GetCalculator<PercentageCalculator>().Percent = 10m;

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.FCL, new TestContainers(Factory, "20GP", 3), 0m, 0m, testRate.Header);

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 3, results.Count);

			var info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["FRT"].PK);
			AssertEquals("Sell Amount", 3000m, info.Amount);

			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["BAF"].PK);
			AssertEquals("Sell Amount", 150m, info.Amount);

			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["CAF"].PK);
			AssertEquals("Sell Amount", 315m, info.Amount);
		}

		public void TestPercentageMinimumAndBaseRateAreNotAppliedWhenBaseAmountIsZero()
		{
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = testRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "20GP");
			var line1 = entry.RateLines[0];
			line1.GetCalculator<UnitCalculator>().PerUnit = 0m;

			var line2 = entry.AddRateLine("BAF", PercentageCalculator.Code);
			line2.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = Helper.ChargeCodes["FRT"].PK;
			line2.GetCalculator<PercentageCalculator>().Percent = 5m;
			line2.GetCalculator<PercentageCalculator>().Minimum = 500;
			line2.GetCalculator<PercentageCalculator>().BaseRate = 100;

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.FCL, new TestContainers(Factory, "20GP", 3), 0m, 0m, testRate.Header);
			var testAutoRater = new FreightAutoRater(new RatingContext());

			var expected = new[]
				{
					new SimpleArInfo
						{
							InvoiceLineDesc = "International Freight",
							Amount = 0m,
							CalculationSingleLineDescription = "FRT: 3 20GP Container(s) @ USD 0.00/Container"
						},
					new SimpleArInfo
						{
							InvoiceLineDesc = "Bunker Adjustment Factor",
							Amount = 0m,
							CalculationDescription = "BAF: 5.00% of (USD 0.00 (FRT))"
						}
				};

			AssertRatingResults(expected, testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue));

			line1.GetCalculator<UnitCalculator>().PerUnit = 100m;

			Factory.Save();

			expected = new[]
				{
					new SimpleArInfo
						{
							InvoiceLineDesc = "International Freight",
							Amount = 300m,
							CalculationSingleLineDescription = "FRT: 3 20GP Container(s) @ USD 100.00/Container"
						},
					new SimpleArInfo
						{
							InvoiceLineDesc = "Bunker Adjustment Factor",
							Amount = 500m,
							CalculationDescription = "BAF: Minimum USD 500.00"
						}
				};

			AssertRatingResults(expected, testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue));

			line1.GetCalculator<UnitCalculator>().PerUnit = 200m;

			Factory.Save();

			expected = new[]
				{
					new SimpleArInfo
						{
							InvoiceLineDesc = "International Freight",
							Amount = 600m,
							CalculationSingleLineDescription = "FRT: 3 20GP Container(s) @ USD 200.00/Container"
						},
					new SimpleArInfo
						{
							InvoiceLineDesc = "Bunker Adjustment Factor",
							Amount = 500m,
							CalculationDescription = "BAF: Minimum USD 500.00"
						}
				};

			AssertRatingResults(expected, testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue));

			line1.GetCalculator<UnitCalculator>().PerUnit = 20000m;

			Factory.Save();

			expected = new[]
				{
					new SimpleArInfo
						{
							InvoiceLineDesc = "International Freight",
							Amount = 60000m,
							CalculationSingleLineDescription = "FRT: 3 20GP Container(s) @ USD 20000.00/Container"
						},
					new SimpleArInfo
						{
							InvoiceLineDesc = "Bunker Adjustment Factor",
							Amount = 3100m,
							CalculationDescription = "BAF: Base Rate USD 100.00 + 5.00% of (USD 60000.00 (FRT))"
						}
				};

			AssertRatingResults(expected, testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue));
		}

		public void TestCalculationOrder1()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);

			Factory.Save();

			var testCompanyTariff = Helper.NewCompanyTariff();
			testCompanyTariff.TH_GlobalRateLevel = 1;
			var companyTariffEntry = testCompanyTariff.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			companyTariffEntry.RateLines.RemoveAndDeleteAll();

			var companyTariffLine1 = companyTariffEntry.AddRateLine(TestFRT.AC_Code, UnitCalculator.Code, QuantityUnit.KG);
			companyTariffLine1.GetCalculator<UnitCalculator>().PerUnit = 50m;

			var companyTariffLine2 = companyTariffEntry.AddRateLine(TestCAF.AC_Code, PercentageCalculator.Code);
			companyTariffLine2.GetCalculator<PercentageCalculator>().Percent = 10m;

			var applyToItem = companyTariffLine2.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			applyToItem.TM_AC = TestFRT.PK;

			var orgTariffEntry = testCompanyTariff.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			orgTariffEntry.RateLines.RemoveAndDeleteAll();

			var orgCompanyTariffLine = orgTariffEntry.AddRateLine(TestAWB.AC_Code, PercentageCalculator.Code);
			orgCompanyTariffLine.GetCalculator<PercentageCalculator>().Percent = 20m;
			orgCompanyTariffLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.OriginCharges);

			testCompanyTariff.Factory.Save();

			var testRate = Helper.NewClientRate(NewClient);
			var entry = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();

			var line1 = entry.AddRateLine(TestCAF.AC_Code, CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			line1.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 10m;

			var line2 = entry.AddRateLine(TestFRT.AC_Code, CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			line2.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 0m;

			var line3 = entry.AddRateLine(TestBAF.AC_Code, PercentageCalculator.Code);
			line3.GetCalculator<PercentageCalculator>().Percent = 1m;
			line3.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.FreightCharges);

			var orgEntry = testRate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			orgEntry.RateLines.RemoveAndDeleteAll();

			var orgLine = orgEntry.AddRateLine(TestBBK.AC_Code, UnitCalculator.Code, QuantityUnit.KG);
			orgLine.GetCalculator<UnitCalculator>().PerUnit = 1m;

			var companyData = testRate.Header.GetCompanyDataForGlbCompany(GlbCompany.CurrentCompany);
			companyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 100M, 0.1M, "NEWTESSYD");

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			var info1 = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			var info2 = results.First(r => r.ChargeCode.PK == TestCAF.PK);
			var info3 = results.First(r => r.ChargeCode.PK == TestBAF.PK);
			var info4 = results.First(r => r.ChargeCode.PK == TestBBK.PK);
			var info5 = results.First(r => r.ChargeCode.PK == TestAWB.PK);

			AssertEquals("FRT Sell Amount", 5000M, info1.Amount);
			AssertEquals("CAF Sell Amount", 550M, info2.Amount);
			AssertEquals("BAF Sell Amount", 55.5M, info3.Amount);
			AssertEquals("BBK Sell Amount", 100M, info4.Amount);
			AssertEquals("AWB Sell Amount", 20M, info5.Amount);

			AssertEquals("Result Count", 5, results.Count);
		}

		public void TestCalculationOrder2()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);
			Factory.Save();

			var testCompanyTariff = Helper.NewCompanyTariff();
			testCompanyTariff.TH_GlobalRateLevel = 1;
			var companyTariffEntry = testCompanyTariff.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			companyTariffEntry.RateLines.RemoveAndDeleteAll();

			var orgTariffEntry = testCompanyTariff.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			orgTariffEntry.RateLines.RemoveAndDeleteAll();

			var orgCTLine = orgTariffEntry.AddRateLine(TestAWB.AC_Code, PercentageCalculator.Code);
			orgCTLine.GetCalculator<PercentageCalculator>().Percent = 20m;

			var applyToItem1 = orgCTLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			applyToItem1.TM_AC = TestBAF.PK;

			var testRate = Helper.NewClientRate(NewClient);
			var entry = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();

			var frtLine = entry.AddRateLine(TestFRT.AC_Code, UnitCalculator.Code, QuantityUnit.KG);
			frtLine.GetCalculator<UnitCalculator>().PerUnit = 50m;

			var bafLine = entry.AddRateLine(TestBAF.AC_Code, PercentageCalculator.Code);
			bafLine.GetCalculator<PercentageCalculator>().Percent = 1m;

			var applyToItem2 = bafLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			applyToItem2.TM_AC = TestFRT.PK;

			var companyData = testRate.Header.GetCompanyDataForGlbCompany(GlbCompany.CurrentCompany);
			companyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			testCompanyTariff.Factory.Save();
			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 100M, 0.1M, "NEWTESSYD");

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			var info1 = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			var info2 = results.First(r => r.ChargeCode.PK == TestBAF.PK);
			var info3 = results.First(r => r.ChargeCode.PK == TestAWB.PK);

			AssertEquals("FRT Sell Amount", 5000M, info1.Amount);
			AssertEquals("BAF Sell Amount", 50M, info2.Amount);
			AssertEquals("AWB Sell Amount", 10M, info3.Amount);

			AssertEquals("Result Count", 3, results.Count);
		}

		public void TestCalculationOrder3()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);
			Factory.Save();

			var testCompanyTariff = Helper.NewCompanyTariff();
			testCompanyTariff.TH_GlobalRateLevel = 1;
			var companyTariffEntry = testCompanyTariff.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			companyTariffEntry.RateLines.RemoveAndDeleteAll();

			var orgTariffEntry = testCompanyTariff.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			orgTariffEntry.RateLines.RemoveAndDeleteAll();

			var orgCTLine = orgTariffEntry.AddRateLine(TestAWB.AC_Code, PercentageCalculator.Code);
			orgCTLine.GetCalculator<PercentageCalculator>().Percent = 20m;

			var applyToItem1 = orgCTLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			applyToItem1.TM_AC = TestFRT.PK;

			var testRate = Helper.NewClientRate(NewClient);
			var entry = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();

			var frtLine = entry.AddRateLine(TestFRT.AC_Code, UnitCalculator.Code, QuantityUnit.KG);
			frtLine.GetCalculator<UnitCalculator>().PerUnit = 50m;

			var bafLine = entry.AddRateLine(TestBAF.AC_Code, PercentageCalculator.Code);
			bafLine.GetCalculator<PercentageCalculator>().Percent = 1m;

			var applyToItem2 = bafLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			applyToItem2.TM_AC = TestAWB.PK;

			var companyData = testRate.Header.GetCompanyDataForGlbCompany(GlbCompany.CurrentCompany);
			companyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			testCompanyTariff.Factory.Save();
			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 100M, 0.1M, "NEWTESSYD");

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			var info1 = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			var info2 = results.First(r => r.ChargeCode.PK == TestBAF.PK);
			var info3 = results.First(r => r.ChargeCode.PK == TestAWB.PK);

			AssertEquals("FRT Sell Amount", 5000M, info1.Amount);
			AssertEquals("BAF Sell Amount", 10M, info2.Amount);
			AssertEquals("AWB Sell Amount", 1000M, info3.Amount);

			AssertEquals("Result Count", 3, results.Count);
		}

		public void TestCalculationOrder4()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);
			Factory.Save();

			var testCompanyTariff = Helper.NewCompanyTariff();
			testCompanyTariff.TH_GlobalRateLevel = 1;
			var companyTariffEntry = testCompanyTariff.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			companyTariffEntry.RateLines.RemoveAndDeleteAll();

			var orgTariffEntry = testCompanyTariff.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			orgTariffEntry.RateLines.RemoveAndDeleteAll();

			var orgCTLine = orgTariffEntry.AddRateLine(TestAWB.AC_Code, PercentageCalculator.Code);
			orgCTLine.GetCalculator<PercentageCalculator>().Percent = 20m;

			orgCTLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.AllCharges);

			testCompanyTariff.Factory.Save();

			var testRate = Helper.NewClientRate(NewClient);
			var entry = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();

			var frtLine = entry.AddRateLine(TestFRT.AC_Code, UnitCalculator.Code, QuantityUnit.KG);
			frtLine.GetCalculator<UnitCalculator>().PerUnit = 50m;

			var bafLine = entry.AddRateLine(TestBAF.AC_Code, PercentageCalculator.Code);
			bafLine.GetCalculator<PercentageCalculator>().Percent = 1m;

			bafLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.AllCharges);

			var companyData = testRate.Header.GetCompanyDataForGlbCompany(GlbCompany.CurrentCompany);
			companyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 100M, 0.1M, "NEWTESSYD");

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			var info1 = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			var info2 = results.First(r => r.ChargeCode.PK == TestBAF.PK);
			var info3 = results.First(r => r.ChargeCode.PK == TestAWB.PK);

			AssertEquals("FRT Sell Amount", 5000M, info1.Amount);
			AssertEquals("BAF Sell Amount", 50M, info2.Amount);
			AssertEquals("AWB Sell Amount", 1000M, info3.Amount);

			AssertEquals("Result Count", 3, results.Count);

			// Test results remain the same after existing charge is present

			var exCh1 = Factory.NewWithValidTestData<TestCharge>();
			exCh1.JR_AC = TestFRT.PK;
			exCh1.JR_LocalSellAmt = 5000M;

			var exCh2 = Factory.NewWithValidTestData<TestCharge>();
			exCh2.JR_AC = TestBAF.PK;
			exCh2.JR_LocalSellAmt = 50M;

			var exCh3 = Factory.NewWithValidTestData<TestCharge>();
			exCh3.JR_AC = TestAWB.PK;
			exCh3.JR_LocalSellAmt = 1000M;

			testObject.SetExistingCharges(new[] { exCh1, exCh2, exCh3 });

			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			info1 = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			info2 = results.First(r => r.ChargeCode.PK == TestBAF.PK);
			info3 = results.First(r => r.ChargeCode.PK == TestAWB.PK);

			AssertEquals("FRT Sell Amount", 5000M, info1.Amount);
			AssertEquals("Since for BAF charge code, Percentage Calculator was used then the existing charge is counted in percentage calculation", 100m, info2.Amount);
			AssertEquals("Since for AWB charge code, Percentage Calculator was used then the existing charge is counted in percentage calculation", 2000M, info3.Amount);

			AssertEquals("Result Count", 3, results.Count);
		}

		public void TestCalculationOrder5()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);
			Factory.Save();

			var testCompanyTariff = Helper.NewCompanyTariff();
			testCompanyTariff.TH_GlobalRateLevel = 1;
			var companyTariffEntry = testCompanyTariff.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			companyTariffEntry.RateLines.RemoveAndDeleteAll();

			var orgTariffEntry = testCompanyTariff.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			orgTariffEntry.RateLines.RemoveAndDeleteAll();

			var orgCTLine = orgTariffEntry.AddRateLine(TestAWB.AC_Code, PercentageCalculator.Code);
			orgCTLine.GetCalculator<PercentageCalculator>().Percent = 20m;
			var applyToItem1 = orgCTLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode);
			applyToItem1.TM_AC = TestBAF.PK;
			orgCTLine.SetCalculationOrder(2);

			testCompanyTariff.Factory.Save();

			var testRate = Helper.NewClientRate(NewClient);
			var entry = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();

			var frtLine = entry.AddRateLine(TestFRT.AC_Code, UnitCalculator.Code, QuantityUnit.KG);
			frtLine.GetCalculator<UnitCalculator>().PerUnit = 50m;

			var bafLine = entry.AddRateLine(TestBAF.AC_Code, PercentageCalculator.Code);
			bafLine.GetCalculator<PercentageCalculator>().Percent = 1m;
			bafLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.AllCharges);
			bafLine.SetCalculationOrder(4);

			var companyData = testRate.Header.GetCompanyDataForGlbCompany(GlbCompany.CurrentCompany);
			companyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 100M, 0.1M, "NEWTESSYD");

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			var info1 = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			var info2 = results.First(r => r.ChargeCode.PK == TestBAF.PK);
			var info3 = results.First(r => r.ChargeCode.PK == TestAWB.PK);

			AssertEquals("FRT Sell Amount", 5000M, info1.Amount);
			AssertEquals("BAF Sell Amount", 50M, info2.Amount);
			AssertEquals("AWB Sell Amount", 0M, info3.Amount);

			AssertEquals("Result Count", 3, results.Count);

			// Test results remain the same after existing charge is present

			var exCh1 = Factory.NewWithValidTestData<TestCharge>();
			exCh1.JR_AC = TestFRT.PK;
			exCh1.JR_LocalSellAmt = 5000M;

			var exCh2 = Factory.NewWithValidTestData<TestCharge>();
			exCh2.JR_AC = TestBAF.PK;
			exCh2.JR_LocalSellAmt = 50M;

			var exCh3 = Factory.NewWithValidTestData<TestCharge>();
			exCh3.JR_AC = TestAWB.PK;
			exCh3.JR_LocalSellAmt = 0M;

			testObject.SetExistingCharges(new[] { exCh1, exCh2, exCh3 });

			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			info1 = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			info2 = results.First(r => r.ChargeCode.PK == TestBAF.PK);
			info3 = results.First(r => r.ChargeCode.PK == TestAWB.PK);

			AssertEquals("FRT Sell Amount", 5000M, info1.Amount);
			AssertEquals("Since for BAF charge code, Percentage Calculator was used then the existing charge is counted in percentage calculation", 100M, info2.Amount);
			AssertEquals("Even though BAF was there AWB didn't take it into account because of the CalculationPriority", 0M, info3.Amount);

			AssertEquals("Result Count", 3, results.Count);
		}

		public void TestPercentageCalculatorLoopedCalculationCompanyTariffBased()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);
			Factory.Save();

			var testCompanyTariff = Helper.NewCompanyTariff();
			testCompanyTariff.TH_GlobalRateLevel = 1;
			var companyTariffEntry = testCompanyTariff.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			companyTariffEntry.RateLines.RemoveAndDeleteAll();
			var companyTariffLine1 = companyTariffEntry.AddRateLine(TestFRT.AC_Code, UnitCalculator.Code, QuantityUnit.KG);
			companyTariffLine1.GetCalculator<UnitCalculator>().PerUnit = 5m;
			var companyTariffLine2 = companyTariffEntry.AddRateLine(TestCAF.AC_Code, PercentageCalculator.Code);
			companyTariffLine2.GetCalculator<PercentageCalculator>().Percent = 10m;
			companyTariffLine2.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.FreightCharges);

			testCompanyTariff.Factory.Save();

			var testRate = Helper.NewClientRate(NewClient);
			var entry = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();
			var line2 = entry.AddRateLine(TestFRT.AC_Code, CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			line2.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 0m;
			var line1 = entry.AddRateLine(TestCAF.AC_Code, CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			line1.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 10m;

			var companyData = testRate.Header.GetCompanyDataForGlbCompany(GlbCompany.CurrentCompany);
			companyData.RateTariffLevels.SetLevel(CompanyTariff.GetLevelCode("AIR"), 1);

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 100M, 0.1M, "NEWTESSYD");

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 2, results.Count);

			var info = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			AssertEquals("Sell Amount", 500M, info.Amount);

			info = results.First(r => r.ChargeCode.PK == TestCAF.PK);
			AssertEquals("Sell Amount", 55M, info.Amount);
		}

		#endregion

		#region AIR Freight Tests

		public void TestAutoRateAIR()
		{
			SetupExportClientRatesForAutoRater();
			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 55M, .5M, "NEWTESSYD");

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Result Count", 9, results.Count);

			var info = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			AssertEquals("FRT Chargeable Weight", 83.333M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", Core.Constants.Weight.Kilograms, info.GetChargeableFromBasisTest.Unit);
			AssertEquals("Sell Rate", 333.33M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestBAF.PK);
			AssertEquals("BAF Chargeable", 333.33M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("Chargeable", "AUD", info.GetChargeableFromBasisTest.Unit);
			AssertEquals("Sell Rate", 45.40M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestCAF.PK);
			AssertEquals("CAF Chargeable", 333.33M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("Chargeable", "AUD", info.GetChargeableFromBasisTest.Unit);
			AssertEquals("Sell Rate", 29.17M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestAWB.PK);
			AssertEquals("Sell Rate", 50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestBBK.PK);
			AssertEquals("FRT Chargeable Weight", 83.333M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", Core.Constants.Weight.Kilograms, info.GetChargeableFromBasisTest.Unit);
			AssertEquals("Sell Rate", 37.50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestTHC.PK);
			AssertEquals("Sell Rate", 18.50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestADF.PK);
			AssertEquals("Sell Rate", 50.00M, info.Amount);
			AssertEquals("Sell Currency", "USD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestITF.PK);
			AssertEquals("FRT Chargeable Weight", 83.333M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", Core.Constants.Weight.Kilograms, info.GetChargeableFromBasisTest.Unit);
			AssertEquals("Sell Rate", 125.00M, info.Amount);
			AssertEquals("Sell Currency", "USD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestLOL.PK);
			AssertEquals("FRT Chargeable Weight", 83.333M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", Core.Constants.Weight.Kilograms, info.GetChargeableFromBasisTest.Unit);
			AssertEquals("Sell Rate", 38.33M, info.Amount);
			AssertEquals("Sell Currency", "USD", info.Currency);
		}

		public void TestAutoRateULD()
		{
			SetupExportClientRatesForAutoRater();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.ULD, new TestContainers(Factory, "20GP", 3), 2.5M, 0M, "NEWTESSYD");

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			var info = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			AssertEquals("FRT Chargeable Weight", 3M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", "20GP", info.GetChargeableFromBasisTest.Unit);

			AssertContainsExactElementsInAnyOrder
			(
				new[]
				{
					"TESTFRT: 450.00 AUD",
					"TESTAWB: 0.00 AUD",
					"TESTBBK: 10.00 AUD",
					"TESTTHC: 18.50 AUD",
					"TESTADF: 50.00 USD",
					"TESTITF: 25.00 USD",
					"TESTLOL: 6.00 USD"
				},
				results.Select(result => $"{result.ChargeCode.AC_Code}: {result.Amount} {result.Currency}")
			);
		}

		public void TestAutoRateAIRWithALLModeOnOriginCharges()
		{
			var testRate = SetupExportClientRatesForAutoRater();
			var originEntry = testRate.AddRateEntry("ORG", "ALL", "AUSYD", "", "", "");
			var originRateLine = originEntry.AddRateLine(TestANY.AC_Code, FlatCalculator.Code);
			originRateLine.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS).TM_Value = 35.00M;

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 55M, .5M, "NEWTESSYD");

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Result Count", 10, results.Count);

			var info = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			AssertEquals("FRT Chargeable Weight", 83.333M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", Core.Constants.Weight.Kilograms, info.GetChargeableFromBasisTest.Unit);
			AssertEquals("Sell Rate", 333.33M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestBAF.PK);
			AssertEquals("FRT Chargeable Weight", 333.33M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", "AUD", info.GetChargeableFromBasisTest.Unit);
			AssertEquals("Sell Rate", 45.40M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestCAF.PK);
			AssertEquals("FRT Chargeable Weight", 333.33M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", "AUD", info.GetChargeableFromBasisTest.Unit);
			AssertEquals("Sell Rate", 29.17M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestAWB.PK);
			AssertEquals("Sell Rate", 50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestBBK.PK);
			AssertEquals("Sell Rate", 37.50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestTHC.PK);
			AssertEquals("Sell Rate", 18.50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestANY.PK);
			AssertEquals("Sell Rate", 35.00M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestADF.PK);
			AssertEquals("Sell Rate", 50.00M, info.Amount);
			AssertEquals("Sell Currency", "USD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestITF.PK);
			AssertEquals("Sell Rate", 125.00M, info.Amount);
			AssertEquals("Sell Currency", "USD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestLOL.PK);
			AssertEquals("Sell Rate", 38.33M, info.Amount);
			AssertEquals("Sell Currency", "USD", info.Currency);
		}

		public void TestAutoRateAIRWithALLModeOnOriginChargesWithOverride()
		{
			var testRate = SetupExportClientRatesForAutoRater();

			var filter = new ZQuery(RateEntrySchema.TI_Mode, SQLComparisonOperator.Equal, Core.Constants.RateMode.AIR);
			filter.AddToFilter(JoinCondition.And, RateEntrySchema.TI_OriginLRC, SQLComparisonOperator.Equal, "AUSYD");

			var originAirEntry = testRate.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG).Find(filter)[0] as RateEntry;
			var originAirRateLine = originAirEntry.AddRateLine(TestANY.AC_Code, FlatCalculator.Code);
			originAirRateLine.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)25m;

			var originAllEntry = testRate.AddRateEntry("ORG", "ALL", "AUSYD", "", "", "");
			var originAllRateLine = originAllEntry.AddRateLine(TestANY.AC_Code, FlatCalculator.Code);
			originAllRateLine.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)35m;

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 55M, .5M, "NEWTESSYD");

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Result Count", 10, results.Count);

			var info = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			AssertEquals("FRT Chargeable Weight", 83.333M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", Core.Constants.Weight.Kilograms, info.GetChargeableFromBasisTest.Unit);
			AssertEquals("Sell Rate", 333.33M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestBAF.PK);
			AssertEquals("Sell Rate", 45.40M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestCAF.PK);
			AssertEquals("Sell Rate", 29.17M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestAWB.PK);
			AssertEquals("Sell Rate", 50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestBBK.PK);
			AssertEquals("Sell Rate", 37.50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestTHC.PK);
			AssertEquals("Sell Rate", 18.50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestANY.PK);
			AssertEquals("Sell Rate", 25.00M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestADF.PK);
			AssertEquals("Sell Rate", 50.00M, info.Amount);
			AssertEquals("Sell Currency", "USD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestITF.PK);
			AssertEquals("Sell Rate", 125.00M, info.Amount);
			AssertEquals("Sell Currency", "USD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestLOL.PK);
			AssertEquals("Sell Rate", 38.33M, info.Amount);
			AssertEquals("Sell Currency", "USD", info.Currency);
		}

		public void TestAutoRateAIRWithGlobals()
		{
			SetupExportClientRatesForAutoRater();
			SetupGlobalTariffForAutoRater();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 55M, .5M, "NEWTESSYD");

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Result Count", 13, results.Count);

			var info = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			AssertEquals("FRT Chargeable Weight", 83.333M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", Core.Constants.Weight.Kilograms, info.GetChargeableFromBasisTest.Unit);
			AssertEquals("Sell Rate", 333.33M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestBAF.PK);
			AssertEquals("Sell Rate", 45.40M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestCAF.PK);
			AssertEquals("Sell Rate", 29.17M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestAWB.PK);
			AssertEquals("Sell Rate", 50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestBBK.PK);
			AssertEquals("Sell Rate", 37.50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestTHC.PK);
			AssertEquals("Sell Rate", 18.50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestADF.PK);
			AssertEquals("Sell Rate", 50.00M, info.Amount);
			AssertEquals("Sell Currency", "USD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestITF.PK);
			AssertEquals("Sell Rate", 125.00M, info.Amount);
			AssertEquals("Sell Currency", "USD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestLOL.PK);
			AssertEquals("Sell Rate", 38.33M, info.Amount);
			AssertEquals("Sell Currency", "USD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestGL1.PK);
			AssertEquals("Sell Rate", 65.43M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestGL2.PK);
			AssertEquals("Sell Rate", 78.12M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestGL3.PK);
			AssertEquals("Sell Rate", 20.00M, info.Amount);
			AssertEquals("Sell Currency", "USD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestGL4.PK);
			AssertEquals("Sell Rate", 54.17M, info.Amount);
			AssertEquals("Sell Currency", "USD", info.Currency);
		}

		public void TestAutoRateAIRWithGlobalOverride()
		{
			var rate = SetupExportClientRatesForAutoRater();
			SetupGlobalTariffForAutoRater();

			var filter = new ZQuery(RateEntrySchema.TI_Mode, SQLComparisonOperator.Equal, Core.Constants.RateMode.AIR);
			filter.AddToFilter(JoinCondition.And, RateEntrySchema.TI_OriginLRC, SQLComparisonOperator.Equal, "AUSYD");
			var matchedEntry = (RateEntry)rate.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG).Find(filter)[0];
			var originRateLine = matchedEntry.AddRateLine(TestGL1.AC_Code, FlatCalculator.Code);
			originRateLine.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)55.25m;

			filter = new ZQuery(RateEntrySchema.TI_Mode, SQLComparisonOperator.Equal, Core.Constants.RateMode.AIR);
			filter.AddToFilter(JoinCondition.And, RateEntrySchema.TI_DestinationLRC, SQLComparisonOperator.Equal, "USLAX");
			matchedEntry = (RateEntry)rate.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.DST).Find(filter)[0];
			var destinationRateLine = matchedEntry.AddRateLine(TestGL3.AC_Code, UnitCalculator.Code, Core.Constants.Weight.Kilograms);
			destinationRateLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1.04m;

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 55M, .5M, "NEWTESSYD");

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Result Count", 13, results.Count);

			var info = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			AssertEquals("FRT Chargeable Weight", 83.333M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", Core.Constants.Weight.Kilograms, info.GetChargeableFromBasisTest.Unit);
			AssertEquals("Sell Rate", 333.33M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestBAF.PK);
			AssertEquals("Sell Rate", 45.40M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestCAF.PK);
			AssertEquals("Sell Rate", 29.17M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestAWB.PK);
			AssertEquals("Sell Rate", 50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestBBK.PK);
			AssertEquals("Sell Rate", 37.50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestTHC.PK);
			AssertEquals("Sell Rate", 18.50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestADF.PK);
			AssertEquals("Sell Rate", 50.00M, info.Amount);
			AssertEquals("Sell Currency", "USD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestITF.PK);
			AssertEquals("Sell Rate", 125.00M, info.Amount);
			AssertEquals("Sell Currency", "USD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestLOL.PK);
			AssertEquals("Sell Rate", 38.33M, info.Amount);
			AssertEquals("Sell Currency", "USD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestGL1.PK);
			AssertEquals("Sell Rate", 55.25M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestGL2.PK);
			AssertEquals("Sell Rate", 78.12M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestGL3.PK);
			AssertEquals("Sell Rate", 86.67M, info.Amount);
			AssertEquals("Sell Currency", "USD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestGL4.PK);
			AssertEquals("Sell Rate", 54.17M, info.Amount);
			AssertEquals("Sell Currency", "USD", info.Currency);
		}

		public void TestAutoRateAIRWithUseGlobalRatesForOriginDest()
		{
			SetupExportClientRatesForAutoRater();
			SetupGlobalTariffForAutoRater();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 55M, .5M, NewClient);

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Result Count", 13, results.Count);

			NewClient.CompanyData.RateTariffLevels.SetLevel("ORG", 0);
			NewClient.CompanyData.RateTariffLevels.SetLevel("DST", 0);
			Factory.Save();
			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 9, results.Count);

			NewClient.CompanyData.RateTariffLevels.SetLevel("ORG", 0);
			NewClient.CompanyData.RateTariffLevels.SetLevel("DST", 1);
			Factory.Save();
			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 11, results.Count);

			NewClient.CompanyData.RateTariffLevels.SetLevel("ORG", 1);
			NewClient.CompanyData.RateTariffLevels.SetLevel("DST", 0);
			Factory.Save();
			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 11, results.Count);
		}

		public void TestAutoRateAIRWithUseGlobalRatesForFreight()
		{
			SetupExportClientRatesForAutoRater(false);
			SetupGlobalTariffForAutoRater();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 55M, .5M, NewClient);

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Result Count", 13, results.Count);

			NewClient.CompanyData.RateTariffLevels.SetLevel("FRT", 0);
			Factory.Save();
			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 10, results.Count);
		}

		public void TestAutoRateAIRWithStdCostsOneLeg_CheckAWBLogMatchesNormalAutoratingLog()
		{
			var standardCost = Helper.NewCosting(null);
			standardCost.AddRateEntryWithFlatRateLine("AIR", "LSE", "AU", "FR", "FRT", 10);
			standardCost.AddRateEntryWithFlatRateLine("AIR", "LSE", "AU", "US", "FRT", 20);

			var carrier = TransportProvider1;
			carrier.OH_IsShippingProvider = true;

			var carrierCost = Helper.NewCosting(carrier);
			carrierCost.AddRateEntryWithFlatRateLine("AIR", "LSE", "AU", "FR", "FRT", 1000);
			carrierCost.AddRateEntryWithFlatRateLine("AIR", "LSE", "AU", "US", "FRT", 2000);

			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = ContainerModes.Loose;
			consol.JK_UniqueConsignRef = "ConsolRef";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.Shipments.AddNew();

			var t1 = consol.Transports[0];
			t1.JW_IsLinked = false;
			t1.JW_VoyageFlight = "VF01";
			t1.JW_RL_NKLoadPort = "AUSYD";
			t1.JW_RL_NKDiscPort = "FRCDG";
			t1.JW_ETD = ZDate.Today.AddDays(2);
			t1.JW_ETA = ZDate.Today.AddDays(4);
			t1.JW_TransportMode = TransportModes.Air;
			t1.JW_TransportType = TransportPlanningType.Flight1;
			t1.JW_CarrierBookingReference = "Route1";
			t1.CarrierPK = carrier.PK;

			var logger = new ElementaryLogger();

			var testAutoRater = new FreightAutoRater(new RatingContext(logger));
			var proxy = new AutoRatingProxy(consol.RatingAdapter);
			var bizoHost = proxy.StandardFreightCost.GetHost();

			var result = testAutoRater.AutoRate(proxy, CostSell.Cost).RateInfoCollection;
			AssertEquals("The best matching rate is for the carrier rate of $2000", 2000, (int)result[0].Amount);

			var log = CalculationLogsLoader.Load(bizoHost).Logs;
			AssertEquals("The standard rate, however, is $20", 20, (int)log[0].BaseRate);
		}

		#endregion

		#region LCL Freight Tests

		public void TestAutoRateLCL()
		{
			SetupExportClientRatesForAutoRater();
			var testObject = new AutoRatingObject("AUSYD", "GBLON", FreightMode.LCL, null, 2750M, 1.5M, "NEWTESSYD");

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Result Count", 9, results.Count);

			var info = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			AssertEquals("FRT Chargeable Weight", 2.750M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", "M3", info.GetChargeableFromBasisTest.Unit);
			AssertEquals("Sell Rate", 343.75M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestBAF.PK);
			AssertEquals("Sell Rate", 46.82M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestCAF.PK);
			AssertEquals("Sell Rate", 30.08M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestAWB.PK);
			AssertEquals("Sell Rate", 50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestBBK.PK);
			AssertEquals("Sell Rate", 50.00M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestTHC.PK);
			AssertEquals("Sell Rate", 18.50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestPSC.PK);
			AssertEquals("Sell Rate", 6.88M, info.Amount);
			AssertEquals("Sell Currency", "GBP", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestITF.PK);
			AssertEquals("Sell Rate", 123.75M, info.Amount);
			AssertEquals("Sell Currency", "GBP", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestLOL.PK);
			AssertEquals("Sell Rate", 121.25M, info.Amount);
			AssertEquals("Sell Currency", "GBP", info.Currency);
		}

		public void TestAutoRateLCLWithSEAOnOriginCharges()
		{
			var rate = SetupExportClientRatesForAutoRater();
			var entry = rate.AddRateEntry("ORG", "SEA", "AUSYD", "", "", "");
			var rateLine = entry.AddRateLine(TestSEA.AC_Code, UnitCalculator.Code, "M3");
			rateLine.RateLineItems.FindByTM_Type(Calculator.Items.Operator.UNT).TM_Value = 15.00M;

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "GBLON", FreightMode.LCL, null, 2750M, 1.5M, "NEWTESSYD");

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Result Count", 10, results.Count);

			var info = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			AssertEquals("FRT Chargeable Weight", 2.750M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", "M3", info.GetChargeableFromBasisTest.Unit);
			AssertEquals("Sell Rate", 343.75M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestBAF.PK);
			AssertEquals("Sell Rate", 46.82M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestCAF.PK);
			AssertEquals("Sell Rate", 30.08M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestAWB.PK);
			AssertEquals("Sell Rate", 50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestBBK.PK);
			AssertEquals("Sell Rate", 50.00M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestTHC.PK);
			AssertEquals("Sell Rate", 18.50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestSEA.PK);
			AssertEquals("Sell Rate", 41.25M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestPSC.PK);
			AssertEquals("Sell Rate", 6.88M, info.Amount);
			AssertEquals("Sell Currency", "GBP", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestITF.PK);
			AssertEquals("Sell Rate", 123.75M, info.Amount);
			AssertEquals("Sell Currency", "GBP", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestLOL.PK);
			AssertEquals("Sell Rate", 121.25M, info.Amount);
			AssertEquals("Sell Currency", "GBP", info.Currency);
		}

		public void TestAutoRateLCLWithSEAOnOriginChargesWithOverride()
		{
			var rate = SetupExportClientRatesForAutoRater();

			var filter = new ZQuery(RateEntrySchema.TI_Mode, SQLComparisonOperator.Equal, Core.Constants.RateMode.LCL);
			filter.AddToFilter(JoinCondition.And, RateEntrySchema.TI_OriginLRC, SQLComparisonOperator.Equal, "AUSYD");
			var originLclEntry = rate.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG).Find(filter)[0] as RateEntry;
			var originLclLine = originLclEntry.AddRateLine(TestSEA.AC_Code, UnitCalculator.Code, "M3");
			originLclLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)12.50m;

			var originSeaEntry = rate.AddRateEntry("ORG", "SEA", "AUSYD", "", "", "");
			var originSeaLine = originSeaEntry.AddRateLine(TestSEA.AC_Code, UnitCalculator.Code, "M3");
			originSeaLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)15.00m;

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "GBLON", FreightMode.LCL, null, 2750M, 1.5M, "NEWTESSYD");

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Result Count", 10, results.Count);

			var info = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			AssertEquals("FRT Chargeable Weight", 2.750M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", "M3", info.GetChargeableFromBasisTest.Unit);
			AssertEquals("Sell Rate", 343.75M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestBAF.PK);
			AssertEquals("Sell Rate", 46.82M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestCAF.PK);
			AssertEquals("Sell Rate", 30.08M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestAWB.PK);
			AssertEquals("Sell Rate", 50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestBBK.PK);
			AssertEquals("Sell Rate", 50.00M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestTHC.PK);
			AssertEquals("Sell Rate", 18.50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestSEA.PK);
			AssertEquals("Sell Rate", 34.38M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestPSC.PK);
			AssertEquals("Sell Rate", 6.88M, info.Amount);
			AssertEquals("Sell Currency", "GBP", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestITF.PK);
			AssertEquals("Sell Rate", 123.75M, info.Amount);
			AssertEquals("Sell Currency", "GBP", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestLOL.PK);
			AssertEquals("Sell Rate", 121.25M, info.Amount);
			AssertEquals("Sell Currency", "GBP", info.Currency);
		}

		public void TestAutoRateLCLWithNoFreightEntry()
		{
			var rate = SetupExportClientRatesForAutoRater();
			rate.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.LCL).RemoveAndDeleteAll();

			var filter = new ZQuery(RateEntrySchema.TI_Mode, SQLComparisonOperator.Equal, Core.Constants.RateMode.LCL);
			filter.AddToFilter(JoinCondition.And, RateEntrySchema.TI_DestinationLRC, SQLComparisonOperator.Equal, "GBLON");
			var entry = (RateEntry)rate.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.DST).Find(filter)[0];

			var stdLine = entry.AddRateLine(TestCTG.AC_Code, CartageCalculator.Code, "M3");
			stdLine.ConversionFactor = new ConversionFactor(250m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			stdLine.Calculator.EquipmentType = "STD";
			stdLine.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)125m;
			stdLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)40m;

			var sdlLine = entry.AddRateLine(TestCTG.AC_Code, CartageCalculator.Code, "M3");
			sdlLine.ConversionFactor = new ConversionFactor(250m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			sdlLine.Calculator.EquipmentType = "SDL";
			sdlLine.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)150m;
			sdlLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)45m;

			var trlLine = entry.AddRateLine(TestCTG.AC_Code, CartageCalculator.Code, "M3");
			trlLine.ConversionFactor = new ConversionFactor(333m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			trlLine.Calculator.EquipmentType = "TRL";
			trlLine.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)175m;
			trlLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)50m;

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "GBLON", FreightMode.LCL, null, 2750M, 1.5M, "NEWTESSYD");
			testObject.DeliveryCartageEquipment = "SDL";

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Result Count", 7, results.Count);

			var info = results.First(r => r.ChargeCode.PK == TestAWB.PK);
			AssertEquals("Sell Rate", 50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestBBK.PK);
			AssertEquals("Sell Rate", 50.00M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestTHC.PK);
			AssertEquals("Sell Rate", 18.50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestPSC.PK);
			AssertEquals("Sell Rate", 6.88M, info.Amount);
			AssertEquals("Sell Currency", "GBP", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestITF.PK);
			AssertEquals("Sell Rate", 123.75M, info.Amount);
			AssertEquals("Sell Currency", "GBP", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestLOL.PK);
			AssertEquals("Sell Rate", 121.25M, info.Amount);
			AssertEquals("Sell Currency", "GBP", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestCTG.PK);
			AssertEquals("Sell Rate", 495M, info.Amount);
			AssertEquals("Sell Currency", "GBP", info.Currency);
		}

		public void TestAutoRateLCLWithDifferentCartageEquipment()
		{
			var rate = SetupExportClientRatesForAutoRater();
			var filter = new ZQuery(RateEntrySchema.TI_Mode, SQLComparisonOperator.Equal, Core.Constants.RateMode.LCL);
			filter.AddToFilter(JoinCondition.And, RateEntrySchema.TI_DestinationLRC, SQLComparisonOperator.Equal, "GBLON");
			var entry = (RateEntry)rate.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.DST).Find(filter)[0];

			var stdLine = entry.AddRateLine(TestCTG.AC_Code, CartageCalculator.Code, "M3");
			stdLine.ConversionFactor = new ConversionFactor(250m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			stdLine.Calculator.EquipmentType = "STD";
			stdLine.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)125m;
			stdLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)40m;

			var sdlLine = entry.AddRateLine(TestCTG.AC_Code, CartageCalculator.Code, "M3");
			sdlLine.ConversionFactor = new ConversionFactor(250m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			sdlLine.Calculator.EquipmentType = "SDL";
			sdlLine.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)150m;
			sdlLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)45m;

			var trlLine = entry.AddRateLine(TestCTG.AC_Code, CartageCalculator.Code, "M3");
			trlLine.ConversionFactor = new ConversionFactor(333m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			trlLine.Calculator.EquipmentType = "TRL";
			trlLine.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)175m;
			trlLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)50m;

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "GBLON", FreightMode.LCL, null, 2750M, 1.5M, "NEWTESSYD");
			testObject.DeliveryCartageEquipment = "SDL";

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Result Count", 10, results.Count);

			var info = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			AssertEquals("FRT Chargeable Weight", 2.750M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", "M3", info.GetChargeableFromBasisTest.Unit);
			AssertEquals("Sell Rate", 343.75M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestBAF.PK);
			AssertEquals("Sell Rate", 46.82M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestCAF.PK);
			AssertEquals("Sell Rate", 30.08M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestAWB.PK);
			AssertEquals("Sell Rate", 50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestBBK.PK);
			AssertEquals("Sell Rate", 50.00M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestTHC.PK);
			AssertEquals("Sell Rate", 18.50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestPSC.PK);
			AssertEquals("Sell Rate", 6.88M, info.Amount);
			AssertEquals("Sell Currency", "GBP", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestITF.PK);
			AssertEquals("Sell Rate", 123.75M, info.Amount);
			AssertEquals("Sell Currency", "GBP", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestLOL.PK);
			AssertEquals("Sell Rate", 121.25M, info.Amount);
			AssertEquals("Sell Currency", "GBP", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestCTG.PK);
			AssertEquals("Sell Rate", 495M, info.Amount);
			AssertEquals("Sell Currency", "GBP", info.Currency);
		}

		public void TestAutoRateLCLWithPercentageOfAllCharges()
		{
			SetupExportClientRatesForAutoRaterWithPercentageOfAllCharges();
			var testObject = new AutoRatingObject("AUPER", "GBLON", FreightMode.LCL, null, 2750M, 1.5M, "NEWTESSYD");
			testObject.DeliveryCartageEquipment = "SDL";

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Result Count", 3, results.Count);

			var info = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			AssertEquals("FRT Chargeable Weight", 2.750M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", "M3", info.GetChargeableFromBasisTest.Unit);
			AssertEquals("Sell Rate", 343.75M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestBAF.PK);
			AssertEquals("Sell Rate", 3.94M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestAWB.PK);
			AssertEquals("Sell Rate", 50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);
		}

		#endregion

		#region FCL Freight Tests

		public void TestAutoRateFCLWithOneContainerType()
		{
			SetupExportClientRatesForAutoRater();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.FCL, new TestContainers(Factory, "20GP", 3), 0M, 0M, "NEWTESSYD");

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Result Count", 7, results.Count);

			var info = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			AssertEquals("FRT Chargeable Weight", 3M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", "20GP", info.GetChargeableFromBasisTest.Unit);
			AssertEquals("Sell Rate", 7500M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestBAF.PK);
			AssertEquals("Sell Rate", 1021.50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestCAF.PK);
			AssertEquals("Sell Rate", 656.25M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestAWB.PK);
			AssertEquals("Sell Rate", 50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestBBK.PK);
			AssertEquals("Sell Rate", 300M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestTHC.PK);
			AssertEquals("Sell Rate", 18.50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestPSC.PK);
			AssertEquals("Sell Rate", 450M, info.Amount);
			AssertEquals("Sell Currency", "USD", info.Currency);
		}

		public void TestAutoRateFCLWithOneContainerTypeWithFCLWithoutContainerOnDST()
		{
			var rate = SetupExportClientRatesForAutoRater();
			var rateEntry = rate.AddRateEntry("DST", "FCL", "", "USLAX", "", "");
			var rateLine = rateEntry.AddRateLine(TestANY.AC_Code, FlatCalculator.Code, QuantityUnit.CN);
			rateLine.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)125m;

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.FCL, new TestContainers(Factory, "20GP", 3), 0M, 0M, "NEWTESSYD");

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Result Count", 8, results.Count);

			var info = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			AssertEquals("FRT Chargeable Weight", 3M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", "20GP", info.GetChargeableFromBasisTest.Unit);
			AssertEquals("Sell Rate", 7500M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestBAF.PK);
			AssertEquals("Sell Rate", 1021.50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestCAF.PK);
			AssertEquals("Sell Rate", 656.25M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestAWB.PK);
			AssertEquals("Sell Rate", 50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestBBK.PK);
			AssertEquals("Sell Rate", 300M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestTHC.PK);
			AssertEquals("Sell Rate", 18.50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestPSC.PK);
			AssertEquals("Sell Rate", 450M, info.Amount);
			AssertEquals("Sell Currency", "USD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestANY.PK);
			AssertEquals("Sell Rate", 125M, info.Amount);
			AssertEquals("Sell Currency", "USD", info.Currency);
		}

		public void TestAutoRateFCLWithOneContainerTypeWithFCLWithoutContainerOnDSTAndOverride()
		{
			var rate = SetupExportClientRatesForAutoRater();
			var fclEntry = rate.AddRateEntry("DST", "FCL", "", "USLAX", "", "");
			var fclLine = fclEntry.AddRateLine(TestANY.AC_Code, FlatCalculator.Code, QuantityUnit.CN);
			fclLine.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)125m;

			var filter = new ZQuery(RateEntrySchema.TI_Mode, SQLComparisonOperator.Equal, Core.Constants.RateMode.FCL);
			filter.AddToFilter(JoinCondition.And, RateEntrySchema.TI_RC, SQLComparisonOperator.Equal, GP20.PK);
			filter.AddToFilter(JoinCondition.And, RateEntrySchema.TI_DestinationLRC, SQLComparisonOperator.Equal, "USLAX");

			var containerEntry = (RateEntry)rate.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.DST).Find(filter)[0];
			var containerLine = containerEntry.AddRateLine(TestANY.AC_Code, FlatCalculator.Code, QuantityUnit.CN);
			containerLine.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)200m;

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.FCL, new TestContainers(Factory, "20GP", 3), 0M, 0M, "NEWTESSYD");

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Result Count", 8, results.Count);

			var info = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			AssertEquals("FRT Chargeable Weight", 3M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", "20GP", info.GetChargeableFromBasisTest.Unit);
			AssertEquals("Sell Rate", 7500M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestBAF.PK);
			AssertEquals("Sell Rate", 1021.50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestCAF.PK);
			AssertEquals("Sell Rate", 656.25M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestAWB.PK);
			AssertEquals("Sell Rate", 50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestBBK.PK);
			AssertEquals("Sell Rate", 300M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestTHC.PK);
			AssertEquals("Sell Rate", 18.50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestPSC.PK);
			AssertEquals("Sell Rate", 450M, info.Amount);
			AssertEquals("Sell Currency", "USD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestANY.PK);
			AssertEquals("Sell Rate", 200M, info.Amount);
			AssertEquals("Sell Currency", "USD", info.Currency);
		}

		public void TestAutoRateFCLWithMoreThanOneContainerType()
		{
			SetupExportClientRatesForAutoRater();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.FCL, new TestContainers(Factory, "20GP", 3, "40GP", 2), 0m, 0m, "NEWTESSYD");

			var autoRater = new FreightAutoRater(new RatingContext());
			var results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			var expected = new[]
				{
					new SimpleArInfo
					{
						Amount = 50m,
						InvoiceLineDesc = "Test Airway Bill Fee",
						CalculationSingleLineDescription = "TESTAWB: Base Rate AUD 50.00"
					},
					new SimpleArInfo
					{
						Amount = 50m,
						InvoiceLineDesc = "Test Airway Bill Fee",
						CalculationSingleLineDescription = "TESTAWB: Base Rate AUD 50.00"
					},
					new SimpleArInfo
					{
						Amount = 350m,
						InvoiceLineDesc = "Test Breakbulk",
						CalculationSingleLineDescription = "TESTBBK: 2 40GP Container(s) @ AUD 175.00/Container"
					},
					new SimpleArInfo
					{
						Amount = 300m,
						InvoiceLineDesc = "Test Breakbulk",
						CalculationSingleLineDescription = "TESTBBK: 3 20GP Container(s) @ AUD 100.00/Container"
					},
					new SimpleArInfo
					{
						Amount = 8000m,
						InvoiceLineDesc = "Test Freight",
						CalculationSingleLineDescription = "TESTFRT: 2 40GP Container(s) @ AUD 4000.00/Container"
					},
					new SimpleArInfo
					{
						Amount = 7500m,
						InvoiceLineDesc = "Test Freight",
						CalculationSingleLineDescription = "TESTFRT: 3 20GP Container(s) @ AUD 2500.00/Container"
					},
					new SimpleArInfo
					{
						Amount = 500m,
						InvoiceLineDesc = "Test Port Services Charge",
						CalculationSingleLineDescription = "TESTPSC: 2 40GP Container(s) @ USD 250.00/Container"
					},
					new SimpleArInfo
					{
						Amount = 450m,
						InvoiceLineDesc = "Test Port Services Charge",
						CalculationSingleLineDescription = "TESTPSC: 3 20GP Container(s) @ USD 150.00/Container"
					},
					new SimpleArInfo
					{
						Amount = 18.5m,
						InvoiceLineDesc = "Test Terminal Handling Charge",
						CalculationSingleLineDescription = "TESTTHC: Base Rate AUD 18.50"
					},
					new SimpleArInfo
					{
						Amount = 18.5m,
						InvoiceLineDesc = "Test Terminal Handling Charge",
						CalculationSingleLineDescription = "TESTTHC: Base Rate AUD 18.50"
					},
					new SimpleArInfo
					{
						Amount = 1021.50m,
						InvoiceLineDesc = "Test BAF",
						CalculationDescription = "TESTBAF: 13.62% of (AUD 7500.00 (TESTFRT))"
					},
					new SimpleArInfo
					{
						Amount = 1089.60m,
						InvoiceLineDesc = "Test BAF",
						CalculationDescription = "TESTBAF: 13.62% of (AUD 8000.00 (TESTFRT))"
					},
					new SimpleArInfo
					{
						Amount = 656.25m,
						InvoiceLineDesc = "Test CAF",
						CalculationDescription = "TESTCAF: 8.75% of (AUD 7500.00 (TESTFRT))"
					},
					new SimpleArInfo
					{
						Amount = 700.00m,
						InvoiceLineDesc = "Test CAF",
						CalculationDescription = "TESTCAF: 8.75% of (AUD 8000.00 (TESTFRT))"
					}
				};

			AssertRatingResults(expected, results);
		}

		public void TestAutoRateFCLWithMoreThanOneContainerType2()
		{
			var gp20 = Helper.Containers["20GP"];
			var gp40 = Helper.Containers["40GP"];
			var hc40 = Helper.Containers["40HC"];
			gp40.RC_FreightRateClass = "40";
			hc40.RC_FreightRateClass = "40";
			gp20.RC_HandlingRateClass = "GP";
			gp40.RC_HandlingRateClass = "GP";

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var rateEntry1 = clientRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "20GP");
			rateEntry1.TI_MatchContainerRateClass = true;
			rateEntry1.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 1000m;
			rateEntry1.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.CN);
			rateEntry1.RateLines[1].GetCalculator<UnitCalculator>().PerUnit = 60m;

			var rateEntry2 = clientRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "40GP");
			rateEntry2.TI_MatchContainerRateClass = true;
			rateEntry2.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 1900m;
			rateEntry2.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.CN);
			rateEntry2.RateLines[1].GetCalculator<UnitCalculator>().PerUnit = 120m;

			var rateEntry3 = clientRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "40HC");
			rateEntry3.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 2100m;

			var rateEntry4 = clientRate.AddRateEntry("DST", "FCL", "AUSYD", "USLAX", "", "40GP");
			rateEntry4.TI_MatchContainerRateClass = true;
			rateEntry4.AddRateLine("DPCH", UnitCalculator.Code, QuantityUnit.CN).Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)150m;

			var rateEntry5 = clientRate.AddRateEntry("DST", "FCL", "AUSYD", "USLAX", "", "40HC");
			rateEntry5.TI_MatchContainerRateClass = true;
			rateEntry5.AddRateLine("DPCH", UnitCalculator.Code, QuantityUnit.CN).Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)170m;

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.FCL, new TestContainers(Factory, "20GP", 3, "40GP", 2, "40HC", 1), 0m, 0m, clientRate.Header);

			var autoRater = new FreightAutoRater(new RatingContext());
			var results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Total results count", 7, results.Count);

			var sumOfAllFRTCharges = results.Where(x => x.ChargeCode.PK == Helper.ChargeCodes["FRT"].PK).Sum(x => x.Amount);
			AssertEquals("FRT", 8900m, sumOfAllFRTCharges);

			var sumOfAllBFCharges = results.Where(x => x.ChargeCode.PK == Helper.ChargeCodes["BAF"].PK).Sum(x => x.Amount);
			AssertEquals("BAF", 540m, sumOfAllBFCharges);

			var sumOfAllDPCHharges = results.Where(x => x.ChargeCode.PK == Helper.ChargeCodes["DPCH"].PK).Sum(x => x.Amount);
			AssertEquals("DPCH", 920m, sumOfAllDPCHharges);
		}

		public void TestAutoRateFCLWithMoreThanOneContainerType4()
		{
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var entry1 = testRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "20GP");
			entry1.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 1000m;
			entry1.RateLines[0].TL_WeightVolume = RatingConstants.Units.HB;

			var entry2 = testRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "40GP");
			entry2.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 1900m;
			entry2.RateLines[0].TL_WeightVolume = RatingConstants.Units.HB;

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.FCL, new TestContainers(Factory, "20GP", 3, "40GP", 2), 0m, 0m, testRate.Header);
			testObject.Measures.Shipments = 1;

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			var expected = new[]
						{
							new SimpleArInfo
								{
									InvoiceLineDesc = @"RATE NOTE: International Freight
	Charge cannot be calculated due to conflicting rates found.",
								},
						};

			AssertRatingResults(expected, results);
		}

		public void TestAutoRateFCLWithOneContainerTypeAndHBRate()
		{
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var entry1 = testRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "20GP");
			entry1.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 1000m;
			entry1.RateLines[0].TL_WeightVolume = RatingConstants.Units.HB;

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.FCL, new TestContainers(Factory, "20GP", 3, "40GP", 2), 0m, 0m, testRate.Header);
			testObject.Measures.Shipments = 1;

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Result Count", 1, results.Count);

			var info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["FRT"].PK);
			AssertEquals("FRT", 1000m, info.Amount);
			AssertEquals("FRT: 1 House Bill(s) @ USD 1000.00/House Bill", info.SingleLineDescription);
			AssertEquals("International Freight", info.InvoiceLineDescription);
		}

		public void TestAutoRateFCLWithGlobalsOverride()
		{
			var rate = SetupExportClientRatesForAutoRater();
			SetupGlobalTariffForAutoRater();

			var entry = rate.AddRateEntry("DST", "ALL", "", "USLAX", "", "20GP");
			var rateLine = entry.AddRateLine(TestGL5.AC_Code, FlatCalculator.Code);
			rateLine.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS).TM_Value = 10M;

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.FCL, new TestContainers(Factory, "20GP", 3), 0M, 0M, "NEWTESSYD");

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Result Count", 9, results.Count);

			var info = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			AssertEquals("FRT Chargeable Weight", 3M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", "20GP", info.GetChargeableFromBasisTest.Unit);
			AssertEquals("Sell Rate", 7500M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestBAF.PK);
			AssertEquals("Sell Rate", 1021.50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestCAF.PK);
			AssertEquals("Sell Rate", 656.25M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestGL1.PK);
			AssertEquals("Sell Rate", 65.43M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestAWB.PK);
			AssertEquals("Sell Rate", 50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestBBK.PK);
			AssertEquals("Sell Rate", 300M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestTHC.PK);
			AssertEquals("Sell Rate", 18.50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestPSC.PK);
			AssertEquals("Sell Rate", 450M, info.Amount);
			AssertEquals("Sell Currency", "USD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestGL5.PK);
			AssertEquals("Sell Rate", 10.00M, info.Amount);
			AssertEquals("Sell Currency", "USD", info.Currency);
		}

		#endregion

		#region Combined FCL/LCL Freight Tests

		public void TestAutoRateCombinedFCL_LCL()
		{
			AutoRateInfo FindInfo(AutoRateInfoCollection infos, AccChargeCode chargeCode, RefContainer container)
			{
				var containerPK = container?.PK ?? ZGuid.Empty;

				var matchedInfo = infos.FirstOrDefault(i =>
				{
					var rateInfoContainerPK = i.Entry?.TI_RC ?? ZGuid.Empty;
					return i.ChargeCode.PK == chargeCode.PK && rateInfoContainerPK == containerPK;
				});

				return matchedInfo;
			}

			SetupExportClientRatesForAutoRater();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.FCL, new TestContainers(Factory, "20GP", 2, MeasureInfo.ContainerInfo.LCL, 10000m, 9m), 0M, 0M, "NEWTESSYD");

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Result Count", 11, results.Count);

			// 20GP
			var info = FindInfo(results, TestFRT, GP20);
			AssertEquals("FRT Chargeable Weight", 2M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", "20GP", info.GetChargeableFromBasisTest.Unit);
			AssertEquals("Sell Rate", 5000M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = FindInfo(results, TestBAF, GP20);
			AssertEquals("Sell Rate", 681M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = FindInfo(results, TestCAF, GP20);
			AssertEquals("Sell Rate", 437.5M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = FindInfo(results, TestAWB, GP20);
			AssertEquals("Sell Rate", 50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = FindInfo(results, TestBBK, GP20);
			AssertEquals("Sell Rate", 200M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = FindInfo(results, TestTHC, GP20);
			AssertEquals("Sell Rate", 18.50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = FindInfo(results, TestPSC, GP20);
			AssertEquals("Sell Rate", 300M, info.Amount);
			AssertEquals("Sell Currency", "USD", info.Currency);

			// LCL
			info = FindInfo(results, TestFRT, null);
			AssertEquals("FRT Chargeable Weight", 10M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", "M3", info.GetChargeableFromBasisTest.Unit);
			AssertEquals("Sell Rate", 1100M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = FindInfo(results, TestAWB, null);
			AssertEquals("Sell Rate", 50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = FindInfo(results, TestBBK, null);
			AssertEquals("Sell Rate", 150M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = FindInfo(results, TestTHC, null);
			AssertEquals("Sell Rate", 18.50M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);
		}

		#endregion

		#region ComplexFreight

		public void TestComplexFreight()
		{
			#region Client Rate

			InsertClientChargeCodesForAutoRaterTests(Factory);

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			testRate.TH_OH = NewClient.PK;

			var aIRRateEntry1 = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "", "");
			aIRRateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			aIRRateEntry1.RateLines.RemoveAndDeleteAll();

			var aIRRateLine1a = aIRRateEntry1.AddRateLine(TestFRT.AC_Code, UnitCalculator.Code, QuantityUnit.KG);
			((UnitCalculator)aIRRateLine1a.Calculator).PerUnit = 5m;

			var aIRRateLine1b = aIRRateEntry1.AddRateLine(TestBAF.AC_Code, FlatCalculator.Code);
			((FlatCalculator)aIRRateLine1b.Calculator).BaseRate = 50m;

			var aIRRateLine1c = aIRRateEntry1.AddRateLine(TestCAF.AC_Code, FlatCalculator.Code);
			((FlatCalculator)aIRRateLine1c.Calculator).BaseRate = 25m;

			var aIRRateEntry2 = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "", "");
			aIRRateEntry2.TI_RH_NKCommodityCode = "GEN";
			aIRRateEntry2.RateLines.RemoveAndDeleteAll();

			var aIRRateLine2b = aIRRateEntry2.AddRateLine(TestBAF.AC_Code, FlatCalculator.Code);
			((FlatCalculator)aIRRateLine2b.Calculator).BaseRate = 45m;

			var aIRRateLine2c = aIRRateEntry2.AddRateLine(TestCAF.AC_Code, FlatCalculator.Code);
			((FlatCalculator)aIRRateLine2c.Calculator).BaseRate = 30m;

			var aIRRateEntry3 = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "STD", "");
			aIRRateEntry3.TI_RH_NKCommodityCode = ZString.Empty;
			aIRRateEntry3.RateLines.RemoveAndDeleteAll();

			var aIRRateLine3c = aIRRateEntry3.AddRateLine(TestCAF.AC_Code, FlatCalculator.Code);
			((FlatCalculator)aIRRateLine3c.Calculator).BaseRate = 20m;

			Factory.Save();

			#endregion

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 100M, .5M, testRate.Header);

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Result Count", 3, results.Count);

			var info = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			AssertEquals("Sell Rate", 500M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestBAF.PK);
			AssertEquals("Sell Rate", 45M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			info = results.First(r => r.ChargeCode.PK == TestCAF.PK);
			AssertEquals("Sell Rate", 30M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			Env.Registry.Rating.FreightSearchPriorities = RateEntrySchema.TI_RS_NKServiceLevel_NI.Name + "," +
				RateEntrySchema.TI_RH_NKCommodityCode.Name + "," +
				RateEntrySchema.TI_OH_TransportProvider.Name + "," +
				RateEntrySchema.TI_ViaLRC.Name + "," +
				RateEntrySchema.TI_HBLDeliveryMode.Name + ",";

			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Result Count", 3, results.Count);

			info = results.First(r => r.ChargeCode.PK == TestCAF.PK);
			AssertEquals("Sell Rate", 20M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);

			var aIRRateEntryG1 = testRate.AddRateEntry("AIR", "LSE", "AU", "US");
			aIRRateEntryG1.RateLines.RemoveAndDeleteAll();
			var aIRRateLineG1a = aIRRateEntryG1.AddRateLine("FRT", FlatCalculator.Code);
			((FlatCalculator)aIRRateLineG1a.Calculator).BaseRate = 32m;

			Factory.Save();

			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Result Count", 4, results.Count);

			info = results.First(r => r.ChargeCode.PK == aIRRateLineG1a.TL_AC);
			AssertEquals("Sell Rate", 32M, info.Amount);
			AssertEquals("Sell Currency", "AUD", info.Currency);
		}

		public void TestFreightWithOverridenPorts()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var line1 = rate.AddRateEntry("AIR", "LSE", "AU", "US").RateLines[0];
			line1.TL_RateCalculator = UnitCalculator.Code;
			line1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;
			var line2 = rate.AddRateEntry("AIR", "LSE", "AU", "USLAX").RateLines[0];
			line2.TL_RateCalculator = UnitCalculator.Code;
			line2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)4m;
			var line3 = rate.AddRateEntry("AIR", "LSE", "AUMEL", "USLAX").RateLines[0];
			line3.TL_RateCalculator = UnitCalculator.Code;
			line3.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)3m;

			Factory.Save();

			var testAutoRater = new FreightAutoRater(new RatingContext());

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 100m, 0m, rate.Header);
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 1, results.Count);
			var info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["FRT"].PK);
			AssertEquals("Sell Rate", 400M, info.Amount);

			testObject = new AutoRatingObject("AUMEL", "USLAX", FreightMode.LSE, null, 100m, 0m, rate.Header);
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 1, results.Count);
			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["FRT"].PK);
			AssertEquals("Sell Rate", 300M, info.Amount);

			testObject = new AutoRatingObject("AUMEL", "USSFO", FreightMode.LSE, null, 100m, 0m, rate.Header);
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 1, results.Count);
			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["FRT"].PK);
			AssertEquals("Sell Rate", 500M, info.Amount);
		}

		#endregion

		#region Multiple Conversion Factors

		public void TestMixedSystemsAndConversionFactors()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();

			var line = entry.AddRateLine("FRT", UnitCalculator.Code, "LB");
			line.Calculator[UnitCalculator.Items.Operator.UNT] = (ZDecimal)5m;

			var line2 = entry.AddRateLine("FSC", UnitCalculator.Code, QuantityUnit.KG);
			line2.Calculator[UnitCalculator.Items.Operator.UNT] = (ZDecimal)5m;

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.FCL, null, 10000m, 0m, rate.Header);

			var testAutoRater = new FreightAutoRater(new RatingContext());

			var expected = new[]
							{
								new SimpleArInfo
									{
										InvoiceLineDesc = "International Freight",
										Amount = 110231.13m,
										CalculationSingleLineDescription = "FRT: 22046.226 Pound(s) @ USD 5.00/LB"
									},
								new SimpleArInfo
									{
										InvoiceLineDesc = "Fuel Surcharge",
										Amount = 50000m,
										CalculationSingleLineDescription = "FSC: 10000 Kilogram(s) @ USD 5.00/KG"
									}
							};

			AssertRatingResults(expected, testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue));

			testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.FCL, null, 10000m, 10m, rate.Header);
			testAutoRater = new FreightAutoRater(new RatingContext());

			expected = new[]
						{
							new SimpleArInfo
									{
										InvoiceLineDesc = "International Freight",
										Amount = 176573.34m,
										CalculationSingleLineDescription = "FRT: 35314.667 Pound(s) @ USD 5.00/LB"		//This happens because shipment weight and shipment volume are first converted to pounds and feet and then chargeable amount is calculated using default imperial factor 1 CF = 100 lbs
									},
									new SimpleArInfo
									{
										InvoiceLineDesc = "Fuel Surcharge",
										Amount = 50000m,
										CalculationSingleLineDescription = "FSC: 10000 Kilogram(s) @ USD 5.00/KG"
									}
						};

			AssertRatingResults(expected, testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue));
		}

		public void TestMultipleConversionFactors()
		{
			#region Client Rate

			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = NewClient.PK;

			var rateEntry1 = clientRate.AddRateEntry("LCL", "LCL", "AUSYD", "SGSIN");
			var rateLine1 = rateEntry1.RateLines[0];
			rateLine1.TL_RateCalculator = UnitCalculator.Code;
			rateLine1.TL_WeightVolume = "M3";
			rateLine1.ConversionFactor = new ConversionFactor(500m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			rateLine1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)60m;

			var rateEntry2 = clientRate.AddRateEntry("LCL", "LCL", "AU", "SG");
			rateEntry2.RateLines.RemoveAndDeleteAll();
			var rateLine2 = rateEntry2.AddRateLine("FSC", UnitCalculator.Code, QuantityUnit.M3);
			rateLine2.ConversionFactor = ConversionFactor.Standard.Metric.Sea;
			rateLine2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)3m;

			var rateEntry3 = clientRate.AddRateEntry("LCL", "LCL", "SGSIN", "GBLON");
			var rateLine3a = rateEntry3.RateLines[0];
			rateLine3a.TL_RateCalculator = UnitCalculator.Code;
			rateLine3a.TL_WeightVolume = "M3";
			rateLine3a.ConversionFactor = new ConversionFactor(800m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			rateLine3a.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)50m;
			var rateLine3b = rateEntry3.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.M3);
			rateLine3b.ConversionFactor = new ConversionFactor(800m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			rateLine3b.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;

			var rateEntry4 = clientRate.AddRateEntry("ORG", "LCL", "AUSYD", "");
			var rateLine4a = rateEntry4.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.M3);
			rateLine4a.ConversionFactor = ConversionFactor.Empty;
			rateLine4a.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1m;
			var rateLine4b = rateEntry4.AddRateLine("OCART", CartageCalculator.Code, QuantityUnit.M3);
			rateLine4b.ConversionFactor = new ConversionFactor(400m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			rateLine4b.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)10m;

			var rateEntry5 = clientRate.AddRateEntry("DST", "LCL", "", "SGSIN");
			var rateLine5a = rateEntry5.AddRateLine("DDOC", UnitCalculator.Code, QuantityUnit.M3);
			rateLine5a.ConversionFactor = ConversionFactor.Empty;
			rateLine5a.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1m;
			var rateLine5b = rateEntry5.AddRateLine("DCART", CartageCalculator.Code, QuantityUnit.M3);
			rateLine5b.ConversionFactor = new ConversionFactor(250m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			rateLine5b.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)10m;

			var rateEntry6 = clientRate.AddRateEntry("DST", "LCL", "", "GBLON");
			var rateLine6a = rateEntry6.AddRateLine("DDOC", UnitCalculator.Code, QuantityUnit.M3);
			rateLine6a.ConversionFactor = ConversionFactor.Empty;
			rateLine6a.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1m;
			var rateLine6b = rateEntry6.AddRateLine("DCART", CartageCalculator.Code, QuantityUnit.M3);
			rateLine6b.ConversionFactor = new ConversionFactor(250m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			rateLine6b.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)10m;

			Factory.Save();

			#endregion

			var testObject = new AutoRatingObject("AUSYD", "SGSIN", FreightMode.LCL, null, 2000M, 1M, "NEWTESSYD");

			var autoRater = new FreightAutoRater(new RatingContext());
			var results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals(6, results.Count);

			var info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["FRT"].PK);
			AssertEquals(4m, info.GetChargeableFromBasisTest.Amount);

			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["FSC"].PK);
			AssertEquals(2m, info.GetChargeableFromBasisTest.Amount);

			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["ODOC"].PK);
			AssertEquals(4m, info.GetChargeableFromBasisTest.Amount);

			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["OCART"].PK);
			AssertEquals(5m, info.GetChargeableFromBasisTest.Amount);

			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["DDOC"].PK);
			AssertEquals(4m, info.GetChargeableFromBasisTest.Amount);

			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["DCART"].PK);
			AssertEquals(8m, info.GetChargeableFromBasisTest.Amount);

			testObject.SetVia(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "SGSIN"));
			testObject.Destination = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "GBLON");
			autoRater = new FreightAutoRater(new RatingContext());
			testObject.ConsumerType = JobInvoicingConsumerTypes.Shipment;
			results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			var frtResults = results.Where(x => x.ChargeCode.PK == Helper.ChargeCodes["FRT"].PK).ToArray();
			var descriptions = frtResults.Select(x => x.SingleLineDescription).ToArray();

			AssertContainsExactElementsInAnyOrder(new[] { "FRT: 2.5 Cubic Meter(s) @ USD 50.00/M3", "FRT: 4 Cubic Meter(s) @ USD 60.00/M3" }, descriptions);
			Assert(frtResults.Where(x => x.GetChargeableFromBasisTest.Amount == 4m).Any());
			Assert(frtResults.Where(x => x.GetChargeableFromBasisTest.Amount == 2.5m).Any());
			AssertEquals(365M, frtResults.Sum(x => x.Amount));

			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["FSC"].PK);
			AssertEquals(2m, info.GetChargeableFromBasisTest.Amount);

			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["BAF"].PK);
			AssertEquals(2.5m, info.GetChargeableFromBasisTest.Amount);

			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["ODOC"].PK);
			AssertEquals(4m, info.GetChargeableFromBasisTest.Amount);

			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["OCART"].PK);
			AssertEquals(5m, info.GetChargeableFromBasisTest.Amount);

			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["DDOC"].PK);
			AssertEquals(2.5m, info.GetChargeableFromBasisTest.Amount);

			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["DCART"].PK);
			AssertEquals(8m, info.GetChargeableFromBasisTest.Amount);
		}

		#endregion

		#region Rail/Road Freight Tests

		public void TestRoadModeDefaultings()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());

			var entry1 = rate.AddRateEntry("DST", "FTL", "", "NL", "", "20GP");
			var entry2 = rate.AddRateEntry("DST", "LRO", "", "NL");
			var entry3 = rate.AddRateEntry("DST", "FRO", "", "NL");
			var entry4 = rate.AddRateEntry("DST", "ROA", "", "NL");

			var line1 = entry1.AddRateLine("DFSC", UnitCalculator.Code, RatingConstants.Units.KG);
			var line2 = entry2.AddRateLine("DFSC", UnitCalculator.Code, RatingConstants.Units.KG);
			var line3 = entry3.AddRateLine("DFSC", UnitCalculator.Code, RatingConstants.Units.KG);
			var line4 = entry4.AddRateLine("DFSC", UnitCalculator.Code, RatingConstants.Units.KG);

			line1.GetCalculator<UnitCalculator>().PerUnit = .05m;
			line2.GetCalculator<UnitCalculator>().PerUnit = .06m;
			line3.GetCalculator<UnitCalculator>().PerUnit = .07m;
			line4.GetCalculator<UnitCalculator>().PerUnit = .08m;

			Factory.Save();

			var autoRater = new FreightAutoRater(new RatingContext());

			var autoRatingObject = new AutoRatingObject("DE", "NL", FreightMode.FTL, new TestContainers(Factory, "20GP", 1), 10000, 5M, rate.Header);
			var info = new SimpleArInfo
			{
				InvoiceLineDesc = "Delivery Fuel Surcharge",
				Amount = 500.00m,
				CalculationSingleLineDescription = "DFSC: 10000 Kilogram(s) @ EUR 0.05/KG"
			};

			AssertRatingResults(new[] { info }, autoRater.AutoRate(new AutoRatingProxy(autoRatingObject), CostSell.Revenue));

			autoRatingObject = new AutoRatingObject("DE", "NL", FreightMode.FTL, null, 10000, 5M, rate.Header);
			info = new SimpleArInfo
			{
				InvoiceLineDesc = "Delivery Fuel Surcharge",
				Amount = 800.00m,
				CalculationSingleLineDescription = "DFSC: 10000 Kilogram(s) @ EUR 0.08/KG"
			};

			AssertRatingResults(new[] { info }, autoRater.AutoRate(new AutoRatingProxy(autoRatingObject), CostSell.Revenue));

			autoRatingObject = new AutoRatingObject("DE", "NL", FreightMode.LRO, null, 10000, 5M, rate.Header);
			info = new SimpleArInfo
			{
				InvoiceLineDesc = "Delivery Fuel Surcharge",
				Amount = 600.00m,
				CalculationSingleLineDescription = "DFSC: 10000 Kilogram(s) @ EUR 0.06/KG"
			};

			AssertRatingResults(new[] { info }, autoRater.AutoRate(new AutoRatingProxy(autoRatingObject), CostSell.Revenue));

			autoRatingObject = new AutoRatingObject("DE", "NL", FreightMode.FRO, null, 10000, 5M, rate.Header);
			info = new SimpleArInfo
			{
				InvoiceLineDesc = "Delivery Fuel Surcharge",
				Amount = 700.00m,
				CalculationSingleLineDescription = "DFSC: 10000 Kilogram(s) @ EUR 0.07/KG"
			};

			AssertRatingResults(new[] { info }, autoRater.AutoRate(new AutoRatingProxy(autoRatingObject), CostSell.Revenue));
		}

		public void TestRoadFTL()
		{
			SetupRailRoadRates(FreightMode.FTL);

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.FTL, new TestContainers(Factory, "VAN1", 3), 2750M, 1.5M, "NEWTESSYD");

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Result Count", 4, results.Count);
			var info = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			AssertEquals("FRT Chargeable Amount", 3M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", "VAN1", info.GetChargeableFromBasisTest.Unit);
			AssertEquals("FRT Sell Amount", 375M, info.Amount);
		}

		public void TestRoadFCL()
		{
			SetupRailRoadRates(FreightMode.FRO);

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.FRO, new TestContainers(Factory, "20GP", 3), 2750M, 1.5M, "NEWTESSYD");
			testObject.DeliveryCartageEquipment = "SDL";

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Result Count", 4, results.Count);
			var info = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			AssertEquals("FRT Chargeable Amount", 3M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", "20GP", info.GetChargeableFromBasisTest.Unit);
			AssertEquals("FRT Sell Amount", 7500M, info.Amount);
		}

		public void TestRailFCL()
		{
			SetupRailRoadRates(FreightMode.FRA);

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.FRA, new TestContainers(Factory, "20GP", 3), 2750M, 1.5M, "NEWTESSYD");
			testObject.DeliveryCartageEquipment = "SDL";

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Result Count", 4, results.Count);
			var info = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			AssertEquals("FRT Chargeable Amount", 3M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", "20GP", info.GetChargeableFromBasisTest.Unit);
			AssertEquals("FRT Sell Amount", 7500M, info.Amount);
		}

		public void TestRoadLCL()
		{
			SetupRailRoadRates(FreightMode.LRO);

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LRO, null, 2750M, 1.5M, "NEWTESSYD");
			testObject.DeliveryCartageEquipment = "SDL";

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Result Count", 4, results.Count);
			var info = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			AssertEquals("FRT Chargeable Weight", 2.750M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", "M3", info.GetChargeableFromBasisTest.Unit);
			AssertEquals("FRT Sell Amount", 343.75M, info.Amount);
		}

		public void TestRailLCL()
		{
			SetupRailRoadRates(FreightMode.LRA);

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LRA, null, 2750M, 1.5M, "NEWTESSYD");
			testObject.DeliveryCartageEquipment = "SDL";

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Result Count", 4, results.Count);
			var info = results.First(r => r.ChargeCode.PK == TestFRT.PK);
			AssertEquals("FRT Chargeable Weight", 2.750M, info.GetChargeableFromBasisTest.Amount);
			AssertEquals("FRT Chargeable Unit", "M3", info.GetChargeableFromBasisTest.Unit);
			AssertEquals("FRT Sell Amount", 343.75M, info.Amount);
		}

		void SetupRailRoadRates(FreightMode mode)
		{
			var testRate = Factory.New<ClientRate>();
			InsertClientChargeCodesForAutoRaterTests(Factory);
			testRate.TH_OH = NewClient.PK;
			testRate.Header.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			ZString generalMode = (mode & FreightMode.RAI) != 0 ? Core.Constants.RateMode.RAI : Core.Constants.RateMode.ROA;

			if (mode == FreightMode.LRO || mode == FreightMode.LRA || mode == FreightMode.FTL)
			{
				var lCLRateEntry = testRate.AddRateEntry("LCL", mode.ToString(), "AUSYD", "USLAX", "STD", "");
				lCLRateEntry.RateLines.RemoveAndDeleteAll();
				var lCLRateLine = lCLRateEntry.AddRateLine(TestFRT.AC_Code, CombinedCalculator.Code, QuantityUnit.M3);
				lCLRateLine.ConversionFactor = ConversionFactor.Standard.Metric.Sea;
				lCLRateLine.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)150m;
				lCLRateLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)125m;
				if (mode == FreightMode.FTL)
				{
					lCLRateEntry.TI_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "VAN1").PK;
					lCLRateLine.TL_WeightVolume = RatingConstants.Units.CN;
				}
			}
			else
			{
				// FCL
				var fCLRateEntry = testRate.AddRateEntry("FCL", generalMode, "AUSYD", "USLAX", "STD", "20GP");
				fCLRateEntry.RateLines.RemoveAndDeleteAll();
				var fCLRateLine = fCLRateEntry.AddRateLine(TestFRT.AC_Code, CombinedCalculator.Code, QuantityUnit.CN);
				fCLRateLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2500m;
			}

			//Origin
			var oRGRateEntry1 = testRate.AddRateEntry("ORG", mode.ToString(), "AUSYD", "");
			var oRGRateLine1 = oRGRateEntry1.AddRateLine(TestTHC.AC_Code, FlatCalculator.Code);
			((FlatCalculator)oRGRateLine1.Calculator).BaseRate = 50M;

			var oRGRateEntry2 = testRate.AddRateEntry("ORG", generalMode, "AUSYD", "");
			var quantityUnit = (mode & FreightMode.Containerised) != 0 ? RatingConstants.Units.CN : RatingConstants.Units.M3;
			var oRGRateLine2 = oRGRateEntry2.AddRateLine(TestBBK.AC_Code, CombinedCalculator.Code, quantityUnit);
			oRGRateLine2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)10m;

			var oRGRateEntry3 = testRate.AddRateEntry("ORG", "ALL", "AUSYD", "");
			var oRGRateLine3 = oRGRateEntry3.AddRateLine(TestANY.AC_Code, FlatCalculator.Code);
			((FlatCalculator)oRGRateLine3.Calculator).BaseRate = 30M;

			Factory.Save();
		}

		#endregion

		#region Several Pack Lines Different Commodities

		public void TestWithSeveralPackLinesHavingDifferentCommodities()
		{
			var org = Helper.NewOrgHeader(1);

			var companyTariff = Helper.NewCompanyTariff();
			var costEntry = companyTariff.AddRateEntry("AIR", "LSE", "AUSYD", "KRSEL", "", "");
			costEntry.TI_RH_NKCommodityCode = ZString.Empty;
			costEntry.RateLines.RemoveAndDeleteAll();

			var tariffLine = costEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			tariffLine.Calculator[UnitCalculator.Items.Operator.UNT] = (ZDecimal)5m;

			companyTariff.Factory.Save();

			var rate = Helper.NewClientRate(org);
			var rateEntry1 = rate.AddRateEntry("AIR", "LSE", "AUSYD", "KRSEL", "", "");
			rateEntry1.TI_RH_NKCommodityCode = "ALUM";
			rateEntry1.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.Calculator[UnitCalculator.Items.Operator.UNT] = (ZDecimal)4m;

			var rateEntry2 = rate.AddRateEntry("AIR", "LSE", "AUSYD", "KRSEL", "", "");
			rateEntry2.TI_RH_NKCommodityCode = "AABT";
			rateEntry2.RateLines.RemoveAndDeleteAll();

			var rateLine2 = rateEntry2.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine2.Calculator[UnitCalculator.Items.Operator.UNT] = (ZDecimal)3m;

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "KRSEL", FreightMode.LSE, null, 0m, 0m, rate.Header);

			var parts = new RateablePartList { WeightUnit = "KG", HasCommodity = true };
			parts.AddPart(new RateablePart { Weight = 300, CommodityCode = "ALUM" });
			parts.AddPart(new RateablePart { Weight = 400, CommodityCode = "ALUM" });
			parts.AddPart(new RateablePart { Weight = 100, CommodityCode = "ALUM" });
			testObject.Measures.AddPartList(MeasureType.Weight, parts);

			testObject.Measures.SetChargeableWithCommodity("", Core.Constants.Weight.Kilograms, 800m, 800m, 800m);
			testObject.Creditors = Creditors.New(GetNewTestOrgWithSourceFromHeader(TransportProvider1));

			var autoRater = new FreightAutoRater(new RatingContext());
			var results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("FRT: 800 Kilogram(s) @ AUD 4.00/KG", results[0].SingleLineDescription);
			AssertEquals(3200m, results[0].Amount);

			AssertEquals("Result Count", 1, results.Count);

			testObject = new AutoRatingObject("AUSYD", "KRSEL", FreightMode.LSE, null, 0m, 0m, rate.Header);

			parts = new RateablePartList { WeightUnit = "KG", HasCommodity = true };
			parts.AddPart(new RateablePart { Weight = 300, CommodityCode = "ALUM" });
			parts.AddPart(new RateablePart { Weight = 400, CommodityCode = "AABT" });
			parts.AddPart(new RateablePart { Weight = 100, CommodityCode = "APLC" });
			parts.AddPart(new RateablePart { Weight = 200 });
			testObject.Measures.AddPartList(MeasureType.Weight, parts);

			testObject.Measures.SetChargeableWithCommodity("", Core.Constants.Weight.Kilograms, 1000m, 1000m, 1000m);

			testObject.Creditors = Creditors.New(GetNewTestOrgWithSourceFromHeader(TransportProvider1));

			autoRater = new FreightAutoRater(new RatingContext());
			results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			var descriptions = results.Select(x => x.SingleLineDescription).ToArray();
			AssertContainsExactElementsInAnyOrder(new[] { "FRT: 300 Kilogram(s) @ AUD 4.00/KG", "FRT: 400 Kilogram(s) @ AUD 3.00/KG", "FRT: 300 Kilogram(s) @ AUD 5.00/KG" }, descriptions);

			AssertEquals(3900m, results.Sum(x => x.Amount));
			AssertEquals("Result Count", 3, results.Count);
		}

		#endregion

		#region Warehouse

		#region TestWarehouseRating_SupportsConsumeTypeWarehouseAdHocServiceJob

		public void TestWarehouseRating_SupportsConsumeTypeWarehouseAdHocServiceJob()
		{
			var warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IWhsWarehouse)));
			warehouse[WhsWarehouseSchema.WW_WarehouseCode] = "W1";

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());

			var entry = rate.AddRateEntry("WHS", "ALL", "", "");
			entry.TI_WW_Warehouse = warehouse.PK;
			var line = entry.AddRateLine("ODOC", FlatCalculator.Code);
			line.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)100m;

			Factory.Save();

			var testObject = new AutoRatingObject();
			testObject.ConsumerType = JobInvoicingConsumerTypes.WarehouseAdHocServiceJob;
			testObject.RateTypeToUse = RateType.Warehouse;
			testObject.Measures.SetQuantity(MeasureType.Weight, 0m, Core.Constants.Weight.Kilograms);
			testObject.Measures.SetQuantity(MeasureType.Volume, 0m, Core.Constants.Volume.CubicMetres);
			Action<RateableMeasureSet> lazyPopulate = measures => { measures.AddWarehousePackage(0, 0, 100m, warehouse.PK, "", ""); };
			testObject.Measures.CreateWarehousePackageList(lazyPopulate);
			testObject.DebtorOrgs[RatingDebtorOrgTypes.LC] = rate.Header;

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals(1, results.Count);
			AssertEquals("ODOC: Base Rate AUD 100.00", results[0].SingleLineDescription);
			AssertEquals(100m, results[0].Amount);
		}

		#endregion

		public void TestWarehouseRating_ChecksWarehouse()
		{
			var warehouse1 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IWhsWarehouse)));
			warehouse1[WhsWarehouseSchema.WW_WarehouseCode] = "W1";

			var warehouse2 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IWhsWarehouse)));
			warehouse1[WhsWarehouseSchema.WW_WarehouseCode] = "W2";

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());

			var entry = rate.AddRateEntry("WHS", "ALL", "", "");
			entry.TI_WW_Warehouse = warehouse1.PK;
			var line1 = entry.AddRateLine("ODOC", FlatCalculator.Code);
			line1.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)100m;

			var entry2 = rate.AddRateEntry("WHS", "ALL", "", "");
			entry2.TI_WW_Warehouse = warehouse2.PK;
			var line2 = entry2.AddRateLine("ODOC", FlatCalculator.Code);
			line2.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)300m;

			Factory.Save();

			var testObject = new AutoRatingObject();
			testObject.ConsumerType = JobInvoicingConsumerTypes.WarehouseInwards;
			testObject.RateTypeToUse = RateType.Warehouse;
			testObject.Measures.SetQuantity(MeasureType.Weight, 0m, Core.Constants.Weight.Kilograms);
			testObject.Measures.SetQuantity(MeasureType.Volume, 0m, Core.Constants.Volume.CubicMetres);

			Action<RateableMeasureSet> lazyPopulate = measures => { measures.AddWarehousePackage(0, 0, 100m, warehouse1.PK, "", ""); };
			testObject.Measures.CreateWarehousePackageList(lazyPopulate);
			testObject.DebtorOrgs[RatingDebtorOrgTypes.LC] = rate.Header;

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals(1, results.Count);
			AssertEquals("ODOC: Base Rate AUD 100.00", results[0].SingleLineDescription);
			AssertEquals(100m, results[0].Amount);
		}

		public void TestWarehouseRating_PackageCommodity()
		{
			var warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IWhsWarehouse)));
			warehouse[WhsWarehouseSchema.WW_WarehouseCode] = "W1";

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());

			var entry = rate.AddRateEntry("WHS", "ALL", "", "", commodity: "HAZ");
			entry.TI_WW_Warehouse = warehouse.PK;

			var line1 = entry.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.PK);
			line1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)10m;

			Factory.Save();

			var testObject = new AutoRatingObject();
			testObject.ConsumerType = JobInvoicingConsumerTypes.WarehouseInwards;
			testObject.RateTypeToUse = RateType.Warehouse;

			Action<RateableMeasureSet> lazyPopulate = measures =>
			{
				measures.AddWarehousePackage(0, 0, 100m, warehouse.PK, Constants.PkgUnit.Pallet, "HAZ");
				measures.AddWarehousePackage(0, 0, 100m, warehouse.PK, Constants.PkgUnit.Pallet, "DSH");
			};
			testObject.Measures.CreateWarehousePackageList(lazyPopulate);
			testObject.DebtorOrgs[RatingDebtorOrgTypes.LC] = rate.Header;

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals(1, results.Count);
			AssertEquals("ODOC: 100 Package(s) @ AUD 10.00/Package", results[0].SingleLineDescription);
			AssertEquals(1000m, results[0].Amount);
		}

		public void TestWarehouseRating_JobMinimums_SpecificProductRates()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());

			var part1 = Helper.NewOrgSupplierPart(rate.Header);
			var part2 = Helper.NewOrgSupplierPart(rate.Header);

			var entry = rate.AddRateEntry("WHS", "ALL", "", "");

			var line1 = entry.AddRateLine("ODOC", UnitCalculator.Code, Constants.PkgUnit.Unit);
			line1.TL_OP_ProductNumber = part1.PK;
			line1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1.5m;

			var line2 = entry.AddRateLine("ODOC", UnitCalculator.Code, Constants.PkgUnit.Unit);
			line2.TL_OP_ProductNumber = part2.PK;
			line2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2.5m;

			var line3 = entry.AddRateLine("ODOC", MinimumCalculator.Code);
			line3.Calculator[MinimumCalculator.Items.MIN] = (ZDecimal)50m;
			line3.Calculator[MinimumCalculator.Items.MinimumType] = (ZString)CalculatorConstants.Text.MIN_ChargeCode;

			Factory.Save();

			var testObject = new AutoRatingObject();
			testObject.ConsumerType = JobInvoicingConsumerTypes.WarehouseInwards;
			testObject.RateTypeToUse = RateType.Warehouse;
			testObject.Measures.SetQuantity(MeasureType.Weight, 0m, Core.Constants.Weight.Kilograms);
			testObject.Measures.SetQuantity(MeasureType.Volume, 0m, Core.Constants.Volume.CubicMetres);

			testObject.Measures.CreateWarehouseProductList(null, includeProductAttributes: false);
			// 20 x 1.5 = $30
			testObject.Measures.AddWarehouseProduct((0, null), (0, null), 20m, ZGuid.Empty, part1.PK, "");
			// 10 x 2.5 = $25
			testObject.Measures.AddWarehouseProduct((0, null), (0, null), 10m, ZGuid.Empty, part2.PK, "");
			testObject.DebtorOrgs[RatingDebtorOrgTypes.LC] = rate.Header;

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);

			var expected = new[]
							{
								new SimpleArInfo
									{
										Amount = 30m,
										InvoiceLineDesc = "Origin Documentation Fee - PROD1 (###1)",
										CalculationSingleLineDescription = "ODOC: 20 Unit(s) @ AUD 1.50/Unit"
									},
								new SimpleArInfo
									{
										Amount = 25m,
										InvoiceLineDesc = "Origin Documentation Fee - PROD2 (###2)",
										CalculationSingleLineDescription = "ODOC: 10 Unit(s) @ AUD 2.50/Unit"
									}
							};

			AssertRatingResults(expected, results);

			line3.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)60m;
			Factory.Save();

			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);

			expected = new[]
						{
							new SimpleArInfo
								{
									Amount = 60m,
									InvoiceLineDesc = "Origin Documentation Fee",
									CalculationSingleLineDescription = "ODOC: MIN AUD 60.00 (Charge Code Minimum)"
									 }
						};

			AssertRatingResults(expected, results);
		}

		public void TestWarehouseRating_JobMinimums_GeneralProductRates()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());

			var part1 = Helper.NewOrgSupplierPart(rate.Header);
			var part2 = Helper.NewOrgSupplierPart(rate.Header);

			var entry = rate.AddRateEntry("WHS", "ALL", "", "");

			var line1 = entry.AddRateLine("ODOC", UnitCalculator.Code, Constants.PkgUnit.Unit);
			line1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1.5m;

			var line2 = entry.AddRateLine("ODOC", MinimumCalculator.Code);
			line2.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)30m;
			line2.Calculator[MinimumCalculator.Items.MinimumType] = (ZString)CalculatorConstants.Text.MIN_ChargeCode;

			Factory.Save();

			var testObject = new AutoRatingObject();
			testObject.ConsumerType = JobInvoicingConsumerTypes.WarehouseInwards;
			testObject.RateTypeToUse = RateType.Warehouse;
			testObject.Measures.SetQuantity(MeasureType.Weight, 0m, Core.Constants.Weight.Kilograms);
			testObject.Measures.SetQuantity(MeasureType.Volume, 0m, Core.Constants.Volume.CubicMetres);

			testObject.Measures.CreateWarehouseProductList(null, includeProductAttributes: false);
			testObject.Measures.AddWarehouseProduct((0, null), (0, null), 20m, ZGuid.Empty, part1.PK, "");
			testObject.Measures.AddWarehouseProduct((0, null), (0, null), 10m, ZGuid.Empty, part2.PK, "");
			testObject.DebtorOrgs[RatingDebtorOrgTypes.LC] = rate.Header;

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals(2, results.Count);
			AssertEquals(30m, results[0].Amount);
			AssertEquals("Origin Documentation Fee - PROD1 (###1)", results[0].InvoiceLineDescription);
			AssertEquals("ODOC: 20 Unit(s) @ AUD 1.50/Unit", results[0].SingleLineDescription);
			AssertEquals(15m, results[1].Amount);
			AssertEquals("Origin Documentation Fee - PROD2 (###2)", results[1].InvoiceLineDescription);
			AssertEquals("ODOC: 10 Unit(s) @ AUD 1.50/Unit", results[1].SingleLineDescription);

			line2.Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)50m;
			Factory.Save();

			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals(1, results.Count);
			AssertEquals(50m, results[0].Amount);
			AssertEquals("Origin Documentation Fee", results[0].InvoiceLineDescription);
			AssertEquals("ODOC: MIN AUD 50.00 (Charge Code Minimum)", results[0].SingleLineDescription);
		}

		public void TestWarehouseRating_IsJobLevelFlag()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());

			var part1 = Helper.NewOrgSupplierPart(rate.Header);
			var part2 = Helper.NewOrgSupplierPart(rate.Header);
			var part3 = Helper.NewOrgSupplierPart(rate.Header);

			var entry = rate.AddRateEntry("WHS", "ALL", "", "");

			var rateForProduct2 = entry.AddRateLine("ODOC", UnitCalculator.Code, Constants.PkgUnit.Unit);
			rateForProduct2.TL_OP_ProductNumber = part2.PK;
			rateForProduct2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1.5m;

			// Rate for a product not in the warehouse
			var rateToIgnore = entry.AddRateLine("ODOC", UnitCalculator.Code, Constants.PkgUnit.Unit);
			rateToIgnore.TL_OP_ProductNumber = part3.PK;
			rateToIgnore.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2.5m;

			var rateForAllProducts = entry.AddRateLine("ODOC", UnitCalculator.Code, Constants.PkgUnit.Unit);
			rateForAllProducts.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)3.5m;

			Factory.Save();

			AssertEquals(rateForProduct2.TL_IsWhsJobLevelCharge, false);
			AssertEquals(rateToIgnore.TL_IsWhsJobLevelCharge, false);
			AssertEquals(rateForAllProducts.TL_IsWhsJobLevelCharge, false);

			var testObject = new AutoRatingObject();
			testObject.ConsumerType = JobInvoicingConsumerTypes.WarehouseInwards;
			testObject.RateTypeToUse = RateType.Warehouse;
			testObject.Measures.SetQuantity(MeasureType.JobVolume, 0m, "M3");
			testObject.Measures.SetQuantity(MeasureType.JobWeight, 0m, "");

			testObject.Measures.CreateWarehouseProductList(null, includeProductAttributes: false);
			testObject.Measures.AddWarehouseProduct((0, null), (0, null), 20m, ZGuid.Empty, part1.PK, "");
			testObject.Measures.AddWarehouseProduct((0, null), (0, null), 10m, ZGuid.Empty, part2.PK, "");
			testObject.Measures.AddWarehouseProduct((0, null), (0, null), 2m, ZGuid.Empty, part2.PK, "");
			testObject.Measures.SetQuantity(MeasureType.JobUnit, 13m, "");
			testObject.Measures.SetQuantity(MeasureType.Package, 3m, "");
			testObject.Measures.SetQuantity(MeasureType.ChargeablePallet, 2m, "");
			testObject.DebtorOrgs[RatingDebtorOrgTypes.LC] = rate.Header;

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);

			// For Product 1 the rate for all products is best, since there's no specific rate
			var product1Expected = new SimpleArInfo
			{
				Amount = 70m,
				InvoiceLineDesc = "Origin Documentation Fee - PROD1 (###1)",
				CalculationSingleLineDescription = "ODOC: 20 Unit(s) @ AUD 3.50/Unit"
			};

			var expected = new[]
							{
								// For product 2 there is a specific rate that is best
								new SimpleArInfo
									{
										Amount = 18m,
										InvoiceLineDesc = "Origin Documentation Fee - PROD2 (###2)",
										CalculationSingleLineDescription = "ODOC: 12 Unit(s) @ AUD 1.50/Unit"
									},
								product1Expected
							};

			AssertRatingResults(expected, results);

			AssertRatingResults(expected, results);

			// This part of the test seems unrealistic.
			// The rate for product 2 is made job level so it applies to the job total amount, not each line.
			// However, the job total amount does not have a product attribute.
			// The rate will match the job because rating will fallback to looking at all the lines, and there are some lines with product 2.
			// The test expects that the rate description will include the name of product 2, but with an amount that is the job level amount
			// which is the total for all products.
			// So the rate is a mix of both job level and line level values, which makes little sense.
			// It would be more sensible if job level rate lines did not allow the product field (TL_OP_Product) to be entered.
			// They already restrict the unit field to those values that exists as job level quantities.
			// Couldn't find any learning material that covers this case.
			// Note, if there wasn't a second rate, the job-level charge for product 2 would NOT include the product name
			// since the name requires a filter on product and that requires a rate with a MeasureType that has the product dimension.
			rateForProduct2.TL_IsWhsJobLevelCharge = true;
			rateToIgnore.TL_IsWhsJobLevelCharge = true;

			Factory.Save();

			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);

			expected = new[]
						{
							product1Expected,
							new SimpleArInfo
								{
									Amount = 19.5m,
									InvoiceLineDesc = "Origin Documentation Fee - PROD2 (###2)",
									CalculationSingleLineDescription = "ODOC: 13 Unit(s) @ AUD 1.50/Unit"
								}
						};

			AssertRatingResults(expected, results);
		}

		public void TestWarehouseRating_Simple()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());

			var part = Helper.NewOrgSupplierPart(rate.Header);
			var part1 = Helper.NewOrgSupplierPart(rate.Header);
			var part2 = Helper.NewOrgSupplierPart(rate.Header);

			var entry = rate.AddRateEntry("WHS", "ALL", "", "");

			var line1 = entry.AddRateLine("ODOC", UnitCalculator.Code, Constants.PkgUnit.Unit);
			line1.TL_OP_ProductNumber = part1.PK;
			line1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1.5m;

			var line2 = entry.AddRateLine("ODOC", UnitCalculator.Code, Constants.PkgUnit.Unit);
			line2.TL_OP_ProductNumber = part2.PK;
			line2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2.5m;

			var line = entry.AddRateLine("ODOC", UnitCalculator.Code, Constants.PkgUnit.Unit);
			line.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)3.5m;

			Factory.Save();

			var testObject = new AutoRatingObject();
			testObject.ConsumerType = JobInvoicingConsumerTypes.WarehouseInwards;
			testObject.RateTypeToUse = RateType.Warehouse;
			testObject.Measures.CreateWarehouseProductList(null, includeProductAttributes: false);
			testObject.Measures.AddWarehouseProduct((0, null), (0, null), 20m, ZGuid.Empty, part.PK, "");
			testObject.Measures.AddWarehouseProduct((100m, null), (0, null), 10m, ZGuid.Empty, part1.PK, "");
			testObject.DebtorOrgs[RatingDebtorOrgTypes.LC] = rate.Header;

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);

			var expected = new[]
				{
					new SimpleArInfo
						{
							Amount = 15m,
							InvoiceLineDesc = "Origin Documentation Fee - PROD2 (###2)",
							CalculationSingleLineDescription = "ODOC: 10 Unit(s) @ AUD 1.50/Unit"
						},
					new SimpleArInfo
						{
							Amount = 70m,
							InvoiceLineDesc = "Origin Documentation Fee - PROD1 (###1)",
							CalculationSingleLineDescription = "ODOC: 20 Unit(s) @ AUD 3.50/Unit"
						}
				};

			AssertRatingResults(expected, results);

			testObject.RateableMeasures = new RateableMeasureSet();
			testObject.Measures.CreateWarehouseProductList(null, includeProductAttributes: false);
			testObject.Measures.AddWarehouseProduct((100m, null), (0.5m, null), 10m, ZGuid.Empty, part1.PK, "");
			testObject.Measures.AddWarehouseProduct((150m, null), (0.5m, null), 1m, ZGuid.Empty, part2.PK, "");

			line1.TL_WeightVolume = QuantityUnit.KG;
			line2.TL_WeightVolume = QuantityUnit.KG;
			line.TL_WeightVolume = QuantityUnit.KG;

			Factory.Save();

			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);

			expected = new[]
				{
					new SimpleArInfo
						{
							Amount = 150m,
							InvoiceLineDesc = "Origin Documentation Fee - PROD2 (###2)",
							CalculationSingleLineDescription = "ODOC: 100 Kilogram(s) @ AUD 1.50/KG"
						},
					new SimpleArInfo
						{
							Amount = 375m,
							InvoiceLineDesc = "Origin Documentation Fee - PROD3 (###3)",
							CalculationSingleLineDescription = "ODOC: 150 Kilogram(s) @ AUD 2.50/KG"
						}
				};

			AssertRatingResults(expected, results);

			line.TL_WeightVolume = QuantityUnit.M3;

			Factory.Save();

			// This annoying test is now trying to rate "line" by volume, but the job has no volume.
			// In the original implementation using old style measures, the volume measure wasn't set.
			// The two parts with weight got their total weight converted to volume using
			// standard conversion factors, which actually comes out to 1.5 M3 and then rounds to 2.
			// Hence "2 Cubic Meters" in the assertion below.
			// To get the same affect with new measures, we just add another product with 2 M3 volume.
			testObject.Measures.AddWarehouseProduct((0m, null), (2m, null), 1m, ZGuid.Empty, ZGuid.Empty, "");
			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);

			expected = new[]
				{
					new SimpleArInfo
						{
							Amount = 150m,
							InvoiceLineDesc = "Origin Documentation Fee - PROD2 (###2)",
							CalculationSingleLineDescription = "ODOC: 100 Kilogram(s) @ AUD 1.50/KG"
						},
					new SimpleArInfo
						{
							Amount = 375m,
							InvoiceLineDesc = "Origin Documentation Fee - PROD3 (###3)",
							CalculationSingleLineDescription = "ODOC: 150 Kilogram(s) @ AUD 2.50/KG"
						},
					new SimpleArInfo
						{
							Amount = 7m,
							InvoiceLineDesc = "Origin Documentation Fee",
							CalculationSingleLineDescription = "ODOC: 2 Cubic Meter(s) @ AUD 3.50/M3"
						}
				};

			AssertRatingResults(expected, results);
		}

		public void TestWarehouseRating_Commodity()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());

			var part1 = Helper.NewOrgSupplierPart(rate.Header);
			part1.OP_RH_NKCommodityCode = "BANA";
			part1.OP_StockKeepingUnit = Constants.PkgUnit.Carton;
			var part2 = Helper.NewOrgSupplierPart(rate.Header);
			part2.OP_RH_NKCommodityCode = "DSH";
			part2.OP_StockKeepingUnit = Constants.PkgUnit.Cradle;

			var entry1 = rate.AddRateEntry("WHS", "ALL", "", "");
			entry1.TI_RH_NKCommodityCode = "BANA";
			var line1 = entry1.AddRateLine("ODOC", UnitCalculator.Code, Constants.PkgUnit.Carton);
			line1.TL_OP_ProductNumber = part1.PK;
			line1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)3m;

			var entry2 = rate.AddRateEntry("WHS", "ALL", "", "");
			entry2.TI_RH_NKCommodityCode = "DSH";
			var line2 = entry2.AddRateLine("OTHC", UnitCalculator.Code, Constants.PkgUnit.Cradle);
			line2.TL_OP_ProductNumber = part2.PK;
			line2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;

			Factory.Save();

			var testObject = new AutoRatingObject();
			testObject.ConsumerType = JobInvoicingConsumerTypes.WarehouseOutwards;
			testObject.RateTypeToUse = RateType.Warehouse;
			testObject.Measures.SetQuantity(MeasureType.Weight, 0m, Core.Constants.Weight.Kilograms);
			testObject.Measures.SetQuantity(MeasureType.Volume, 0m, Core.Constants.Volume.CubicMetres);

			testObject.Measures.CreateWarehouseProductList(null, includeProductAttributes: false);
			testObject.Measures.AddWarehouseProduct((0m, null), (0, null), 20m, ZGuid.Empty, part1.PK, "BANA");
			testObject.Measures.AddWarehouseProduct((0m, null), (0, null), 10m, ZGuid.Empty, part2.PK, "DSH");
			testObject.DebtorOrgs[RatingDebtorOrgTypes.LC] = rate.Header;

			Factory.Save();

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals(2, results.Count);
			var info1 = results.FirstOrDefault(x => x.ChargeCode.AC_Code == "ODOC");
			AssertEquals(60m, info1.Amount);
			AssertEquals("ODOC: 20 Carton(s) @ AUD 3.00/Carton", info1.SingleLineDescription);

			var info2 = results.FirstOrDefault(x => x.ChargeCode.AC_Code == "OTHC");
			AssertEquals(50m, info2.Amount);
			AssertEquals("OTHC: 10 Cradle(s) @ AUD 5.00/Cradle", info2.SingleLineDescription);

			line2.TL_WeightVolume = Constants.PkgUnit.Dozen;

			Factory.Save();

			var result = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);
			var info = result.RateInfoCollection.FirstOrDefault(x => x.ChargeCode.AC_Code == "OTHC");
			AssertNull("RateLine Filtered OTHC-UNT-DOZ-Client Rate TESTORG1\treason:\tProduct ###2 has no DOZ definition.", info);
		}

		// Note, in production there is only one warehouse per adapter, so this test could be dropped
		public void TestWarehouseRating_DifferentWarehouses()
		{
			var warehouse1 = Helper.NewWarehouse();
			var warehouse2 = Helper.NewWarehouse();

			var supplier = Helper.NewOrgHeader();

			var productPart1 = Helper.NewOrgSupplierPart(supplier);
			var productPart2 = Helper.NewOrgSupplierPart(supplier);
			var productPart3 = Helper.NewOrgSupplierPart(supplier);

			var clientRate = Helper.NewClientRate(supplier);
			var warehouseEntry1 = clientRate.AddRateEntry("WHS", "ALL", "", "");
			warehouseEntry1.TI_WW_Warehouse = warehouse1.PK;
			var line = warehouseEntry1.AddRateLine("ODOC", UnitCalculator.Code, Constants.PkgUnit.Unit);
			if (productPart1 != null)
			{
				line.TL_OP_ProductNumber = productPart1.PK;
			}
			line.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1.1m;

			var line1 = warehouseEntry1.AddRateLine("ODOC", UnitCalculator.Code, Constants.PkgUnit.Unit);
			line1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1.5m;

			var warehouseEntry2 = clientRate.AddRateEntry("WHS", "ALL", "", "");
			warehouseEntry2.TI_WW_Warehouse = warehouse2.PK;
			var line2 = warehouseEntry2.AddRateLine("ODOC", UnitCalculator.Code, Constants.PkgUnit.Unit);
			if (productPart2 != null)
			{
				line2.TL_OP_ProductNumber = productPart2.PK;
			}
			line2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2.2m;

			var allEntry = clientRate.AddRateEntry("WHS", "ALL", "", "");
			var line3 = allEntry.AddRateLine("ODOC", UnitCalculator.Code, Constants.PkgUnit.Unit);
			if (productPart3 != null)
			{
				line3.TL_OP_ProductNumber = productPart3.PK;
			}
			line3.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)3.3m;

			var line4 = allEntry.AddRateLine("ODOC", UnitCalculator.Code, Constants.PkgUnit.Unit);
			line4.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)3.4m;

			Factory.Save();

			var testObject = new AutoRatingObject();
			testObject.ConsumerType = JobInvoicingConsumerTypes.WarehouseInwards;
			testObject.RateTypeToUse = RateType.Warehouse;
			testObject.Measures.SetQuantity(MeasureType.Weight, 0m, Core.Constants.Weight.Kilograms);
			testObject.Measures.SetQuantity(MeasureType.Volume, 0m, Core.Constants.Volume.CubicMetres);

			// Note, in production there is only one warehouse per adapter, so this test could be dropped
			testObject.Measures.CreateWarehouseProductList(null, includeProductAttributes: false);
			testObject.Measures.AddWarehouseProduct((0m, null), (0, null), 11m, warehouse1.PK, productPart1.PK, "");
			testObject.Measures.AddWarehouseProduct((0m, null), (0, null), 22m, warehouse1.PK, productPart2.PK, "");
			testObject.Measures.AddWarehouseProduct((0m, null), (0, null), 33m, warehouse1.PK, productPart3.PK, "");
			testObject.Measures.AddWarehouseProduct((0m, null), (0, null), 44m, warehouse2.PK, productPart1.PK, "");
			testObject.Measures.AddWarehouseProduct((0m, null), (0, null), 55m, warehouse2.PK, productPart2.PK, "");
			testObject.Measures.AddWarehouseProduct((0m, null), (0, null), 66m, warehouse2.PK, productPart3.PK, "");
			testObject.DebtorOrgs[RatingDebtorOrgTypes.LC] = supplier;

			var autoRater = new FreightAutoRater(new RatingContext());
			var results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			// total of 583
			AssertResult(results, 12.1m, "11 Unit(s) @ AUD 1.10/Unit");
			AssertResult(results, 149.6m, "44 Unit(s) @ AUD 3.40/Unit");
			AssertResult(results, 33m, "22 Unit(s) @ AUD 1.50/Unit");
			AssertResult(results, 121m, "55 Unit(s) @ AUD 2.20/Unit");
			AssertResult(results, 49.50m, "33 Unit(s) @ AUD 1.50/Unit");
			AssertResult(results, 217.80m, "66 Unit(s) @ AUD 3.30/Unit");
		}

		public void TestWarehouseRating_Cartage()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var supplierPart1 = Helper.NewOrgSupplierPart(clientRate.Header);
			var supplierPart2 = Helper.NewOrgSupplierPart(clientRate.Header);

			var rateEntry = clientRate.AddRateEntry("WHS");
			rateEntry.TI_Mode = Core.Constants.RateMode.ALL;

			var rateLine1 = rateEntry.AddRateLine("ODOC", CartageCalculator.Code, RatingConstants.Units.M3);
			rateLine1.TL_OP_ProductNumber = supplierPart1.PK;
			rateLine1.GetCalculator<CartageCalculator>().PerUnit = 10m;
			rateLine1.ConversionFactor = new ConversionFactor(1000m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

			var rateLine2 = rateEntry.AddRateLine("ODOC", CartageCalculator.Code, RatingConstants.Units.M3);
			rateLine2.TL_OP_ProductNumber = supplierPart2.PK;
			rateLine2.GetCalculator<CartageCalculator>().PerUnit = 50m;
			rateLine2.ConversionFactor = new ConversionFactor(250m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

			Factory.Save();

			var testObject = new AutoRatingObject();
			testObject.RateTypeToUse = RateType.Warehouse;

			testObject.Measures.CreateWarehouseProductList(null, includeProductAttributes: false);
			testObject.Measures.AddWarehouseProduct((1000m, null), (0.8m, null), 1m, ZGuid.Empty, supplierPart1.PK, "");
			testObject.Measures.AddWarehouseProduct((500m, null), (1m, null), 1m, ZGuid.Empty, supplierPart2.PK, "");

			testObject.DebtorOrgs[RatingDebtorOrgTypes.LC] = clientRate.Header;

			var autoRater = new FreightAutoRater(new RatingContext());

			var results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals(1, results.Count);
			AssertResult(results, 110m, "ODOC: 1 Cubic Meter(s) @ AUD 10.00/M3", "ODOC: 2 Cubic Meter(s) @ AUD 50.00/M3");
		}

		public void TestWarehouseRating_LocationType()
		{
			var client = Helper.NewOrgHeader();
			var locationInfo1 = new LocationMeasure(ZGuid.NewZGuid(), "AAA", "LOC1");
			var locationInfo2 = new LocationMeasure(ZGuid.NewZGuid(), "BBB", "LOC2");
			var locationInfo3 = new LocationMeasure(ZGuid.NewZGuid(), "AAA", "LOC3");
			var product1 = Helper.NewOrgSupplierPart(client);
			var product2 = Helper.NewOrgSupplierPart(client);
			var product3 = Helper.NewOrgSupplierPart(client);
			var product4 = Helper.NewOrgSupplierPart(client);

			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry("WHS", "ALL", "", "");
			var line1 = entry.AddRateLine("ODOC", WarehouseLocationTypeCalculator.Code);
			line1.Calculator.AddRateLineItem("AAA", 0m, 100m);
			line1.Calculator.AddRateLineItem("BBB", 0m, 200m);
			line1.Calculator.AddRateLineItem("CCC", 0m, 180m);

			Factory.Save();

			var testObject = new AutoRatingObject();
			testObject.ConsumerType = JobInvoicingConsumerTypes.WarehouseStorage;
			testObject.RateTypeToUse = RateType.Warehouse;
			testObject.Measures.SetQuantity(MeasureType.Weight, 0m, Core.Constants.Weight.Kilograms);
			testObject.Measures.SetQuantity(MeasureType.Volume, 0m, Core.Constants.Volume.CubicMetres);
			testObject.DebtorOrgs[RatingDebtorOrgTypes.LC] = rate.Header;

			testObject.Measures.CreateLocationPalletList(null);
			testObject.Measures.AddLocationPallet(1m, ZGuid.Empty, locationInfo1, product1.PK, ProductAttributesMeasure.Empty, "");
			testObject.Measures.AddLocationPallet(1m, ZGuid.Empty, locationInfo1, product2.PK, ProductAttributesMeasure.Empty, "");
			testObject.Measures.AddLocationPallet(1m, ZGuid.Empty, locationInfo2, product3.PK, ProductAttributesMeasure.Empty, "");
			testObject.Measures.AddLocationPallet(1m, ZGuid.Empty, locationInfo2, product2.PK, ProductAttributesMeasure.Empty, "");
			testObject.Measures.AddLocationPallet(1m, ZGuid.Empty, locationInfo2, product1.PK, ProductAttributesMeasure.Empty, "");
			testObject.Measures.AddLocationPallet(1m, ZGuid.Empty, locationInfo3, product4.PK, ProductAttributesMeasure.Empty, "");
			testObject.Measures.AddLocationPallet(1m, ZGuid.Empty, locationInfo3, product2.PK, ProductAttributesMeasure.Empty, "");
			testObject.Measures.AddLocationPallet(1m, ZGuid.Empty, locationInfo3, product3.PK, ProductAttributesMeasure.Empty, "");
			testObject.Measures.AddLocationPallet(1m, ZGuid.Empty, locationInfo3, product1.PK, ProductAttributesMeasure.Empty, "");

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals(3, results.Count);
			AssertEquals(100m, results[0].Amount);
			AssertEquals("Origin Documentation Fee", results[0].InvoiceLineDescription);
			AssertEquals("ODOC: Base Rate AUD 100.00 (LOC1 (AAA) contains (PROD1, PROD2))", results[0].SingleLineDescription);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.LocationType, "AAA"), results[0].Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.LocationDesc, "LOC1"), results[0].Attributes.Attributes);

			AssertEquals(100m, results[1].Amount);
			AssertEquals("Origin Documentation Fee", results[1].InvoiceLineDescription);
			AssertEquals("ODOC: Base Rate AUD 100.00 (LOC3 (AAA) contains (PROD1, PROD2, PROD3, ...))", results[1].SingleLineDescription);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.LocationType, "AAA"), results[1].Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.LocationDesc, "LOC3"), results[1].Attributes.Attributes);

			AssertEquals(200m, results[2].Amount);
			AssertEquals("Origin Documentation Fee", results[2].InvoiceLineDescription);
			AssertEquals("ODOC: Base Rate AUD 200.00 (LOC2 (BBB) contains (PROD1, PROD2, PROD3))", results[2].SingleLineDescription);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.LocationType, "BBB"), results[2].Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.LocationDesc, "LOC2"), results[2].Attributes.Attributes);
		}

		public void TestWarehouseRating_LocationPallet()
		{
			var client = Helper.NewOrgHeader();
			var locationInfo1 = new LocationMeasure(ZGuid.NewZGuid(), "AAA", "LOC1");
			var locationInfo2 = new LocationMeasure(ZGuid.NewZGuid(), "BBB", "LOC2");
			var locationInfo3 = new LocationMeasure(ZGuid.NewZGuid(), "AAA", "LOC3");
			var product1 = Helper.NewOrgSupplierPart(client);
			var product2 = Helper.NewOrgSupplierPart(client);

			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry("WHS", "ALL", "", "");

			var line1 = entry.AddRateLine("ODOC", UnitCalculator.Code, RatingConstants.Units.PL);
			line1.GetCalculator<UnitCalculator>().PerUnit = 10m;

			var line2 = entry.AddRateLine("ODOC", UnitCalculator.Code, RatingConstants.Units.PL);
			line2.TL_OP_ProductNumber = product2.PK;
			line2.GetCalculator<UnitCalculator>().PerUnit = 8m;

			Factory.Save();

			var testObject = new AutoRatingObject();
			testObject.ConsumerType = JobInvoicingConsumerTypes.WarehouseStorage;
			testObject.RateTypeToUse = RateType.Warehouse;
			testObject.Measures.SetQuantity(MeasureType.Weight, 0m, Core.Constants.Weight.Kilograms);
			testObject.Measures.SetQuantity(MeasureType.Volume, 0m, Core.Constants.Volume.CubicMetres);
			testObject.DebtorOrgs[RatingDebtorOrgTypes.LC] = rate.Header;

			testObject.Measures.CreateLocationPalletList(null);
			testObject.Measures.AddLocationPallet(5m, ZGuid.Empty, locationInfo1, product1.PK, ProductAttributesMeasure.Empty, "");
			testObject.Measures.AddLocationPallet(7m, ZGuid.Empty, locationInfo1, product2.PK, ProductAttributesMeasure.Empty, "");
			testObject.Measures.AddLocationPallet(9m, ZGuid.Empty, locationInfo2, product2.PK, ProductAttributesMeasure.Empty, "");
			testObject.Measures.AddLocationPallet(10m, ZGuid.Empty, locationInfo3, product1.PK, ProductAttributesMeasure.Empty, "");

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);

			var expected = new[]
							{
								new SimpleArInfo
									{
										Amount = 150m,
										InvoiceLineDesc = "Origin Documentation Fee - PROD1 (###1)",
										CalculationSingleLineDescription = "ODOC: 15 Pallet(s) @ AUD 10.00/Pallet"
									},
								new SimpleArInfo
									{
										Amount = 128m,
										InvoiceLineDesc = "Origin Documentation Fee - PROD2 (###2)",
										CalculationSingleLineDescription = "ODOC: 16 Pallet(s) @ AUD 8.00/Pallet"
									}
							};

			AssertRatingResults(expected, results);
		}

		public void TestWarehouseRating_LocationPalletRounding()
		{
			var client = Helper.NewOrgHeader();
			var locationInfo1 = new LocationMeasure(ZGuid.NewZGuid(), "AAA", "LOC1");
			var locationInfo2 = new LocationMeasure(ZGuid.NewZGuid(), "BBB", "LOC2");
			var locationInfo3 = new LocationMeasure(ZGuid.NewZGuid(), "AAA", "LOC3");
			var product1 = Helper.NewOrgSupplierPart(client);
			var product2 = Helper.NewOrgSupplierPart(client);

			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry("WHS", "ALL", "", "");

			var line1 = entry.AddRateLine("ODOC", UnitCalculator.Code, RatingConstants.Units.PL);
			line1.GetCalculator<UnitCalculator>().PerUnit = 10m;
			line1.TL_Rounding = RatingRoundingTypes.UpToHalf;

			var line2 = entry.AddRateLine("ODOC", UnitCalculator.Code, RatingConstants.Units.PL);
			line2.TL_OP_ProductNumber = product2.PK;
			line2.GetCalculator<UnitCalculator>().PerUnit = 8m;
			line2.TL_Rounding = RatingRoundingTypes.UpToHalf;

			Factory.Save();

			var testObject = new AutoRatingObject();
			testObject.ConsumerType = JobInvoicingConsumerTypes.WarehouseStorage;
			testObject.RateTypeToUse = RateType.Warehouse;
			testObject.Measures.SetQuantity(MeasureType.Weight, 0m, Core.Constants.Weight.Kilograms);
			testObject.Measures.SetQuantity(MeasureType.Volume, 0m, Core.Constants.Volume.CubicMetres);
			testObject.DebtorOrgs[RatingDebtorOrgTypes.LC] = rate.Header;

			testObject.Measures.CreateLocationPalletList(null);
			testObject.Measures.AddLocationPallet(0.4m, ZGuid.Empty, locationInfo1, product1.PK, ProductAttributesMeasure.Empty, "");
			testObject.Measures.AddLocationPallet(0.2m, ZGuid.Empty, locationInfo1, product2.PK, ProductAttributesMeasure.Empty, "");
			testObject.Measures.AddLocationPallet(0.1m, ZGuid.Empty, locationInfo2, product2.PK, ProductAttributesMeasure.Empty, "");
			testObject.Measures.AddLocationPallet(0.6m, ZGuid.Empty, locationInfo3, product1.PK, ProductAttributesMeasure.Empty, "");

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);

			var expected = new[]
							{
								new SimpleArInfo
									{
										Amount = 15m,
										InvoiceLineDesc = "Origin Documentation Fee - PROD1 (###1)",
										CalculationSingleLineDescription = "ODOC: 1.5 Pallet(s) @ AUD 10.00/Pallet"
									},
								new SimpleArInfo
									{
										Amount = 8m,
										InvoiceLineDesc = "Origin Documentation Fee - PROD2 (###2)",
										CalculationSingleLineDescription = "ODOC: 1 Pallet(s) @ AUD 8.00/Pallet"
									}
							};

			AssertRatingResults(expected, results);
		}

		public void TestWarehouseRating_ProductAttributes()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());

			var part1 = Helper.NewOrgSupplierPart(rate.Header);
			part1.OP_RH_NKCommodityCode = "HAZ";
			var part2 = Helper.NewOrgSupplierPart(rate.Header);
			var part3 = Helper.NewOrgSupplierPart(rate.Header);

			var entry = rate.AddRateEntry("WHS", "ALL", "", "");

			var line1 = entry.AddRateLine("ODOC", UnitCalculator.Code, Constants.PkgUnit.Unit);
			line1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2m;

			Factory.Save();

			var testObject = new AutoRatingObject();
			testObject.ConsumerType = JobInvoicingConsumerTypes.WarehouseInwards;
			testObject.RateTypeToUse = RateType.Warehouse;
			testObject.Measures.SetQuantity(MeasureType.Weight, 0m, Core.Constants.Weight.Kilograms);
			testObject.Measures.SetQuantity(MeasureType.Volume, 0m, Core.Constants.Volume.CubicMetres);
			testObject.DebtorOrgs[RatingDebtorOrgTypes.LC] = rate.Header;

			testObject.Measures.CreateWarehouseProductList(null, includeProductAttributes: true);
			testObject.Measures.AddWarehouseProduct((0, null), (0, null), 20m, ZGuid.Empty, part1.PK, new ProductAttributesMeasure("AA1", "BB1", "CC1"), "");
			testObject.Measures.AddWarehouseProduct((0, null), (0, null), 15m, ZGuid.Empty, part1.PK, new ProductAttributesMeasure("AA1", "BB2", "CC1"), "");
			testObject.Measures.AddWarehouseProduct((0, null), (0, null), 8m, ZGuid.Empty, part1.PK, new ProductAttributesMeasure("AA1", "BB1", "CC1"), "");
			testObject.Measures.AddWarehouseProduct((0, null), (0, null), 12m, ZGuid.Empty, part2.PK, ProductAttributesMeasure.Empty, "");
			testObject.Measures.AddWarehouseProduct((0, null), (0, null), 11m, ZGuid.Empty, part3.PK, new ProductAttributesMeasure("AA1", "BB1", "CC1"), "");
			testObject.Measures.AddWarehouseProduct((0, null), (0, null), 22m, ZGuid.Empty, part3.PK, new ProductAttributesMeasure("", "BB2", ""), "");
			testObject.Measures.AddWarehouseProduct((0, null), (0, null), 1m, ZGuid.Empty, part2.PK, new ProductAttributesMeasure("", "", "", "SN1"), "");

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals(6, results.Count);

			AssertEquals(56m, results[0].Amount);
			AssertEquals("Origin Documentation Fee - PROD1 (###1) AA1 BB1 CC1", results[0].InvoiceLineDescription);
			AssertEquals("ODOC: 28 Unit(s) @ AUD 2.00/Unit", results[0].SingleLineDescription);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Product, "PROD1"), results[0].Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Commodity, "HAZ"), results[0].Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Attrib1, "AA1"), results[0].Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Attrib2, "BB1"), results[0].Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Attrib3, "CC1"), results[0].Attributes.Attributes);

			AssertEquals(30m, results[1].Amount);
			AssertEquals("Origin Documentation Fee - PROD1 (###1) AA1 BB2 CC1", results[1].InvoiceLineDescription);
			AssertEquals("ODOC: 15 Unit(s) @ AUD 2.00/Unit", results[1].SingleLineDescription);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Product, "PROD1"), results[1].Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Commodity, "HAZ"), results[1].Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Attrib1, "AA1"), results[1].Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Attrib2, "BB2"), results[1].Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Attrib3, "CC1"), results[1].Attributes.Attributes);

			AssertEquals(24m, results[2].Amount);
			AssertEquals("Origin Documentation Fee - PROD2 (###2)", results[2].InvoiceLineDescription);
			AssertEquals("ODOC: 12 Unit(s) @ AUD 2.00/Unit", results[2].SingleLineDescription);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Product, "PROD2"), results[2].Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Commodity, ""), results[2].Attributes.Attributes);

			AssertEquals(2m, results[3].Amount);
			AssertEquals("Origin Documentation Fee - PROD2 (###2) SN1", results[3].InvoiceLineDescription);
			AssertEquals("ODOC: 1 Unit(s) @ AUD 2.00/Unit", results[3].SingleLineDescription);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Product, "PROD2"), results[3].Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Commodity, ""), results[3].Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Attrib1, ""), results[3].Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Attrib2, ""), results[3].Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Attrib3, ""), results[3].Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.SerialNumber, "SN1"), results[3].Attributes.Attributes);

			AssertEquals(22m, results[4].Amount);
			AssertEquals("Origin Documentation Fee - PROD3 (###3) AA1 BB1 CC1", results[4].InvoiceLineDescription);
			AssertEquals("ODOC: 11 Unit(s) @ AUD 2.00/Unit", results[4].SingleLineDescription);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Product, "PROD3"), results[4].Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Commodity, ""), results[4].Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Attrib1, "AA1"), results[4].Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Attrib2, "BB1"), results[4].Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Attrib3, "CC1"), results[4].Attributes.Attributes);

			AssertEquals(44m, results[5].Amount);
			AssertEquals("Origin Documentation Fee - PROD3 (###3) BB2", results[5].InvoiceLineDescription);
			AssertEquals("ODOC: 22 Unit(s) @ AUD 2.00/Unit", results[5].SingleLineDescription);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Product, "PROD3"), results[5].Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Commodity, ""), results[5].Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Attrib1, ""), results[5].Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Attrib2, "BB2"), results[5].Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Attrib3, ""), results[5].Attributes.Attributes);
		}

		public void TestWarehouseRating_DifferentStockKeepingUnit()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());

			var part1 = Helper.NewOrgSupplierPart(rate.Header, Constants.PkgUnit.Bottle);
			Helper.AddPartUnit(part1, Constants.PkgUnit.Bottle, Constants.PkgUnit.Carton, 24);
			Helper.AddPartUnit(part1, Constants.PkgUnit.Carton, Constants.PkgUnit.Pallet, 10);
			var part2 = Helper.NewOrgSupplierPart(rate.Header, Constants.PkgUnit.Box);
			Helper.AddPartUnit(part2, Constants.PkgUnit.Box, Constants.PkgUnit.Pallet, 30);
			var part3 = Helper.NewOrgSupplierPart(rate.Header, Constants.PkgUnit.Unit);
			Helper.AddPartUnit(part3, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 4);

			var entry = rate.AddRateEntry("WHS", "ALL", "", "");

			var line1 = entry.AddRateLine("ODOC", UnitCalculator.Code, Constants.PkgUnit.Pallet);
			line1.GetCalculator<UnitCalculator>().PerUnit = 5m;

			var line2a = entry.AddRateLine("OCART", UnitCalculator.Code, Constants.PkgUnit.Carton);
			line2a.GetCalculator<UnitCalculator>().PerUnit = 1m;
			line2a.TL_OP_ProductNumber = part1.PK;

			var line2b = entry.AddRateLine("OCART", UnitCalculator.Code, Constants.PkgUnit.Box);
			line2b.GetCalculator<UnitCalculator>().PerUnit = 10m;
			line2b.TL_OP_ProductNumber = part2.PK;

			var line2c = entry.AddRateLine("OCART", UnitCalculator.Code, Constants.PkgUnit.Unit);
			line2c.GetCalculator<UnitCalculator>().PerUnit = 150m;
			line2c.TL_OP_ProductNumber = part3.PK;

			Factory.Save();

			var testObject = new AutoRatingObject();
			testObject.ConsumerType = JobInvoicingConsumerTypes.WarehouseInwards;
			testObject.RateTypeToUse = RateType.Warehouse;
			testObject.Measures.SetQuantity(MeasureType.Weight, 0m, Core.Constants.Weight.Kilograms);
			testObject.Measures.SetQuantity(MeasureType.Volume, 0m, Core.Constants.Volume.CubicMetres);
			testObject.DebtorOrgs[RatingDebtorOrgTypes.LC] = rate.Header;

			testObject.Measures.CreateWarehouseProductList(null, includeProductAttributes: false);
			testObject.Measures.AddWarehouseProduct((0, null), (0, null), 480m, ZGuid.Empty, part1.PK, "");
			testObject.Measures.AddWarehouseProduct((0, null), (0, null), 90m, ZGuid.Empty, part2.PK, "");
			testObject.Measures.AddWarehouseProduct((0, null), (0, null), 20m, ZGuid.Empty, part3.PK, "");

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);

			var expected = new[]
							{
								new SimpleArInfo
									{
										Amount = 20m,
										InvoiceLineDesc = "Pick Up Cartage - PROD1 (###1)",
										CalculationSingleLineDescription = "OCART: 20 Carton(s) @ AUD 1.00/Carton"
									},
								new SimpleArInfo
									{
										Amount = 900m,
										InvoiceLineDesc = "Pick Up Cartage - PROD2 (###2)",
										CalculationSingleLineDescription = "OCART: 90 Box(s) @ AUD 10.00/Box"
									},
								new SimpleArInfo
									{
										Amount = 3000m,
										InvoiceLineDesc = "Pick Up Cartage - PROD3 (###3)",
										CalculationSingleLineDescription = "OCART: 20 Unit(s) @ AUD 150.00/Unit"
									},
								new SimpleArInfo
									{
										Amount = 10m,
										InvoiceLineDesc = "Origin Documentation Fee - PROD1 (###1)",
										CalculationSingleLineDescription = "ODOC: 2 Pallet(s) @ AUD 5.00/Pallet"
									},
								new SimpleArInfo
									{
										Amount = 15m,
										InvoiceLineDesc = "Origin Documentation Fee - PROD2 (###2)",
										CalculationSingleLineDescription = "ODOC: 3 Pallet(s) @ AUD 5.00/Pallet"
									},
								new SimpleArInfo
									{
										Amount = 25m,
										InvoiceLineDesc = "Origin Documentation Fee - PROD3 (###3)",
										CalculationSingleLineDescription = "ODOC: 5 Pallet(s) @ AUD 5.00/Pallet"
									}
							};

			AssertRatingResults(expected, results);
		}

		public void TestWarehouseRating_PerDocketLine()
		{
			ZString docketRef1 = "DREF#1";

			ZString docketRef2 = "DREF#2";

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());

			var part1 = Helper.NewOrgSupplierPart(rate.Header);
			var part2 = Helper.NewOrgSupplierPart(rate.Header);

			var entry = rate.AddRateEntry("WHS", "ALL", "", "");

			var line1 = entry.AddRateLine("ODOC", UnitCalculator.Code, Constants.PkgUnit.Unit);
			line1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2m;

			Factory.Save();

			var testObject = new AutoRatingObject();
			testObject.ConsumerType = JobInvoicingConsumerTypes.WarehouseInwards;
			testObject.RateTypeToUse = RateType.Warehouse;
			testObject.Measures.SetQuantity(MeasureType.Weight, 0m, Core.Constants.Weight.Kilograms);
			testObject.Measures.SetQuantity(MeasureType.Volume, 0m, Core.Constants.Volume.CubicMetres);
			testObject.DebtorOrgs[RatingDebtorOrgTypes.LC] = rate.Header;

			testObject.Measures.CreateWarehouseDocketLines(null, useNormalMeasures: true, useStorageMeasures: false,
				optionalAttributes: RateableMeasureSet.WarehouseProductOptionalAttributes.PackageType | RateableMeasureSet.WarehouseProductOptionalAttributes.ProductAttributes);
			testObject.Measures.AddWarehouseDocketNormalLine((0, null), (0, null), 10m, ZGuid.Empty, part1.PK, ProductAttributesMeasure.Empty, "", docketRef1, "");
			testObject.Measures.AddWarehouseDocketNormalLine((0, null), (0, null), 20m, ZGuid.Empty, part2.PK, ProductAttributesMeasure.Empty, "", docketRef1, "");
			testObject.Measures.AddWarehouseDocketNormalLine((0, null), (0, null), 30m, ZGuid.Empty, part2.PK, ProductAttributesMeasure.Empty, "", docketRef2, "");
			testObject.Measures.AddWarehouseDocketNormalLine((0, null), (0, null), 40m, ZGuid.Empty, part2.PK, ProductAttributesMeasure.Empty, "", docketRef2, "");
			testObject.Measures.AddWarehouseDocketNormalLine((0, null), (0, null), 50m, ZGuid.Empty, part1.PK, ProductAttributesMeasure.Empty, "", docketRef2, "");

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);

			var expected = new[]
							{
								new SimpleArInfo
									{
										Amount = 20m,
										InvoiceLineDesc = "Origin Documentation Fee DREF#1 - PROD1 (###1)",
									},
								new SimpleArInfo
									{
										Amount = 40m,
										InvoiceLineDesc = "Origin Documentation Fee DREF#1 - PROD2 (###2)",
									},
								new SimpleArInfo
									{
										Amount = 100m,
										InvoiceLineDesc = "Origin Documentation Fee DREF#2 - PROD1 (###1)",
									},
								new SimpleArInfo
									{
										Amount = 140m,
										InvoiceLineDesc = "Origin Documentation Fee DREF#2 - PROD2 (###2)",
									}
							};

			AssertRatingResults(expected, results);

			var autoRrateInfo = results.RateInfoCollection.Single(o => o.Amount == 20m);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Product, "PROD1"), autoRrateInfo.Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.DocketReference, "DREF#1"), autoRrateInfo.Attributes.Attributes);

			autoRrateInfo = results.RateInfoCollection.Single(o => o.Amount == 40m);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Product, "PROD2"), autoRrateInfo.Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.DocketReference, "DREF#1"), autoRrateInfo.Attributes.Attributes);

			autoRrateInfo = results.RateInfoCollection.Single(o => o.Amount == 100m);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Product, "PROD1"), autoRrateInfo.Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.DocketReference, "DREF#2"), autoRrateInfo.Attributes.Attributes);

			autoRrateInfo = results.RateInfoCollection.Single(o => o.Amount == 140m);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.Product, "PROD2"), autoRrateInfo.Attributes.Attributes);
			AssertCollectionContains(new RateAttribute(JobChargeAttribTypeList.Codes.DocketReference, "DREF#2"), autoRrateInfo.Attributes.Attributes);
		}

		public void TestWarehouseRating_SpecificProductRates()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());

			var part1 = Helper.NewOrgSupplierPart(rate.Header);
			var part2 = Helper.NewOrgSupplierPart(rate.Header);
			var part3 = Helper.NewOrgSupplierPart(rate.Header);

			var entry = rate.AddRateEntry("WHS", "ALL", "", "");

			var line1 = entry.AddRateLine("ODOC", UnitCalculator.Code, Constants.PkgUnit.Unit);
			line1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1.5m;
			line1.TL_OP_ProductNumber = part1.PK;

			var line2 = entry.AddRateLine("ODOC", UnitCalculator.Code, Constants.PkgUnit.Unit);
			line2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1.5m;
			line2.TL_OP_ProductNumber = part2.PK;

			Factory.Save();

			var testObject = new AutoRatingObject();
			testObject.ConsumerType = JobInvoicingConsumerTypes.WarehouseInwards;
			testObject.RateTypeToUse = RateType.Warehouse;
			testObject.Measures.SetQuantity(MeasureType.Weight, 0m, Core.Constants.Weight.Kilograms);
			testObject.Measures.SetQuantity(MeasureType.Volume, 0m, Core.Constants.Volume.CubicMetres);
			testObject.DebtorOrgs[RatingDebtorOrgTypes.LC] = rate.Header;

			testObject.Measures.CreateWarehouseProductList(null, includeProductAttributes: false);
			testObject.Measures.AddWarehouseProduct((0, null), (0, null), 20m, ZGuid.Empty, part1.PK, "");
			testObject.Measures.AddWarehouseProduct((0, null), (0, null), 10m, ZGuid.Empty, part2.PK, "");
			testObject.Measures.AddWarehouseProduct((0, null), (0, null), 5m, ZGuid.Empty, part3.PK, "");

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);

			var expected = new[]
							{
								new SimpleArInfo
									{
										Amount = 30m,
										InvoiceLineDesc = "Origin Documentation Fee - PROD1 (###1)",
										CalculationSingleLineDescription = "ODOC: 20 Unit(s) @ AUD 1.50/Unit"
									},
								new SimpleArInfo
									{
										Amount = 15m,
										InvoiceLineDesc = "Origin Documentation Fee - PROD2 (###2)",
										CalculationSingleLineDescription = "ODOC: 10 Unit(s) @ AUD 1.50/Unit"
									}
							};

			AssertRatingResults(expected, results);
		}

		#region TestWarehouseRating_MeasureTypesAreNotCompatible_ContainerCountAndUnits

		public void TestWarehouseRating_MeasureTypesAreNotCompatible_ContainerCountAndUnits()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry("WHS", "ALL", "", "", "", "20GP");
			var linePerUnit = entry.AddRateLine("ODOC", UnitCalculator.Code, Constants.PkgUnit.Unit);
			linePerUnit.TL_IsOnPallets = false;
			linePerUnit.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1.5m;
			var linePerContainer = entry.AddRateLine("ODOC", UnitCalculator.Code, RatingConstants.Units.CN);
			linePerContainer.TL_IsOnPallets = true;
			linePerContainer.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2.5m;

			Factory.Save();

			var testObject = new AutoRatingObject();
			testObject.ConsumerType = JobInvoicingConsumerTypes.WarehouseInwards;
			testObject.RateTypeToUse = RateType.Warehouse;
			testObject.DebtorOrgs[RatingDebtorOrgTypes.LC] = rate.Header;

			var refContainer20gp = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			testObject.Measures.AddContainerGroup(refContainer20gp.PK, "", false, "", new[] { new MeasureInfo.ContainerInfo() });

			testObject.Measures.CreateWarehouseProductList(null, includeProductAttributes: false);
			testObject.Measures.AddWarehouseProduct((0, null), (0, null), 20m, Helper.NewWarehouse().PK, Helper.NewOrgSupplierPart(rate.Header).PK, "");

			Factory.Save();

			var testAutoRater = new FreightAutoRater(new RatingContext());

			// Job have 1 not palletized container, so rate line for not palletized containers should be used (line1)
			var rateResults1 = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);

			var expected = new[]
				{
					new SimpleArInfo
						{
							Amount = 30m,
							InvoiceLineDesc = "Origin Documentation Fee - PROD1 (###1)",
							CalculationSingleLineDescription = "ODOC: 20 Unit(s) @ AUD 1.50/Unit"
						}
				};

			AssertRatingResults(expected, rateResults1);

			// Job have 1 palletized container and a service charge, so rate line for palletized containers should be used (line2)
			testObject.Measures.RemoveContainerList();
			testObject.Measures.AddContainerGroup(refContainer20gp.PK, "", true, "", new[] { new MeasureInfo.ContainerInfo() });
			var rateResults2 = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);

			expected = new[]
				{
					new SimpleArInfo
						{
							Amount = 2.5m,
							InvoiceLineDesc = "Origin Documentation Fee",
							CalculationSingleLineDescription = "ODOC: 1 20GP Container(s) @ AUD 2.50/Container"
						},
					new SimpleArInfo
						{
							Amount = 30m,
							InvoiceLineDesc = "Origin Documentation Fee - PROD1 (###1)",
							CalculationSingleLineDescription = "ODOC: 20 Unit(s) @ AUD 1.50/Unit"
						}
				};

			AssertRatingResults(expected, rateResults2);

			// Making Both rate lines for not palletized containers.
			linePerContainer.TL_IsOnPallets = false;
			testObject.Measures.RemoveContainerList();
			testObject.Measures.CreateContainerList(includePalletized: true, includeOwnership: true);
			testObject.Measures.AddContainerGroup(refContainer20gp.PK, "", false, "", new[] { new MeasureInfo.ContainerInfo() });

			Factory.Save();

			// Job have 1 not palletized container, so rate lines for not palletized containers should be used (line1 and line2)
			var rateResults3 = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);

			expected = new[]
				{
					new SimpleArInfo
						{
							Amount = 2.5m,
							InvoiceLineDesc = "Origin Documentation Fee",
							CalculationSingleLineDescription = "ODOC: 1 20GP Container(s) @ AUD 2.50/Container"
						},
					new SimpleArInfo
						{
							Amount = 30m,
							InvoiceLineDesc = "Origin Documentation Fee - PROD1 (###1)",
							CalculationSingleLineDescription = "ODOC: 20 Unit(s) @ AUD 1.50/Unit"
						}
				};

			AssertRatingResults(expected, rateResults3);

			// Job have 1 palletized container, so rate lines should only return documentation fee.
			testObject.Measures.RemoveContainerList();
			testObject.Measures.CreateContainerList(includePalletized: true, includeOwnership: true);
			testObject.Measures.AddContainerGroup(refContainer20gp.PK, "", true, "", new[] { new MeasureInfo.ContainerInfo() });
			var rateResults4 = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);

			expected = new[]
				{
					new SimpleArInfo
						{
							Amount = 30m,
							InvoiceLineDesc = "Origin Documentation Fee - PROD1 (###1)",
							CalculationSingleLineDescription = "ODOC: 20 Unit(s) @ AUD 1.50/Unit"
						}
				};

			AssertRatingResults(expected, rateResults4);
		}

		#endregion

		#region TestWarehouseRating_MeasureTypesAreNotCompatible_PalletLocationAndUnits

		public void TestWarehouseRating_MeasureTypesAreNotCompatible_PalletLocationAndUnits()
		{
			var orgHeader = Helper.NewOrgHeader();
			var warehouse = Helper.NewWarehouse();
			var supplierPart1 = Helper.NewOrgSupplierPart(orgHeader);
			var supplierPart2 = Helper.NewOrgSupplierPart(orgHeader);

			var rate = Helper.NewClientRate(orgHeader);
			var entry = rate.AddRateEntry("WHS", "ALL", "", "", "", "");
			var line = entry.AddRateLine("ODOC", UnitCalculator.Code, Constants.PkgUnit.Unit);
			line.TL_IsOnPallets = false;
			line.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1.5m;
			var line1 = line;
			var line3 = entry.AddRateLine("ODOC", UnitCalculator.Code, RatingConstants.Units.PL);
			line3.TL_IsOnPallets = false;
			line3.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2.5m;
			var line2 = line3;
			line2.TL_OP_ProductNumber = supplierPart2.PK;

			Factory.Save();

			var testObject = new AutoRatingObject();
			testObject.ConsumerType = JobInvoicingConsumerTypes.WarehouseInwards;
			testObject.RateTypeToUse = RateType.Warehouse;
			testObject.Measures.SetQuantity(MeasureType.Weight, 0m, Core.Constants.Weight.Kilograms);
			testObject.Measures.SetQuantity(MeasureType.Volume, 0m, Core.Constants.Volume.CubicMetres);
			testObject.DebtorOrgs[RatingDebtorOrgTypes.LC] = rate.Header;

			// Note original measure had docket, but this doesn't happen in production
			testObject.Measures.CreateLocationPalletList(null);
			testObject.Measures.AddLocationPallet(5m, warehouse.PK, LocationMeasure.Empty, supplierPart1.PK, ProductAttributesMeasure.Empty, "");

			testObject.Measures.CreateWarehouseProductList(null, includeProductAttributes: false);
			testObject.Measures.AddWarehouseProduct((0, null), (0, null), 20m, warehouse.PK, supplierPart1.PK, "");

			var testAutoRater = new FreightAutoRater(new RatingContext());

			// Job have 1 not palletized container, so rate line for not palletized containers should be used (line1)
			var rateResults1 = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);

			var expected = new[]
				{
					new SimpleArInfo
						{
							Amount = 30m,
							InvoiceLineDesc = "Origin Documentation Fee - PROD1 (###1)",
							CalculationSingleLineDescription = "ODOC: 20 Unit(s) @ AUD 1.50/Unit"
						}
				};

			AssertRatingResults(expected, rateResults1);
		}

		#endregion

		#region AssertResult

		void AssertResult(AutoRateInfoCollection results, ZDecimal amount, params string[] descriptions)
		{
			foreach (AutoRateInfo info in results)
			{
				if (info.Amount == amount)
				{
					AssertEquals(amount, info.Amount);

					foreach (var description in descriptions)
					{
						var errorMessage = ZString.Format("Expected Description: {0}\r\nActual Description: {1}", description, info.SingleLineDescription);
						Assert(errorMessage, info.SingleLineDescription.Contains(description));
					}
					return;
				}
			}

			Assert("No results contained this sell amount", false);
		}

		#endregion

		#endregion

		#region Personal Effects / Tea Chests

		public void TestTeaChestPackageRating()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");

			entry.RateLines.RemoveAndDeleteAll();
			var line1 = entry.AddRateLine("FRT", UnitCalculator.Code, Core.Constants.Volume.TeaChest);
			line1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)10;

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 100m, 0m, rate.Header);
			testObject.ConsumerType = JobInvoicingConsumerTypes.Shipment;
			testObject.RateTypeToUse = RateType.Forwarding;
			testObject.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "FOB"));
			// This code is testing the special code in the engine that knows to use criteria.PackageInformation when the rate unit is TeaChest.
			// It doesn't seem to use the actual volume measures.
			var guid1 = ZGuid.NewZGuid();
			var guid2 = ZGuid.NewZGuid();
			testObject.Measures.SetQuantity(MeasureType.Volume, 20m + 10m, Core.Constants.Volume.CubicMetres);

			testObject.PackageInformation = new List<PackageInformation>();
			testObject.PackageInformation.Add(new PackageInformation(guid1.ToGuid(), "Book Box", 3, 2.1m, Core.Constants.Volume.CubicMetres, ""));
			testObject.PackageInformation.Add(new PackageInformation(guid2.ToGuid(), "Bike", 1, 4.3m, Core.Constants.Volume.CubicMetres, ""));

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals(1, results.Count);
			AssertEquals(501.76m, results[0].Amount);
			AssertEquals("International Freight", results[0].InvoiceLineDescription);
			AssertEquals("FRT: 50.176008 Tea Chest(s) - 3 Book Box (16.464003 Tea Chest volume) + 1 Bike (33.712005 Tea Chest volume) @ AUD 10.00/TE", results[0].SingleLineDescription);
		}

		public void TestTeaChestPackageRating_Rounding()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");

			entry.RateLines.RemoveAndDeleteAll();
			var line1 = entry.AddRateLine("FRT", UnitCalculator.Code, Core.Constants.Volume.TeaChest);
			line1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)10;
			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 0m, 0m, rate.Header);
			testObject.ConsumerType = JobInvoicingConsumerTypes.Shipment;
			testObject.RateTypeToUse = RateType.Forwarding;
			testObject.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "FOB"));
			// This code is testing the special code in the engine that knows to use criteria.PackageInformation when the rate unit is TeaChest.
			// It doesn't seem to use the actual volume measures.
			var guid1 = ZGuid.NewZGuid();
			var guid2 = ZGuid.NewZGuid();
			testObject.Measures.SetQuantity(MeasureType.Volume, 20m + 10m, Core.Constants.Volume.CubicMetres);

			testObject.PackageInformation = new List<PackageInformation>();
			testObject.PackageInformation.Add(new PackageInformation(guid1.ToGuid(), "Book Box", 3, 0.4m, Core.Constants.Volume.CubicMetres, ""));
			testObject.PackageInformation.Add(new PackageInformation(guid2.ToGuid(), "Bike", 1, 0.9m, Core.Constants.Volume.TeaChest, ""));

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals(1, results.Count);
			AssertEquals(101.92m, results[0].Amount);
			AssertEquals("International Freight", results[0].InvoiceLineDescription);
			AssertEquals("FRT: 10.192002 Tea Chest(s) - 3 Book Box (3.136001 Tea Chest volume) + 1 Bike (0.9 Tea Chest volume) @ AUD 10.00/TE", results[0].SingleLineDescription);

			line1.TL_Rounding = RatingRoundingTypes.UpTo1IfLessThanOne;
			Factory.Save();

			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals(1, results.Count);
			AssertEquals(109.76m, results[0].Amount);
			AssertEquals("International Freight", results[0].InvoiceLineDescription);
			AssertEquals("FRT: 10.976002 Tea Chest(s) - 3 Book Box (3.136001 Tea Chest volume) + 1 Bike (1 Tea Chest volume) @ AUD 10.00/TE", results[0].SingleLineDescription);
		}

		#endregion

		#region Transport

		public void TestTransportRating_PercentageWithExistingButNotUsed()
		{
			var tariff1 = Helper.NewCompanyTariff();
			tariff1.AddRateEntry(RatingConstants.RateCategory.TBC, "ALL", "AU", "").AddRateLine("ODOC", FlatCalculator.Code).Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)10m;
			tariff1.Factory.Save();

			var rate1 = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var entry1 = rate1.AddRateEntry(RatingConstants.RateCategory.TBC, "FTL", "AUSYD", "");

			var line1a = entry1.AddRateLine("OCART", CartageCalculator.Code, RatingConstants.Units.CN);
			line1a.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)45m;
			((CartageCalculator)line1a.Calculator).EquipmentType = Constants.FCLEquipmentNeeded.SideLoader;

			var line1b = entry1.AddRateLine("DCART", PercentageCalculator.Code, RatingConstants.Units.CN);
			line1b.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = Helper.ChargeCodes["OCART"].PK;
			line1b.GetCalculator<PercentageCalculator>().Percent = 5m;

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "", FreightMode.FTL, new TestContainers(Factory, "20GP", 2), 0m, 0m, rate1.Header);
			testObject.RateTypeToUse = RateType.TransportBookings;
			testObject.PickupCartageEquipment = Constants.FCLEquipmentNeeded.SideLoader;

			var existingCharge = (JobCharge)Factory.New(ObjectFactory.GetType(typeof(ICharge)));
			existingCharge.JR_AC = Helper.ChargeCodes["OCART"].PK;
			existingCharge.JR_LocalSellAmt = 500m;
			existingCharge.JR_SellRatingOverride = false;

			testObject.SetExistingCharges(new[] { existingCharge });
			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals(3, results.Count);
			AssertResult(results, 10m, "ODOC: Base Rate AUD 10.00");
			AssertResult(results, 90m, "OCART: 2 Container(s) @ AUD 45.00/Container");
			AssertResult(results, 4.5m, "DCART: 5.00% of (AUD 90.00 (OCART))");
		}

		public void TestTransportRating_PercentageWithExisting()
		{
			var tariff1 = Helper.NewCompanyTariff();
			tariff1.AddRateEntry(RatingConstants.RateCategory.TBC, "ALL", "AU", "").AddRateLine("ODOC", FlatCalculator.Code).Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)10m;
			tariff1.Factory.Save();

			var rate1 = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var entry1 = rate1.AddRateEntry(RatingConstants.RateCategory.TBC, "FTL", "AUSYD", "");

			var line1a = entry1.AddRateLine("DCART", PercentageCalculator.Code, RatingConstants.Units.CN);
			line1a.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = Helper.ChargeCodes["OCART"].PK;
			line1a.GetCalculator<PercentageCalculator>().Percent = 5m;

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "", FreightMode.FTL, new TestContainers(Factory, "20GP", 2), 0m, 0m, rate1.Header);
			testObject.RateTypeToUse = RateType.TransportBookings;
			testObject.PickupCartageEquipment = Constants.FCLEquipmentNeeded.SideLoader;

			var existingCharge = (JobCharge)Factory.New(ObjectFactory.GetType(typeof(ICharge)));
			existingCharge.JR_AC = Helper.ChargeCodes["OCART"].PK;
			existingCharge.JR_LocalSellAmt = 500m;

			testObject.SetExistingCharges(new[] { existingCharge });
			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			//Use Existing
			AssertResult(results, 10m, "ODOC: Base Rate AUD 10.00");
			AssertResult(results, 25m, "DCART: 5.00% of (AUD 500.00 (OCART*))");
			AssertEquals(2, results.Count);

			//Existing already used
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals(
				@"Here we roughly emulate the situation when rating happens several times for multiple objects and manually entered existing charge is present. It should be used only once by percentage calculator. Controversial feature but it is not my fix at the moment.
We still show autorated charge with zero sell/cost amounts just to have a hint on how it works.",
				2,
				results.Count);
			AssertResult(results, 10m, "ODOC: Base Rate AUD 10.00");
			AssertResult(results, 0m, "DCART: 5.00% of (AUD 0.00 (OCART))");

			//Clear Used - Use Existing again

			testAutoRater.RatingContext.PercentageLinesApplied.Clear();
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertResult(results, 10m, "ODOC: Base Rate AUD 10.00");
			AssertResult(results, 25m, "DCART: 5.00% of (AUD 500.00 (OCART*))");
			AssertEquals(2, results.Count);
		}

		public void TestTransportRating_SellCalcSingleLineDesc()
		{
			var chargeCode = Helper.ChargeCodes.New("_CTG", "Cartage", FlatCalculator.Code, ChargeCodeGroupList.Codes.Transport);
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line1 = rate.AddRateEntry(RatingConstants.RateCategory.TRN, "FRO", "AUSYD", "").AddRateLine(chargeCode.AC_Code, UnitCalculator.Code, RatingConstants.Units.CN);
			line1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)45m;

			Factory.Save();

			var testObject = new AutoRatingObject
			{
				ConsumerType = JobInvoicingConsumerTypes.LocalCartage,
				RateTypeToUse = RateType.LocalTransport,
				ChargeCodeGroups = new ChargeCodeGroupCollection { ChargeCodeGroupList.Codes.Transport },
				FreightMode = FreightMode.FRO,
				Origin = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD"),
				Destination = null,
			};

			testObject.DebtorOrgs[RatingDebtorOrgTypes.LC] = rate.Header;
			testObject.Measures.SetQuantity(MeasureType.Weight, 0m, Core.Constants.Weight.Kilograms);

			var testAutoRater = new FreightAutoRater(new RatingContext());

			const string sellCalcChargeCode = "_CTG: ";
			const string sellCalcDescription = "1 Container(s) @ AUD 45.00/Container";

			// Cartage Leg Only
			testObject.Measures.AddContainerWithNumberAndCartageLeg(ZGuid.Empty, "", ZGuid.NewZGuid(), new MeasureInfo.ContainerInfo());
			var autoRateResults = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Correct Number of Rating Results.", 1, autoRateResults.Count);
			AssertEquals("Expected unmodified Invoice Line Desc (No container details).", sellCalcChargeCode + sellCalcDescription, autoRateResults[0].SingleLineDescription);

			// Container Type - No Container Number
			testObject.Measures.RemoveContainerList();
			testObject.Measures.AddContainerWithNumberAndCartageLeg(GP20.PK, "", ZGuid.NewZGuid(), new MeasureInfo.ContainerInfo());
			autoRateResults = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Correct Number of Rating Results.", 1, autoRateResults.Count);
			AssertEquals("Expected unmodified Invoice Line Desc (No container num).", sellCalcChargeCode + sellCalcDescription, autoRateResults[0].SingleLineDescription);

			// Container Number - No Container Type
			testObject.Measures.RemoveContainerList();
			testObject.Measures.AddContainerWithNumberAndCartageLeg(ZGuid.Empty, new ZString("ANLU2346565"), ZGuid.NewZGuid(), new MeasureInfo.ContainerInfo());
			autoRateResults = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Correct Number of Rating Results.", 1, autoRateResults.Count);
			AssertEquals("Expected modified Invoice Line Desc.", sellCalcChargeCode + "ANLU2346565 - " + sellCalcDescription, autoRateResults[0].SingleLineDescription);

			// Container Number - Container Type
			testObject.Measures.RemoveContainerList();
			testObject.Measures.AddContainerWithNumberAndCartageLeg(GP20.PK, new ZString("ANLU2346565"), ZGuid.NewZGuid(), new MeasureInfo.ContainerInfo());
			autoRateResults = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Correct Number of Rating Results.", 1, autoRateResults.Count);
			AssertEquals("Expected modified Invoice Line Desc.", sellCalcChargeCode + "ANLU2346565 (20GP) - " + sellCalcDescription, autoRateResults[0].SingleLineDescription);
		}

		public void TestTransportRating()
		{
			var tariff = Helper.NewCompanyTariff();
			tariff.AddRateEntry("TBC", "ALL", "AU", "").AddRateLine("ODOC", FlatCalculator.Code).Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)10m;
			tariff.Factory.Save();

			var rate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var line2 = rate.AddRateEntry("TBC", "FTL", "AUSYD", "").AddRateLine("OCART", CartageCalculator.Code, RatingConstants.Units.CN);
			line2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)45m;
			((CartageCalculator)line2.Calculator).EquipmentType = Constants.FCLEquipmentNeeded.SideLoader;

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "", FreightMode.FTL, new TestContainers(Factory, "20GP", 2), 0m, 0m, rate.Header);
			testObject.RateTypeToUse = RateType.TransportBookings;
			testObject.PickupCartageEquipment = Constants.FCLEquipmentNeeded.SideLoader;

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals(2, results.Count);
			AssertResult(results, 10m, "ODOC: Base Rate AUD 10.00");
			AssertResult(results, 90m, "OCART: 2 Container(s) @ AUD 45.00/Container");
		}

		public void TestTransportRatingWithDropMode_Any()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var rateEntry = rate.AddRateEntry("TBC", "ALL", "AUSYD", "");

			var line = rateEntry.AddRateLine("OCART", CartageCalculator.Code, RatingConstants.Units.CN);
			line.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)45m;
			((CartageCalculator)line.Calculator).EquipmentType = Core.Constants.EquipmentNeeded.Any;

			var line2 = rateEntry.AddRateLine("DCART", CartageCalculator.Code, RatingConstants.Units.CN);
			line2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)50m;
			((CartageCalculator)line2.Calculator).EquipmentType = Core.Constants.FCLEquipmentNeeded.SideLoader;

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "", FreightMode.FCL, new TestContainers(Factory, "20GP", 2), 0m, 0m, rate.Header);
			testObject.RateTypeToUse = RateType.TransportBookings;
			testObject.PickupCartageEquipment = Constants.FCLEquipmentNeeded.Trailer;

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals(1, results.Count);
			AssertResult(results, 90m, "OCART: 2 Container(s) @ AUD 45.00/Container");

			testObject.PickupCartageEquipment = Constants.FCLEquipmentNeeded.SideLoader;

			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals(2, results.Count);
			AssertResult(results, 90m, "OCART: 2 Container(s) @ AUD 45.00/Container");
			AssertResult(results, 100m, "DCART: 2 Container(s) @ AUD 50.00/Container");
		}

		#endregion

		#region Freight AutoRating With Consignor/Consignee/ControllingCustomer

		public void TestFreightWithConsignorConsigneeControllingCustomer()
		{
			var consignor1 = Helper.NewOrgHeader();
			var consignor2 = Helper.NewOrgHeader();
			var consignee1 = Helper.NewOrgHeader();
			var consignee2 = Helper.NewOrgHeader();
			var controllingCustomer1 = Helper.NewOrgHeader();

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1 = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry1.RateLines[0].TL_RateCalculator = UnitCalculator.Code;
			entry1.RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;

			var entry2 = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry2.TI_OH_Consignor = consignor1.PK;
			entry2.RateLines[0].TL_RateCalculator = UnitCalculator.Code;
			entry2.RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)4m;

			var entry3 = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry3.TI_OH_Consignee = consignee1.PK;
			entry3.RateLines[0].TL_RateCalculator = UnitCalculator.Code;
			entry3.RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)3m;

			var entry4 = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry4.TI_OH_Consignor = consignor1.PK;
			entry4.TI_OH_Consignee = consignee1.PK;
			entry4.RateLines[0].TL_RateCalculator = UnitCalculator.Code;
			entry4.RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)2m;

			var entry5 = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry5.TI_OH_ControllingCustomer = controllingCustomer1.PK;
			entry5.TI_OH_Consignee = consignee2.PK;
			entry5.TI_OH_Consignor = consignor2.PK;
			entry5.RateLines[0].TL_RateCalculator = UnitCalculator.Code;
			entry5.RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1m;

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 100m, 0m, rate.Header);

			var testAutoRater = new FreightAutoRater(new RatingContext());

			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 1, results.Count);
			AssertEquals(500m, results[0].Amount);

			testObject.Consignor = consignor1;
			testObject.Consignee = consignee2;
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 1, results.Count);
			AssertEquals(400m, results[0].Amount);

			testObject.Consignor = consignor2;
			testObject.Consignee = consignee1;
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 1, results.Count);
			AssertEquals(300m, results[0].Amount);

			testObject.Consignor = consignor1;
			testObject.Consignee = consignee1;
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 1, results.Count);
			AssertEquals(200m, results[0].Amount);

			testObject.Consignor = consignor2;
			testObject.Consignee = consignee2;
			testObject.DebtorOrgs[RatingDebtorOrgTypes.CCUS] = controllingCustomer1;
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 1, results.Count);
			AssertEquals(100m, results[0].Amount);
		}

		#endregion

		#region Load Unaccepted Forwarding Quotes

		public void TestLoadUnacceptedForwardingQuotes()
		{
			var org = Helper.NewOrgHeader();
			var quote1 = Helper.NewQuote(org);
			var quote2 = Helper.NewQuote(org);
			quote1.AddRateEntry("LCL", "LCL", "AUSYD", "GBLON");
			quote2.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "GBLON", FreightMode.LCL, null, 2750M, 1.5M, org);

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var result = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue, true);

			AssertEquals("UnacceptedQuotes Count", 1, result.UnacceptedQuotes.Count);
			AssertNotNull(result.UnacceptedQuotes[0] as Quote);
		}

		#endregion

		#region Load Unaccepted Shipping Quotes

		public void TestLoadUnacceptedShippingQuotes()
		{
			var org = Helper.NewOrgHeader();
			var quote1 = Helper.NewQuote(org);
			var quote2 = Helper.NewQuote(org);
			quote1.AddRateEntry("SNC", "LCL", "AUSYD", "GBLON");
			quote2.AddRateEntry("SNC", "LCL", "AUSYD", "USLAX");

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "GBLON", FreightMode.LCL, null, 2750M, 1.5M, org);
			testObject.RateTypeToUse = RateType.Shipping;

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var result = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue, true);

			AssertEquals("UnacceptedQuotes Count", 1, result.UnacceptedQuotes.Count);
		}

		#endregion

		#region Load Costing Or Company Tariff Entires

		public void TestLoadCostingEntires()
		{
			GP40.RC_HandlingRateClass = "40GN";
			Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40HC").RC_HandlingRateClass = "40GN";

			var costing1 = Helper.NewCosting(Helper.NewOrgHeader());
			var fCLRateLine11 = costing1.AddRateEntry("FCL", "SEA", "AUSYD", "GBLON", "STD", "20GP").RateLines[0];
			fCLRateLine11.Parent.TI_RH_NKCommodityCode = "HAZ";
			var fCLRateLine21 = costing1.AddRateEntry("FCL", "SEA", "AUSYD", "GBLON", "STD", "40GP").RateLines[0];
			var fCLRateLine31 = costing1.AddRateEntry("FCL", "SEA", "AUSYD", "GBLON", "STD", "20GP").RateLines[0];
			var oRGRateLine11 = costing1.AddRateEntry("ORG", "SEA", "AUSYD", "", "STD", "").AddRateLine("ODOC", FlatCalculator.Code);
			var oRGRateLine21 = costing1.AddRateEntry("ORG", "FCL", "AUSYD", "", "STD", "40HC").AddRateLine("ODOC", FlatCalculator.Code);
			oRGRateLine21.Parent.TI_MatchContainerRateClass = true;

			var costing2 = Helper.NewCosting(Helper.NewOrgHeader());
			var fCLRateLine41 = costing2.AddRateEntry("FCL", "SEA", "AUSYD", "GBLON", "STD", "20GP").RateLines[0];
			var fCLRateLine51 = costing2.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "STD", "20GP").RateLines[0];

			var asserter = new RateLineGettersAsserter(Factory);
			asserter.Criteria = new TestRatingCriteria("AUSYD", "GBLON", 1, GP20, null);
			asserter.Criteria.Creditors = Creditors.New(GetNewTestOrgWithSourceFromHeader(costing1.Header));

			asserter.AssertGetCostRateLines(new[] { fCLRateLine31 }, fCLRateLine11);
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), fCLRateLine21);
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), fCLRateLine31);
			asserter.AssertGetCostRateLines(new[] { fCLRateLine31 }, fCLRateLine41);
			asserter.AssertGetCostRateLines(new[] { fCLRateLine31 }, fCLRateLine51);
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), oRGRateLine11);
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), oRGRateLine21);

			asserter.Criteria = new TestRatingCriteria("AUSYD", "GBLON", 1, GP20, null);
			asserter.Criteria.Creditors = Creditors.New(GetNewTestOrgWithSourceFromHeader(costing2.Header));

			asserter.AssertGetCostRateLines(new[] { fCLRateLine41 }, fCLRateLine11);
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), fCLRateLine21);
			asserter.AssertGetCostRateLines(new[] { fCLRateLine41 }, fCLRateLine31);
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), fCLRateLine41);
			asserter.AssertGetCostRateLines(new[] { fCLRateLine41 }, fCLRateLine51);

			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), oRGRateLine11);
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), oRGRateLine21);

			asserter.Criteria = new TestRatingCriteria("AUSYD", "GBLON", 1, GP20, null);
			asserter.Criteria.Creditors = Creditors.New();

			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), fCLRateLine11);
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), fCLRateLine21);
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), fCLRateLine31);
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), fCLRateLine41);
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), fCLRateLine51);
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), oRGRateLine11);
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), oRGRateLine21);

			asserter.Criteria = new TestRatingCriteria("AUSYD", "GBLON", 1, GP40, NewClient2);
			asserter.Criteria.Creditors = Creditors.New(GetNewTestOrgWithSourceFromHeader(costing1.Header));

			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), fCLRateLine11);
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), fCLRateLine21);
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), fCLRateLine31);
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), fCLRateLine41);
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), fCLRateLine51);
			asserter.AssertGetCostRateLines(new[] { oRGRateLine21 }, oRGRateLine11);
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), oRGRateLine21);

			asserter.Criteria = new TestRatingCriteria("AUSYD", "GBLON", 1, GP40, NewClient2);
			asserter.Criteria.Creditors = Creditors.New(GetNewTestOrgWithSourceFromHeader(costing2.Header));

			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), fCLRateLine11);
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), fCLRateLine21);
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), fCLRateLine31);
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), fCLRateLine41);
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), fCLRateLine51);
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), oRGRateLine11);
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), oRGRateLine21);

			asserter.Criteria = new TestRatingCriteria("AUSYD", "GBLON", 1, GP40, NewClient2);
			asserter.Criteria.Creditors = Creditors.New();

			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), fCLRateLine11);
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), fCLRateLine21);
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), fCLRateLine31);
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), fCLRateLine41);
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), fCLRateLine51);
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), oRGRateLine11);
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), oRGRateLine21);
		}

		public void TestLoadCompanyTariffEntires()
		{
			var tariff = Factory.New<CompanyTariff>();
			var fCLRateLine11 = tariff.AddRateEntry("FCL", "SEA", "AUSYD", "GBLON", "STD", "20GP").RateLines[0];
			fCLRateLine11.Parent.TI_RH_NKCommodityCode = "HAZ";
			var fCLRateLine21 = tariff.AddRateEntry("FCL", "SEA", "AUSYD", "GBLON", "STD", "40GP").RateLines[0];
			var fCLRateLine31 = tariff.AddRateEntry("FCL", "SEA", "AUSYD", "GBLON", "STD", "20GP").RateLines[0];
			var oRGRateLine11 = tariff.AddRateEntry("ORG", "SEA", "AUSYD", "", "STD", "").AddRateLine("ODOC", FlatCalculator.Code);
			var oRGRateLine21 = tariff.AddRateEntry("ORG", "FCL", "AUSYD", "", "STD", "40GP").AddRateLine("ODOC", FlatCalculator.Code);

			var tariff2 = Factory.New<CompanyTariff>();
			var orgCollection = tariff2.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG);
			orgCollection.Load();
			var tariff2Entry1 = orgCollection[0];
			tariff2Entry1.RateLines.OverrideTariffLines(new[] { oRGRateLine11, oRGRateLine21 });

			var org = Helper.NewOrgHeader(1);
			var clientRate = Helper.NewClientRate(org);
			var crFCLRateLine11 = clientRate.AddRateEntry("FCL", "SEA", "AUSYD", "GBLON", "STD", "20GP").RateLines[0];
			crFCLRateLine11.Parent.TI_RH_NKCommodityCode = "HAZ";
			var crFCLRateLine21 = clientRate.AddRateEntry("FCL", "SEA", "AUSYD", "GBLON", "STD", "40GP").RateLines[0];
			var crFCLRateLine31 = clientRate.AddRateEntry("FCL", "SEA", "AUSYD", "GBLON", "STD", "20GP").RateLines[0];
			var crORGRateLine11 = clientRate.AddRateEntry("ORG", "SEA", "AUSYD", "", "STD", "").AddRateLine("ODOC", FlatCalculator.Code);
			var crORGRateLine21 = clientRate.AddRateEntry("ORG", "FCL", "AUSYD", "", "STD", "40GP").AddRateLine("ODOC", FlatCalculator.Code);

			Factory.Save();

			var asserter = new RateLineGettersAsserter(Factory);
			asserter.Criteria = new TestRatingCriteria("AUSYD", "GBLON", 1, GP20, null);
			asserter.Criteria.JobDirection = Directions.Export;
			asserter.Criteria.ClearCache();

			asserter.AssertGetCompanyTariffRateLines(Array.Empty<RateLine>(), fCLRateLine11);
			asserter.AssertGetCompanyTariffRateLines(new[] { fCLRateLine31 }, crFCLRateLine11);
			asserter.AssertGetCompanyTariffRateLines(Array.Empty<RateLine>(), crFCLRateLine21);
			asserter.AssertGetCompanyTariffRateLines(new[] { fCLRateLine31 }, crFCLRateLine31);
			asserter.AssertGetCompanyTariffRateLines(new[] { oRGRateLine11 }, crORGRateLine11);
			asserter.AssertGetCompanyTariffRateLines(Array.Empty<RateLine>(), crORGRateLine21);

			asserter.Criteria = new TestRatingCriteria("AUSYD", "GBLON", 1, GP40, null);
			asserter.Criteria.JobDirection = Directions.Export;
			asserter.Criteria.ClearCache();

			asserter.AssertGetCompanyTariffRateLines(Array.Empty<RateLine>(), crFCLRateLine11);
			asserter.AssertGetCompanyTariffRateLines(new[] { fCLRateLine21 }, crFCLRateLine21);
			asserter.AssertGetCompanyTariffRateLines(Array.Empty<RateLine>(), crFCLRateLine31);
			asserter.AssertGetCompanyTariffRateLines(new[] { oRGRateLine21 }, crORGRateLine11);
			asserter.AssertGetCompanyTariffRateLines(new[] { oRGRateLine21 }, crORGRateLine21);
			asserter.AssertGetCompanyTariffRateLines(new[] { oRGRateLine21 }, crORGRateLine11);

			org.CompanyData.RateTariffLevels.SetLevel("ORG", nameof(OrgRateTariffLevel.Directions.EXP), "SEA", 2);
			asserter.Criteria.ClearCache();
			asserter.AssertGetCompanyTariffRateLines(new[] { oRGRateLine21 }, crORGRateLine11);

			org.CompanyData.RateTariffLevels.SetLevel("ORG", nameof(OrgRateTariffLevel.Directions.IMP), "SEA", 3);
			asserter.Criteria.ClearCache();
			asserter.AssertGetCostRateLines(Array.Empty<RateLine>(), crORGRateLine11);

			org.CompanyData.RateTariffLevels.SetLevel("ORG", nameof(OrgRateTariffLevel.Directions.EXP), "SEA", 3);
			asserter.Criteria.ClearCache();
			asserter.AssertGetCompanyTariffRateLines(Array.Empty<RateLine>(), crORGRateLine11);
		}

		class RateLineGettersAsserter
		{
			public RateLineGettersAsserter(BusinessObjectFactory factory)
			{
				this.factory = factory;
				AutoRater = new FreightAutoRater(new MockRatingContext(factory));
			}

			FreightAutoRater AutoRater { get; }
			public TestRatingCriteria Criteria { get; set; }
			readonly BusinessObjectFactory factory;

			public void AssertGetCompanyTariffRateLines(IEnumerable<RateLine> expected, RateLine lineToPass, string message = "")
			{
				var parameters = new AutoRatingCalculatorParametersWithoutFilter(Criteria, AutoRater);
				lineToPass.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;

				var calc = (CompanyTariffOrCostBasedCalculator)lineToPass.Calculator;
				var testResult = calc.GetRelatedLines(parameters);

				AssertContainsExactElementsInAnyOrder(message, DisplayTextProvider, expected.Select(x => RateLine.GetBO(x).PK), testResult.Select(x => RateLine.GetBO(x).PK));
			}

			public void AssertGetCostRateLines(IEnumerable<RateLine> expected, RateLine lineToPass, string message = "")
			{
				var parameters = new AutoRatingCalculatorParametersWithoutFilter(Criteria, AutoRater);
				lineToPass.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;

				var calc = (CompanyTariffOrCostBasedCalculator)lineToPass.Calculator;
				var testResult = calc.GetRelatedLines(parameters);

				AssertContainsExactElementsInAnyOrder(message, DisplayTextProvider, expected.Select(x => RateLine.GetBO(x).PK), testResult.Select(x => RateLine.GetBO(x).PK));
			}

			string DisplayTextProvider(ZGuid linePK)
			{
				var line = factory.Load<RateLine>(linePK);
				var entry = line != null ? line.Parent : null;

				return
					GetText(() => entry.TI_RateCategory) + " " +
					GetText(() => entry.TI_Mode) + " " +
					GetText(() => entry.TI_OriginLRC) + " " +
					GetText(() => entry.TI_DestinationLRC) + " " +
					GetText(() => entry.ServiceLevel_NI.RS_Code) + " " +
					GetText(() => entry.CommodityCode.RH_Code) + " " +
					GetText(() => entry.Container.RC_Code) + " " +
					GetText(() => line.ChargeCode.AC_Code);
			}

			string GetText(Func<string> function)
			{
				try
				{
					return function();
				}
				catch (NullReferenceException)
				{
					return "null";
				}
			}
		}

		class MockRatingContext : IRatingContext
		{
			public MockRatingContext(BusinessObjectFactory factory)
			{
				Factory = factory;
				Logger = new TestLogger();
				var dummyLogger = new DummyLogger();
				ProviderCW1Rates = new CW1RatesProvider(Factory, dummyLogger);
				RatesProvider = new AggregatedRatesProvider(new[] { ProviderCW1Rates });
				RateCalculationLogWrapper = new CalculationLogsWrapper();
			}

			public IEnumerable<IRatesProvider> RatesProviders { get; }

			public ILogger Logger { get; }

			public IDialogService DialogService { get; }

			public BusinessObjectFactory Factory { get; }

			public bool SearchForRatesMode { get; set; }

			public bool IsInRebateCalculationMode { get; set; }

			public Dictionary<ZGuid, HashSet<ZGuid>> PercentageLinesApplied { get; } = new Dictionary<ZGuid, HashSet<ZGuid>>();

			public string RawResponse { get; set; }

			public bool IsManualCostSelectMode => false;

			public IRatesProvider RatesProvider { get; }

			public IUrsRatesProvider ProviderUrsRates => null;

			public ICW1RatesProvider ProviderCW1Rates { get; }

			public List<AutoRateInfo> InterimAutoRatingResults { get; } = new List<AutoRateInfo>();

			public CalculationLogsWrapper RateCalculationLogWrapper { get; }
		}

		#endregion

		#region Calculate Costs

		public void TestClientAndCarrierServiceLevel_WithNoResult()
		{
			var client = Helper.NewOrgHeader();
			var rate = Helper.NewClientRate(client);

			var entry = rate.AddRateEntryWithFlatRateLine("AIR", "LSE", "AUSYD", "USLAX", "FRT", 2000m);
			entry.TI_RS_NKServiceLevel_NI = "CLI";
			entry.TI_PL_NKCarrierServiceLevel = "XXX";
			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 55M, .5M, rate.Header);
			testObject.ServiceLevel = new ServiceLevelRatingInformation(new ServiceLevelInfo("CLI", ServiceLevelType.Client), new ServiceLevelInfo("CAR", ServiceLevelType.Carrier));

			var testLogger = new TestLogger();
			var testAutoRater = new FreightAutoRater(new RatingContext(testLogger));
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("CLI & XXX available. Looking for CLI & CAR. No results expected", 0, results.Count);
		}

		public void TestClientAndCarrierServiceLevel_WithResult()
		{
			var client = Helper.NewOrgHeader();
			var rate = Helper.NewClientRate(client);

			var entry = rate.AddRateEntryWithFlatRateLine("AIR", "LSE", "AUSYD", "USLAX", "FRT", 2000m);
			entry.TI_RS_NKServiceLevel_NI = "CLI";
			entry.TI_PL_NKCarrierServiceLevel = "CAR";
			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 55M, .5M, rate.Header);
			testObject.ServiceLevel = new ServiceLevelRatingInformation(new ServiceLevelInfo("CLI", ServiceLevelType.Client), new ServiceLevelInfo("CAR", ServiceLevelType.Carrier));

			var testLogger = new TestLogger();
			var testAutoRater = new FreightAutoRater(new RatingContext(testLogger));

			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("CLI & CAR available. Looking for CLI & CAR. Results expected", 1, results.Count);
			var info = results.First(c => c.ChargeCode.AC_Code == "FRT");
			AssertEquals("Sell Rate", 2000m, info.Amount);
		}

		#endregion

		#region Arrival and Departure Date Tests

		[TestDate(2014, 1, 1)]
		public void TestAutoRateUsesArrivalDateForDestinationCharges()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var airRateEntry1 = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			airRateEntry1.RateLines.RemoveAndDeleteAll();
			airRateEntry1.TI_RateStartDate = ZDate.Today.AddDays(-1);
			airRateEntry1.TI_RateEndDate = ZDate.Today.AddMonths(1);
			airRateEntry1.AddRateLine(TestFRT.AC_Code, FlatCalculator.Code);
			var airRateEntry2 = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "US");
			airRateEntry2.RateLines.RemoveAndDeleteAll();
			airRateEntry2.TI_RateStartDate = ZDate.Today.AddDays(1);
			airRateEntry2.TI_RateEndDate = ZDate.Today.AddMonths(1);
			airRateEntry2.AddRateLine(TestBAF.AC_Code, FlatCalculator.Code);

			var orgRateEntry1 = testRate.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			orgRateEntry1.TI_RateStartDate = ZDate.Today.AddDays(-1);
			orgRateEntry1.TI_RateEndDate = ZDate.Today.AddMonths(1);
			orgRateEntry1.AddRateLine(TestADF.AC_Code, FlatCalculator.Code);
			var orgRateEntry2 = testRate.AddRateEntry("ORG", "AIR", "AUSYD", "US");
			orgRateEntry2.TI_RateStartDate = ZDate.Today.AddDays(1);
			orgRateEntry2.TI_RateEndDate = ZDate.Today.AddMonths(1);
			orgRateEntry2.AddRateLine(TestANY.AC_Code, FlatCalculator.Code);

			var dstRateEntry1 = testRate.AddRateEntry("DST", "AIR", "AUSYD", "USLAX");
			dstRateEntry1.TI_RateStartDate = ZDate.Today.AddDays(-1);
			dstRateEntry1.TI_RateEndDate = ZDate.Today.AddMonths(1);
			dstRateEntry1.AddRateLine(TestAWB.AC_Code, PercentageCalculator.Code);
			var dstRateEntry2 = testRate.AddRateEntry("DST", "AIR", "AUSYD", "US");
			dstRateEntry2.TI_RateStartDate = ZDate.Today.AddDays(1);
			dstRateEntry2.TI_RateEndDate = ZDate.Today.AddMonths(1);
			dstRateEntry2.AddRateLine(TestCAF.AC_Code, FlatCalculator.Code);
			var dstRateEntry3 = testRate.AddRateEntry("DST", "AIR", "AUSYD", "USCA");
			dstRateEntry3.TI_RateStartDate = ZDate.Today.AddDays(6);
			dstRateEntry3.TI_RateEndDate = ZDate.Today.AddMonths(1);
			dstRateEntry3.AddRateLine(TestBBK.AC_Code, FlatCalculator.Code);

			Factory.Save();

			var testAutoRater = new FreightAutoRater(new RatingContext());

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 55M, .5M, testRate.Header);
			// depart today, arrive today + 5 days
			var jobDatesProvider = new Mock<IJobDatesProvider>();
			testObject.JobDatesProvider = jobDatesProvider.Object;
			jobDatesProvider.Setup(m => m.EarliestPossibleDate).Returns(ZDate.Today);
			jobDatesProvider.Setup(m => m.LatestPossibleDate).Returns(ZDate.Today.AddDays(5));
			jobDatesProvider.Setup(m => m.GetJobDateByType(JobDateTypes.Codes.DepartureDate, It.IsAny<string>())).Returns(ZDate.Today);
			jobDatesProvider.Setup(m => m.GetJobDateByType(JobDateTypes.Codes.ArrivalDate, It.IsAny<string>())).Returns(ZDate.Today.AddDays(5));

			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertNotNull("Active Air Rate should be included", results.First(r => r.ChargeCode.PK == TestFRT.PK));
			AssertNull("Air Rate that is active from tomorrow should not be included", results.FirstOrDefault(r => r.ChargeCode.PK == TestBAF.PK));

			AssertNotNull("Active Origin Rate should be included", results.First(r => r.ChargeCode.PK == TestADF.PK));
			AssertNull("Origin Rate that is active from tomorrow should not be included", results.FirstOrDefault(r => r.ChargeCode.PK == TestANY.PK));

			AssertNotNull("Active Destination Rate should be included", results.First(r => r.ChargeCode.PK == TestAWB.PK));
			AssertNull("Destination Rate that is active from tomorrow should not be included", results.FirstOrDefault(r => r.ChargeCode.PK == TestCAF.PK));
			AssertNull("Destination Rate that is active from tomorrow should not be included", results.FirstOrDefault(r => r.ChargeCode.PK == TestBBK.PK));
		}

		public void TestAutoRateWhenChargeCodeIsNull()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var aIRRateEntry1 = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			aIRRateEntry1.RateLines.RemoveAndDeleteAll();
			aIRRateEntry1.TI_RateStartDate = ZDate.Today.AddDays(-1);
			aIRRateEntry1.TI_RateEndDate = ZDate.Today.AddMonths(1);
			aIRRateEntry1.AddRateLine(TestFRT.AC_Code, FlatCalculator.Code);
			Factory.Save();

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var rateLines = testAutoRater.Factory.Load<RateEntry>(aIRRateEntry1.PK).RateLines;

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 55M, .5M, testRate.Header);
			// depart today, arrive today + 5 days
			var jobDatesProvider = new Mock<IJobDatesProvider>();
			testObject.JobDatesProvider = jobDatesProvider.Object;
			jobDatesProvider.Setup(m => m.EarliestPossibleDate).Returns(ZDate.Today);
			jobDatesProvider.Setup(m => m.LatestPossibleDate).Returns(ZDate.Today.AddDays(5));
			jobDatesProvider.Setup(m => m.GetJobDateByType(JobDateTypes.Codes.DepartureDate, It.IsAny<string>())).Returns(ZDate.Today);
			jobDatesProvider.Setup(m => m.GetJobDateByType(JobDateTypes.Codes.ArrivalDate, It.IsAny<string>())).Returns(ZDate.Today.AddDays(5));

			var sqlText = "DELETE dbo.RateLines WHERE TL_AC IN (SELECT AC_PK FROM dbo.AccChargeCode WHERE AC_Code = '" + TestFRT.AC_Code + "')";
			((CargoWise.Data.IDbConnected)Factory).Connection.ExecuteNonQuery(sqlText);
			sqlText = "DELETE dbo.AccChargeCode WHERE AC_Code = '" + TestFRT.AC_Code + "'";
			((CargoWise.Data.IDbConnected)Factory).Connection.ExecuteNonQuery(sqlText);

			AssertNoExceptionThrown(() => testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue));
		}

		#endregion

		#region CreateLocationFilter

		public void TestCreateLocationFilter_CriteriaHasDatesRange_SelectRatesWithinSpecifiedRange()
		{
			var criteriaStartDate = ZDate.Today.AddDays(AutoRater.ExpiredRateNotificationDays > 0 ? -AutoRater.ExpiredRateNotificationDays : -1);
			var criteriaEndDate = ZDate.Today.AddDays(AutoRater.ExpiredRateNotificationDays > 0 ? +AutoRater.ExpiredRateNotificationDays : 1);
			var criteriaMiddleTime = new ZDate(criteriaStartDate.AddDays((criteriaEndDate - criteriaStartDate).Days / 2));

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());

			var rateEntry1 = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			rateEntry1.TI_RateStartDate = criteriaStartDate.AddDays(-1);
			rateEntry1.TI_RateEndDate = criteriaMiddleTime;

			var rateEntry2 = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			rateEntry2.TI_RateStartDate = criteriaMiddleTime.AddDays(1);
			rateEntry2.TI_RateEndDate = criteriaMiddleTime.AddDays(1);

			var rateEntry3 = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			rateEntry3.TI_RateStartDate = criteriaMiddleTime.AddDays(2);
			rateEntry3.TI_RateEndDate = criteriaMiddleTime.AddDays(3);

			var rateEntry4 = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			rateEntry4.TI_RateStartDate = criteriaMiddleTime.AddDays(4);
			rateEntry4.TI_RateEndDate = ZDate.Empty;

			var rateEntry6 = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			rateEntry6.TI_RateStartDate = criteriaStartDate.AddDays(-3);
			rateEntry6.TI_RateEndDate = criteriaStartDate.AddDays(-2);

			Factory.Save();

			var autoRatingObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 55M, .5M, rate.Header);

			var jobDatesProvider = new Mock<IJobDatesProvider>();
			autoRatingObject.JobDatesProvider = jobDatesProvider.Object;
			jobDatesProvider.Setup(m => m.EarliestPossibleDate).Returns(criteriaStartDate);
			jobDatesProvider.Setup(m => m.LatestPossibleDate).Returns(criteriaEndDate);

			var autoRating = new AutoRatingProxy(autoRatingObject);
			var criteria = new RatingCriteria(autoRating, Factory);

			var query  = RateEntryQueries.GetQuery(criteria, isCosting: false, isInterCompanyTariff: false);
			var entries = Factory.Load<RateEntry>(query);

			var expectedEntries = new List<ZGuid> { rateEntry1.PK, rateEntry2.PK, rateEntry3.PK, rateEntry4.PK };
			var actualEntries = entries.Select(e => e.PK);
			AssertContainsExactElementsInAnyOrder(expectedEntries, actualEntries);
		}

		#endregion

		#region CreateStrictOriginDestinationFilter

		public void TestCreateStrictOriginDestinationFilter_CriteriaHasDatesRangeAndCarrier_SelectRatesWithinSpecifiedRange()
		{
			var criteriaStartDate = ZDate.Today.AddDays(AutoRater.ExpiredRateNotificationDays > 0 ? -AutoRater.ExpiredRateNotificationDays : -1);
			var criteriaEndDate = ZDate.Today.AddDays(AutoRater.ExpiredRateNotificationDays > 0 ? +AutoRater.ExpiredRateNotificationDays : 1);
			var criteriaMiddleTime = new ZDate(criteriaStartDate.AddDays((criteriaEndDate - criteriaStartDate).Days / 2));

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var carrier1 = Helper.NewOrgHeader();
			var carrier2 = Helper.NewOrgHeader();
			Factory.Save();

			var rateEntry1 = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			rateEntry1.TI_RateStartDate = criteriaStartDate.AddDays(-1);
			rateEntry1.TI_RateEndDate = criteriaMiddleTime;

			var rateEntry2 = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			rateEntry2.TI_RateStartDate = criteriaMiddleTime.AddDays(1);
			rateEntry2.TI_RateEndDate = criteriaMiddleTime.AddDays(1);

			var rateEntry3 = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			rateEntry3.TI_RateStartDate = criteriaMiddleTime.AddDays(2);
			rateEntry3.TI_RateEndDate = criteriaMiddleTime.AddDays(3);

			var rateEntry4 = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			rateEntry4.TI_RateStartDate = criteriaMiddleTime.AddDays(4);
			rateEntry4.TI_RateEndDate = ZDate.Empty;

			var rateEntry6 = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			rateEntry6.TI_RateStartDate = criteriaStartDate.AddDays(-3);
			rateEntry6.TI_RateEndDate = criteriaStartDate.AddDays(-2);

			var rateEntry8 = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			rateEntry8.TI_RateStartDate = criteriaMiddleTime.AddDays(4);
			rateEntry8.TI_RateEndDate = ZDate.Empty;
			rateEntry8.TI_OH_TransportProvider = carrier2.PK;

			var rateEntry10 = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			rateEntry10.TI_RateStartDate = criteriaMiddleTime.AddDays(4);
			rateEntry10.TI_RateEndDate = ZDate.Empty;
			rateEntry10.TI_OH_TransportProvider = carrier1.PK;

			Factory.Save();

			var autoRatingObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 55M, .5M, rate.Header);
			autoRatingObject.Carrier = carrier1;

			var jobDatesProvider = new Mock<IJobDatesProvider>();
			autoRatingObject.JobDatesProvider = jobDatesProvider.Object;
			jobDatesProvider.Setup(m => m.EarliestPossibleDate).Returns(criteriaStartDate);
			jobDatesProvider.Setup(m => m.LatestPossibleDate).Returns(criteriaEndDate);

			var autoRating = new AutoRatingProxy(autoRatingObject);
			var criteria = new RatingCriteria(autoRating, Factory);

			var query = RateEntryQueries.GetQuery(criteria, isCosting: false, isInterCompanyTariff: false);
			var entries = Factory.Load<RateEntry>(query);

			var expectedEntries = new List<ZGuid> { rateEntry1.PK, rateEntry2.PK, rateEntry3.PK, rateEntry4.PK, rateEntry10.PK };
			var actualEntries = entries.Select(e => e.PK);
			AssertContainsExactElementsInAnyOrder(expectedEntries, actualEntries);
		}

		#endregion

		#region Organisation Types

		public void TestFindBestMatchWhenRateExistsForNonApplicableOrganisation()
		{
			var testRateConsignee = Helper.NewClientRate(Consignee);
			testRateConsignee.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			Factory.Save();

			var rater = new FreightAutoRater(new RatingContext());
			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 2750M, 1.5M, Consignor);
			testObject.Consignee = Consignee;
			testObject.Consignor = Consignor;

			testObject.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "DDP"));
			var results = rater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("No match found - Consignee rates are NOT relevant for EXPORT, prepaid charge (DDP)", 0, results.Count);

			testObject.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "EXW"));
			results = rater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("One match found - Consignee rates ARE relevant for EXPORT, collect charge (EXW)", 1, results.Count);
		}

		#endregion

		#region INCOTERM Applicable Charges

		public void TestChargesNotApplicableToIncoTermRemovedFromClientRate()
		{
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());

			testRate.AddRateEntry("AIR", "LSE", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "INBOM");

			var destEntry = testRate.AddRateEntry("DST", "AIR", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "INBOM");
			destEntry.AddRateLine("DDOC", FlatCalculator.Code);
			Factory.Save();

			var testObject = new AutoRatingObject(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "INBOM", FreightMode.LSE, null, 55M, .5M, testRate.Header);

			var testAutoRater = new FreightAutoRater(new RatingContext());

			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 2, results.Count);

			var info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["FRT"].PK);
			AssertNotNull("Freight charge should be included as INCOTERM is ExWorks", info);

			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["DDOC"].PK);
			AssertNotNull("Destination charge should be included as INCOTERM is ExWorks", info);

			testObject.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "CIF"));

			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 1, results.Count);

			info = results.FirstOrDefault(r => r.ChargeCode.PK == Helper.ChargeCodes["FRT"].PK);
			AssertNull("Freight charge should NOT be included as INCOTERM is CIF", info);

			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["DDOC"].PK);
			AssertNotNull("Destination charge should be included as INCOTERM is CIF", info);

			IncoTermRegistry.Instance.ChargeAgentAlwaysCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Helper.ChargeCodes["FRT"].PK.ToString());
			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 2, results.Count);

			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["FRT"].PK);
			AssertNotNull("Freight charge should be included as INCOTERM is CIF but FRT is always charged to agent", info);

			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["DDOC"].PK);
			AssertNotNull("Destination charge should be included as INCOTERM is CIF", info);
		}

		public void TestClientChargesNotApplicableToAgentRate()
		{
			var collection = new RatesPrioritiesCollection();
			collection.AddNew().OrganizationType = nameof(RatingDebtorOrgTypes.LC);
			collection.AddNew().OrganizationType = nameof(RatingDebtorOrgTypes.CNE);

			RatingDataRegistry.Instance.ImportCollectPriorities.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());

			testRate.AddRateEntry("AIR", "LSE", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "INBOM");

			var destEntry = testRate.AddRateEntry("DST", "AIR", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "INBOM");
			destEntry.AddRateLine("DDOC", FlatCalculator.Code);
			Factory.Save();

			var testObject = new AutoRatingObject(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "INBOM", FreightMode.LSE, null, 55M, .5M, Helper.NewOrgHeader());
			testObject.DebtorOrgs[RatingDebtorOrgTypes.AG] = testRate.Header;

			var testAutoRater = new FreightAutoRater(new RatingContext());

			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 0, results.Count);

			IncoTermRegistry.Instance.ChargeLocalClientAlwaysCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Helper.ChargeCodes["FRT"].PK.ToString());

			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 1, results.Count);

			var info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["FRT"].PK);
			AssertNotNull("Freight charge should be included as it bypasses 'Client charge from agent rate' rule", info);

			info = results.FirstOrDefault(r => r.ChargeCode.PK == Helper.ChargeCodes["DDOC"].PK);
			AssertNull("Destination charge should not be included", info);
		}

		public void TestChargesNotApplicableToIncoTermRemovedFromCompanyTariff()
		{
			var tariff = Helper.NewCompanyTariff();
			tariff.AddRateEntry("AIR", "LSE", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "INBOM");
			var destEntry = tariff.AddRateEntry("DST", "AIR", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "INBOM");
			destEntry.AddRateLine("DDOC", FlatCalculator.Code);
			tariff.Factory.Save();

			var client = Helper.NewOrgHeader(1);

			Factory.Save();

			var testObject = new AutoRatingObject(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "INBOM", FreightMode.LSE, null, 55M, .5M, client);

			var testAutoRater = new FreightAutoRater(new RatingContext());

			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 2, results.Count);

			var info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["FRT"].PK);
			AssertNotNull("Freight charge should be included as INCOTERM is ExWorks", info);

			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["DDOC"].PK);
			AssertNotNull("Destination charge should be included as INCOTERM is ExWorks", info);

			testObject.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "CIF"));

			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 1, results.Count);

			info = results.FirstOrDefault(r => r.ChargeCode.PK == Helper.ChargeCodes["FRT"].PK);
			AssertNull("Freight charge should NOT be included as INCOTERM is CIF", info);

			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["DDOC"].PK);
			AssertNotNull("Destination charge should be included as INCOTERM is CIF", info);

			IncoTermRegistry.Instance.ChargeAgentAlwaysCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Helper.ChargeCodes["FRT"].PK.ToString());
			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 1, results.Count);

			info = results.FirstOrDefault(r => r.ChargeCode.PK == Helper.ChargeCodes["FRT"].PK);
			AssertNull("Freight charge should not be included because there is no Agent and registry says Agent should be always charged for FRT", info);

			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["DDOC"].PK);
			AssertNotNull("Destination charge should be included as INCOTERM is CIF", info);
		}

		#endregion

		#region With Dodgy Via

		public void TestDodgyVia()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var entry1 = rate.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");
			entry1.TI_ViaLRC = "AUBNE";
			entry1.RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)100m;

			var entry2 = rate.AddRateEntry("LCL", "LCL", "USLAX", "AUSYD");
			entry2.TI_ViaLRC = "AUBNE";
			entry2.RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)80m;

			var tariff = Helper.NewCompanyTariff();
			tariff.AddRateEntry("LCL", "LCL", "AUEC", "USLAX").RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)110m;
			tariff.AddRateEntry("LCL", "LCL", "USLAX", "AUEC").RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)90m;

			Factory.Save();

			var autoRater = new FreightAutoRater(new RatingContext());
			var viaLocation = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");

			var autoRatingObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LCL, null, 500m, 1m, rate.Header);
			autoRatingObject.SetVia(viaLocation);

			var results = autoRater.AutoRate(new AutoRatingProxy(autoRatingObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 1, results.Count);
			AssertEquals(100m, results[0].Amount);

			autoRatingObject = new AutoRatingObject("USLAX", "AUSYD", FreightMode.LCL, null, 500m, 1m, rate.Header);
			autoRatingObject.SetVia(viaLocation);

			results = autoRater.AutoRate(new AutoRatingProxy(autoRatingObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 1, results.Count);
			AssertEquals(80m, results[0].Amount);
		}

		#endregion

		#region Onforwarding Charges

		public void TestOnforwardingCharges()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			rate.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX").RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)90m;
			var entry2 = rate.AddRateEntry("LCL");
			entry2.TI_ViaLRC = "SGSIN";
			entry2.RateLines.RemoveAndDeleteAll();
			entry2.AddRateLine("BAF", FlatCalculator.Code);
			entry2.RateLines[0].Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)15m;

			Factory.Save();

			var autoRater = new FreightAutoRater(new RatingContext());

			var autoRatingObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LCL, null, 500m, 1m, rate.Header);
			autoRatingObject.SetVia(LocationHelper.GetLocationFromString("SGSIN", Factory));

			var results = autoRater.AutoRate(new AutoRatingProxy(autoRatingObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 2, results.Count);

			var autoRateInfo = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["FRT"].PK);
			AssertEquals("Sell Rate", 90M, autoRateInfo.Amount);

			autoRateInfo = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["BAF"].PK);
			AssertEquals("Sell Rate", 15M, autoRateInfo.Amount);
		}

		#endregion

		#region Different Company Tariff Levels

		public void TestDifferentCompanyTariffLevels()
		{
			var tariff1 = Helper.NewCompanyTariff();
			var fRTLine = tariff1.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];
			fRTLine.TL_RateCalculator = UnitCalculator.Code;
			fRTLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;
			tariff1.AddRateEntry("ORG", "AIR", "AUSYD", "").AddRateLine("ODOC", FlatCalculator.Code).Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)20m;
			tariff1.AddRateEntry("DST", "AIR", "", "USLAX").AddRateLine("DDOC", FlatCalculator.Code).Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)10m;
			tariff1.Factory.Save();

			var tariff2 = Helper.NewCompanyTariff();
			tariff2.Discounts.SetDiscount(RatingConstants.RateCategory.AIR, 10m);
			tariff2.Factory.Save();

			var tariff3 = Helper.NewCompanyTariff();
			tariff3.Discounts.SetDiscount(RatingConstants.RateCategory.ORG, 20m);
			tariff3.Factory.Save();

			var client = Helper.NewOrgHeader(0, 3, 2, 1);
			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 100m, 0.5m, client);

			var testAutoRater = new FreightAutoRater(new RatingContext());

			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 3, results.Count);
			var info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["FRT"].PK);
			AssertEquals(450m, info.Amount);
			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["ODOC"].PK);
			AssertEquals(16m, info.Amount);
			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["DDOC"].PK);
			AssertEquals(10m, info.Amount);
		}

		public void TestAlwaysCodesOverrideIncotermCodes()
		{
			var tariff1 = Helper.NewCompanyTariff();
			var fRTLine = tariff1.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];
			fRTLine.TL_RateCalculator = UnitCalculator.Code;
			fRTLine.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;
			tariff1.AddRateEntry("ORG", "AIR", "AUSYD", "").AddRateLine("ODOC", FlatCalculator.Code).Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)30m;
			tariff1.AddRateEntry("DST", "AIR", "", "USLAX").AddRateLine("DDOC", FlatCalculator.Code).Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)20m;

			var tariff2 = Helper.NewCompanyTariff();
			tariff2.TH_GlobalRateLevel = 2;
			tariff2.Discounts.SetDiscount(RatingConstants.RateCategory.AIR, 10m);
			tariff2.Discounts.SetDiscount(RatingConstants.RateCategory.ORG, 20m);
			tariff2.Discounts.SetDiscount(RatingConstants.RateCategory.DST, 30m);

			var client = Helper.NewOrgHeader(1);
			var agent = Helper.NewOrgHeader(2);

			tariff1.Factory.Save();
			tariff2.Factory.Save();
			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 100m, 0.5m, client);
			testObject.DebtorOrgs[RatingDebtorOrgTypes.AG] = agent;

			var testAutoRater = new FreightAutoRater(new RatingContext());

			testObject.JobDirection = Directions.Export;
			testObject.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "DDP"));

			IncoTermRegistry.Instance.ChargeAgentAlwaysCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(),
																		Guid.Empty,
																		Guid.Empty,
																		Helper.ChargeCodes["DDOC"].PK + "," + Helper.ChargeCodes["ODOC"].PK + "," + Helper.ChargeCodes["FRT"].PK);

			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 3, results.Count);
			var info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["ODOC"].PK);
			AssertEquals(24m, info.Amount);
			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["FRT"].PK);
			AssertEquals(450m, info.Amount);
			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["DDOC"].PK);
			AssertEquals(14m, info.Amount);

			IncoTermRegistry.Instance.ChargeLocalClientAlwaysCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(),
																			Guid.Empty,
																			Guid.Empty,
																			Helper.ChargeCodes["DDOC"].PK + "," + Helper.ChargeCodes["ODOC"].PK + "," + Helper.ChargeCodes["FRT"].PK);

			Factory.Save();

			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 3, results.Count);
			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["ODOC"].PK);
			AssertEquals(30m, info.Amount);
			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["FRT"].PK);
			AssertEquals(500m, info.Amount);
			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["DDOC"].PK);
			AssertEquals(20m, info.Amount);

			testObject.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "EXW"));
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 3, results.Count);
			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["ODOC"].PK);
			AssertEquals(30m, info.Amount);
			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["FRT"].PK);
			AssertEquals(500m, info.Amount);
			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["DDOC"].PK);
			AssertEquals(20m, info.Amount);
		}

		#endregion

		#region Without Container Type

		public void TestWithoutContainerType()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var entry1 = rate.AddRateEntry("DST", "FCL", "", "AUSYD");
			var line1 = entry1.AddRateLine("DCART", UnitCalculator.Code, RatingConstants.Units.CN);
			line1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)25m;

			Factory.Save();

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var testObject = new AutoRatingObject("USLAX", "AUSYD", FreightMode.FCL, new TestContainers(Factory, ZGuid.Empty, 1, "20GP", 2), 0m, 0m, rate.Header);

			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 1, results.Count);

			AssertEquals("Chargeable Amount", 3m, results[0].GetChargeableFromBasisTest.Amount);
			AssertEquals("Chargeable Unit", "CN", results[0].GetChargeableFromBasisTest.Unit);
			AssertEquals("Sell Rate", 75m, results[0].Amount);
		}

		#endregion

		#region With Multiple Agency Charges

		public void TestWithMultipleAgencyCharges()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var dstEntry = rate.AddRateEntry("DST", "AIR", "", "AUSYD");
			var line1 = dstEntry.AddRateLine("CCLR", AgencyCalculator.Code);
			line1.GetCalculator<AgencyCalculator>().AgencyRate = 120m;
			var line2 = dstEntry.AddRateLine("CCLR", AgencyCalculator.Code);
			line2.Calculator.MessageType = SharedJobMessageTypeList.Codes.Import;
			line2.GetCalculator<AgencyCalculator>().AgencyRate = 100m;
			var line3 = dstEntry.AddRateLine("CCLR", AgencyCalculator.Code);
			line3.Calculator.MessageType = SharedJobMessageTypeList.Codes.Import;
			line3.Calculator.MessageSubType = "FRM";
			line3.GetCalculator<AgencyCalculator>().AgencyRate = 110m;

			Factory.Save();

			var testObject = new AutoRatingObject("USLAX", "AUSYD", FreightMode.LSE, null, 100m, 0.5m, rate.Header);
			testObject.ChargeCodeGroups.Add(ChargeCodeGroupList.Codes.Brokerage);
			testObject.MessageType = SharedJobMessageTypeList.Codes.Import;
			testObject.MessageSubType = "FRM";
			var testAutoRater = new FreightAutoRater(new RatingContext());

			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 1, results.Count);
			var info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["CCLR"].PK);
			AssertEquals(110m, info.Amount);

			testObject.MessageSubType = "SAC";
			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 1, results.Count);
			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["CCLR"].PK);
			AssertEquals(100m, info.Amount);

			testObject.MessageType = SharedJobMessageTypeList.Codes.MiscellaneousCustoms;
			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 1, results.Count);
			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["CCLR"].PK);
			AssertEquals(120m, info.Amount);
		}

		#endregion

		#region With Different AutoRatingDateFiltering

		public void TestWithDifferentAutoRatingDateFiltering()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());

			var entry1f = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry1f.TI_RateEndDate = ZDate.Today.AddDays(7);
			entry1f.RateLines[0].TL_RateCalculator = UnitCalculator.Code;
			entry1f.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 5m;

			var entry2f = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry2f.TI_RateStartDate = ZDate.Today.AddDays(8);
			entry2f.RateLines[0].TL_RateCalculator = UnitCalculator.Code;
			entry2f.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 6m;

			var entry1o = rate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry1o.TI_RateEndDate = ZDate.Today.AddDays(7);
			entry1o.AddRateLine("ODOC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 20m;

			var entry2o = rate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry2o.TI_RateStartDate = ZDate.Today.AddDays(8);
			entry2o.AddRateLine("ODOC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 25m;

			var entry1d = rate.AddRateEntry("DST", "AIR", "", "USLAX");
			entry1d.TI_RateEndDate = ZDate.Today.AddDays(7);
			entry1d.AddRateLine("DDOC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 10m;

			var entry2d = rate.AddRateEntry("DST", "AIR", "", "USLAX");
			entry2d.TI_RateStartDate = ZDate.Today.AddDays(8);
			entry2d.AddRateLine("DDOC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 15m;

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 100m, 0.5m, rate.Header);
			testObject.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "EXW"));

			var jobDatesProvider = new Mock<IJobDatesProvider>();
			testObject.JobDatesProvider = jobDatesProvider.Object;
			jobDatesProvider.Setup(m => m.EarliestPossibleDate).Returns(ZDate.Today);
			jobDatesProvider.Setup(m => m.LatestPossibleDate).Returns(ZDate.Today.AddDays(10));
			jobDatesProvider.Setup(m => m.GetJobDateByType(JobDateTypes.Codes.DepartureDate, It.IsAny<string>())).Returns(ZDate.Today);
			jobDatesProvider.Setup(m => m.GetJobDateByType(JobDateTypes.Codes.ArrivalDate, It.IsAny<string>())).Returns(ZDate.Today.AddDays(10));

			var testAutoRater = new FreightAutoRater(new RatingContext());

			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 3, results.Count);
			var info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["FRT"].PK);
			AssertEquals(500m, info.Amount);
			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["ODOC"].PK);
			AssertEquals(20m, info.Amount);
			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["DDOC"].PK);
			AssertEquals(15m, info.Amount);

			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.Value.FilterType = Constants.RatingDateFilterTypes.Codes.Arrival;
			var configuration = new AutoRateDateByChargeGroupConfiguration
			{
				FilterType = Constants.RatingDateFilterTypes.Codes.Arrival
			};
			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configuration);

			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 3, results.Count);
			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["FRT"].PK);
			AssertEquals(600m, info.Amount);
			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["ODOC"].PK);
			AssertEquals(25m, info.Amount);
			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["DDOC"].PK);
			AssertEquals(15m, info.Amount);

			configuration.FilterType = Constants.RatingDateFilterTypes.Codes.Departure;
			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configuration);
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 3, results.Count);
			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["FRT"].PK);
			AssertEquals(500m, info.Amount);
			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["ODOC"].PK);
			AssertEquals(20m, info.Amount);
			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["DDOC"].PK);
			AssertEquals(10m, info.Amount);

			AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.Value.FilterType = Constants.RatingDateFilterTypes.Codes.Standard;
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 3, results.Count);
			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["FRT"].PK);
			AssertEquals(500m, info.Amount);
			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["ODOC"].PK);
			AssertEquals(20m, info.Amount);
			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["DDOC"].PK);
			AssertEquals(15m, info.Amount);
		}

		public void TestAutoRatingDateFiltering_WhenFallbackIsNotAllowed_DoNotFallbackToStandardDateTypes()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());

			var rateStartDate = ZDate.Today;
			var rateEndDate = rateStartDate.AddMonths(6);

			var entry1 = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry1.TI_RateStartDate = rateStartDate;
			entry1.TI_RateEndDate = rateEndDate;
			var rateLine1 = entry1.RateLines[0];

			Factory.Save();

			var typesToReturnEmptyDate = new ZString[]
			{
				JobDateTypes.Codes.HouseBillIssueDate,
				// The below types have the highest priorities and affect the test result.
				// Having the date values on them defeats the purpose of AutoRateDateConfiguration and the test.
				JobDateTypes.Codes.CostingAutoratingDateOverride,
				JobDateTypes.Codes.RevenueAutoratingDateOverride
			};

			var jobDatesProvider = new Mock<IJobDatesProvider>();
			jobDatesProvider.Setup(m => m.EarliestPossibleDate).Returns(rateStartDate.AddDays(-1));
			jobDatesProvider.Setup(m => m.LatestPossibleDate).Returns(rateEndDate.AddDays(1));
			jobDatesProvider
				.Setup(m => m.GetJobDateByType(It.Is<ZString>(p => typesToReturnEmptyDate.Contains(p)), It.IsAny<string>()))
				.Returns(ZDate.Empty);
			jobDatesProvider
				.Setup(m => m.GetJobDateByType(It.Is<ZString>(p => !typesToReturnEmptyDate.Contains(p)), It.IsAny<string>()))
				.Returns(rateStartDate.AddDays(1));

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 15M, 1M, rate.Header);
			testObject.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "EXW"));
			testObject.JobDatesProvider = jobDatesProvider.Object;

			var testAutoRater = new FreightAutoRater(new RatingContext());

			SetupAutoRateDateConfiguration(isFallbackEnabled: true);
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			var info = results.FirstOrDefault(r => r.ChargeCode.PK == Helper.ChargeCodes["FRT"].PK);
			AssertEquals("Precondition: When fallback is enabled, the rate line is not removed", info?.Line?.PK, rateLine1.PK);

			SetupAutoRateDateConfiguration(isFallbackEnabled: false);
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("When fallback is disabled, the rate line is removed", 0, results.Count);

			#region Helper Methods

			void SetupAutoRateDateConfiguration(bool isFallbackEnabled)
			{
				var autoRateDate = new AutoRateDate
				{
					JobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All,
					Mode = JobConfigurationSelectorLookups.ModeAdditionalCodes.All,
					DirectionCode = Constants.FreightShipmentDirection.Code.All,
					DateType = JobDateTypes.Codes.HouseBillIssueDate,
					IsFallbackDisabled = !isFallbackEnabled
				};

				var configuration = new AutoRateDateByChargeGroupConfiguration
				{
					FilterType = Constants.RatingDateFilterTypes.Codes.Custom,
				};

				var freightChargeGroup = configuration.AutoRateDateByChargeGroups.Cast<AutoRateDateByChargeGroup>()
						.First(item => item.ChargeGroup == ChargeCodeGroupList.Codes.Freight);

				freightChargeGroup.ChargeGroupSettings.Add(autoRateDate);

				AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configuration);
			}

			#endregion
		}

		#endregion

		#region With Local Freight

		public void TestWithLocalFreight()
		{
			var consignor = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader();

			var consignorRate = Helper.NewClientRate(consignor);
			consignorRate.AddRateEntry("LCL", "LCL", "USLAX", "AUSYD").RateLines[0].GetCalculator<MinimumOrPerUnitCalculator>().PerUnit = 100m;
			consignorRate.AddRateEntry("LCL", "LCL", "AUSYD", "AUMEL").RateLines[0].GetCalculator<MinimumOrPerUnitCalculator>().PerUnit = 50m;

			Factory.Save();

			var autoRater = new FreightAutoRater(new RatingContext());

			var testObject = new AutoRatingObject("USLAX", "AUMEL", FreightMode.LCL, null, 500m, 0.5m, consignee);
			testObject.SetVia(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD"));
			testObject.JobDirection = Directions.Import;
			testObject.Consignor = consignor;
			testObject.Consignee = consignee;

			testObject.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "FOB"));
			var results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Charges should not come from Consignor's rate for IMP FRT Collect", 0, results.Count);

			testObject.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "CIF"));
			results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("In a case of Via with local leg -- ONLY local IMP FRT Prepaid leg's rate is pulled from Consignor's rate", 1, results.Count);

			var info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["FRT"].PK);
			AssertEquals(25m, info.Amount);
		}

		#endregion

		#region Mail AutoRating

		public void TestMailAutoRating()
		{
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			testRate.AddRateEntry("DST", "MAI", "", "AUSYD").AddRateLine("DDOC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 15m;
			testRate.AddRateEntry("DST", "ALL", "", "AU").AddRateLine("DSEC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 5m;
			testRate.AddRateEntry("DST", "AIR", "", "AU").AddRateLine("DAWB", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 25m;

			Factory.Save();

			var testAutoRater = new FreightAutoRater(new RatingContext());

			var testObject = new AutoRatingObject("USLAX", "AUSYD", FreightMode.MAI, null, 10m, 0.01m, testRate.Header);
			testObject.JobDirection = Directions.Import;

			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 2, results.Count);
			var info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["DDOC"].PK);
			AssertEquals(15m, info.Amount);
			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["DSEC"].PK);
			AssertEquals(5m, info.Amount);
		}

		#endregion

		#region Container Storage

		public void TestContainerStorageAutoRating()
		{
			Helper.ChargeCodes.New("_TCONTST", "Test Storage", UnitCalculator.Code, ChargeCodeGroupList.Codes.ContainerStorage);
			Helper.ChargeCodes.New("_TLILO", "Test LILO", FlatCalculator.Code, ChargeCodeGroupList.Codes.ContainerStorage);

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry1 = clientRate.AddRateEntry("CST", "ALL", "", "");
			rateEntry1.AddRateLine("_TLILO", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 25m;
			var rateEntry2 = clientRate.AddRateEntry("CST", "SEA", "", "", "", "20GP");
			rateEntry2.AddRateLine("_TCONTST", UnitCalculator.Code, "DY").GetCalculator<UnitCalculator>().PerUnit = 40m;
			var rateEntry3 = clientRate.AddRateEntry("CST", "SEA", "", "", "", "40GP");
			rateEntry3.AddRateLine("_TCONTST", UnitCalculator.Code, "DY").GetCalculator<UnitCalculator>().PerUnit = 60m;

			Factory.Save();

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var criteria = new TestRatingCriteria();
			criteria.LocalClient = clientRate.Header;
			criteria.RateTypeToUse = RateType.CFS;
			criteria.ChargeCodeGroups = new ChargeCodeGroupCollection();
			criteria.ChargeCodeGroups.Add(ChargeCodeGroupList.Codes.ContainerStorage);
			criteria.FreightMode = FreightMode.FCL;
			new TestContainers(Factory, "20GP", 1).PopulateContainerList(criteria.RateableMeasures);
			criteria.SetTime(new TimeInfo(10, 0, 0));
			criteria.PaymentTerm = null;

			var results = testAutoRater.AutoRate(criteria, CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 2, results.Count);
			var info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["_TLILO"].PK);
			AssertEquals(25m, info.Amount);
			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["_TCONTST"].PK);
			AssertEquals(400m, info.Amount);
		}

		#endregion

		#region Palletized Containers

		public void TestWithPalletizedContainers()
		{
			var refContainer1 = Helper.Containers["20GP"].PK;
			var refContainer2 = Helper.Containers["40GP"].PK;

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry1 = clientRate.AddRateEntry("WHS");
			rateEntry1.TI_RC = refContainer1;
			var rateLine1a = rateEntry1.AddRateLine("ODOC", UnitCalculator.Code, "CN");
			rateLine1a.GetCalculator<UnitCalculator>().PerUnit = 100m;
			var rateLine1b = rateEntry1.AddRateLine("ODOC", UnitCalculator.Code, "CN");
			rateLine1b.GetCalculator<UnitCalculator>().PerUnit = 70m;
			rateLine1b.TL_IsOnPallets = true;

			var rateEntry2 = clientRate.AddRateEntry("WHS");
			rateEntry2.TI_RC = refContainer2;
			var rateLine2 = rateEntry2.AddRateLine("ODOC", UnitCalculator.Code, "CN");
			rateLine2.GetCalculator<UnitCalculator>().PerUnit = 190m;

			Factory.Save();

			var testObject = new AutoRatingObject();
			testObject.ConsumerType = JobInvoicingConsumerTypes.WarehouseInwards;
			testObject.RateTypeToUse = RateType.Warehouse;
			testObject.Measures.SetQuantity(MeasureType.Weight, 0m, Core.Constants.Weight.Kilograms);
			testObject.DebtorOrgs[RatingDebtorOrgTypes.LC] = clientRate.Header;

			testObject.Measures.CreateContainerList(includePalletized: true, includeOwnership: true);
			testObject.Measures.AddContainerGroup(refContainer1, "", false, "", new[] { new MeasureInfo.ContainerInfo() });
			testObject.Measures.AddContainerGroup(refContainer1, "", true, "", new[] { new MeasureInfo.ContainerInfo() });
			testObject.Measures.AddContainerGroup(refContainer2, "", false, "", new[] { new MeasureInfo.ContainerInfo() });
			testObject.Measures.AddContainerGroup(refContainer2, "", true, "", new[] { new MeasureInfo.ContainerInfo() });

			var autoRater = new FreightAutoRater(new RatingContext());
			var results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection.ToArray();

			AssertEquals(2, results.Length);
			AssertContains("1 20GP Container(s) @ AUD 100.00/Container", results[0].SingleLineDescription);
			AssertContains("1 20GP Container(s) @ AUD 70.00/Container", results[0].SingleLineDescription);
			AssertContains("1 40GP Container(s) @ AUD 190.00/Container", results[1].SingleLineDescription);
			AssertEquals(100m + 70m + 190m, results.Sum(x => x.Amount));
		}

		#endregion

		#region With Different Commodities

		public void TestWithDifferentCommodities()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var rateEntry1 = clientRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "", "");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			var rateLine1 = rateEntry1.RateLines[0];
			rateLine1.TL_RateCalculator = UnitCalculator.Code;
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 5m;
			var warRateLine = rateEntry1.AddRateLine("WAR", PercentageCalculator.Code);
			warRateLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = Helper.ChargeCodes["FRT"].PK;
			warRateLine.GetCalculator<PercentageCalculator>().Percent = 10m;

			var rateEntry2 = clientRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "", "");
			rateEntry2.TI_RH_NKCommodityCode = "GEN";
			var rateLine2 = rateEntry2.RateLines[0];
			rateLine2.TL_RateCalculator = UnitCalculator.Code;
			rateLine2.GetCalculator<UnitCalculator>().PerUnit = 6m;

			var rateEntry3 = clientRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "", "");
			rateEntry3.TI_RH_NKCommodityCode = "HAZ";
			var rateLine3 = rateEntry3.RateLines[0];
			rateLine3.TL_RateCalculator = UnitCalculator.Code;
			rateLine3.GetCalculator<UnitCalculator>().PerUnit = 7m;

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 0m, 0m, clientRate.Header);
			testObject.Measures.CreateWeightAndVolumeListWithCommodityAndPackageType();
			testObject.Measures.AddWeightAndVolumeWithCommodityAndPackageType(50m, null, "GEN", "");
			testObject.Measures.AddWeightAndVolumeWithCommodityAndPackageType(100m, null, "HAZ", "");
			testObject.Measures.AddWeightAndVolumeWithCommodityAndPackageType(80m, null, "REF", "");

			var autoRater = new FreightAutoRater(new RatingContext());
			var results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Result Count", 4, results.Count);
			AssertEquals("Result should equal (50*6 + 100*7 + 80*5)", 1400m, results.Where(x => x.ChargeCode.PK == Helper.ChargeCodes["FRT"].PK).Sum(x => x.Amount));
			var info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["WAR"].PK);
			AssertEquals(140m, info.Amount);
		}

		#endregion

		#region CFS

		public void TestCFSRating()
		{
			var chargeCodeCFSPack = Helper.ChargeCodes["CFSPACK"];
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line1 = rate.AddRateEntry("PAC", "FCL", "AUSYD", "").AddRateLine(chargeCodeCFSPack, UnitCalculator.Code, RatingConstants.Units.CN);
			line1.GetCalculator<UnitCalculator>().PerUnit = 140m;

			var chargeCodeCFSUnpack = Helper.ChargeCodes["CFSUNPA"];
			var line2 = rate.AddRateEntry("UNP", "FCL", "", "USLAX").AddRateLine(chargeCodeCFSUnpack, UnitCalculator.Code, RatingConstants.Units.CN);
			line2.GetCalculator<UnitCalculator>().PerUnit = 110m;

			var line3 = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "20GP").RateLines[0];
			line3.GetCalculator<UnitCalculator>().PerUnit = 1000m;

			var line4 = rate.AddRateEntry("ORG", "FCL", "AUSYD", "").AddRateLine("ODOC", FlatCalculator.Code);
			line4.GetCalculator<FlatCalculator>().BaseRate = 35m;

			var line5 = rate.AddRateEntry("DST", "FCL", "", "USLAX").AddRateLine("DDOC", FlatCalculator.Code);
			line5.GetCalculator<FlatCalculator>().BaseRate = 25m;

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.FCL, new TestContainers(Factory, "20GP", 3), 0m, 0m, rate.Header);
			testObject.ChargeCodeGroups.Add(ChargeCodeGroupList.Codes.CFSLoadList);
			testObject.ChargeCodeGroups.Add(ChargeCodeGroupList.Codes.Destination);
			testObject.ChargeCodeGroups.Add(ChargeCodeGroupList.Codes.Origin);
			testObject.ChargeCodeGroups.Add(ChargeCodeGroupList.Codes.Freight);

			var testAutoRater = new FreightAutoRater(new RatingContext());

			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 3, results.Count);

			var info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["FRT"].PK);
			AssertEquals(3000m, info.Amount);

			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["ODOC"].PK);
			AssertEquals(35m, info.Amount);

			info = results.First(r => r.ChargeCode.PK == Helper.ChargeCodes["DDOC"].PK);
			AssertEquals(25m, info.Amount);

			// Add services and change rate type of the same job
			testObject.JobServices = new JobServicesCollection
			{
				new JobServiceInfo(true, ChargeCodeGroupList.Codes.CFSLoadList, chargeCodeCFSPack.AC_ChargeSubGroup, "Packing Service", 1m),
				new JobServiceInfo(true, ChargeCodeGroupList.Codes.CFSLoadList, chargeCodeCFSUnpack.AC_ChargeSubGroup, "Unpacking Service", 1m)
			};
			testObject.RateTypeToUse = RateType.CFS;
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals(2, results.Count);

			info = results.First(r => r.ChargeCode.PK == chargeCodeCFSPack.PK);
			AssertEquals(420m, info.Amount);

			info = results.First(r => r.ChargeCode.PK == chargeCodeCFSUnpack.PK);
			AssertEquals(330m, info.Amount);

			// Change rate type of the same job
			testObject.RateTypeToUse = RateType.CFS | RateType.Forwarding;
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count", 5, results.Count);
		}

		#endregion

		#region Zone Rating

		public void TestZoneRating_CarrierZone()
		{
			TestCaseHelper.ClearTable(RefZonePivotSchema.Constants.TableName);
			TestCaseHelper.ClearTable(RefZoneHeaderSchema.Constants.TableName);

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			Factory.Save();

			var carrierZone = Helper.NewInternationalZone("ZUB1", carrier, "AUMEL");

			var rate = Helper.NewClientRate(NewClient);
			var entry1 = rate.AddRateEntry("AIR", "LSE", "ZUB1", "USLAX");

			Factory.Save();

			var testObject = new AutoRatingObject("AUMEL", "USLAX", FreightMode.LSE, null, 300m, 0m, NewClient);

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals(0, results.Count);

			testObject.Carrier = carrier;
			testObject.Creditors = Creditors.New(GetNewTestOrgWithSourceFromHeader(carrier));
			Factory.Save();

			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals(1, results.Count);
			AssertEquals(entry1.PK, results[0].Entry.PK);

			var entry2 = rate.AddRateEntry("AIR", "LSE", "AUMEL", "USLAX");
			Factory.Save();

			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals(1, results.Count);
			AssertEquals("Ports more important than Zones", entry2.PK, results[0].Entry.PK);
		}

		public void TestZoneRating_CarrierAndClientZone()
		{
			var client = NewClient;
			var rate = Helper.NewClientRate(client);
			var entry1 = rate.AddRateEntry("AIR", "LSE", "ZUB1", "USLAX");

			Factory.Save();

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;

			var carrierZone = Helper.NewInternationalZone("ZUB1", carrier, "AUMEL");
			var clientZone = Helper.NewInternationalZone("ZUB2", client, "AUMEL");

			Factory.Save();

			var testObject = new AutoRatingObject("AUMEL", "USLAX", FreightMode.LSE, null, 300m, 0m, client);

			var testAutoRater = new FreightAutoRater(new RatingContext());
			var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals(0, results.Count);

			testObject.Carrier = carrier;
			testObject.Creditors = Creditors.New(GetNewTestOrgWithSourceFromHeader(carrier));

			Factory.Save();

			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals(1, results.Count);
			AssertEquals(entry1.PK, results[0].Entry.PK);

			var entry3 = rate.AddRateEntry("AIR", "LSE", "ZUB2", "USLAX");

			Factory.Save();

			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals(1, results.Count);
			AssertEquals("Client Zone takes precedence over Carrier Zone", entry3.PK, results[0].Entry.PK);

			entry3.Delete();
			Factory.Save();

			testAutoRater = new FreightAutoRater(new RatingContext());
			results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals(1, results.Count);
			AssertEquals(entry1.PK, results[0].Entry.PK);
		}

		#endregion

		#region Weight Break Override

		public void TestWeightBreakOverride()
		{
			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartSell();

				var testRate = SetupExportClientRatesForAutoRater();

				var testObject = new AutoRatingObjectWithWeightBreakOverride("AUSYD", "USLAX", FreightMode.LSE, 120m, 0m);
				testObject.DebtorOrgs[RatingDebtorOrgTypes.LC] = testRate.Header;

				var testAutoRater = new FreightAutoRater(new RatingContext());
				var results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
				var info = results.First(r => r.ChargeCode.PK == TestFRT.PK);
				AssertEquals("Sell Rate", 120m * 3.5m, info.Amount);

				testObject.WeightBreakOverrideForTest = 35m;
				testObject.WeightBreakOverrideUnit = Core.Constants.Weight.Kilograms;
				results = testAutoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
				info = results.First(r => r.ChargeCode.PK == TestFRT.PK);
				AssertEquals("Sell Rate", 35m * 4.5m, info.Amount);
				AssertEquals("TESTFRT: 35 Kilogram(s) @ AUD 4.50/KG", info.SingleLineDescription);
			}
		}

		#endregion

		#region AutoRate FCL With More Than One Flat Calculator

		public void TestAutoRateFCLWithMoreThanOneFlatCalculatorsProblem()
		{
			// Note: This test has the effect of ensuring SumUpSameCharges is called before
			// charges are applied to the job.
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var entry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.SEA, "AUSYD", "USLAX");
			var line1 = entry1.AddRateLine("DDOC", FlatCalculator.Code);
			line1.Calculator[FlatCalculator.Items.Operator.BAS] = (ZDecimal)1000m;

			var entry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.SEA, "AU", "US");
			var line2 = entry1.AddRateLine("DDOC", FlatCalculator.Code);
			line2.Calculator[FlatCalculator.Items.Operator.BAS] = (ZDecimal)1900m;

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.FCL, null, 0m, 0m, clientRate.Header);

			var autoRater = new FreightAutoRater(new RatingContext());
			var results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Result Count", 1, results.Count);

			AssertEquals(2900m, results[0].Amount);

			var descriptions = results[0].SingleLineDescription.Split('\n').Select(desc => (string)desc.Trim()).ToArray();
			AssertContainsExactElementsInAnyOrder(new[] { "DDOC: Base Rate USD 1000.00", "DDOC: Base Rate USD 1900.00" }, descriptions);

			AssertEquals("Destination Documentation Fee", results[0].InvoiceLineDescription);
		}

		#endregion

		public void TestAutorateShouldContinueIfUserSkipSeaRateSelection()
		{
			var containerPK = Helper.Containers["20GP"].PK;

			var costing = Helper.NewCosting(null);
			var costEntry = costing.AddRateEntry("FCL", "SEA", "AU", "");
			costEntry.TI_RC = containerPK;

			var costLine = costEntry.RateLines[0];
			costLine.TL_RateCalculator = UnitCalculator.Code;
			costLine.GetCalculator<UnitCalculator>().PerUnit = 5m;

			Factory.Save();

			var dummyObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.FCL, new TestContainers(Factory, containerPK, 3), 0m, 0m, NewClient, supportsManualRateSelection: true);

			using (_Rating.Start(new Mock<IAutoRatingGUIInteractor>().Object, isEqualization: false))
			{
				var context = RatingContext.CreateForManualSelect(new TestLogger(), new Mock<IDialogService>().Object);
				var autoRater = new FreightAutoRater(context);
				Mock.Get(_Rating.Interactor)
					.Setup(m => m.SelectRate(It.IsAny<IRatingContext>(), It.IsAny<RatingCriteria>()))
					.Returns((IEnumerable<AutoRateInfo>)null);

				var results = autoRater.AutoRate(new AutoRatingProxy(dummyObject), CostSell.Cost);
				var expected = new[]
				{
					new SimpleArInfo
					{
						Amount = 15m,
						InvoiceLineDesc = "International Freight",
						CalculationSingleLineDescription = "FRT: 3 20GP Container(s) @ USD 5.00/Container"
					}
				};

				AssertRatingResults(expected, results);
			}
		}

		public void TestAutorateShouldContinueIfUserSkipAirRateSelection()
		{
			var costing = Helper.NewCosting(null);
			var costEntry = costing.AddRateEntry("AIR", "LSE", "AU", "");

			var costLine = costEntry.RateLines[0];
			costLine.TL_RateCalculator = UnitCalculator.Code;
			costLine.TL_WeightVolume = Core.Constants.Weight.Kilograms;
			costLine.GetCalculator<UnitCalculator>().PerUnit = 5m;

			Factory.Save();

			var dummyObject = new AutoRatingObject("AUSYD", "USLAX", FreightMode.LSE, null, 200m, 1m, NewClient, supportsManualRateSelection: true);

			IEnumerable<AutoRateInfo> skipRates = null;
			var guiInteractor = new Mock<IAutoRatingGUIInteractor>();
			guiInteractor
				.Setup(i => i.SelectRate(Moq.It.IsAny<IRatingContext>(), Moq.It.IsAny<RatingCriteria>()))
				.Returns(skipRates);

			using (_Rating.Start(guiInteractor.Object, isEqualization: false))
			{
				var context = RatingContext.CreateForManualSelect(new TestLogger(), new Mock<IDialogService>().Object);
				var autoRater = new FreightAutoRater(context);

				var results = autoRater.AutoRate(new AutoRatingProxy(dummyObject), CostSell.Cost);
				var expected = new[]
				{
					new SimpleArInfo
					{
						Amount = 1000m,
						InvoiceLineDesc = "International Freight",
						CalculationSingleLineDescription = "FRT: 200 Kilogram(s) @ AUD 5.00/KG"
					}
				};

				AssertRatingResults(expected, results);
			}
		}

		OrgWithSource GetNewTestOrgWithSourceFromHeader(OrgHeader org)
		{
			return OrgWithSource.New(org, new List<string>() { "Provider" });
		}

		#region Merge Charges Only Within Adapter

		public void TestMergeCharges_WithinAdapter()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var rateEntry = clientRate.AddRateEntry("DST", "FCL", "", "AUSYD");
			var line1 = rateEntry.AddRateLine("DCART", UnitCalculator.Code, RatingConstants.Units.KG);
			line1.GetCalculator<UnitCalculator>().PerUnit = 4m;
			var line2 = rateEntry.AddRateLine("DCART", FlatCalculator.Code);
			line2.GetCalculator<FlatCalculator>().BaseRate = 500m;

			Factory.Save();

			var autoRater = new FreightAutoRater(new RatingContext());
			var testObject = new AutoRatingObject("USLAX", "AUSYD", FreightMode.FCL, null, 50m, 0m, clientRate.Header);

			var results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;

			AssertEquals("Only one results will be valid as we're merging within adapter", 1, results.Count);
			var result = results[0];
			AssertEquals(MergeChargeOptions.WithinAdapter, testObject.MergeCharges);
			AssertEquals(700m, result.Amount);
			AssertEquals("DCART: 50 Kilogram(s) @ AUD 4.00/KG\n\tDCART: Base Rate AUD 500.00", result.SingleLineDescription);
		}

		#endregion

		#region Excluded From Autocosting Tests
		public void TestLocalStandardCostingThatIsExcludedFromAutocosting()
		{
			var origin = "AUSYD";
			var destination = "USLAX";

			var client = Helper.NewOrgHeader();
			var costing = Helper.NewCosting(null);

			var autoRater = new FreightAutoRater(new RatingContext());

			var entry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, origin, destination, "FRT", 100m);
			Factory.Save();

			var testObject = new AutoRatingObject(origin, destination, FreightMode.LSE, null, 2750M, 1.5M, client);

			var results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Cost).RateInfoCollection;
			AssertEquals("Result Count", 1, results.Count);

			entry.TI_IsExcludedFromAutoRating = true;
			Factory.Save();

			results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Cost).RateInfoCollection;
			AssertEquals("Result Count", 0, results.Count);
		}

		public void TestGlobalStandardCostingThatIsExcludedFromAutocosting()
		{
			var origin = "AUSYD";
			var destination = "USLAX";

			var client = Helper.NewOrgHeader();
			var costing = Helper.NewGlobalCosting(null);
			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("FRT");

			var entry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, origin, destination);
			entry.RateLines.RemoveAndDeleteAll();
			entry.AddRateLine(globalChargeCode).GetCalculator<FlatCalculator>().BaseRate = 200m;
			Factory.Save();

			var autoRater = new FreightAutoRater(new RatingContext());

			var testObject = new AutoRatingObject(origin, destination, FreightMode.LSE, null, 2750M, 1.5M, client);

			var results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Cost).RateInfoCollection;
			AssertEquals("Result Count", 1, results.Count);

			entry.TI_IsExcludedFromAutoRating = true;
			Factory.Save();

			results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Cost).RateInfoCollection;
			AssertEquals("Result Count", 0, results.Count);
		}
		#endregion

		#region Exclude from Company Tariffs Tests
		public void TestRemoveCompanyTariffChargeCodeIfExistsInClientRate()
		{
			var origin = "AUSYD";
			var destination = "USLAX";

			var companytariff = Helper.NewCompanyTariff();

			var entry = companytariff.AddRateEntry("LCL", "LCL", origin, destination);
			entry.RateLines.RemoveAndDeleteAll();

			var tariffLine1 = entry.AddRateLine("BAF", FlatCalculator.Code);
			tariffLine1.Calculator[FlatCalculator.Items.Operator.BAS] = (ZDecimal)100m;

			var tariffLine2 = entry.AddRateLine("CAF", FlatCalculator.Code);
			tariffLine2.Calculator[FlatCalculator.Items.Operator.BAS] = (ZDecimal)200m;

			companytariff.Factory.Save();

			var client = Helper.NewOrgHeader(1);

			var rate = Helper.NewClientRate(client);

			var clientEntry = rate.AddRateEntry("LCL", "LCL", origin, destination);
			clientEntry.RateLines.RemoveAndDeleteAll();
			clientEntry.AddRateLine("BAF", ExcludeCompanyTariffsCalculator.Code);

			rate.Factory.Save();

			var testObject = new AutoRatingObject(origin, destination, FreightMode.LCL, null, 2750M, 1.5M, client);
			var autoRater = new FreightAutoRater(new RatingContext());

			var results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count with EXL Filter", 1, results.Count);

			results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Cost).RateInfoCollection;
			AssertEquals("Result Count with EXL Filter", 0, results.Count);

			clientEntry.RateLines.RemoveAndDeleteAll();
			rate.Factory.Save();

			results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertEquals("Result Count without EXL Filter", 2, results.Count);

			results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Cost).RateInfoCollection;
			AssertEquals("Result Count without EXL Filter", 0, results.Count);
		}
		#endregion

		public void TestAutoRateImportCollect_ShouldFilterPrepaidEntry()
		{
			var expected = new[]
			{
				new SimpleArInfo { Amount = 100m, InvoiceLineDesc = "International Freight" },
				new SimpleArInfo { Amount = 200m, InvoiceLineDesc = "Bunker Adjustment Factor" },
			};
			AssertFilterCostRateLine_GivenCriteriaPaymentTerm("USLAX", "AUSYD", Constants.PaymentType.Collect, expected);
		}

		public void TestAutoRateExportPrepaid_ShouldFilterCollectEntry()
		{
			var expected = new[]
			{
				new SimpleArInfo { Amount = 100m, InvoiceLineDesc = "International Freight" },
				new SimpleArInfo { Amount = 300m, InvoiceLineDesc = "Currency Adjustment Factor" },
			};
			AssertFilterCostRateLine_GivenCriteriaPaymentTerm("AUSYD", "USLAX", Constants.PaymentType.Prepaid, expected);
		}

		public void TestAutoRateUndefinedPrepaidCollectPaymentTerm_ShouldNotFilterEntries()
		{
			var expected = new[]
			{
				new SimpleArInfo { Amount = 100m, InvoiceLineDesc = "International Freight" },
				new SimpleArInfo { Amount = 200m, InvoiceLineDesc = "Bunker Adjustment Factor" },
				new SimpleArInfo { Amount = 300m, InvoiceLineDesc = "Currency Adjustment Factor" },
			};
			AssertFilterCostRateLine_GivenCriteriaPaymentTerm("AUSYD", "USLAX", null, expected);
		}

		void AssertFilterCostRateLine_GivenCriteriaPaymentTerm(string origin, string destination, string jobPaymentType, SimpleArInfo[] expected)
		{
			var client = Helper.NewOrgHeader();
			var costing = Helper.NewCosting(null);

			var entry1 = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, origin, destination, "FRT", 100m);
			entry1.TI_PaymentTerm = "";

			var entry2 = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, origin, destination, "BAF", 200m);
			entry2.TI_PaymentTerm = WRConstants.PaymentTerm.Collect;

			var entry3 = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, origin, destination, "CAF", 300m);
			entry3.TI_PaymentTerm = WRConstants.PaymentTerm.Prepaid;

			Factory.Save();

			var autoRater = new FreightAutoRater(new RatingContext());
			var testObject = new AutoRatingObject(origin, destination, FreightMode.LSE, null, 2750M, 1.5M, client);
			testObject.JobDirection = origin == "AUSYD" ? Directions.Export : Directions.Import;
			testObject.Consignee = Consignee;
			testObject.Consignor = Consignor;

			if (!string.IsNullOrWhiteSpace(jobPaymentType))
			{
				testObject.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.PrepaidCollect, CostSell.Cost, jobPaymentType));
			}

			var results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Cost).RateInfoCollection;
			AssertRatingResults(expected, results);
		}

		#region Payment Term Override
		public void TestPaymentTermOverride_ExactMatch()
		{
			var expected = new[]
			{
				new SimpleArInfo { Amount = 300m, InvoiceLineDesc = "International Freight" },
			};

			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);

			var entry1 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "USLAX", "FRT", 100m);
			entry1.TI_PaymentTerm = "";

			var entry2 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "USLAX", "FRT", 200m);
			entry2.TI_PaymentTerm = RatingConstants.PaymentTerms.Collect;

			var entry3 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "USLAX", "FRT", 300m);
			entry3.TI_PaymentTerm = RatingConstants.PaymentTerms.Prepaid;

			Factory.Save();

			var autoRater = new FreightAutoRater(new RatingContext());
			var origin = "AUSYD";
			var destination = "USLAX";
			var testObject = new AutoRatingObject(origin, destination, FreightMode.LSE, null, 2750M, 1.5M, client, "PPD");

			testObject.Consignee = Consignee;
			testObject.Consignor = Consignor;

			var results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertRatingResults(expected, results);
		}

		public void TestPaymentTermOverride_Fallback()
		{
			var expected = new[]
			{
				new SimpleArInfo { Amount = 100m, InvoiceLineDesc = "International Freight" },
			};

			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);

			var entry1 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "USLAX", "FRT", 100m);
			entry1.TI_PaymentTerm = "";

			var entry2 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "USLAX", "FRT", 200m);
			entry2.TI_PaymentTerm = RatingConstants.PaymentTerms.Collect;

			Factory.Save();

			var autoRater = new FreightAutoRater(new RatingContext());
			var origin = "AUSYD";
			var destination = "USLAX";
			var testObject = new AutoRatingObject(origin, destination, FreightMode.LSE, null, 2750M, 1.5M, client, "PPD");

			testObject.Consignee = Consignee;
			testObject.Consignor = Consignor;

			var results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertRatingResults(expected, results);
		}

		public void TestPaymentTermOverride_CriterialNotSpecified()
		{
			var expected = new[]
			{
				new SimpleArInfo { Amount = 100m, InvoiceLineDesc = "International Freight" },
			};

			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);

			var entry1 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "USLAX", "FRT", 100m);
			entry1.TI_PaymentTerm = "";

			var entry2 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "USLAX", "FRT", 200m);
			entry2.TI_PaymentTerm = RatingConstants.PaymentTerms.Collect;

			var entry3 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "USLAX", "FRT", 200m);
			entry3.TI_PaymentTerm = RatingConstants.PaymentTerms.Prepaid;

			Factory.Save();

			var autoRater = new FreightAutoRater(new RatingContext());
			var origin = "AUSYD";
			var destination = "USLAX";
			var testObject = new AutoRatingObject(origin, destination, FreightMode.LSE, null, 2750M, 1.5M, client, "");

			testObject.Consignee = Consignee;
			testObject.Consignor = Consignor;

			var results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue).RateInfoCollection;
			AssertRatingResults(expected, results);
		}
		#endregion

		#region Auto Rating Logging RatingAdapter - Test Cases

		public void TestAutoRatingLoggingRatingAdapterIfExceptionOccurs_AutoRate()
		{
			AssertAutoRatingLoggingRatingAdapterIfExceptionOccurs("AutoRate", (testAutoRater, criteria) => testAutoRater.AutoRate(criteria, CostSell.Revenue));
		}

		public void TestAutoRatingLoggingRatingAdapterIfExceptionOccurs_CalculateResultsForBestMatches()
		{
			var filterOptions = new NotApplicableRateLineRemover.FilterOptions { DisableSpotFilter = true };
			AssertAutoRatingLoggingRatingAdapterIfExceptionOccurs("CalculateResultsForBestMatches", (testAutoRater, criteria) => testAutoRater.CalculateResultsForBestMatches(CostSell.Revenue, criteria, new List<RateEntry>(), filterOptions));
		}

		void AssertAutoRatingLoggingRatingAdapterIfExceptionOccurs(string methodToTest, Action<FreightAutoRater, RatingCriteria> action)
		{
			const string expectedException = "Exception from TestAutoRatingLoggingRatingAdapterIfExceptionOccurs.";

			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneePK = Consignee.PK;
			shipment.ConsignorPK = Consignor.PK;

			var ratingContext = new Mock<IRatingContext>();
			ratingContext.SetupSequence(m => m.Factory)
				.Returns(Factory)
				.Throws(new Exception(expectedException));

			var testAutoRater = new FreightAutoRater(ratingContext.Object);
			var proxy = new AutoRatingProxy(shipment.RatingAdapter);
			var criteria = new RatingCriteria(proxy, Factory);
			var exception = AssertExceptionThrown<Exception>(() => action(testAutoRater, criteria));

			AssertEquals("Precondition", expectedException, exception.Message);

			var classFullName = typeof(FreightAutoRater).FullName;
			var stackTraceLines = exception.StackTrace.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);
			var index = stackTraceLines.IndexOf((x) => x.Contains($"{classFullName}.{methodToTest}("));
			Assert("Precondition: index should be greater than or equal to zero.", index >= 0);
			Assert("Precondition: stackTraceLines.Length should be greater than index + 3.", stackTraceLines.Length > (index + 3));

			CombineAssertions("To re-throw exception while logging RatingAdapter info, we should use ExceptionDispatchInfo as it will maintain exception trace with original line num.", () =>
			{
				Assert(stackTraceLines[++index].Contains("--- End of stack trace from previous location where exception was thrown ---"));
				Assert(stackTraceLines[++index].Contains("System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()"));
				Assert(stackTraceLines[++index].Contains($"{classFullName}.{methodToTest}("));
			});

			var autoRatingInfo = exception.Data["AutoRatingInfo"];
			AssertNotNull(autoRatingInfo);

			var xmlDocument = new XmlDocument();
			AssertNoExceptionThrown(() => xmlDocument.LoadXml(autoRatingInfo.ToString()));

			var consignorAddress = xmlDocument.SelectNodes("root//ForwardingShipmentRatingAdapter//IAutoRatingOrganisations//PickupAddress");
			Assert(consignorAddress.Count > 0);
			AssertEquals("CONSIGNOR #1 NSW", consignorAddress[0].InnerText);

			var consigneeAddress = xmlDocument.SelectNodes("root//ForwardingShipmentRatingAdapter//IAutoRatingOrganisations//DeliveryAddress");
			Assert(consigneeAddress.Count > 0);
			AssertEquals("CONSIGNEE #1 NSW", consigneeAddress[0].InnerText);
		}

		public void TestBuildAutoRatingInfoForLoggingException_DoesNotThrowException()
		{
			var ratingContext = new Mock<IRatingContext>();
			ratingContext.SetupSequence(m => m.Factory)
				.Returns(Factory)
				.Throws(new Exception("Exception XYZ."));

			var testAutoRater = new FreightAutoRater(ratingContext.Object);
			var nullCriteria = (RatingCriteria)null;
			var exception = AssertExceptionThrown<Exception>(() => testAutoRater.AutoRate(nullCriteria, CostSell.Revenue));

			// Checks that the exception reported is the correct one.
			AssertEquals("Precondition", "Exception XYZ.", exception.Message);

			// Checks the handling of an exception caused while preparing the
			// AutoRatingInfo for the caught `Exception XYZ`
			var autoRatingInfo = (ZString)exception.Data["AutoRatingInfo"];
			AssertNotNull(autoRatingInfo);
			AssertContains("<Message>Object reference not set to an instance of an object.</Message>", autoRatingInfo);
		}

		public void TestAddAutoRatingInfoToException_HandlesDuplicateKey()
		{
			var expectedException = new Exception("Exception XYZ.");
			expectedException.Data.Add("AutoRatingInfo", "Hello Rating Explorer, blah blah.");

			var ratingContext = new Mock<IRatingContext>();
			ratingContext.SetupSequence(m => m.Factory)
				.Returns(Factory)
				.Throws(expectedException);

			var nullCriteria = (RatingCriteria)null;
			var testAutoRater = new FreightAutoRater(ratingContext.Object);
			var exception = AssertExceptionThrown<Exception>(() => testAutoRater.AutoRate(nullCriteria, CostSell.Revenue));

			AssertEquals("Precondition", "Exception XYZ.", exception.Message);

			var autoRatingInfo = exception.Data["AutoRatingInfo"];
			AssertNotNull(autoRatingInfo);
			AssertEquals("Hello Rating Explorer, blah blah.", autoRatingInfo);
		}

		#endregion

		#region Load Possible Matching Rates

		public void TestAutoRate_PossibleMatches()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD", commodity: "WOOL");

			Factory.Save();

			var autoRater = new FreightAutoRater(new RatingContext());
			var testObject = new AutoRatingObject("UAIEV", "AUSYD", FreightMode.LCL, null, 1000M, 5M, NewClient);
			testObject.OverriddenCommodity = new[] { Factory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, "WOOL") };

			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartSell();

				var results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue, searchForAdditionalRates: true);
				AssertEquals(1, results.PossibleMatchesWrapper.PossibleMatches.Count);

				testObject.GatewayBillingSupporter = new GatewayBillingSupporterForTest(null, null, true);
				results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue, searchForAdditionalRates: true);
				AssertEquals("Possible matches should not work for gateway billing.", 0, results.PossibleMatchesWrapper.PossibleMatches.Count);

				testObject.GatewayBillingSupporter = null;
				rateEntry.TI_RH_NKCommodityCode = "SHIP";
				results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);
				AssertEquals(0, results.PossibleMatchesWrapper.PossibleMatches.Count);
			}
		}

		#endregion

		#region Usage Report - Rates Loaded Stats

		public void TestExecuteAutoRatingRevenue_ShouldReportAutoRateUsageEvent()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			clientRate.AddRateEntryWithFlatRateLine("AIR", "LSE", "AUSYD", "CNNKG", "FRT", 600m);
			clientRate.AddRateEntryWithFlatRateLine("AIR", "LSE", "AU", "", "FRT", 600m, commodity: "HAZ");

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "CNNKG", FreightMode.LSE, null, 1000m, 0m, NewClient);
			var autoRater = new FreightAutoRater(new RatingContext());

			autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);

			var helper = new UsageCollectorTestHelper(Factory);
			var messages = helper.LoadUsageMessages(UsageFeatures.Codes.RatesLoaded);
			var message = messages.First();
			var actualStats = message.GetProperty<RatesLoadedStats>(UsageProperties.RatesLoadedStats);
			var elapsedTime = message.GetProperty<long>(UsageProperties.ElapsedTime);
			var dbStats = message.GetProperty<DbStats>(UsageProperties.DBStats);
			AssertEquals("LoadedRatesCount should match", 2, actualStats.LoadedRatesCount);
			AssertEquals("FilteredRatesCount should match", 1, actualStats.FilteredRatesCount);
			AssertGreaterThan("TotalElapsedTimeMs should match", elapsedTime, 0);
			AssertNotNull(dbStats);
		}

		public void TestExecuteAutoRatingCosting_ShouldReportAutoRateUsageEvent()
		{
			var costing = Helper.NewCosting(null);
			costing.AddRateEntryWithFlatRateLine("AIR", "LSE", "AUSYD", "CNNKG", "FRT", 600m);
			costing.AddRateEntryWithFlatRateLine("AIR", "LSE", "AU", "", "FRT", 600m, commodity: "HAZ");

			Factory.Save();

			var testObject = new AutoRatingObject("AUSYD", "CNNKG", FreightMode.LSE, null, 1000m, 0m, NewClient);
			var autoRater = new FreightAutoRater(new RatingContext());

			autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Cost);

			var helper = new UsageCollectorTestHelper(Factory);
			var messages = helper.LoadUsageMessages(UsageFeatures.Codes.RatesLoaded);
			var message = messages.First();
			var actualStats = message.GetProperty<RatesLoadedStats>(UsageProperties.RatesLoadedStats);
			var elapsedTime = message.GetProperty<long>(UsageProperties.ElapsedTime);
			var dbStats = message.GetProperty<DbStats>(UsageProperties.DBStats);
			AssertEquals("LoadedRatesCount should match", 2, actualStats.LoadedRatesCount);
			AssertEquals("FilteredRatesCount should match", 1, actualStats.FilteredRatesCount);
			AssertGreaterThan("TotalElapsedTimeMs should match", elapsedTime, 0);
			AssertNotNull(dbStats);
		}

		#endregion
	}

	#region Test Objects

	#region Extensions

	public static class FreightAutoRateTestExtensions
	{
		public static AutoRateResult AutoRate(this FreightAutoRater autorater, AutoRatingProxy proxy, CostSell costOrSell, bool searchForAdditionalRates = false)
		{
			return autorater.AutoRate(new RatingCriteria(proxy, autorater.Factory), costOrSell, searchForAdditionalRates);
		}
	}

	#endregion

	#region Charge

	public class TestCharge : JobCharge, IAutoRatingChargeInfo
	{
		public TestCharge(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public override ZDecimal JR_OSCostGSTAmt_Calc { get => base.JR_OSCostGSTAmt; set => base.JR_OSCostGSTAmt = value; }

		public override ILocation CostPlaceOfSupplyLocation => null;

		public override ILocation SellPlaceOfSupplyLocation => null;

		public override bool CanReautorate(CostSell costOrSell, params ZString[] adapterIDs)
		{
			return costOrSell == CostSell.Cost ? JR_Calc_CostRatingBehavior == JobChargeLookups.ReAutorateCharge : JR_Calc_SellRatingBehavior == JobChargeLookups.ReAutorateCharge;
		}
	}

	#endregion

	#region Autorating Object

	public class AutoRatingObject : AutoRatingProxy
	{
		public AutoRatingObject()
			: base(null)
		{
			factory = new BusinessObjectFactory();
			ValuesCanBeSet = true;

			RateTypeToUse = RateType.Forwarding;

			ChargeCodeGroups = new ChargeCodeGroupCollection();
			ChargeCodeGroups.Add(ChargeCodeGroupList.Codes.Freight);
			ChargeCodeGroups.Add(ChargeCodeGroupList.Codes.Origin);
			ChargeCodeGroups.Add(ChargeCodeGroupList.Codes.Destination);

			JobDirection = Directions.Import;
			PaymentTerm = new PaymentTermInfos();
			PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "EXW"));
			ServiceLevel = new ServiceLevelRatingInformation(new ServiceLevelInfo("STD", ServiceLevelType.Client));

			JobDatesProvider = new JobDatesProvider<DummyBusinessObject>(factory.NewWithValidTestData<DummyBusinessObject>());
			RateableMeasures = new RateableMeasureSet();
			CurrencyConverter = NonOrgSpecificExRateCurrencyConverter.Default(factory);
		}

		public AutoRatingObject(ZString origin, ZString destination, FreightMode freightMode, TestContainers containers, ZDecimal weightInKG, ZDecimal volumeInM3, ZString clientName)
			: this(origin, destination, freightMode, containers, weightInKG, volumeInM3, null, ZString.Empty)
		{
			DebtorOrgs[RatingDebtorOrgTypes.LC] = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, clientName);
		}

		public AutoRatingObject(ZString origin, ZString destination, FreightMode freightMode, TestContainers containers, ZDecimal weightInKG, ZDecimal volumeInM3, OrgHeader billTo, bool? supportsManualRateSelection = null)
			: this(origin, destination, freightMode, containers, weightInKG, volumeInM3, billTo, ZString.Empty)
		{
			this.supportsManualRateSelection = supportsManualRateSelection;
		}

		public AutoRatingObject(ZString origin, ZString destination, FreightMode freightMode, TestContainers containers, ZDecimal weightInKG, ZDecimal volumeInM3, OrgHeader billTo, ZString paymentTermOverride)
			: this()
		{
			Origin = LocationHelper.GetLocationFromString(origin, factory);
			Destination = LocationHelper.GetLocationFromString(destination, factory);
			FreightMode = freightMode;
			var measures = Measures;
			measures.SetQuantity(MeasureType.Weight, weightInKG, Core.Constants.Weight.Kilograms);
			measures.SetQuantity(MeasureType.Volume, volumeInM3, Core.Constants.Volume.CubicMetres);

			if (containers != null)
			{
				containers.PopulateContainerList(measures);
			}

			if ((freightMode & FreightMode.AIR) != 0)
			{
				measures.SetQuantity(MeasureType.Chargeable, weightInKG, Core.Constants.Weight.Kilograms);
			}
			else
			{
				measures.SetQuantity(MeasureType.Chargeable, volumeInM3, "M3");
			}

			DebtorOrgs[RatingDebtorOrgTypes.LC] = billTo;
			this.paymentTermOverride = paymentTermOverride;
		}

		public override bool IsIntercompanyTariffApplicable(BillingType billingType, CostSell costSell) => false;

		public override bool ContinueWithDefaultCosting(BillingType billingType) => true;

		public Func<RatingAdaptersProvider> AdaptersProviderGetter { get; set; }

		public override PaymentTermInfos PaymentTerm { get; set; }

		public override Creditors Creditors { get; set; }

		public override bool SupportsManualRateSelection => supportsManualRateSelection ?? base.SupportsManualRateSelection;

		readonly bool? supportsManualRateSelection;

		public override Collection<IBusiness> AutoRatedFor
		{
			get
			{
				if (fAutoRatedFor == null)
				{
					var fOrg = new TestAutoRatedForObject();
					fAutoRatedFor = new Collection<IBusiness>();
					fAutoRatedFor.Add(fOrg);
				}

				return fAutoRatedFor;
			}
		}

		Collection<IBusiness> fAutoRatedFor;

		public override DebtorOrgCollection DebtorOrgs
		{
			get { return (debtorOrgs = base.DebtorOrgs ?? debtorOrgs ?? new DebtorOrgCollection()); }

			set { base.DebtorOrgs = (debtorOrgs = value); }
		}
		DebtorOrgCollection debtorOrgs;

		public OrgHeader Consignor
		{
			get { return DebtorOrgs[RatingDebtorOrgTypes.CNR]; }
			set { DebtorOrgs[RatingDebtorOrgTypes.CNR] = value; }
		}

		public OrgHeader Consignee
		{
			get { return DebtorOrgs[RatingDebtorOrgTypes.CNE]; }
			set { DebtorOrgs[RatingDebtorOrgTypes.CNE] = value; }
		}

		readonly ZString paymentTermOverride;
		readonly BusinessObjectFactory factory;

		public override ZString PaymentTermOverride => paymentTermOverride;

		public RateableMeasureSet Measures => (RateableMeasureSet)RateableMeasures;

		public override IAutoRatingChargeInfo[] GetExistingCharges(bool fromAllCompanies = false)
		{
			return overridenCharges?.ToArray() ?? base.GetExistingCharges(fromAllCompanies);
		}

		public void SetExistingCharges(IEnumerable<JobCharge> charges)
		{
			overridenCharges = charges;
		}

		IEnumerable<JobCharge> overridenCharges;

		public override ILocation GetVia(CostSell costOrSell) => viaForTest ?? base.GetVia(costOrSell);
		public void SetVia(ILocation via) => viaForTest = via;
		ILocation viaForTest;
	}

	public class AutoRatingObjectWithTariffLevel : RatingAdapter, IAutoRatingCompanyTariffLevelProvider
	{
		#region IAutoRatingCompanyTariffLevelProvider Members

		int IAutoRatingCompanyTariffLevelProvider.TariffLevel
		{
			get { return 1; }
		}

		#endregion

		#region IAutoRating Members

		public override IJobExRateCurrencyConverter CurrencyConverter =>
			fCurrencyConverter ?? (fCurrencyConverter = NonOrgSpecificExRateCurrencyConverter.Default(new BusinessObjectFactory()));

		IJobExRateCurrencyConverter fCurrencyConverter;

		public override IJobDatesProvider JobDatesProvider
		{
			get { return new JobDatesProvider<DummyBusinessObject>(new BusinessObjectFactory().NewWithValidTestData<DummyBusinessObject>()); }
		}

		public override ChargeCodeGroupCollection ChargeCodeGroups
		{
			get
			{
				if (fChargeCodeGroups == null)
				{
					fChargeCodeGroups = new ChargeCodeGroupCollection();
				}

				fChargeCodeGroups.CostChargesFilter = ChargeCodeFilter.AutorateNothing;

				return fChargeCodeGroups;
			}
		}
		ChargeCodeGroupCollection fChargeCodeGroups;

		public override MergeChargeOptions MergeCharges
		{
			get { return MergeChargeOptions.WithinAdapter; }
		}

		public override RateType RateTypeToUse
		{
			get { return RateType.Forwarding; }
		}

		public override Collection<IBusiness> AutoRatedFor
		{
			get
			{
				if (fAutoRatedFor == null)
				{
					var fOrg = new TestAutoRatedForObject();
					fAutoRatedFor = new Collection<IBusiness>();
					fAutoRatedFor.Add(fOrg);
				}

				return fAutoRatedFor;
			}
		}

		Collection<IBusiness> fAutoRatedFor;

		#endregion

		#region IAutoRatingFreightInfo Members

		public override FreightMode FreightMode
		{
			get { return FreightMode.AIR; }
		}

		public Money FreightSellRate
		{
			get { return Money.Invalid; }
		}

		public ZString SellAutoratingMode
		{
			get { return Core.Constants.FreightRateAutoratingModes.Code.StandardRate; }
		}

		public Money FreightCostRate
		{
			get { return Money.Invalid; }
		}

		public ZString CostAutoratingMode
		{
			get { return Core.Constants.FreightRateAutoratingModes.Code.StandardRate; }
		}

		public ZDateTime JobArrivalDate
		{
			get { return ZDateTime.Now; }
		}

		public ZDateTime JobDepartureDate
		{
			get { return ZDateTime.Now; }
		}

		public override ZString JobID => "JobId";

		#endregion
	}

	public class AutoRatingObjectWithWeightBreakOverride : AutoRatingObject, IAutoRatingWeightBreakOverrideProvider
	{
		public AutoRatingObjectWithWeightBreakOverride(ZString origin, ZString destination, FreightMode freightMode, ZDecimal weightInKG, ZDecimal volumeInM3)
			: base(origin, destination, freightMode, null, weightInKG, volumeInM3, billTo: null)
		{
		}

		#region IAutoRatingWeightBreakOverrideProvider Members

		decimal? IAutoRatingWeightBreakOverrideProvider.WeightBreakOverride
		{
			get { return WeightBreakOverrideForTest; }
		}

		public decimal? WeightBreakOverrideForTest;

		#endregion
	}

	public class SimpleArInfo
	{
		public SimpleArInfo() { }

		public SimpleArInfo(AutoRateInfo info)
		{
			isAutoRateInfo = true;
			InvoiceLineDesc = info.InvoiceLineDescription;
			Amount = info.Amount;
			CalculationDescription = info.Description;
			CalculationSingleLineDescription = info.SingleLineDescription;
		}

		readonly bool isAutoRateInfo;

		public ZString InvoiceLineDesc;
		public decimal Amount;
		public ZString CalculationDescription;
		public ZString CalculationSingleLineDescription;
		public bool IgnoreNewLineTabSpace; // to avoid the complexity of mismatching newLine, tab and space symbols between actual and test result.

		public static IEqualityComparer<SimpleArInfo> Comparer
		{
			get { return new LambdaComparer<SimpleArInfo>(Compare, Hash); }
		}

		static int Hash(SimpleArInfo x)
		{
			return HashCodeHelper.GetCompositeHashCode(new object[] { x.Amount, x.InvoiceLineDesc });
		}

		static bool Compare(SimpleArInfo x, SimpleArInfo y)
		{
			var ignoreNewLineTabSpace = x.IgnoreNewLineTabSpace;

			var result = x.Amount.Equals(y.Amount)
							&& Sanitize(x.InvoiceLineDesc, ignoreNewLineTabSpace).Equals(Sanitize(y.InvoiceLineDesc, ignoreNewLineTabSpace))
							&& CompareCalcLineDescriptions(x.CalculationSingleLineDescription, y.CalculationSingleLineDescription);

			if (result && (!x.CalculationDescription.IsEmpty || !y.CalculationDescription.IsEmpty))
			{
				result = x.isAutoRateInfo
							 ? y.CalculationDescription.IsEmpty || x.CalculationDescription.Contains(y.CalculationDescription)
							 : x.CalculationDescription.IsEmpty || y.CalculationDescription.Contains(x.CalculationDescription);
			}

			return result;
		}

		static ZString Sanitize(ZString text, bool ignoreNewLineTabSpace)
		{
			return ignoreNewLineTabSpace
				? text
				.Replace("\r", "")
				.Replace("\n", "")
				.Replace("\t", "")
				.Replace(" ", "")
				: text;
		}

		static bool CompareCalcLineDescriptions(string desc1, string desc2)
		{
			var strings1 = desc1.Split(new[] { "\r\n", "\n\t" }, StringSplitOptions.RemoveEmptyEntries).Where(x => !string.IsNullOrEmpty(x.Trim()));
			var strings2 = desc2.Split(new[] { "\r\n", "\n\t" }, StringSplitOptions.RemoveEmptyEntries).Where(x => !string.IsNullOrEmpty(x.Trim()));
			return !strings1.Except(strings2).Any();
		}

		public override string ToString()
		{
			var result = new ZStringBuilder();
			result.Append(ZString.Format("InvoiceLineDesc: {0}", InvoiceLineDesc));
			result.Append(ZString.Format("Amount: {0}", Amount));
			result.Append(ZString.Format("CalculationSingleLineDescription: {0}", CalculationSingleLineDescription));

			if (!CalculationDescription.IsEmpty)
			{
				result.Append("CalculationDescription:");
				result.Append(CalculationDescription);
			}

			return result.ToStringWithNewLineBetweenAppends();
		}
	}

	#endregion

	#endregion
}
