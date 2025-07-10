using System;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class ExcludeCompanyTariffsCalculatorTest : CalculatorTest
	{
		public override void TestCheckOrCreateItems()
		{
			Assert("Nothing to test", true);
		}

		public override void TestMapping()
		{
			Assert("Nothing to test", true);
		}

		public override void TestGetCloneCode()
		{
			AssertGetCloneCode(CalculatorCode);
		}

		public override void TestGetCloneLineItems()
		{
			Assert("Nothing to test", true);
		}

		public override void TestQuotationLines()
		{
			Assert("Nothing to test", true);
		}

		public override void TestDocLineAmount()
		{
			Assert("This calculator is only valid for ClientRate", true);
		}

		protected override Type CalculatorType
		{
			get { return typeof(ExcludeCompanyTariffsCalculator); }
		}

		protected override string CalculatorCode
		{
			get { return ExcludeCompanyTariffsCalculator.Code; }
		}
	}
}
