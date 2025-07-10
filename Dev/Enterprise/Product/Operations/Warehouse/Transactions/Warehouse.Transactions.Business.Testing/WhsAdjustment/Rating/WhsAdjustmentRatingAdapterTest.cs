using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsAdjustmentRatingAdapterTest : WhsDocketRatingAdapterTest<WhsAdjustment>
	{
		#region IAutoRating / IJobInvoicingPlugIn Members

		protected override void TestIAutoRatingConsumerTypeCore()
		{
			var jobInvPlugIn = GetIAutoRating(GetNewDocket());
			AssertEquals(JobInvoicingConsumerTypes.WarehouseStorage, jobInvPlugIn.InvoicingSupporter.ConsumerType);
		}

		protected override Dictionary<string, int> ExpectedHitsForPickupAddressCore => new Dictionary<string, int>();

		public override void TestIAutoRatingPopulateChargeCodeGroups()
		{
			var iAdjustment = GetIAutoRating(GetNewDocket());
			AssertEquals(2, iAdjustment.ChargeCodeGroups.Count);
			AssertCollectionContains(ChargeCodeGroupList.Codes.WHSInwards, iAdjustment.ChargeCodeGroups);
			AssertCollectionContains(ChargeCodeGroupList.Codes.WHSOutwards, iAdjustment.ChargeCodeGroups);
		}

		public override void TestIAutoRatingAdapterTypeAndID()
		{
			var docket = GetNewDocket();
			var adapter = GetIAutoRating(docket);
			AssertEquals(AdapterType.WarehouseAdjustment, adapter.AdapterType);
			AssertEquals(docket.WD_DocketID, adapter.OperationalJobCode);
		}

		protected override void TestIAutoRatingFreightInfo_MeasuresCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;

			data.Part2.OP_StockKeepingUnit = Constants.PkgUnit.Carton;

			Helper.SetProductWeightAndVolume(data.Part1, 2m, "KG", 3m, "M3");
			Helper.SetProductWeightAndVolume(data.Part2, 10m, "KG", 0.01m, "M3");
			data.Part1.OP_RH_NKCommodityCode = "HAZ";
			data.Part2.OP_RH_NKCommodityCode = "HAZ";

			data.Part2.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part2, Constants.PkgUnit.Carton, Constants.PkgUnit.Pallet, 10m);
			Helper.CreateProductUnit(data.Part2, Constants.PkgUnit.Unit, Constants.PkgUnit.Carton, 5m);

			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10, data.Whs1.DefaultLocation);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part2, 20, data.Whs1.DefaultLocation);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 30, data.Whs1.DefaultLocation);
			adjustment.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(adjustment);

			var measures = (RateableMeasureSet)(GetIAutoRating(adjustment)).RateableMeasures;

			AssertEquals(4, measures.MeasureTypeCount);
			foreach (MeasureType measure in new[] { MeasureType.StorageUnit, MeasureType.StorageVolume, MeasureType.StorageWeight })
			{
				Assert(measures.MeasureHasWarehouse(measure));
				Assert(measures.MeasureHasDocketReference(measure));
				Assert(measures.MeasureHasProduct(measure));
				Assert(measures.MeasureHasCommodity(measure));
				Assert(measures.MeasureHasChargeGroupToUse(measure));
			}

			AssertNotNull(measures.Time);

			AssertEquals("Line1=10 + Line3=30 + Line2=20", 60m, measures.GetActual(MeasureType.StorageUnit));
			AssertEquals("40 Units", 40m, measures.UnitsByProduct_ForTest(MeasureType.StorageUnit, data.Part1.PK));
			AssertEquals("80 KG", 80m, measures.UnitsByProduct_ForTest(MeasureType.StorageWeight, data.Part1.PK));
			AssertEquals("120 M3", 120m, measures.UnitsByProduct_ForTest(MeasureType.StorageVolume, data.Part1.PK));
			AssertEquals("20 Cartons", 20m, measures.UnitsByProduct_ForTest(MeasureType.StorageUnit, data.Part2.PK));
			AssertEquals("200 KG", 200m, measures.UnitsByProduct_ForTest(MeasureType.StorageWeight, data.Part2.PK));
			AssertEquals("0.2 M3", 0.2m, measures.UnitsByProduct_ForTest(MeasureType.StorageVolume, data.Part2.PK));

			// Check by commodity
			AssertEquals("60 Units", 60m, measures.UnitsByCommodity_ForTest(MeasureType.StorageUnit, "HAZ"));
			AssertEquals("280 KG", 280m, measures.UnitsByCommodity_ForTest(MeasureType.StorageWeight, "HAZ"));
			AssertEquals("120.2 M3", 120.2m, measures.UnitsByCommodity_ForTest(MeasureType.StorageVolume, "HAZ"));
		}

		protected override Dictionary<string, int> ExpectedDbHitsForSplitPeriodBillingMeasuresCore => new Dictionary<string, int>
		{
			{  OrgCompanyDataSchema.Constants.TableName, 1 },
		};

		public new void TestIAutoRatingFreightInfo_Measures_Volume()
		{
			Assert("Already tested TestIAutoRatingFreightInfo_MeasuresCore", true);
		}

		public new void TestIAutoRatingFreightInfo_Measures_Weight()
		{
			Assert("Already tested TestIAutoRatingFreightInfo_MeasuresCore", true);
		}

		#region TestIAutoRatingFreightInfo_Measures_UnitInvalid

		public override void TestIAutoRatingFreightInfo_Measures_WhenWeightUnitIsInvalid()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewDocket();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;

			data.Part1.OP_WeightUQ = "0";
			data.Part1.OP_StockKeepingUnit = "0";
			var line1 = docket.Lines.AddNew();
			line1.WE_OP = data.Part1.PK;
			line1.Validation.ValidateAll();

			SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(line1, 5m);
			docket.WD_FinalisedDate = ZDateTimeOffset.Now;

			var expectedErrorStr =
