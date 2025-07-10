using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using InvoiceType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice.InvoiceType.InvoiceTypes;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	sealed class AanzftaBuilder : CertificateOfOriginBuilder<Aanzfta, AanzftaLineItem, OriginCriterionListAanzfta>
	{
		readonly CertificateOfOriginBuilderFlags initialFlags;
		protected override Aanzfta CreateCertificate(ZString sourceType, ZString sourceId) => new Aanzfta(sourceType, sourceId) { AgreementInfo = new AgreementInfo() };
		protected override AanzftaLineItem CreateLineItem(object lineItemId) => new AanzftaLineItem(lineItemId);
		public override string GetDefaultOriginCriterionCode() => OriginCriterionList.Codes.WO;
		public override ZString ShipmentDocumentName { get; } = ShipmentDocumentNames.AANZFTACertificateOfOrigin;
		public override CertificateOfOriginBuilderFlags InitialFlags => initialFlags;

		public AanzftaBuilder(ForwardingShipment shipment) : base(shipment)
		{
			initialFlags = new CertificateOfOriginBuilderFlags
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

				ValidateDepartureDate = shipment.JS_RL_NKOrigin.Substring(0, 2).ToUpper() == Constants.CountryCodes.NewZealand,
				ValidateArrivalDate = false,

				InvoiceType = InvoiceType.Short,

				ValidateSignature = true,

				ValidateLineItemOrigin = false,
				ValidateLineItemOriginCriterion = true,
				ValidateLineItemItemNumber = true,
				ValidateLineItemWeight = true
			};
		}

		#region Validation

		protected override void AddValidation(Aanzfta aanzfta)
		{
			aanzfta.ThirdPartyInvoiceIssuerInfo.AddMessageError(
				() => aanzfta.IsSubjectOfThirdPartyInvoice && string.IsNullOrWhiteSpace(aanzfta.ThirdPartyInvoiceIssuer),
				Enterprise.Freight.Forwarding.Documents.DataObjects.Res.GetString("E2396285-5D47-496A-8408-47C7D8E584A5", "Third party invoice issuer details are required."));

			aanzfta.IsSubjectOfThirdPartyInvoiceInfo.ValueChanged += (sender, e) => { aanzfta.Validate(nameof(aanzfta.ThirdPartyInvoiceIssuer)); };
		}

		#endregion
	}
}
