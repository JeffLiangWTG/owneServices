using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class MinimumCalculatorTest : CalculatorTest
	{
		public void TestIsJobLevelAvailable()
		{
			AssertEquals(false, TestCalculator.IsJobLevelAvailable);
		}

		public void TestRequiresWeightVolume()
		{
			AssertEquals(false, TestCalculator.Line.RequiresWeightVolume());
		}

		public override void TestCheckOrCreateItems()
		{
			AssertNull(Line.RateLineItems.FindByTM_Type(MinimumCalculator.Items.MIN));
			AssertNull(Line.RateLineItems.FindByTM_Type(MinimumCalculator.Items.MinimumType));

			InitialiseTestCalculator();

			AssertNotNull(Line.RateLineItems.FindByTM_Type(MinimumCalculator.Items.MIN));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(MinimumCalculator.Items.MinimumType));

			AssertEquals(2, Line.RateLineItems.Count);

			AssertEquals(Line.RateLineItems.FindByTM_Type(MinimumCalculator.Items.MIN).TM_RelevantValueInfo, TestCalculator.Decimal1Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(MinimumCalculator.Items.MinimumType).TM_TextInfo, TestCalculator.String1Info);

			AssertEquals(TestCalculator.IsJobMinimumInfo, TestCalculator.Bool1Info);
			AssertEquals(TestCalculator.IsChargeCodeMinimumInfo, TestCalculator.Bool2Info);
		}

		public override void TestMapping()
		{
			TestMapping(MinimumCalculator.Items.MIN, "Decimal1");
			TestMapping(MinimumCalculator.Items.MinimumType, "String1");
		}

		public override void TestDocLineAmount()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var rateEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA);

			var rateLine10 = AddMinRateLine(rateEntry, "FRT", "AUD", CalculatorConstants.Text.MIN_ChargeCode, 10m);
			var rateLine11 = AddMinRateLine(rateEntry, "FRT", "AUD", CalculatorConstants.Text.MIN_ChargeCode, 11m);

			var rateLine20 = AddMinRateLine(rateEntry, "FRT", "USD", CalculatorConstants.Text.MIN_ChargeCode, 20m);

			var rateLine30 = AddMinRateLine(rateEntry, "FRT", "AUD", CalculatorConstants.Text.MIN_Job, 30m);
			var rateLine31 = AddMinRateLine(rateEntry, "FRT", "AUD", CalculatorConstants.Text.MIN_Job, 31m);

			var rateLine40 = AddMinRateLine(rateEntry, "FRT", "NZD", CalculatorConstants.Text.MIN_Job, 0m);
			var rateLine41 = AddMinRateLine(rateEntry, "FRT", "NZD", CalculatorConstants.Text.MIN_Job, 40m);

			var docLineAmount = rateLine10.Calculator.GetDocLineAmount()
				+ rateLine11.Calculator.GetDocLineAmount()
				+ rateLine20.Calculator.GetDocLineAmount()
				+ rateLine30.Calculator.GetDocLineAmount()
				+ rateLine31.Calculator.GetDocLineAmount()
				+ rateLine40.Calculator.GetDocLineAmount()
				+ rateLine41.Calculator.GetDocLineAmount();

			AssertQuotationLineList
			(
				docLineAmount,
				new[]
				{
					"Charge Code Minimum|AUD|10.00|",
					"Charge Code Minimum|USD|20.00|",
					"Job Minimum|AUD|30.00|",
					"Job Minimum|NZD|40.00|",
				}
			);
		}

		static RateLine AddMinRateLine(RateEntry rateEntry, ZString chargeCode, string currency, string minType, ZDecimal amount)
		{
			var rateLine = rateEntry.AddRateLine(chargeCode, MinimumCalculator.Code, currencyCode: currency);
			rateLine.Calculator[MinimumCalculator.Items.MinimumType] = (ZString)minType;
			rateLine.Calculator[MinimumCalculator.Items.MIN] = amount;
			return rateLine;
		}

		public override void TestQuotationLines()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "", "");

			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "USD";

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate - Job Minimum||Not Charged|", quotationLines[0].ToString());

			TestCalculator.MinimumValue = 200.5m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate - Job Minimum|USD|200.50|", quotationLines[0].ToString());

			TestCalculator.IsChargeCodeMinimum = true;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate - Charge Code Minimum|USD|200.50|", quotationLines[0].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Charge - Charge Code Minimum|USD|200.50|", quotationLines[0].ToString());

			var consignee = Helper.NewOrgHeader();
			var consignor = Helper.NewOrgHeader();
			Line.Parent.TI_OH_Consignee = consignee.PK;
			Line.Parent.TI_OH_Consignor = consignor.PK;
			Factory.Save();
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals(string.Format(@"Test Rate - Charge Code Minimum 
-  From {0} To {1}|USD|200.50|", consignor.OH_FullName, consignee.OH_FullName), quotationLines[0].ToString());
		}

		public void TestCalculation()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			Line.TL_RX_NKCurrency = "AUD";

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);

			TestCalculator.IsJobMinimum = true;
			TestCalculator.MinimumValue = 50m;

			AssertEquals(false, TestCalculator.IsChargeCodeMinimum);
			AssertCalculation(parameters, 50m, "MIN AUD 50.00 (Job Minimum)");

			parameters.Results.AddNew(ChargeCode, "AUD", 10M);
			parameters.Results.AddNew(ChargeCode, "AUD", 5M);
			parameters.Results.AddNew(ChargeCode, "AUD", 20M);
			parameters.Results.AddNew(ChargeCode, "AUD", 10M);

			AssertCalculation(parameters, 50m, "MIN AUD 50.00 (Job Minimum)");

			parameters.Results.AddNew(ChargeCode, "AUD", 45M);
			parameters.Results.AddNew(ChargeCode, "AUD", 10M);

			//AssertCalculation(parameters, 0m, "Calculation failed due to incorrect or missing data");

			Criteria.CurrencyConverter = new TestCurrencyConverter(Factory);
			Line.TL_RX_NKCurrency = "USD";

			TestCalculator.MinimumValue = 50m;

			AssertCalculation(parameters, 50m, "MIN USD 50.00 (Job Minimum)");

			TestCalculator.IsChargeCodeMinimum = true;
			TestCalculator.MinimumValue = 50m;

			AssertEquals(false, TestCalculator.IsJobMinimum);
			AssertCalculation(parameters, 50m, "MIN USD 50.00 (Charge Code Minimum)");
		}

		public void TestCalculate_JobHasExistingChargesAndMultipleChargesAppliesToFound_ShouldApplyMinValue()
		{
			Criteria.SetExistingCharges(new[]
			{
				CreateCostCharge(chargeCode: ChargeCode.AC_Code, amount: 1500, currency: "AUD")
			});

			var autoRateInfos = new AutoRateInfoCollection(Factory);
			autoRateInfos.AddNew(Helper.ChargeCodes[ChargeCode.AC_Code], amount: 1500, currency: "AUD");
			autoRateInfos.AddNew(Helper.ChargeCodes[ChargeCode.AC_Code], amount: 2000, currency: "AUD");

			TestCalculator.MinimumValue = 5000m;
			TestCalculator.IsChargeCodeMinimum = true;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria, autoRateInfos);
			var result = TestCalculator.Calculate(parameters).results.Single();

			AssertEquals("The description should match the expected minimum value message.",
				"MIN AUD 5000.00 (Charge Code Minimum)",
				result.Description);

			AssertEquals(
				"Having multiple applicable charges should not be a problem; the calculator should apply the minimum value and not crash.",
				5000m,
				result.PaymentBases.Calculate().amount);
		}

		public override void TestGetCloneCode()
		{
			AssertGetCloneCode(CalculatorCode);
		}

		public override void TestGetCloneLineItems()
		{
			TestCalculator.Line.ViewAgentRates = false;
			TestCalculator.MinimumValue = 25m;
			TestCalculator.Line.ViewAgentRates = true;
			TestCalculator.MinimumValue = 40m;

			var clientRate = Factory.New<ClientRate>();
			var entry = clientRate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			var source = (CompanyTariffOrCostBasedCalculator)line.Calculator;

			source.PerUnit = 6m;
			source.BaseRate = 80m;
			var cloneHelper = new CompanyTariffOrCostLineCloneHelper(line);
			var clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			clonedLine.GetCalculator<MinimumCalculator>().Line.ViewAgentRates = false;
			AssertEquals(111m, clonedLine.GetCalculator<MinimumCalculator>().MinimumValue);
			clonedLine.GetCalculator<MinimumCalculator>().Line.ViewAgentRates = true;
			AssertEquals(126m, clonedLine.GetCalculator<MinimumCalculator>().MinimumValue);

			source.PerUnit = 0m;
			source.Percent = 20m;
			source.CalculationOrder = CompanyTariffOrCostBasedCalculator.Items.PercentFirst;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			clonedLine.GetCalculator<MinimumCalculator>().Line.ViewAgentRates = false;
			AssertEquals(110m, clonedLine.GetCalculator<MinimumCalculator>().MinimumValue);
			clonedLine.GetCalculator<MinimumCalculator>().Line.ViewAgentRates = true;
			AssertEquals(128m, clonedLine.GetCalculator<MinimumCalculator>().MinimumValue);

			source.CalculationOrder = CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			clonedLine.GetCalculator<MinimumCalculator>().Line.ViewAgentRates = false;
			AssertEquals(126m, clonedLine.GetCalculator<MinimumCalculator>().MinimumValue);
			clonedLine.GetCalculator<MinimumCalculator>().Line.ViewAgentRates = true;
			AssertEquals(144m, clonedLine.GetCalculator<MinimumCalculator>().MinimumValue);
		}

		public override void TestGetCostsComparerChargesSummary()
		{
			TestCalculator.MinimumValue = 50m;
			var items = TestCalculator.GetCostsComparerChargesSummary(new List<RateLine>());
			AssertEquals(1, items.Count);
			AssertChargesSummaryItem(items[0], "MIN", 50m);
		}

		public override void TestPricePerSingleChargeable()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateEntry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "NZAKL");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine("FRT", MinimumCalculator.Code);
			rateLine.Calculator[MinimumCalculator.Items.MinimumType] = (ZString)CalculatorConstants.Text.MIN_ChargeCode;
			rateLine.Calculator[MinimumCalculator.Items.MIN] = (ZDecimal)50m;

			AssertEquals("AUD 50.0000", rateEntry.AllInCost());
			AssertEquals("", rateEntry.FreightRatePerChargeableUnit());
		}

		#region Implementation

		protected override Type CalculatorType
		{
			get { return typeof(MinimumCalculator); }
		}

		protected override string CalculatorCode
		{
			get { return MinimumCalculator.Code; }
		}

		new MinimumCalculator TestCalculator
		{
			get { return (MinimumCalculator)base.TestCalculator; }
		}

		#endregion
	}
}
