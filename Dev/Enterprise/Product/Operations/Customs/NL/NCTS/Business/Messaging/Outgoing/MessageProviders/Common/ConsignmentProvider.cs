using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.NL.NCTS.Business;

public class ConsignmentProvider : INCTSConsignment
{
	protected readonly NctsHeader nctsHeader;
	protected readonly NctsCommonMovementHeader movementHeader;

	public ConsignmentProvider(NctsHeader nctsHeader)
	{
		this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		movementHeader = Argument.NotNull(nctsHeader.CommonMovementHeader, nameof(movementHeader));
	}

	public virtual string CountryOfDispatch => movementHeader.BM_RN_NKCountryOfDispatch;

	public string CountryOfDestination => movementHeader.BM_RL_NKDestinationPort;

	public bool ContainerIndicator => nctsHeader.DepartureHeaderContainers.Count > 0;

	public int? InlandModeOfTransport => InlandModeOfTransportCore;
	protected virtual int? InlandModeOfTransportCore => DateTimeProviderHelper.ConvertStringToNullableInt(movementHeader.BM_InlandTransportMode);

	public int? ModeOfTransportAtTheBorder => DateTimeProviderHelper.ConvertStringToNullableInt(movementHeader.BM_ExportTransportMode);

	public decimal GrossMass => movementHeader.BM_GrossWeight.Normalize();

	public virtual string ReferenceNumberUCR => movementHeader.BM_UniqueConsignmentReference;

	public INCTSParty Carrier => CachedValueHelper.GetValue(ref carrier, () => movementHeader.DocAddresses.Cast<JobDocAddress>().FirstOrDefault(x => x.E2_AddressType == AutoDocAddressTypes.Codes.Carrier) is JobDocAddress carrierAddress ? new PartyProvider(carrierAddress) : null);
	CachedValue<INCTSParty> carrier;

	public INCTSParty Consignee => CachedValueHelper.GetValue(ref consignee, () => nctsHeader.Consignee is JobDocAddress consigneeAddress ? new OmitNameAndAddressPartyProvider(consigneeAddress) : null);
	CachedValue<INCTSParty> consignee;

	public INCTSParty Consignor => ConsignorCore;
	protected virtual INCTSParty ConsignorCore => CachedValueHelper.GetValue(ref consignor, () => nctsHeader.Consignor is JobDocAddress consignorAddress ? new OmitNameAndAddressPartyProvider(consignorAddress) : null);
	CachedValue<INCTSParty> consignor;

	public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActors ??= GetAdditionalSupplyChainActorsCore();
	IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActors;
	protected IReadOnlyCollection<IAdditionalSupplyChainActor> GetAdditionalSupplyChainActorsCore() => NctsDataRetrieveMethods.GetCusReferences(movementHeader.Factory, movementHeader.PK).Select((cr, index) => new AdditionalSupplyChainActorProvider(cr, index + 1)).ToArray();

	public IReadOnlyCollection<CargoWise.Customs.NL.MessageContracts.Interfaces.INCTSTransportEquipment> TransportEquipments => transportEquipments ??= nctsHeader.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>().Select((ctr, index) => new TransportEquipmentsForNctsHeaderContainerProvider(ctr, index + 1)).ToArray<CargoWise.Customs.NL.MessageContracts.Interfaces.INCTSTransportEquipment>();
	IReadOnlyCollection<CargoWise.Customs.NL.MessageContracts.Interfaces.INCTSTransportEquipment> transportEquipments;

	public ILocationOfGoods LocationOfGoods => CachedValueHelper.GetValue(ref locationOfGoods, () => new LocationOfGoodsProvider(nctsHeader));
	CachedValue<ILocationOfGoods> locationOfGoods;

	public IReadOnlyCollection<INCTSDepartureTransportMeans> DepartureTransportMeans => departureTransportMeans ??= GetDepartureTransportMeans().ToArray();
	IReadOnlyCollection<INCTSDepartureTransportMeans> departureTransportMeans;

	public IReadOnlyCollection<ICountryOfRoutingOfConsignment> CountryOfRoutingOfConsignments => countryOfRoutingOfConsignments ??= nctsHeader.CountriesOfRouting.Cast<CountryOfRouting>().Select(it => new CountryOfRoutingOfConsignmentsProvider(it.CY_Data, it.CY_Order)).ToArray();
	ICountryOfRoutingOfConsignment[] countryOfRoutingOfConsignments;

