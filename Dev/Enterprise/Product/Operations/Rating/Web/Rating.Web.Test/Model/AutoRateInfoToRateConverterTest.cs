using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Web.Model;
using Enterprise.Rating.Web.Model.Conversion;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Web.Test.Model
{
	public class AutoRateInfoToRateConverterTest : RatingTestCase
	{
		public void TestConvert_OnlyRateLinesWithCalculations()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var entry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var line1 = entry.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			line1.RateLineItems.RemoveAndDeleteAll();
			var item1 = line1.RateLineItems.AddNew();
			item1.TM_Type = Calculator.Items.Operator.BAS;
			item1.TM_Value = 50m;
			item1.TM_AgentDeclaredRate = 0m;

			var line2 = entry.AddRateLine("BAF", FlatCalculator.Code, "KG", "AUD");
			line1.RateLineItems.RemoveAndDeleteAll();
			var item2 = line2.RateLineItems.AddNew();
			item2.TM_Type = Calculator.Items.Operator.BAS;
			item2.TM_Value = 70m;
			item2.TM_AgentDeclaredRate = 0m;

			var query = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			var businessObject = new RateQueryBusinessObject(Factory, query, SourceEndpoint.JobCharges);
			var criteria = new RatingCriteria(businessObject.RatingAdapter, Factory, false);
			var autoRatingParameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var calculationResult = CalculationResult.CreateForTest(line1, 50, 0, 50, autoRatingParameters.Criteria);

			var rateInfo = new AutoRateInfo(calculationResult, autoRatingParameters, Factory);

			var logger = new ElementaryLogger();
			var converter = new AutoRateInfoToRateConverter(Factory);

			var conversionResults = converter.Convert((RateQueryRatingAdapter)businessObject.RatingAdapter, new[] { rateInfo }, logger);
			AssertEquals(1, conversionResults.Count);

			var convertedRate = conversionResults.First();
			AssertEquals(1, convertedRate.Charges.Length);
			AssertEquals("FRT", convertedRate.Charges[0].ChargeCode.CWCode);
			AssertEquals("FRT", convertedRate.Charges[0].ChargeGroup);
			AssertEquals(string.Empty, convertedRate.Charges[0].ChargeSubGroup);
		}

		public void TestConvert_CalculationItem_Cost()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var entry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var line = entry.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			line.RateLineItems.RemoveAndDeleteAll();
			var item1 = line.RateLineItems.AddNew();
			item1.TM_Type = Calculator.Items.Operator.BAS;
			item1.TM_Value = 50m;
			item1.TM_AgentDeclaredRate = 0m;

			var query = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			var businessObject = new RateQueryBusinessObject(Factory, query, SourceEndpoint.JobCharges);
			var criteria = new RatingCriteria(businessObject.RatingAdapter, Factory, false);
			var autoRatingParameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var calculationResult = CalculationResult.CreateForTest(line, 50, 0, 50, autoRatingParameters.Criteria);

			var rateInfo = new AutoRateInfo(calculationResult, autoRatingParameters, Factory);

			var expectedCost = new CalculationItem()
			{
				Amount = 50,
				Currency = "AUD",
				Audit = "FRT: Maximum AUD 50.00",
				LocalAmount = 50,
				LocalCurrency = "AUD",
				ExchangeRate = 1
			};

			var logger = new ElementaryLogger();
			var converter = new AutoRateInfoToRateConverter(Factory);

			var conversionResults = converter.Convert((RateQueryRatingAdapter)businessObject.RatingAdapter, new[] { rateInfo }, logger);
			var convertedRate = conversionResults.First();

			AssertCalculationItem(expectedCost, convertedRate.Charges[0].Cost);
			AssertNull(convertedRate.Charges[0].Revenue);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestConvert_CalculationItem_Cost_ForeignCurrency()
		{
			GlbCompany.CurrentCompany.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 5, 0);

			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var entry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var line = entry.AddRateLine("FRT", FlatCalculator.Code, "KG", "USD");
			line.RateLineItems.RemoveAndDeleteAll();
			var item1 = line.RateLineItems.AddNew();
			item1.TM_Type = Calculator.Items.Operator.BAS;
			item1.TM_Value = 50m;
			item1.TM_AgentDeclaredRate = 0m;

			Helper.NewExchangeRate("USD", ExchangeRateTypes.Code.BuyRate, new ZDecimal(0.68M));

			Factory.Save();

			var query = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			var businessObject = new RateQueryBusinessObject(Factory, query, SourceEndpoint.JobCharges);
			var criteria = new RatingCriteria(businessObject.RatingAdapter, Factory, false);
			var autoRatingParameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var calculationResult = CalculationResult.CreateForTest(line, 50, 0, 50, autoRatingParameters.Criteria);

			var rateInfo = new AutoRateInfo(calculationResult, autoRatingParameters, Factory);

			var expectedCost = new CalculationItem()
			{
				Amount = 50,
				Currency = "USD",
				Audit = "FRT: Maximum USD 50.00",
				LocalAmount = 73.53M,
				LocalCurrency = "AUD",
				ExchangeRate = 0.68M // CFX Uplift on Company/Branch level does not have any effect on cost exchange rate
			};

			var logger = new ElementaryLogger();
			var converter = new AutoRateInfoToRateConverter(Factory);

			var conversionResults = converter.Convert((RateQueryRatingAdapter)businessObject.RatingAdapter, new[] { rateInfo }, logger);
			var convertedRate = conversionResults.First();

			AssertCalculationItem(expectedCost, convertedRate.Charges[0].Cost);
			AssertNull(convertedRate.Charges[0].Revenue);
		}

		public void TestConvert_CalculationItem_Revenue()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);

			var rate = Helper.NewClientRate(NewClient);
			var entry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "", "");
			entry.RateLines.RemoveAndDeleteAll();

			var line = entry.AddRateLine(TestBAF.AC_Code, FlatCalculator.Code);

			((FlatCalculator)line.Calculator).BaseRate = 50m;
			line.ViewAgentRates = false;

			Factory.Save();

			var query = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			var businessObject = new RateQueryBusinessObject(Factory, query, SourceEndpoint.JobCharges);
			var criteria = new RatingCriteria(businessObject.RatingAdapter, Factory, false);
			var autoRatingParameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var calculationResult = CalculationResult.CreateForTest(line, 50, 0, 50, autoRatingParameters.Criteria);

			var rateInfo = new AutoRateInfo(calculationResult, autoRatingParameters, Factory);

			var expectedRevenue = new CalculationItem()
			{
				Amount = 50,
				Currency = "AUD",
				Audit = "TESTBAF: Maximum AUD 50.00",
				LocalAmount = 50,
				LocalCurrency = "AUD",
				ExchangeRate = 1
			};

			var logger = new ElementaryLogger();
			var converter = new AutoRateInfoToRateConverter(Factory);

			var conversionResults = converter.Convert((RateQueryRatingAdapter)businessObject.RatingAdapter, new[] { rateInfo }, logger);
			var convertedRate = conversionResults.First();

			AssertCalculationItem(expectedRevenue, convertedRate.Charges[0].Revenue);
			AssertNull(convertedRate.Charges[0].Cost);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestConvert_CalculationItem_Revenue_ForeignCurrency()
		{
			GlbCompany.CurrentCompany.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 5, 0);

			InsertClientChargeCodesForAutoRaterTests(Factory);

			var rate = Helper.NewClientRate(NewClient);
			var entry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "", "");
			entry.RateLines.RemoveAndDeleteAll();

			var line = entry.AddRateLine(TestBAF.AC_Code, FlatCalculator.Code, currencyCode: "USD");

			((FlatCalculator)line.Calculator).BaseRate = 50m;
			line.ViewAgentRates = false;

			Helper.NewExchangeRate("USD", ExchangeRateTypes.Code.BuyRate, new ZDecimal(0.68M));
			Factory.Save();

			var query = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			var businessObject = new RateQueryBusinessObject(Factory, query, SourceEndpoint.JobCharges);
			var criteria = new RatingCriteria(businessObject.RatingAdapter, Factory, false);
			var autoRatingParameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var calculationResult = CalculationResult.CreateForTest(line, 50, 0, 50, autoRatingParameters.Criteria);

			var rateInfo = new AutoRateInfo(calculationResult, autoRatingParameters, Factory);

			var expectedRevenue = new CalculationItem()
			{
				Amount = 50,
				Currency = "USD",
				Audit = "TESTBAF: Maximum USD 50.00",
				LocalAmount = 77.40M,
				LocalCurrency = "AUD",
				ExchangeRate = 0.646M // CFX uplift on Company/Branch level will affect Exchange Rate directly => 0.68 - 5% = 0.646
			};

			var logger = new ElementaryLogger();
			var converter = new AutoRateInfoToRateConverter(Factory);

			var conversionResults = converter.Convert((RateQueryRatingAdapter)businessObject.RatingAdapter, new[] { rateInfo }, logger);
			var convertedRate = conversionResults.First();

			AssertCalculationItem(expectedRevenue, convertedRate.Charges[0].Revenue);
			AssertNull(convertedRate.Charges[0].Cost);
		}

		public void AssertCalculationItem(CalculationItem expected, CalculationItem actual)
		{
			AssertEquals(expected.Amount, actual.Amount);
			AssertEquals(expected.Currency, actual.Currency);
			AssertEquals(expected.Audit, actual.Audit);
			AssertEquals(expected.LocalAmount, actual.LocalAmount);
			AssertEquals(expected.LocalCurrency, actual.LocalCurrency);
			AssertEquals(expected.ExchangeRate, actual.ExchangeRate);
		}
	}
}
