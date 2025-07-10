using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public interface IContainerDefaultingStrategy
	{
		OrgAddress CalculateReleaseContainerYard();
		OrgAddress CalculateReturnContanierYard();
		(ZDateTime returnedBy, ZDateTime availableDate, ZString availableDateName) CalculateRequiredBy();
		(ZDateTime storageStart, ZDateTime availableDate, ZString availableDateName) CalculateStorageStart(ZString direction);
		ZDateTime CalculateStorageStartAvailableDate(ZString direction, ZString creditorType);

		IContainerPenaltyMatchResult GetMatchedStoragePenalty(ZString direction, ZString creditorType, ZString processType, CommonShipment shipment = null);
		IContainerPenaltyMatchResult GetMatchedDetentionPenalty(ZString detentionPort, ZString direction, ZString processType, CommonShipment shipment = null);
		IContainerPenaltyMatchResult GetMatchedMDDPenalty(ZString detentionPort, ZString direction, ZString processType, CommonShipment shipment = null);

		ContainerPenaltyDate CalculateAvailableDateForStorage(ZString direction, ZString creditorType, ZString processType, CommonShipment shipment = null);
		ContainerPenaltyDate CalculateAvailableDateForDetention(ZString detentionPort, ZString direction, ZString processType, CommonShipment shipment = null);
		ContainerPenaltyDate CalculateAvailableDateForMDD(ZString detentionPort, ZString direction, ZString processType, CommonShipment shipment = null);
		IEnumerable<IContainerPenaltyMatchResult> CalculateMatchedPenalties(ZString processType, CommonShipment shipment = null);
	}
}
