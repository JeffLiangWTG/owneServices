using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgHeaderOriginalCollectionsTest : TestCaseWithFactory
	{
		public void TestCollectionsReturnNullWhenCollectionsNotAccessed()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.originalCollections = null;
			AssertNotNull(org.OriginalCollections);
			AssertNull(org.OriginalCollections.OrganisationNamesExceptBrands);
			AssertNull(org.OriginalCollections.OrganisationNamesFromBrands);
			AssertNull(org.OriginalCollections.Addresses);
			AssertNull(org.OriginalCollections.CustomsCodes);
		}

		public void TestCollectionsDoNotReturnNullWhenCollectionsAccessed()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.originalCollections = null;
			OrgAddressDependentCollection col1 = org.AddressesNoAutoCreate;
			OrgCusCodeCollection col2 = org.CustomsCodes;
			OrgBrandOrRelatedNameCollection col3 = org.BrandsOrRelatedNames;
			AssertNotNull(org.OriginalCollections);
			AssertNotNull(org.OriginalCollections.OrganisationNamesExceptBrands);
			AssertNotNull(org.OriginalCollections.OrganisationNamesFromBrands);
			AssertNotNull(org.OriginalCollections.Addresses);
			AssertNotNull(org.OriginalCollections.CustomsCodes);
		}
	}
}
