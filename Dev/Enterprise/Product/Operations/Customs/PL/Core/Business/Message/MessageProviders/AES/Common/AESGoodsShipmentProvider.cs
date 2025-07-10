using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.PL.Business.AesRuleHelper;
using CusEntryHeader = Enterprise.Customs.PL.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction;
using CusEntryLine = Enterprise.Customs.PL.Business.Declaration.CusEntryLine;
using JobComInvoiceLine = Enterprise.Customs.PL.Business.Declaration.JobComInvoiceLine;
using JobDeclaration = Enterprise.Customs.PL.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.PL.Business;

public class AESGoodsShipmentProvider : IGoodsShipment
{
	public AESGoodsShipmentProvider(CusEntryHeader entryHeader)
	{
		EntryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		declaration = Argument.NotNull(entryHeader.Declaration, $"{nameof(entryHeader)}.{nameof(entryHeader.Declaration)}");
		EntryInstruction = Argument.NotNull(entryHeader.EntryInstruction, $"{nameof(entryHeader)}.{nameof(entryHeader.EntryInstruction)}");
	}

	protected CusEntryHeader EntryHeader { get; }

	protected CusEntryInstruction EntryInstruction { get; }

	readonly JobDeclaration declaration;

	public string NatureOfTransaction => CachedValueHelper.GetValue(ref natureOfTransaction, GetNatureOfTransaction);
	CachedValue<string> natureOfTransaction;

	public string CountryOfExport => declaration.JE_GoodsOrigin;

	public string CountryOfDestination => CachedValueHelper.GetValue(ref countryOfDestination, GetCountryOfDestination);
	CachedValue<string> countryOfDestination;

	public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActors ??=
		EntryInstruction.CusSupplyChainActorReferences
			.Select((x, i) => new AESAdditionalSupplyChainActorProvider(x, i + 1)).ToArray<IAdditionalSupplyChainActor>();

	IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActors;

	public IDeliveryTerms DeliveryTerms => CachedValueHelper.GetValue(ref deliveryTerms, GetDeliveryTerms);
	CachedValue<IDeliveryTerms> deliveryTerms;

	public IWarehouse Warehouses => CachedValueHelper.GetValue(ref warehouses, GetWarehouses);
	CachedValue<IWarehouse> warehouses;

	public IReadOnlyCollection<IDocument> PreviousDocuments => previousDocuments ??= GetPreviousDocuments();
	IReadOnlyCollection<IDocument> previousDocuments;

	public IReadOnlyCollection<ISupportingDocument> SupportingDocuments => supportingDocuments ??= GetSupportingDocuments();
	IReadOnlyCollection<ISupportingDocument> supportingDocuments;

	public IReadOnlyCollection<IDocument> AdditionalReferences => additionalReferences ??= GetAdditionalReferences();
	IReadOnlyCollection<IDocument> additionalReferences;

	public IReadOnlyCollection<IDocument> AdditionalInformation => additionalInformation ??= GetAdditionalInformation();
	IReadOnlyCollection<IDocument> additionalInformation;

	public IAESConsignment AESConsignment => CachedValueHelper.GetValue(ref aesConsignment, GetConsignment);
	CachedValue<IAESConsignment> aesConsignment;

	public IReadOnlyCollection<IGoodsItem> GoodsItems => goodsItems ??= EntryHeader.MergedLines.Select(GetNewGoodsItem).ToArray();
	IReadOnlyCollection<IGoodsItem> goodsItems;

	protected virtual IDeliveryTerms GetDeliveryTerms() => !CheckRuleC0375()
		? new AESDeliveryTermsProvider(EntryHeader.RandomHeader)
		: null;

	protected virtual IReadOnlyCollection<IDocument> GetAdditionalReferences() =>
		IsAESTransitionPeriod
			? Array.Empty<IDocument>()
			: EntryInstruction.AdditionalInfos
				.Cast<AdditionalInfo>()
				.Where(x => x.CSI_SubType == AdditionalInfoKindList.Codes.REF)
				.Select((x, i) => new AESDocumentProvider(x, true))
				.ToArray<IDocument>();

	protected virtual IAESConsignment GetConsignment() => new AESConsignmentProvider(EntryHeader);

