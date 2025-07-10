using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class ConsignorOrgRelatedPartySubsetCollection : OrgRelatedPartySubsetCollection
	{
		public ConsignorOrgRelatedPartySubsetCollection(OrgRelatedPartyDependentCollection completeCollection)
			: base(completeCollection)
		{
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var relatedPartyElement = (OrgRelatedParty)element;

			if (relatedPartyElement.PR_PartyType == RelatedPartyTypeList.Codes.ServiceProviderCreditor)
			{
				return false;
			}

			return relatedPartyElement.PR_FreightDirection == RelatedPartyDirectionList.Codes.Pickup
					|| relatedPartyElement.PR_FreightDirection == RelatedPartyDirectionList.Codes.PickupAndDelivery
					|| relatedPartyElement.PR_PartyType == RelatedPartyTypeList.Codes.WarehouseForwarder;
		}

		internal bool IsThisPartOfTheCollectionInternal(BusinessObject element)
		{
			return IsThisPartOfTheCollection(element);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((OrgRelatedParty)child).PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
		}
	}
}
