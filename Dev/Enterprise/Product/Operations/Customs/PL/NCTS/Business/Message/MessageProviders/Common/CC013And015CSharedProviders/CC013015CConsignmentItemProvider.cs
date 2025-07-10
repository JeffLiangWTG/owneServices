using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC013015CConsignmentItemProvider : IConsignmentItem
{
	public CC013015CConsignmentItemProvider(NctsDepartureCargoDesc goodsItem)
	{
		this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		header = (NctsHeader)Argument.NotNull(goodsItem.Header, $"{nameof(NctsDepartureCargoDesc)}.{nameof(NctsDepartureCargoDesc.Header)}");
		bill = (NctsBill)Argument.NotNull(goodsItem.Bill, $"{nameof(NctsDepartureCargoDesc)}.{nameof(NctsDepartureCargoDesc.Bill)}");
		movementHeader = (NctsDepartureMovementHeader)Argument.NotNull(goodsItem.MoveHeader, $"{nameof(NctsDepartureCargoDesc)}.{nameof(NctsDepartureCargoDesc.MoveHeader)}");
	}

	readonly NctsDepartureCargoDesc goodsItem;
	readonly NctsBill bill;
	readonly NctsDepartureMovementHeader movementHeader;
	readonly NctsHeader header;

	public string GoodsItemNumber => goodsItem.BY_LineNo.ToString();

	public string DeclarationGoodsItemNumber => goodsItem.BY_DeclarationGoodsItemNumber.ToString();

	public string DeclarationType => CachedValueHelper.GetValue(ref declarationType, () => CheckRuleC0045() ? goodsItem.BY_Type : null);
	CachedValue<string> declarationType;

	public string CountryOfDispatch => CachedValueHelper.GetValue(ref countryOfDispatch, () => CheckRuleB2104() ? goodsItem.BY_RN_NKCountryOfDispatch : null);
	CachedValue<string> countryOfDispatch;

	public string CountryOfDestination => CachedValueHelper.GetValue(ref countryOfDestination, () => CheckRuleC0343() ? goodsItem.BY_RN_NKCountryOfDestination : null);
	CachedValue<string> countryOfDestination;

	public string ReferenceNumberUCR => CachedValueHelper.GetValue(ref referenceNumberUCR, () =>
	{
		var isNeeded = IsInPhase5TransitionPeriod ? movementHeader.BM_UniqueConsignmentReference.IsEmpty : CheckRuleC0502();
		return isNeeded ? goodsItem.BY_CommercialReferenceNumber : null;
	});
	CachedValue<string> referenceNumberUCR;

	public IConsignee Consignee => CachedValueHelper.GetValue(ref consignee, () =>
		goodsItem.Consignee.IsValidAddress && CheckRulesB1820AndB2400AndG0001Combined()
			? new ConsigneeProvider(goodsItem.Consignee, IsInPhase5TransitionPeriod)
			: null);
	CachedValue<IConsignee> consignee;

	public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActor => additionalSupplyChainActor ??= GetAdditionalSupplyChainActor();
	IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActor;

	public ICommodity Commodity => new CommodityProvider(goodsItem);

	public IReadOnlyCollection<IPackaging> Packaging => packaging ??= GetPackaging();
	IReadOnlyCollection<IPackaging> packaging;

	public IReadOnlyCollection<IAdditionalReference> AdditionalReference => additionalReference ??= GetAdditionalReference();
	IReadOnlyCollection<IAdditionalReference> additionalReference;

	public IReadOnlyCollection<IAdditionalInformation> AdditionalInformation => additionalInformation ??= GetAdditionalInformation();
	IReadOnlyCollection<IAdditionalInformation> additionalInformation;

	public IReadOnlyCollection<IPreviousDocument> PreviousDocument => previousDocument ??= GetPreviousDocument();
	IReadOnlyCollection<IPreviousDocument> previousDocument;

	public IReadOnlyCollection<ISupportingDocument> SupportingDocument => supportingDocument ??= GetSupportingDocuments();
	IReadOnlyCollection<ISupportingDocument> supportingDocument;

	public IReadOnlyCollection<IDocument> TransportDocument => IsRuleB2400 ? Array.Empty<IDocument>() : (transportDocument ??= GetTransportDocument());
	IReadOnlyCollection<IDocument> transportDocument;

	public string TransportCharges => CachedValueHelper.GetValue(ref transportCharges, () => isRuleB1875 || IsRuleB2400 ? null : bill.GoodsItems[0]?.BY_TransportChargesMethodOfPayment);
	CachedValue<string> transportCharges;

	IReadOnlyCollection<IDocument> GetTransportDocument() => IsRuleB2400 ? null
		: goodsItem.AdditionalInfos.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument).Select((x, i) => new TransportDocumentProvider(i + 1, x)).ToList();

	IReadOnlyCollection<IAdditionalInformation> GetAdditionalInformation() => goodsItem.AdditionalInfos
		.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation)
		.Select((x, i) => new AdditionalInformationProvider(i + 1, x))
		.ToList();

	IReadOnlyCollection<IAdditionalReference> GetAdditionalReference() => goodsItem.AdditionalInfos
		.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference)
		.Select((x, i) => new AdditionalReferenceProvider(i + 1, x, IsInPhase5TransitionPeriod))
		.ToList();

	IReadOnlyCollection<IAdditionalSupplyChainActor> GetAdditionalSupplyChainActor() =>
		goodsItem.Bill.CusSupplyChainActorReferences.Cast<CusSupplyChainActorReference>()
			.Select((x, i) => new CC013015CAdditionalSupplyChainActorProvider(i + 1, x))
			.ToArray();

	IReadOnlyCollection<IPreviousDocument> GetPreviousDocument() => goodsItem.PreviousDocuments.Lines
		.Cast<NctsPreviousDocument>()
		.Select((x, i) => new PreviousDocumentProvider(i + 1, x, IsInPhase5TransitionPeriod))
		.ToArray();

	IReadOnlyCollection<ISupportingDocument> GetSupportingDocuments() => goodsItem.SupportingDocuments.Lines
		.Cast<NctsSupportingDocument>()
		.Select((x, i) => new SupportingDocumentProvider(i + 1, x, IsInPhase5TransitionPeriod))
		.ToArray();

	IReadOnlyCollection<IPackaging> GetPackaging() =>
		goodsItem.Packages.Cast<NctsPackage>()
			.Select((x, i) => new PackagingProvider(i + 1, x, IsInPhase5TransitionPeriod))
			.ToArray();

	bool IsRuleB2400 => !IsInPhase5TransitionPeriod;

	bool isRuleB1875 => IsInPhase5TransitionPeriod
						&& goodsItem.MoveHeader.BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.NON
						&& !goodsItem.MoveHeader.BM_MethodOfPayment.IsEmpty;

	bool CheckRuleC0045() => movementHeader.BM_InBondEntryType == NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5;

	bool CheckRuleB2104() => movementHeader.BM_RN_NKCountryOfDispatch.IsEmpty && bill.B0_RN_NKCountryOfExport.IsEmpty;

	bool CheckRuleC0343() => movementHeader.BM_RL_NKDestinationPort.IsEmpty;

	bool CheckRuleC0502() => movementHeader.BM_UniqueConsignmentReference.IsEmpty
							&& bill.B0_ReferenceID.IsEmpty;

	bool CheckRulesB1820AndB2400AndG0001Combined()
	{
		return IsInPhase5TransitionPeriod && !(header.Consignee.IsValidAddress || IsAdditionalDocumentINF30600Present());

		bool IsAdditionalDocumentINF30600Present()
		{
			Predicate<AdditionalInfo> predicate = x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation && x.CSI_Code == PL.Business.Constants.AdditionalInfoCodes._30600;
			return header.AdditionalDocuments.Any(x => predicate.Invoke(x))
				|| bill.AdditionalDocuments.Any(x => predicate.Invoke(x))
				|| goodsItem.AdditionalInfos.Any(x => predicate.Invoke(x));
		}
	}

	bool IsInPhase5TransitionPeriod => CachedValueHelper.GetValue(ref isInPhase5TransitionPeriod, () => header.IsInPhase5TransitionPeriod);
	CachedValue<bool> isInPhase5TransitionPeriod;
}