@"Invalid Weight Unit in this Product Code: P1. Please use following valid unit types:
DT, G, HG, KG, KT, LB, LT, MC, MG, OT, OZ, T, TL, TN";
			var rateableMeasures = (RateableMeasureSet)GetIAutoRating(docket).RateableMeasures;
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.StorageWeight));
			AssertEquals(expectedErrorStr, rateableMeasures.GetMeasureErrors(MeasureType.StorageWeight).Single());

			data.Part1.OP_WeightUQ = "0";
			data.Part1.OP_StockKeepingUnit = Constants.Weight.Kilograms;
			rateableMeasures = (RateableMeasureSet)GetIAutoRating(docket).RateableMeasures;
			AssertEquals(5m, rateableMeasures.GetActual(MeasureType.StorageWeight));
			AssertEquals(Constants.Weight.Kilograms, rateableMeasures.GetUnit(MeasureType.StorageWeight));
			Assert(!rateableMeasures.GetMeasureErrors(MeasureType.StorageWeight).Any());
		}

		public override void TestIAutoRatingFreightInfo_Measures_WhenVolumeUnitIsInvalid()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewDocket();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;

			data.Part1.OP_CubicUQ = "0";
			data.Part1.OP_StockKeepingUnit = "0";
			var line1 = docket.Lines.AddNew();
			line1.WE_OP = data.Part1.PK;
			line1.Validation.ValidateAll();

			SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(line1, 5m);
			docket.WD_FinalisedDate = ZDateTimeOffset.Now;

			var expectedErrorStr =
@"Invalid Volume Unit in this Product Code: P1. Please use following valid unit types:
CC, CF, CI, CY, D3, GA, GI, L, M3, ML, TE";
			var rateableMeasures = (RateableMeasureSet)GetIAutoRating(docket).RateableMeasures;
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.StorageVolume));
			AssertEquals(expectedErrorStr, rateableMeasures.GetMeasureErrors(MeasureType.StorageVolume).Single());

			data.Part1.OP_CubicUQ = "0";
			data.Part1.OP_StockKeepingUnit = Constants.Volume.CubicMetres;

			rateableMeasures = (RateableMeasureSet)GetIAutoRating(docket).RateableMeasures;
			AssertEquals(5m, rateableMeasures.GetActual(MeasureType.StorageVolume));

			data.Part1.OP_CubicUQ = Constants.Volume.CubicMetres;
			data.Part1.OP_StockKeepingUnit = Constants.Volume.CubicMetres;

			rateableMeasures = (RateableMeasureSet)GetIAutoRating(docket).RateableMeasures;
			AssertEquals(5m, rateableMeasures.GetActual(MeasureType.StorageVolume));
			AssertEquals(Constants.Volume.CubicMetres, rateableMeasures.GetUnit(MeasureType.StorageVolume));
			Assert(!rateableMeasures.GetMeasureErrors(MeasureType.StorageVolume).Any());
		}

		public override void TestIAutoRatingFreightInfo_Measures_WhenMultiFieldsAreInvalid()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewDocket();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;

			data.Part1.OP_WeightUQ = "0";
			data.Part1.OP_CubicUQ = "0";
			data.Part1.OP_StockKeepingUnit = "0";
			var line1 = docket.Lines.AddNew();
			line1.WE_OP = data.Part1.PK;
			line1.Validation.ValidateAll();

			SetLineUnitsMetForTestIAutoRatingFreightInfoMeasures(line1, 5m);
			docket.WD_FinalisedDate = ZDateTimeOffset.Now;

			var expectedVolumeErrorStr =
