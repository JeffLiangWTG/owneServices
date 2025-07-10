using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using ILocationOfGoods = CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS.ILocationOfGoods;

namespace Enterprise.Customs.PL.NCTS.Business;

public abstract class CC013015ConsignmentProvider : ConsignmentProviderBase, IConsignment
{
	protected CC013015ConsignmentProvider(NctsDepartureMovementHeader movementHeader, string messageCode)
		: base(movementHeader)
	{
		this.movementHeader = movementHeader;
		nctsHeader = Argument.NotNull(movementHeader.Header, $"{nameof(movementHeader)}.{nameof(NctsDepartureMovementHeader.Header)}");
		this.messageCode = Argument.NotNullOrEmpty(messageCode, nameof(messageCode));
	}

	readonly NctsDepartureMovementHeader movementHeader;
	readonly NctsHeader nctsHeader;
	readonly string messageCode;

	public string CountryOfDestination => movementHeader.BM_RL_NKDestinationPort;

	public NCTSIndicator? ContainerIndicator => nctsHeader.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>()
		.Any(x => x.BC_ContainerNum != ZString.Empty && x.IsContainerised) ? NCTSIndicator.YES : NCTSIndicator.NO;

	public string InlandModeOfTransport => CachedValueHelper.GetValue(ref inlandModeOfTransport, () => movementHeader.BM_InlandTransportMode);
	CachedValue<string> inlandModeOfTransport;

	public string ModeOfTransportAtTheBorder => movementHeader.BM_ExportTransportMode;

	public decimal GrossMass => CachedValueHelper.GetValue(ref grossMass, () => WeightRounding.Round(InPhase5TransitionPeriod, movementHeader.GrossWeightInKilograms));
	CachedValue<decimal> grossMass;

	public ICarrier Carrier => CachedValueHelper.GetValue(ref carrier, () => movementHeader.Carrier != null ? new CarrierProvider(movementHeader.Carrier) : null);
	CachedValue<ICarrier> carrier;

	public IConsignor Consignor => CachedValueHelper.GetValue(ref consignor, () => GetConsignor(nctsHeader.Consignor));
	CachedValue<IConsignor> consignor;

	public IReadOnlyCollection<ITransportEquipment> TransportEquipment => transportEquipment ??= GetTransportEquipment();
	IReadOnlyCollection<ITransportEquipment> transportEquipment;

	public ILocationOfGoods LocationOfGoods => locationOfGoods ??= new LocationOfGoodsProvider(movementHeader.GoodsLocation);
	ILocationOfGoods locationOfGoods;

	public IReadOnlyCollection<ICountry> CountryOfRoutingOfConsignment => countryOfRoutingOfConsignment ??= GetCountryOfRoutingOfConsignment();
	IReadOnlyCollection<ICountry> countryOfRoutingOfConsignment;

	public IPlaceOfLoadingOrUnloading PlaceOfLoading => CachedValueHelper.GetValue(ref placeOfLoading, GetPlaceOfLoading);
	CachedValue<IPlaceOfLoadingOrUnloading> placeOfLoading;

	public IPlaceOfLoadingOrUnloading PlaceOfUnloading => CachedValueHelper.GetValue(ref placeOfUnloading, GetPlaceOfUnloading);
	CachedValue<IPlaceOfLoadingOrUnloading> placeOfUnloading;

	public IReadOnlyCollection<IHouseConsignment> HouseConsignment => houseConsignment ??= GetHouseConsignments();
	IReadOnlyCollection<IHouseConsignment> houseConsignment;

	public string CountryOfDispatch => CachedValueHelper.GetValue(ref countryOfDispatch, () => RuleB2104 ? movementHeader.BM_RN_NKCountryOfDispatch : null);
	CachedValue<string> countryOfDispatch;

	public string ReferenceNumberUCR => movementHeader.BM_UniqueConsignmentReference;

	public IConsignee Consignee => CachedValueHelper.GetValue(ref consignee, () => GetConsignee(nctsHeader.Consignee));
	CachedValue<IConsignee> consignee;

	public IReadOnlyCollection<IActiveBorderTransportMeansType> ActiveBorderTransportMeans => activeBorderTransportMeans ??= GetActiveBorderTransportMeans();
	IReadOnlyCollection<IActiveBorderTransportMeansType> activeBorderTransportMeans;

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

	public string TransportCharges => CachedValueHelper.GetValue(ref transportCharges, ()
		=> movementHeader.BM_TypeOfSecurity != NctsTypeOfSecurityList.Codes.NON ? movementHeader.BM_MethodOfPayment : null);
	CachedValue<string> transportCharges;

