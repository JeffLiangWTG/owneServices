using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using InvoiceType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice.InvoiceType.InvoiceTypes;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	sealed class COONZBuilder : CertificateOfOriginBuilder<COONZ, COONZLineItem, OriginCriterionListCOONZ>
	{
		protected override COONZ CreateCertificate(ZString sourceType, ZString sourceId) => new COONZ(sourceType, sourceId);
		protected override COONZLineItem CreateLineItem(object lineItemId) => new COONZLineItem(lineItemId);
		public override ZString HarmonisedCodeCountry => Core.Constants.CountryCodes.NewZealand;
		public override string GetDefaultOriginCriterionCode() => OriginCriterionListCOONZ.Codes.WO;
		public override ZString ShipmentDocumentName { get; } = ShipmentDocumentNames.NZCertificateOfOrigin;
		
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
			ValidatePortOfDestination = false,

			ValidateTransportReference = false,

			ValidateDepartureDate = false,
			ValidateArrivalDate = false,

			InvoiceType = InvoiceType.None,

			ValidateSignature = true,

			ValidateLineItemOrigin = true,
			ValidateLineItemOriginCriterion = false,
			ValidateLineItemItemNumber = false,
			ValidateLineItemWeight = true,
		};

		public COONZBuilder(ForwardingShipment shipment) : base(shipment)
		{
		}

		protected override void Populate(COONZ certificate)
		{
			if (certificate is null)
			{
				throw new ArgumentNullException(nameof(certificate));
			}
			certificate.LineItemOriginCountries = string.Join(", ", certificate.LineItems.Select(i => i.Origin).Distinct()).Trim().Trim(',');
		}
	}
}
