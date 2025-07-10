using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class SplitMonthBillingCalculatorTest : CalculatorTest
	{
		public override void TestCheckOrCreateItems()
		{
			Assert(true);
		}

		public override void TestMapping()
		{
			Assert(true);
		}

		public override void TestQuotationLines()
		{
			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "USD";

			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL);

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate||Not Charged|", quotationLines[0].ToString());

			TestCalculator.Minimum = 100m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|USD|100.00|", quotationLines[0].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Charge|USD|100.00|", quotationLines[0].ToString());

			TestCalculator.Minimum = 0m;
			TestCalculator.BaseRate = (ZDecimal)15m;
			TestCalculator["-10"] = (ZDecimal)50m;
			TestCalculator["+10"] = (ZDecimal)60m;
			TestCalculator["+20"] = (ZDecimal)70m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(5, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Base Rate|USD|15.00|", quotationLines[1].ToString());
			AssertEquals("Less than 10 Day(s)|USD|50.00|per KG", quotationLines[2].ToString());
			AssertEquals("10 Day(s) to less than 20 Day(s)|USD|60.00|per KG", quotationLines[3].ToString());
			AssertEquals("20 Day(s) and above|USD|70.00|per KG", quotationLines[4].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(5, quotationLines.Count);
			AssertEquals("Test Charge|||", quotationLines[0].ToString());
			AssertEquals("Base Rate|USD|15.00|", quotationLines[1].ToString());
			AssertEquals("Less than 10 Day(s)|USD|50.00|per KG", quotationLines[2].ToString());
			AssertEquals("10 Day(s) to less than 20 Day(s)|USD|60.00|per KG", quotationLines[3].ToString());
			AssertEquals("20 Day(s) and above|USD|70.00|per KG", quotationLines[4].ToString());

			Line.TL_RX_NKCurrency = "EUR";
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(5, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Base Rate|EUR|15.00|", quotationLines[1].ToString());
			AssertEquals("Less than 10 Day(s)|EUR|50.00|per KG", quotationLines[2].ToString());
			AssertEquals("10 Day(s) to less than 20 Day(s)|EUR|60.00|per KG", quotationLines[3].ToString());
			AssertEquals("20 Day(s) and above|EUR|70.00|per KG", quotationLines[4].ToString());

			Line.TL_RX_NKCurrency = "SGD";
			TestCalculator.Minimum = 100m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(6, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Minimum|SGD|100.00|", quotationLines[1].ToString());
			AssertEquals("Base Rate|SGD|15.00|", quotationLines[2].ToString());
			AssertEquals("Less than 10 Day(s)|SGD|50.00|per KG", quotationLines[3].ToString());
			AssertEquals("10 Day(s) to less than 20 Day(s)|SGD|60.00|per KG", quotationLines[4].ToString());
			AssertEquals("20 Day(s) and above|SGD|70.00|per KG", quotationLines[5].ToString());
		}

		public override void TestDocLineAmount()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var rateEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA);

			var rateLine10 = AddSplitMonthBillingRateLine(rateEntry, "FRT", "KG", "AUD", min: 100, flat: 101, ("-5", 102), ("+5", 103));
			var rateLine11 = AddSplitMonthBillingRateLine(rateEntry, "FRT", "KG", "AUD", min: 110, flat: 111, ("-5", 112), ("+5", 113), ("+6", 114));
			var rateLine12 = AddSplitMonthBillingRateLine(rateEntry, "FRT", "M3", "AUD", min: 120, flat: 121, ("-5", 122), ("+5", 123));

			var rateLine20 = AddSplitMonthBillingRateLine(rateEntry, "FRT", "KG", "USD", min: 200, flat: 201, ("-5", 202), ("+5", 203));

			var docLineAmount = rateLine10.Calculator.GetDocLineAmount()
				+ rateLine11.Calculator.GetDocLineAmount()
				+ rateLine12.Calculator.GetDocLineAmount()
				+ rateLine20.Calculator.GetDocLineAmount();

			AssertQuotationLineList
			(
				docLineAmount,
				new[]
				{
					"Minimum|AUD|100.00|",
					"|AUD|333.00|",
					"Less than 5 Day(s)|AUD|214.00|per KG",
					"5 Day(s) and above|AUD|103.00|per KG",
					"5 Day(s) to less than 6 Day(s)|AUD|113.00|per KG",
					"6 Day(s) and above|AUD|114.00|per KG",
					"Less than 5 Day(s)|AUD|122.00|per M3",
					"5 Day(s) and above|AUD|123.00|per M3",
					"Minimum|USD|200.00|",
					"|USD|201.00|",
					"Less than 5 Day(s)|USD|202.00|per KG",
					"5 Day(s) and above|USD|203.00|per KG"
				}
			);
		}

		static RateLine AddSplitMonthBillingRateLine(RateEntry rateEntry, ZString chargeCode, string unit, string currency, decimal min, decimal flat, params (string Break, decimal Rate)[] items)
		{
			var rateLine = rateEntry.AddRateLine(chargeCode, SplitMonthBillingCalculator.Code, unit, currency);
			var calculator = rateLine.GetCalculator<SplitMonthBillingCalculator>();
			calculator.Minimum = min;
			calculator.BaseRate = flat;
			foreach (var item in items)
			{
				calculator[item.Break] = (ZDecimal)item.Rate;
			}

			return rateLine;
		}

		protected override void TestQuotationLinesWMCore()
		{
			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "USD";

			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL);

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate||Not Charged|", quotationLines[0].ToString());

			TestCalculator.Minimum = 100m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|USD|100.00|", quotationLines[0].ToString());

			TestCalculator.Minimum = 0m;
			TestCalculator.BaseRate = (ZDecimal)15m;
			TestCalculator["-10"] = (ZDecimal)50m;
			TestCalculator["+10"] = (ZDecimal)60m;
			TestCalculator["+20"] = (ZDecimal)70m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(5, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Base Rate|USD|15.00|", quotationLines[1].ToString());
			AssertEquals("Less than 10 Day(s)|USD|50.00|per W/M", quotationLines[2].ToString());
			AssertEquals("10 Day(s) to less than 20 Day(s)|USD|60.00|per W/M", quotationLines[3].ToString());
			AssertEquals("20 Day(s) and above|USD|70.00|per W/M", quotationLines[4].ToString());

			Line.TL_RX_NKCurrency = "EUR";
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(5, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Base Rate|EUR|15.00|", quotationLines[1].ToString());
			AssertEquals("Less than 10 Day(s)|EUR|50.00|per W/M", quotationLines[2].ToString());
			AssertEquals("10 Day(s) to less than 20 Day(s)|EUR|60.00|per W/M", quotationLines[3].ToString());
			AssertEquals("20 Day(s) and above|EUR|70.00|per W/M", quotationLines[4].ToString());

			Line.TL_RX_NKCurrency = "SGD";
			TestCalculator.Minimum = 100m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(6, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Minimum|SGD|100.00|", quotationLines[1].ToString());
			AssertEquals("Base Rate|SGD|15.00|", quotationLines[2].ToString());
			AssertEquals("Less than 10 Day(s)|SGD|50.00|per W/M", quotationLines[3].ToString());
			AssertEquals("10 Day(s) to less than 20 Day(s)|SGD|60.00|per W/M", quotationLines[4].ToString());
			AssertEquals("20 Day(s) and above|SGD|70.00|per W/M", quotationLines[5].ToString());
		}

		public void TestWharehouseInwardsQuotationLines()
		{
			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "USD";

			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL);

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate||Not Charged|", quotationLines[0].ToString());

			TestCalculator.Minimum = 100m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|USD|100.00|", quotationLines[0].ToString());

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "CCC";
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSInwards;
			chargeCode.AC_RateCalculator = SplitMonthBillingCalculator.Code;

			Line.TL_AC = chargeCode.PK;
			Line.TL_RateDesc = "";
			TestCalculator.Minimum = 0m;
			TestCalculator["-10"] = (ZDecimal)50m;
			TestCalculator["+10"] = (ZDecimal)60m;
			TestCalculator["+20"] = (ZDecimal)70m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("|||", quotationLines[0].ToString());
			AssertEquals("Received before 10 Day(s)|USD|50.00|per KG", quotationLines[1].ToString());
			AssertEquals("10 Day(s) to received before 20 Day(s)|USD|60.00|per KG", quotationLines[2].ToString());
			AssertEquals("20 Day(s) and above|USD|70.00|per KG", quotationLines[3].ToString());

			Line.TL_RX_NKCurrency = "EUR";
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("|||", quotationLines[0].ToString());
			AssertEquals("Received before 10 Day(s)|EUR|50.00|per KG", quotationLines[1].ToString());
			AssertEquals("10 Day(s) to received before 20 Day(s)|EUR|60.00|per KG", quotationLines[2].ToString());
			AssertEquals("20 Day(s) and above|EUR|70.00|per KG", quotationLines[3].ToString());

			Line.TL_RX_NKCurrency = "SGD";
			TestCalculator.Minimum = 100m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(5, quotationLines.Count);
			AssertEquals("|||", quotationLines[0].ToString());
			AssertEquals("Minimum|SGD|100.00|", quotationLines[1].ToString());
			AssertEquals("Received before 10 Day(s)|SGD|50.00|per KG", quotationLines[2].ToString());
			AssertEquals("10 Day(s) to received before 20 Day(s)|SGD|60.00|per KG", quotationLines[3].ToString());
			AssertEquals("20 Day(s) and above|SGD|70.00|per KG", quotationLines[4].ToString());
		}

		public void TestWharehouseOutwardsQuotationLines()
		{
			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "USD";

			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL);

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate||Not Charged|", quotationLines[0].ToString());

			TestCalculator.Minimum = 100m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|USD|100.00|", quotationLines[0].ToString());

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "CCC";
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSOutwards;
			chargeCode.AC_RateCalculator = SplitMonthBillingCalculator.Code;

			Line.TL_AC = chargeCode.PK;
			Line.TL_RateDesc = "";
			TestCalculator.Minimum = 0m;
			TestCalculator["-10"] = (ZDecimal)50m;
			TestCalculator["+10"] = (ZDecimal)60m;
			TestCalculator["+20"] = (ZDecimal)70m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("|||", quotationLines[0].ToString());
			AssertEquals("Released before 10 Day(s)|USD|50.00|per KG", quotationLines[1].ToString());
			AssertEquals("10 Day(s) to released before 20 Day(s)|USD|60.00|per KG", quotationLines[2].ToString());
			AssertEquals("20 Day(s) and above|USD|70.00|per KG", quotationLines[3].ToString());

			Line.TL_RX_NKCurrency = "EUR";
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("|||", quotationLines[0].ToString());
			AssertEquals("Released before 10 Day(s)|EUR|50.00|per KG", quotationLines[1].ToString());
			AssertEquals("10 Day(s) to released before 20 Day(s)|EUR|60.00|per KG", quotationLines[2].ToString());
			AssertEquals("20 Day(s) and above|EUR|70.00|per KG", quotationLines[3].ToString());

			Line.TL_RX_NKCurrency = "SGD";
			TestCalculator.Minimum = 100m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(5, quotationLines.Count);
			AssertEquals("|||", quotationLines[0].ToString());
			AssertEquals("Minimum|SGD|100.00|", quotationLines[1].ToString());
			AssertEquals("Released before 10 Day(s)|SGD|50.00|per KG", quotationLines[2].ToString());
			AssertEquals("10 Day(s) to released before 20 Day(s)|SGD|60.00|per KG", quotationLines[3].ToString());
			AssertEquals("20 Day(s) and above|SGD|70.00|per KG", quotationLines[4].ToString());
		}

		public void TestCalculation()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			TestCalculator["-10"] = (ZDecimal)45m;
			TestCalculator["+10"] = (ZDecimal)62.4m;
			TestCalculator["+20"] = (ZDecimal)81.53m;
			Line.TL_WeightVolume = "M3";
			Line.TL_RX_NKCurrency = "EUR";

			Criteria.CurrencyConverter = new TestCurrencyConverter(Factory);
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.ChargeableAmount = new Quantity(6M, QuantityUnit.M3);

			parameters.SetTime(3, 0);
			AssertCalculation(parameters, 270m, "6 Cubic Meter(s) (Less Than 10 Day(s)) @ EUR 45.00/M3");

			parameters.SetTime(15, 0);
			AssertCalculation(parameters, 374.40m, "6 Cubic Meter(s) (Greater Than 10 Day(s) Less Than 20 Day(s)) @ EUR 62.40/M3");

			parameters.SetTime(25, 0);
			AssertCalculation(parameters, 489.18m, "6 Cubic Meter(s) (Greater Than 20 Day(s)) @ EUR 81.53/M3");

			TestCalculator.BaseRate = (ZDecimal)20m;

			parameters.SetTime(3, 0);
			AssertCalculation(parameters, 290m, "Base Rate EUR 20.00 + 6 Cubic Meter(s) (Less Than 10 Day(s)) @ EUR 45.00/M3");

			parameters.SetTime(15, 0);
			AssertCalculation(parameters, 394.40m, "Base Rate EUR 20.00 + 6 Cubic Meter(s) (Greater Than 10 Day(s) Less Than 20 Day(s)) @ EUR 62.40/M3");

			parameters.SetTime(25, 0);
			AssertCalculation(parameters, 509.18m, "Base Rate EUR 20.00 + 6 Cubic Meter(s) (Greater Than 20 Day(s)) @ EUR 81.53/M3");
		}

		public void TestCalculationWithCallForPricingFlag()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			TestCalculator["-10"] = (ZDecimal)45m;
			TestCalculator["+10"] = (ZDecimal)62.4m;
			TestCalculator["+20"] = (ZDecimal)81.53m;

			Line.TL_WeightVolume = "M3";
			Line.TL_RX_NKCurrency = "EUR";

			TestCalculator.RateLineItems[2].TM_CallForPricing = true;
			TestCalculator.RateLineItems[2].TM_Text = "Call us for pricing";

			Criteria.CurrencyConverter = new TestCurrencyConverter(Factory);
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			using (_Rating.Start(new LoggerDecorator()))
			{
				parameters.ChargeableAmount = new Quantity(6M, QuantityUnit.M3);

				parameters.SetTime(3, 0);
				AssertCalculation(parameters, 270m, "6 Cubic Meter(s) (Less Than 10 Day(s)) @ EUR 45.00/M3");

				parameters.SetTime(15, 0);
				AssertCalculation(parameters, 374.40m, "6 Cubic Meter(s) (Greater Than 10 Day(s) Less Than 20 Day(s)) @ EUR 62.40/M3");

				parameters.SetTime(25, 0);
				AssertExceptionThrown<AutoRater.CallForPriceException>(() => TestCalculator.Calculate(parameters));

				TestCalculator.BaseRate = 20m;

				parameters.SetTime(3, 0);
				AssertCalculation(parameters, 290m, "Base Rate EUR 20.00 + 6 Cubic Meter(s) (Less Than 10 Day(s)) @ EUR 45.00/M3");

				parameters.SetTime(15, 0);
				AssertCalculation(parameters, 394.40m, "Base Rate EUR 20.00 + 6 Cubic Meter(s) (Greater Than 10 Day(s) Less Than 20 Day(s)) @ EUR 62.40/M3");
			}
		}

		public void TestWarehouseInwardsCalculation()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "CCC";
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSInwards;
			chargeCode.AC_RateCalculator = SplitMonthBillingCalculator.Code;

			TestCalculator["-10"] = (ZDecimal)45m;
			TestCalculator["+10"] = (ZDecimal)62.4m;
			TestCalculator["+20"] = (ZDecimal)81.53m;
			Line.TL_WeightVolume = "M3";
			Line.TL_RX_NKCurrency = "EUR";
			Line.TL_AC = chargeCode.PK;

			Criteria.CurrencyConverter = new TestCurrencyConverter(Factory);
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.ChargeableAmount = new Quantity(6M, QuantityUnit.M3);

			parameters.SetTime(3, 0);
			AssertCalculation(parameters, 270m, "6 Cubic Meter(s) (Received Before 10 Day(s)) @ EUR 45.00/M3");

			parameters.SetTime(15, 0);
			AssertCalculation(parameters, 374.40m, "6 Cubic Meter(s) (Received After 10 Day(s) Received Before 20 Day(s)) @ EUR 62.40/M3");

			parameters.SetTime(25, 0);
			AssertCalculation(parameters, 489.18m, "6 Cubic Meter(s) (Received After 20 Day(s)) @ EUR 81.53/M3");
		}

		public void TestWarehouseOutwardsCalculation()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			Line.TL_WeightVolume = QuantityUnit.M3;
			Line.TL_RX_NKCurrency = "EUR";

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "CCC";
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSOutwards;
			chargeCode.AC_RateCalculator = SplitMonthBillingCalculator.Code;

			TestCalculator["-10"] = (ZDecimal)45m;
			TestCalculator["+10"] = (ZDecimal)62.4m;
			TestCalculator["+20"] = (ZDecimal)81.53m;
			Line.TL_AC = chargeCode.PK;

			Criteria.CurrencyConverter = new TestCurrencyConverter(Factory);
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.ChargeableAmount = new Quantity(6M, QuantityUnit.M3);

			parameters.SetTime(3, 0);
			AssertCalculation(parameters, 270m, "6 Cubic Meter(s) (Released Before 10 Day(s)) @ EUR 45.00/M3");

			parameters.SetTime(15, 0);
			AssertCalculation(parameters, 374.40m, "6 Cubic Meter(s) (Released After 10 Day(s) Released Before 20 Day(s)) @ EUR 62.40/M3");

			parameters.SetTime(25, 0);
			AssertCalculation(parameters, 489.18m, "6 Cubic Meter(s) (Released After 20 Day(s)) @ EUR 81.53/M3");
		}

		public void TestCalculate_ChargeableIsEmpty_ShouldNotCalculate()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "CCC";
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.ContainerStorage;
			chargeCode.AC_RateCalculator = SplitMonthBillingCalculator.Code;

			Line.TL_AC = chargeCode.PK;
			Line.TL_WeightVolume = QuantityUnit.KG;

			TestCalculator["-10"] = (ZDecimal)10m;
			TestCalculator["+10"] = (ZDecimal)9m;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.ChargeableAmount = Quantity.Empty("The job has no storage service");

			var (result, error) = TestCalculator.Calculate(parameters);

			AssertNullOrEmpty("Error should be null or empty.", error);
			AssertEquals("Result should be empty since chargeable is empty.", 0, result.Count());
		}

		public override void TestGetCloneCode()
		{
			AssertGetCloneCode(CalculatorCode);
		}

		public override void TestGetCloneLineItems()
		{
			TestCalculator.Line.ViewAgentRates = false;
			TestCalculator["-10"] = (ZDecimal)80m;
			TestCalculator["+10"] = (ZDecimal)90m;
			TestCalculator["+15"] = (ZDecimal)120m;
			TestCalculator.Line.ViewAgentRates = true;
			TestCalculator["-10"] = (ZDecimal)100m;
			TestCalculator["+10"] = (ZDecimal)120m;
			TestCalculator["+15"] = (ZDecimal)150m;

			var clientRate = Factory.New<ClientRate>();
			var entry = clientRate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			var source = (CompanyTariffOrCostBasedCalculator)line.Calculator;
			source.PerUnit = 6m;
			source.Minimum = 100m;

			var cloneHelper = new CompanyTariffOrCostLineCloneHelper(line);
			var clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			clonedLine.Calculator.Line.ViewAgentRates = false;
			AssertEquals(80m, clonedLine.Calculator["-10"]);
			AssertEquals(90m, clonedLine.Calculator["+10"]);
			AssertEquals(120m, clonedLine.Calculator["+15"]);
			clonedLine.Calculator.Line.ViewAgentRates = true;
			AssertEquals(100m, clonedLine.Calculator["-10"]);
			AssertEquals(120m, clonedLine.Calculator["+10"]);
			AssertEquals(150m, clonedLine.Calculator["+15"]);

			Line.RateLineItems.RemoveAndDelete(Calculator.Items.Operator.Minus, Calculator.Items.Operator.Plus);
			TestCalculator.Line.ViewAgentRates = false;
			TestCalculator.Minimum = 70m;
			TestCalculator["-10"] = (ZDecimal)80m;
			TestCalculator["+10"] = (ZDecimal)90m;
			TestCalculator["+15"] = (ZDecimal)120m;
			TestCalculator.Line.ViewAgentRates = true;
			TestCalculator.Minimum = 80m;
			TestCalculator["-10"] = (ZDecimal)100m;
			TestCalculator["+10"] = (ZDecimal)120m;
			TestCalculator["+15"] = (ZDecimal)150m;

			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			clonedLine.Calculator.Line.ViewAgentRates = false;
			AssertEquals(170m, clonedLine.GetCalculator<SplitMonthBillingCalculator>().Minimum);
			AssertEquals(80m, clonedLine.Calculator["-10"]);
			AssertEquals(90m, clonedLine.Calculator["+10"]);
			AssertEquals(120m, clonedLine.Calculator["+15"]);
			clonedLine.Calculator.Line.ViewAgentRates = true;
			AssertEquals(180m, clonedLine.GetCalculator<SplitMonthBillingCalculator>().Minimum);
			AssertEquals(100m, clonedLine.Calculator["-10"]);
			AssertEquals(120m, clonedLine.Calculator["+10"]);
			AssertEquals(150m, clonedLine.Calculator["+15"]);

			source.Minimum = 0m;
			source.BaseRate = 50m;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			clonedLine.Calculator.Line.ViewAgentRates = false;
			AssertEquals(70m, clonedLine.GetCalculator<SplitMonthBillingCalculator>().Minimum);
			AssertEquals(130m, clonedLine.Calculator["-10"]);
			AssertEquals(140m, clonedLine.Calculator["+10"]);
			AssertEquals(170m, clonedLine.Calculator["+15"]);
			clonedLine.Calculator.Line.ViewAgentRates = true;
			AssertEquals(80m, clonedLine.GetCalculator<SplitMonthBillingCalculator>().Minimum);
			AssertEquals(150m, clonedLine.Calculator["-10"]);
			AssertEquals(170m, clonedLine.Calculator["+10"]);
			AssertEquals(200m, clonedLine.Calculator["+15"]);

			source.PerUnit = 0m;
			source.Percent = 20m;
			source.CalculationOrder = Calculator.Items.PercentFirst;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			clonedLine.Calculator.Line.ViewAgentRates = false;
			AssertEquals(84m, clonedLine.GetCalculator<SplitMonthBillingCalculator>().Minimum);
			AssertEquals(146m, clonedLine.Calculator["-10"]);
			AssertEquals(158m, clonedLine.Calculator["+10"]);
			AssertEquals(194m, clonedLine.Calculator["+15"]);
			clonedLine.Calculator.Line.ViewAgentRates = true;
			AssertEquals(96m, clonedLine.GetCalculator<SplitMonthBillingCalculator>().Minimum);
			AssertEquals(170m, clonedLine.Calculator["-10"]);
			AssertEquals(194m, clonedLine.Calculator["+10"]);
			AssertEquals(230m, clonedLine.Calculator["+15"]);

			source.CalculationOrder = Calculator.Items.IncreaseFirst;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			clonedLine.Calculator.Line.ViewAgentRates = false;
			AssertEquals(84m, clonedLine.GetCalculator<SplitMonthBillingCalculator>().Minimum);
			AssertEquals(156m, clonedLine.Calculator["-10"]);
			AssertEquals(168m, clonedLine.Calculator["+10"]);
			AssertEquals(204m, clonedLine.Calculator["+15"]);
			clonedLine.Calculator.Line.ViewAgentRates = true;
			AssertEquals(96m, clonedLine.GetCalculator<SplitMonthBillingCalculator>().Minimum);
			AssertEquals(180m, clonedLine.Calculator["-10"]);
			AssertEquals(204m, clonedLine.Calculator["+10"]);
			AssertEquals(240m, clonedLine.Calculator["+15"]);
		}

		protected override Type CalculatorType
		{
			get { return typeof(SplitMonthBillingCalculator); }
		}

		protected override string CalculatorCode
		{
			get { return SplitMonthBillingCalculator.Code; }
		}

		new SplitMonthBillingCalculator TestCalculator
		{
			get { return (SplitMonthBillingCalculator)base.TestCalculator; }
		}

		#region Validation

		public void TestValidateTM_Break()
		{
			var item1a = TestCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 50m, 10m);
			AssertHasError(item1a.TM_BreakInfo, "Number of Days in month should be less or equal to 31.");
			TestCalculator.RateLineItems[0].TM_Break = 10m;
			AssertNoErrors(item1a.TM_BreakInfo);
		}

		public void TestValidateRateOperator()
		{
			var dummyChargeCode = Factory.New<AccChargeCode>();
			dummyChargeCode.AC_RateCalculator = SplitMonthBillingCalculator.Code;

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var testEntry = testRate.AddRateEntry("DST");

			var dSTRateLine = testEntry.RateLines.AddNew();
			dSTRateLine.TL_AC = dummyChargeCode.PK;

			var line1 = dSTRateLine.RateLineItems.AddNew();
			line1.TM_Type = Calculator.Items.Operator.BAS;
			AssertEquals("Has Errors", false, line1.TM_TypeInfo.HasErrors());

			line1.TM_Type = Calculator.Items.Operator.UNT;
			AssertEquals("Has Errors", true, line1.TM_TypeInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.UNTNotAllowed, line1.TM_TypeInfo.GetErrors().GetFirstMessage());

			line1.TM_Type = Calculator.Items.Operator.MIN;
			AssertEquals("Has Errors", false, line1.TM_TypeInfo.HasErrors());

			line1.TM_Type = Calculator.Items.Operator.Minus;
			AssertEquals("Has Errors", true, line1.TM_TypeInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.MoreLinesRequired, line1.TM_TypeInfo.GetErrors().GetFirstMessage());

			var line2 = dSTRateLine.RateLineItems.AddNew();
			line2.TM_Type = Calculator.Items.Operator.BAS;
			AssertNoErrors(line2.TM_TypeInfo);

			line2.TM_Type = Calculator.Items.Operator.Minus;
			AssertEquals("Has Errors", true, line2.TM_TypeInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.DuplicateMINorMAXorBASorUNTorMinusNotAllowed, line2.TM_TypeInfo.GetErrors().GetFirstMessage());

			line2.TM_Type = Calculator.Items.Operator.Plus;
			AssertEquals("Has Errors", false, line2.TM_TypeInfo.HasErrors());

			line2.TM_Type = Calculator.Items.Operator.UNT;
			AssertEquals("Has Errors", true, line2.TM_TypeInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.UNTNotAllowed, line2.TM_TypeInfo.GetErrors().GetFirstMessage());

			line2.TM_Type = Calculator.Items.Operator.MIN;
			AssertNoErrors(line2.TM_TypeInfo);

			var line3 = dSTRateLine.RateLineItems.AddNew();
			line3.TM_Type = Calculator.Items.Operator.BAS;
			AssertNoErrors(line3.TM_TypeInfo);

			line3.TM_Type = Calculator.Items.Operator.Minus;
			AssertEquals("Has Errors", true, line3.TM_TypeInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.DuplicateMINorMAXorBASorUNTorMinusNotAllowed, line3.TM_TypeInfo.GetErrors().GetFirstMessage());

			line3.TM_Type = Calculator.Items.Operator.Plus;
			AssertEquals("Has Errors", false, line3.TM_TypeInfo.HasErrors());

			line3.TM_Type = Calculator.Items.Operator.UNT;
			AssertHasError(line3.TM_TypeInfo, ErrorMessages.UNTNotAllowed);

			line3.TM_Type = Calculator.Items.Operator.MIN;
			AssertEquals("Has Errors", true, line3.TM_TypeInfo.HasErrors());
			AssertEquals("Error Message", ErrorMessages.DuplicateMINorMAXorBASorUNTorMinusNotAllowed, line3.TM_TypeInfo.GetErrors().GetFirstMessage());
		}

		#endregion
	}
}
