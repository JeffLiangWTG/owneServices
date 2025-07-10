using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.CodeLists;
using AddressValidation = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base.AddressValidation;
using InvoiceType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice.InvoiceType.InvoiceTypes;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	sealed class COOUSBuilder : CertificateOfOriginBuilder<COOUS, COOUSLineItem, OriginCriterionListEmpty>
	{
		protected override COOUS CreateCertificate(ZString sourceType, ZString sourceId) => new COOUS(sourceType, sourceId);
		protected override COOUSLineItem CreateLineItem(object lineItemId) => new COOUSLineItem(lineItemId);
		public override string GetDefaultOriginCriterionCode() => string.Empty;
		public override ZString ShipmentDocumentName { get; } = ShipmentDocumentNames.COOUSCertificateOfOrigin;

		public override CertificateOfOriginBuilderFlags InitialFlags => new CertificateOfOriginBuilderFlags
		{
			ValidateProducerPhone = ContactValidation.Never,
			ValidateProducerEmail = ContactValidation.Never,
			ValidateProducerAddress = AddressValidation.None,

			ValidateConsignorPhone = false,
			ValidateConsignorEmail = false,

			ValidateConsigneePhone = ContactValidation.Never,
			ValidateConsigneeEmail = ContactValidation.Never,
			ValidateConsigneeAddress = true,

			ValidatePortOfLoading = false,
			ValidatePortOfDischarge = false,
			ValidatePortOfOrigin = false,
			ValidatePortOfDestination = false,

			ValidateTransportReference = false,

			ValidateDepartureDate = false,
			ValidateArrivalDate = false,

			InvoiceType = InvoiceType.None,

			ValidateSignature = true,

			ValidateLineHarmonizedCode = false,
			ValidateLineItemOrigin = true,
			ValidateLineItemOriginCriterion = false,
			ValidateLineItemItemNumber = true,
			ValidateLineItemWeight = true,

			ValidateCurrentUserCity = false,
		};

		public COOUSBuilder(ForwardingShipment shipment) : base(shipment)
		{
		}

		#region Implementation

		protected override void Populate(COOUS certificate)
		{
			if (certificate is null)
			{
				throw new ArgumentNullException(nameof(certificate));
			}
			certificate.ShipmentNumber = shipment.JS_UniqueConsignRef;

			certificate.LineItemOriginCountries = string.Join(", ", certificate.LineItems.Select(i => i.Origin).Distinct()).Trim().Trim(',');
		}

		protected override void AddValidation(COOUS certificate)
		{
			certificate.ShipmentNumberInfo.AddMessageError(
				() => string.IsNullOrWhiteSpace(certificate.ShipmentNumber),
				Res.GetString("A463A3E6-5661-49BB-B669-1179FFEC7A38", "Shipment Number is required"));
		}

		#endregion
	}
}
