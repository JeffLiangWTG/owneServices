using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class TimeCalculatorTest : BaseCombinedCalculatorTest<TimeCalculator>
	{
		protected override int NumberOfRateLineItemsAfterInitialization { get { return 5; } }

		public void TestUnitsDifferentToRateLineUnits()
		{
			Line.TL_WeightVolume = QuantityUnit.HR;

			var rateLineItem = TestCalculator.AddRateLineItem("UNT", 0m, 3m);
			rateLineItem.TM_BreakWeightVolume = QuantityUnit.HR;
			AssertHasErrors(Line.TL_WeightVolumeInfo);

			Line.TL_WeightVolume = QuantityUnit.DY;
			AssertNoErrors(rateLineItem.TM_BreakWeightVolumeInfo);
		}

		public override void TestCheckOrCreateItems()
		{
			AssertNull(Line.RateLineItems.FindByTM_Type(TimeCalculator.Items.ExcludeHolidays));

			base.TestCheckOrCreateItems();

			AssertNotNull(Line.RateLineItems.FindByTM_Type(TimeCalculator.Items.ExcludeHolidays));

			AssertEquals(Line.RateLineItems.FindByTM_Type(TimeCalculator.Items.ExcludeHolidays).TM_TextInfo, TestCalculator.String1Info);
			AssertEquals(TimeCalculator.Items.ExcludeWeekendsAndPublicHolidays, TestCalculator.ExcludeHolidays);

			Assert(TestCalculator.IsAccumulated);
			Assert(!TestCalculator.UseHigherChargeableLowerRateRule);
			Assert(TestCalculator.UseInclusiveBreaks);
		}

		public void TestCheckOrCreateItemsAreNotRecreatedEverytime()
		{
			TestCalculator.IsAccumulated = false;
			TestCalculator.ExcludeHolidays = TimeCalculator.Items.ExcludeHolidays;
			Factory.Save();

			Assert(!TestCalculator.IsAccumulated);
			AssertEquals(TimeCalculator.Items.ExcludeHolidays, TestCalculator.ExcludeHolidays);

			var newFactory = new BusinessObjectFactory();
			var rateLineInAnotherFactory = newFactory.Load<RateLine>(TestCalculator.Line.PK);
			rateLineInAnotherFactory.InitializeCalculator();

			Assert("Reloading the calculator should not reset to default values", !rateLineInAnotherFactory.Calculator.IsAccumulated);
			AssertEquals("Reloading the calculator should not reset to default ", TimeCalculator.Items.ExcludeHolidays, TestCalculator.ExcludeHolidays);
		}

		public new void TestInclusiveBreaksIsNotChangedAfterResettingUseAccumulated()
		{
			TestCalculator.IsAccumulated = false;
			base.TestInclusiveBreaksIsNotChangedAfterResettingUseAccumulated();
		}

		public void TestCheckOrCreateItemsExcludeHolidaysInTimeRatingisFalse()
		{
			Env.Registry.Rating.ExcludeHolidaysInTimeRating = false;
			InitialiseTestCalculator();
			AssertEquals("", TestCalculator.ExcludeHolidays);
		}

		public override void TestMapping()
		{
			TestMapping(TimeCalculator.Items.ExcludeHolidays, "String1");
		}

		public override void TestList1()
		{
			AssertEquals("List1", typeof(CodeDescriptionPairList), TestCalculator.List1.GetType());
			Assert("Count > 0", TestCalculator.List1.Count > 1);
		}

		public override void TestIsAccumulatedAndUseHigherChargeableLowerRateRuleWhenBreaksPerIsNotEmpty()
		{
			var newLine = Line.Parent.RateLines.AddNew();
			newLine.TL_AC = ChargeCode.PK;
			newLine.TL_RateDesc = "New Test Rate 1";
			newLine.TL_WeightVolume = QuantityUnit.KG;
			var testCalc = (TimeCalculator)base.TestCalculator;

			Assert("BreaksPerInfo should be readonly", testCalc.BreaksPerInfo.ReadOnly);
		}

		public override void TestQuotationLines()
		{
			Line.TL_WeightVolume = "M3";
			Line.TL_RX_NKCurrency = "USD";

			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL);

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate||Not Charged|", quotationLines[0].ToString());

			TestCalculator.AddRateLineItem("UNT", 0m, 100m, RatingConstants.Units.DY);
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|USD|100.00|per M3 x Day", quotationLines[0].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Charge|USD|100.00|per M3 x Day", quotationLines[0].ToString());

			Line.RateLineItems.RemoveAndDeleteAll();

			TestCalculator.Minimum = 100m;
			var item = TestCalculator.AddRateLineItem("-", 24m, 10m);
			item.TM_BreakWeightVolume = QuantityUnit.HR;
			TestCalculator["+24"] = (ZDecimal)15m;
			TestCalculator["+72"] = (ZDecimal)20m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(5, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Minimum|USD|100.00|", quotationLines[1].ToString());
			AssertEquals("Less than 24 Hour(s)|USD|10.00|per M3 x Hour", quotationLines[2].ToString());
			AssertEquals("24 Hour(s) to less than 72 Hour(s)|USD|15.00|per M3 x Hour", quotationLines[3].ToString());
			AssertEquals("72 Hour(s) and above|USD|20.00|per M3 x Hour", quotationLines[4].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(5, quotationLines.Count);
			AssertEquals("Test Charge|||", quotationLines[0].ToString());
			AssertEquals("Minimum|USD|100.00|", quotationLines[1].ToString());
			AssertEquals("Less than 24 Hour(s)|USD|10.00|per M3 x Hour", quotationLines[2].ToString());
			AssertEquals("24 Hour(s) to less than 72 Hour(s)|USD|15.00|per M3 x Hour", quotationLines[3].ToString());
			AssertEquals("72 Hour(s) and above|USD|20.00|per M3 x Hour", quotationLines[4].ToString());
		}

		public override void TestDocLineAmount()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var rateEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA);

			var rateLine10 = AddTimeRateLine(rateEntry, "FRT", "KG", "AUD", min: 100, max: 109, flat: 101, perUnit: 0, ("-", 5, 101, QuantityUnit.HR), ("+", 5, 102, ""));
			var rateLine11 = AddTimeRateLine(rateEntry, "FRT", "KG", "AUD", min: 110, max: 119, flat: 111, perUnit: 0, ("-", 5, 111, QuantityUnit.HR), ("+", 5, 112, ""));
			var rateLine12 = AddTimeRateLine(rateEntry, "FRT", "KG", "AUD", min: 120, max: 129, flat: 121, perUnit: 0, ("-", 5, 121, QuantityUnit.WK), ("+", 5, 122, ""));
			var rateLine13 = AddTimeRateLine(rateEntry, "FRT", "KG", "AUD", min: 130, max: 139, flat: 131, perUnit: 132);

			var rateLine20 = AddTimeRateLine(rateEntry, "FRT", "KG", "USD", min: 200, max: 209, flat: 201, perUnit: 202, ("-", 5, 201, QuantityUnit.HR), ("+", 5, 202, ""));

			var docLineAmount = rateLine10.Calculator.GetDocLineAmount()
				+ rateLine11.Calculator.GetDocLineAmount()
				+ rateLine12.Calculator.GetDocLineAmount()
				+ rateLine13.Calculator.GetDocLineAmount()
				+ rateLine20.Calculator.GetDocLineAmount();

			AssertQuotationLineList
			(
				docLineAmount,
				new[]
				{
					"Minimum|AUD|100.00|",
					"|AUD|464.00|",
					"Maximum|AUD|139.00|",
					"Up to 5 Hour(s)|AUD|212.00|per KG x Hour",
					"More than 5 Hour(s)|AUD|214.00|per KG x Hour",
					"Up to 5 Week(s)|AUD|121.00|per KG x Week",
					"More than 5 Week(s)|AUD|122.00|per KG x Week",
					"|AUD|132.00|KG x Day",
					"Minimum|USD|200.00|",
					"|USD|201.00|",
					"Maximum|USD|209.00|",
					"|USD|202.00|KG x Day",
					"Up to 5 Day(s)|USD|201.00|per KG x Day",
					"More than 5 Day(s)|USD|202.00|per KG x Day"
				}
			);
		}

		static RateLine AddTimeRateLine(RateEntry rateEntry, ZString chargeCode, string unit, string currency, decimal min, decimal max, decimal flat, decimal perUnit, params (string Break, decimal BreakAmount, decimal Rate, string Unit)[] items)
		{
			var rateLine = rateEntry.AddRateLine(chargeCode, TimeCalculator.Code, unit, currencyCode: currency);
			var calculator = rateLine.GetCalculator<TimeCalculator>();
			calculator.Minimum = min;
			calculator.Maximum = max;
			calculator.BaseRate = flat;
			if (perUnit != 0)
			{
				calculator.PerUnit = perUnit;
			}

			foreach (var item in items)
			{
				var rateLineItem = calculator.AddRateLineItem(item.Break, item.BreakAmount, item.Rate);
				rateLineItem.TM_BreakWeightVolume = item.Unit;
			}

			return rateLine;
		}

		protected override void TestQuotationLinesWMCore()
		{
			Line.TL_WeightVolume = "M3";
			Line.TL_RX_NKCurrency = "USD";

			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL);

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate||Not Charged|", quotationLines[0].ToString());

			var item = TestCalculator.AddRateLineItem("UNT", 0m, 100m);
			item.TM_BreakWeightVolume = QuantityUnit.DY;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|USD|100.00|per W/M x Day", quotationLines[0].ToString());

			Line.RateLineItems.RemoveAndDeleteAll();

			TestCalculator.Minimum = 100m;
			item = TestCalculator.AddRateLineItem("-", 24m, 10m);
			item.TM_BreakWeightVolume = QuantityUnit.HR;
			TestCalculator["+24"] = (ZDecimal)15m;
			TestCalculator["+72"] = (ZDecimal)20m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(5, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Minimum|USD|100.00|", quotationLines[1].ToString());
			AssertEquals("Less than 24 Hour(s)|USD|10.00|per W/M x Hour", quotationLines[2].ToString());
			AssertEquals("24 Hour(s) to less than 72 Hour(s)|USD|15.00|per W/M x Hour", quotationLines[3].ToString());
			AssertEquals("72 Hour(s) and above|USD|20.00|per W/M x Hour", quotationLines[4].ToString());
		}

		public void TestQuotationLinesNonAccumulated()
		{
			Line.TL_WeightVolume = "M3";
			Line.TL_RX_NKCurrency = "USD";

			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL);

			TestCalculator.IsAccumulated = false;
			TestCalculator.Minimum = 100m;
			TestCalculator.AddRateLineItem("-", 10m, 10m, QuantityUnit.DY);
			TestCalculator["+10"] = (ZDecimal)15m;
			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(4, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Minimum|USD|100.00|", quotationLines[1].ToString());
			AssertEquals("Less than 10 Day(s)|USD|10.00|per M3 x Day", quotationLines[2].ToString());
			AssertEquals("10 Day(s) and above|USD|15.00|per M3 x Day", quotationLines[3].ToString());
		}

		public void TestNonSlidingNonAccumulated_Min()
		{
			TestCalculator.IsAccumulated = false;
			TestCalculator.AddRateLineItem("MIN", 0m, 100);

			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "AUD";

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.ChargeableAmount = new Quantity(10M, QuantityUnit.KG);

			parameters.SetTime(1, 0);
			AssertCalculation(parameters, 100m, "Minimum AUD 100.00");
			parameters.SetTime(0, 1);
			AssertCalculation(parameters, 100m, "Minimum AUD 100.00");
			parameters.SetTime(0, 0, 1);
			AssertCalculation(parameters, 100m, "Minimum AUD 100.00");
		}

		public void TestNonSlidingNonAccumulated_MinAndBase()
		{
			TestCalculator.IsAccumulated = false;
			var minLine = TestCalculator.AddRateLineItem("MIN", 0m, 100);
			var baseLine = TestCalculator.AddRateLineItem("BAS", 0m, 50);

			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "AUD";

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.ChargeableAmount = new Quantity(10M, QuantityUnit.KG);

			parameters.SetTime(1, 0);
			AssertCalculation(parameters, 100m, "Minimum AUD 100.00");
			parameters.SetTime(0, 1);
			AssertCalculation(parameters, 100m, "Minimum AUD 100.00");
			parameters.SetTime(0, 0, 1);
			AssertCalculation(parameters, 100m, "Minimum AUD 100.00");

			minLine.TM_RelevantValue = 50;
			baseLine.TM_RelevantValue = 100;
			parameters.SetTime(1, 0);
			AssertCalculation(parameters, 100m, "Base Rate AUD 100.00");
			parameters.SetTime(0, 1);
			AssertCalculation(parameters, 100m, "Base Rate AUD 100.00");
			parameters.SetTime(0, 0, 1);
			AssertCalculation(parameters, 100m, "Base Rate AUD 100.00");
		}

		public void TestNonSlidingNonAccumulated_Max()
		{
			TestCalculator.IsAccumulated = false;
			TestCalculator.AddRateLineItem("MAX", 0m, 100);

			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "AUD";

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.ChargeableAmount = new Quantity(10M, QuantityUnit.KG);

			parameters.SetTime(1, 0);
			AssertCalculation(parameters, 0m, "10 KG x Day (10 Kilogram(s) x 1 Day(s)) @ AUD 0.00/KG x Day");
			parameters.SetTime(0, 1);
			AssertCalculation(parameters, 0m, "10 KG x Day (10 Kilogram(s) x 1 Day(s)) @ AUD 0.00/KG x Day");
			parameters.SetTime(0, 0, 1);
			AssertCalculation(parameters, 0m, "10 KG x Day (10 Kilogram(s) x 1 Day(s)) @ AUD 0.00/KG x Day");
		}

		public void TestNonSlidingNonAccumulated_MaxAndBase()
		{
			TestCalculator.IsAccumulated = false;
			TestCalculator.AddRateLineItem("MAX", 0m, 100);
			TestCalculator.AddRateLineItem("BAS", 0m, 200);

			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "AUD";

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.ChargeableAmount = new Quantity(10M, QuantityUnit.KG);

			parameters.SetTime(1, 0);
			AssertCalculation(parameters, 100m, "Maximum AUD 100.00");
			parameters.SetTime(0, 1);
			AssertCalculation(parameters, 100m, "Maximum AUD 100.00");
			parameters.SetTime(0, 0, 1);
			AssertCalculation(parameters, 100m, "Maximum AUD 100.00");
		}

		public void TestNonSlidingNonAccumulated_MinAndBaseAndMax()
		{
			TestCalculator.IsAccumulated = false;

			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "AUD";

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.ChargeableAmount = new Quantity(10M, QuantityUnit.KG);

			var minLine = TestCalculator.AddRateLineItem("MIN", 0m, 100);
			var baseLine = TestCalculator.AddRateLineItem("BAS", 0m, 50);
			var maxLine = TestCalculator.AddRateLineItem("MAX", 0m, 70);
			parameters.SetTime(1, 0);
			AssertCalculation(parameters, 100m, "Minimum AUD 100.00");
			parameters.SetTime(0, 1);
			AssertCalculation(parameters, 100m, "Minimum AUD 100.00");
			parameters.SetTime(0, 0, 1);
			AssertCalculation(parameters, 100m, "Minimum AUD 100.00");

			minLine.TM_RelevantValue = 50;
			baseLine.TM_RelevantValue = 200;
			maxLine.TM_RelevantValue = 70;
			parameters.SetTime(1, 0);
			AssertCalculation(parameters, 70m, "Maximum AUD 70.00");
			parameters.SetTime(0, 1);
			AssertCalculation(parameters, 70m, "Maximum AUD 70.00");
			parameters.SetTime(0, 0, 1);
			AssertCalculation(parameters, 70m, "Maximum AUD 70.00");

			minLine.TM_RelevantValue = 80;
			baseLine.TM_RelevantValue = 100;
			maxLine.TM_RelevantValue = 70;
			parameters.SetTime(1, 0);
			AssertCalculation(parameters, 80m, "Minimum AUD 80.00");
			parameters.SetTime(0, 1);
			AssertCalculation(parameters, 80m, "Minimum AUD 80.00");
			parameters.SetTime(0, 0, 1);
			AssertCalculation(parameters, 80m, "Minimum AUD 80.00");

			minLine.TM_RelevantValue = 50;
			baseLine.TM_RelevantValue = 100;
			maxLine.TM_RelevantValue = 200;
			parameters.SetTime(1, 0);
			AssertCalculation(parameters, 100m, "Base Rate AUD 100.00");
			parameters.SetTime(0, 1);
			AssertCalculation(parameters, 100m, "Base Rate AUD 100.00");
			parameters.SetTime(0, 0, 1);
			AssertCalculation(parameters, 100m, "Base Rate AUD 100.00");

			minLine.TM_RelevantValue = 100;
			baseLine.TM_RelevantValue = 75;
			maxLine.TM_RelevantValue = 50;
			parameters.SetTime(1, 0);
			AssertCalculation(parameters, 100m, "Minimum AUD 100.00");
			parameters.SetTime(0, 1);
			AssertCalculation(parameters, 100m, "Minimum AUD 100.00");
			parameters.SetTime(0, 0, 1);
			AssertCalculation(parameters, 100m, "Minimum AUD 100.00");

			minLine.TM_RelevantValue = 75;
			baseLine.TM_RelevantValue = 50;
			maxLine.TM_RelevantValue = 100;
			parameters.SetTime(1, 0);
			AssertCalculation(parameters, 75m, "Minimum AUD 75.00");
			parameters.SetTime(0, 1);
			AssertCalculation(parameters, 75m, "Minimum AUD 75.00");
			parameters.SetTime(0, 0, 1);
			AssertCalculation(parameters, 75m, "Minimum AUD 75.00");
		}

		public void TestNonSlidingNonAccumulated_ByMeasureUnit()
		{
			TestCalculator.IsAccumulated = false;

			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "AUD";

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.ChargeableAmount = new Quantity(2M, QuantityUnit.KG);

			var unitLine = TestCalculator.AddRateLineItem("UNT", 0m, 10);
			unitLine.TM_BreakWeightVolume = QuantityUnit.HR;
			parameters.SetTime(0, 0, 1);
			AssertCalculation(parameters, 0.33, "0.0333 KG x Hour (2 Kilogram(s) x 0.0166666666666667 Hour(s)) @ AUD 10.00/KG x Hour");
			parameters.SetTime(0, 1, 0);
			AssertCalculation(parameters, 2 * 10 * 1 * 1, "2 KG x Hour (2 Kilogram(s) x 1 Hour(s)) @ AUD 10.00/KG x Hour");
			parameters.SetTime(1, 0);
			AssertCalculation(parameters, 2 * 10 * 1 * 24, "48 KG x Hour (2 Kilogram(s) x 24 Hour(s)) @ AUD 10.00/KG x Hour");
			parameters.SetTime(3, 0);
			AssertCalculation(parameters, 2 * 10 * 3 * 24, "144 KG x Hour (2 Kilogram(s) x 72 Hour(s)) @ AUD 10.00/KG x Hour");

			unitLine.TM_BreakWeightVolume = QuantityUnit.DY;
			parameters.SetTime(0, 0, 1);
			AssertCalculation(parameters, 2 * 10 * 1, "2 KG x Day (2 Kilogram(s) x 1 Day(s)) @ AUD 10.00/KG x Day");
			parameters.SetTime(0, 11);
			AssertCalculation(parameters, 2 * 10 * 1, "2 KG x Day (2 Kilogram(s) x 1 Day(s)) @ AUD 10.00/KG x Day");
			parameters.SetTime(1, 0);
			AssertCalculation(parameters, 2 * 10 * 1, "2 KG x Day (2 Kilogram(s) x 1 Day(s)) @ AUD 10.00/KG x Day");
			parameters.SetTime(3, 0);
			AssertCalculation(parameters, 2 * 10 * 3, "6 KG x Day (2 Kilogram(s) x 3 Day(s)) @ AUD 10.00/KG x Day");

			unitLine.TM_BreakWeightVolume = QuantityUnit.WK;
			parameters.SetTime(0, 0, 1);
			AssertCalculation(parameters, 2 * 10 * 1, "2 KG x Week (2 Kilogram(s) x 1 Week(s)) @ AUD 10.00/KG x Week");
			parameters.SetTime(0, 11);
			AssertCalculation(parameters, 2 * 10 * 1, "2 KG x Week (2 Kilogram(s) x 1 Week(s)) @ AUD 10.00/KG x Week");
			parameters.SetTime(1, 0);
			AssertCalculation(parameters, 2 * 10 * 1, "2 KG x Week (2 Kilogram(s) x 1 Week(s)) @ AUD 10.00/KG x Week");
			parameters.SetTime(7, 0);
			AssertCalculation(parameters, 2 * 10 * 1, "2 KG x Week (2 Kilogram(s) x 1 Week(s)) @ AUD 10.00/KG x Week");
			parameters.SetTime(21, 0);
			AssertCalculation(parameters, 2 * 10 * 3, "6 KG x Week (2 Kilogram(s) x 3 Week(s)) @ AUD 10.00/KG x Week");

			unitLine.TM_BreakWeightVolume = QuantityUnit.DY;
			TestCalculator.AddRateLineItem("BAS", 0m, 50);
			parameters.SetTime(0, 0, 1);
			AssertCalculation(parameters, 2 * 10 * 1 + 50, "Base Rate AUD 50.00 + 2 KG x Day (2 Kilogram(s) x 1 Day(s)) @ AUD 10.00/KG x Day");
			parameters.SetTime(0, 11);
			AssertCalculation(parameters, 2 * 10 * 1 + 50, "Base Rate AUD 50.00 + 2 KG x Day (2 Kilogram(s) x 1 Day(s)) @ AUD 10.00/KG x Day");
			parameters.SetTime(1, 0);
			AssertCalculation(parameters, 2 * 10 * 1 + 50, "Base Rate AUD 50.00 + 2 KG x Day (2 Kilogram(s) x 1 Day(s)) @ AUD 10.00/KG x Day");
			parameters.SetTime(3, 0);
			AssertCalculation(parameters, 2 * 10 * 3 + 50, "Base Rate AUD 50.00 + 6 KG x Day (2 Kilogram(s) x 3 Day(s)) @ AUD 10.00/KG x Day");

			unitLine.TM_BreakWeightVolume = QuantityUnit.DY;
			TestCalculator.AddRateLineItem("MAX", 0m, 44);
			parameters.SetTime(0, 0, 1);
			AssertCalculation(parameters, 44, "Maximum AUD 44.00");
			parameters.SetTime(0, 11);
			AssertCalculation(parameters, 44, "Maximum AUD 44.00");
			parameters.SetTime(1, 0);
			AssertCalculation(parameters, 44, "Maximum AUD 44.00");
			parameters.SetTime(3, 0);
			AssertCalculation(parameters, 44, "Maximum AUD 44.00");

			unitLine.TM_BreakWeightVolume = QuantityUnit.DY;
			TestCalculator.AddRateLineItem("MIN", 0m, 555);
			parameters.SetTime(0, 0, 1);
			AssertCalculation(parameters, 555, "Minimum AUD 555.00");
			parameters.SetTime(0, 11);
			AssertCalculation(parameters, 555, "Minimum AUD 555.00");
			parameters.SetTime(1, 0);
			AssertCalculation(parameters, 555, "Minimum AUD 555.00");
			parameters.SetTime(3, 0);
			AssertCalculation(parameters, 555, "Minimum AUD 555.00");
		}

		public void TestNonSlidingNonAccumulated_ByContainer()
		{
			TestCalculator.IsAccumulated = false;

			Line.TL_WeightVolume = "CN";
			Line.TL_RX_NKCurrency = "AUD";

			var container20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			var measures = Criteria.RateableMeasures;
			var testContainers = new TestContainers(Factory, container20GP, 5);
			testContainers.PopulateContainerList(measures);

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);

			var unitLine = TestCalculator.AddRateLineItem("UNT", 0m, 10);

			unitLine.TM_BreakWeightVolume = QuantityUnit.HR;
			parameters.SetTime(0, 0, 1);
			AssertCalculation(parameters, 0.83, "0.0833 Container x Hour (5 Container(s) x 0.0166666666666667 Hour(s)) @ AUD 10.00/Container x Hour");
			parameters.SetTime(0, 1, 0);
			AssertCalculation(parameters, 10 * 5 * 1, "5 Container x Hour (5 Container(s) x 1 Hour(s)) @ AUD 10.00/Container x Hour");
			parameters.SetTime(1, 0, 0);
			AssertCalculation(parameters, 10 * 5 * 24, "120 Container x Hour (5 Container(s) x 24 Hour(s)) @ AUD 10.00/Container x Hour");

			unitLine.TM_BreakWeightVolume = QuantityUnit.DY;
			parameters.SetTime(0, 0, 1);
			AssertCalculation(parameters, 10 * 5 * 1, "5 Container x Day (5 Container(s) x 1 Day(s)) @ AUD 10.00/Container x Day");
			parameters.SetTime(0, 1, 0);
			AssertCalculation(parameters, 10 * 5 * 1, "5 Container x Day (5 Container(s) x 1 Day(s)) @ AUD 10.00/Container x Day");
			parameters.SetTime(1, 0, 0);
			AssertCalculation(parameters, 10 * 5 * 1, "5 Container x Day (5 Container(s) x 1 Day(s)) @ AUD 10.00/Container x Day");
			parameters.SetTime(2, 0, 0);
			AssertCalculation(parameters, 10 * 5 * 2, "10 Container x Day (5 Container(s) x 2 Day(s)) @ AUD 10.00/Container x Day");

			unitLine.TM_BreakWeightVolume = QuantityUnit.WK;
			parameters.SetTime(0, 0, 1);
			AssertCalculation(parameters, 10 * 5 * 1, "5 Container x Week (5 Container(s) x 1 Week(s)) @ AUD 10.00/Container x Week");
			parameters.SetTime(0, 1, 0);
			AssertCalculation(parameters, 10 * 5 * 1, "5 Container x Week (5 Container(s) x 1 Week(s)) @ AUD 10.00/Container x Week");
			parameters.SetTime(1, 0, 0);
			AssertCalculation(parameters, 10 * 5 * 1, "5 Container x Week (5 Container(s) x 1 Week(s)) @ AUD 10.00/Container x Week");
			parameters.SetTime(7, 0, 0);
			AssertCalculation(parameters, 10 * 5 * 1, "5 Container x Week (5 Container(s) x 1 Week(s)) @ AUD 10.00/Container x Week");
			parameters.SetTime(14, 0, 0);
			AssertCalculation(parameters, 10 * 5 * 2, "10 Container x Week (5 Container(s) x 2 Week(s)) @ AUD 10.00/Container x Week");
		}

		public void TestNonSlidingNonAccumulated_ByService()
		{
			TestCalculator.IsAccumulated = false;
			Line.TL_WeightVolume = QuantityUnit.SV;
			var jobServiceInfo = new JobServiceInfo(true, "ORG", "FUM", "Fumigation Service", 1, TimeSpan.FromMinutes(120));
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.Criteria.JobServices.Add(jobServiceInfo);
			ChargeCode.AC_ChargeSubGroup = "FUM";

			var unitLine = TestCalculator.AddRateLineItem("UNT", 0m, 10);
			unitLine.TM_BreakWeightVolume = QuantityUnit.HR;
			jobServiceInfo.ServiceCount = 1;
			AssertCalculation(parameters, 20, "2 Origin Fumigation x Hour (1 Origin Fumigation x 2 Hour(s)) @ AUD 10.00/Origin Fumigation x Hour");

			var maxLine = TestCalculator.AddRateLineItem("MAX", 0m, 100);
			jobServiceInfo.ServiceCount = 100000;
			AssertCalculation(parameters, 100, "Maximum AUD 100.00");
		}

		public void TestCalculation()
		{
			Line.TL_WeightVolume = "M3";
			Line.TL_RX_NKCurrency = "AUD";

			TestCalculator.Minimum = 100m;
			TestCalculator.BaseRate = 5m;
			var unitRateItem = TestCalculator.AddRateLineItem("UNT", 0m, 15m);
			unitRateItem.TM_BreakWeightVolume = QuantityUnit.DY;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.ChargeableAmount = new Quantity(6M, QuantityUnit.M3);
			parameters.SetTime(1, 0);
			AssertCalculation(parameters, 100m, "Minimum AUD 100.00");

			parameters.SetTime(3, 0);
			AssertCalculation(parameters, 275m, "Base Rate AUD 5.00 + 18 M3 x Day (6 Cubic Meter(s) x 3 Day(s)) @ AUD 15.00/M3 x Day");

			unitRateItem.TM_BreakWeightVolume = QuantityUnit.WK;
			parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.ChargeableAmount = new Quantity(6M, QuantityUnit.M3);
			parameters.SetTime(3, 0);
			AssertCalculation(parameters, 100m, "Minimum AUD 100.00");

			parameters.SetTime(14, 0);
			AssertCalculation(parameters, 185m, "Base Rate AUD 5.00 + 12 M3 x Week (6 Cubic Meter(s) x 2 Week(s)) @ AUD 15.00/M3 x Week");

			Line.RateLineItems.RemoveAndDelete(Calculator.Items.Operator.MIN, Calculator.Items.Operator.BAS, Calculator.Items.Operator.UNT);
			TestCalculator.AddRateLineItem("-", 15m, 5m, QuantityUnit.DY);
			TestCalculator["+15"] = (ZDecimal)6m;

			parameters.ChargeableAmount = new Quantity(6M, QuantityUnit.M3);
			parameters.SetTime(1, 0);
			AssertCalculation(parameters, 30m, "6 M3 x Day (6 Cubic Meter(s) x 1 Day(s)) @ AUD 5.00/M3 x Day");

			parameters.SetTime(14, 0);
			AssertCalculation(parameters, 420m, "84 M3 x Day (6 Cubic Meter(s) x 14 Day(s)) @ AUD 5.00/M3 x Day");

			parameters.SetTime(16, 0);
			AssertCalculation(parameters, 486m, "6 M3 x Day (6 Cubic Meter(s) x 1 Day(s)) @ AUD 6.00/M3 x Day + 90 M3 x Day (6 Cubic Meter(s) x 15 Day(s)) @ AUD 5.00/M3 x Day");
		}

		public void TestCalculationWM()
		{
			DocumentsDataRegistry.Instance.FreightChargesConversionFactorDisplayOption.SetValue(Enterprise.ZArchitecture.Environment.EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "W/M");

			try
			{
				TestCalculation();
			}
			finally
			{
				DocumentsDataRegistry.Instance.FreightChargesConversionFactorDisplayOption.SetValue(Enterprise.ZArchitecture.Environment.EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CON");
			}
		}

		public void TestCalculationUseHigherChargeableLowerRateRule_WhenLineUnitAndItemUnitAreDifferent_ShouldUseBreakUnitForPerUnitCalculationAndLineUnitForOverallCalculation()
		{
			Line.TL_WeightVolume = QuantityUnit.SV;

			var lowestItem = TestCalculator.AddRateLineItem("-", 1, 100);
			lowestItem.TM_BreakWeightVolume = QuantityUnit.HR;
			TestCalculator.AddRateLineItem("+", 1, 100);
			TestCalculator.AddRateLineItem("+", 2, 95);

			TestCalculator.UseHigherChargeableLowerRateRule = true;
			TestCalculator.IsAccumulated = false;

			ChargeCode.AC_ChargeSubGroup = "FUM";
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			var jobServiceInfo = new JobServiceInfo(true, "ORG", "FUM", "Fumigation Service", 1, TimeSpan.FromMinutes(117));
			parameters.Criteria.JobServices.Add(jobServiceInfo);
			AssertCalculation(parameters, 190, "2 Origin Fumigation x Hour (1 Origin Fumigation x 2 Hour(s)) @ AUD 95.00/Origin Fumigation x Hour");
		}

		public void TestCalculationNonAccumulated()
		{
			Line.TL_WeightVolume = QuantityUnit.M3;
			Line.TL_RX_NKCurrency = "AUD";

			TestCalculator.IsAccumulated = false;
			TestCalculator.Minimum = 100m;
			TestCalculator.BaseRate = 20m;
			TestCalculator.AddRateLineItem("-", 10m, 5m, QuantityUnit.DY);
			TestCalculator["+10"] = (ZDecimal)6m;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.ChargeableAmount = new Quantity(4M, QuantityUnit.M3);

			parameters.SetTime(1, 0);
			AssertCalculation(parameters, 100m, "Minimum AUD 100.00");

			parameters.SetTime(9, 0);
			AssertCalculation(parameters, 200m, "Base Rate AUD 20.00 + 36 M3 x Day (4 Cubic Meter(s) x 9 Day(s)) @ AUD 5.00/M3 x Day");

			parameters.SetTime(10, 0);
			AssertCalculation(parameters, 260m, "Base Rate AUD 20.00 + 40 M3 x Day (4 Cubic Meter(s) x 10 Day(s)) @ AUD 6.00/M3 x Day");

			parameters.SetTime(12, 0);
			AssertCalculation(parameters, 308m, "Base Rate AUD 20.00 + 48 M3 x Day (4 Cubic Meter(s) x 12 Day(s)) @ AUD 6.00/M3 x Day");
		}

		public void TestValidateExcludeHolidays()
		{
			TestCalculator.ExcludeHolidays = TimeCalculator.Items.ExcludeSundaysAndPublicHolidays;
			Assert(!TestCalculator.ExcludeHolidaysInfo.HasErrors());

			TestCalculator.ExcludeHolidays = "###";
			Assert(TestCalculator.ExcludeHolidaysInfo.HasErrors());
			AssertEquals("Enter a valid selection.", TestCalculator.ExcludeHolidaysInfo.GetErrors().GetFirstMessage());

			TestCalculator.ExcludeHolidays = "";
			Assert(!TestCalculator.ExcludeHolidaysInfo.HasErrors());

			TestCalculator.ExcludeHolidays = TimeCalculator.Items.ExcludeWeekendsAndPublicHolidays;
			Assert(!TestCalculator.ExcludeHolidaysInfo.HasErrors());
		}

		public override void TestGetCloneCode()
		{
			AssertGetCloneCode(CalculatorCode);
		}

		public override void TestGetCloneLineItems()
		{
			TestCalculator.Minimum = 50m;
			var item = TestCalculator.AddRateLineItem(Calculator.Items.Operator.UNT, 0m, 5m);
			item.TM_BreakWeightVolume = QuantityUnit.DY;

			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			var entry = clientRate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			var source = (CompanyTariffOrCostBasedCalculator)line.Calculator;

			source.PerUnit = 6m;
			source.Minimum = 100m;
			var cloneHelper = new CompanyTariffOrCostLineCloneHelper(line);
			var clonedLine = cloneHelper.CreateClone(Line, Factory);
			AssertEquals(Line.RateLineItems.Count, clonedLine.RateLineItems.Count);
			AssertEquals(150m, clonedLine.GetCalculator<TimeCalculator>().Minimum);
			AssertEquals(11m, clonedLine.GetCalculator<TimeCalculator>().PerUnit);
			AssertEquals(0m, clonedLine.GetCalculator<TimeCalculator>().BaseRate);
			AssertEquals(RatingConstants.Units.DY, clonedLine.GetCalculator<TimeCalculator>().TimeUnit);

			source.Minimum = 0m;
			source.BaseRate = 80m;
			clonedLine = cloneHelper.CreateClone(Line, Factory);
			AssertEquals(Line.RateLineItems.Count + 1, clonedLine.RateLineItems.Count);
			AssertEquals(50m, clonedLine.GetCalculator<TimeCalculator>().Minimum);
			AssertEquals(11m, clonedLine.GetCalculator<TimeCalculator>().PerUnit);
			AssertEquals(80m, clonedLine.GetCalculator<TimeCalculator>().BaseRate);

			TestCalculator.BaseRate = 25m;
			source.PerUnit = 0m;
			source.PerUnitPercent = 20m;
			source.Minimum = 10m;
			source.Percent = 20m;
			source.PerUnitPercent = 20m;
			source.CalculationOrder = Calculator.Items.PercentFirst;
			clonedLine = cloneHelper.CreateClone(Line, Factory);
			AssertEquals(Line.RateLineItems.Count, clonedLine.RateLineItems.Count);
			AssertEquals(70m, clonedLine.GetCalculator<TimeCalculator>().Minimum);
			AssertEquals(6m, clonedLine.GetCalculator<TimeCalculator>().PerUnit);
			AssertEquals(110m, clonedLine.GetCalculator<TimeCalculator>().BaseRate);
			AssertEquals(RatingConstants.Units.DY, clonedLine.GetCalculator<TimeCalculator>().TimeUnit);

			source.CalculationOrder = Calculator.Items.IncreaseFirst;
			clonedLine = cloneHelper.CreateClone(Line, Factory);
			AssertEquals(72m, clonedLine.GetCalculator<TimeCalculator>().Minimum);
			AssertEquals(6m, clonedLine.GetCalculator<TimeCalculator>().PerUnit);
			AssertEquals(126m, clonedLine.GetCalculator<TimeCalculator>().BaseRate);
			AssertEquals(RatingConstants.Units.DY, clonedLine.GetCalculator<TimeCalculator>().TimeUnit);

			source.Percent = 0m;
			source.PerUnitPercent = 0m;
			source.BaseRate = 70m;
			source["-100"] = (ZDecimal)2m;
			source["+100"] = (ZDecimal)1m;
			clonedLine = cloneHelper.CreateClone(Line, Factory);
			AssertEquals(95m, clonedLine.GetCalculator<TimeCalculator>().BaseRate);
			AssertEquals(7m, clonedLine.Calculator["-100"]);
			AssertEquals(6m, clonedLine.Calculator["+100"]);
			AssertEquals(RatingConstants.Units.DY, clonedLine.GetCalculator<TimeCalculator>().TimeUnit);

			Line.RateLineItems.RemoveAndDelete(Calculator.Items.Operator.MIN, Calculator.Items.Operator.BAS, Calculator.Items.Operator.UNT);
			item = TestCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 45m, 6m);
			item.TM_BreakWeightVolume = QuantityUnit.HR;
			TestCalculator["+45"] = (ZDecimal)5m;
			TestCalculator["+100"] = (ZDecimal)4m;
			TestCalculator["+250"] = (ZDecimal)3m;

			clonedLine = cloneHelper.CreateClone(Line, Factory);
			AssertEquals(70m, clonedLine.GetCalculator<TimeCalculator>().BaseRate);
			AssertEquals(8m, clonedLine.Calculator["-45"]);
			AssertEquals(7m, clonedLine.Calculator["+45"]);
			AssertEquals(5m, clonedLine.Calculator["+100"]);
			AssertEquals(4m, clonedLine.Calculator["+250"]);
			AssertEquals(RatingConstants.Units.HR, clonedLine.GetCalculator<TimeCalculator>().TimeUnit);

			Line.RateLineItems.RemoveAndDelete(Calculator.Items.Operator.Minus, Calculator.Items.Operator.Plus);

			item = TestCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 250m, 12m);
			item.TM_BreakWeightVolume = QuantityUnit.HR;
			TestCalculator["+250"] = (ZDecimal)11m;
			TestCalculator["+500"] = (ZDecimal)8m;

			clonedLine = cloneHelper.CreateClone(Line, Factory);
			AssertEquals(70m, clonedLine.GetCalculator<TimeCalculator>().BaseRate);
			AssertEquals(14m, clonedLine.Calculator["-100"]);
			AssertEquals(13m, clonedLine.Calculator["+100"]);
			AssertEquals(12m, clonedLine.Calculator["+250"]);
			AssertEquals(9m, clonedLine.Calculator["+500"]);
			AssertEquals(RatingConstants.Units.HR, clonedLine.GetCalculator<TimeCalculator>().TimeUnit);
		}

		public void TestExcludedHolidays()
		{
			TestCalculator.ExcludeHolidays = "";
			AssertEquals((TimeInfo.Exclusion)0, TestCalculator.ExcludedHolidays);

			TestCalculator.ExcludeHolidays = TimeCalculator.Items.ExcludeWeekendsAndPublicHolidays;
			AssertEquals(TimeInfo.Exclusion.WeekendsPublicHolidays, TestCalculator.ExcludedHolidays);

			TestCalculator.ExcludeHolidays = TimeCalculator.Items.ExcludeSundaysAndPublicHolidays;
			AssertEquals(TimeInfo.Exclusion.Sundays | TimeInfo.Exclusion.PublicHolidays, TestCalculator.ExcludedHolidays);
		}

		public void TestAutoRateDescription()
		{
			var date1 = new ZDateTime(2005, 1, 1).ToShortDateString();
			var date2 = new ZDateTime(2005, 1, 10).ToShortDateString();
			Criteria.SetTime(new TimeInfo(new ZDateTime(2005, 1, 1), new ZDateTime(2005, 1, 10)));
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			AssertEquals("Test Rate", TestCalculator.AutoRateDescription(parameters));

			Line.ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSStorage;
			AssertEquals("Test Rate for 6 days (" + date1 + " - " + date2 + ")", TestCalculator.AutoRateDescription(parameters));

			Line.ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.CFSShipment;
			Line.ChargeCode.AC_ChargeSubGroup = ChargeCodeSubGroupList.Storage;
			AssertEquals("Test Rate for 6 days (" + date1 + " - " + date2 + ")", TestCalculator.AutoRateDescription(parameters));

			TestCalculator.ExcludeHolidays = "";
			AssertEquals("Test Rate for 10 days (" + date1 + " - " + date2 + ")", TestCalculator.AutoRateDescription(parameters));

			Criteria.SetTime(new TimeInfo(1, 11, 0));
			AssertEquals("Test Rate for 2 days", TestCalculator.AutoRateDescription(parameters));

			Criteria.SetTime(new TimeInfo(0, 0, 0));
			AssertEquals("Test Rate", TestCalculator.AutoRateDescription(parameters));
		}

		public void TestAutoRateDescription_DocketReferenceNotPrintedForWhsStorage()
		{
			Criteria.SetTime(new TimeInfo(new ZDateTime(2005, 1, 1), new ZDateTime(2005, 1, 10)));
			Criteria.ConsumerType = JobInvoicingConsumerTypes.WarehouseStorage;
			Criteria.RateableMeasures.CreateWarehouseDocketLines(null, false, true, RateableMeasureSet.WarehouseProductOptionalAttributes.None);
			Criteria.RateableMeasures.AddWarehouseDocketStorageLine((0, null), (0, null), 1, ZGuid.Empty, "DOCKET1", ZGuid.Empty, "");

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			var filteredParams = parameters.CreatedFilteredParametersByDocket_ForTest("DOCKET1");

			AssertEquals(true, TestCalculator.AutoRateDescription(filteredParams).Contains("DOCKET1"));

			Line.ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSStorage;
			AssertEquals("Whs Storage charges should never contain a Docket Reference", false, TestCalculator.AutoRateDescription(filteredParams).Contains("DOCKET1"));
		}

		public void TestExcludeHolidaysItemCloning()
		{
			TestCalculator.ExcludeHolidays = TimeCalculator.Items.ExcludeSundaysAndPublicHolidays;
			var clone = Line.Clone(Line.Parent.RateLines);

			clone.RateLineItems.Clone(Line);

			AssertEquals(TimeCalculator.Items.ExcludeSundaysAndPublicHolidays, ((TimeCalculator)clone.Calculator).ExcludeHolidays);
		}

		public override void TestCalculationWithCallForPricingFlag()
		{
			Line.TL_WeightVolume = "M3";
			Line.TL_RX_NKCurrency = "AUD";
			Line.RateLineItems.RemoveAndDelete(Calculator.Items.Operator.MIN, Calculator.Items.Operator.BAS, Calculator.Items.Operator.UNT);

			var calculatorParams = new AutoRatingCalculatorParametersForTesting(Criteria);

			TestCalculator["+15"] = (ZDecimal)6m;
			var plus15 = TestCalculator.RateLineItems[TestCalculator.RateLineItems.Count - 1];
			TestCalculator.AddRateLineItem("-", 15m, 5m, QuantityUnit.DY);
			plus15.TM_CallForPricing = true;
			plus15.TM_Text = "Call us for pricing";
			TestCalculator.IsAccumulated = false;

			calculatorParams.ChargeableAmount = new Quantity(6M, QuantityUnit.M3);

			calculatorParams.SetTime(16, 0);
			using (_Rating.Start(new LoggerDecorator()))
			{
				AssertExceptionThrown<AutoRater.CallForPriceException>(() => TestCalculator.Calculate(calculatorParams));
			}
		}

		public void TestTimeCalculatorIsBreakUnitReadonly()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AU", "");
			var rateLine = rateEntry.AddRateLine("ODOC", TimeCalculator.Code, QuantityUnit.CN);
			rateLine.RateLineItems.RemoveAndDeleteAll();
			var item = rateLine.RateLineItems.AddNew();

			item.TM_Type = Calculator.Items.Operator.Minus;
			AssertEquals(false, item.TM_BreakWeightVolumeInfo.ReadOnly);

			item.TM_Type = Calculator.Items.Operator.MIN;
			AssertEquals(true, item.TM_BreakWeightVolumeInfo.ReadOnly);

			item.TM_Type = Calculator.Items.Operator.UNT;
			AssertEquals(false, item.TM_BreakWeightVolumeInfo.ReadOnly);

			item.TM_Type = Calculator.Items.Operator.Plus;
			item.TM_Break = 50m;
			AssertEquals(true, item.IsLowestPlus());
			AssertEquals(false, item.TM_BreakWeightVolumeInfo.ReadOnly);

			item = rateLine.RateLineItems.AddNew();
			item.TM_Type = Calculator.Items.Operator.Plus;
			item.TM_Break = 150m;
			AssertEquals(false, item.IsLowestPlus());
			AssertEquals(true, item.TM_BreakWeightVolumeInfo.ReadOnly);
		}

		#region Calculator Description Conversion

		public void TestCalculate_WhenAddingBasOperatorLine_ThenItIsPrintedCorrectlyInCalculatorDescriptionAttribute()
		{
			Entry.TI_Mode = RateMode.LCL;

			Line.TL_AC = Env.Registry.FreightChargeCode;
			Line.TL_RateCalculator = TimeCalculator.Code;
			Line.TL_RX_NKCurrency = CurrencyCodes.UnitedStates;
			Line.TL_WeightVolume = QuantityUnit.DY;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.SetTime(7, 0);
			parameters.ChargeableAmount = new Quantity(40.0m, QuantityUnit.KG);

			Line.RateLineItems.RemoveAndDeleteAll();

			AssertCalculatorDescriptionAfterCalculating(parameters, Line, string.Empty, "It should not contain calculator description");

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.BAS, 0m, 3m);
			var expected = @"BAS. Rate USD 3";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with BAS");

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.MIN, 0m, 100m);
			expected = @"BAS. Rate USD 3 +
