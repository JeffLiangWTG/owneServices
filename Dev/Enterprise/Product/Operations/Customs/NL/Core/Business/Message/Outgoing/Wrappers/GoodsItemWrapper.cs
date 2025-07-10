using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.NL.Business.Declaration.CusEntryHeader;
using CusEntryLine = Enterprise.Customs.NL.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.NL.Business;

public class GoodsItemWrapper : IGoodsItem
{
	public GoodsItemWrapper(CusEntryLine cusEntryLine, JobDeclarationMessageSendingObject messageSendingObject)
	{
		this.cusEntryLine = Argument.NotNull(cusEntryLine, nameof(cusEntryLine));
		this.messageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
		cusEntryHeader = Argument.NotNull(cusEntryLine.Header, nameof(cusEntryLine.Header));
		entryInstruction = Argument.NotNull(cusEntryHeader.EntryInstruction, nameof(cusEntryHeader.EntryInstruction));
		declaration = Argument.NotNull(cusEntryHeader.Declaration, nameof(cusEntryHeader.Declaration));
		randomLine = Argument.NotNull(cusEntryLine.RandomLine, nameof(cusEntryLine.RandomLine));
		randomHeader = Argument.NotNull(randomLine.InvoiceHeader, nameof(randomLine.InvoiceHeader));
	}
	readonly CusEntryLine cusEntryLine;
	readonly JobDeclaration declaration;
	readonly CusEntryHeader cusEntryHeader;
	readonly JobComInvoiceHeader randomHeader;
	readonly JobComInvoiceLine randomLine;
	readonly CusEntryInstruction entryInstruction;
	readonly JobDeclarationMessageSendingObject messageSendingObject;

	public int SequenceNumeric => cusEntryLine.CL_LineNumber;

	public decimal CustomsValueAmount => cusEntryLine.CL_CustomsValue;

	public string CurrencyId => cusEntryLine.InvoiceCurrency?.Code;

	public decimal StatisticalValueAmount => cusEntryLine.CL_StatisticalValue;

	public string TransactionNatureCode => GetTransactionNature();

