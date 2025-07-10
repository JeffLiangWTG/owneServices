using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Yard.Business;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal
{
	public class CYDDeliveryCollectionDataObjectReader : DataObjectCollectionReader<Container, CYDDelivery>
	{
		public CYDDeliveryCollectionDataObjectReader(DataObjectList<Container> containers, CYDTransportationUnit transportationUnit, Shipment subShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(containers)
		{
			this.transportationUnit = Argument.NotNull(transportationUnit, nameof(transportationUnit));
			this.subShipment = Argument.NotNull(subShipment, nameof(subShipment));
			this.logger = Argument.NotNull(logger, nameof(logger));
			this.factory = Argument.NotNull(factory, nameof(factory));

			deliveryHeader = factory.New<CYDDeliveryHeader>();
			deliveryHeader.YDH_WW_Yard = transportationUnit.YTU_WW_Yard;
			deliveryCollection = deliveryHeader.DeliveryCollection;
			bookingReference = ContainerYardUniversalHelper.GetBookingConfirmationReference(subShipment);
			transportReference = ContainerYardUniversalHelper.GetTransportReference(subShipment);
		}

		readonly CYDTransportationUnit transportationUnit;
		readonly Shipment subShipment;
		readonly IXmlImportLogger logger;
		readonly UniversalObjectFactory factory;
		readonly CYDDeliveryHeader deliveryHeader;
		readonly CYDDeliveryCollection deliveryCollection;
		readonly string bookingReference;
		readonly string transportReference;

		protected override CYDDelivery[] BusinessObjects => deliveryCollection.Cast<CYDDelivery>().ToArray();

		protected override void AddToCollection(CYDDelivery delivery)
		{
			if (delivery.DeliveryHeader == null)
			{
				deliveryCollection.Add(delivery);
			}
		}

		protected override void RemoveFromCollection(CYDDelivery delivery) => deliveryCollection.Remove(delivery);

		protected override CYDDelivery FindMatchingBusinessObject(Container dataObject)
		{
			return null;
		}

		protected override void ReadIntoCollectionCore()
		{
			base.ReadIntoCollectionCore();
			if (!deliveryCollection.Any())
			{
				deliveryHeader.Delete();
			}
		}

		protected override CYDDelivery ReadIntoBusinessObject(Container container, CYDDelivery delivery)
		{
			var reader = new CYDDeliveryDataObjectReader(container, bookingReference, transportReference, transportationUnit, subShipment, logger, factory);
			return reader.ReadIntoBusinessObject();
		}
	}
}
