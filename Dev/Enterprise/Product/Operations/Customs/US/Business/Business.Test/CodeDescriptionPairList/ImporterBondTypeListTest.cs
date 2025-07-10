using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ImporterBondTypeListTest : TestCase
	{
		public void TestICodeDescriptionPairListProviderMembers()
		{
			DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider list = new ImporterBondTypeList();
			AssertEquals(list, list.GetCodeDescriptionPairList());
		}
	}
}
