using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

static class BaseEDIMessageExtensions
{
	public static NctsCommonMovementHeader GetRelatedCommonMovementHeader(this BaseEDIMessage message)
		=> message?.EM_LinkedObject switch
		{
			NctsHeader nctsHeader => nctsHeader.ArrivalMovementHeader,
			NctsDepartureMovementHeader movementHeader => movementHeader,
			_ => null
		};

	public static NctsHeader GetRelatedNctsHeader(this BaseEDIMessage message)
		=> message?.EM_LinkedObject switch
		{
			NctsHeader nctsHeader => nctsHeader,
			NctsDepartureMovementHeader movementHeader => movementHeader.Header,
			_ => null
		};
}
