using CargoWise.EntityFramework;
using CargoWise.Types;
namespace Enterprise.MasterFiles.Integration
{
	public interface IRefVesselNameFinder
	{
		ZString FindVesselNameByLloydsNumber(ZString lloydsNumber, BusinessObjectFactory factory);
	}
}
