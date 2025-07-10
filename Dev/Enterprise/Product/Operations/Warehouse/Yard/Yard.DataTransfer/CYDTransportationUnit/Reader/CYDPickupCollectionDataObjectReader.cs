using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Yard.Business;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal
{
	public class CYDPickupCollectionDataObjectReader : DataObjectCollectionReader<Container, CYDPickup>
	{
		public CYDPickupCollectionDataObjectReader(DataObjectList<Container> containers, CYDTransportationUnit transportationUnit, Shipment subShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(containers)
		{
			this.transportationUnit = Argument.NotNull(transportationUnit, nameof(transportationUnit));
			this.subShipment = Argument.NotNull(subShipment, nameof(subShipment));
			this.logger = Argument.NotNull(logger, nameof(logger));
			this.factory = Argument.NotNull(factory, nameof(factory));

			pickupHeader = factory.New<CYDPickupHeader>();
			pickupHeader.YPH_WW_Yard = transportationUnit.YTU_WW_Yard;
			pickupCollection = pickupHeader.PickupCollection;
			bookingReference = ContainerYardUniversalHelper.GetBookingConfirmationReference(subShipment);
			transportReference = ContainerYardUniversalHelper.GetTransportReference(subShipment);
		}

		readonly CYDTransportationUnit transportationUnit;
		readonly Shipment subShipment;
		readonly IXmlImportLogger logger;
		readonly UniversalObjectFactory factory;
		readonly CYDPickupHeader pickupHeader;
		readonly CYDPickupCollection pickupCollection;
		readonly string bookingReference;
		readonly string transportReference;

		protected override CYDPickup[] BusinessObjects => pickupCollection.Cast<CYDPickup>().ToArray();

		protected override void AddToCollection(CYDPickup pickup)
		{
			if (pickup.PickupHeader == null)
			{
				pickupCollection.Add(pickup);
			}
		}

		protected override void RemoveFromCollection(CYDPickup pickup) => pickupCollection.Remove(pickup);

		protected override CYDPickup FindMatchingBusinessObject(Container dataObject)
		{
			return null;
		}

		protected override void ReadIntoCollectionCore()
		{
			base.ReadIntoCollectionCore();
			if (!pickupCollection.Any())
			{
				pickupHeader.Delete();
			}
		}

		protected override CYDPickup ReadIntoBusinessObject(Container container, CYDPickup pickup)
		{
			if (string.IsNullOrEmpty(bookingReference))
			{
				throw new DataObjectReadFailureException(Res.GetString("c07675f5-319f-4c4b-9407-78048b9b02b2", "Booking Confirmation Reference is required in UXML for pickup."));
			}

			var reader = new CYDPickupDataObjectReader(container, bookingReference, transportReference, transportationUnit, subShipment, logger, factory);
			return reader.ReadIntoBusinessObject();
		}
	}
}