	string GetTransactionNature()
	{
		if (allowedDeclarationTypesForTransactionNature.Contains(entryInstruction.CEI_Style) && entryInstruction.ZG_TransNature.IsEmpty)
		{
			var transitionPeriod = declaration.IsExport ? FuncsHelper.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, ZDateTime.Today, null, FuncsHelper.ValidationOptions.UseCountryDefinitionFirstOtherwiseEUN)
														: FuncsHelper.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.DMSTransitionPeriod, ZDateTime.Today, null, FuncsHelper.ValidationOptions.UseCountryDefinitionFirstOtherwiseEUN);
			if (transitionPeriod)
			{
				return randomLine.ZG_TransNature.IsEmpty ? randomHeader.JZ_ValuationCode : randomLine.ZG_TransNature;
			}
			else
			{
				var transNatureInvLines = declaration.Invoices.SelectMany(x => x.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.ZG_TransNature).Where(x => !x.IsEmpty));
				if (transNatureInvLines.Any())
				{
					return randomLine.ZG_TransNature.IsEmpty ? randomHeader.JZ_ValuationCode : randomLine.ZG_TransNature;
				}
				else
				{
					var transNatureInvHeaders = declaration.Invoices.Select(x => x.JZ_ValuationCode).Distinct();
					return transNatureInvHeaders.Count() > 1 ? randomHeader.JZ_ValuationCode : string.Empty;
				}
			}
		}
		else
		{
			return string.Empty;
		}
	}

	public string DispatchCountryCode => DispatchCountryCodeHelper.GetDispatchCountryCodeForGoodsItem(cusEntryLine);

	public string AcceptanceDateTime => null;

	public IReadOnlyCollection<IAdditionalReference> AdditionalReferences => additionalReferences ??= cusEntryLine.AdditionalInfos.Cast<AdditionalInfo>().Where(x => x.IsAnAdditionalReference).Select((x, index) => new AdditionalReferenceWrapper(x, index + 1)).ToArray();
	IReadOnlyCollection<IAdditionalReference> additionalReferences;

	public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformations ??= cusEntryLine.AdditionalInfos.Cast<AdditionalInfo>().Where(x => x.IsAnAdditionalInformation).Select((x, index) => new AdditionalInformationWrapper(x, index + 1)).ToArray();
	IReadOnlyCollection<IAdditionalInformation> additionalInformations;

	public IReadOnlyCollection<IAEOMutualRecognitionParty> AEOMutualRecognitionParties => aeoMutualRecognitionParties ??= randomLine.CusSupplyChainActorReferences.Cast<EU.Business.Declaration.CusSupplyChainActorReference>()
		.Select((x, index) => new AEOMutualRecognitionPartyWrapper(x, index + 1)).ToArray();
	IReadOnlyCollection<IAEOMutualRecognitionParty> aeoMutualRecognitionParties;

	public IReadOnlyCollection<IAuthorisation> Authorisations => authorisations ??= cusEntryLine.CusAuthorizationUsages.Cast<CusAuthorizationUsage>().Select((x, index) => new AuthorisationWrapper(x, index + 1)).ToArray();
	IReadOnlyCollection<IAuthorisation> authorisations;

	public IParty Buyer => CachedValueHelper.GetValue(ref buyer, () => GetBuyer());

	CachedValue<IParty> buyer;

	IParty GetBuyer()
	{
		if (entryInstruction.CEI_Style.In(new ZString[] { DeclarationTypeList.Codes.H1, DeclarationTypeList.Codes.I1 }))
		{
			if (!cusEntryHeader.AllLineBuyersAreEmptyOrEqualToDeclaration())
			{
				return PartyWrapper.New(randomLine.BuyerDocAddress.Organisation);
			}
		}
		return null;
	}

	public ICommodity Commodity => new CommodityWrapper(cusEntryLine);

	public ICustomsValuation CustomsValuation => new CustomsValuationWrapper(randomLine);

	public IDestination Destination => CachedValueHelper.GetValue(ref destination, () => GetDestination());
	CachedValue<IDestination> destination;

	IDestination GetDestination()
	{
		IDestination result = null;
		var countryOfDestination_Declaration = declaration.JE_RL_NKFinalDestination.Left(2);

		if (WrapperHelper.IsDestinationSpecified(entryInstruction.CEI_Style) && !cusEntryHeader.AllLineDestinationsEqualToDeclaration())
		{
			var countryOfDestiantion_Line = randomLine.ZG_CountryOfDestination;
			if (!countryOfDestiantion_Line.EqualsIgnoringCase(countryOfDestination_Declaration))
			{
				result = countryOfDestiantion_Line.IsEmpty ? new DestinationWrapper(countryOfDestination_Declaration) : new DestinationWrapper(countryOfDestiantion_Line);
			}
			else
			{
				result = new DestinationWrapper(countryOfDestiantion_Line);
			}
		}
		return result;
	}

	public IParty Exporter => PartyWrapper.New(declaration.Exporter);

	public IProcedure GovernmentProcedure => new GovernmentProcedureWrapper(randomLine);

	public IReadOnlyCollection<IOrigin> Origins => origins ??= GetOrigins().ToArray();
	IReadOnlyCollection<IOrigin> origins;

	IEnumerable<IOrigin> GetOrigins()
	{
		var primaryPreference = randomLine.JI_PrimaryPreference;
		var countryOfOrigin = randomLine.JI_CountryOfOrigin;
		var countryOfSupply = randomLine.ZG_CountryOfSupply;
		if (declaration.IsExport)
		{
			if (!countryOfOrigin.IsEmpty)
			{
				yield return new OriginWrapper(countryOfOrigin, "1", 1);
			}
		}
		else
		{
			switch (primaryPreference.SubstringSafe(0, 1))
			{
				case "1":
					if (!countryOfOrigin.IsEmpty)
					{
						yield return new OriginWrapper(countryOfOrigin, "1", 1);
					}
					break;
				case "2":
				case "3":
				case "4":
					if (!countryOfSupply.IsEmpty)
					{
						yield return new OriginWrapper(countryOfSupply, "1", 1);
					}
					if (!countryOfOrigin.IsEmpty)
					{
						yield return new OriginWrapper(countryOfOrigin, "2", 2);
					}
					break;
			}
		}
	}

	public IReadOnlyCollection<IPackaging> Packagings => packagings ??= declaration.Packages.Select((x, index) => new PackagingWrapper(x, index + 1)).ToArray();
	IReadOnlyCollection<IPackaging> packagings;

	public IReadOnlyCollection<IPreviousDocument> PreviousDocuments => previousDocuments ??= cusEntryLine.PreviousDocuments.Cast<EU.Business.Declaration.MultiLineAddInfos.PreviousDocument>().Where(x => x.ParentIsJobComInvoiceLine).Select((x, index) => new PreviousDocumentWrapper(x, index + 1)).ToArray();
	IReadOnlyCollection<IPreviousDocument> previousDocuments;

	public IParty Seller => CachedValueHelper.GetValue(ref seller, () => GetSeller());

	CachedValue<IParty> seller;

	IParty GetSeller()
	{
		if (entryInstruction.CEI_Style.In(new ZString[] { DeclarationTypeList.Codes.H1, DeclarationTypeList.Codes.I1 }))
		{
			if (!cusEntryHeader.AllLineSellersAreEmptyOrEqualToDeclaration())
			{
				return PartyWrapper.New(randomLine.SellerDocAddress.Organisation);
			}
		}
		return null;
	}

	public IReadOnlyCollection<ISupportingDocument> SupportingDocuments => supportingDocuments ??= cusEntryLine.SupportingDocuments.Cast<SupportingDocument>().Select((x, index) => new SupportingDocumentWrapper(x, index + 1)).ToArray();
	IReadOnlyCollection<ISupportingDocument> supportingDocuments;

	public IUCR UCR => new UCRWrapper(randomHeader.JZ_UCR);

	public IValuationAdjustment ValuationAdjustment => null;

	public IReadOnlyCollection<ITransportContractDocument> TransportContractDocuments => transportContractDocuments ??= cusEntryLine.AdditionalInfos.Cast<AdditionalInfo>().Where(x => x.IsATransportDocument).Select((x, index) => new TransportContractDocumentWrapper(x, index + 1)).ToArray();
	IReadOnlyCollection<ITransportContractDocument> transportContractDocuments;

	public string ExportCountry
	{
		get
		{
			var exportCountryOnShipment = declaration.Origin?.Country.Code ?? null;
			var distinctCountriesOnInvoiceLines = declaration.DistinctCountriesOnInvoiceLines;
			var exportCountryOnInvoiceLineOrInheritFromShipmentIfEmpty = randomLine.JI_RN_NKCountryOfExport.IsEmpty ? exportCountryOnShipment : randomLine.JI_RN_NKCountryOfExport;

			return distinctCountriesOnInvoiceLines.Count == 1 ? null : exportCountryOnInvoiceLineOrInheritFromShipmentIfEmpty;
		}
	}

	public IParty Consignee => CachedValueHelper.GetValue(ref consignee, () =>
	{
		IParty result = null;
		if (IncludeConsignee)
		{
			result = randomLine.ConsigneeAddress?.Header is OrgHeader orgHeader ? PartyWrapper.New(orgHeader) : PartyWrapper.New(declaration.ImporterDocumentaryAddress?.Organisation);
		}
		return result;
	});
	CachedValue<IParty> consignee;

	bool IncludeConsignee
	{
		get
		{
			if (messageSendingObject.MessageType == ExportSendMessageTypes.Codes.DEC && declaration.IsExport)
			{
				return !cusEntryHeader.AllLineConsigneesAreEmpty() && !cusEntryLine.AdditionalInfos.Any((AdditionalInfo x) => x.IsAnAdditionalInformation && x.CSI_Code == EU.Business.UniversalReferenceConstants.AdditionalDocumentTypes._30600)
					&& !entryInstruction.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.IsAnAdditionalInformation && x.CSI_Code == EU.Business.UniversalReferenceConstants.AdditionalDocumentTypes._30600);
			}
			return true;
		}
	}

	public IParty Consignor => CachedValueHelper.GetValue(ref consignor, () =>
	{
		IParty result = null;
		if (IncludeConsignor)
		{
			result = randomLine.ExporterAddress?.Header is OrgHeader orgHeader ? PartyWrapper.New(orgHeader) : PartyWrapper.New(declaration.SupplierDocumentaryAddress?.Organisation);
		}
		return result;
	});
	CachedValue<IParty> consignor;

	bool IncludeConsignor => !(messageSendingObject.MessageType == ExportSendMessageTypes.Codes.DEC && declaration.IsExport) || !cusEntryHeader.AllLineConsignorsAreEmpty();
	public IFreight Freight => new FreightWrapper(cusEntryHeader.RandomHeader);

	DispatchCountryCodeHelper DispatchCountryCodeHelper => dispatchCountryCodeHelper ?? (dispatchCountryCodeHelper = new DispatchCountryCodeHelper(cusEntryHeader));

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
