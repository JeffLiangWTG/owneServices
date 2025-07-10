using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public interface IScreeningPartyProvider : IScreeningStatusProvider
	{
		ScreeningParty[] ScreeningParties { get; }
		ZString GetWorstScreeningStatus();
		ZString GetWorstScreeningStatusUnlessManuallyCleared();
	}
}
