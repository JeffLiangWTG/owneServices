using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IRefVessel
	{
		ZString RV_Code { get; }
		ZString RV_LloydsNumber { get; }
	}
}
