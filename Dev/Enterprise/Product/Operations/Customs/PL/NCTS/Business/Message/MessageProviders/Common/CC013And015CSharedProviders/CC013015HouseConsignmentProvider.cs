using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public abstract class CC013015HouseConsignmentProvider : ConsignmentProviderBase, IHouseConsignment
{
	protected CC013015HouseConsignmentProvider(int sequenceNumber, NctsBill bill)
		: base(bill)
	{
		this.bill = bill;
		SequenceNumber = sequenceNumber.ToString();
	}

	readonly NctsBill bill;

	public string SequenceNumber { get; }

	public decimal GrossMass => CachedValueHelper.GetValue(ref grossMass, () => WeightRounding.Round(InPhase5TransitionPeriod, bill.GrossWeightInKilograms));
	CachedValue<decimal> grossMass;

	public IConsignor Consignor => CachedValueHelper.GetValue(ref consignor, () => GetConsignor(bill.Consignor));
	CachedValue<IConsignor> consignor;

	public IReadOnlyCollection<IConsignmentItem> ConsignmentItem => consignmentItem ??= GetConsignmentItems();// WI00560677 / WI00560299
	IReadOnlyCollection<IConsignmentItem> consignmentItem;

	public string CountryOfDispatch => CachedValueHelper.GetValue(ref countryOfDispatch, GetCountryOfDispatch);
	CachedValue<string> countryOfDispatch;

	public string ReferenceNumberUCR => CachedValueHelper.GetValue(ref referenceNumberUCR, GetReferenceNumberUCR);
	CachedValue<string> referenceNumberUCR;

	public string TransportCharges => CachedValueHelper.GetValue(ref transportCharges, () => GetTransportCharges()?.MethodOfPayment);
	CachedValue<string> transportCharges;

	public IConsignee Consignee => CachedValueHelper.GetValue(ref consignee, () => GetConsignee(bill.Consignee));
	CachedValue<IConsignee> consignee;

	public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActor => additionalSupplyChainActor ??= GetAdditionalSupplyChainActor();
	IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActor;

	public IReadOnlyCollection<IAdditionalReference> AdditionalReference => additionalReference ??= GetAdditionalReference();
	IReadOnlyCollection<IAdditionalReference> additionalReference;

	public IReadOnlyCollection<IAdditionalInformation> AdditionalInformation => additionalInformation ??= GetAdditionalInformation();
	IReadOnlyCollection<IAdditionalInformation> additionalInformation;

	public IReadOnlyCollection<IComplementedDocument> PreviousDocument => previousDocument ??= GetPreviousDocument();
	IReadOnlyCollection<IComplementedDocument> previousDocument;

	public IReadOnlyCollection<ISupportingDocument> SupportingDocument => supportingDocument ??= GetSupportingDocument();
	IReadOnlyCollection<ISupportingDocument> supportingDocument;

	public IReadOnlyCollection<IDocument> TransportDocument => transportDocument ??= GetTransportDocument();
	IReadOnlyCollection<IDocument> transportDocument;

	protected abstract IConsignment ParentConsignment { get; }

	protected abstract ITransitOperation TransitOperation { get; }

	protected override string GetDepartureTransportMeansTransportMode()
		=> ParentConsignment is IConsignment consignment
			&& !InPhase5TransitionPeriod
			&& consignment.DepartureTransportMeans.IsNullOrEmpty()
			&& !bill.TransportTypeAtDeparture.IsEmpty
		? consignment.InlandModeOfTransport
		: null;

	string GetCountryOfDispatch()
		=> ParentConsignment is IConsignment consignment
			&& consignment.CountryOfDispatch.IsNullOrEmpty()
			? bill.B0_RN_NKCountryOfExport
			: null;

	string GetReferenceNumberUCR()
		=> ParentConsignment is IConsignment consignment
			&& consignment.ReferenceNumberUCR.IsNullOrEmpty()
			&& !InPhase5TransitionPeriod
		? bill.B0_ReferenceID
		: null;

	ITransportCharges GetTransportCharges() => InPhase5TransitionPeriod || CheckRuleC0186
		? null
		: new TransportChargesProvider(bill);

	bool CheckRuleC0186 => TransitOperation.Security == ExportSecurityTypeList.Codes.NotUsed;

	IConsignor GetConsignor(JobDocAddress consignor)
		=> consignor.IsValidAddress
			&& CheckRuleC0542
			&& CheckRuleG0123
			&& !InPhase5TransitionPeriod
		? new ConsignorProvider(consignor, InPhase5TransitionPeriod)
		: null;

	bool CheckRuleC0542 => TransitOperation is ITransitOperation transitOperation
		&& transitOperation.Security != ExportSecurityTypeList.Codes.NotUsed
		&& transitOperation.ReducedDatasetIndicator != NCTSIndicator.YES;

	bool CheckRuleG0123 => bill.Header.Bills.Cast<NctsBill>().Any(b => b.Consignor.OrganisationPK != bill.Header.Principal.E2_OA_Address);

	IConsignee GetConsignee(JobDocAddress consignee)
		=> consignee.IsValidAddress
			&& CheckRuleC0001
			&& !InPhase5TransitionPeriod
		? new ConsigneeProvider(consignee, InPhase5TransitionPeriod)
		: null;

	bool CheckRuleC0001 => bill.Header is NctsHeader header
		&& NoInfoToSkipConsignee(header.AdditionalDocuments)
		&& NoInfoToSkipConsignee(bill.AdditionalDocuments)
		&& header.Consignee.OrganisationPK.IsEmpty;

	bool NoInfoToSkipConsignee(IEnumerable<AdditionalInfo> additionalInfos)
		=> additionalInfos.All(x => x.CSI_SubType != AdditionalInfoSubTypeList.Codes.AdditionalInformation || x.CSI_Code != PL.Business.Constants.AdditionalInfoCodes._30600);

	IReadOnlyCollection<IAdditionalSupplyChainActor> GetAdditionalSupplyChainActor() => bill.CusSupplyChainActorReferences.Cast<CusSupplyChainActorReference>()
		.Select((x, i) => new CC013015CAdditionalSupplyChainActorProvider(i + 1, x))
		.ToArray();

	IReadOnlyCollection<IConsignmentItem> GetConsignmentItems() => bill.GoodsItems
		.OrderBy(x => x.BY_DeclarationGoodsItemNumber)
		.Select(x => new CC013015CConsignmentItemProvider(x))
		.ToArray();

	IReadOnlyCollection<IDocument> GetTransportDocument() => !InPhase5TransitionPeriod
		? bill.AdditionalDocuments
			.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument)
			.Select((x, i) => new TransportDocumentProvider(i + 1, x))
			.ToArray()
		: null;

	IReadOnlyCollection<IAdditionalInformation> GetAdditionalInformation() => !InPhase5TransitionPeriod
		? bill.AdditionalDocuments
			.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation)
			.Select((x, i) => new AdditionalInformationProvider(i + 1, x))
			.ToArray()
		: null;

	IReadOnlyCollection<IAdditionalReference> GetAdditionalReference() => !InPhase5TransitionPeriod
		? bill.AdditionalDocuments
			.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference)
			.Select((x, i) => new AdditionalReferenceProvider(i + 1, x, InPhase5TransitionPeriod))
			.ToArray()
		: null;

	IReadOnlyCollection<ISupportingDocument> GetSupportingDocument() => !InPhase5TransitionPeriod
		? bill.SupportingDocuments
			.Select((x, i) => new SupportingDocumentProvider(i + 1, x, InPhase5TransitionPeriod))
			.ToArray()
		: null;

	IReadOnlyCollection<IComplementedDocument> GetPreviousDocument() => !InPhase5TransitionPeriod
		? bill.PreviousDocuments.Lines
			.Cast<CommonPreviousDocument>()
			.Select((x, i) => new PreviousDocumentProvider(i + 1, x, InPhase5TransitionPeriod))
			.ToArray()
		: null;
}