	public IReadOnlyCollection<IActiveBorderTransportMeans> ActiveBorderTransportMeans => activeBorderTransportMeans ??= GetActiveBorderTransportMeans();
	IReadOnlyCollection<IActiveBorderTransportMeans> activeBorderTransportMeans;

	IReadOnlyCollection<IActiveBorderTransportMeans> GetActiveBorderTransportMeans() => !movementHeader.BM_ActiveBorderIdentificationType.IsEmpty && !movementHeader.BM_TOLCarrierID.IsEmpty && !movementHeader.BM_RN_NKTOLCarrierNationality.IsEmpty ? new IActiveBorderTransportMeans[] { new ActiveBorderTransportMeansProvider(movementHeader) } : Array.Empty<IActiveBorderTransportMeans>();

	public IPlace PlaceOfLoading => placeOfLoading ??= new PlaceOfLoadingProvider(movementHeader);
	IPlace placeOfLoading;

	public IPlace PlaceOfUnloading => placeOfUnloading ??= new PlaceOfUnloadingProvider(movementHeader);
	IPlace placeOfUnloading;

	public IReadOnlyCollection<INCTSPreviousDocument> PreviousDocuments => previousDocuments ??= nctsHeader.PreviousDocuments.Select(csi => new PreviousDocumentProvider(csi)).ToArray();
	IReadOnlyCollection<INCTSPreviousDocument> previousDocuments;

	public IReadOnlyCollection<INCTSSupportingDocument> SupportingDocuments => supportingDocuments ??= nctsHeader.MovementHeader.SupportingDocuments.Select(csi => new SupportingDocumentProvider(csi)).ToArray();
	IReadOnlyCollection<INCTSSupportingDocument> supportingDocuments;

	public IReadOnlyCollection<IDocument> TransportDocuments => transportDocuments ??= nctsHeader.AdditionalDocuments.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument).Select(csi => new DocumentProvider(csi)).ToArray();
	IReadOnlyCollection<IDocument> transportDocuments;

	public IReadOnlyCollection<IDocument> AdditionalReferences => additionalReferences ??= nctsHeader.AdditionalDocuments.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference).Select(csi => new DocumentProvider(csi)).ToArray();
	IReadOnlyCollection<IDocument> additionalReferences;

	public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformation ??= nctsHeader.AdditionalDocuments.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation).Select(csi => new AdditionalInformationProvider(csi)).ToArray();
	IReadOnlyCollection<IAdditionalInformation> additionalInformation;

	public IReadOnlyCollection<IHouseConsignment> HouseConsignments => houseConsignments ??= nctsHeader.Bills.Cast<NctsBill>().OrderBy(x => x.SequenceNumber).Select((bill) => new HouseConsignmentProvider(bill)).ToArray();
	IReadOnlyCollection<IHouseConsignment> houseConsignments;

	public string TransportChargesMethodOfPayment => movementHeader.BM_MethodOfPayment;

	public IReadOnlyCollection<INCTSIncident> Incidents => nctsHeader.BH_HeaderType == NctsMovementType.Codes.Arrival ? nctsHeader.EnRouteIncidents.Select((incident, index) => new IncidentProvider(incident, index + 1)).ToArray() : Array.Empty<INCTSIncident>();

	IEnumerable<INCTSDepartureTransportMeans> GetDepartureTransportMeans()
	{
		var sequence = 1;
		if (!movementHeader.BM_TransportAtDeparture.IsEmpty)
		{
			yield return new DepartureTransportMeansTransportAtDepartureProvider(movementHeader, sequence++);
		}
		if (!movementHeader.BM_TransportAtDepartureTrailer1RegNo.IsEmpty)
		{
			yield return new DepartureTransportMeansTransportAtDepartureTrailer1RegNoProvider(movementHeader, sequence++);
		}
		if (!movementHeader.BM_TransportAtDepartureTrailer2RegNo.IsEmpty)
		{
			yield return new DepartureTransportMeansTransportAtDepartureTrailer2RegNoProvider(movementHeader, sequence++);
		}
		if (!movementHeader.BM_AircraftIDAtDeparture.IsEmpty)
		{
			yield return new DepartureTransportMeansAircraftIDAtDepartureProvider(movementHeader, sequence++);
		}
	}

	protected bool ReducedDatasetIndicator => movementHeader.BM_ReducedDatasetIndicator;
}
