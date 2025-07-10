using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgManagementRelatedParty))]
	sealed class OrgManagementRelatedPartyTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetRelatedManagementSubsidiariesQuery()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var subOrgA = Factory.NewWithValidTestData<OrgHeader>();
			var entMngParty1A = OrgManagementRelatedPartyTestHelper.Create(Factory, org1, subOrgA);

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var comMngParty2A = OrgManagementRelatedPartyTestHelper.Create(Factory, org2, subOrgA, GlbCompany.CurrentCompany);

			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var subOrgB = Factory.NewWithValidTestData<OrgHeader>();
			var entMngParty3B = OrgManagementRelatedPartyTestHelper.Create(Factory, org3, subOrgB);
			var subOrgC = Factory.NewWithValidTestData<OrgHeader>();
			var comMngParty3C = OrgManagementRelatedPartyTestHelper.Create(Factory, org3, subOrgC, GlbCompany.CurrentCompany);
			var differentCompany = Factory.NewWithValidTestData<GlbCompany>();
			var subOrgD = Factory.NewWithValidTestData<OrgHeader>();
			OrgManagementRelatedPartyTestHelper.Create(Factory, org3, subOrgD, differentCompany);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgManagementRelatedParty>.PKOnlyComparer, new[] { entMngParty1A }, Factory.Load<OrgManagementRelatedParty>(OrgManagementRelatedParty.GetRelatedManagementSubsidiariesQuery(org1)));
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgManagementRelatedParty>.PKOnlyComparer, new[] { comMngParty2A }, Factory.Load<OrgManagementRelatedParty>(OrgManagementRelatedParty.GetRelatedManagementSubsidiariesQuery(org2)));
			AssertContainsExactElementsInAnyOrder("Should not include related party for different company", BusinessObjectEqualityComparer<OrgManagementRelatedParty>.PKOnlyComparer, new[] { entMngParty3B, comMngParty3C }, Factory.Load<OrgManagementRelatedParty>(OrgManagementRelatedParty.GetRelatedManagementSubsidiariesQuery(org3)));
		}

		public void TestGetRelatedManagementParentsQuery()
		{
			var orgA = Factory.NewWithValidTestData<OrgHeader>();
			var parentEntOrgA = Factory.NewWithValidTestData<OrgHeader>();
			var entMngPartyA = OrgManagementRelatedPartyTestHelper.Create(Factory, parentEntOrgA, orgA);

			var orgB = Factory.NewWithValidTestData<OrgHeader>();
			var parentComOrgB = Factory.NewWithValidTestData<OrgHeader>();
			var comMngPartyB = OrgManagementRelatedPartyTestHelper.Create(Factory, parentComOrgB, orgB, GlbCompany.CurrentCompany);

			var orgC = Factory.NewWithValidTestData<OrgHeader>();
			var parentEntOrgC = Factory.NewWithValidTestData<OrgHeader>();
			var entMngPartyC = OrgManagementRelatedPartyTestHelper.Create(Factory, parentEntOrgC, orgC);
			var parentComOrgC = Factory.NewWithValidTestData<OrgHeader>();
			var comMngPartyC = OrgManagementRelatedPartyTestHelper.Create(Factory, parentComOrgC, orgC, GlbCompany.CurrentCompany);
			var differentCompany = Factory.NewWithValidTestData<GlbCompany>();
			var parentComOrgCForDifferentCompany = Factory.NewWithValidTestData<OrgHeader>();
			OrgManagementRelatedPartyTestHelper.Create(Factory, parentComOrgCForDifferentCompany, orgC, differentCompany);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgManagementRelatedParty>.PKOnlyComparer, new[] { entMngPartyA }, Factory.Load<OrgManagementRelatedParty>(OrgManagementRelatedParty.GetRelatedManagementParentsQuery(orgA)));
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgManagementRelatedParty>.PKOnlyComparer, new[] { comMngPartyB }, Factory.Load<OrgManagementRelatedParty>(OrgManagementRelatedParty.GetRelatedManagementParentsQuery(orgB)));
			AssertContainsExactElementsInAnyOrder("Should not include related party for different company", BusinessObjectEqualityComparer<OrgManagementRelatedParty>.PKOnlyComparer, new[] { entMngPartyC, comMngPartyC }, Factory.Load<OrgManagementRelatedParty>(OrgManagementRelatedParty.GetRelatedManagementParentsQuery(orgC)));
		}
	}
}
