using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class TaxDeferIndicatorListPartialTest : TestCase
	{
		public void TestGetCodeDescriptionPairList()
		{
			DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider list = new TaxDeferIndicatorList();
			AssertEquals(list, list.GetCodeDescriptionPairList());
		}
	}
}
