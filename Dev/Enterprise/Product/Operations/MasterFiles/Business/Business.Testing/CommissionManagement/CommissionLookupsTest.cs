using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CommissionLookupsTest : CommissionLookupsTestCase<CommissionLookups>
	{
		protected override CommissionLookups GetNewLookups()
		{
			return CommissionLookups.New(Factory);
		}

		public void TestGetShouldShowServicesAndSubModules()
		{
			var lookups = new CommissionLookupsForTest(Factory);
			AssertEquals(false, lookups.GetShouldShowServicesAndSubModules());
		}

		class CommissionLookupsForTest : CommissionLookups
		{
			public CommissionLookupsForTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}
	}
}
