using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalXml = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class HVLVShipmentCollectionDataObjectReader : DataObjectCollectionReader<UniversalXml.Shipment, Shipment>
	{
		public HVLVShipmentCollectionDataObjectReader(Trip trip, UniversalXml.Shipment consolLevelDataObject, UniversalXml.Shipment shipmentLevelDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, UniversalXml.Shipment[] shipments)
			: base(shipments)
		{
			this.trip = Argument.NotNull(trip, nameof(trip));
			this.shipmentLevelDataObject = Argument.NotNull(shipmentLevelDataObject, nameof(shipmentLevelDataObject));
			this.consolLevelDataObject = Argument.NotNull(consolLevelDataObject, nameof(consolLevelDataObject));
			this.logger = Argument.NotNull(logger, nameof(logger));
			this.factory = Argument.NotNull(factory, nameof(factory));
		}

		readonly Trip trip;
		readonly UniversalXml.Shipment shipmentLevelDataObject;
		readonly UniversalXml.Shipment consolLevelDataObject;
		readonly IXmlImportLogger logger;
		readonly UniversalObjectFactory factory;

		protected override Shipment[] BusinessObjects
		{
			get
			{
				var query = new ZQuery(CusInBondBillSchema.B0_BH, trip.PK);
				return factory.Load<Shipment>(query);
			}
		}

		protected override void AddToCollection(Shipment shipment) { shipment.B0_BH = trip.PK; }

		protected override void RemoveFromCollection(Shipment shipment) => trip.Shipments.Delete(shipment);

		protected override Shipment FindMatchingBusinessObject(UniversalXml.Shipment dataObject)
		{
			if (dataObject.WayBillNumber.HasValue)
			{
				var waybillNumber = dataObject.WayBillNumber.Value;
				return trip.GetShipmentFromLookup(waybillNumber);
			}

			return null;
		}

		protected override Shipment ReadIntoBusinessObject(UniversalXml.Shipment dataObject, Shipment shipmentBO)
		{
			var reader = new HVLVShipmentDataObjectReader(dataObject, trip, shipmentLevelDataObject, consolLevelDataObject, logger, factory);
			return reader.ReadIntoBusinessObject();
		}
	}
}
