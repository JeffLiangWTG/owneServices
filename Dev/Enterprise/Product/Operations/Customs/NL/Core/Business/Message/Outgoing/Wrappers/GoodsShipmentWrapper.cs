using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business;

public class GoodsShipmentWrapper : IGoodsShipment
{
	public GoodsShipmentWrapper(CusEntryHeader cusEntryHeader, JobDeclarationMessageSendingObject messageSendingObject)
	{
		entryHeader = Argument.NotNull(cusEntryHeader, nameof(cusEntryHeader));
		this.messageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
		declaration = Argument.NotNull(cusEntryHeader.Declaration, nameof(cusEntryHeader.Declaration));
		randomHeader = Argument.NotNull(cusEntryHeader.RandomHeader, nameof(cusEntryHeader.RandomHeader));
		entryInstruction = Argument.NotNull(cusEntryHeader.EntryInstruction, nameof(cusEntryHeader.EntryInstruction));
	}
	readonly CusEntryHeader entryHeader;
	readonly JobDeclaration declaration;
	readonly JobComInvoiceHeader randomHeader;
	readonly CusEntryInstruction entryInstruction;
	readonly JobDeclarationMessageSendingObject messageSendingObject;

	public int SequenceNumeric => 1;

	public string TransactionNatureCode => GetTransactionNatureCode();

