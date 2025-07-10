using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ConversionExtensionsTest : TestCase
	{
		public static void TestRemoveNonNumericCharacters()
		{
			var expectedText = "000232523658971236";
			ZString stringNumericCharacters = "TXA#0002325-23658971236";
			AssertEquals(expectedText, stringNumericCharacters.RemoveNonNumericCharacters());
		}
	}
}
