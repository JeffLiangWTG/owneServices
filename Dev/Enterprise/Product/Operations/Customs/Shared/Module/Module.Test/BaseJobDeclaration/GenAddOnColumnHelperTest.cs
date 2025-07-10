using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Module.Testing
{
	sealed class GenAddOnColumnHelperTest : TestCaseWithFactory
	{
		public void TestHelper()
		{
			var helper = new GenAddOnColumnHelper();
			AssertNotNull(helper.SimpleQueryHelper);
			AssertNotNull(helper.QueryHelper);
		}
	}
}
