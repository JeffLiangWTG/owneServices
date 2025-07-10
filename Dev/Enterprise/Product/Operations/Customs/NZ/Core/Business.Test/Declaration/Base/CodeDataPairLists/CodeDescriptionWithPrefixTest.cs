using Enterprise.Customs.DataTransfer.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Testing
{
	public class CodeDescriptionWithPrefixTest : TestCase
	{
		public void TestCodeDescriptionWithPrefix()
		{
			var value = new CodeDescriptionWithPrefix(CodeDescriptionPairForTesting.New("CES", "TEST"), "TYP");

			AssertEquals("Code Matches", "CES", value.Code);
			AssertEquals("Description is as Expected", "TYP | TEST", value.Description);
		}
	}
}
