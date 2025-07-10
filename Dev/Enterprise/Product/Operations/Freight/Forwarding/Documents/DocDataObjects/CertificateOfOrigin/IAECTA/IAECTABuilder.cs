using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.CodeLists;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Address;
using InvoiceType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice.InvoiceType.InvoiceTypes;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	sealed class IAECTABuilder : CertificateOfOriginBuilder<IAECTA, IAECTALineItem, OriginCriterionListIAECTA>
	{
		protected override IAECTA CreateCertificate(ZString sourceType, ZString sourceId) => new IAECTA(sourceType, sourceId);
		protected override IAECTALineItem CreateLineItem(object packlineId) => new IAECTALineItem(packlineId);
		public override string GetDefaultOriginCriterionCode() => OriginCriterionListIAECTA.Codes.WO;
		public override ZString ShipmentDocumentName { get; } = ShipmentDocumentNames.IAECTACertificateOfOrigin;

		public override CertificateOfOriginBuilderFlags InitialFlags => new CertificateOfOriginBuilderFlags
		{
			ValidateProducerAddress = AddressValidation.FullIfDifferentFromConsignor,

			ValidateConsigneeAddress = false,
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
			ValidateLineItemItemNumber = true,
			ValidateLineItemWeight = true,
		};

		public IAECTABuilder(ForwardingShipment shipment) : base(shipment)
		{
		}

		protected override void PopulateAddresses(IAECTA iaecta)
		{
			base.PopulateAddresses(iaecta);

			iaecta.AddressCollection = GetAddressBusinessObjectCollection(iaecta.ExporterAddress, iaecta.ProducerAddress);
		}

		AddressBusinessObjectConfiguration GetAddressBusinessObjectCollection(Address exporterAddress, Address producerAddress) => new AddressBusinessObjectConfiguration(certificateName: "IAECTA", enableUnknown: true)
		{
			new AddressBusinessObject(exporterAddress, AddressType.EXPORTER),
			new AddressBusinessObject(producerAddress, AddressType.PRODUCER)
		};

		#region Validation

		protected override void AddValidation(IAECTA certificate)
		{
			certificate.ThirdPartyInvoiceIssuerInfo.AddMessageError(
				() => certificate.IsSubjectOfThirdPartyInvoice && string.IsNullOrWhiteSpace(certificate.ThirdPartyInvoiceIssuer),
				Enterprise.Freight.Forwarding.Documents.DataObjects.Res.GetString("CC525E31-E8FF-46EF-9D9B-0121752D26E5", "Third party invoice issuer details are required."));

			certificate.IsSubjectOfThirdPartyInvoiceInfo.ValueChanged += (sender, e) => { certificate.Validate(nameof(certificate.ThirdPartyInvoiceIssuer));  };
		}

		#endregion
	}
}
