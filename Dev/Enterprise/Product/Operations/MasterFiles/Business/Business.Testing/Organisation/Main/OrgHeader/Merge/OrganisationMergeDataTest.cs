using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrganisationMergeDataTest : TestCaseWithFactory
	{
		public void TestOldOrgHeader_RetrievedFromMergeOrgHeaderIfSet()
		{
			var oldOrg = Factory.NewWithValidTestData<OrgHeader>();
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			var mergeOrgHeader = new MergeOrgHeader(Factory, oldOrg, newOrg);

			var mergeData = new OrganisationMergeData(ZGuid.Empty, ZGuid.Empty, null, null);
			mergeData.MergeOrgHeader = mergeOrgHeader;
			AssertEquals("OldOrgHeader should retrieve old organisation from the MergeOrgHeader", oldOrg.PK, mergeData.OldOrgHeader.PK);
		}

		public void TestOldOrgHeader_LoadedCorrectlyWhenMergeOrgHeaderIsNotSet()
		{
			var oldOrg = Factory.NewWithValidTestData<OrgHeader>();
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var mergeData = new OrganisationMergeData(oldOrg.PK, newOrg.PK, null, null);
			AssertEquals("OldOrgHeader should retrieve old organisation from the factory", oldOrg.PK, mergeData.OldOrgHeader.PK);
		}

		public void TestNewOrgHeader_RetrievedFromMergeOrgHeaderIfSet()
		{
			var oldOrg = Factory.NewWithValidTestData<OrgHeader>();
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			var mergeOrgHeader = new MergeOrgHeader(Factory, oldOrg, newOrg);

			var mergeData = new OrganisationMergeData(ZGuid.Empty, ZGuid.Empty, null, null);
			mergeData.MergeOrgHeader = mergeOrgHeader;
			AssertEquals("NewOrgHeader should retrieve new organisation from the MergeOrgHeader", newOrg.PK, mergeData.NewOrgHeader.PK);
		}

		public void TestNewOrgHeader_LoadedCorrectlyWhenMergeOrgHeaderIsNotSet()
		{
			var oldOrg = Factory.NewWithValidTestData<OrgHeader>();
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var mergeData = new OrganisationMergeData(oldOrg.PK, newOrg.PK, null, null);
			AssertEquals("NewOrgHeader should retrieve new organisation from the factory", newOrg.PK, mergeData.NewOrgHeader.PK);
		}

		public void TestConnectionAndCollectionsAreInitialised()
		{
			var mergeData = new OrganisationMergeData(ZGuid.Empty, ZGuid.Empty, null, null);
			AssertEquals("Connection property should be set to the default connection when creating OrganisationMergeData", Db.Connection, mergeData.Connection);
			AssertNotNull("Merged addresses property should be initialised when creating OrganisationMergeData", mergeData.MergedAddresses);
			AssertNotNull("Initial brands property should be initialised when creating OrganisationMergeData", mergeData.InitialBrands);
		}
	}
}
