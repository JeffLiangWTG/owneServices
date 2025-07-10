using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	class QuestionForFormulaSpecificValueTest : TestCase
	{
		public void TestIsDefaultPrecisionScale()
		{
			Assert("only giving in a Question", new QuestionForFormulaSpecificValue("test").IsDefaultPrecisionScale);
			Assert("giving in a Question and 0 limit", new QuestionForFormulaSpecificValue("test", 0, 0).IsDefaultPrecisionScale);
			Assert("giving in a Question and precision limit", !new QuestionForFormulaSpecificValue("test", 5, 0).IsDefaultPrecisionScale);
			Assert("giving in a Question and scale limit", !new QuestionForFormulaSpecificValue("test", 0, 3).IsDefaultPrecisionScale);
			Assert("giving in a Question and both limits", !new QuestionForFormulaSpecificValue("test", 5, 3).IsDefaultPrecisionScale);
		}

		public void TestADefaultFormulaSpecificValueIsRequired()
		{
			AssertEquals("Value should be specified. The default value should be 0.", QuestionForFormulaSpecificValue.ADefaultFormulaSpecificValueIsRequired(0));
			AssertEquals("Value should be specified. The default value should be 0.0.", QuestionForFormulaSpecificValue.ADefaultFormulaSpecificValueIsRequired(1));
			AssertEquals("Value should be specified. The default value should be 0.00.", QuestionForFormulaSpecificValue.ADefaultFormulaSpecificValueIsRequired(2));
			AssertEquals("Value should be specified. The default value should be 0.000.", QuestionForFormulaSpecificValue.ADefaultFormulaSpecificValueIsRequired(3));
			AssertEquals("Value should be specified. The default value should be 0.0000.", QuestionForFormulaSpecificValue.ADefaultFormulaSpecificValueIsRequired(4));
		}

		public void TestGetDecimalFormat()
		{
			AssertEquals("0.00", QuestionForFormulaSpecificValue.GetDecimalFormat("", 5, 2));
			AssertEquals("0.00", QuestionForFormulaSpecificValue.GetDecimalFormat("234sasdf", 5, 2));
			AssertEquals("123.13", QuestionForFormulaSpecificValue.GetDecimalFormat("123.125", 5, 2));
			AssertEquals("999.99", QuestionForFormulaSpecificValue.GetDecimalFormat("3233", 5, 2));
		}

		public void TestGetMaximumValue()
		{
			AssertEquals(999.99m, QuestionForFormulaSpecificValue.GetMaximumValue(5, 2));
			AssertEquals(0.99m, QuestionForFormulaSpecificValue.GetMaximumValue(2, 2));
			AssertEquals(99.999m, QuestionForFormulaSpecificValue.GetMaximumValue(5, 3));
			AssertEquals(0.99m, QuestionForFormulaSpecificValue.GetMaximumValue(1, 2));
		}
	}
}
