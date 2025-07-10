using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC044CProvider : MessageHeaderProvider, ICC044C
{
	public CC044CProvider(NctsHeader nctsHeader) : base(nctsHeader)
	{
		arrivalMovementHeader = Argument.NotNull(nctsHeader.ArrivalMovementHeader, nameof(nctsHeader.ArrivalMovementHeader));
	}

	readonly NctsArrivalMovementHeader arrivalMovementHeader;

	public string MRN => nctsHeader.MovementReferenceNumber;

	public string OtherThingsToReport => arrivalMovementHeader.OtherThingsToReport;

	public string CustomsOfficeOfDestination => arrivalMovementHeader.DestinationCustomsOfficeCode;

	public string TraderIdentificationNumber => CachedValueHelper.GetValue(ref traderIdentificationNumber, () => nctsHeader.DestinationTrader?.Organisation.GetIdentificationNumber());
	CachedValue<string> traderIdentificationNumber;

	public bool Conform => arrivalMovementHeader.BM_NoChangesToReport;

	public bool UnloadingCompletion => arrivalMovementHeader.BM_UnloadingCompleted;

	public string StateOfSeals => HasDeclaredSeals ? arrivalMovementHeader.BM_StateOfSealsBoolean ? "1" : "0" : null;
	bool HasDeclaredSeals => CachedValueHelper.GetValue(ref hasDeclaredSeals, () => nctsHeader.ArrivalHeaderContainers.Cast<NctsArrivalHeaderContainer>().Any(c => !c.TotalSealCount.IsEmpty && c.BC_UnloadedState != NctsUnloadedStateList.Codes.NEW) || nctsHeader.EnRouteIncidents.Any(i => i.IncidentContainers.Any(c => !c.TotalSealCount.IsEmpty)));
	CachedValue<bool> hasDeclaredSeals;

	public DateTime Unloadingdate => arrivalMovementHeader.BM_UnloadingDate.IsValid ? arrivalMovementHeader.BM_UnloadingDate.ToDateTime() : DateTime.MinValue;

	public string UnloadingRemark => arrivalMovementHeader.BM_UnloadingRemarks;

	public INCTSConsignmentType06 Consignment => CachedValueHelper.GetValue(ref consignment, () => EmitConsignment ? new ConsignmentType06Provider(nctsHeader) : null);
	CachedValue<ConsignmentType06Provider> consignment;

	public override string MessageType => NLConstants.WCoTypeCodes.UnloadingRemarks;

	bool EmitConsignment => CachedValueHelper.GetValue(ref emitConsignment, () => (!arrivalMovementHeader.BM_NoChangesToReport && nctsHeader.Bills.Any(x => x.MovementDetail.B9_UnloadedState != NctsUnloadedStateList.Codes.DEC))
								|| arrivalMovementHeader.ArrivalTransportInfos.Any(x => x.TPM_TransportState != NctsUnloadedStateList.Codes.DEC)
								|| nctsHeader.ArrivalHeaderContainers.Cast<NctsArrivalHeaderContainer>().Any(x => x.BC_UnloadedState != NctsUnloadedStateList.Codes.DEC)
								|| nctsHeader.ArrivalHeaderContainers.Cast<NctsArrivalHeaderContainer>().SelectMany(x => x.Seals).Cast<CusSeal>().Any(x => x.BK_UnloadingState != NctsUnloadedStateList.Codes.DEC && x.BK_UnloadingState != NctsUnloadedStateList.Codes.DAM));
	CachedValue<bool> emitConsignment;
}


