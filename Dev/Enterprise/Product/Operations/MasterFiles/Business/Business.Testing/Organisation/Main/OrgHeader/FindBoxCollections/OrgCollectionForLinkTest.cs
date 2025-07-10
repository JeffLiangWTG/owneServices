using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCollectionForLink))]
	sealed class OrgCollectionForLinkTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			return new OrgCollectionForLink(Factory, org1, org2);
		}

		public void TestAllowNewTemporaryOrganisations()
		{
			var orgsForLink = new OrgCollectionForLink(Factory, Factory.NewWithValidTestData<OrgHeader>(), Factory.NewWithValidTestData<OrgHeader>());
			Assert(orgsForLink.AllowNewTemporaryOrganisations);
		}

		public void TestFilterOutMasterAndTargetOrgs()
		{
			var masterOrg = Factory.NewWithValidTestData<OrgHeader>();
			masterOrg.OH_IsUserFlag1 = true;
			var targetOrg = Factory.NewWithValidTestData<OrgHeader>();
			targetOrg.OH_IsUserFlag1 = true;
			var exampleOrg = Factory.NewWithValidTestData<OrgHeader>();
			exampleOrg.OH_IsUserFlag1 = true;
			Factory.Save();

			var orgsForLink = new OrgCollectionForLinkForTest(new BusinessObjectFactory(), masterOrg, targetOrg);

			AssertEquals("Collection.Count", 0, orgsForLink.Count);

			orgsForLink.Load();

			Assert("Collection.Count > 0", orgsForLink.Count > 0);
			Assert("Collection contains exampleOrg", orgsForLink.Contains(exampleOrg.PK));
			Assert("Collection does not contain masterOrg", !orgsForLink.Contains(masterOrg.PK));
			Assert("Collection does not contain targetOrg", !orgsForLink.Contains(targetOrg.PK));
		}
	}
}