MIN. Rate USD 100.";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with BAS/MIN");

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.MAX, 0m, 300m);
			expected = @"BAS. Rate USD 3 +
MIN. Rate USD 100, MAX. Rate USD 300.";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with BAS/MIN/MAX");
		}

		public void TestCalculate_WhenAddingMinusPlusOperatorLine_ThenItIsPrintedCorrectlyInCalculatorDescriptionAttribute()
		{
			Entry.TI_Mode = RateMode.LCL;

			Line.TL_AC = Env.Registry.FreightChargeCode;
			Line.TL_RateCalculator = TimeCalculator.Code;
			Line.TL_RX_NKCurrency = CurrencyCodes.UnitedStates;
			Line.TL_WeightVolume = QuantityUnit.DY;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.SetTime(7, 0);
			parameters.ChargeableAmount = new Quantity(40.0m, QuantityUnit.KG);

			Line.RateLineItems.RemoveAndDeleteAll();

			AssertCalculatorDescriptionAfterCalculating(parameters, Line, string.Empty, "It should not contain calculator description");

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 20m, 8m);
			var expected = @"USD 8/Day";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with PLUS");

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 10m, 9m);
			expected = @"Unit total < 10 Day(s) USD 9/Day
Unit total >= 10 Day(s) USD 9/Day
Unit total >= 20 Day(s) USD 8/Day";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with multiple PLUS");

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 10m, 10m);
			expected = @"Unit total < 10 Day(s) USD 10/Day
