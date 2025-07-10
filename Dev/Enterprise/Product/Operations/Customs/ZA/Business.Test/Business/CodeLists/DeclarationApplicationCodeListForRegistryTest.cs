using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class DeclarationApplicationCodeListForRegistryTest : TestCase
	{
		public void TestDeclarationApplicationCodeListForRegistry()
		{
			var testList = new DeclarationApplicationCodeListForRegistry();
			AssertEquals(4, testList.Count);
			Assert(testList.ContainsCode(DeclarationApplicationCodeList.Codes.Interfaced));
			Assert(testList.ContainsCode(DeclarationApplicationCodeList.Codes.Builtin));
			Assert(testList.ContainsCode(DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted));
			Assert(testList.ContainsCode(DeclarationApplicationCodeListForRegistry.Codes.BothInterfaceDefaulted));
		}
	}
}
