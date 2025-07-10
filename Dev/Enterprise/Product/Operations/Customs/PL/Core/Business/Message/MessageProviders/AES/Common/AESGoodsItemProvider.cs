using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using static Enterprise.Customs.PL.Business.AesRuleHelper;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business;

public class AESGoodsItemProvider : IGoodsItem
{
	public AESGoodsItemProvider(CusEntryLine entryLine, int index, AESGoodsShipmentProvider parentProvider)
	{
		EntryLine = Argument.NotNull(entryLine, nameof(entryLine));
		entryHeader = entryLine.Header;
		declaration = Argument.NotNull(entryLine.Declaration, $"{nameof(entryLine)}.{nameof(CusEntryLine.Declaration)}");
		EntryInstruction = Argument.NotNull(entryHeader.EntryInstruction, $"{nameof(entryHeader)}.{nameof(CusEntryHeader.EntryInstruction)}");
		invoiceLine = entryLine.RandomLine;
		DeclarationGoodsItemNumber = index;
		this.parentProvider = Argument.NotNull(parentProvider, nameof(parentProvider));
	}

	readonly AESGoodsShipmentProvider parentProvider;
	protected CusEntryLine EntryLine { get; }
	readonly CusEntryHeader entryHeader;
	readonly JobDeclaration declaration;
	protected CusEntryInstruction EntryInstruction { get; }
	protected readonly JobComInvoiceLine invoiceLine;

	public int DeclarationGoodsItemNumber { get; }

	public decimal? StatisticalValueValue => CachedValueHelper.GetValue(ref statisticalValueValue, GetStatisticalValue);
	CachedValue<decimal?> statisticalValueValue;

	public string NatureOfTransaction => CachedValueHelper.GetValue(ref natureOfTransaction, GetNatureOfTransaction);
	protected virtual string GetNatureOfTransaction() => !IsAESTransitionPeriod
														&& parentProvider.NatureOfTransaction.IsNullOrEmpty()
														&& !CheckRuleR0375()
		? MessageProviderHelper.ReturnNullIfEmpty(invoiceLine.InvoiceHeader.JZ_ValuationCode)
		: null;
	CachedValue<string> natureOfTransaction;

	public string CountryOfExport => null; // future use

	public string CountryOfDestination => CachedValueHelper.GetValue(ref countryOfDestination, GetCountryOfDestination);
	CachedValue<string> countryOfDestination;

	public string ReferenceNumberUCR => null; // future use

	public IReadOnlyCollection<IAuthorisationNumber> Authorisations => authorisations ??= invoiceLine.CusAuthorizationUsages
		.GroupBy(x => new { x.AGC_Code, x.AGC_Number })
		.Select((x, i) => new AESAuthorisationNumberProvider(x.First(), i + 1))
		.ToArray<IAuthorisationNumber>();
	IReadOnlyCollection<IAuthorisationNumber> authorisations;

	public IProcedure Procedure => CachedValueHelper.GetValue(ref procedure, () => new AESProcedureProvider(invoiceLine));
	CachedValue<IProcedure> procedure;

	public IConsigneeConsignor Consignor => CachedValueHelper.GetValue(ref consignor, GetConsignor);
	protected virtual IConsigneeConsignor GetConsignor() => AESConsigneeConsignorProvider.NewOrNull(invoiceLine.ExporterAddress);
	CachedValue<IConsigneeConsignor> consignor;

	public IConsigneeConsignor Consignee => CachedValueHelper.GetValue(ref consignee, () => CheckRuleC0351() ? null : GetConsigneeCore());
	protected virtual IConsigneeConsignor GetConsigneeCore() => AESConsigneeConsignorProvider.NewOrNull(invoiceLine.ConsigneeAddress);
	CachedValue<IConsigneeConsignor> consignee;

	public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors =>
		additionalSupplyChainActors ??= invoiceLine.CusSupplyChainActorReferences
			.Select((x, i) => new AESAdditionalSupplyChainActorProvider(x, i + 1))
			.ToArray();
	IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActors;

	public IOrigin Origin => CachedValueHelper.GetValue(ref origin, () => new AESOriginProvider(invoiceLine));
	CachedValue<IOrigin> origin;

