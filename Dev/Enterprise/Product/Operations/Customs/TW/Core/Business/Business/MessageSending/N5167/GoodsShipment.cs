using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.N5167
{
	public class GoodsShipment : IGoodsShipment
	{
		public GoodsShipment(CusEntryHeader header)
		{
			this.header = header;
		}

		readonly CusEntryHeader header;

		public IEnumerable<IAdditionalDocument> AdditionalDocuments => null;

		public IEnumerable<IGovernmentAgencyGoodsItem> GovernmentAgencyGoodsItems
		{
			get
			{
				yield return new GovernmentAgencyGoodsItem(header);
			}
		}

		public ZDateTime ExitDateTime => ZDateTime.Empty;

		public ZDecimal ItemChargeAmount => ZDecimal.Zero;

		public ZDecimal TotalCIFAmount => ZDecimal.Zero;

		public IPartyDetails Consignee => null;

		public IConsignment Consignment => null;

		public IPartyDetails Consignor => null;

		public ICustomsValuation CustomsValuation => null;

		public ZString DeliveryDestinationName => ZString.Empty;

		public IEnumerable<IGoodsShipmentDutyTaxFee> DutyTaxFees => null;

		public IPartyDetails NotifyParty => null;

		public IPartyDetails Seller => null;

		public ZString TradeTermsConditionCode => ZString.Empty;

		public ZString UCR => ZString.Empty;

		public IPartyDetails Buyer => null;

		public IPartyDetails Exporter => null;

		public IEnumerable<IGoodsMeasure> GoodsMeasures => null;

		public IEnumerable<IAdditionalInformation> AdditionalInformations => null;

		public IEnumerable<IAdditionalDeclaration> AdditionalDeclarations => null;
	}
}
