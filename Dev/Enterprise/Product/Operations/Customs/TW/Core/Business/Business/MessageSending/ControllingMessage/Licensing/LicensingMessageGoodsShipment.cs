using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class LicensingMessageGoodsShipment : IGoodsShipment
	{
		protected CusTWControllingMessageHeader Header { get; }

		protected JobDeclaration Declaration { get; }

		public LicensingMessageGoodsShipment(CusTWControllingMessageHeader header)
		{
			Header = Argument.NotNull(header, nameof(header));
			Declaration = Argument.NotNull(header.Declaration, nameof(header.Declaration));
		}

		IEnumerable<IAdditionalDocument> IGoodsShipment.AdditionalDocuments => null;

		IEnumerable<IGovernmentAgencyGoodsItem> IGoodsShipment.GovernmentAgencyGoodsItems => Header.ControllingMessageHeaderLinkInvoiceLines.Where(x => x.Link).Select((x, index) => GenerateLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCore(index + 1, Header, x.Invoiceline));

		protected virtual LicensingMessageGoodsShipmentGovernmentAgencyGoodsItem GenerateLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCore(ZInt sequenceNumeric, CusTWControllingMessageHeader header, JobComInvoiceLine invoiceLine)
		{
			return new LicensingMessageGoodsShipmentGovernmentAgencyGoodsItem(sequenceNumeric, header, invoiceLine);
		}

		ZDateTime IGoodsShipment.ExitDateTime => GetExitDateTimeCore();

		protected virtual ZDateTime GetExitDateTimeCore() => Declaration.JE_ExportDate;

		ZDecimal IGoodsShipment.ItemChargeAmount => GetItemChargeAmountCore();

		protected virtual ZDecimal GetItemChargeAmountCore() => Declaration.EntryHeader?.CH_TotalIMPFOBAmountInInvoiceCurrency ?? default;

		ZDecimal IGoodsShipment.TotalCIFAmount => ZDecimal.Zero;

		IPartyDetails IGoodsShipment.Consignee => GetConsigneeCore();

		protected virtual IPartyDetails GetConsigneeCore()
		{
			IPartyDetails result = null;
			if (Declaration.IntermConsignee is OrgHeader consigneeHeader && consigneeHeader.MainAddress is OrgAddress address)
			{
				var consigneeID = ZString.Empty;
				var consigneeTypeCode = ZString.Empty;
				if (ConsigneeIdAndTypeCodeIsRequired)
				{
					var idAndType = PartyHelper.GetConsigneeIdAndTypeCode(consigneeHeader);
					consigneeID = idAndType.CustomsRegNo;
					consigneeTypeCode = idAndType.TypeCode;
				}
				result = new PartyDetailsWrapper(consigneeID, ZString.Empty, ZString.Empty, consigneeTypeCode, address);
			}
			return result;
		}

		protected virtual bool ConsigneeIdAndTypeCodeIsRequired => false;

		IConsignment IGoodsShipment.Consignment => GetConsignmentCore();

		protected virtual IConsignment GetConsignmentCore() => new LicensingMessageGoodsShipmentConsignment(Header);

		IPartyDetails IGoodsShipment.Consignor => new PartyDetailsWrapper(ZString.Empty, ZString.Empty, ZString.Empty, null);

		ICustomsValuation IGoodsShipment.CustomsValuation => null;

		ZString IGoodsShipment.DeliveryDestinationName => null;

		IEnumerable<IGoodsShipmentDutyTaxFee> IGoodsShipment.DutyTaxFees => null;

		IPartyDetails IGoodsShipment.NotifyParty => new PartyDetailsWrapper(ZString.Empty, ZString.Empty, ZString.Empty, null);

		IPartyDetails IGoodsShipment.Seller => GetSellerCore();

		ZString IGoodsShipment.TradeTermsConditionCode => null;

		protected virtual IPartyDetails GetSellerCore() => Header.SupplierDocumentaryAddress is TWJobDocAddress docAddress ? new LicensingMessagePartyDetailsWrapper(docAddress) : null;

		ZString IGoodsShipment.UCR => null;

		IPartyDetails IGoodsShipment.Buyer => GetBuyerCore();

		protected virtual IPartyDetails GetBuyerCore()
		{
			IPartyDetails result = null;
			var importererDocumentaryAddress = Header.ImporterDocumentaryAddress;
			if (importererDocumentaryAddress != null && (importererDocumentaryAddress.E2_AddressOverride || importererDocumentaryAddress.HasRealOrganisation))
			{
				result = new Buyer(Declaration, importererDocumentaryAddress.Organisation, importererDocumentaryAddress);
			}
			return result;
		}

		IPartyDetails IGoodsShipment.Exporter => GetExporterCore();

		protected virtual IPartyDetails GetExporterCore() => new LicensingMessagePartyDetailsWrapper(Header.SupplierDocumentaryAddress, Header.IsCertificate15);

		IEnumerable<IGoodsMeasure> IGoodsShipment.GoodsMeasures => null;

		IEnumerable<IAdditionalInformation> IGoodsShipment.AdditionalInformations => null;

		IEnumerable<IAdditionalDeclaration> IGoodsShipment.AdditionalDeclarations => null;
	}
}
