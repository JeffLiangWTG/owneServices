using System;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class ProfitShareRebateCalculatorTest : BasePercentageCalculatorTest
	{
		public override void TestCheckOrCreateItems()
		{
			AssertNull(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER));
			AssertNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MIN));
			AssertNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS));
			AssertNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MAX));
			AssertNull(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Text.ZeroWhenLoss));

			InitialiseTestCalculator();

			AssertNotNull(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MIN));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MAX));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Text.ZeroWhenLoss));

			AssertEquals(5, Line.RateLineItems.Count);

			AssertEquals(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.PER).TM_RelevantValueInfo, TestCalculator.Decimal1Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MIN).TM_RelevantValueInfo, TestCalculator.Decimal2Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.BAS).TM_RelevantValueInfo, TestCalculator.Decimal3Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(Calculator.Items.Operator.MAX).TM_RelevantValueInfo, TestCalculator.Decimal4Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(CalculatorConstants.Text.ZeroWhenLoss).TM_TextInfo, TestCalculator.Bool1Info);
		}

		public override void TestMapping()
		{
			TestMapping(CalculatorConstants.Type.PER, "Decimal1");
			TestMapping(Calculator.Items.Operator.MIN, "Decimal2");
			TestMapping(Calculator.Items.Operator.BAS, "Decimal3");
			TestMapping(Calculator.Items.Operator.MAX, "Decimal4");
			TestMapping(CalculatorConstants.Text.ZeroWhenLoss, "Bool1");
		}

		public override void TestQuotationLines()
		{
			Assert("Not required for this calculator", true);
		}

		public override void TestDocLineAmount()
		{
			Assert("Not required for this calculator", true);
		}

		public override void TestGetCloneCode()
		{
			AssertGetCloneCode(CalculatorCode);
		}

		public override void TestGetCloneLineItems()
		{
			Assert("Not required for this calculator", true);
		}

		public override void TestGetCostsComparerChargesSummary()
		{
			Assert("Calculator does not apply to Costings", true);
		}

		#region Implementation

		protected override Type CalculatorType
		{
			get { return typeof(ProfitShareRebateCalculator); }
		}

		protected override string CalculatorCode
		{
			get { return ProfitShareRebateCalculator.Code; }
		}

		new ProfitShareRebateCalculator TestCalculator
		{
			get { return (ProfitShareRebateCalculator)base.TestCalculator; }
		}

		#endregion
	}
}