	public ICommodity Commodity => CachedValueHelper.GetValue(ref commodity, GetCommodity);
	protected virtual ICommodity GetCommodity() => AESCommodityProvider.NewOrNull(EntryLine);
	CachedValue<ICommodity> commodity;

	public IReadOnlyCollection<IPackaging> Packaging => packaging ??= GetPackaging();
	IReadOnlyCollection<IPackaging> packaging;

	public IReadOnlyCollection<IPreviousDocument> PreviousDocuments => previousDocuments ??= GetPreviousDocuments();
	IReadOnlyCollection<IPreviousDocument> previousDocuments;

	public IReadOnlyCollection<IPreviousDocumentSpecialProcedures> PreviousDocumentSpecialProcedures =>
		previousDocumentSpecialProcedures ??= GetPreviousDocumentSpecialProcedures();
	protected virtual IReadOnlyCollection<IPreviousDocumentSpecialProcedures> GetPreviousDocumentSpecialProcedures() =>
		invoiceLine.InvoiceHeader.PreviousDocuments.Cast<PreviousDocument>()
			.Concat(invoiceLine.PreviousDocuments.Cast<PreviousDocument>())
			.Concat(EntryInstruction.PreviousDocuments.Cast<PreviousDocument>())
			.Where(x => !IsPreviousDocumentCode(x))
			.Select((x, i) => new AESGoodsItemPreviousDocumentSpecialProceduresProvider(x))
			.ToArray<IPreviousDocumentSpecialProcedures>();
	IReadOnlyCollection<IPreviousDocumentSpecialProcedures> previousDocumentSpecialProcedures;

	public IReadOnlyCollection<ISupportingDocument> SupportingDocuments => supportingDocuments ??= GetSupportingDocuments();
	IReadOnlyCollection<ISupportingDocument> supportingDocuments;

	public IReadOnlyCollection<IDocument> TransportDocuments => transportDocuments ??= GetTransportDocuments();
	protected virtual IReadOnlyCollection<IDocument> GetTransportDocuments() => !IsAESTransitionPeriod
		? Array.Empty<IDocument>()
		: invoiceLine.InvoiceHeader.AdditionalInfos.Cast<AdditionalInfo>()
			.Concat(invoiceLine.AdditionalInfos.Cast<AdditionalInfo>())
			.Concat(EntryInstruction.AdditionalInfos.Cast<AdditionalInfo>())
			.Where(x => x.CSI_SubType == AdditionalInfoKindList.Codes.TRA)
			.GroupBy(x => new { x.CSI_Code, x.CSI_ReferenceNumber })
			.Select(x => new AESDocumentProvider(x.FirstOrDefault(), true))
			.ToArray<IDocument>();
	IReadOnlyCollection<IDocument> transportDocuments;

	public IReadOnlyCollection<IDocument> AdditionalReferences => additionalReferences ??= GetAdditionalReferences();
	protected virtual IReadOnlyCollection<IDocument> GetAdditionalReferences() => GetAdditionalInfosForSubType(GetAdditionalReferenceCore, AdditionalInfoKindList.Codes.REF);
	protected virtual IDocument GetAdditionalReferenceCore(Enterprise.Customs.Business.CusSupportingInfo cusSupportingInfo) =>
		new AESDocumentProvider(cusSupportingInfo, referenceNumberAsDescription: true);
	IReadOnlyCollection<IDocument> additionalReferences;

	public IReadOnlyCollection<IDocument> AdditionalInformations => additionalInformations ??= GetAdditionalInformations();
	protected IReadOnlyCollection<IDocument> GetAdditionalInformations() => GetAdditionalInfosForSubType(GetAdditionalInformationCore, AdditionalInfoKindList.Codes.INF, new ZString[] { AdditionalInfoCodes._4PL03 });
	protected virtual IDocument GetAdditionalInformationCore(Enterprise.Customs.Business.CusSupportingInfo cusSupportingInfo) =>
		new AESDocumentProvider(cusSupportingInfo, referenceNumberAsDescription: false);
	IReadOnlyCollection<IDocument> additionalInformations;

