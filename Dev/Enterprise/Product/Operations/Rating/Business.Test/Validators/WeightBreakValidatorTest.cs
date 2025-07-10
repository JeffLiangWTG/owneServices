using CargoWise.EntityFramework.Testing;

namespace Enterprise.Rating.Business.Testing
{
	public class WeightBreakValidatorTest : TestCaseWithFactory
	{
		public void TestRequiredIfRateOperatorIsMinusOrPlus()
		{
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line = testRate.AddRateEntry("ORG", "ALL", "AUSYD", "").AddRateLine("ODOC");
			line.TL_RateCalculator = CombinedCalculator.Code;
			var rateLineItem = line.RateLineItems.AddNew();

			rateLineItem.TM_Type = Calculator.Items.Operator.Minus;
			rateLineItem.Validation.ValidateTM_Break();
			AssertHasError(rateLineItem.TM_BreakInfo, ErrorMessages.WeightBreakRequired);
			rateLineItem.TM_Break = 45m;
			AssertNoErrors(rateLineItem.TM_BreakInfo);
			rateLineItem.TM_Break = 0m;
			AssertHasError(rateLineItem.TM_BreakInfo, ErrorMessages.WeightBreakRequired);

			rateLineItem.TM_Type = Calculator.Items.Operator.Plus;
			rateLineItem.Validation.ValidateTM_Break();
			AssertHasError(rateLineItem.TM_BreakInfo, ErrorMessages.WeightBreakRequired);
			rateLineItem.TM_Break = 45m;
			AssertNoErrors(rateLineItem.TM_BreakInfo);
			rateLineItem.TM_Break = 0m;
			AssertHasError(rateLineItem.TM_BreakInfo, ErrorMessages.WeightBreakRequired);

			rateLineItem.TM_Break = 45m;
			rateLineItem.TM_Type = Calculator.Items.Operator.UNT;
			AssertEquals(0m, rateLineItem.TM_Break);
			AssertNoErrors(rateLineItem.TM_BreakInfo);
		}

		public void TestBreakIsNotNegative()
		{
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line = testRate.AddRateEntry("ORG", "ALL", "AUSYD", "").AddRateLine("ODOC");
			line.TL_RateCalculator = CombinedCalculator.Code;
			var rateLineItem = line.RateLineItems.AddNew();

			rateLineItem.TM_Type = Calculator.Items.Operator.Plus;
			rateLineItem.TM_Break = -10m;
			AssertHasErrorContaining(rateLineItem.TM_BreakInfo, "Break cannot be negative");
			rateLineItem.TM_Break = 5m;
			AssertNoError("Changing the break to positive should remove error", rateLineItem.TM_BreakInfo, "Break cannot be negative");

			rateLineItem.TM_Type = Calculator.Items.Operator.Minus;
			rateLineItem.TM_Break = -10m;
			AssertHasErrorContaining(rateLineItem.TM_BreakInfo, "Break cannot be negative");

			rateLineItem.TM_Type = Calculator.Items.Operator.MIN;
			AssertNoError("Error should only occur for rates that allow breaks", rateLineItem.TM_BreakInfo, "Break cannot be negative");
		}

		public void TestMatchingPlusAndMinusWeightBreak()
		{
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line = testRate.AddRateEntry("ORG", "ALL", "AUSYD", "").AddRateLine("ODOC");
			line.TL_RateCalculator = CombinedCalculator.Code;

			var firstItem = line.RateLineItems.AddNew();
			firstItem.TM_Type = Calculator.Items.Operator.Plus;
			firstItem.TM_Break = 45m;

			Assert("Pre-con", firstItem.TM_BreakInfo.HasWarning(ErrorMessages.LowestBreakActsAsPlusAndMinus("-")));

			var secondItem = line.RateLineItems.AddNew();
			secondItem.TM_Type = Calculator.Items.Operator.Plus;
			secondItem.TM_Break = 100m;

			firstItem.Validation.ValidateAll();

			AssertHasWarning("Should still have warning on first", firstItem.TM_BreakInfo, ErrorMessages.LowestBreakActsAsPlusAndMinus("-"));

			secondItem.TM_Type = Calculator.Items.Operator.Minus;

			firstItem.Validation.ValidateAll();

			AssertNoWarning("Should no longer have warning", firstItem.TM_BreakInfo, ErrorMessages.LowestBreakActsAsPlusAndMinus("-"));

			AssertEquals("Should not sync dates if not sure user intented to create a lower break", 45m, firstItem.TM_Break);
			AssertEquals("Should not sync dates if not sure user intented to create a lower break", 100m, secondItem.TM_Break);

			AssertHasError(firstItem.TM_BreakInfo, ErrorMessages.WeightBreakLessThanMinusBreak);

			firstItem.TM_Break = 100m;

			AssertNoError(firstItem.TM_BreakInfo, ErrorMessages.WeightBreakLessThanMinusBreak);

			firstItem.Delete();

			secondItem.Validation.ValidateAll();

			AssertHasWarning("Should having warning on the the minus item", secondItem.TM_BreakInfo, ErrorMessages.LowestBreakActsAsPlusAndMinus("+"));

			var thirdItem = line.RateLineItems.AddNew();
			thirdItem.TM_Type = Calculator.Items.Operator.Plus;
			thirdItem.TM_Break = 100m;

			secondItem.Validation.ValidateAll();

			AssertNoWarning("Should no longer have a warning as plus and minus have matching breaks", secondItem.TM_BreakInfo, ErrorMessages.LowestBreakActsAsPlusAndMinus("+"));
		}

