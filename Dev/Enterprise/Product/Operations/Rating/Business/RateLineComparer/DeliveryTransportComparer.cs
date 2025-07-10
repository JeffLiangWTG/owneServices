using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	class DeliveryTransportComparer : BaseRateLineComparer
	{
		public DeliveryTransportComparer(RatingCriteria criteria)
		{
			this.criteria = criteria;
		}

		readonly RatingCriteria criteria;

		public override int Compare(FastLine line1, FastLine line2)
		{
			if (criteria?.AutoRatedFor?.FirstOrDefault() is CommonShipment commonShipment)
			{
				var shipmentDeliveryTransport = commonShipment.DocsAndCartage?.JP_OA_DeliveryCartageCoAddr_ZAddress?.OrgHeader?.PK ?? ZGuid.Empty;

				if (!shipmentDeliveryTransport.IsValid)
				{
					return 0;
				}

				var org1 = line1.ParentRateEntry?.ParentRatingHeader?.Header;
				var org2 = line2.ParentRateEntry?.ParentRatingHeader?.Header;

				if (org1 == null || org2 == null)
				{
					return 0;
				}

				if (org1.PK == shipmentDeliveryTransport &&
					org1.AllRelatedParties.Cast<OrgRelatedParty>()
					.Any(o => o.PR_PartyType == RelatedPartyTypeList.Codes.ServiceProviderCreditor && o.PR_OH_RelatedParty == org2.PK))
				{
					return -1;
				}

				if (org2.PK == shipmentDeliveryTransport &&
					org2.AllRelatedParties.Cast<OrgRelatedParty>()
					.Any(o => o.PR_PartyType == RelatedPartyTypeList.Codes.ServiceProviderCreditor && o.PR_OH_RelatedParty == org1.PK))
				{
					return 1;
				}
			}

			return 0;
		}

		protected override string GetName()
		{
			return (NoResString)"Delivery Transport"; // log message, subject to change, more for support people as of now
		}
	}
}
