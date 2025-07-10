using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class ZAProvisionalPaymentTypesTest : TestCaseWithFactory
	{
		public void TestICodeDescriptionPairListProvider()
		{
			var provider = new ZAProvisionalPaymentTypes() as DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider;
			var list = provider.GetCodeDescriptionPairList();
			CombineAssertions(() =>
			{
				AssertEquals("Expecting 8 PP Types", 8, list.Count);
				AssertEquals("Header Level Code", true, list.ContainsCode(HeaderLevelProvisionalPayments.Codes.PPE));
				AssertEquals("Line Level Code", true, list.ContainsCode(LineLevelProvisionalPayments.Codes.PPA));
			});
		}
	}
}
