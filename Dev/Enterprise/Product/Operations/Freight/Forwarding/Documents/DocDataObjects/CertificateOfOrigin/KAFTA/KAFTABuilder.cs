using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.CodeLists;
using InvoiceType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice.InvoiceType.InvoiceTypes;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	sealed class KAFTABuilder : CertificateOfOriginBuilder<KAFTA, KAFTALineItem, OriginCriterionListKAFTA>
	{
		protected override KAFTA CreateCertificate(ZString sourceType, ZString sourceId) => new KAFTA(sourceType, sourceId);
		protected override KAFTALineItem CreateLineItem(object packlineId) => new KAFTALineItem(packlineId);
		public override string GetDefaultOriginCriterionCode() => OriginCriterionListKAFTA.Codes.WO;
		public override ZString ShipmentDocumentName { get; } = ShipmentDocumentNames.KAFTACertificateOfOrigin;

		public override CertificateOfOriginBuilderFlags InitialFlags => new CertificateOfOriginBuilderFlags
		{
			ValidateProducerAddress = AddressValidation.None,
			ValidateProducerEmail = ContactValidation.IfAddressSet,
			ValidateProducerPhone = ContactValidation.IfAddressSet,

			ValidateConsigneeEmail = ContactValidation.IfAddressSet,
			ValidateConsigneePhone = ContactValidation.IfAddressSet,

			ValidateConsignorEmail = true,
			ValidateConsignorPhone = true,

			ValidatePortOfLoading = false,
			ValidatePortOfDischarge = false,
			ValidatePortOfOrigin = true,
			ValidatePortOfDestination = true,

			ValidateTransportReference = false,

			ValidateDepartureDate = false,
			ValidateArrivalDate = false,

			InvoiceType = InvoiceType.Short,

			ValidateSignature = true,

			ValidateLineItemOrigin = false,
			ValidateLineItemOriginCriterion = true,
			ValidateLineItemItemNumber = true
		};

		public KAFTABuilder(ForwardingShipment shipment) : base(shipment)
		{
		}
	}
}