	public string TransportChargesMethodOfPayment => CachedValueHelper.GetValue(ref transportChargesMethodOfPayment, () => CheckRuleR0013G()
		? EntryLine.TransportChargesMethodOfPayment.ToString()
		: null);
	CachedValue<string> transportChargesMethodOfPayment;

	IReadOnlyCollection<IDocument> GetAdditionalInfosForSubType(Func<Enterprise.Customs.Business.CusSupportingInfo, IDocument> documentProvider, string subType, ZString[] excludeCodes = null)
	{
		var subTypeIsNotInf = subType != AdditionalInfoKindList.Codes.INF;
		excludeCodes ??= Array.Empty<ZString>();
		var additionalInfos = invoiceLine.InvoiceHeader.AdditionalInfos.Cast<AdditionalInfo>()
			.Concat(invoiceLine.AdditionalInfos.Cast<AdditionalInfo>())
			.Where(x => x.CSI_SubType == subType && !excludeCodes.Contains(x.CSI_Code));

		if (IsAESTransitionPeriod && (subTypeIsNotInf || EntryInstruction.CEI_SubStyle != SubStyleCodes.Z))
		{
			additionalInfos = additionalInfos.Concat(EntryInstruction.GetAdditionalInfosBySubtypeAndDistinctByCode(subType, excludeCodes));
		}

		return additionalInfos
			.OrderBy(x => subTypeIsNotInf ? 0 : RuleR0093E.GetKeyEuLessThanPl(x.CSI_Code, RuleR0093E.Patterns.a1an4))
			.GroupBy(x => new { code = x.CSI_Code, groupNumber = subTypeIsNotInf ? x.CSI_ReferenceNumber : x.CSI_Description })
			.Select(x => documentProvider(x.First()))
			.ToArray();
	}

	protected bool IsAESTransitionPeriod => EntryInstruction.IsAESTransitionPeriod();

	bool CheckRuleC0028() => EntryInstruction.HasSubStyleEqualsBorCorEorF();

	bool CheckRuleR0375() => EntryInstruction.HasSubStyleEqualsBorCorEorF();

	bool CheckRuleC0351() => !IsAESTransitionPeriod
		&& EntryLine.AdditionalInfos
			.Any(x => x.CSI_Code == AdditionalInfoCodes._30600);

	bool CheckRuleR0013G()
	{
		var methodOfPayment = EntryLine.TransportChargesMethodOfPayment;
		return methodOfPayment.IsEmpty ||
			entryHeader.MergedLines.Any(entryLine => entryLine.TransportChargesMethodOfPayment != methodOfPayment);
	}

	static bool IsPreviousDocumentCode(PreviousDocument document)
	{
		var code = document?.CSI_Code ?? ZString.Empty;
		return !(code == PreviousDocumentCodes.MRN || code == PreviousDocumentCodes.CLE || code == PreviousDocumentCodes.SDE ||
				 code == PreviousDocumentCodes.OGL || code == PreviousDocumentCodes.ZZZ);
	}

	static decimal GetAmountSum(SupportingDocument document) =>
		GetEqualSiblingsOnInvoiceHeaderLevel(document)?.Sum(x => x.CSI_Value) ?? 0m;

	static decimal GetQuantitySum(SupportingDocument document) =>
		GetEqualSiblingsOnInvoiceHeaderLevel(document)?.Sum(x => x.CSI_Quantity) ?? 0m;

	static IEnumerable<SupportingDocument> GetEqualSiblingsOnInvoiceHeaderLevel(SupportingDocument document) =>
		(document?.Parent as JobComInvoiceLine)?.InvoiceHeader.InvoiceLines.Cast<JobComInvoiceLine>()
			.SelectMany(x => x.SupportingDocuments.Cast<SupportingDocument>())
			.Where(x => x.KeyToDeterimeUniqueness == document.KeyToDeterimeUniqueness);

