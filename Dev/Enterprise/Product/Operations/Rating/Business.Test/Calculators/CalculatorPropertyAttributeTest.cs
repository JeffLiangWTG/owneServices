using CargoWise.Types;

namespace Enterprise.Rating.Business.Testing
{
	public class CalculatorPropertyAttributeTest : RatingTestCase
	{
		#region Properties and Setup

		RateLine Line;
		RateLineItem StringLineItem;
		RateLineItem BoolLineItem;
		RateLineItem DecimalLineItem;
		RateLineItem IntLineItem;
		CalculatorToTestCalculatorPropertyAttribute TestCalculator;

		protected override void SetUp()
		{
			base.SetUp();

			var rateEntry = Factory.NewWithValidTestData<RateEntry>();

			Line = rateEntry.RateLines.AddNew();
			Line.TL_RateCalculator = CalculatorToTestCalculatorPropertyAttribute.Code;

			StringLineItem = Line.RateLineItems.FindByTM_Type("IT1");
			StringLineItem.TM_Text = "StringValue";

			BoolLineItem = Line.RateLineItems.FindByTM_Type("IT2");
			BoolLineItem.TM_Text = "Y";

			DecimalLineItem = Line.RateLineItems.FindByTM_Type("IT3");
			DecimalLineItem.TM_RelevantValue = 10m;

			IntLineItem = Line.RateLineItems.FindByTM_Type("IT4");
			IntLineItem.TM_RelevantValue = 20m;

			TestCalculator = (CalculatorToTestCalculatorPropertyAttribute)Line.Calculator;
		}

		#endregion

		#region Test Methods

		public void TestGetValue()
		{
			var stringAttr = TestCalculator.GetCalculatorPropertyAttribute("IT1");
			AssertEquals(StringLineItem.TM_Text, (ZString)stringAttr.GetValue(TestCalculator, 0));

			var boolAttr = TestCalculator.GetCalculatorPropertyAttribute("IT2");
			AssertEquals(BoolLineItem.TM_Text == "Y", (ZBool)boolAttr.GetValue(TestCalculator, 0));

			var decimalAttr = TestCalculator.GetCalculatorPropertyAttribute("IT3");
			AssertEquals(DecimalLineItem.TM_RelevantValue, (ZDecimal)decimalAttr.GetValue(TestCalculator, 0));

			var intAttr = TestCalculator.GetCalculatorPropertyAttribute("IT4");
			AssertEquals(IntLineItem.TM_RelevantValue, (ZDecimal)(ZInt)intAttr.GetValue(TestCalculator, 0));
		}

		public void TestSetValue()
		{
			var stringAttr = TestCalculator.GetCalculatorPropertyAttribute("IT1");
			stringAttr.SetValue(TestCalculator, 0, (ZString)"ChangedStringValue");
			AssertEquals("ChangedStringValue", StringLineItem.TM_Text);

			var boolAttr = TestCalculator.GetCalculatorPropertyAttribute("IT2");
			boolAttr.SetValue(TestCalculator, 0, ZBool.False);
			AssertEquals("N", BoolLineItem.TM_Text);

			var decimalAttr = TestCalculator.GetCalculatorPropertyAttribute("IT3");
			decimalAttr.SetValue(TestCalculator, 0, (ZDecimal)20m);
			AssertEquals(20m, DecimalLineItem.TM_RelevantValue);

			var intAttr = TestCalculator.GetCalculatorPropertyAttribute("IT4");
			intAttr.SetValue(TestCalculator, 0, (ZInt)30m);
			AssertEquals(30m, IntLineItem.TM_RelevantValue);
		}

		#endregion
	}
}
