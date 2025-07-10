using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	public interface ITSWStatus
	{
		JobDeclaration Declaration { get; }
		ZString ResponseStatus { get; }
		ZString EnterpriseStatus { get; }
		ZString Agency { get; }
	}
}
