using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TW.Business
{
	class NX5105Declaration_GoodsShipment : IGoodsShipment
	{
		public NX5105Declaration_GoodsShipment(CusEntryHeader entryHeader, SupportingDocumentCollection supportingDocuments, IStorageDocsBaseCollection[] allEDocs, bool includeControllingMessageInformation)
		{
			EntryHeader = Argument.NotNull(entryHeader, "entryHeader");
			declaration = Argument.NotNull(entryHeader.Declaration, "declaration");
			entryInstruction = Argument.NotNull(entryHeader.EntryInstruction, "entryInstruction");
			this.supportingDocuments = supportingDocuments;
			this.allEDocs = allEDocs;
			IncludeControllingMessageInformation = includeControllingMessageInformation;
		}

		protected readonly ZBool IncludeControllingMessageInformation;

		protected CusEntryHeader EntryHeader { get; }

		readonly JobDeclaration declaration;
		readonly CusEntryInstruction entryInstruction;
		readonly SupportingDocumentCollection supportingDocuments;
		readonly IStorageDocsBaseCollection[] allEDocs;

		IEnumerable<IGovernmentAgencyGoodsItem> IGoodsShipment.GovernmentAgencyGoodsItems => governmentAgencyGoodsItems ?? (governmentAgencyGoodsItems = GetGovernmentAgencyGoodsItemsCore());
		IEnumerable<IGovernmentAgencyGoodsItem> governmentAgencyGoodsItems;

		protected virtual IEnumerable<IGovernmentAgencyGoodsItem> GetGovernmentAgencyGoodsItemsCore()
		{
			var sortedLines = EntryHeader?.MergedLines.OfType<CusEntryLine>().OrderBy(line => line.CL_LineNumber);

			foreach (var entryLine in sortedLines)
			{
				if (IncludeControllingMessageInformation)
				{
					yield return new NX5105CMGovernmentAgencyGoodsItem(entryLine, entryLine.FirstInvoiceLine);
				}
				else
				{
					yield return new NX5105GoodsShipment_GovernmentAgencyGoodsItem(entryLine, entryLine.FirstInvoiceLine);
				}
			}
		}

		ZDateTime IGoodsShipment.ExitDateTime => declaration.JE_ExportDate;

		[DecimalPlaces(2)]
		ZDecimal IGoodsShipment.ItemChargeAmount => EntryHeader.CH_TotalIMPFOBAmountInInvoiceCurrency;

		ZDecimal IGoodsShipment.TotalCIFAmount => EntryHeader.CustomsValue;

		IPartyDetails IGoodsShipment.Consignee
		{
			get
			{
				IPartyDetails partyDetails = null;
				if (declaration.ConsigneeAddresses.FindByDocAddressType(DocAddressType.ConsigneeAddress) is TWConsignorOrConsigneeAddress consignee)
				{
					partyDetails = new NX5105ConsigneeWrapper(declaration, consignee);
				}
				return partyDetails;
			}
		}

		IConsignment IGoodsShipment.Consignment => new NX5105GoodsShipment_Consignment(EntryHeader);

		IPartyDetails IGoodsShipment.Consignor
		{
			get
			{
				IPartyDetails partyDetails = null;
				if (declaration.ConsignorAddresses.FindByDocAddressType(DocAddressType.ConsignorAddress) is TWConsignorOrConsigneeAddress consignor)
				{
					partyDetails = new NX5105ConsignorWrapper(declaration, consignor);
				}
				return partyDetails;
			}
		}

		ICustomsValuation IGoodsShipment.CustomsValuation => new NX5105GoodsShipment_CustomsValuation(EntryHeader);

		IEnumerable<IGoodsShipmentDutyTaxFee> IGoodsShipment.DutyTaxFees
		{
			get
			{
				var dutyTaxFeeCharges = EntryHeader.DutyTaxFeeCharges;
				return dutyTaxFeeCharges.Any() ? dutyTaxFeeCharges.Cast<DutyTaxFeeCharge>().Select(x => new GoodsShipmentDutyTaxFeeWrapper(x.ChargeType, x.ChargeAmount))
					: new[] { new GoodsShipmentDutyTaxFeeWrapper(DutyTaxFeeCodeList.Codes.A10, ZDecimal.Zero) };
			}
		}

		IPartyDetails IGoodsShipment.NotifyParty
		{
			get
			{
				IPartyDetails partyDetails = null;
				var notifyParty = declaration.NotifyParty;
				if (notifyParty != null && notifyParty.CountryCode == CountryCodes.Taiwan)
				{
					partyDetails = new NX5105NotifyPartyWrapper(notifyParty.MainAddress, OrgCusCode.CodeTypes.VATCode, OrgCusCode.CodeTypes.PassportID, OrgCusCode.TaiwanCodeTypes.PID);
				}
				return partyDetails;
			}
		}

		IPartyDetails IGoodsShipment.Seller
		{
			get
			{
				IPartyDetails partyDetails = null;
				var supplierDocumentaryAddress = declaration?.SupplierDocumentaryAddress;
				if (supplierDocumentaryAddress != null && (supplierDocumentaryAddress.E2_AddressOverride || supplierDocumentaryAddress.HasRealOrganisation))
				{
					var mainAddress = supplierDocumentaryAddress.Address;
					partyDetails = new NX5105SellerWrapper(mainAddress, supplierDocumentaryAddress.CBPCode, supplierDocumentaryAddress, OrgCusCode.CodeTypes.VATCode, OrgCusCode.CodeTypes.PassportID, OrgCusCode.TaiwanCodeTypes.PID);
				}
				return partyDetails;
			}
		}

		ZString IGoodsShipment.TradeTermsConditionCode => EntryHeader.CH_DeclarationIncoterm;

		ZString IGoodsShipment.UCR => entryInstruction.UCRNumber;

		#region Not Applicable

		IEnumerable<IAdditionalDocument> IGoodsShipment.AdditionalDocuments
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

		ZString IGoodsShipment.DeliveryDestinationName => ZString.Empty;

		IPartyDetails IGoodsShipment.Buyer => null;

		IPartyDetails IGoodsShipment.Exporter => null;

		IEnumerable<IGoodsMeasure> IGoodsShipment.GoodsMeasures => null;

		IEnumerable<IAdditionalInformation> IGoodsShipment.AdditionalInformations => null;

		public IEnumerable<IAdditionalDeclaration> AdditionalDeclarations => null;

		#endregion
	}
}
