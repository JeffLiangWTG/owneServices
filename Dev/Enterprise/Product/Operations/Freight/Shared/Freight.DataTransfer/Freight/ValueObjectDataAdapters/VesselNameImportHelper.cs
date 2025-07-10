using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer
{
	public static class VesselNameImportHelper
	{
		public static ZString MatchVesselAndGetVesselName(Xsd.SailingForPlannedLegs sailing, BusinessObjectFactory factory)
		{
			return MatchVesselAndGetVesselNameCore(sailing.VesselName, sailing.LloydsNo, factory);
		}

		public static ZString MatchVesselAndGetVesselName(Xsd.SailingWithVesselVoyage sailing, BusinessObjectFactory factory)
		{
			return MatchVesselAndGetVesselNameCore(sailing.VesselName, sailing.LloydsNo, factory);
		}

		static ZString MatchVesselAndGetVesselNameCore(ZString vesselName, ZString lloydsNo, BusinessObjectFactory factory)
		{
			if (lloydsNo.IsEmpty)
			{
				return vesselName;
			}

			var vessel = RefVessel.LookupVesselByLloyds(lloydsNo, factory) ?? RefVessel.LookupVesselByCode(vesselName, factory);
			if (vessel != null)
			{
				return vessel.RV_Name;
			}
			else
			{
				return vesselName.IsEmpty ? lloydsNo : vesselName;
			}
		}
	}
}
