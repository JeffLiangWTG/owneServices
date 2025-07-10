using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using InvoiceType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice.InvoiceType.InvoiceTypes;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	sealed class JAEPABuilder : CertificateOfOriginBuilder<JAEPA, JAEPALineItem, OriginCriterionListJAEPA>
	{
		protected override JAEPA CreateCertificate(ZString sourceType, ZString sourceId) => new JAEPA(sourceType, sourceId);
		protected override JAEPALineItem CreateLineItem(object lineItemId) => new JAEPALineItem(lineItemId);
		public override string GetDefaultOriginCriterionCode() => OriginCriterionListJAEPA.Codes.WO;
		public override ZString ShipmentDocumentName { get; } = ShipmentDocumentNames.JAEPACertificateOfOrigin;

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
			ValidatePortOfOrigin = true,
			ValidatePortOfDestination = true,

			ValidateTransportReference = false,

			ValidateDepartureDate = false,
			ValidateArrivalDate = false,

			InvoiceType = InvoiceType.Short,

			ValidateSignature = true,

			ValidateLineItemOrigin = false,
			ValidateLineItemOriginCriterion = true,
			ValidateLineItemItemNumber = true,
			ValidateLineItemWeight = true,
		};

		public JAEPABuilder(ForwardingShipment shipment) : base(shipment)
		{
		}
	}
}
