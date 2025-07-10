using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.GateManagement.Business;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	class GteVehicleDriverBookingCollectionDataObjectReader : DataObjectCollectionReader<Crew, GteVehicleDriverBooking>
	{
		public GteVehicleDriverBookingCollectionDataObjectReader(GteBooking booking, UniversalShipment parentDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, Crew[] vehicleDriverBookingDataObjectCollection)
			: base(vehicleDriverBookingDataObjectCollection)
		{
			this.logger = Argument.NotNull(logger, nameof(logger));
			this.factory = Argument.NotNull(factory, nameof(factory));
			this.parentDataObject = Argument.NotNull(parentDataObject, nameof(parentDataObject));

			vehicleDriverBookingCollection = Argument.NotNull(booking.VehicleDriverBookings, nameof(booking.VehicleDriverBookings));
		}

		readonly GteVehicleDriverBookingCollection vehicleDriverBookingCollection;
		readonly UniversalShipment parentDataObject;
		readonly IXmlImportLogger logger;
		readonly UniversalObjectFactory factory;

		protected override GteVehicleDriverBooking[] BusinessObjects => vehicleDriverBookingCollection.Cast<GteVehicleDriverBooking>().ToArray();

		protected override void AddToCollection(GteVehicleDriverBooking vehicleDriverBooking) => vehicleDriverBookingCollection.Add(vehicleDriverBooking);

		protected override void RemoveFromCollection(GteVehicleDriverBooking businessObject) => vehicleDriverBookingCollection.RemoveAndDelete(businessObject);

		protected override GteVehicleDriverBooking FindMatchingBusinessObject(Crew dataObject)
		{
			return null;
		}

		protected override GteVehicleDriverBooking ReadIntoBusinessObject(Crew dataObject, GteVehicleDriverBooking vehicleDriverBooking)
		{
			var reader = new GteVehicleDriverBookingDataObjectReader(dataObject, parentDataObject, logger, factory);
			return reader.ReadIntoBusinessObject();
		}
	}
}
