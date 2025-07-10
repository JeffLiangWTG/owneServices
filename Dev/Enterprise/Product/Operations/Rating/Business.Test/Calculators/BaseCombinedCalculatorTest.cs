using System;
using System.Linq;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.Testing
{
	abstract class BaseCombinedCalculatorTest<T> : CalculatorTest
		where T : BaseCombinedCalculator
	{
		protected virtual int NumberOfRateLineItemsAfterInitialization { get { return 4; } }

		public override void TestCheckOrCreateItems()
		{
			AssertNull(Line.RateLineItems.FindByTM_Type(BaseCombinedCalculator.Items.UseAccumulated));
			AssertNull(Line.RateLineItems.FindByTM_Type(BaseCombinedCalculator.Items.HigherChargeableLowerRate));
			AssertNull(Line.RateLineItems.FindByTM_Type(BaseCombinedCalculator.Items.UseInclusiveBreaks));
			AssertNull(Line.RateLineItems.FindByTM_Type(BaseCombinedCalculator.Items.BreaksPer));

			InitialiseTestCalculator();
			AssertEquals(NumberOfRateLineItemsAfterInitialization, Line.RateLineItems.Count);
			AssertNotNull(Line.RateLineItems.FindByTM_Type(BaseCombinedCalculator.Items.UseAccumulated));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(BaseCombinedCalculator.Items.HigherChargeableLowerRate));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(BaseCombinedCalculator.Items.UseInclusiveBreaks));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(BaseCombinedCalculator.Items.BreaksPer));

			AssertEquals(Line.RateLineItems.FindByTM_Type(BaseCombinedCalculator.Items.UseAccumulated).TM_TextInfo, TestCalculator.Bool1Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(BaseCombinedCalculator.Items.HigherChargeableLowerRate).TM_TextInfo, TestCalculator.Bool2Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(BaseCombinedCalculator.Items.UseInclusiveBreaks).TM_TextInfo, TestCalculator.Bool3Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(BaseCombinedCalculator.Items.BreaksPer).TM_TextInfo, TestCalculator.String3Info);
		}

		public override void TestMapping()
		{
			TestMapping(BaseCombinedCalculator.Items.UseAccumulated, "Bool1");
			TestMapping(BaseCombinedCalculator.Items.HigherChargeableLowerRate, "Bool2");
			TestMapping(BaseCombinedCalculator.Items.UseInclusiveBreaks, "Bool3");
			TestMapping(BaseCombinedCalculator.Items.BreaksPer, "String3");
		}

		public override void TestList3()
		{
			AssertEquals("List3 Type", typeof(CodeDescriptionPairList), TestCalculator.List3.GetType());
			AssertEquals("List3 Count", 2, TestCalculator.List3.Count);
			AssertEquals("CTT, CTN", TestCalculator.List3.CodesAsString);
		}

		public void TestUseInclusiveBreaksWhenAccumulated()
		{
			var newLine = Line.Parent.RateLines.AddNew();
			newLine.TL_AC = ChargeCode.PK;
			newLine.TL_RateDesc = "New Test Rate 1";
			var testCalc = newLine.Calculator as T;

			testCalc.IsAccumulated = true;
			AssertEquals(true, testCalc.UseInclusiveBreaks);

			testCalc.UseInclusiveBreaks = false;

			testCalc.Bool1 = true;
			AssertEquals(true, testCalc.UseInclusiveBreaks);
		}

		public virtual void TestIsAccumulatedAndUseHigherChargeableLowerRateRuleWhenBreaksPerIsNotEmpty()
		{
			var newLine = Line.Parent.RateLines.AddNew();
			newLine.TL_AC = ChargeCode.PK;
			newLine.TL_RateDesc = "New Test Rate 1";
			newLine.TL_WeightVolume = QuantityUnit.KG;
			var testCalc = newLine.Calculator as T;

			Assert("Pre-condition:BreaksPerInfo should not be readonly", !testCalc.BreaksPerInfo.ReadOnly);
			testCalc.BreaksPer = BaseCombinedCalculator.Items.BreaksPerContainerTypeOrClass;
			Assert(testCalc.IsAccumulatedInfo.ReadOnly);
			Assert("IsAccumulated", testCalc.IsAccumulated);
			Assert(testCalc.UseHigherChargeableLowerRateRuleInfo.ReadOnly);
			Assert("UseHigherChargeableLowerRateRule", !testCalc.UseHigherChargeableLowerRateRule);

			testCalc.IsAccumulated = false;//not possible from UI as it's readonly
			Assert(testCalc.IsAccumulated);

			testCalc.UseHigherChargeableLowerRateRule = true;//not possible from UI as it's readonly
			Assert(!testCalc.UseHigherChargeableLowerRateRule);

			testCalc.BreaksPer = string.Empty;
			Assert(!testCalc.IsAccumulatedInfo.ReadOnly);
			Assert(!testCalc.IsAccumulated);
			Assert(!testCalc.UseHigherChargeableLowerRateRuleInfo.ReadOnly);
			Assert(testCalc.UseHigherChargeableLowerRateRule);
		}

		public virtual void TestDefaultUseHigherRule()
		{
			var newLine = Line.Parent.RateLines.AddNew();
			newLine.TL_AC = ChargeCode.PK;
			newLine.TL_RateDesc = "New Test Rate 1";
			var testCalc = newLine.Calculator as T;

			AssertEquals("Default Rule taken from registry", false, testCalc.UseHigherChargeableLowerRateRule);

			RatingDataRegistry.Instance.UseHigherWeightOrUnitLowerRateRuleDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			newLine = Line.Parent.RateLines.AddNew();
			newLine.TL_AC = ChargeCode.PK;
			newLine.TL_RateDesc = "New Test Rate 2";
			testCalc = newLine.Calculator as T;

			AssertEquals("Default Rule taken from registry", true, testCalc.UseHigherChargeableLowerRateRule);
		}

		public void TestInclusiveBreaksIsNotChangedAfterResettingUseAccumulated()
		{
			Assert(!TestCalculator.UseInclusiveBreaks);

			TestCalculator.IsAccumulated = true;
			Assert(TestCalculator.UseInclusiveBreaks);

			TestCalculator.IsAccumulated = false;
			Assert(!TestCalculator.UseInclusiveBreaks);

			TestCalculator.UseInclusiveBreaks = true;

			TestCalculator.IsAccumulated = true;
			Assert(TestCalculator.UseInclusiveBreaks);

			TestCalculator.IsAccumulated = false;
			Assert(TestCalculator.UseInclusiveBreaks);
		}

		public virtual void TestCalculationWithCallForPricingFlag()
		{
			Line.TL_WeightVolume = "KG";
			Line.TL_RX_NKCurrency = "AUD";
			var calculatorParams = new AutoRatingCalculatorParametersForTesting(Criteria);

			TestCalculator.RateLineItems.RemoveAndDeleteAll();
			TestCalculator.AddRateLineItem("-", 45m, 2.00m, 0m);

			var plus45 = TestCalculator.AddRateLineItem("+", 45m, 0m, 0m);
			plus45.TM_CallForPricing = true;
			plus45.TM_Text = "Call us for pricing";

			calculatorParams.ChargeableAmount = new Quantity(60m, QuantityUnit.KG);
			using (_Rating.Start(new LoggerDecorator()))
			{
				AssertExceptionThrown<AutoRater.CallForPriceException>(() => TestCalculator.Calculate(calculatorParams));
			}
		}

		public void TestLogging_MinimumChargeableWeightDescription()
		{
			Line.TL_AC = ChargeCode.PK;
			Line.TL_RateCalculator = CombinedCalculator.Code;
			Line.TL_WeightVolume = "KG";

			var calculator = (CombinedCalculator)Line.Calculator;
			calculator.UseInclusiveBreaks = true;
			calculator.AddRateLineItem("-", 100, 20);
			calculator.AddRateLineItem("+", 100, 30);
			calculator.AddRateLineItem("+", 200, 40);
			calculator.AddRateLineItem("MIN", 100, 0);

			var calculatorParams = new AutoRatingCalculatorParametersForTesting(Criteria);
			calculatorParams.ChargeableAmount = new Quantity(120, "KG");
			var results =
				calculator
				.Calculate(calculatorParams).results
				.Select(x => (string)x.Description);

			var expectedResults1 = new[] {
				"120 Kilogram(s) @ AUD 30.00/KG"
			};
			AssertContainsExactElementsInAnyOrder(
				"The Chargeable has NOT been modified",
				expectedResults1,
				results
			);

			// The below text is what is written out to the Autorating Log
			var logs1 = ((LoggerDecorator)calculatorParams.Logger).GetLogs();
			AssertEquals(
				"The chargeable has not changed, so, there is no message about it",
				0,
				logs1.Count()
			);

			calculatorParams = new AutoRatingCalculatorParametersForTesting(Criteria);
			calculatorParams.ChargeableAmount = new Quantity(20, "KG");
			results =
				calculator
				.Calculate(calculatorParams).results
				.Select(x => (string)x.Description);

			var expectedResults2 = new[] {
				"Min 100 Kilogram(s) @ AUD 20.00/KG"
			};
			AssertContainsExactElementsInAnyOrder(
				"The Chargeable has been adjusted as per the MIN break to be a bigger value",
				expectedResults2,
				results
			);

			// The below text is what is written out to the Autorating Log
			var logs2 = ((LoggerDecorator)calculatorParams.Logger).GetLogs();
			var expectedLogs = new[] {
				"Information: Minimum Chargeable Weight 100 KG is used instead of Chargeable Weight 20 KG because charge CCC uses Minimum Weight as the Minimum Chargeable."
			};
			AssertContainsExactElementsInAnyOrder(
				"The log message should match the expected output",
				expectedLogs,
				logs2
			);
		}

		public void TestIsSlidingBreakUnitReadOnly()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AU", "");

			var rateLine1 = rateEntry.AddRateLine("OBILL", CalculatorCode, Core.Constants.PkgUnit.Unit);

			var isCalculatorUsingSlidingItems = !rateLine1.Lookups.WeightBreaks.Cast<CodeDescriptionPair>().Select(x => x.Code).Contains(Calculator.Items.Operator.Minus);
			if (isCalculatorUsingSlidingItems || CalculatorCode == TimeCalculator.Code)
			{
				Assert("Calculator does not allow sliding items so this test doesn't no apply ", true);
				return;
			}

			var minusItem1 = rateLine1.Calculator.AddRateLineItem(Calculator.Items.Operator.Minus, 45, 6);
			var plusItem1 = rateLine1.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 45, 8);

			var rateLine2 = rateEntry.AddRateLine("ODOC", CalculatorCode, QuantityUnit.KM);
			var minusItem2 = rateLine2.Calculator.AddRateLineItem(Calculator.Items.Operator.Minus, 45, 9);
			var plusItem2 = rateLine2.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 45, 7);

			var rateLine3 = rateEntry.AddRateLine("OCART", CalculatorCode, QuantityUnit.KG);
			var minusItem3 = rateLine3.Calculator.AddRateLineItem(Calculator.Items.Operator.Minus, 45, 1);
			var plusItem3 = rateLine3.Calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 45, 2);

			CombineAssertions("Break units are only allowed on the first plus/minus item depending on the rateline's chargeable unit", () =>
			{
				AssertEquals(false, minusItem1.TM_BreakWeightVolumeInfo.ReadOnly);
				AssertEquals(true, plusItem1.TM_BreakWeightVolumeInfo.ReadOnly);

				AssertEquals("Distance chargeable units should not allow break units", true, minusItem2.TM_BreakWeightVolumeInfo.ReadOnly);
				AssertEquals(true, plusItem2.TM_BreakWeightVolumeInfo.ReadOnly);

				AssertEquals(false, minusItem3.TM_BreakWeightVolumeInfo.ReadOnly);
				AssertEquals(true, plusItem3.TM_BreakWeightVolumeInfo.ReadOnly);
			});

			minusItem1.TM_BreakWeightVolume = Core.Constants.PkgUnit.Box;
			minusItem3.TM_BreakWeightVolume = Core.Constants.PkgUnit.Box;

			AssertHasErrors("Top level package units are not allowed for break units", minusItem1.TM_BreakWeightVolumeInfo);
			AssertHasErrors("Should only allow distance units", minusItem3.TM_BreakWeightVolumeInfo);

			minusItem1.TM_BreakWeightVolume = QuantityUnit.KG;
			minusItem3.TM_BreakWeightVolume = QuantityUnit.KG;

			AssertNoErrors(minusItem1.TM_BreakWeightVolumeInfo);
			AssertHasErrors(minusItem3.TM_BreakWeightVolumeInfo);

			minusItem1.TM_BreakWeightVolume = QuantityUnit.MI;
			minusItem3.TM_BreakWeightVolume = QuantityUnit.MI;

			AssertHasErrors("Distance is not a valid break units for a top level pack", minusItem1.TM_BreakWeightVolumeInfo);
			AssertNoErrors(minusItem3.TM_BreakWeightVolumeInfo);
		}
	}
}
