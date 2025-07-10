using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.Business;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.NX5901
{
	public class GoodsShipment : IGoodsShipment
	{
		public GoodsShipment(IAdditionalSupportingDocument addtionalDoc)
		{
			AddtionalDoc = addtionalDoc;
		}

		IAdditionalSupportingDocument AddtionalDoc { get; }

		public ZString CriteriaCode => null;

		public ZString PreferentialCriteria => null;

		public ZString ProducerCode => null;

		public ZString OtherCriteria => null;

		public IEnumerable<IAdditionalDeclaration> AdditionalDeclarations => null;

		IEnumerable<IAdditionalDocument> IGoodsShipment.AdditionalDocuments => new GoodsShipmentAdditionalDocumentWrapper().GetDocuments(AddtionalDoc);

		IEnumerable<IGovernmentAgencyGoodsItem> IGoodsShipment.GovernmentAgencyGoodsItems => null;

		ZDateTime IGoodsShipment.ExitDateTime => ZDateTime.Empty;

		ZDecimal IGoodsShipment.ItemChargeAmount => ZDecimal.Zero;

		ZDecimal IGoodsShipment.TotalCIFAmount => ZDecimal.Zero;

		IPartyDetails IGoodsShipment.Consignee => null;

		IConsignment IGoodsShipment.Consignment => null;

		IPartyDetails IGoodsShipment.Consignor => null;

		ICustomsValuation IGoodsShipment.CustomsValuation => null;

		ZString IGoodsShipment.DeliveryDestinationName => null;

		IEnumerable<IGoodsShipmentDutyTaxFee> IGoodsShipment.DutyTaxFees => null;

		IPartyDetails IGoodsShipment.NotifyParty => null;

		IPartyDetails IGoodsShipment.Seller => null;

		ZString IGoodsShipment.TradeTermsConditionCode => null;

		ZString IGoodsShipment.UCR => null;

		IPartyDetails IGoodsShipment.Buyer => null;

		IPartyDetails IGoodsShipment.Exporter => null;

		IEnumerable<IGoodsMeasure> IGoodsShipment.GoodsMeasures => null;

		IEnumerable<IAdditionalInformation> IGoodsShipment.AdditionalInformations => null;
	}
}