Unit total >= 10 Day(s) USD 9/Day
Unit total >= 20 Day(s) USD 8/Day";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with MINUS/PLUS");

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 30m, 0m);
			expected = @"Unit total < 10 Day(s) USD 10/Day
Unit total >= 10 Day(s) USD 9/Day
Unit total >= 20 Day(s) USD 8/Day";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with zero rate or base in MINUS/PLUS");

			Line.RateLineItems.RemoveAndDeleteAll();

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 20m, -8m);
			expected = @"USD -8/Day";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with negative number in PLUS");

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 10m, -9m);
			expected = @"Unit total < 10 Day(s) USD -9/Day
Unit total >= 10 Day(s) USD -9/Day
Unit total >= 20 Day(s) USD -8/Day";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with negative number in multiple PLUS");

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 10m, -10m);
			expected = @"Unit total < 10 Day(s) USD -10/Day
Unit total >= 10 Day(s) USD -9/Day
Unit total >= 20 Day(s) USD -8/Day";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with negative number in MINUS/PLUS");
		}

		public void TestCalculate_WhenAddingNonBasMinusPlusBasOperatorLine_ThenItIsPrintedCorrectlyInCalculatorDescriptionAttribute()
		{
			Entry.TI_Mode = RateMode.LCL;

			Line.TL_AC = Env.Registry.FreightChargeCode;
			Line.TL_RateCalculator = TimeCalculator.Code;
			Line.TL_RX_NKCurrency = CurrencyCodes.UnitedStates;
			Line.TL_WeightVolume = QuantityUnit.DY;

			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.SetTime(7, 0);
			parameters.ChargeableAmount = new Quantity(40.0m, QuantityUnit.KG);

			Line.RateLineItems.RemoveAndDeleteAll();

			AssertCalculatorDescriptionAfterCalculating(parameters, Line, string.Empty, "It should not contain calculator description");

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.UNT, 0m, 3m);
			var expected = "USD 3/Day.";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with UNT");

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.MIN, 0m, 5m);
			expected = "USD 3/Day, MIN. Rate USD 5.";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with UNT/MIN");

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.MAX, 0m, 300m);
			expected = "USD 3/Day, MIN. Rate USD 5, MAX. Rate USD 300.";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with UNT/MIN/MAX");

			TestCalculator.RateLineBizO.RateLineItems[0].TM_BreakWeightVolume = QuantityUnit.CN;
			expected = "USD 3/Container/Day, MIN. Rate USD 5, MAX. Rate USD 300.";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with UNT/MIN/MAX and multiple units");
		}

		public void TestCalculate_WhenAddingAllTypesOfLines_ThenItIsPrintedCorrectlyInCalculatorDescriptionAttribute()
		{
			Entry.TI_Mode = RateMode.LCL;

			Line.TL_AC = Env.Registry.FreightChargeCode;
			Line.TL_RateCalculator = TimeCalculator.Code;
			Line.TL_RX_NKCurrency = CurrencyCodes.UnitedStates;
			Line.TL_WeightVolume = QuantityUnit.DY;

			var measures = Criteria.RateableMeasures;
			measures.AddContainerWithNumber(ZGuid.Empty, "CONT00001", new MeasureInfo.ContainerInfo(100m, Weight.Pounds, 15m, Volume.CubicMetres, 1, 1, "CONT00001"));
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.SetTime(7, 0);

			Line.RateLineItems.RemoveAndDeleteAll();

			AssertCalculatorDescriptionAfterCalculating(parameters, Line, string.Empty, "It should not contain calculator description");

			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Minus, 10m, 10m);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 20m, 8m);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.Plus, 10m, 9m);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.BAS, 0m, 3m);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.MIN, 0m, 100m);
			TestCalculator.AddRateLineItem(Calculator.Items.Operator.MAX, 0m, 300m);
			var expected = @"BAS. Rate USD 3 +
