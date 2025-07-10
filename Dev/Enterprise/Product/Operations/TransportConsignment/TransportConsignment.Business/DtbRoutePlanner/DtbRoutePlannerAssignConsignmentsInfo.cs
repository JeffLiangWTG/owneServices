using CargoWise.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbRoutePlannerAssignConsignmentsInfo
	{
		public DtbRoutePlannerAssignConsignmentsInfo(OrgHeader carrier)
		{
			Carrier = Argument.NotNull(carrier, "carrier");
			AssigningType = RunSheetView.Carriers;
		}

		public DtbRoutePlannerAssignConsignmentsInfo(GlbStaff driver)
		{
			Driver = Argument.NotNull(driver, "driver");
			AssigningType = RunSheetView.Drivers;
		}

		public DtbRoutePlannerAssignConsignmentsInfo(RefEquipment vehicle)
		{
			Vehicle = Argument.NotNull(vehicle, "vehicle");
			AssigningType = RunSheetView.Vehicles;
		}

		public readonly OrgHeader Carrier;
		public readonly GlbStaff Driver;
		public readonly RefEquipment Vehicle;
		public readonly RunSheetView AssigningType;
	}
}
