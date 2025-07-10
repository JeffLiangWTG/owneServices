using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDYardTransportationUnitRatingDatesProvider<T>(T parent) : JobDatesProvider<T>(parent)
		where T : BusinessObject, ICYDJobInvoicingSupporter
	{
		protected override ZDateTime GetArrivalDateCore()
		{
			return GetGateInDateOverrideCore();
		}

		protected override ZDateTime GetDepartureDateCore()
		{
			return GetGateOutDateOverrideCore();
		}

		protected override ZDateTime GetGateInDateOverrideCore()
		{
			if (Parent is CYDTransportationUnit transportationUnit)
			{
				return transportationUnit.YTU_GateInTime.ToLocalZDateTime();
			}
			return ZDateTime.Empty;
		}

		protected override ZDateTime GetGateOutDateOverrideCore()
		{
			if (Parent is CYDTransportationUnit transportationUnit)
			{
				return transportationUnit.YTU_GateOutTime.ToLocalZDateTime();
			}
			return ZDateTime.Empty;
		}
	}
}
