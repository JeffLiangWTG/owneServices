using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IScreeningStatusProvider
	{
		ZString ScreeningStatus { get; set; }
	}
}
