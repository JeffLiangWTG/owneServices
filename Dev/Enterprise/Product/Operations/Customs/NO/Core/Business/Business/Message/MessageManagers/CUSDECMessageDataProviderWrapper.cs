using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Edifact.V902.Elements;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NO.Business;

sealed class CUSDECMessageDataProviderWrapper : ICUSDECMessageDataProvider
{
	public CUSDECMessageDataProviderWrapper(MessageSendingObject messageSendingObject)
	{
		Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
		entryHeader = Argument.NotNull(messageSendingObject.Header, nameof(messageSendingObject.Header));
		declaration = entryHeader.Declaration;
		entryInstruction = entryHeader.EntryInstruction;
		invoiceHeader = Argument.NotNull(entryHeader.RandomHeader, nameof(entryHeader.RandomHeader));
		var entryLine = Argument.NotNull(entryHeader.RandomEntryLine, nameof(entryHeader.RandomEntryLine));
		randomInvoiceLine = Argument.NotNull(entryLine.RandomLine, nameof(entryLine.RandomLine));
		messagesType = messageSendingObject.MessageType;
		customsOffice = messageSendingObject.CustomsOffice;
	}

	readonly CusEntryHeader entryHeader;
	readonly JobDeclaration declaration;
	readonly CusEntryInstruction entryInstruction;
	readonly JobComInvoiceHeader invoiceHeader;
	readonly JobComInvoiceLine randomInvoiceLine;
	readonly ZString messagesType;
	readonly ZString customsOffice;

	JobComInvoiceLine[] MergedInvoiceLines => entryHeader.MergedLines.SelectMany(x => x.InvoiceLines).Cast<JobComInvoiceLine>().ToArray();

	public ZDateTime InterchangeTime => interchangeTime;
	readonly ZDateTime interchangeTime;

	public ZString DeclarationReferenceNumber => declaration.JE_DeclarationReference;

	public ZString AssociationAssignedCode => declaration.IsImport ? "NEP-I" : "NEP";

	public ZString LocalReferenceNumber => entryHeader.CH_BGMReference.IsEmpty
		? NOEDIMessage.JobReferenceNumberPlaceHolder
		: entryHeader.CH_BGMReference;

	public DocumentMessageNameCodedList DocumentMessageName => declaration.IsImport
		? DocumentMessageNameCodedList.GoodsDeclarationForImportation
		: DocumentMessageNameCodedList.GoodsDeclarationForExportation;

	public ZString MessageType => messagesType;

	public ZString DeclarationType => string.Concat(declaration.JE_MessageSubType, entryInstruction?.CEI_Style);

	public ZString TransactionNature => invoiceHeader.JZ_ValuationCode;

	public ZString ControlNumber => declaration.IsImport
		? declaration.Importer.GetDefermentApprovalNumberCodeOrEmpty()
		: declaration.Supplier.GetDefermentApprovalNumberCodeOrEmpty();

	public ZString GoodsNumber => declaration.JE_GoodsNumber;

	public ZString GoodsNumberPosition => entryInstruction is not { CEI_SubPosition.IsEmpty: false }
		? declaration.JE_Position
		: $"{declaration.JE_Position}/{entryInstruction.CEI_SubPosition}";

	public ZString GoodsDestination => declaration.IsExport ? declaration.JE_GoodsDestination : ZString.Empty;

	public ZString GoodsOrigin => GetGoodsOriginCore();

	ZString GetGoodsOriginCore()
	{
		var goodsOrigin = declaration.JE_GoodsOrigin;
		return declaration.IsImport || goodsOrigin != Constants.CountryCodes.Norway
			? goodsOrigin
			: ZString.Empty;
	}

	public ZString CustomsOfficeOfExit => declaration.IsExport ? declaration.JE_CustomsOffice : ZString.Empty;

	public ZString LocationOfGoods => declaration.JE_LocationOfGoods;

	public ZString TransportMode => declaration.JE_CustomsTransportMode;

	public ZString TransportNationality => declaration.JE_RN_NKTransportNationality;

	public ZString ContainerMode => declaration.JE_ContainerMode;

	public ZString RelatedDeclaration => IsReExportOrFinalImport ? entryHeader.CH_ReCalcOrigDecl : ZString.Empty;

	public ZString ReExportReason => IsReExportOrFinalImport ? entryHeader.CH_ReCalcReason : ZString.Empty;

	ZBool IsReExportOrFinalImport => declaration.JE_CopyStatus == NODeclarationCopyStatus.Codes.ReExport || declaration.JE_CopyStatus == NODeclarationCopyStatus.Codes.FinalImport;

	public ZString ExporterCustomsRegNo => declaration.IsExport ? GetCustomsRegNo(declaration.Supplier) : ZString.Empty;

	public ZString ImporterCustomsRegNo => declaration.IsImport ? GetCustomsRegNo(declaration.Importer) : ZString.Empty;

	static ZString GetCustomsRegNo(OrgHeader orgHeader)
	{
		if (orgHeader is null)
		{
			return ZString.Empty;
		}

		return orgHeader.OH_Category.ToString() switch
		{
			UniversalReferenceConstants.OrgHeaderType.NaturalPerson => orgHeader.GetSocialSecurityNumberOrEmpty(),
			UniversalReferenceConstants.OrgCodeType.OrganizationNumber => orgHeader.GetOrganizationNumberOrEmpty(),
			_ => GetDefaultValueForRegNo()
		};

		ZString GetDefaultValueForRegNo()
		{
			var mvaCode = orgHeader.GetMVACodeOrEmpty();
			return mvaCode.IsEmpty ? orgHeader.GetOrganizationNumberOrEmpty() : mvaCode;
		}
	}