	public bool CheckRuleC0806()
		=> movementHeader.BM_ExportTransportMode != ModeOfTransportList.Codes._5_PostalConsignment
			&& (ActiveBorderTransportMeansIsRequired || !ActiveBorderTransportMeansIsEmpty);

	bool ActiveBorderTransportMeansIsEmpty
		=> movementHeader.BM_CustomsOfficeAtBorder.IsEmpty
			&& movementHeader.BM_ActiveBorderIdentificationType.IsEmpty
			&& movementHeader.BM_TOLCarrierID.IsEmpty
			&& movementHeader.BM_RN_NKTOLCarrierNationality.IsEmpty
			&& movementHeader.BM_ConveyanceNumber.IsEmpty;

	protected override string GetDepartureTransportMeansTransportMode() => InlandModeOfTransport;

	protected override string GetTransportMeanNationality(string transportMode, string value)
	{
		if (transportMode == ModeOfTransportList.Codes._2_RailTransport)
		{
			return GetTransportMeanNationality_B1897(value);
		}

		return value;
	}

	string GetTransportMeanNationality_B1897(string value)
		=> !(InPhase5TransitionPeriod && movementHeader.ValidationDecider is INctsDepartureMovementHeaderPhase5ValidationDecider validationDecider && validationDecider.IsRuleB1897Active) ? value : null;

	protected abstract bool HasCustomsOfficeOfTransitDeclared();

	protected abstract IReadOnlyCollection<IHouseConsignment> GetHouseConsignments();

	protected abstract ITransitOperation TransitOperation { get; }

	bool RuleC0542 => movementHeader.BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.NON && movementHeader.BM_ReducedDatasetIndicator;

	bool RuleG0123 => nctsHeader.Consignor.E2_OA_Address == nctsHeader.Principal.E2_OA_Address;

	bool RuleC0001 => nctsHeader.AdditionalDocuments.Any(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation
		&& x.CSI_Type == CusSupportingInfoTypeList.Codes.AdditionalInfo
		&& x.CSI_Code == PL.Business.Constants.AdditionalInfoCodes._30600);

	bool RuleB2104 => CachedValueHelper.GetValue(ref ruleB2104, ()
		=> !movementHeader.BM_RN_NKCountryOfDispatch.IsEmpty);
	CachedValue<bool> ruleB2104;

	IReadOnlyCollection<ITransportEquipment> GetTransportEquipment() => nctsHeader.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>()
		.Select((x, i) => new TransportEquipmentForDepartureProvider(i + 1, x, messageCode))
		.ToArray();

	IConsignor GetConsignor(JobDocAddress consignor)
		=> RuleC0542 || RuleG0123
			? null
			: new ConsignorProvider(consignor, InPhase5TransitionPeriod);

	IConsignee GetConsignee(JobDocAddress consignee)
		=> consignee.IsValidAddress && !RuleC0001
			? new ConsigneeProvider(consignee, InPhase5TransitionPeriod)
			: null;

	IReadOnlyCollection<ICountry> GetCountryOfRoutingOfConsignment()
	{
		var security = movementHeader.BM_TypeOfSecurity;
		return !security.IsEmpty && security != NctsTypeOfSecurityList.Codes.NON
			? nctsHeader.CountriesOfRouting.Select((x, i) => new CountryProvider(i + 1, x.CY_Data)).ToArray()
			: Array.Empty<ICountry>();
	}

	bool ActiveBorderTransportMeansIsRequired
		=> movementHeader.BM_AdditionalDeclarationType == NctsTypeOfAdditionalDeclarationList.Codes.A
			&& (movementHeader.BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.ENT
				|| movementHeader.BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.EXI
				|| movementHeader.BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.BTH);

	IPlaceOfLoadingOrUnloading GetPlaceOfLoading() => !InPhase5TransitionPeriod
		? PlaceOfLoadingOrUnloadingProvider.NewOrNull(movementHeader.BM_PortOfPresentationCode, movementHeader.BM_PlaceOfLoading, movementHeader.Factory, InPhase5TransitionPeriod)
		: movementHeader.BM_TypeOfSecurity != NctsTypeOfSecurityList.Codes.NON
			? PlaceOfLoadingOrUnloadingProvider.NewOrNull(movementHeader.BM_PortOfPresentationCode, movementHeader.BM_PlaceOfLoading, movementHeader.Factory, InPhase5TransitionPeriod)
			: null;

