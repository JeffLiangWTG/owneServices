using System;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.PL.NCTS.Business.Message.MessageProviders.Common;

class UnloadingRemarkProvider : IUnloadingRemark
{
	public UnloadingRemarkProvider(NctsArrivalMovementHeader movementHeader)
	{
		this.movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));
	}

	readonly NctsArrivalMovementHeader movementHeader;

	public NCTSIndicator Conform => CachedValueHelper.GetValue(ref conform, () => movementHeader.BM_NoChangesToReport ? NCTSIndicator.YES : NCTSIndicator.NO);
	CachedValue<NCTSIndicator> conform;

	public NCTSIndicator UnloadingCompletion => CachedValueHelper.GetValue(ref unloadingCompletion, () => movementHeader.BM_UnloadingCompleted ? NCTSIndicator.YES : NCTSIndicator.NO);
	CachedValue<NCTSIndicator> unloadingCompletion;

	public DateTime UnloadingDate => movementHeader.BM_UnloadingDate.ToDateTime();

	public NCTSIndicator? StateOfSeals => CachedValueHelper.GetValue(ref stateOfSeals, () => movementHeader.BM_StateOfSealsBoolean ? NCTSIndicator.YES : NCTSIndicator.NO);
	CachedValue<NCTSIndicator> stateOfSeals;

	public string UnloadingRemark => movementHeader.BM_UnloadingRemarks;
}
