using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	class CommonCusReferenceLookupsTest : TestCaseWithFactory
	{
		public void TestOwnersList()
		{
			AssertType<OrganisationsFindBoxCollection>(cusReference.Lookups.OwnersList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusReference = Factory.New<CommonCusReferenceForTest>();
		}
		CommonCusReferenceForTest cusReference;
	}
}
