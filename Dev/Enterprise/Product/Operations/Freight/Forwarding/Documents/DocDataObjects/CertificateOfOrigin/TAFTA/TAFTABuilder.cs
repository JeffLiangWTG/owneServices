using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.CodeLists;
using InvoiceType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice.InvoiceType.InvoiceTypes;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	sealed class TAFTABuilder : CertificateOfOriginBuilder<TAFTA, TAFTALineItem, OriginCriterionListTAFTA>
	{
		protected override TAFTA CreateCertificate(ZString sourceType, ZString sourceId) => new TAFTA(sourceType, sourceId);
		protected override TAFTALineItem CreateLineItem(object lineItemId) => new TAFTALineItem(lineItemId);
		public override string GetDefaultOriginCriterionCode() => OriginCriterionListTAFTA.Codes.WO;
		public override ZString ShipmentDocumentName { get; } = ShipmentDocumentNames.TAFTACertificateOfOrigin;

		public override CertificateOfOriginBuilderFlags InitialFlags => new CertificateOfOriginBuilderFlags
		{
			ValidateProducerAddress = AddressValidation.None,

			ValidateConsigneeAddress = true,
			ValidateConsigneeEmail = ContactValidation.Always,
			ValidateConsigneePhone = ContactValidation.Always,

			ValidateConsignorEmail = false,
			ValidateConsignorPhone = false,

			ValidatePortOfLoading = true,
			ValidatePortOfDischarge = true,
			ValidatePortOfOrigin = false,
			ValidatePortOfDestination = false,

			ValidateTransportReference = true,

			ValidateDepartureDate = true,
			ValidateArrivalDate = false,

			InvoiceType = InvoiceType.None,

			ValidateSignature = true,

			ValidateLineItemOrigin = false,
			ValidateLineItemOriginCriterion = true,
			ValidateLineItemItemNumber = false,
			ValidateLineItemWeight = false,
			ValidateLineItemMarksAndNumbers = false
		};

		public TAFTABuilder(ForwardingShipment shipment) : base(shipment)
		{
		}

		#region Implementation

		protected override void PopulateAddresses(TAFTA tafta)
		{
			base.PopulateAddresses(tafta);

			tafta.BuyerAddress = AddressBuilder
				.Create(context, shipment?.BuyerDocAddress)
				.AddAsAgentInfoToCompanyName(shipment?.BuyerDocAddress);
		}

		protected override void PopulateLineItem(BusinessObject brokerageInvoiceLineBO, ForwardingPackLine shipmentPackline, TAFTALineItem outboundCertificatePackline)
		{
			base.PopulateLineItem(brokerageInvoiceLineBO, shipmentPackline, outboundCertificatePackline);

			outboundCertificatePackline.ProductNumber = brokerageInvoiceLineBO != null ? ((BaseJobComInvoiceLine)brokerageInvoiceLineBO).JI_PartNo : shipmentPackline.Products.PackProductManager.Value;
		}

		#endregion
	}
}
