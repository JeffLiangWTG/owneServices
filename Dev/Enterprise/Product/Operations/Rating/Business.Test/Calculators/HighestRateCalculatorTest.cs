using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Testing;
using Enterprise.ZArchitecture.Core;
using WiseRates.Api.Model;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class HighestRateCalculatorTest : CalculatorTest
	{
		public void TestShouldApplyUnitAsFreighted()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_AC = chargeCode.PK;
			rateLine.TL_RateCalculator = HighestRateCalculator.Code;
			var calculator = (HighestRateCalculator)CalculatorFactory.GetCalculator(rateLine);

			AssertEquals(true, calculator.EnableAsFreightedMode);

			rateLine.TL_AC = Env.Registry.FreightChargeCode;
			AssertEquals(false, calculator.EnableAsFreightedMode);
		}

		public override void TestList1()
		{
			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry("AIR");
			var rateLine = rateEntry.RateLines.AddNew();
			var chargeCode = Factory.New<AccChargeCode>();
			rateLine.TL_AC = chargeCode.PK;
			rateLine.TL_RateCalculator = HighestRateCalculator.Code;
			var calculator = (HighestRateCalculator)CalculatorFactory.GetCalculator(rateLine);

			AssertEquals(3, calculator.List1.Count);
			AssertEquals(HighestRateCalculator.Items.HighestRate, calculator.List1[0].Code);
			AssertEquals(HighestRateCalculator.Items.AsFreightedHighestRateWhenMin, calculator.List1[1].Code);
			AssertEquals(HighestRateCalculator.Items.AsFreightedDontApplyWhenMin, calculator.List1[2].Code);
		}

		public override void TestCheckOrCreateItems()
		{
			Assert(true);
		}

		public override void TestMapping()
		{
			Assert(true);
		}

		public override void TestGetCloneCode()
		{
			AssertGetCloneCode(CalculatorCode);
		}

		public void TestRateEntryHavingHighestRateCalculator_RateLineActualWeightOrVolumeSelected_WhenCloning_NewRateLineShouldHaveActualWeightOrVolumeSelected()
		{
			AddRateLineItem(Calculator.Items.Operator.MIN, "", 400m, 0m);
			AddRateLineItem(Calculator.Items.Operator.UNT, "KG", 10m, 50m);
			CombineAssertions("Preconditions", () =>
			{
				Assert("Default Actual Weight/Volume status", Line.UseOnlyActualWeightMeasure);
				AssertEquals("Default Actual Percentage", (byte)100, Line.TL_ActualPercentage);
			});

			var clonedEntry = Entry.DeepClone(Entry.Parent.EntryCollections["ORG"]);
			var clonedRateLine = clonedEntry.RateLines.Cast<RateLine>().Single();
			AssertEquals("Cloned rate line should copy Actual Weight/Volume status", true, clonedRateLine.UseOnlyActualWeightMeasure);
			AssertEquals("Cloned rate line should copy Actual Percentage", (byte)100, clonedRateLine.TL_ActualPercentage);

			var clonedCalculator = clonedRateLine.GetCalculator<HighestRateCalculator>();
			AssertNotNull("Should clone HRC", clonedCalculator);

			AssertContainsExactElementsInAnyOrder("Should clone rate line items",
				GetComparableValues(TestCalculator),
				GetComparableValues(clonedCalculator));
		}

		public void TestRateEntryHavingHighestRateCalculator_RateLineActualWeightOrVolumeUnselected_WhenCloning_NewRateLineShouldHaveActualWeightOrVolumeUnselected()
		{
			AddRateLineItem(Calculator.Items.Operator.MIN, "", 400m, 0m);
			AddRateLineItem(Calculator.Items.Operator.UNT, "KG", 10m, 50m);
			CombineAssertions("Preconditions", () =>
			{
				Assert("Default Actual Weight/Volume status", Line.UseOnlyActualWeightMeasure);
				AssertEquals("Default Actual Percentage", (byte)100, Line.TL_ActualPercentage);
			});

			Line.UseOnlyActualWeightMeasure = false;
			AssertEquals("Default Actual Percentage", (byte)0, Line.TL_ActualPercentage);

			var clonedEntry = Entry.DeepClone(Entry.Parent.EntryCollections["ORG"]);
			var clonedRateLine = clonedEntry.RateLines.Cast<RateLine>().Single();
			AssertEquals("Cloned rate line should copy Actual Weight/Volume status", false, clonedRateLine.UseOnlyActualWeightMeasure);
			AssertEquals("Cloned rate line should copy Actual Percentage", (byte)0, clonedRateLine.TL_ActualPercentage);

			var clonedCalculator = clonedRateLine.GetCalculator<HighestRateCalculator>();
			AssertNotNull("Should clone HRC", clonedCalculator);

			AssertContainsExactElementsInAnyOrder("Should clone rate line items",
				GetComparableValues(TestCalculator),
				GetComparableValues(clonedCalculator));
		}

		static IEnumerable<(ZString, ZString, ZDecimal, ZDecimal)> GetComparableValues(Calculator calculator)
		{
			return calculator.RateLineItems.Cast<RateLineItem>().Select(item => (item.TM_Type, item.TM_BreakWeightVolume, item.TM_Value, item.TM_FlatAmount));
		}

		public void TestRequiresWeightVolume()
		{
			AssertEquals("RequiresWeightVolume = False", false, TestCalculator.Line.RequiresWeightVolume());
		}

		public override void TestDocLineAmount()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var rateEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA);

			var rateLine10 = AddHighestRateLine(rateEntry, "FRT", "AUD", (Calculator.Items.Operator.MIN, "", 100m, 0m), (Calculator.Items.Operator.UNT, "KG", 101m, 102m), (Calculator.Items.Operator.UNT, "M3", 103m, 104m));
			var rateLine11 = AddHighestRateLine(rateEntry, "FRT", "AUD", (Calculator.Items.Operator.MIN, "", 110m, 0m), (Calculator.Items.Operator.UNT, "KG", 111m, 112m), (Calculator.Items.Operator.UNT, "M3", 113m, 114m));
			var rateLine12 = AddHighestRateLine(rateEntry, "FRT", "AUD", (Calculator.Items.Operator.MIN, "", 120m, 0m), (Calculator.Items.Operator.UNT, "KG", 121m, 122m), (Calculator.Items.Operator.UNT, "L", 123m, 124m));

			var rateLine20 = AddHighestRateLine(rateEntry, "FRT", "USD", (Calculator.Items.Operator.MIN, "", 200m, 0m), (Calculator.Items.Operator.UNT, "KG", 201m, 202m), (Calculator.Items.Operator.UNT, "M3", 203m, 204m));

			var docLineAmount = rateLine10.Calculator.GetDocLineAmount()
				+ rateLine11.Calculator.GetDocLineAmount()
				+ rateLine12.Calculator.GetDocLineAmount()
				+ rateLine20.Calculator.GetDocLineAmount();

			AssertQuotationLineList
			(
				docLineAmount,
				new[]
				{
					"The Highest Rate Of|||",
					"|AUD|101.00|per KG",
					"|AUD|102.00|",
					"|AUD|103.00|per M3",
					"|AUD|104.00|",
					"Minimum|AUD|100.00|",

					"The Highest Rate Of|||",
					"|AUD|111.00|per KG",
					"|AUD|112.00|",
					"|AUD|113.00|per M3",
					"|AUD|114.00|",

					"The Highest Rate Of|||",
					"|AUD|121.00|per KG",
					"|AUD|122.00|",
					"|AUD|123.00|per L",
					"|AUD|124.00|",

					"The Highest Rate Of|||",
					"|USD|201.00|per KG",
					"|USD|202.00|",
					"|USD|203.00|per M3",
					"|USD|204.00|",
					"Minimum|USD|200.00|"
				}
			);
		}

		static RateLine AddHighestRateLine(RateEntry rateEntry, string chargeCode, string currency, params (string itemType, string unit, decimal rate, decimal flat)[] items)
		{
			var rateLine = rateEntry.AddRateLine(chargeCode, HighestRateCalculator.Code, currencyCode: currency);

			foreach (var item in items)
			{
				var newLineItem = rateLine.RateLineItems.AddNew();
				newLineItem.TM_Type = item.itemType;
				newLineItem.TM_BreakWeightVolume = item.unit;
				newLineItem.TM_RelevantValue = item.rate;
				newLineItem.TM_FlatAmount = item.flat;
			}

			return rateLine;
		}

		public override void TestQuotationLines()
		{
			Line.TL_RX_NKCurrency = "USD";

			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "", "");

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(0, quotationLines.Count);

			var minimumItem = AddRateLineItem(Calculator.Items.Operator.MIN, "", 100m, 0m);
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("The Highest Rate Of|||", quotationLines[1].ToString());
			AssertEquals("Minimum|USD|100.00|", quotationLines[2].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Charge|||", quotationLines[0].ToString());
			AssertEquals("The Highest Rate Of|||", quotationLines[1].ToString());
			AssertEquals("Minimum|USD|100.00|", quotationLines[2].ToString());

			var weightItem = AddRateLineItem(Calculator.Items.Operator.UNT, "KG", 10m, 20m);
			weightItem.TM_UnitMultiple = 1000;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(5, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("The Highest Rate Of|||", quotationLines[1].ToString());
			AssertEquals("Weight Rate|USD|10.00|per 1000 KG", quotationLines[2].ToString());
			AssertEquals("Flat Amount|USD|20.00|", quotationLines[3].ToString());
			AssertEquals("Minimum|USD|100.00|", quotationLines[4].ToString());

			var volumeItem = AddRateLineItem(Calculator.Items.Operator.UNT, "M3", 30m, 40m);
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(7, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("The Highest Rate Of|||", quotationLines[1].ToString());
			AssertEquals("Weight Rate|USD|10.00|per 1000 KG", quotationLines[2].ToString());
			AssertEquals("Flat Amount|USD|20.00|", quotationLines[3].ToString());
			AssertEquals("Volume Rate|USD|30.00|per M3", quotationLines[4].ToString());
			AssertEquals("Flat Amount|USD|40.00|", quotationLines[5].ToString());
			AssertEquals("Minimum|USD|100.00|", quotationLines[6].ToString());

			weightItem.TM_FlatAmount = 0m;
			weightItem.TM_BreakWeightVolume = "T";
			weightItem.TM_UnitMultiple = 10;
			volumeItem.TM_RelevantValue = 0m;
			volumeItem.TM_BreakWeightVolume = "CF";
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(6, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("The Highest Rate Of|||", quotationLines[1].ToString());
			AssertEquals("Weight Rate|USD|10.00|per 10 T", quotationLines[2].ToString());
			AssertEquals("Volume Rate|||", quotationLines[3].ToString());
			AssertEquals("Flat Amount|USD|40.00|", quotationLines[4].ToString());
			AssertEquals("Minimum|USD|100.00|", quotationLines[5].ToString());

			TestCalculator.RateLineItems.Remove(minimumItem);
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(5, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("The Highest Rate Of|||", quotationLines[1].ToString());
			AssertEquals("Weight Rate|USD|10.00|per 10 T", quotationLines[2].ToString());
			AssertEquals("Volume Rate|||", quotationLines[3].ToString());
			AssertEquals("Flat Amount|USD|40.00|", quotationLines[4].ToString());
		}

		protected override void TestQuotationLinesWMCore()
		{
			Line.TL_RX_NKCurrency = "USD";

			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var parentEntry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "", "");

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(0, quotationLines.Count);

			var minimumItem = AddRateLineItem(Calculator.Items.Operator.MIN, "", 100m, 0m);
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("The Highest Rate Of|||", quotationLines[1].ToString());
			AssertEquals("Minimum|USD|100.00|", quotationLines[2].ToString());

			var weightItem = AddRateLineItem(Calculator.Items.Operator.UNT, "KG", 10m, 20m);
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(5, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("The Highest Rate Of|||", quotationLines[1].ToString());
			AssertEquals("Weight Rate|USD|10.00|per W/M", quotationLines[2].ToString());
			AssertEquals("Flat Amount|USD|20.00|", quotationLines[3].ToString());
			AssertEquals("Minimum|USD|100.00|", quotationLines[4].ToString());

			var volumeItem = AddRateLineItem(Calculator.Items.Operator.UNT, "M3", 30m, 40m);
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(7, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("The Highest Rate Of|||", quotationLines[1].ToString());
			AssertEquals("Weight Rate|USD|10.00|per W/M", quotationLines[2].ToString());
			AssertEquals("Flat Amount|USD|20.00|", quotationLines[3].ToString());
			AssertEquals("Volume Rate|USD|30.00|per W/M", quotationLines[4].ToString());
			AssertEquals("Flat Amount|USD|40.00|", quotationLines[5].ToString());
			AssertEquals("Minimum|USD|100.00|", quotationLines[6].ToString());

			weightItem.TM_FlatAmount = 0m;
			weightItem.TM_BreakWeightVolume = "T";
			volumeItem.TM_RelevantValue = 0m;
			volumeItem.TM_BreakWeightVolume = "CF";
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(6, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("The Highest Rate Of|||", quotationLines[1].ToString());
			AssertEquals("Weight Rate|USD|10.00|per W/M", quotationLines[2].ToString());
			AssertEquals("Volume Rate|||", quotationLines[3].ToString());
			AssertEquals("Flat Amount|USD|40.00|", quotationLines[4].ToString());
			AssertEquals("Minimum|USD|100.00|", quotationLines[5].ToString());

			TestCalculator.RateLineItems.Remove(minimumItem);
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(5, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("The Highest Rate Of|||", quotationLines[1].ToString());
			AssertEquals("Weight Rate|USD|10.00|per W/M", quotationLines[2].ToString());
			AssertEquals("Volume Rate|||", quotationLines[3].ToString());
			AssertEquals("Flat Amount|USD|40.00|", quotationLines[4].ToString());
		}

		public void TestCalculation()
		{
			Line.TL_RX_NKCurrency = "USD";

			Criteria.RateableMeasures.SetQuantity(Integration.MeasureType.Weight, 100m, "KG");
			Criteria.RateableMeasures.SetQuantity(Integration.MeasureType.Volume, 20m, "M3");
			var parameters = new AutoRatingCalculatorParametersForTesting(Criteria);
			parameters.AddLineMeasureMatch(Integration.MeasureType.Volume, TestCalculator.RateLineBizO, 0);
			parameters.AddLineMeasureMatch(Integration.MeasureType.Weight, TestCalculator.RateLineBizO, 0);

			var minimumItem = AddRateLineItem(Calculator.Items.Operator.MIN, "", 400m, 0m);
			AssertCalculation(parameters, 400.00m, "MIN USD 400.00");

			var weightItem = AddRateLineItem(Calculator.Items.Operator.UNT, "KG", 10m, 50m);
			AssertCalculation(parameters, 1050.00m, "Base Rate USD 50.00 + 100 Kilogram(s) @ USD 10.00/KG");

			weightItem.TM_BreakWeightVolume = "T";
			AssertCalculation(parameters, 400.00m, "Minimum USD 400.00");

			minimumItem.TM_RelevantValue = 10;
			AssertCalculation(parameters, 51.0m, "Base Rate USD 50.00 + 0.1 Tonne(s) @ USD 10.00/T");

			weightItem.TM_BreakWeightVolume = "KG";
			minimumItem.TM_RelevantValue = 100;
			var volumeItem = AddRateLineItem(Calculator.Items.Operator.UNT, "M3", 50m, 200m);
			AssertCalculation(parameters, 1200.00m, "Base Rate USD 200.00 + 20 Cubic Meter(s) @ USD 50.00/M3");

			volumeItem.TM_RelevantValue = 40;
			AssertCalculation(parameters, 1050.00m, "Base Rate USD 50.00 + 100 Kilogram(s) @ USD 10.00/KG");

			minimumItem.TM_RelevantValue = 2000;
			AssertCalculation(parameters, 2000.00m, "Minimum USD 2000.00");
		}

		public void TestCalculationForWiseLine()
		{
			var wiseLine = new WiseLine(Factory, new Charge())
			{
				TL_AC = Helper.ChargeCodes["ODOC"].PK,
				TL_WeightVolume = "KG",
				TL_RX_NKCurrency = "USD",
				RateCalculatorType = Integration.CalculatorType.HighestRate
			};
			// $10 / 2KG or $250 / M3, factor 50KG / M3
			wiseLine.ChildRateLineItems = new[]
			{
				new WiseLineItem(
					parent: wiseLine, tm_type: Calculator.Items.Operator.UNT, tm_text: string.Empty, tm_relevantValue: 10,
					tm_ac: ZString.Empty, tm_break: 0m, tm_flatAmount: 0m, tm_callForPricing: null, tm_BreakWeightVolume: QuantityUnit.KG, tm_unitMultiple: 2),
				new WiseLineItem(
					parent: wiseLine, tm_type: Calculator.Items.Operator.UNT, tm_text: string.Empty, tm_relevantValue: 250,
					tm_ac: ZString.Empty, tm_break: 0m, tm_flatAmount: 0m, tm_callForPricing: null, tm_BreakWeightVolume: QuantityUnit.M3, tm_unitMultiple: 1),
				new WiseLineItem(
					parent: wiseLine, tm_type: Calculator.Items.RatePickRule, tm_text: "", tm_relevantValue: 0,
					tm_ac: ZString.Empty, tm_break: 0m, tm_flatAmount: 0m, tm_callForPricing: null),
			};

			var wiseEntry = new WiseEntry(new Rate { Origin = "AUSYD", Destination = "UAIEV", ProviderRateId = "McLaren" }, Factory);
			wiseEntry.ChildRateLines = new[] { wiseLine };
			wiseLine.ParentRateEntry = wiseEntry;

			var criteria = new TestRatingCriteria();
			criteria.RateableMeasures.SetQuantity(Integration.MeasureType.Weight, 100, "KG");
			criteria.RateableMeasures.SetQuantity(Integration.MeasureType.Volume, 20, "M3");
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			parameters.AddLineMeasureMatch(Integration.MeasureType.Volume, wiseLine, 0);
			parameters.AddLineMeasureMatch(Integration.MeasureType.Weight, wiseLine, 0);
			AssertCalculation
			(
				expectedAmount: 5000,
				expectedDescription: "20 Cubic Meter(s) @ USD 250.00/M3",
				message: "[100KG @ ($10 / 2KG, factor 50KG / M3)] < [20M3 @ ($250 / M3, factor 50KG / M3)]"
			);

			criteria.RateableMeasures.SetQuantity(Integration.MeasureType.Weight, 1001m, "KG");
			parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			parameters.AddLineMeasureMatch(Integration.MeasureType.Weight, wiseLine, 0);
			AssertCalculation
			(
				expectedAmount: 5005,
				expectedDescription: "1001 Kilogram(s) @ USD 10.00/2 KG",
				message: "[1001KG @ ($10 / 2KG, factor 50KG / M3)] > [20M3 @ ($250 / M3, factor 50KG / M3)]."
			);

			void AssertCalculation(ZDecimal expectedAmount, ZString expectedDescription, string message)
			{
				var (results, error) = wiseLine.Calculator.Calculate(parameters);
				AssertGreaterThan(results.Count(), 0);
				AssertNullOrEmpty("error", error);

				var result = results.Single();
				var autoRateInfo = new AutoRateInfo(result, parameters, Factory);
				var calculatorType = CalculatorType.ToString();

				CombineAssertions(message, () =>
				{
					AssertEquals(calculatorType + " description:", result.Description, expectedDescription);
					AssertEquals(calculatorType + " rounded amount:", Utilities.Round(autoRateInfo.Amount, result.Currency?.Decimals ?? 2), expectedAmount);
				});
			}
		}

		public void TestCalculation_MissingUnits_ShouldError()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AU", "", removeLines: true);
			var line = rateEntry.AddHighestRateCharge("OQUAR", 10m, "", 10m, "");

			Criteria.RateableMeasures.SetQuantity(Integration.MeasureType.Weight, 100m, "KG");
			Criteria.RateableMeasures.SetQuantity(Integration.MeasureType.Volume, 20m, "M3");

			var calcParams = new AutoRatingCalculatorParametersForTesting(Criteria);
			calcParams.AddLineMeasureMatch(Integration.MeasureType.Volume, line, 0);
			calcParams.AddLineMeasureMatch(Integration.MeasureType.Weight, line, 0);

			var calculator = line.Calculator;
			var (_, error) = calculator.Calculate(calcParams);

			AssertEquals("Should have an error", "Missing required Unit value", error);
		}

		public void TestCalculationLog()
		{
			var cc = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);
			cc.AC_RateCalculator = HighestRateCalculator.Code;
			TestCalculator.RateLineBizO.TL_AC = cc.PK;

			Criteria.RateableMeasures.SetQuantity(Integration.MeasureType.Weight, 100m, "KG");
			Criteria.RateableMeasures.SetQuantity(Integration.MeasureType.Volume, 20m, "M3");

			var weightItem = AddRateLineItem(Calculator.Items.Operator.UNT, "KG", 10m, 0m);
			var volumeItem = AddRateLineItem(Calculator.Items.Operator.UNT, "M3", 40m, 0m);

			var calcParams = new AutoRatingCalculatorParametersForTesting(Criteria);
			calcParams.AddLineMeasureMatch(Integration.MeasureType.Volume, volumeItem.Parent, 0);
			calcParams.AddLineMeasureMatch(Integration.MeasureType.Weight, weightItem.Parent, 0);

			var result = Calculate(calcParams);
			AssertEquals("Unit from weight", "KG", result.FreightChargeCodeCalculationLog.Unit);
			AssertEquals(1, result.FreightChargeCodeCalculationLog.Steps.Count);
			AssertEquals("Amount from weight", 1000m, result.FreightChargeCodeCalculationLog.Steps[0].Result);

			volumeItem.TM_RelevantValue = 60m;
			result = Calculate(calcParams);

			AssertEquals("Unit from volume", "M3", result.FreightChargeCodeCalculationLog.Unit);
			AssertEquals(1, result.FreightChargeCodeCalculationLog.Steps.Count);
			AssertEquals("Amount from volume", 1200m, result.FreightChargeCodeCalculationLog.Steps[0].Result);
		}

		public void TestValidateRateOperator()
		{
			var dummyChargeCode = Factory.New<AccChargeCode>();
			dummyChargeCode.AC_RateCalculator = HighestRateCalculator.Code;

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var testEntry = testRate.AddRateEntry("ORG");
			var rateLine = testEntry.RateLines.AddNew();
			rateLine.TL_AC = dummyChargeCode.PK;

			var lineItem1 = rateLine.RateLineItems.AddNew();
			lineItem1.TM_Type = Calculator.Items.Operator.MIN;
			AssertEquals("No Errors", false, lineItem1.TM_TypeInfo.HasErrors());

			var lineItem2 = rateLine.RateLineItems.AddNew();
			lineItem2.TM_Type = Calculator.Items.Operator.MIN;
			AssertEquals("Has Errors", true, lineItem2.TM_TypeInfo.HasErrors());
			AssertEquals("Error Message: DuplicateMINNotAllowed", ErrorMessages.DuplicateMINNotAllowed, lineItem2.TM_TypeInfo.GetErrors().GetFirstMessage());

			lineItem2.TM_Type = Calculator.Items.Operator.UNT;
			AssertEquals("No Errors", false, lineItem2.TM_TypeInfo.HasErrors());

			var lineItem3 = rateLine.RateLineItems.AddNew();
			lineItem3.TM_Type = "XXX";
			AssertEquals("Has Errors", true, lineItem3.TM_TypeInfo.HasErrors());
			AssertEquals("Error Message: InvalidRateOperator", ErrorMessages.InvalidRateOperator, lineItem3.TM_TypeInfo.GetErrors().GetFirstMessage());

			lineItem3.TM_Type = Calculator.Items.Operator.UNT;
			AssertEquals("No Errors", false, lineItem3.TM_TypeInfo.HasErrors());

			var lineItem4 = rateLine.RateLineItems.AddNew();
			lineItem4.TM_Type = Calculator.Items.Operator.UNT;
			AssertEquals("Has Errors", true, lineItem4.TM_TypeInfo.HasErrors());
			AssertEquals("Error Message: MoreThanTwoUNTNotAllowed", ErrorMessages.MoreThanTwoUNTNotAllowed, lineItem4.TM_TypeInfo.GetErrors().GetFirstMessage());

			lineItem4.TM_Type = HighestRateCalculator.Items.RatePickRule;
			lineItem3.Validation.ValidateTM_Type();     //ToDo: remove this line and fix the test. It should clear the red dot automatically when lineItem4 is no longer in conflict. But this kind of validation should be performed in a generic way on the RateLineItemCollection level. LINQ GroupBy should be good to search for duplicates.
			AssertNoErrors(lineItem3.TM_TypeInfo);

			rateLine.RateLineItems.Remove(lineItem4);
			lineItem1.Validation.ValidateTM_Type();     //same here
			lineItem2.Validation.ValidateTM_Type();     //same here
			AssertNoErrors(lineItem1.TM_TypeInfo);
			AssertNoErrors(lineItem2.TM_TypeInfo);
			AssertNoErrors(lineItem3.TM_TypeInfo);
		}

		public void TestValidateMeasureUnits()
		{
			var dummyChargeCode = Factory.New<AccChargeCode>();
			dummyChargeCode.AC_RateCalculator = HighestRateCalculator.Code;

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var testEntry = testRate.AddRateEntry("ORG");
			var rateLine = testEntry.RateLines.AddNew();
			rateLine.TL_AC = dummyChargeCode.PK;

			var lineItem1 = rateLine.RateLineItems.AddNew();
			lineItem1.TM_Type = Calculator.Items.Operator.UNT;
			lineItem1.TM_BreakWeightVolume = "";
			AssertEquals("Has Errors", true, lineItem1.TM_BreakWeightVolumeInfo.HasErrors());
			AssertEquals("Error Message", "Please enter a " + lineItem1.TM_BreakWeightVolumeInfo.Description + ".", lineItem1.TM_BreakWeightVolumeInfo.GetErrors().GetFirstMessage());

			lineItem1.TM_BreakWeightVolume = "XX";
			AssertEquals("Has Errors", true, lineItem1.TM_BreakWeightVolumeInfo.HasErrors());
			AssertEquals(true, lineItem1.TM_BreakWeightVolumeInfo.GetErrors().Contains("Calculator Unit should match Rate Line Unit."));
			AssertEquals(true, lineItem1.TM_BreakWeightVolumeInfo.GetErrors().Contains("Enter a valid " + lineItem1.TM_BreakWeightVolumeInfo.Description + "."));

			lineItem1.TM_BreakWeightVolume = "KG";
			AssertEquals("No Errors", true, lineItem1.TM_BreakWeightVolumeInfo.HasErrors());
			AssertEquals(true, lineItem1.TM_BreakWeightVolumeInfo.GetErrors().Contains("Calculator Unit should match Rate Line Unit."));
			AssertEquals(false, lineItem1.TM_BreakWeightVolumeInfo.GetErrors().Contains("Enter a valid " + lineItem1.TM_BreakWeightVolumeInfo.Description + "."));

			var lineItem2 = rateLine.RateLineItems.AddNew();
			lineItem2.TM_Type = Calculator.Items.Operator.UNT;
			lineItem2.TM_BreakWeightVolume = "M3";
			AssertEquals("No Errors", false, lineItem2.TM_BreakWeightVolumeInfo.HasErrors());

			lineItem2.TM_BreakWeightVolume = "KG";
			AssertEquals("Has Errors", true, lineItem2.TM_BreakWeightVolumeInfo.HasErrors());
			AssertEquals("Error Message: SameUNTNotAllowed", ErrorMessages.SameUNTNotAllowed, lineItem2.TM_BreakWeightVolumeInfo.GetErrors().GetFirstMessage());

			lineItem2.TM_BreakWeightVolume = "M3";
			AssertEquals("No Errors", false, lineItem2.TM_BreakWeightVolumeInfo.HasErrors());
		}

		public override void TestGetCloneLineItems()
		{
			TestCalculator.Minimum = 25m;

			var clientRate = Factory.New<ClientRate>();
			var entry = clientRate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			var source = (CompanyTariffOrCostBasedCalculator)line.Calculator;
			source.PerUnit = 6m;
			source.BaseRate = 80m;

			var cloneHelper = new CompanyTariffOrCostLineCloneHelper(line);
			var clonedLine = cloneHelper.CreateClone(Line, Factory);
			AssertEquals(25m, clonedLine.GetCalculator<HighestRateCalculator>().Minimum);

			source.PerUnit = 0m;
			source.Percent = 20m;
			source.Minimum = 12;
			source.CalculationOrder = CompanyTariffOrCostBasedCalculator.Items.PercentFirst;
			clonedLine = cloneHelper.CreateClone(Line, Factory);
			AssertEquals("25 * 1.2 + 12", 42m, clonedLine.GetCalculator<HighestRateCalculator>().Minimum);

			source.CalculationOrder = CompanyTariffOrCostBasedCalculator.Items.IncreaseFirst;
			clonedLine = cloneHelper.CreateClone(Line, Factory);
			AssertEquals("(25 + 12) * 1.2", 44.4m, clonedLine.GetCalculator<HighestRateCalculator>().Minimum);
		}

		public override void TestPricePerSingleChargeable()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateLine = costing.AddRateEntry("AIR", "LSE", "AUSYD", "NZAKL").AddRateLine("FRT", HighestRateCalculator.Code, "KG", "AUD");
			rateLine.RateLineItems.RemoveAndDeleteAll();

			var minItem = rateLine.Calculator.AddRateLineItem(Calculator.Items.Operator.MIN, 0m, 400m, 0m);
			var unitItem = rateLine.Calculator.AddRateLineItem(Calculator.Items.Operator.UNT, 0m, 3m, 30m);
			unitItem.TM_BreakWeightVolume = "KG";

			AssertEquals("AUD 400.0000", rateLine.ParentRateEntry.AllInCost());
			AssertEquals("AUD 3.0000/KG", rateLine.ParentRateEntry.FreightRatePerChargeableUnit());
		}

		#region Implementation

		RateLineItem AddRateLineItem(ZString type, ZString breakWeightVolume, ZDecimal rate, ZDecimal flatAmount)
		{
			var newLineItem = TestCalculator.RateLineBizO.RateLineItems.AddNew();
			newLineItem.TM_Type = type;
			newLineItem.TM_BreakWeightVolume = breakWeightVolume;
			newLineItem.TM_RelevantValue = rate;
			newLineItem.TM_FlatAmount = flatAmount;

			return newLineItem;
		}

		protected override Type CalculatorType
		{
			get { return typeof(HighestRateCalculator); }
		}

		protected override string CalculatorCode
		{
			get { return HighestRateCalculator.Code; }
		}

		new HighestRateCalculator TestCalculator
		{
			get { return (HighestRateCalculator)base.TestCalculator; }
		}

		#endregion
	}
}
