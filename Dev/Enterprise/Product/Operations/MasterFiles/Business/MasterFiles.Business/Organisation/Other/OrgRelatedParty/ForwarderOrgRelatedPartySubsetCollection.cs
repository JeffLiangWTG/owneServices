using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class ForwarderOrgRelatedPartySubsetCollection : OrgRelatedPartySubsetCollection
	{
		public ForwarderOrgRelatedPartySubsetCollection(OrgRelatedPartyDependentCollection completeCollection)
			: base(completeCollection)
		{
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var relatedParty = (OrgRelatedParty)element;
			return relatedParty.PR_FreightDirection == RelatedPartyDirectionList.Codes.Forwarder
				|| (relatedParty.PR_PartyType == RelatedPartyTypeList.Codes.ForwarderCoLoadWith
					&& relatedParty.PR_FreightDirection == RelatedPartyDirectionList.Codes.Pickup);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((OrgRelatedParty)child).PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
		}
	}
}
