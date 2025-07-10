using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.GateManagement.Business;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	public class GteVehicleEntryCollectionDataObjectReader : DataObjectCollectionReader<UniversalShipment, GteVehicleEntry>
	{
		public GteVehicleEntryCollectionDataObjectReader(GteVehicleMovement vehicleMovement, GteBooking booking, IXmlImportLogger logger, UniversalObjectFactory factory, UniversalShipment[] dataObjects) : base(dataObjects)
		{
			this.vehicleMovement = Argument.NotNull(vehicleMovement, nameof(vehicleMovement));
			this.booking = Argument.NotNull(booking, nameof(booking));
			this.logger = Argument.NotNull(logger, nameof(logger));
			this.factory = Argument.NotNull(factory, nameof(factory));
			vehicleEntryCollection = Argument.NotNull(vehicleMovement.VehicleEntries, nameof(vehicleMovement.VehicleEntries));
		}

		readonly GteVehicleMovement vehicleMovement;
		readonly GteBooking booking;
		readonly GteVehicleEntryCollection vehicleEntryCollection;
		readonly IXmlImportLogger logger;
		readonly UniversalObjectFactory factory;

		protected override GteVehicleEntry[] BusinessObjects => vehicleEntryCollection.Cast<GteVehicleEntry>().ToArray();

		protected override void AddToCollection(GteVehicleEntry businessObject) => vehicleEntryCollection.Add(businessObject);

		protected override void RemoveFromCollection(GteVehicleEntry businessObject)
		{
			vehicleEntryCollection.RemoveFromRelationship(businessObject);
			vehicleEntryCollection.Delete(businessObject);
		}

		protected override GteVehicleEntry FindMatchingBusinessObject(UniversalShipment dataObject) => null;

		protected override GteVehicleEntry ReadIntoBusinessObject(UniversalShipment dataObject, GteVehicleEntry businessObject)
		{
			var reader = new GteVehicleEntryDataObjectReader(vehicleMovement, booking, dataObject, logger, factory);
			return reader.ReadIntoBusinessObject();
		}
	}
}
