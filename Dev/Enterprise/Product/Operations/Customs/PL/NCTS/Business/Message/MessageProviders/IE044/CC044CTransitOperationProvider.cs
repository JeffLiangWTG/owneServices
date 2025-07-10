using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC044CTransitOperationProvider : ICC044CTransitOperation
{
	readonly NctsArrivalMovementHeader movementHeader;
	readonly NctsHeader nctsHeader;

	public CC044CTransitOperationProvider(NctsArrivalMovementHeader movementHeader)
	{
		this.movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));
		nctsHeader = Argument.NotNull(movementHeader.Header, nameof(movementHeader) + "." + nameof(movementHeader.Header));
	}

	public string MRN => nctsHeader.ArrivalMrnFromUser;

	public string OtherThingsToReport => movementHeader.OtherThingsToReport;
}