	string GetTransactionNatureCode()
	{
		var transitionPeriod = declaration.IsExport ? FuncsHelper.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, ZDateTime.Today, null, FuncsHelper.ValidationOptions.UseCountryDefinitionFirstOtherwiseEUN)
													: FuncsHelper.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.DMSTransitionPeriod, ZDateTime.Today, null, FuncsHelper.ValidationOptions.UseCountryDefinitionFirstOtherwiseEUN);

		if (allowedDeclarationTypesForTransactionNature.Contains(entryInstruction.CEI_Style) && !transitionPeriod && !entryInstruction.ZG_TransNature.IsEmpty)
		{
			var transNatureInvLines = declaration.Invoices.SelectMany(x => x.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.ZG_TransNature).Where(x => !x.IsEmpty));
			var transNatureInvHeaders = declaration.Invoices.Select(x => x.JZ_ValuationCode).Distinct();

			return transNatureInvLines.IsNullOrEmpty() && transNatureInvHeaders.Count() == 1 && !randomHeader.JZ_ValuationCode.IsEmpty ? randomHeader.JZ_ValuationCode : entryInstruction.ZG_TransNature;
		}
		else if (allowedDeclarationTypesForTransactionNature.Contains(entryInstruction.CEI_Style) &&  transitionPeriod && !entryInstruction.ZG_TransNature.IsEmpty)
		{
			return entryInstruction.ZG_TransNature;
		}
		return string.Empty;
	}

	public string DispatchCountryCode => DispatchCountryCodeHelper.GetDispatchCountryCodeForShipment();

	public decimal InvoiceAmount => entryHeader.InvoiceHeaders.Sum(x => x.InvoiceAmount.Amount);

	public string InvoiceCurrencyCode => randomHeader.InvoiceAmount.Currency?.Code;

	public string AcceptanceDateTime => entryHeader.DMSFallbackIsActive ? entryHeader.FallbackEntryNumberIssueDate.ToString("yyyyMMdd") : entryHeader.MovementReferenceNumberIssueDate.ToString("yyyyMMdd");

	public IReadOnlyCollection<IAEOMutualRecognitionParty> AEOMutualRecognitionParties => aeoMutualRecognitionParties ??= entryInstruction
		.CusSupplyChainActorReferences.Cast<EU.Business.Declaration.CusSupplyChainActorReference>()
		.Select((x, index) => new AEOMutualRecognitionPartyWrapper(x, index + 1)).ToArray();
	IReadOnlyCollection<IAEOMutualRecognitionParty> aeoMutualRecognitionParties;

	public IParty Buyer => CachedValueHelper.GetValue(ref buyer, () => GetBuyer());

	CachedValue<IParty> buyer;

	IParty GetBuyer()
	{
		if (entryInstruction.CEI_Style.In(new ZString[] { DeclarationTypeList.Codes.H1, DeclarationTypeList.Codes.I1 }))
		{
			if (entryHeader.AllLineBuyersAreEmptyOrEqualToDeclaration())
			{
				return PartyWrapper.New(declaration.ConsigneeOrgAddress);
			}
		}
		return null;
	}

	public IReadOnlyCollection<IAdditionalReference> AdditionalReferences => additionalReferences ??= entryInstruction.AdditionalInfos.Where(x => x.IsAnAdditionalReference).Select((x, index) => new AdditionalReferenceWrapper(x, index + 1)).ToArray();
	IReadOnlyCollection<IAdditionalReference> additionalReferences;

	public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformations ??= entryInstruction.AdditionalInfos.Where(x => x.IsAnAdditionalInformation).Select((x, index) => new AdditionalInformationWrapper(x, index + 1)).ToArray();
	IReadOnlyCollection<IAdditionalInformation> additionalInformations;

	public IConsignment Consignment => new ConsignmentWrapper(entryHeader, messageSendingObject);

	public IDestination Destination => CachedValueHelper.GetValue(ref destination, () => GetDestination());

	CachedValue<IDestination> destination;

	IDestination GetDestination()
	{
		if (WrapperHelper.IsDestinationSpecified(entryHeader.EntryInstruction.CEI_Style))
		{
			var declarationDestination = declaration.JE_RL_NKFinalDestination;
			if (!declarationDestination.IsEmpty && entryHeader.AllLineDestinationsEqualToDeclaration())
			{
				return new DestinationWrapper(declarationDestination);
			}
		}
		return null;
	}

	public IReadOnlyCollection<IDomesticDutyTaxParty> DomesticDutyTaxParties => domesticDutyTaxParties ??= entryInstruction.FiscalReferences.Select((x, index) => new DomesticDutyTaxPartyWrapper(x, index + 1)).ToArray();
	IReadOnlyCollection<IDomesticDutyTaxParty> domesticDutyTaxParties;

	public string ExportCountry
	{
		get
		{
			var exportCountryOnShipment = declaration.Origin?.Country.Code ?? string.Empty;
			var distinctCountriesOnInvoiceLines = declaration.DistinctCountriesOnInvoiceLines;

			return distinctCountriesOnInvoiceLines.Count == 1 ? distinctCountriesOnInvoiceLines.First() : string.Empty;
		}
	}

	public IParty Exporter => PartyWrapper.New(declaration.Exporter);

	public IReadOnlyCollection<IPreviousDocument> PreviousDocuments => previousDocuments ??= entryInstruction.PreviousDocuments.Cast<EU.Business.Declaration.MultiLineAddInfos.PreviousDocument>().Select((x, index) => new PreviousDocumentWrapper(x, index + 1)).ToArray();
	IReadOnlyCollection<IPreviousDocument> previousDocuments;

	public IParty Seller => CachedValueHelper.GetValue(ref seller, () => GetSeller());

	CachedValue<IParty> seller;

	IParty GetSeller()
	{
		if (entryInstruction.CEI_Style.In(new ZString[] { DeclarationTypeList.Codes.H1, DeclarationTypeList.Codes.I1 }))
		{
			if (entryHeader.AllLineSellersAreEmptyOrEqualToDeclaration())
			{
				return PartyWrapper.New(declaration.Seller);
			}
		}
		return null;
	}

	public IReadOnlyCollection<ISupportingDocument> SupportingDocuments => supportingDocuments ??= entryInstruction.SupportingDocuments.Cast<SupportingDocument>().Select((x, index) => new SupportingDocumentWrapper(x, index + 1)).ToArray();
	IReadOnlyCollection<ISupportingDocument> supportingDocuments;

	public ITradeTerms TradeTerms => new TradeTermsWrapper(entryHeader);

	public IReadOnlyCollection<IGoodsItem> GovernmentAgencyGoodsItems => governmentAgencyGoodsItems ??= entryHeader.MergedLines.Select(x => new GoodsItemWrapper(x, messageSendingObject)).ToArray();
	IReadOnlyCollection<IGoodsItem> governmentAgencyGoodsItems;

	public IWarehouse Warehouse
	{
		get
		{
			IWarehouse result = null;
			if (declaration.WarehouseAddress?.Header is OrgHeader warehouseHeader)
			{
				var authorizationHeaders = CusAuthorisationHeader.Loader.GetAuthorisations(warehouseHeader.Factory, Core.Constants.CountryCodes.Netherlands, new ZString[] { Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP }, ZDateTime.Today, warehouseHeader.PK);
				if (authorizationHeaders.FirstOrDefault() is Customs.Business.CusAuthorisationHeader cusAuthorisationHeader)
				{
					result = new WarehouseWrapper(cusAuthorisationHeader);
				}
			}
			return result;
		}
	}

	public string ExitDateTime => entryHeader.CH_ExitDate.ToString("yyyyMMdd");
	DispatchCountryCodeHelper DispatchCountryCodeHelper => dispatchCountryCodeHelper ?? (dispatchCountryCodeHelper = new DispatchCountryCodeHelper(entryHeader));

	DispatchCountryCodeHelper dispatchCountryCodeHelper;

	readonly List<string> allowedDeclarationTypesForTransactionNature =
	[
			DeclarationTypeList.Codes.B1,
			DeclarationTypeList.Codes.B2,
			DeclarationTypeList.Codes.B4,
			DeclarationTypeList.Codes.C1,
			DeclarationTypeList.Codes.H1,
			DeclarationTypeList.Codes.H3,
			DeclarationTypeList.Codes.H4,
			DeclarationTypeList.Codes.H5,
			DeclarationTypeList.Codes.I1,
	];
}
