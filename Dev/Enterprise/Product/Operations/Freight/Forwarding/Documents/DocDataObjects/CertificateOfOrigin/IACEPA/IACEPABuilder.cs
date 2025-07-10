using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using InvoiceType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice.InvoiceType.InvoiceTypes;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	sealed class IACEPABuilder : CertificateOfOriginBuilder<IACEPA, IACEPALineItem, OriginCriterionListIACEPA>
	{
		protected override IACEPA CreateCertificate(ZString sourceType, ZString sourceId) => new IACEPA(sourceType, sourceId);
		protected override IACEPALineItem CreateLineItem(object packlineId) => new IACEPALineItem(packlineId);
		public override string GetDefaultOriginCriterionCode() => OriginCriterionListIACEPA.Codes.WO;
		public override ZString ShipmentDocumentName { get; } = ShipmentDocumentNames.IACEPACertificateOfOrigin;

		public override CertificateOfOriginBuilderFlags InitialFlags => new CertificateOfOriginBuilderFlags
		{
			ValidateProducerAddress = AddressValidation.None,

			ValidateConsigneeAddress = true,
			ValidateConsigneeEmail = ContactValidation.Never,
			ValidateConsigneePhone = ContactValidation.Never,

			ValidateConsignorEmail = true,
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
			ValidateLineItemWeight = false,
			ValidateLineItemMarksAndNumbers = false
		};

		public IACEPABuilder(ForwardingShipment shipment) : base(shipment)
		{
		}

		#region Validation

		protected override void AddValidation(IACEPA iacepa)
		{
			iacepa.ThirdPartyInvoiceIssuerInfo.AddMessageError(
				() => (iacepa.IsSubjectOfThirdPartyInvoice && string.IsNullOrWhiteSpace(iacepa.ThirdPartyInvoiceIssuer)),
				Res.GetString("69c8b23e-4fdd-402d-bdf4-7e4ea1992062", "Third Party Invoice detail is required."));

			iacepa.IsSubjectOfThirdPartyInvoiceInfo.ValueChanged += (sender, e) => { iacepa.Validate(nameof(iacepa.ThirdPartyInvoiceIssuer)); };

			iacepa.ExhibitionDetailInfo.AddMessageError(
				() => (iacepa.IsExhibition && string.IsNullOrWhiteSpace(iacepa.ExhibitionDetail)),
				Res.GetString("aa0caf60-71f9-4794-ad5e-c8a0e95736f2", "Exhibition detail is required."));

			iacepa.IsExhibitionInfo.ValueChanged += (sender, e) => { iacepa.Validate(nameof(iacepa.ExhibitionDetail)); };
		}

		#endregion
	}
}
