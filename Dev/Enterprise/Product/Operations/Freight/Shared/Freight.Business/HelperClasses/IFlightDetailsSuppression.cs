using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public interface IFlightDetailsSuppression : IImportExport
	{
		bool IsAir { get; }
		ZBool HasActualRCVPassed { get; }
		ZBool HasETDPassed { get; }
		ZBool IsPassengerFlight { get; }
		ZBool HasFinalRoutingLegATDPassed { get; }
	}
}
