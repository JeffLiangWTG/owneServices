using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgAgentRelationshipCollection))]
	sealed class OrgAgentRelationshipCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgAgentRelationshipCollection(Factory);
		}

		public void TestOrgBeingViewedFrom()
		{
			OrgHeader testHeader = OrgHeader.New(Factory);
			OrgAgentRelationship testRelationship = testHeader.AgentRelationships.AddNew();

			AssertNull("OrgBeingViewedFrom should be null", testRelationship.OrgBeingViewedFrom);
			testHeader.AgentRelationships.SetOrganisationReadOnly(testHeader);
			AssertEquals("OrgBeingViewedFrom should be set on the organisation", testHeader, testRelationship.OrgBeingViewedFrom);
		}
	}
}
