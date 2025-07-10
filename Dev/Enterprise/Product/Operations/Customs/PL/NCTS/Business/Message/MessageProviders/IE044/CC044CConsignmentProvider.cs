using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.NCTS.Business.Message.MessageProviders.IE044;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC044CConsignmentProvider : ICC044CConsignment
{
	public CC044CConsignmentProvider(NctsArrivalMovementHeader movementHeader, ICC044C cc044cProvider)
	{
		this.movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));
		nctsHeader = Argument.NotNull(movementHeader.Header, $"{nameof(NctsArrivalMovementHeader)}.{nameof(NctsArrivalMovementHeader.Header)}");
	}

	readonly NctsArrivalMovementHeader movementHeader;
	readonly NctsHeader nctsHeader;

	public decimal? GrossMass => movementHeader.BM_NoChangesToReport ? null : WeightRounding.Round(nctsHeader.IsInPhase5TransitionPeriod, movementHeader.BM_GrossWeightUnloaded);

	public IReadOnlyCollection<ITransportEquipment> TransportEquipment => transportEquipment ?? (transportEquipment = GetTransportEquipment());
	IReadOnlyCollection<ITransportEquipment> transportEquipment;

	public IReadOnlyCollection<IDepartureTransportMeans> DepartureTransportMeans => departureTransportMeans ?? (departureTransportMeans = GetDepartureTransportMeans());
	IReadOnlyCollection<IDepartureTransportMeans> departureTransportMeans;

	public IReadOnlyCollection<ICC044CHouseConsignment> HouseConsignment => houseConsignment ?? (houseConsignment = GetHouseConsignments());
	IReadOnlyCollection<ICC044CHouseConsignment> houseConsignment;

	public IReadOnlyCollection<ISupportingDocument> SupportingDocument => supportingDocument ?? (supportingDocument = GetSupportingDocument());
	IReadOnlyCollection<ISupportingDocument> supportingDocument;

	public IReadOnlyCollection<IDocument> TransportDocument => transportDocument ?? (transportDocument = GetTransportDocument());
	IReadOnlyCollection<IDocument> transportDocument;

	public IReadOnlyCollection<IDocument> AdditionalReference => additionalReference ?? (additionalReference = GetAdditionalReference());
	IReadOnlyCollection<IDocument> additionalReference;

	IReadOnlyCollection<ITransportEquipment> GetTransportEquipment() => nctsHeader.ArrivalHeaderContainers.Cast<NctsArrivalHeaderContainer>()
		.Select((x, i) => new CC044CTransportEquipmentsProvider(i + 1, x))
		.ToArray();

	IReadOnlyCollection<IDepartureTransportMeans> GetDepartureTransportMeans() => movementHeader.ArrivalTransportInfos.Cast<ArrivalCusTransportMeans>()
		.Select((x, i) => new DepartureTransportMeansProvider(i + 1, x.TPM_TypeOfIdentification, x.TPM_IdentificationNumber, x.TPM_RN_NKTransportNationality))
		.ToArray();

	IReadOnlyCollection<ICC044CHouseConsignment> GetHouseConsignments() => nctsHeader.Bills.Cast<NctsBill>()
		.Select((x, i) => new CC044CHouseConsignmentProvider(i + 1, x))
		.ToArray();

	IReadOnlyCollection<ISupportingDocument> GetSupportingDocument() => movementHeader.SupportingDocuments
		.Select((x, i) => new SupportingDocumentProvider(i + 1, x, nctsHeader.IsInPhase5TransitionPeriod))
		.ToArray();

	IReadOnlyCollection<IDocument> GetTransportDocument() => movementHeader.AdditionalDocuments
			.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument)
			.Select((x, i) => new TransportDocumentProvider(i + 1, x))
			.ToArray();

	IReadOnlyCollection<IDocument> GetAdditionalReference() => movementHeader.AdditionalDocuments
			.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference)
			.Select((x, i) => new CC044CAdditionalReferenceProvider(i + 1, x))
			.ToArray();
}
