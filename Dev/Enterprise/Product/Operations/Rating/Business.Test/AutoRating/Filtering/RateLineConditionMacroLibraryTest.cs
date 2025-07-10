using System.Collections.Generic;
using CargoWise.Macros;
using CargoWise.Macros.Testing;

namespace Enterprise.Rating.Business.Testing
{
	sealed class RateLineConditionMacroLibraryTest : TestCaseWithMacros
	{
		public void TestConvert()
		{
			AssertMacroRun("Convert(123.456789, \"KM\", \"M\")", new MacroRun { ExpectedResult = 123456.789m });
			AssertMacroRun("Convert(123.456789, \"KG\", \"G\")", new MacroRun { ExpectedResult = 123456.789m });
			AssertMacroRun("Convert(123.456789, \"M2\", \"CM2\")", new MacroRun { ExpectedResult = 1234567.89m });
			AssertMacroRun("Convert(123.456789, \"M3\", \"CC\")", new MacroRun { ExpectedResult = 123456789m });

			AssertMacroRun("Convert(ThisIsntANumber, \"M3\", \"CM3\")", new MacroRun { ExpectedResult = 0m });
			AssertMacroRun("Convert(123, \"ThisIsntAUnit\", \"CM3\")", new MacroRun { ExpectedResult = 0m });
			AssertMacroRun("Convert(123, \"M3\", \"ThisIsntAUnit\")", new MacroRun { ExpectedResult = 0m });
		}

		protected override IEnumerable<IMacroLibrary> Libraries
		{
			get { yield return new RateLineConditionMacroLibrary(); }
		}
	}
}
