using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using ICustomsOffice = CargoWise.Customs.PL.MessageContracts.Interfaces.ICustomsOffice;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC007CProvider : CCProviderBase, ICC007C
{
	readonly NctsArrivalMovementHeader movementHeader;

	public CC007CProvider(NctsArrivalMovementHeader movementHeader, string messageType)
		: base(Argument.NotNull(movementHeader, nameof(movementHeader)), Argument.NotNull(messageType, nameof(messageType)))
	{
		this.movementHeader = movementHeader;
	}

	public ICC007CTransitOperation TransitOperation => transitOperation ?? (transitOperation = new CC007CTransitOperationProvider(movementHeader));
	ICC007CTransitOperation transitOperation;

	public ICustomsOffice CustomsOfficeOfDestinationActual => CachedValueHelper.GetValue(ref officeOfDestination, GetOfficeOfDestination);
	CachedValue<ICustomsOffice> officeOfDestination;

	public ITraderAtDestination TraderAtDestination => CachedValueHelper.GetValue(ref traderAtDestination, GetTrader);
	CachedValue<ITraderAtDestination> traderAtDestination;

	public ICC007Consignment Consignment => consignment ?? (consignment = new CC007ConsignmentProvider(movementHeader));
	ICC007Consignment consignment;

	public PhaseID? PhaseID => CachedValueHelper.GetValue(ref phaseId, () => movementHeader.IsInPhase5TransitionPeriod
		? CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS.PhaseID.NCTS_5_0
		: CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS.PhaseID.NCTS_5_1);
	CachedValue<PhaseID?> phaseId;

	ICustomsOffice GetOfficeOfDestination()
	{
		var office = movementHeader.Header.ArrivalMovementHeader.CustomsOffices.Cast<NctsPLOfficeCode>()
			.FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival);
		return CustomsOfficeProvider.NewOrNull(office);
	}

	ITraderAtDestination GetTrader()
	{
		return movementHeader.Header?.DestinationTrader is { Address: not null } jobDocAddress
			? new TraderAtDestinationProvider(jobDocAddress)
			: null;
	}
}