Unit total < 10 Day(s) USD 10/Day
Unit total >= 10 Day(s) USD 9/Day
Unit total >= 20 Day(s) USD 8/Day
MIN. Rate USD 100, MAX. Rate USD 300.";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with BAS/MIN/MAX/MINUS/PLUS");

			TestCalculator.UseInclusiveBreaks = true;
			expected = @"BAS. Rate USD 3 +
Unit total <= 10 Day(s) USD 10/Day
Unit total > 10 Day(s) USD 9/Day
Unit total > 20 Day(s) USD 8/Day
MIN. Rate USD 100, MAX. Rate USD 300.";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with BAS/MIN/MAX/MINUS/PLUS with inclusive breaks");

			TestCalculator.IsAccumulated = true;
			expected = @"BAS. Rate USD 3 +
Unit portion <= 10 Day(s) USD 10/Day
Unit portion > 10 Day(s) USD 9/Day
Unit portion > 20 Day(s) USD 8/Day
MIN. Rate USD 100, MAX. Rate USD 300.";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with MIN/MAX/BAS/MINUS/2PLUS with cumulative calculation");

			TestCalculator.UseHigherChargeableLowerRateRule = true;
			expected = @"BAS. Rate USD 3 +
Unit portion <= 10 Day(s) USD 10/Day
Unit portion > 10 Day(s) USD 9/Day
Unit portion > 20 Day(s) USD 8/Day
MIN. Rate USD 100, MAX. Rate USD 300, Higher Break Lower Rate is applied.";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with MIN/MAX/BAS/MINUS/2PLUS with HigherChargeableLowerRateRule");

			Line.TL_RX_NKCurrency = CurrencyCodes.Australia;
			Line.TL_WeightVolume = QuantityUnit.KG;

			expected = @"BAS. Rate AUD 3 +
