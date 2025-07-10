using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Yard.Business;
using static Enterprise.Core.Constants.GateManagementConstants;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal;

public class CYDDeliveryDataObjectWriter(IDataWritingManager writeManager) : DataObjectWriter<CYDDelivery, Shipment>(writeManager)
{
	protected override Shipment PopulateDataObject(CYDDelivery delivery)
	{
		var subShipment = new Shipment(writeManager.WriterStrategy);

		subShipment.DataContext = DataContextFactory.New();
		subShipment.DataContext.AddDataSource(DataContextType.CYDDelivery, delivery.YDL_DeliveryID);

		subShipment.BookingConfirmationReference = delivery.ReceiveAdviceLine?.ReceiveAdvice.YRA_AcceptanceNumber;

		subShipment.TransportBookingDirection = new TransportBookingDirection
		{
			Code = TransportBookingDirections.Codes.Delivery,
			Description = TransportBookingDirections.Descriptions.Delivery
		};

		subShipment.SetContainerCollection(() =>
		{
			var containers = new DataObjectList<Container>();
			var container = new Container(writeManager.WriterStrategy)
			{
				ContainerType = ContainerType.New(delivery.UnitLineItem.ContainerType),
				ContainerNumber = delivery.LinkedYardUnit?.YUS_UnitID,
			};

			containers.Add(container);
			return containers;
		});

		return subShipment;
	}
}
