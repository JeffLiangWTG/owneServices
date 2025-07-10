using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Test
{
	public  class FreightTextLimitCalculatorTest : TestCaseWithFactory
	{
		public void TestEmptyTextReturnsEmpty()
		{
			var result = calculator.CalculateLimits(ZString.Empty, 5, 10);
			AssertEquals(result, ZString.Empty);
		}

		public void TestZeroLinesReturnsEmpty()
		{
			var result = calculator.CalculateLimits("Some text", 0, 10);
			AssertEquals(result, ZString.Empty);
		}

		public void TestZeroLineLengthReturnsEmpty()
		{
			var result = calculator.CalculateLimits("Some text", 5, 0);
			AssertEquals(result, ZString.Empty);
		}

		public void TestTextShorterThanLineLength()
		{
			var result = calculator.CalculateLimits("Some text", 5, 10);
			NUnit.Framework.Assert.That((string)result, Is.EqualTo("Some text"));
			AssertEquals(result, "Some text");
		}

		public void TestTextLongerThanLineLength()
		{
			var result = calculator.CalculateLimits("Some text that is longer than the line length", 5, 10);
			AssertEquals(result, "Some text ");
		}

		public void TestTextLongerThanLineLengthAndMultipleLines()
		{
			var inputText = $"Some text that is longer than the line length{System.Environment.NewLine}And has multiple lines{System.Environment.NewLine}And some lines are bigger than line length{System.Environment.NewLine}{System.Environment.NewLine}{System.Environment.NewLine}Shorter{System.Environment.NewLine}Last line length should be exactly equal to line length";
			var result = calculator.CalculateLimits(inputText, 5, 10);
			AssertEquals(result, $"Some tex{System.Environment.NewLine}And has {System.Environment.NewLine}And some{System.Environment.NewLine}Shorter{System.Environment.NewLine}Last line ");
		}
		protected override void SetUp()
		{
			base.SetUp();
			calculator = new FreightTextLimitCalculator();
		}

		FreightTextLimitCalculator calculator;
	}
}
