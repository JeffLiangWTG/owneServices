using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgAgentRelationshipValidationTest : BusinessObjectValidationTestCase
	{
		public void TestO3_OH_GroupNetworkOrFranchise()
		{
			var headOffice = Factory.New<OrgHeader>();
			var relationship = Factory.New<OrgAgentRelationship>();
			relationship.O3_OH_GroupNetworkOrFranchise = headOffice.PK;

			var details1 = relationship.ProfitShareDetails.AddNew();
			var party11 = details1.PartyDetails.AddNew();
			var party12 = details1.PartyDetails.AddNew();
			var details2 = relationship.ProfitShareDetails.AddNew();
			var party21 = details2.PartyDetails.AddNew();

			party11.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent;
			party12.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.HeadOfficeFranchisor;
			party21.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.HeadOfficeFranchisor;

			AssertNoErrors(party11.PS_PartyTypeInfo);
			AssertNoErrors(party12.PS_PartyTypeInfo);
			AssertNoErrors(party21.PS_PartyTypeInfo);

			relationship.O3_OH_GroupNetworkOrFranchise = ZGuid.Empty;
			AssertNoErrors(party11.PS_PartyTypeInfo);
			AssertHasErrors(party12.PS_PartyTypeInfo);
			AssertHasErrors(party21.PS_PartyTypeInfo);

			relationship.O3_OH_GroupNetworkOrFranchise = headOffice.PK;
			AssertNoErrors(party11.PS_PartyTypeInfo);
			AssertNoErrors(party12.PS_PartyTypeInfo);
			AssertNoErrors(party21.PS_PartyTypeInfo);
		}
	}
}
