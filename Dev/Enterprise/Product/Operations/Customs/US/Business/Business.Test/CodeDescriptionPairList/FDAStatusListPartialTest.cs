using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FDAStatusListTest : TestCase
	{
		public void TestGetCodeDescriptionPairList()
		{
			DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider list = new FDAStatusList();
			AssertEquals(list, list.GetCodeDescriptionPairList());
		}
	}
}
