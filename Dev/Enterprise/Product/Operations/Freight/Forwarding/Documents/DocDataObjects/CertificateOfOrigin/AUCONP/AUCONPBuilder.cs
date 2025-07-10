using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.CodeLists;
using InvoiceType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice.InvoiceType.InvoiceTypes;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	sealed class AUCONPBuilder : CertificateOfOriginBuilder<AUCONP, AUCONPLineItem, OriginCriterionListEmpty>
	{
		protected override AUCONP CreateCertificate(ZString sourceType, ZString sourceId) => new AUCONP(sourceType, sourceId);
		protected override AUCONPLineItem CreateLineItem(object lineItemId) => new AUCONPLineItem(lineItemId);
		public override string GetDefaultOriginCriterionCode() => string.Empty;
		public override ZString ShipmentDocumentName { get; } = ShipmentDocumentNames.AUCONPCertificateOfOrigin;

		public AUCONPBuilder(ForwardingShipment shipment) : base(shipment)
		{
		}

		public override CertificateOfOriginBuilderFlags InitialFlags => new CertificateOfOriginBuilderFlags
		{
			ValidateProducerAddress = AddressValidation.None,

			ValidateConsigneeAddress = true,
			ValidateConsigneeEmail = ContactValidation.Never,
			ValidateConsigneePhone = ContactValidation.Never,

			ValidateConsignorEmail = false,
			ValidateConsignorPhone = false,

			ValidatePortOfLoading = false,
			ValidatePortOfDischarge = false,
			ValidatePortOfOrigin = false,
			ValidatePortOfDestination = true,

			ValidateTransportReference = false,

			ValidateDepartureDate = false,
			ValidateArrivalDate = false,

			InvoiceType = InvoiceType.None,

			ValidateSignature = false,

			ValidateLineItemOrigin = false,
			ValidateLineItemOriginCriterion = false,
			ValidateLineItemItemNumber = true,
			ValidateLineItemWeight = true
		};
	}
}
