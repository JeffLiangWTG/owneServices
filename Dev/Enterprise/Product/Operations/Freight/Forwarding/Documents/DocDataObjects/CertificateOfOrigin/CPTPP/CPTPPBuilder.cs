using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Address;
using InvoiceType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice.InvoiceType.InvoiceTypes;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	sealed class CPTPPBuilder : CertificateOfOriginBuilder<CPTPP, CPTPPLineItem, OriginCriterionListCPTPP>
	{
		public CPTPPBuilder(ForwardingShipment shipment) : base(shipment) { }

		protected override CPTPP CreateCertificate(ZString sourceType, ZString sourceId) => new CPTPP(sourceType, sourceId);
		protected override CPTPPLineItem CreateLineItem(object lineItemId) => new CPTPPLineItem(lineItemId);
		public override string GetDefaultOriginCriterionCode() => OriginCriterionListCPTPP.Codes.WO;
		public override ZString ShipmentDocumentName { get; } = ShipmentDocumentNames.CPTPPCertificateOfOrigin;

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

		protected override void PopulateAddresses(CPTPP cptpp)
		{
			base.PopulateAddresses(cptpp);

			cptpp.AddressCollection = GetAddressBusinessObjectCollection(cptpp.ExporterAddress, cptpp.ProducerAddress);
		}

		AddressBusinessObjectConfiguration GetAddressBusinessObjectCollection(Address exporterAddress, Address producerAddress) => new AddressBusinessObjectConfiguration(certificateName: "CPTPP", enableUnknown: false)
		{
			new AddressBusinessObject(exporterAddress, AddressType.EXPORTER),
			new AddressBusinessObject(producerAddress, AddressType.PRODUCER)
		};
	}
}
