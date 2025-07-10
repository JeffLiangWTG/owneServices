using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class ServiceOrgRelatedPartySubsetCollection : OrgRelatedPartySubsetCollection<OrgRelatedParty>
	{
		public ServiceOrgRelatedPartySubsetCollection(OrgRelatedPartyDependentCollection completeCollection)
			: base(completeCollection)
		{
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var relatedPartyElement = (OrgRelatedParty)element;
			return relatedPartyElement.PR_PartyType == RelatedPartyTypeList.Codes.AuthorizedCargoReporter;
		}
	}
}
