using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class RelatedParty
	{
		public RelatedParty(string relationshipCode, ZGuid relatedOrganisationPK)
		{
			this.RelationshipCode = relationshipCode;
			this.RelatedOrganisationPK = relatedOrganisationPK;
		}
		public readonly string RelationshipCode;
		public readonly ZGuid RelatedOrganisationPK;
	}

	public class RelatedPartyWithCode
	{
		public RelatedPartyWithCode(string relationshipCode, string relatedOrganisationCode)
		{
			this.RelationshipCode = relationshipCode;
			this.RelatedOrganisationCode = relatedOrganisationCode;
		}
		public readonly string RelationshipCode;
		public readonly string RelatedOrganisationCode;
	}
}
