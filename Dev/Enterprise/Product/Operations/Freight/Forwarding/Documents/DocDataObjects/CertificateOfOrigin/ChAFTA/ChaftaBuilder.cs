using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using AddressValidation = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base.AddressValidation;
using InvoiceType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice.InvoiceType.InvoiceTypes;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	sealed class ChaftaBuilder : CertificateOfOriginBuilder<Chafta, ChaftaLineItem, OriginCriterionList>
	{
		protected override Chafta CreateCertificate(ZString sourceType, ZString sourceId) => new Chafta(sourceType, sourceId);
		protected override ChaftaLineItem CreateLineItem(object lineItemId) => new ChaftaLineItem(lineItemId);
		public override ZString HarmonisedCodeCountry { get; } = Core.Constants.CountryCodes.Australia;
		public override string GetDefaultOriginCriterionCode() => OriginCriterionList.Codes.WO;
		public override ZString ShipmentDocumentName { get; } = ShipmentDocumentNames.ChaftaCertificateOfOrigin;

		public override CertificateOfOriginBuilderFlags InitialFlags => new CertificateOfOriginBuilderFlags
		{
			ValidateProducerAddress = AddressValidation.None,

			ValidateConsigneeAddress = false,
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
			
			InvoiceType = InvoiceType.Short,

			ValidateSignature = true,

			ValidateLineItemOrigin = false,
			ValidateLineItemOriginCriterion = true,
			ValidateLineItemItemNumber = true,
			ValidateLineItemWeight = true,
		};

		public ChaftaBuilder(ForwardingShipment shipment) : base(shipment)
		{
		}
	}
}