	IPlaceOfLoadingOrUnloading GetPlaceOfUnloading() => CheckRuleB1858() ? null
		: PlaceOfLoadingOrUnloadingProvider.NewOrNull(movementHeader.BM_ForeignDestPortKCode, movementHeader.BM_PlaceOfUnloading, movementHeader.Factory, InPhase5TransitionPeriod);

	bool CheckRuleB1858() => InPhase5TransitionPeriod
		? TransitOperation is ITransitOperation transitOperation && transitOperation.Security == ExportSecurityTypeList.Codes.NotUsed
		: CheckRuleC0191();

	bool CheckRuleC0191() => TransitOperation is ITransitOperation transitOperation && transitOperation.Security == ExportSecurityTypeList.Codes.NotUsed;

	IReadOnlyCollection<IActiveBorderTransportMeansType> GetActiveBorderTransportMeans()
	{
		var result = new List<IActiveBorderTransportMeansType>();

		if (InPhase5TransitionPeriod)
		{
			if (movementHeader.BM_ExportTransportMode != ModeOfTransportList.Codes._5_PostalConsignment || !ActiveBorderTransportMeansIsEmpty)
			{
				result.Add(new ActiveBorderTransportMeansTypeProvider(1, movementHeader));
			}
		}
		else if (CheckRuleC0806())
		{
			var sequenceNumber = 1;
			result.Add(new ActiveBorderTransportMeansTypeProvider(sequenceNumber, movementHeader));
			if (HasCustomsOfficeOfTransitDeclared())
			{
				foreach (var transportMean in movementHeader.AdditionalTransportAtBorderList.Take(8).Cast<DepartureCusTransportMeans>())
				{
					result.Add(new AdditionalBorderTransportMeansTypeProvider(++sequenceNumber, transportMean, movementHeader));
				}
			}
		}

		return result;
	}

	IReadOnlyCollection<IAdditionalSupplyChainActor> GetAdditionalSupplyChainActor() => nctsHeader.CusSupplyChainActors.Cast<CusSupplyChainActorReference>()
		.Select((x, i) => new CC013015CAdditionalSupplyChainActorProvider(i + 1, x))
		.ToArray();

	IReadOnlyCollection<IAdditionalReference> GetAdditionalReference() => !InPhase5TransitionPeriod
		? nctsHeader.AdditionalDocuments
			.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference)
			.Select((x, i) => new AdditionalReferenceProvider(i + 1, x, InPhase5TransitionPeriod))
			.ToArray()
		: Array.Empty<IAdditionalReference>();

	IReadOnlyCollection<IAdditionalInformation> GetAdditionalInformation()
	{
		if (InPhase5TransitionPeriod)
		{
			return Array.Empty<IAdditionalInformation>();
		}

		var specialCodes = new HashSet<string> { "POW01", "PCS01" };
		bool hasFoundSpecialCode = false;
		int sequenceNumber = 1;

		return nctsHeader.AdditionalDocuments
			.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation
				&& x.CSI_Type == CusSupportingInfoTypeList.Codes.AdditionalInfo
				&& IsNotSpecialCodeOrFirstSpecialCode(x.CSI_Code))
			.Select(x => new AdditionalInformationProvider(sequenceNumber++, x))
			.ToArray();

		bool IsNotSpecialCodeOrFirstSpecialCode(ZString code)
		{
			if (!specialCodes.Contains(code))
			{
				return true;
			}
			if (!hasFoundSpecialCode)
			{
				hasFoundSpecialCode = true;
				return true;
			}
			return false;
		}
	}

	IReadOnlyCollection<IComplementedDocument> GetPreviousDocument() => !InPhase5TransitionPeriod
		? nctsHeader.PreviousDocuments.Lines
			.Cast<CommonPreviousDocument>()
			.Select((x, i) => new PreviousDocumentProvider(i + 1, x, InPhase5TransitionPeriod))
			.ToArray()
		: Array.Empty<IComplementedDocument>();

	IReadOnlyCollection<ISupportingDocument> GetSupportingDocument() => !InPhase5TransitionPeriod
		? nctsHeader.MovementHeader.SupportingDocuments
			.Select((x, i) => new SupportingDocumentProvider(i + 1, x, InPhase5TransitionPeriod))
			.ToArray()
		: Array.Empty<ISupportingDocument>();

	IReadOnlyCollection<IDocument> GetTransportDocument() => !InPhase5TransitionPeriod
		? nctsHeader.AdditionalDocuments
			.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument)
			.Select((x, i) => new TransportDocumentProvider(i + 1, x))
			.ToArray()
		: Array.Empty<IDocument>();
}
