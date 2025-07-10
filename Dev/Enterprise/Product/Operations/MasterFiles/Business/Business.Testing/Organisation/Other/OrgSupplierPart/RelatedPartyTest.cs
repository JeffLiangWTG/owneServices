using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RelatedPartyTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var owner = OrgHeader.New(Factory);
			owner.OH_Code = "OWNER";

			var relatedParty = new RelatedParty(OrgPartRelation.RelationshipTypes.Owner, owner.PK);
			AssertEquals("RelationshipCode", OrgPartRelation.RelationshipTypes.Owner, relatedParty.RelationshipCode);
			AssertEquals("RelatedOrganisationPK", owner.PK, relatedParty.RelatedOrganisationPK);
		}
	}
}
