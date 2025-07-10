using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class OrgRelatedPartyCollectionView : BusinessObjectCollectionView<OrgRelatedParty>
	{
		public OrgRelatedPartyCollectionView(OrgRelatedPartyCollection collectionToFilter)
			: base(collectionToFilter)
		{
		}

		public OrgRelatedPartyCollectionView(OrgRelatedPartyDependentCollection collectionToFilter)
			: base(collectionToFilter)
		{
		}

		public void FilterByPartyType(ZString party, ZString direction)
		{
			this.partyType = party;
			this.direction = direction;
			this.Rebuild();
		}

		ZString partyType;
		ZString direction;

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			bool result = true;
			bool enteredPartyType = !this.partyType.IsEmpty;
			bool enteredDirection = !this.direction.IsEmpty;

			OrgRelatedParty party = (OrgRelatedParty)element;

			if (enteredPartyType)
			{
				result = party.PR_PartyType == partyType;
			}
			if (enteredDirection && result)
			{
				result = party.PR_FreightDirection == direction;
			}
			return result;
		}

		public void ClearRelatedPartyFilters()
		{
			FilterByPartyType("", "");
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			var orgRelatedParty = elementToDelete as OrgRelatedParty;
			if (orgRelatedParty.PR_CustomsStatus == CSARelatedPartyStatusList.Codes.Added)
			{
				orgRelatedParty.PR_CustomsStatus = CSARelatedPartyStatusList.Codes.DeletePendingSeeCustomsMessagingMenu;
			}
			else
			{
				base.RemoveAndDelete(elementToDelete);
			}
		}
	}
}
