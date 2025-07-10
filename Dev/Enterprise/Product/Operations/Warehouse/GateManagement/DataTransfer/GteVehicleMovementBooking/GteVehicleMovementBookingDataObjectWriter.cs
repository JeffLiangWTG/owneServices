using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.GateManagement.Business;
using WTG.StaticAnalysis.Annotation;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	[CodeAlive("This DataObjectWriter is to be used in the future")]
	public class GteVehicleMovementBookingDataObjectWriter : DataObjectWriter<GteVehicleMovementBooking, UniversalShipment>
	{
		public GteVehicleMovementBookingDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(GteVehicleMovementBooking vehicleMovementBooking)
		{
			var dataObject = new UniversalShipment(writeManager.WriterStrategy);
			var vehicleRun = new VehicleRun();
			var vehicle = new Vehicle();
			var vehicleRegistration = new Registration();

			vehicleRegistration.Number = vehicleMovementBooking.GBV_VehicleRegistration;
			vehicle.Registration = vehicleRegistration;
			vehicleRun.Vehicle = vehicle;
			dataObject.VehicleRun = vehicleRun;

			var refContainer = vehicleMovementBooking.VehicleType;
			if (refContainer != null)
			{
				vehicle.VehicleType = new CodeDescriptionPair10Char()
				{
					Code = refContainer.RC_Code,
					Description = refContainer.RC_DescriptionMultilingual
				};
			}
			return dataObject;
		}
	}
}
