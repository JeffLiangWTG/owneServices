using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class LookupsHelperTest : WhsTestCaseWithFactory
	{
		#region TestPackTypesInLookupsHelper

		public void TestPackTypesInLookupsHelper()
		{
			var packTypesWithStandardUnits = LookupsHelper.PackTypesWithStandardUnits(Factory);
			AssertNotNull(packTypesWithStandardUnits);
			Assert(packTypesWithStandardUnits is CodeDescriptionPairList);
		}

		#endregion
	}
}
