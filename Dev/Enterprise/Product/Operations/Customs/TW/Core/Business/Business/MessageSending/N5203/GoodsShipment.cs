using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business.N5203
{
	public class GoodsShipment : IGoodsShipment
	{
		public GoodsShipment(CusEntryHeader entryHeader, SupportingDocumentCollection supportingDocuments, IStorageDocsBaseCollection[] allEDocs)
		{
			EntryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			this.supportingDocuments = supportingDocuments;
			this.allEDocs = allEDocs;
			var declaration = entryHeader.Declaration;
			Declaration = Argument.NotNull(declaration, nameof(declaration));
		}

		protected readonly CusEntryHeader EntryHeader;
		protected readonly JobDeclaration Declaration;
		readonly SupportingDocumentCollection supportingDocuments;
		readonly IStorageDocsBaseCollection[] allEDocs;

		public IEnumerable<IAdditionalDocument> AdditionalDocuments
		{
			get
			{
				foreach (var supportingDocument in supportingDocuments.Cast<SupportingDocument>())
				{
					var edoc = supportingDocument.EDoc.IsValid ? allEDocs.GetFromUniqueKey(supportingDocument.EDoc.ToGuid()) : null;
					yield return new GoodsShipmentAdditionalDocumentWrapper(supportingDocument, edoc);
				}
			}
		}

		public IEnumerable<IGovernmentAgencyGoodsItem> GovernmentAgencyGoodsItems => governmentAgencyGoodsItems ?? (governmentAgencyGoodsItems = GovernmentAgencyGoodsItemsCore);
		IEnumerable<IGovernmentAgencyGoodsItem> governmentAgencyGoodsItems;

		protected virtual IEnumerable<IGovernmentAgencyGoodsItem> GovernmentAgencyGoodsItemsCore
		{
			get
			{
				var sortedLines = EntryHeader?.AllEntryLines.OfType<CusEntryLine>().OrderBy(line => line.CL_LineNumber);
				foreach (var entryLine in sortedLines)
				{
					yield return new GovernmentAgencyGoodsItem(entryLine, entryLine.FirstInvoiceLine);
				}
			}
		}

		public ZDateTime ExitDateTime => ZDateTime.Empty;

		[DecimalPlaces(0)]
		public ZDecimal ItemChargeAmount => ItemChargeAmountCore;
		protected virtual ZDecimal ItemChargeAmountCore => EntryHeader.CH_TotalCustomsValueInLocalCurrency;

		public ZDecimal TotalCIFAmount => ZDecimal.Zero;

		public IPartyDetails Consignee
		{
			get
			{
				PartyDetails result = null;
				if (Declaration.ConsigneeAddresses.FindByDocAddressType(DocAddressType.ConsigneeAddress) is TWConsignorOrConsigneeAddress consignee)
				{
					result = new Consignee(Declaration, consignee);
				}
				return result;
			}
		}

		public IConsignment Consignment => new Consignment(EntryHeader);

		public IPartyDetails Consignor
		{
			get
			{
				PartyDetails result = null;
				if (Declaration.ConsignorAddresses.FindByDocAddressType(DocAddressType.ConsignorAddress) is TWConsignorOrConsigneeAddress consignor)
				{
					result = new Consignor(Declaration, consignor);
				}
				return result;
			}
		}

		public ICustomsValuation CustomsValuation => new CustomsValuation(EntryHeader);

		public ZString DeliveryDestinationName => ZString.Empty;

		public IEnumerable<IGoodsShipmentDutyTaxFee> DutyTaxFees
		{
			get
			{
				foreach (DutyTaxFeeCharge charge in EntryHeader.DutyTaxFeeCharges)
				{
					yield return new GoodsShipmentDutyTaxFeeWrapper(charge.ChargeType, charge.ChargeAmount);
				}
			}
		}

		public IPartyDetails NotifyParty
		{
			get
			{
				PartyDetails result = null;
				var notifyPartyOrg = Declaration.NotifyParty;
				if (notifyPartyOrg != null)
				{
					result = new NotifyParty(Declaration, notifyPartyOrg);
				}
				return result;
			}
		}

		public IPartyDetails Seller => null;

		public ZString TradeTermsConditionCode => EntryHeader.CH_DeclarationIncoterm;

		public ZString UCR => EntryHeader.EntryInstruction?.UCRNumber ?? ZString.Empty;

		public IPartyDetails Buyer
		{
			get
			{
				var importererDocumentaryAddress = Declaration.ImporterDocumentaryAddress;
				PartyDetails result = null;
				if (importererDocumentaryAddress != null && (importererDocumentaryAddress.E2_AddressOverride || importererDocumentaryAddress.HasRealOrganisation))
				{
					result = new Buyer(Declaration, importererDocumentaryAddress.Organisation, importererDocumentaryAddress);
				}
				return result;
			}
		}

		public IPartyDetails Exporter => PartyHelper.GetExporter(Declaration);

		public IEnumerable<IGoodsMeasure> GoodsMeasures => null;

		public IEnumerable<IAdditionalInformation> AdditionalInformations => null;

		public IEnumerable<IAdditionalDeclaration> AdditionalDeclarations => null;
	}
}
