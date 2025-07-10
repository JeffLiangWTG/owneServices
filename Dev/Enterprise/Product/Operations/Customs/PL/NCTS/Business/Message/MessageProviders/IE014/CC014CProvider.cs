using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using ICustomsOffice = CargoWise.Customs.PL.MessageContracts.Interfaces.ICustomsOffice;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC014CProvider : CCProviderBase, ICC014C
{
	public CC014CProvider(NctsDepartureMovementHeader movementHeader, string messageType, MessageSendingObject messageSendingObject)
		: base(Argument.NotNull(movementHeader, nameof(movementHeader)), Argument.NotNull(messageType, nameof(messageType)))
	{
		this.movementHeader = movementHeader;
		this.messageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
	}

	readonly NctsDepartureMovementHeader movementHeader;
	readonly MessageSendingObject messageSendingObject;
	public ICC014CTransitOperation TransitOperation => transitOperation ?? (transitOperation = new CC014CTransitOperationProvider(messageSendingObject));
	ICC014CTransitOperation transitOperation;

	public ICustomsOffice CustomsOfficeOfDeparture => CachedValueHelper.GetValue(ref customsOfficeOfDeparture, () => CustomsOfficeProvider.NewOrNull(GetCustomsOfficeOfType(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture)));
	CachedValue<ICustomsOffice> customsOfficeOfDeparture;

	public IHolderOfTheTransitProcedureWithMaxLength HolderOfTheTransitProcedure => holderOfTheTransitProcedure ?? (holderOfTheTransitProcedure = new HolderOfTheTransitProcedureProvider(nctsHeader.Principal, movementHeader));
	IHolderOfTheTransitProcedureWithMaxLength holderOfTheTransitProcedure;

	public PhaseID? PhaseID => CachedValueHelper.GetValue(ref phaseId, () => movementHeader.IsInPhase5TransitionPeriod
		? CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS.PhaseID.NCTS_5_0
		: CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS.PhaseID.NCTS_5_1);
	CachedValue<PhaseID?> phaseId;

	public ICC014CInvalidation Invalidation => invalidation ?? (invalidation = new CC014InvalidationProvider(movementHeader, messageSendingObject));
	ICC014CInvalidation invalidation;

	NctsPLOfficeCode GetCustomsOfficeOfType(ZString officeCode) => nctsHeader.MovementHeader.CustomsOffices.Cast<NctsPLOfficeCode>().FirstOrDefault(x => x.CY_Code == officeCode);
}
