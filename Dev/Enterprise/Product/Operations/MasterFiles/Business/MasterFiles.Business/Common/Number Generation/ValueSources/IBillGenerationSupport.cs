
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IBillGenerationSupport
	{
		BusinessObjectFactory Factory { get; }

		OrgHeader CarrierPrincipal { get; }
		ZString TransportMode { get; }
		ZString ServiceLevel { get; }
		RefUNLOCO Origin { get; }
		RefUNLOCO Destination { get; }
		RefUNLOCO Load { get; }
		RefUNLOCO Discharge { get; }
		ZString TranshipmentIndicator { get; }
	}
}