	string GetCountryOfDestination()
	{
		var invoiceLines = declaration.InvoiceLines.Cast<JobComInvoiceLine>().ToList();
		var uniqueCountriesOfDestination = invoiceLines.Select(x => x.ZG_CountryOfDestination).Distinct().ToList();
		if (uniqueCountriesOfDestination.Count == 1)
		{
			return null;
		}

		var countriesOfDestination = uniqueCountriesOfDestination
			.Union(new[] { declaration.JE_GoodsDestination })
			.Where(x => x != ZString.Empty)
			.Distinct();

		return countriesOfDestination.Count() > 1 && !invoiceLine.ZG_CountryOfDestination.IsEmpty
			? invoiceLine.ZG_CountryOfDestination.ToString()
			: null;
	}

	IReadOnlyCollection<IPackaging> GetPackaging()
	{
		return EntryLine.PackagingDetails
			.GroupBy(x => new { x.Package.CW_MarksAndNos, x.Package.CW_PackType })
			.Select(group => group.ToList())
			.Select((packagePivots, i) => new AESPackagingProvider(packagePivots.ToArray(), declaration, i + 1))
			.ToArray<IPackaging>();
	}

	IReadOnlyCollection<IPreviousDocument> GetPreviousDocuments()
	{
		return invoiceLine.InvoiceHeader.PreviousDocuments.Cast<PreviousDocument>()
			.Concat(invoiceLine.PreviousDocuments.Cast<PreviousDocument>())
			.Concat(GetPreviousDocumentsFromEntryInstruction(invoiceLine))
			.Where(IsPreviousDocumentCode)
			.OrderBy(x => RuleR0093E.GetKeyEuLessThanPl(x.CSI_Code, RuleR0093E.Patterns.n1an3))
			.Select(GetPreviousDocumentCore)
			.Distinct(PreviousDocumentEqualityComparer.Instance)
			.ToArray();
	}
	protected virtual IPreviousDocument GetPreviousDocumentCore(Enterprise.Customs.Business.CusSupportingInfo cusSupportingInfo) =>
		new AESGoodsItemPreviousDocumentProvider(cusSupportingInfo, EntryInstruction.CEI_Procedure);

	IEnumerable<PreviousDocument> GetPreviousDocumentsFromEntryInstruction(JobComInvoiceLine invoiceLine) =>
		IsAESTransitionPeriod && !EntryInstruction.HasSubStyleEqualsXorYorZ()
			? EntryInstruction.PreviousDocuments.Cast<PreviousDocument>()
			: Array.Empty<PreviousDocument>();

	IReadOnlyCollection<ISupportingDocument> GetSupportingDocuments()
	{
		var supportingDocs = new List<SupportingDocument>();
		supportingDocs.AddRange(invoiceLine.InvoiceHeader.SupportingDocuments.Cast<SupportingDocument>());
		supportingDocs.AddRange(invoiceLine.SupportingDocuments.Cast<SupportingDocument>());
		if (IsAESTransitionPeriod)
		{
			supportingDocs.AddRange(EntryInstruction.SupportingDocuments.Cast<SupportingDocument>());
		}

		return supportingDocs
			.OrderBy(x => RuleR0093E.GetKeyEuLessThanPl(x.CSI_Code, RuleR0093E.Patterns.n1an3))
			.Select(x => invoiceLine.Declaration.IsExport && x.IsParentInvoiceLine
				? GetSupportingDocumentCore(x, () => GetAmountSum(x), () => GetQuantitySum(x))
				: GetSupportingDocumentCore(x, () => x.CSI_Value, () => x.CSI_Quantity))
			.Distinct(SupportingDocumentEqualityProvider.Instance)
			.ToArray();
	}

	protected virtual ISupportingDocument GetSupportingDocumentCore(Enterprise.Customs.Business.CusSupportingInfo cusSupportingInfo, Func<ZDecimal> getAmount, Func<ZDecimal> getQuantity) =>
		new AESGoodsItemSupportingDocumentProvider(cusSupportingInfo, getAmount, getQuantity);

	decimal? GetStatisticalValue() => IsAESTransitionPeriod || EntryLine.Declaration.JE_EntryStyle == EntryStyleListExport.Codes.ExportToSpecialTerritory || !CheckRuleC0028()
		? EntryLine.CL_StatisticalValue.Round(2)
		: null;
}
