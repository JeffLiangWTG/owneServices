using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.GateManagement.Business;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	class GteVehicleMovementBookingCollectionDataObjectReader : DataObjectCollectionReader<UniversalShipment, GteVehicleMovementBooking>
	{
		public GteVehicleMovementBookingCollectionDataObjectReader(GteBooking booking, IXmlImportLogger logger, UniversalObjectFactory factory, UniversalShipment[] vehicleMovementBookingDataObjectCollection)
			: base(vehicleMovementBookingDataObjectCollection)
		{
			this.logger = Argument.NotNull(logger, nameof(logger));
			this.factory = Argument.NotNull(factory, nameof(factory));
			vehicleMovementBookingCollection = Argument.NotNull(booking.VehicleMovementBookings, nameof(booking.VehicleMovementBookings));
		}

		readonly GteVehicleMovementBookingCollection vehicleMovementBookingCollection;
		readonly IXmlImportLogger logger;
		readonly UniversalObjectFactory factory;

		protected override GteVehicleMovementBooking[] BusinessObjects => vehicleMovementBookingCollection.Cast<GteVehicleMovementBooking>().ToArray();

		protected override void AddToCollection(GteVehicleMovementBooking vehicleMovementBooking) => vehicleMovementBookingCollection.Add(vehicleMovementBooking);

		protected override void RemoveFromCollection(GteVehicleMovementBooking businessObject) => vehicleMovementBookingCollection.RemoveAndDelete(businessObject);

		protected override GteVehicleMovementBooking FindMatchingBusinessObject(UniversalShipment dataObject)
		{
			var vehicleRegistration = dataObject?.VehicleRun?.Vehicle?.Registration?.Number.Value ?? string.Empty;
			if (vehicleRegistration == string.Empty)
			{
				return null;
			}

			return vehicleMovementBookingCollection.Find(x => x.GBV_VehicleRegistration == vehicleRegistration).FirstOrDefault();
		}

		protected override GteVehicleMovementBooking ReadIntoBusinessObject(UniversalShipment dataObject, GteVehicleMovementBooking vehicleMovemement)
		{
			var reader = new GteVehicleMovementBookingDataObjectReader(dataObject, vehicleMovemement, logger, factory);
			return reader.ReadIntoBusinessObject();
		}
	}
}
