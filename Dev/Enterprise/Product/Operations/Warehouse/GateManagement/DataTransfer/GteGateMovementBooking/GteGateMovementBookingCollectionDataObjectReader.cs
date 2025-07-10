using CargoWise.Common;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.GateManagement.Business;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	class GteGateMovementBookingCollectionDataObjectReader : DataObjectCollectionReader<UniversalShipment, GteGateMovementBooking>
	{
		public GteGateMovementBookingCollectionDataObjectReader(GteBooking booking, IXmlImportLogger logger, UniversalObjectFactory factory, UniversalShipment[] gateMovementBookingDataObjectCollection)
			: base(gateMovementBookingDataObjectCollection)
		{
			this.logger = Argument.NotNull(logger, nameof(logger));
			this.factory = Argument.NotNull(factory, nameof(factory));

			this.booking = Argument.NotNull(booking, nameof(booking));
			gateMovementBookingCollection = Argument.NotNull(booking.GateMovementBookings, nameof(booking.GateMovementBookings));
		}

		readonly GteBooking booking;
		readonly GteGateMovementBookingCollection gateMovementBookingCollection;
		readonly IXmlImportLogger logger;
		readonly UniversalObjectFactory factory;

		protected override GteGateMovementBooking[] BusinessObjects => gateMovementBookingCollection.ToArray();

		protected override void AddToCollection(GteGateMovementBooking gateMovementBooking) => gateMovementBookingCollection.Add(gateMovementBooking);

		protected override void RemoveFromCollection(GteGateMovementBooking businessObject) => gateMovementBookingCollection.Delete(businessObject);

		protected override GteGateMovementBooking FindMatchingBusinessObject(UniversalShipment dataObject)
		{
			return null;
		}

		protected override GteGateMovementBooking ReadIntoBusinessObject(UniversalShipment dataObject, GteGateMovementBooking gateMovemement)
		{
			var reader = new GteGateMovementBookingDataObjectReader(booking, dataObject, logger, factory);
			return reader.ReadIntoBusinessObject();
		}
	}
}