Unit portion <= 10 Day(s) AUD 10/Day/KG
Unit portion > 10 Day(s) AUD 9/Day/KG
Unit portion > 20 Day(s) AUD 8/Day/KG
MIN. Rate AUD 100, MAX. Rate AUD 300, Higher Break Lower Rate is applied.";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with MIN/MAX/BAS/MINUS/2PLUS while using other currency/unit");

			Line.TL_WeightVolume = QuantityUnit.CN;
			TestCalculator.RateLineBizO.RateLineItems[0].TM_BreakWeightVolume = QuantityUnit.KG;

			expected = @"BAS. Rate AUD 3 +
Unit portion <= 10 Kilogram(s) AUD 10/KG/Container
Unit portion > 10 Kilogram(s) AUD 9/KG/Container
Unit portion > 20 Kilogram(s) AUD 8/KG/Container
MIN. Rate AUD 100, MAX. Rate AUD 300, Higher Break Lower Rate is applied.";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description with MIN/MAX/BAS/MINUS/2PLUS while using different units in rate line and rate line item");

			TestCalculator.ExcludeHolidays = TimeCalculator.Items.ExcludeWeekendsAndPublicHolidays;
			expected = @"BAS. Rate AUD 3 +
Unit portion <= 10 Kilogram(s) AUD 10/KG/Container
Unit portion > 10 Kilogram(s) AUD 9/KG/Container
Unit portion > 20 Kilogram(s) AUD 8/KG/Container
MIN. Rate AUD 100, MAX. Rate AUD 300, Higher Break Lower Rate is applied, Exclude Weekends and Public Holidays.";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description description with MIN/MAX/BAS/MINUS/2PLUS while setting ExcludeWeekendsAndPublicHolidays");

			TestCalculator.ExcludeHolidays = TimeCalculator.Items.ExcludeSundaysAndPublicHolidays;
			expected = @"BAS. Rate AUD 3 +
