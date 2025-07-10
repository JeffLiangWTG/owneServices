using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC170CProvider : CCProviderBase, ICC170C
{
	public CC170CProvider(NctsDepartureMovementHeader movementHeader, string messageType)
		: base(Argument.NotNull(movementHeader, nameof(movementHeader)), Argument.NotNull(messageType, nameof(messageType)))
	{
		this.movementHeader = movementHeader;
	}

	readonly NctsDepartureMovementHeader movementHeader;

	public string TransitOperationLRN => EDIMessage.PL_NCTS_LRN_PlaceHolder;

	public DateTime? TransitOperationLimitDate => CachedValueHelper.GetValue(ref transitOperationLimitDate, () => !movementHeader.BM_ExportDate.IsEmpty && movementHeader.IsSimplifiedNctsProcedure
		? movementHeader.BM_ExportDate.ToDateTime()
		: null);
	CachedValue<DateTime?> transitOperationLimitDate;

	public ICustomsOffice CustomsOfficeOfDeparture => CachedValueHelper.GetValue(ref customsOfficeOfDeparture, () => CustomsOfficeProvider.NewOrNull(GetCustomsOfficeOfType(EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture)));
	CachedValue<ICustomsOffice> customsOfficeOfDeparture;

	public IHolderOfTheTransitProcedureWithMaxLength HolderOfTheTransitProcedure => holderOfTheTransitProcedure ??= new HolderOfTheTransitProcedureProvider(nctsHeader.Principal, movementHeader, useMaxLengthWithDependency: false);
	IHolderOfTheTransitProcedureWithMaxLength holderOfTheTransitProcedure;

	public IRepresentative Representative => representative ??= new RepresentativeProvider(movementHeader.Representative, HolderOfTheTransitProcedure);
	IRepresentative representative;

	public ICC170CConsignment Consignment => consignment ??= new CC170CConsignmentProvider(movementHeader, this);
	ICC170CConsignment consignment;

	public PhaseID? PhaseID => CachedValueHelper.GetValue(ref phaseId, () => movementHeader.IsInPhase5TransitionPeriod
		? CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS.PhaseID.NCTS_5_0
		: CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS.PhaseID.NCTS_5_1);
	CachedValue<PhaseID?> phaseId;

	NctsPLOfficeCode GetCustomsOfficeOfType(ZString officeCode) => nctsHeader.MovementHeader.CustomsOffices.Cast<NctsPLOfficeCode>().FirstOrDefault(x => x.CY_Code == officeCode);
}
