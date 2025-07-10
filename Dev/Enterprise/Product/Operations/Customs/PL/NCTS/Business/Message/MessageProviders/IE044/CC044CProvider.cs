using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.PL.NCTS.Business.Message.MessageProviders.Common;
using ICustomsOffice = CargoWise.Customs.PL.MessageContracts.Interfaces.ICustomsOffice;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC044CProvider : CCProviderBase, ICC044C
{
	public CC044CProvider(NctsArrivalMovementHeader movementHeader, string messageType)
		: base(Argument.NotNull(movementHeader, nameof(movementHeader)), Argument.NotNull(messageType, nameof(messageType)))
	{
		this.movementHeader = movementHeader;
	}

	readonly NctsArrivalMovementHeader movementHeader;

	public ICC044CTransitOperation TransitOperation => transitOperation ?? (transitOperation = new CC044CTransitOperationProvider(movementHeader));
	ICC044CTransitOperation transitOperation;

	public ICustomsOffice CustomsOfficeOfDestinationActual => CachedValueHelper.GetValue(ref officeOfDestination, () => GetOfficeOfDestination());
	CachedValue<ICustomsOffice> officeOfDestination;

	public ITraderAtDestination TraderAtDestination => CachedValueHelper.GetValue(ref traderAtDestination, () => GetTrader());
	CachedValue<ITraderAtDestination> traderAtDestination;

	public IUnloadingRemark UnloadingRemark => unloadingRemark ?? (unloadingRemark = new UnloadingRemarkProvider(movementHeader));
	IUnloadingRemark unloadingRemark;

	public ICC044CConsignment Consignment => consignment ?? (consignment = new CC044CConsignmentProvider(movementHeader, this));
	ICC044CConsignment consignment;

	public PhaseID? PhaseID => CachedValueHelper.GetValue(ref phaseId, () => movementHeader.IsInPhase5TransitionPeriod
		? CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS.PhaseID.NCTS_5_0
		: CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS.PhaseID.NCTS_5_1);
	CachedValue<PhaseID?> phaseId;

	ICustomsOffice GetOfficeOfDestination()
	{
		var office = movementHeader.CustomsOffices.Cast<NctsPLOfficeCode>()
			.FirstOrDefault((NctsPLOfficeCode x) => x.CY_Code == EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival);
		return CustomsOfficeProvider.NewOrNull(office);
	}

	ITraderAtDestination GetTrader()
	{
		return movementHeader.Header?.DestinationTrader is { Address: not null } jobDocAddress
			? new TraderAtDestinationProvider(jobDocAddress)
			: null;
	}
}
