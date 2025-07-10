using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.CodeLists;
using InvoiceType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice.InvoiceType.InvoiceTypes;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	sealed class PAFTABuilder : CertificateOfOriginBuilder<PAFTA, PAFTALineItem, OriginCriterionListPAFTA>
	{
		protected override PAFTA CreateCertificate(ZString sourceType, ZString sourceId) => new PAFTA(sourceType, sourceId);
		protected override PAFTALineItem CreateLineItem(object lineItemId) => new PAFTALineItem(lineItemId);
		public override string GetDefaultOriginCriterionCode() => OriginCriterionListPAFTA.Codes.WO;
		public override ZString ShipmentDocumentName { get; } = ShipmentDocumentNames.PAFTACertificateOfOrigin;

		public override CertificateOfOriginBuilderFlags InitialFlags => new CertificateOfOriginBuilderFlags
		{
			ValidateProducerAddress = AddressValidation.None,

			ValidateConsigneeAddress = true,
			ValidateConsigneeEmail = ContactValidation.Always,
			ValidateConsigneePhone = ContactValidation.Always,

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
			ValidateLineItemItemNumber = true,
			ValidateLineItemWeight = true,
		};

		public PAFTABuilder(ForwardingShipment shipment) : base(shipment)
		{
		}
	}
}
