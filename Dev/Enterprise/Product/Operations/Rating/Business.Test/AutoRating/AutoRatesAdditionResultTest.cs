using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.Business.Testing
{
	public class AutoRatesAdditionResultTest : RatingTestCase
	{
		public void TestGetLogWithDeletedChargeCode()
		{
			var info1 = new AutoRateInfo(Factory);
			var info2 = new AutoRateInfo(Factory);
			var info3 = new AutoRateInfo(Factory);
			var info4 = new AutoRateInfo(Factory);
			var info5 = new AutoRateInfo(Factory);
			var info6 = new AutoRateInfo(Factory);

			var charge1 = Factory.New<JobCharge>();
			var charge2 = Factory.New<JobCharge>();
			var charge3 = Factory.New<JobCharge>();
			var charge4 = Factory.New<JobCharge>();
			var charge5 = Factory.New<JobCharge>();
			var charge6 = Factory.New<JobCharge>();

			var chargeCodeFactory = new TestHelper.ChargeCodeFactory(Factory);
			info1.ChargeCode = chargeCodeFactory["FRT"];
			info2.ChargeCode = chargeCodeFactory["BAF"];

			charge1.JR_AC = info1.ChargeCode.PK;
			charge2.JR_AC = info2.ChargeCode.PK;
			charge3.JR_AC = chargeCodeFactory["ODOC"].PK;
			charge4.JR_AC = chargeCodeFactory["DDOC"].PK;
			charge5.JR_AC = chargeCodeFactory["OCART"].PK;
			charge6.JR_AC = chargeCodeFactory["DCART"].PK;

			charge5.Delete();
			charge6.Delete();

			var createdCharges = new List<ChargeWrapper> { new ChargeWrapper(charge1, info1), new ChargeWrapper(charge2, info2) };
			var modifiedCharges = new List<ChargeWrapper> { new ChargeWrapper(charge3, info3), new ChargeWrapper(charge4, info4) };
			var deletedChargesCount = new List<ChargeWrapper> { new ChargeWrapper(charge5, info5), new ChargeWrapper(charge6, info6) }.Count;
			var autoRates = new List<AutoRateInfo> { info1, info2 };

			var result = new AutoRatesAdditionResult(createdCharges, modifiedCharges, deletedChargesCount, autoRates, CostSell.Revenue);

			var expected = @"The following rates were found:
  • BAF charge
  • FRT charge
Charges created: BAF, FRT
Charges modified: DDOC, ODOC
Charges deleted: 2 Deleted Charges";

			AssertMultilineASCIIEquals("", expected, result.GetLog());
		}

		public void TestGetLogWithDeletedChargeCode_ShouldNotThrowException()
		{
			var info1 = new AutoRateInfo(Factory);
			var info2 = new AutoRateInfo(Factory);
			var autoRates = new List<AutoRateInfo> { info1, info2 };

			var chargeCodeFactory = new TestHelper.ChargeCodeFactory(Factory);
			info1.ChargeCode = chargeCodeFactory["FRT"];
			info2.ChargeCode = chargeCodeFactory["BAF"];

			var charge1 = Factory.New<JobCharge>();
			charge1.JR_AC = info1.ChargeCode.PK;
			var charge2 = Factory.New<JobCharge>();
			charge2.JR_AC = info2.ChargeCode.PK;

			var createdCharges = new List<ChargeWrapper> { new ChargeWrapper(charge1, info1) };
			var modifiedCharges = new List<ChargeWrapper>();
			var result = new AutoRatesAdditionResult(createdCharges, modifiedCharges, 0, autoRates, CostSell.Revenue);
			charge1.Delete();
			result.GetLog();
			AssertEquals(0, ErrorReporter.TotalErrorCount);

			createdCharges = new List<ChargeWrapper>();
			modifiedCharges = new List<ChargeWrapper> { new ChargeWrapper(charge2, info2) };
			result = new AutoRatesAdditionResult(createdCharges, modifiedCharges, 0, autoRates, CostSell.Revenue);
			charge2.Delete();
			result.GetLog();
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TestDeletedChargeCode_ShouldNotThrowException()
		{
			var info1 = new AutoRateInfo(Factory);
			var info2 = new AutoRateInfo(Factory);
			var autoRates = new List<AutoRateInfo> { info1, info2 };

			var chargeCodeFactory = new TestHelper.ChargeCodeFactory(Factory);
			info1.ChargeCode = chargeCodeFactory["FRT"];
			info2.ChargeCode = chargeCodeFactory["BAF"];

			var charge1 = Factory.New<JobCharge>();
			charge1.JR_AC = info1.ChargeCode.PK;
			var charge2 = Factory.New<JobCharge>();
			charge2.JR_AC = info2.ChargeCode.PK;

			var result = new AutoRatesAdditionResult
			(
				createdCharges: new List<ChargeWrapper> { new ChargeWrapper(charge1, info1) },
				modifiedCharges: new List<ChargeWrapper> { new ChargeWrapper(charge2, info2) },
				0,
				autoRates,
				CostSell.Revenue
			);
			charge1.Delete();
			charge2.Delete();
			AssertNullOrEmpty("Precondition", ErrorReporter.LastMessageReported);

			CombineAssertions("Created and modified charges should not have error", () =>
			{
				var createdChargesChargeCode = result.CreatedCharges.Select(x => x.ChargeCode).ToArray();
				var modifiedChargesChargeCode = result.ModifiedCharges.Select(x => x.ChargeCode).ToArray();
				AssertNullOrEmpty("Created/modified charges should have no error", ErrorReporter.LastMessageReported);
			});
		}

		public void TestGetLog_PopulateChargesWithSource()
		{
			var frtChargeCode = Helper.ChargeCodes["FRT"];
			var bafChargeCode = Helper.ChargeCodes["BAF"];
			var fumChargeCode = Helper.ChargeCodes["FUM"];
			var warChargeCode = Helper.ChargeCodes["WAR"];
			var fulChargeCode = Helper.ChargeCodes["FUL"];
			var cusDSBChargeCode = RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value;

			var rateEntryCosting = Helper.NewCosting(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.AIR);
			var costingLine = rateEntryCosting.AddRateLine(frtChargeCode.AC_Code, UnitCalculator.Code, QuantityUnit.KG);

			var spotRatingHeader = Helper.NewCosting(Helper.NewOrgHeader());
			var spotRateEntry = spotRatingHeader.AddRateEntry(RatingConstants.RateCategory.AIR);
			spotRateEntry.RateLines.RemoveAndDeleteAll();
			spotRateEntry.IsSpotEntry = true;
			var spotRateLine = spotRateEntry.AddRateLine(bafChargeCode.AC_Code, UnitCalculator.Code, QuantityUnit.KG);
			spotRateLine.SpotRateDescription = "Negotiated Cost";

			var spotJobServiceHeader = Factory.New<ClientRate>();
			var spotJobServiceRateEntry = spotJobServiceHeader.AddRateEntry(RatingConstants.RateCategory.AIR);
			spotJobServiceRateEntry.RateLines.RemoveAndDeleteAll();
			spotJobServiceRateEntry.IsSpotEntry = true; //SpotRateEntryCreator sets this to true
			spotJobServiceRateEntry.JobServiceForSpotEntry = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Origin, "FUM", "Fumigate", 1m);
			var spotJobServiceRateLine = spotJobServiceRateEntry.AddRateLine(fumChargeCode.AC_Code, UnitCalculator.Code, QuantityUnit.KG);
			spotJobServiceRateLine.SpotRateDescription = RatingDataRegistry.Instance.OriginDemurrageServiceChargeCode.Caption;

			var criteria = new TestRatingCriteria();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			var calculationResultFRT = new CalculationResult(costingLine, new CalculatorOutput(new List<PaymentBasis> { criteria.CreatePaymentBasis(RateInfo.CreateFLT(0, "AUD"), new Quantity(100m, "KG")) }));
			var calculationResultBAF = new CalculationResult(spotRateLine, new CalculatorOutput(new List<PaymentBasis> { criteria.CreatePaymentBasis(RateInfo.CreateFLT(0, "AUD"), new Quantity(50m, "KG")) }));
			var calculationResultFUM = new CalculationResult(spotJobServiceRateLine, new CalculatorOutput(new List<PaymentBasis> { criteria.CreatePaymentBasis(RateInfo.CreateFLT(0, "AUD"), new Quantity(30m, "KG")) }));

			var rateInfoFRT = new AutoRateInfo(calculationResultFRT, parameters, Factory);
			var rateInfoBAF = new AutoRateInfo(calculationResultBAF, parameters, Factory);
			var rateInfoFUM = new AutoRateInfo(calculationResultFUM, parameters, Factory);
			var rateInfoWAR = new AutoRateInfo(Factory) { ChargeCode = warChargeCode };
			var rateInfoFUL = new AutoRateInfo(Factory)
			{
				RateSource = "Rates Service Costing",
				ChargeCode = fulChargeCode
			};
			var rateInfoCUSDSB = new AutoRateInfo(Factory)
			{
				ChargeCode = Factory.Load<AccChargeCode>(cusDSBChargeCode),
				RateSource = "Customs Response message"
			};

			var chargeFRT = Factory.New<JobCharge>();
			var chargeBAF = Factory.New<JobCharge>();
			var chargeFUM = Factory.New<JobCharge>();
			var chargeWAR = Factory.New<JobCharge>();
			var chargeFUL = Factory.New<JobCharge>();
			var chargeCUSDSB = Factory.New<JobCharge>();

			chargeFRT.JR_AC = frtChargeCode.PK;
			chargeBAF.JR_AC = bafChargeCode.PK;
			chargeFUM.JR_AC = fumChargeCode.PK;
			chargeWAR.JR_AC = warChargeCode.PK;
			chargeFUL.JR_AC = fulChargeCode.PK;
			chargeCUSDSB.JR_AC = cusDSBChargeCode;

			var createdCharges = new List<ChargeWrapper> { new ChargeWrapper(chargeFRT, rateInfoFRT), new ChargeWrapper(chargeBAF, rateInfoBAF), new ChargeWrapper(chargeFUM, rateInfoFUM), new ChargeWrapper(chargeWAR, rateInfoWAR), new ChargeWrapper(chargeCUSDSB, rateInfoCUSDSB) };
			var autoRates = new List<AutoRateInfo> { rateInfoFRT, rateInfoBAF, rateInfoFUM, rateInfoWAR, rateInfoCUSDSB };
			var result = new AutoRatesAdditionResult(createdCharges, new List<ChargeWrapper>(), 0, autoRates, CostSell.Revenue);

			var expected = @"The following rates were found:
  • BAF charge from Job Negotiated Cost
  • CUSDSB charge from Customs Response message
  • FRT charge from Costing TESTORG1
  • FUM charge from Job Origin Truck Wait Time
  • WAR charge
Charges created: BAF, CUSDSB, FRT, FUM, WAR";

			AssertMultilineASCIIEquals("Log should contain correct results", expected, result.GetLog());
		}
	}
}
