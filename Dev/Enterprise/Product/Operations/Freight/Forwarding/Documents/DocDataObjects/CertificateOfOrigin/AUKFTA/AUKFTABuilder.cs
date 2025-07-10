using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.CodeLists;
using InvoiceType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice.InvoiceType.InvoiceTypes;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	sealed class AUKFTABuilder : CertificateOfOriginBuilder<AUKFTA, AUKFTALineItem, OriginCriterionListAUKFTA>
	{
		protected override AUKFTA CreateCertificate(ZString sourceType, ZString sourceId) => new AUKFTA(sourceType, sourceId);
		protected override AUKFTALineItem CreateLineItem(object lineItemId) => new AUKFTALineItem(lineItemId);
		public override string GetDefaultOriginCriterionCode() => OriginCriterionListAUKFTA.Codes.WO;

		public override ZString ShipmentDocumentName { get; } = ShipmentDocumentNames.AUKFTACertificateOfOrigin;

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
			ValidatePortOfOrigin = false,
			ValidatePortOfDestination = false,

			ValidateTransportReference = false,

			ValidateDepartureDate = false,
			ValidateArrivalDate = false,

			InvoiceType = InvoiceType.None,

			ValidateSignature = true,

			ValidateLineItemOrigin = false,
			ValidateLineItemOriginCriterion = true,
			ValidateLineItemItemNumber = true,
			ValidateLineItemWeight = true,
		};

		public AUKFTABuilder(ForwardingShipment shipment) : base(shipment)
		{
		}

		#region Implementation

		protected override void Populate(AUKFTA certificate) => certificate.DeclarationCompletedBy = new();

		protected override void AddValidation(AUKFTA certificate) => certificate.DeclarationCompletedBy.ValueInfo
			.AddMessageErrorIfEmpty(Res.GetString("3c30d2e3-85a3-4571-8428-4832caa5b7a7","You must indicate who the declaration was completed by."));

		#endregion
	}
}