	protected virtual IGoodsItem GetNewGoodsItem(CusEntryLine entryLine, int index) => new AESGoodsItemProvider(entryLine, index + 1, this);

	protected virtual string GetNatureOfTransaction() => !CheckRuleC0375()
		&& CheckRuleC0273()
			? EntryHeader.InvoiceHeaders.FirstOrDefault()?.JZ_ValuationCode
			: null;

	bool CheckRuleC0273() => EntryInstruction.Invoices
		.Select(x => x.JZ_ValuationCode)
		.Distinct()
		.Count() == 1;

	bool CheckRuleC0375() => EntryInstruction.HasSubStyleEqualsBorCorEorF();

	bool CheckRuleC0561() => EntryHeader.InvoiceLines.Any(x =>
	{
		var previousProcedureCode = x.PreviousProcedureCode;
		return previousProcedureCode == Constants.ProcedureCodes._71
			|| previousProcedureCode == Constants.ProcedureCodes._76
			|| previousProcedureCode == Constants.ProcedureCodes._77;
	});

	bool IsAESTransitionPeriod => EntryInstruction.IsAESTransitionPeriod();

	IReadOnlyCollection<IDocument> GetAdditionalInformation() =>
		IsAESTransitionPeriod && EntryInstruction.CEI_SubStyle != Constants.SubStyleCodes.Z
			? Array.Empty<IDocument>()
			: EntryInstruction.GetAdditionalInfosBySubtypeAndDistinctByCode(AdditionalInfoKindList.Codes.INF, new ZString[] { Constants.AdditionalInfoCodes._4PL03 })
				.OrderBy(x => RuleR0093E.GetKeyEuLessThanPl(x.CSI_Code, RuleR0093E.Patterns.a1an4))
				.Select((x, i) => new AESDocumentProvider(x, false))
				.ToArray();

	IReadOnlyCollection<IDocument> GetPreviousDocuments() => IsAESTransitionPeriod && !EntryInstruction.HasSubStyleEqualsXorYorZ()
		? Array.Empty<IDocument>()
		: EntryInstruction.PreviousDocuments
			.Cast<PreviousDocument>()
			.OrderBy(x => RuleR0093E.GetKeyEuLessThanPl(x.CSI_Code, RuleR0093E.Patterns.n1an3))
			.Select((x, i) => new AESDocumentProvider(x, true))
			.ToArray<IDocument>();

	ISupportingDocument[] GetSupportingDocuments() => IsAESTransitionPeriod
		? Array.Empty<ISupportingDocument>()
		: EntryInstruction.SupportingDocuments
			.Cast<SupportingDocument>()
			.OrderBy(x => RuleR0093E.GetKeyEuLessThanPl(x.CSI_Code, RuleR0093E.Patterns.n1an3))
			.Select((x, i) => new AESGoodsShipmentSupportingDocumentProvider(x, i + 1))
			.ToArray<ISupportingDocument>();

	string GetCountryOfDestination()
	{
		var invoiceLines = declaration.InvoiceLines.Cast<JobComInvoiceLine>();
		var uniqueCountriesOfDestination = invoiceLines.Select(x => x.ZG_CountryOfDestination).Distinct().ToList();
		if (uniqueCountriesOfDestination.Count == 1)
		{
			var returnCountry = declaration.JE_GoodsDestination.IsEmpty ? uniqueCountriesOfDestination.Single() : declaration.JE_GoodsDestination;
			return returnCountry.IsEmpty ? null : returnCountry.ToString();
		}

		var countriesOfDestination = uniqueCountriesOfDestination
			.Union(new[] { declaration.JE_GoodsDestination })
			.Where(x => x != ZString.Empty)
			.Distinct().ToList();

		return countriesOfDestination.Count == 1
			? countriesOfDestination.First().ToString()
			: null;
	}

	IWarehouse GetWarehouses()
	{
		var cpwCusCode = EntryInstruction.Warehouse?.CustomsCodes.GetOrgCusCodeObjectForCodeTypeAndCountry(
			OrgCusCode.CodeTypes.WarehouseControlledPremisesID,
			Core.Constants.CountryCodes.Poland);
		return cpwCusCode != null
			&& !CheckRuleC0375()
			&& CheckRuleC0561()
				? new AESWarehouseProvider(cpwCusCode)
				: null;
	}
}
