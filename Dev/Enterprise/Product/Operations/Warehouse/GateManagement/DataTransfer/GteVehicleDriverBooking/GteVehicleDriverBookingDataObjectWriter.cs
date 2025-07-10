using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.GateManagement.Business;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	public class GteVehicleDriverBookingDataObjectWriter : DataObjectWriter<GteVehicleDriverBooking, Crew>
	{
		public GteVehicleDriverBookingDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
		}

		protected override Crew PopulateDataObject(GteVehicleDriverBooking sourceBO)
		{
			var crew = new Crew();

			crew.FullName = sourceBO.GBD_DriverName;
			crew.LicenseNumber = sourceBO.GBD_DriverLicenseNumber;
			crew.CrewType = CrewType.Driver;

			return crew;
		}
	}
}