@"Invalid Volume Unit in this Product Code: P1. Please use following valid unit types:
CC, CF, CI, CY, D3, GA, GI, L, M3, ML, TE";
			var expectedWeightErrorStr =
@"Invalid Weight Unit in this Product Code: P1. Please use following valid unit types:
DT, G, HG, KG, KT, LB, LT, MC, MG, OT, OZ, T, TL, TN";
			var rateableMeasures = (RateableMeasureSet)GetIAutoRating(docket).RateableMeasures;
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.StorageWeight));
			AssertEquals(0m, rateableMeasures.GetActual(MeasureType.StorageVolume));
			AssertEquals(expectedVolumeErrorStr, rateableMeasures.GetMeasureErrors(MeasureType.StorageVolume).Single());
			AssertEquals(expectedWeightErrorStr, rateableMeasures.GetMeasureErrors(MeasureType.StorageWeight).Single());
		}

		#endregion

		public new void TestIAutoRatingFreightInfo_Measures_WhenWE_OPNotSet()
		{
			Assert("Can't test this as adjustment cannot be finalised if some lines has no product. Only finalised adjusments are rated.", true);
		}

		public new void TestIAutoRatingFreightInfo_Measures_WithProductAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, 10, data.Whs1.DefaultLocation.ToLocationString(), "11", "22", "33", "", ZDate.Empty, ZDate.Empty);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, 20, data.Whs1.DefaultLocation.ToLocationString(), "44", "55", "66", "", ZDate.Empty, ZDate.Empty);

			adjustment.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(adjustment);

			var rateableMeasures = (RateableMeasureSet)GetIAutoRating(adjustment).RateableMeasures;
			AssertEquals(30m, rateableMeasures.GetActual(MeasureType.StorageUnit));
			AssertEquals("Attributes are ignored so there should be only 1 point", 1, rateableMeasures.GetPartCount(MeasureType.StorageUnit));
		}

		protected override void TestIAutoRatingFreightInfo_Measures_WithProductAttributes_SerialNumberIsKeyCore(bool isFactorySaved)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, 1m, data.Whs1.DefaultLocation.ToLocationString(), "11", "22", "33", "SN1", ZDate.Empty, ZDate.Empty);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, 1m, data.Whs1.DefaultLocation.ToLocationString(), "44", "55", "66", "SN2", ZDate.Empty, ZDate.Empty);

			adjustment.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(adjustment);

			if (isFactorySaved)
			{
				Factory.Save();
			}

			var rateableMeasures = (RateableMeasureSet)GetIAutoRating(adjustment).RateableMeasures;
			AssertEquals(2m, rateableMeasures.GetActual(MeasureType.StorageUnit));
			AssertEquals("Attributes are ignored so there should be only 1 point", 1, rateableMeasures.GetPartCount(MeasureType.StorageUnit));
		}

		protected override void TestIAutoRatingFreightInfo_Measures_SplitPeriodBillingCore()
		{
			Assert("Already tested TestIAutoRatingFreightInfo_MeasuresCore", true);
		}

		public new void TestIAutoRatingFreightInfo_Containers()
		{
			// there are no measures for containers in Adjustments
			Assert("Already tested TestIAutoRatingFreightInfo_MeasuresCore", true);
		}

		#endregion

		#region TestMonthSplitBilling_AdjustmentInOutPartialCharge

		public void TestMonthSplitBilling_AdjustmentInOutPartialCharge()
		{
			var oneMonthAgoDate = ZDateTime.Now.AddMonths(-1);
			var twoMonthAgoDate = ZDateTime.Now.AddMonths(-2);

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSOutwards;
			chargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;
			chargeCode.AC_RateCalculator = "SMB";

			//    +100 (incoming)
			//     |   +5 (ADJ)                        |
			//-----|----┴---┬--------------------------|
			//     |       -8 (ADJ)                    |
			// 2   |               1                   |
			// Mth |               Mth                 |
			// ago |               ago                 |

			// We must charge adjustment out storage only for 3 units in this case, because 5 units were charged in adjustment-in

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(twoMonthAgoDate.Year, twoMonthAgoDate.Month, 2), data.Part1, 100m, data.Whs1.DefaultLocation, "");
			var adjustmentIn = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ_IN");
			Helper.CreateWhsAdjustmentLine(adjustmentIn, data.Part1, 5m, data.Whs1.DefaultLocation);
			adjustmentIn.FinaliseDocketWithoutUserConfirmation();
			adjustmentIn.WD_FinalisedDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 5);

			Factory.Save();

			var adjustmentOut = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ_OUT");
			Helper.CreateWhsAdjustmentLine(adjustmentOut, data.Part1, -8m, data.Whs1.DefaultLocation);
			adjustmentOut.FinaliseDocketWithoutUserConfirmation();
			adjustmentOut.WD_FinalisedDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 8);

			var iAdjustmentIn = GetIAutoRating(adjustmentIn);
			var rateableMeasures = (RateableMeasureSet)iAdjustmentIn.RateableMeasures;
			AssertEquals("Charged storage units must equal 5", 5m, rateableMeasures.GetActual(MeasureType.StorageUnit));
			AssertEquals(false, rateableMeasures.HasMeasureType(MeasureType.Unit));

			var iAdjustmentOut = GetIAutoRating(adjustmentOut);
			rateableMeasures = (RateableMeasureSet)iAdjustmentOut.RateableMeasures;
			AssertEquals("Charged storage units must equal 8-5 = 3", 3m, rateableMeasures.GetActual(MeasureType.StorageUnit));
			AssertEquals(false, rateableMeasures.HasMeasureType(MeasureType.Unit));
		}

		#endregion

		#region TestMonthSplitBilling_AdjustmentOutFullCharge

		public void TestMonthSplitBilling_AdjustmentOutFullCharge()
		{
			var oneMonthAgoDate = ZDateTime.Now.AddMonths(-1);
			var twoMonthAgoDate = ZDateTime.Now.AddMonths(-2);

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSOutwards;
			chargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;
			chargeCode.AC_RateCalculator = "SMB";

			//    +100 (incoming)
			//     |        +5 (ADJ_IN)               |
			//-----|----┬---┴-------------------------|
			//     |   -8 (ADJ_OUT)                   |
			// 2   |               1                  |
			// Mth |               Mth                |
			// ago |               ago                |

			// We must charge order storage for all 8 units in this case, because there are no receives (or adjustments in) before adjustment out in this month

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(twoMonthAgoDate.Year, twoMonthAgoDate.Month, 2), data.Part1, 100m);
			var adjustmentIn = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ_IN");
			Helper.CreateWhsAdjustmentLine(adjustmentIn, data.Part1, 5m, data.Whs1.DefaultLocation);
			adjustmentIn.FinaliseDocketWithoutUserConfirmation();
			adjustmentIn.WD_FinalisedDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 8);

			Factory.Save();
			var adjustmentOut = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ_OUT");
			Helper.CreateWhsAdjustmentLine(adjustmentOut, data.Part1, -8m, data.Whs1.DefaultLocation);
			adjustmentOut.FinaliseDocketWithoutUserConfirmation();
			adjustmentOut.WD_FinalisedDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 2);

			var iAdjustmentOut = GetIAutoRating(adjustmentOut);

			var rateableMeasures = (RateableMeasureSet)iAdjustmentOut.RateableMeasures;
			AssertEquals("Charged storage units must equal 8", 8m, rateableMeasures.GetActual(MeasureType.StorageUnit));
			AssertEquals(false, rateableMeasures.HasMeasureType(MeasureType.Unit));
		}

		#endregion

		#region TestMonthSplitBilling_AdjustmentsOutComplexCase

		public void TestMonthSplitBilling_AdjustmentsOutComplexCase()
		{
			var oneMonthAgoDate = ZDateTime.Now.AddMonths(-1);
			var twoMonthAgoDate = ZDateTime.Now.AddMonths(-2);

			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSOutwards;
			chargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;
			chargeCode.AC_RateCalculator = "SMB";

			// here comes complex case

			//    +100 (incoming)
			//     |   +5 +2   +7      (REC)            |
			//-----|----┴--┴-┬--┴--┬--------------------|
			//     |        -8    -9  (ADJ OUT)         |
			// 2   |                1                   |
			// Mth |                Mth                 |
			// ago |                ago                 |

			// First order should be charged for 8-2-5= 1 unit
			// Second order should be charged for 9-7 = 2 units. We ignore previous docket lines, because previous order used all stock from receives before it.

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(twoMonthAgoDate.Year, twoMonthAgoDate.Month, 2), data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 2), data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 4), data.Part1, 2m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 8), data.Part1, 7m);
			Factory.Save();

			var adjustmentOut1 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ_OUT1");
			Helper.CreateWhsAdjustmentLine(adjustmentOut1, data.Part1, -8m, data.Whs1.DefaultLocation);
			adjustmentOut1.FinaliseDocketWithoutUserConfirmation();
			adjustmentOut1.WD_FinalisedDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 6);

			var adjustmentOut2 = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ_OUT2");
			Helper.CreateWhsAdjustmentLine(adjustmentOut2, data.Part1, -9m, data.Whs1.DefaultLocation);
			adjustmentOut2.FinaliseDocketWithoutUserConfirmation();
			adjustmentOut2.WD_FinalisedDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 10);

			Factory.Save();

			var iAdjustmentOut1 = GetIAutoRating(adjustmentOut1);
			var rateableMeasures = (RateableMeasureSet)iAdjustmentOut1.RateableMeasures;
			AssertEquals("Charged storage units for order1 must be 8-2-5 = 1", 1m, rateableMeasures.GetActual(MeasureType.StorageUnit));
			AssertEquals(false, rateableMeasures.HasMeasureType(MeasureType.Unit));

			var iAdjustmentOut2 = GetIAutoRating(adjustmentOut2);
			rateableMeasures = (RateableMeasureSet)iAdjustmentOut2.RateableMeasures;
			AssertEquals("Charged storage units for order1 must be 9-7 = 2", 2m, rateableMeasures.GetActual(MeasureType.StorageUnit));
			AssertEquals(false, rateableMeasures.HasMeasureType(MeasureType.Unit));
		}

		#endregion

		#region TestSplitBillingNoChargeForUnfinalisedAdjustment

		public void TestSplitBillingNoChargeForUnfinalisedAdjustment()
		{
			var oneMonthAgoDate = ZDateTime.Now.AddMonths(-1);
			var twoMonthAgoDate = ZDateTime.Now.AddMonths(-2);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = false;

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSOutwards;
			chargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;
			chargeCode.AC_RateCalculator = "SMB";

			//    +100 (incoming)
			//     |   +5 (ADJ IN)                     |
			//-----|----┴---┬--------------------------|
			//     |       -8 (ADJ OUT)                |
			// 2   |               1                   |
			// Mth |               Mth                 |
			// ago |               ago                 |

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(twoMonthAgoDate.Year, twoMonthAgoDate.Month, 2), data.Part1, 100m);

			var adjustmentin = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ_IN");
			Helper.CreateWhsAdjustmentLine(adjustmentin, data.Part1, 5m, data.Whs1.DefaultLocation);

			var adjustmentOut = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ_IN");
			Helper.CreateWhsAdjustmentLine(adjustmentOut, data.Part1, -8m, data.Whs1.DefaultLocation);
			adjustmentOut.RunPreSaveValidation();

			Factory.Save();
			// no finalisation!

			var iAdjustmentIn = GetIAutoRating(adjustmentin);
			var iAdjustmentOut = GetIAutoRating(adjustmentOut);

			AssertEquals("Charged Storage units must equal 0, because adjustment is not finalised", 0m, ((RateableMeasureSet)iAdjustmentIn.RateableMeasures).GetActual(MeasureType.StorageUnit));
			AssertEquals("Charged Storage units must equal 0, because adjustment is not finalised", 0m, ((RateableMeasureSet)iAdjustmentOut.RateableMeasures).GetActual(MeasureType.StorageUnit));

			adjustmentin.FinaliseDocketWithoutUserConfirmation();
			adjustmentOut.FinaliseDocketWithoutUserConfirmation();
			adjustmentin.WD_FinalisedDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 3);
			adjustmentOut.WD_FinalisedDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 10);
			Factory.Save();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(adjustmentin);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(adjustmentOut);

			iAdjustmentIn = GetIAutoRating(adjustmentin);
			iAdjustmentOut = GetIAutoRating(adjustmentOut);

			AssertEquals("Charged Storage units must not equal 0, because adjustment is finalised", 5m, ((RateableMeasureSet)iAdjustmentIn.RateableMeasures).GetActual(MeasureType.StorageUnit));
			AssertEquals("Charged Storage units must not equal 0, because adjustment is finalised", 3m, ((RateableMeasureSet)iAdjustmentOut.RateableMeasures).GetActual(MeasureType.StorageUnit));
		}

		#endregion

		#region TestMonthSplitBilling_ChargeStorageInAdvanceCancelsAdjustmentsStorageOutCharges

		public void TestMonthSplitBilling_ChargeStorageInAdvanceCancelsAdjustmentsStorageOutCharges()
		{
			var oneMonthAgoDate = ZDateTime.Now.AddMonths(-1);
			var twoMonthAgoDate = ZDateTime.Now.AddMonths(-2);
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;
			// Setting this flag to true should cancel storage out charges calculation for adjustments
			data.Org1.CompanyData.OB_WhsChargeStorageInAdvance = true;

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSOutwards;
			chargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;
			chargeCode.AC_RateCalculator = "SMB";

			//    +100 (incoming)
			//     |   +5 (ADJ)                        |
			//-----|----┴---┬--------------------------|
			//     |       -8 (ADJ)                    |
			// 2   |               1                   |
			// Mth |               Mth                 |
			// ago |               ago                 |

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(twoMonthAgoDate.Year, twoMonthAgoDate.Month, 2), data.Part1, 100m, data.Whs1.DefaultLocation, "");
			var adjustmentIn = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ_IN");
			Helper.CreateWhsAdjustmentLine(adjustmentIn, data.Part1, 5m, data.Whs1.DefaultLocation);
			adjustmentIn.FinaliseDocketWithoutUserConfirmation();
			adjustmentIn.WD_FinalisedDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 5);

			Factory.Save();

			var adjustmentOut = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ_OUT");
			Helper.CreateWhsAdjustmentLine(adjustmentOut, data.Part1, -8m, data.Whs1.DefaultLocation);
			adjustmentOut.FinaliseDocketWithoutUserConfirmation();
			adjustmentOut.WD_FinalisedDate = new ZDateTimeOffset(oneMonthAgoDate.Year, oneMonthAgoDate.Month, 8);

			var iAdjustmentIn = GetIAutoRating(adjustmentIn);
			var rateableMeasures = (RateableMeasureSet)iAdjustmentIn.RateableMeasures;
			AssertEquals("Charged storage units must equal 5 as receive storage charges are unaffected by OB_WhsChargeStorageInAdvance flag", 5m, rateableMeasures.GetActual(MeasureType.StorageUnit));
			AssertEquals(false, rateableMeasures.HasMeasureType(MeasureType.Unit));

			var iAdjustmentOut = GetIAutoRating(adjustmentOut);
			rateableMeasures = (RateableMeasureSet)iAdjustmentOut.RateableMeasures;
			AssertEquals("Charged storage units must equal 0", 0m, rateableMeasures.GetActual(MeasureType.StorageUnit));
			AssertEquals(false, rateableMeasures.HasMeasureType(MeasureType.Unit));
		}

		#endregion

		#region Implementation

		protected override FinalisableDocketHelper<WhsAdjustment> GetFinalisableHelper() => new FinalisableAdjustmentHelper(Factory);

		protected override WhsDocket GetNewDocket()
		{
			return Factory.New<WhsAdjustment>();
		}

		protected override IAutoRating GetIAutoRating(WhsDocket docket)
		{
			return new WhsAdjustmentRatingAdapter((WhsAdjustment)docket);
		}

		#endregion
	}
}
