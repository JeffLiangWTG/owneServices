using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class UniqueValueCalculatorTest : TestCaseWithFactory
	{
		public void TestGetUniqueValue()
		{
			var elements = new List<string>();
			elements.Add("A");
			elements.Add("A");
			elements.Add("");
			elements.Add("a");
			var result = UniqueValueCalculator.GetUniqueValue<string, ZString>(elements, x => x.ToUpper(), ZString.Empty);
			AssertEquals("A", result);
			result = UniqueValueCalculator.GetUniqueValue<string, ZString>(elements, x => x, ZString.Empty);
			AssertEquals("", result);
		}

		public void TestHasMultiValues()
		{
			var elements = new List<string>();
			elements.Add("A");
			elements.Add("A");
			elements.Add("");
			elements.Add("a");
			var result = UniqueValueCalculator.HasMultiValues<string, ZString>(elements, x => x.ToUpper());
			Assert(!result);
			result = UniqueValueCalculator.HasMultiValues<string, ZString>(elements, x => x);
			Assert(result);
		}
	}
}
