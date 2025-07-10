using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public abstract class NctsMessageInterpreterBase<TDataProvider>(NctsCommonMovementHeader movementHeader)
	: MessageHtmlInterpreterBase<NctsCommonMovementHeader, TDataProvider>(movementHeader)
	where TDataProvider : class
{
	protected NctsCommonMovementHeader MovementHeader => LinkedObject;
	protected NctsHeader NctsHeader => (NctsHeader)MovementHeader.Header;
	protected BusinessObjectFactory Factory => MovementHeader.Factory;
	protected NctsDepartureMovementHeader DepartureMovementHeader => MovementHeader as NctsDepartureMovementHeader ?? NctsHeader.MovementHeader;
	protected NctsArrivalMovementHeader ArrivalMovementHeader => MovementHeader as NctsArrivalMovementHeader ?? NctsHeader.ArrivalMovementHeader;
}