		public void TestMatchingPlusAndMinusWeightBreak_NoWarningForEQUCalculator()
		{
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line = testRate.AddRateEntry("ORG", "ALL", "AUSYD", "").AddRateLine("ODOC");
			line.TL_RateCalculator = EqualizationCalculator.Code;

			var firstItem = line.RateLineItems.AddNew();
			firstItem.TM_Break = 45m;

			AssertNoWarnings(firstItem.TM_BreakInfo);
		}

		public void TestCheckPlusWeightBreakAmount()
		{
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line = testRate.AddRateEntry("ORG", "ALL", "AUSYD", "").AddRateLine("ODOC");
			line.TL_RateCalculator = CombinedCalculator.Code;

			var rateLineItem1 = line.RateLineItems.AddNew();
			var rateLineItem2 = line.RateLineItems.AddNew();
			rateLineItem1.TM_Type = Calculator.Items.Operator.Minus;
			rateLineItem1.TM_Break = 100;
			rateLineItem2.TM_Type = Calculator.Items.Operator.Plus;
			rateLineItem2.TM_Break = 100;
			rateLineItem1.Validation.ValidateAll();
			rateLineItem2.Validation.ValidateAll();
			AssertNoErrors(rateLineItem1);
			AssertNoErrors(rateLineItem2);
			rateLineItem1.TM_Type = Calculator.Items.Operator.Plus;
			AssertHasError("changing TM_Type should validate TM_Break", rateLineItem1.TM_BreakInfo, ErrorMessages.WeightBreakIsTheSameAsAnotherPlusRate);

			rateLineItem1.TM_Type = Calculator.Items.Operator.MIN;
			rateLineItem1.TM_Break = 0;

			rateLineItem2.TM_Type = Calculator.Items.Operator.Minus;
			rateLineItem2.TM_Break = 45m;

			var rateLineItem3 = line.RateLineItems.AddNew();
			rateLineItem3.TM_Type = Calculator.Items.Operator.Plus;
			rateLineItem3.TM_Break = 45m;

			var rateLineItem4 = line.RateLineItems.AddNew();
			rateLineItem4.TM_Break = 30m;
			rateLineItem4.TM_Type = Calculator.Items.Operator.Plus;
			rateLineItem4.Validation.ValidateAll();

			AssertHasError(rateLineItem4.TM_BreakInfo, ErrorMessages.WeightBreakLessThanMinusBreak);

			rateLineItem4.TM_Break = 60m;
			rateLineItem4.Validation.ValidateAll();

			AssertNoError(rateLineItem4.TM_BreakInfo, ErrorMessages.WeightBreakLessThanMinusBreak);

			var rateLineItem5 = line.RateLineItems.AddNew();
			rateLineItem5.TM_Type = Calculator.Items.Operator.Plus;
			rateLineItem5.TM_Break = 90m;
			AssertNoErrors("Expected no error as autorating is no longer bound by the order of entry", rateLineItem5.TM_BreakInfo);

			rateLineItem5.TM_Break = 200m;
			AssertNoErrors(rateLineItem4.TM_BreakInfo);
			AssertNoErrors(rateLineItem5.TM_BreakInfo);

			var rateLineItem6 = line.RateLineItems.AddNew();
			rateLineItem6.TM_Type = Calculator.Items.Operator.Plus;
			rateLineItem6.TM_Break = 200m;
			AssertHasError(rateLineItem6.TM_BreakInfo, ErrorMessages.WeightBreakIsTheSameAsAnotherPlusRate);

			rateLineItem6.TM_Break = 250m;
			AssertNoErrors(rateLineItem6.TM_BreakInfo);

			rateLineItem5.TM_Break = 225m;
			AssertNoErrors(rateLineItem4.TM_BreakInfo);
			AssertNoErrors(rateLineItem5.TM_BreakInfo);
		}

		public void TestCheckPlusWeightBreakAmount_NoErrorForEqualizerCalculator()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateLine = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.ALL, "AUSYD", "").AddRateLine("ODOC");
			rateLine.TL_RateCalculator = CombinedCalculator.Code;

			var rateLineItem1 = rateLine.RateLineItems.AddNew();
			rateLineItem1.TM_Type = Calculator.Items.Operator.Plus;
			rateLineItem1.TM_Break = 45m;

			var rateLineItem2 = rateLine.RateLineItems.AddNew();
			rateLineItem2.TM_Type = Calculator.Items.Operator.Plus;
			rateLineItem2.TM_Break = 45m;
			rateLineItem2.Validation.ValidateAll();

			AssertHasError(rateLineItem2.TM_BreakInfo, ErrorMessages.WeightBreakIsTheSameAsAnotherPlusRate);

			rateLine.TL_RateCalculator = EqualizationCalculator.Code;
			var equalizeItem1 = rateLine.RateLineItems.AddNew();
			equalizeItem1.TM_Type = Calculator.Items.Operator.Plus;
			equalizeItem1.TM_Break = 45m;

			var equalizeItem2 = rateLine.RateLineItems.AddNew();
			equalizeItem2.TM_Type = Calculator.Items.Operator.Plus;
			equalizeItem2.TM_Break = 45m;
			equalizeItem2.Validation.ValidateAll();

			AssertNoError(equalizeItem2.TM_BreakInfo, ErrorMessages.WeightBreakIsTheSameAsAnotherPlusRate);
		}

		public void TestRateLineIsDeleted()
		{
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line = testRate.AddRateEntry("ORG", "ALL", "AUSYD", "").AddRateLine("ODOC");
			line.TL_RateCalculator = CombinedCalculator.Code;
			var rateLineItem = line.RateLineItems.AddNew();

			rateLineItem.Delete();
			var validator = new WeightBreakValidator(line, rateLineItem);

			AssertNoExceptionThrown(() => validator.CheckWeightBreak());
		}

		#region Implementation

		TestHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new TestHelper(Factory);
				}

				return fHelper;
			}
		}

		TestHelper fHelper;

		#endregion
	}
}
