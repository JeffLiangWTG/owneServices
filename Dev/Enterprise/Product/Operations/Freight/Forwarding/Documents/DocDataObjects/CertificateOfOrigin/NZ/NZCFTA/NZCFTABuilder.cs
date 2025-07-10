using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Address;
using AddressType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Address.AddressType;
using AddressValidation = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base.AddressValidation;
using InvoiceType = Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Invoice.InvoiceType.InvoiceTypes;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.NZ
{
	sealed class NZCFTABuilder : CertificateOfOriginBuilder<NZCFTA, NZCFTALineItem, OriginCriterionListNZCFTA>
	{
		protected override NZCFTA CreateCertificate(ZString sourceType, ZString sourceId) => new NZCFTA(sourceType, sourceId);
		protected override NZCFTALineItem CreateLineItem(object lineItemId) => new NZCFTALineItem(lineItemId);
		public override string GetDefaultOriginCriterionCode() => OriginCriterionListNZCFTA.Codes.WO;
		public override ZString HarmonisedCodeCountry { get; } = Core.Constants.CountryCodes.NewZealand;
		public override ZString ShipmentDocumentName { get; } = ShipmentDocumentNames.NZCFTACertificateOfOrigin;

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
			ValidatePortOfOrigin = true,
			ValidatePortOfDestination = true,

			ValidateTransportReference = true,

			ValidateDepartureDate = true,
			ValidateArrivalDate = true,

			InvoiceType = InvoiceType.Complete,

			ValidateSignature = false,

			ValidateLineItemOrigin = false,
			ValidateLineItemOriginCriterion = true,
			ValidateLineItemItemNumber = true,
			ValidateLineItemWeight = false, // adding custom validation
		};

		public NZCFTABuilder(ForwardingShipment shipment) : base(shipment)
		{
		}

		#region Implementation

		protected override void Populate(NZCFTA nzcfta)
		{
			nzcfta.IsNewZealandToChina = IsNewZealandToChina(
				nzcfta.PortOfOrigin?.Country?.Code,
				nzcfta.PortOfDestination?.Country?.Code);
		}

		protected override void PopulateLineItem(BusinessObject brokerageInvoiceLineBO, ForwardingPackLine shipmentPackline, NZCFTALineItem outboundCertificatePackline)
		{
			var quantityUnitList = new NZCustomsTariffQuantityUnitList();
			var quantityUnitCodes = quantityUnitList.GetAllCodesZString().ToList();

			if (brokerageInvoiceLineBO != null)
			{
				var brokerageInvoiceLine = (BaseJobComInvoiceLine)brokerageInvoiceLineBO;
				outboundCertificatePackline.Quantity = new NZMeasurement(brokerageInvoiceLine.JI_Weight, brokerageInvoiceLine.JI_WeightUQ);
				outboundCertificatePackline.QuantityUnit = new CodeDescription(quantityUnitList)
				{
					Code = GetQuanityUnit(quantityUnitCodes, brokerageInvoiceLine.JI_InvoiceUQ)
				};
				outboundCertificatePackline.QuantityNumber = brokerageInvoiceLine.JI_InvoiceQuantity.ToZInt();
			}
			else
			{
				outboundCertificatePackline.Quantity = new NZMeasurement(shipmentPackline.JL_ActualWeight, shipmentPackline.JL_ActualWeightUQ);
				outboundCertificatePackline.QuantityUnit = new CodeDescription(quantityUnitList)
				{
					Code = GetQuanityUnit(quantityUnitCodes, shipmentPackline.JL_F3_NKPackType),
				};
				outboundCertificatePackline.QuantityNumber = shipmentPackline.JL_PackageCount;
			}
		}

		ZString GetQuanityUnit(IEnumerable<ZString> newZealandCustomsTariffQuantityUnitCodes, ZString packType)
		{
			if (NZCustomsTariffQuantityUnitList.NumberOfPacksCodes.Contains(packType))
			{
				return NZCustomsTariffQuantityUnitList.Codes.NMP;
			}

			if (newZealandCustomsTariffQuantityUnitCodes.Contains(packType))
			{
				return packType;
			}

			return ZString.Empty;
		}

		protected override void PopulateAddresses(NZCFTA nzcfta)
		{
			base.PopulateAddresses(nzcfta);

			nzcfta.AddressCollection = GetAddressBusinessObjectCollection(nzcfta.ExporterAddress, nzcfta.ProducerAddress);
		}

		AddressBusinessObjectConfiguration GetAddressBusinessObjectCollection(Address exporterAddress, Address producerAddress) => new AddressBusinessObjectConfiguration(certificateName: "NZCFTA", enableUnknown: true)
		{
			new AddressBusinessObject(exporterAddress, AddressType.EXPORTER),
			new AddressBusinessObject(producerAddress, AddressType.PRODUCER)
		};

		#endregion

		#region Validation

		protected override void AddLineItemValidation(NZCFTALineItem lineItem)
		{
			var quantityUnitList = new NZCustomsTariffQuantityUnitList();
			var weightUnitList = new NZCustomsTariffQuantityWeightUnits();

			if (lineItem.Quantity is NZMeasurement quantity &&
				quantity.Unit is CodeDescription weightUnit)
			{
				quantity.ValueInfo.AddMessageError(() =>
					quantity.Value <= ZDecimal.Zero &&
					lineItem.QuantityNet.Value <= 0,
					Res.GetString("c4a3ed9f-8fcb-4686-9afc-942f84371708", "Weight is mandatory and must be greater than zero."));

				weightUnit.CodeInfo.AddMessageError(() =>
					string.IsNullOrEmpty(weightUnit.Code) &&
					quantity.Value > 0,
					Res.GetString("A867FC0D-E756-40AD-9F24-606E8E493652", "Gross weight quantity value and unit is mandatory."));

				weightUnit.CodeInfo.AddMessageError(() =>
					weightUnitList[weightUnit.Code] == null &&
					quantity.Value > 0,
					Res.GetString("d6cc9278-6140-402b-8992-b7d4a6514d59", "Gross Weight must be converted to an NZ Customs accepted unit."));

				lineItem.QuantityNet.ValueInfo.ValueChanged += (sender, e)
					=> lineItem.Validate(nameof(lineItem.Quantity.Value));

				quantity.ValueInfo.ValueChanged += (sender, e)
					=> lineItem.Validate(nameof(lineItem.Quantity.Unit.Code));
			}

			var quantityUnit = (CodeDescription)lineItem.QuantityUnit;
			quantityUnit?.CodeInfo.AddMessageErrorIfEmpty(Res.GetString("325b753f-a122-46b5-8bc9-3e2f1213dc7c", "Quantity Unit Type required in Box 12."));
			quantityUnit?.CodeInfo.AddMessageError(() => quantityUnitList[lineItem.QuantityUnit.Code] == null, Res.GetString("0aa565b5-c027-454a-b3fc-c6927dff0475", $"Quantity Unit Type must be a NZ Customs accepted Tariff Quantity Unit."));

			lineItem.QuantityNumberInfo.AddMessageError(() => lineItem.QuantityNumber <= 0, Res.GetString("008cc3fe-4cd2-41f8-9e00-45ce23a66ed1", "Quantity Number required in Box 12."));
		}

		protected override void AddSignatureValidation(NZCFTA nzcfta)
		{
			nzcfta.SignatureErrorInfo.AddMessageError(
				() => !IsNewZealandToChina(shipment.Origin?.Country?.Code, shipment.Destination?.Country?.Code)
					&& nzcfta.Signature.Signature == null,
				Res.GetString("be46daa8-a5f4-453a-840f-17818654c3b7", "Signature required, please upload to your CW1 profile."));
		}

		protected override void AddValidation(NZCFTA nzcfta)
		{
			nzcfta.ProducerAddress?.AddressFormattedInfo.AddMessageError(
				() => !nzcfta.ProducerAddressState.IsUnknown && !nzcfta.ProducerAddressState.IsSameAsExporter && string.IsNullOrWhiteSpace(nzcfta.ProducerAddress.City),
				Res.GetString("9e55c318-6085-4798-87a2-525d49d46e87", "Producer City is a mandatory field for a JEVS submission"));

			nzcfta.ProducerAddressState.IsUnknownInfo.ValueChanged += (object sender, System.EventArgs e) => { nzcfta.ProducerAddress.Validate(nameof(nzcfta.ProducerAddress.AddressFormatted)); };
			nzcfta.ProducerAddressState.IsSameAsExporterInfo.ValueChanged += (object sender, System.EventArgs e) => { nzcfta.ProducerAddress.Validate(nameof(nzcfta.ProducerAddress.AddressFormatted)); };
		}

		#endregion
	}
}
