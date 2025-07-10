using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Address;
using Enterprise.MasterFiles.Business;
using AddressValidation = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base.AddressValidation;
using InvoiceType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice.InvoiceType.InvoiceTypes;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin
{
	sealed class RCEPBuilder : CertificateOfOriginBuilder<RCEP, RCEPLineItem, OriginCriterionListRCEP>
	{
		protected override RCEP CreateCertificate(ZString sourceType, ZString sourceId) => new RCEP(sourceType, sourceId);

		protected override RCEPLineItem CreateLineItem(object lineItemId) => new RCEPLineItem(lineItemId)
		{
			FOB = new Money
			{
				Currency = new CodeDescription(context.Currencies as IFindBoxListProvider)
				{
					Code = GlbCompany.CurrentCompany.LocalCurrency.Code
				}
			}
		};

		public override string GetDefaultOriginCriterionCode() => OriginCriterionListRCEP.Codes.WO;
		public override ZString ShipmentDocumentName { get; } = ShipmentDocumentNames.RCEPCertificateOfOrigin;

		public override CertificateOfOriginBuilderFlags InitialFlags => new CertificateOfOriginBuilderFlags
		{
			ValidateProducerAddress = AddressValidation.Complex,

			ValidateConsigneeAddress = true,
			ValidateConsigneeEmail = ContactValidation.Never,
			ValidateConsigneePhone = ContactValidation.Never,

			ValidateConsignorEmail = false,
			ValidateConsignorPhone = false,

			ValidatePortOfLoading = true,
			ValidatePortOfDischarge = true,
			ValidatePortOfOrigin = false,
			ValidatePortOfDestination = true,

			ValidateTransportReference = true,

			ValidateDepartureDate = true,
			ValidateArrivalDate = false,

			InvoiceType = InvoiceType.Short,

			ValidateSignature = true,

			ValidateLineItemOrigin = true,
			ValidateLineItemOriginCriterion = true,
			ValidateLineItemItemNumber = true,
			ValidateLineItemWeight = true,
		};

		protected override string InvalidPacklineOriginError => Res.GetString("981A8B05-9894-4FD4-AB56-4BF86C0E7DAE", "RCEP Country/Region of Origin is required.");

		public RCEPBuilder(ForwardingShipment shipment) : base(shipment)
		{
		}

		protected override void PopulateAddresses(RCEP rcep)
		{
			base.PopulateAddresses(rcep);

			rcep.AddressCollection = GetAddressBusinessObjectCollection(rcep.ExporterAddress, rcep.ProducerAddress);
		}

		AddressBusinessObjectConfiguration GetAddressBusinessObjectCollection(Address exporterAddress, Address producerAddress) => new AddressBusinessObjectConfiguration(certificateName: "RCEP", enableUnknown: true)
		{
			new AddressBusinessObject(exporterAddress, AddressType.EXPORTER),
			new AddressBusinessObject(producerAddress, AddressType.PRODUCER)
		};

		#region Validation

		protected override bool IsTransportReferenceInvalid(RCEP certificate)
			=> IsNewZealandToChina(certificate.PortOfOrigin?.Country?.Code, certificate.PortOfDestination?.Country?.Code) && base.IsTransportReferenceInvalid(certificate);

		protected override void AddValidation(RCEP certificate)
		{
			certificate.AddValidationDependencies(certificate.RemarksInfo, certificate.IsSubjectOfThirdPartyInvoiceInfo);
			certificate.RemarksInfo.AddMessageError(() => certificate.IsSubjectOfThirdPartyInvoice && string.IsNullOrWhiteSpace(certificate.Remarks), Res.GetString("D2B8532A-0D5A-499C-AD57-2F7B20CB164A", "The name and country/region of the company issuing the invoice should be provided in Remarks."));

			certificate.AddValidationDependencies(certificate.RemarksInfo, certificate.IsBacktobackCertificateOfOriginInfo);
			certificate.RemarksInfo.AddMessageError(() => certificate.IsBacktobackCertificateOfOrigin && string.IsNullOrWhiteSpace(certificate.Remarks), Res.GetString("46d03659-9171-4ff7-88fb-6ede5e2f2e36", "The Original Proof of Origin reference no.; date of issuance; issuing country; RCEP country of origin of the first exporting party; and if applicable, the approved exporter authorization code of the first exporting Party should be entered in Remarks Box 14."));

			AddLineItemsValidation(certificate);
		}

		void AddLineItemsValidation(RCEP certificate)
		{
			foreach (var lineItem in certificate.LineItems)
			{
				lineItem.FOB.AmountInfo.AddMessageError(
					() => lineItem.OriginCriterion.Code == OriginCriterionListRCEP.Codes.RVC && lineItem.FOB.Amount == 0,
					Res.GetString(
						"a9a10814-93e6-4a04-bf6d-3da4b56c8a76",
						"The FOB value is required for goods with Origin Conferring Criterion. Enter the FOB value in the Quantity box."));
				((CodeDescription)lineItem.OriginCriterion).CodeInfo.ValueChanged += (object sender, System.EventArgs e) => lineItem.FOB.Validate(nameof(lineItem.FOB.Amount));
			}
		}

		#endregion
	}
}