Unit portion <= 10 Kilogram(s) AUD 10/KG/Container
Unit portion > 10 Kilogram(s) AUD 9/KG/Container
Unit portion > 20 Kilogram(s) AUD 8/KG/Container
MIN. Rate AUD 100, MAX. Rate AUD 300, Higher Break Lower Rate is applied, Exclude Sundays and Public Holidays.";
			AssertCalculatorDescriptionAfterCalculating(parameters, Line, expected, "It should contain calculator description description with MIN/MAX/BAS/MINUS/2PLUS while setting ExcludeSundaysAndPublicHolidays");
		}

		#endregion

		#region Implementation

		void AssertCalculatorDescriptionAfterCalculating(AutoRatingCalculatorParametersForTesting param, RateLine line, string expectedDescription, string message = "")
		{
			var result = line.Calculator.Calculate(param).results.Single();
			var line1Attributes = result.Attributes.Attributes;

			var containsExpected = line1Attributes
				.Any(a => a.Code == JobChargeAttribTypeList.Codes.CalculatorDescription && a.Value == expectedDescription);

			Assert(
				$"Expected an attribute with Code '{JobChargeAttribTypeList.Codes.CalculatorDescription}' and Value '{expectedDescription}'. {message}",
				containsExpected
			);
		}

		protected override Type CalculatorType
		{
			get { return typeof(TimeCalculator); }
		}

		protected override string CalculatorCode
		{
			get { return TimeCalculator.Code; }
		}

		new TimeCalculator TestCalculator
		{
			get { return (TimeCalculator)base.TestCalculator; }
		}

		#endregion
	}
}
