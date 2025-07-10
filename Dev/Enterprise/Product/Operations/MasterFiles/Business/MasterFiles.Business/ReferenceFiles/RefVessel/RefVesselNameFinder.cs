using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class RefVesselNameFinder : IRefVesselNameFinder
	{
		public ZString FindVesselNameByLloydsNumber(ZString lloydsNumber, BusinessObjectFactory factory)
		{
			var vessel = RefVessel.LookupVesselByLloyds(lloydsNumber, factory);

			return vessel != null ? vessel.RV_Code : ZString.Empty;
		}
	}
}