	public PartyQualifierList SenderAddressType => declaration.IsImport
		? PartyQualifierList.Seller
		: PartyQualifierList.Consignee;

	public ZString SenderFullName => GetSenderParty()?.OH_FullName ?? ZString.Empty;

	public ZString SenderAddress1 => GetSenderParty()?.MainAddress.OA_Address1 ?? ZString.Empty;

	public ZString SenderAddress2 => GetSenderParty()?.MainAddress.OA_Address2 ?? ZString.Empty;

	public ZString SenderAddress3 => GetSenderParty() switch
	{
		{ MainAddress: { } address } => FormattableString.Invariant($"{address.OA_PostCode} {address.OA_City} {address.OA_RN_NKCountryCode}"),
		_ => ZString.Empty
	};

	OrgHeader GetSenderParty() => declaration.IsImport ? declaration.Consignor : declaration.Importer;

	public ZString DeclarantCustomsRegNo
	{
		get
		{
			if (declaration is not { DeclarantAddress.Header: { } declarant })
			{
				return ZString.Empty;
			}
			return declarant.GetMVACodeOrEmpty() is { IsEmpty: false } mvaCode
				? mvaCode
				: declarant.GetOrganizationNumberOrEmpty();
		}
	}

	public ZString PaymentMethod => entryHeader.CH_PaymentMethod;

	public ZString CustomsControllingUnit => customsOffice.IsEmpty
		? entryHeader.CH_ToCustomsControllingUnit
		: customsOffice;

	public ZString InitialsOfDeclarant => declaration.JE_GS_NKCusAgent;

	public ZString IncoTerm => invoiceHeader.JZ_IncoTerm;

	public ZString IncoTermPlace => invoiceHeader.JZ_IncoTermPlace;

	public ZString CommercialInvoiceAmount
		=> CusEntryHeaderMessageDataProvider.GetTotalInvoiceAmount().ToNorwegianAmountString();

	ImmutableArray<JobComInvoiceLine> InvoiceLines => invoiceLines ??= entryHeader
		.MergedLines
		.SelectMany(x => x.InvoiceLines.Cast<JobComInvoiceLine>())
		.ToImmutableArray();
	ImmutableArray<JobComInvoiceLine>? invoiceLines;

	public ZString CommercialInvoiceCurrency => CusEntryHeaderMessageDataProvider.HasMultipleCurrencies
		? declaration.LocalCurrencyCode
		: invoiceHeader.JZ_RX_NKInvoice_Currency;

	public ZString FreightAmountNOK => FreightAmountNOKCore.ToNorwegianAmountString();

	ZDecimal FreightAmountNOKCore => InvoiceLines.Sum(x => x.JI_Calc_FreightInLocalCurrency);

	public ZString CurrencyExchangeRate => CurrencyExchangeRateCore.ToNorwegianAmountString();

	ZDecimal CurrencyExchangeRateCore => invoiceHeader.JZ_RX_NKInvoice_Currency == declaration.LocalCurrencyCode || CusEntryHeaderMessageDataProvider.HasMultipleCurrencies
		? 100m
		: randomInvoiceLine.InvoiceLineCurrencyExchangeRateForCustoms;

	public IReadOnlyCollection<ICUSDECMessageDataProvider.IDocumentMessageSummary> DocumentMessageSummaryCollection => documentMessageSummaryCollection ??= GetDocumentMessageSummaryCollection();
	IReadOnlyCollection<ICUSDECMessageDataProvider.IDocumentMessageSummary> documentMessageSummaryCollection;

	IReadOnlyCollection<ICUSDECMessageDataProvider.IDocumentMessageSummary> GetDocumentMessageSummaryCollection()
	{
		return MergedInvoiceLines
			.GroupBy(x => x.InvoiceHeader)
			.Select(x => new CUSDECDocumentMessageSummaryWrapper(entryHeader, x.Key))
			.ToImmutableArray();
	}

	public IReadOnlyCollection<ICUSDECMessageDataProvider.ITotalFeeLines> TotalFeeCollection => totalFeeCollection ??= GetTotalFeeCollection();
	IReadOnlyCollection<ICUSDECMessageDataProvider.ITotalFeeLines> totalFeeCollection;

	IReadOnlyCollection<ICUSDECMessageDataProvider.ITotalFeeLines> GetTotalFeeCollection()
	{
		return entryHeader.MergedLines
			.SelectMany(entry => entry.Fees.Cast<CusEntryLineFee>())
			.Where(fee => !fee.CF_IsLandedCostOnly)
			.GroupBy(fee => fee.CF_DutyCode)
			.Select(grouping => new CUSDECTotalFeeLinesWrapper(new CusEntryHeaderFee() { Duty = grouping.Key, Amount = grouping.Sum(fee => fee.CF_ChargeAmount) } ))
			.ToImmutableArray();
	}

	public ZString TotalFeeAmount => GetTotalFeeAmount().ToNorwegianAmountString();

	ZDecimal GetTotalFeeAmount()
	{
		return TotalFeeCollection.Sum(x => x.Amount);
	}

	public ZString TotalNoOfItemLines => entryHeader.MergedLines.Count.ToString();

	public ZString TotalNoOfPackages => entryInstruction.CEI_PackageCount.ToString();

	CusEntryHeaderMessageDataProvider CusEntryHeaderMessageDataProvider => cusEntryHeaderMessageDataProvider ??= new(entryHeader);
	CusEntryHeaderMessageDataProvider cusEntryHeaderMessageDataProvider;

	#region Implementation

	public IEDIMessageCollectionProvider AsMessageCollectionProvider() => entryHeader;

	#endregion
}
