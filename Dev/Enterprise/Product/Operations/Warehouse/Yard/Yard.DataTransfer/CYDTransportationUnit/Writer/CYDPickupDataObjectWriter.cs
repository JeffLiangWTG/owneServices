using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Yard.Business;
using static Enterprise.Core.Constants.GateManagementConstants;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal;

public class CYDPickupDataObjectWriter(IDataWritingManager writeManager) : DataObjectWriter<CYDPickup, Shipment>(writeManager)
{
	protected override Shipment PopulateDataObject(CYDPickup pickup)
	{
		var subShipment = new Shipment(writeManager.WriterStrategy);

		subShipment.DataContext = DataContextFactory.New();
		subShipment.DataContext.AddDataSource(DataContextType.CYDPickup, pickup.YPL_PickupID);

		subShipment.BookingConfirmationReference = pickup.ReleaseAdviceLine?.ReleaseAdvice.YRE_ReleaseNumber;

		subShipment.TransportBookingDirection = new TransportBookingDirection
		{
			Code = TransportBookingDirections.Codes.Pickup,
			Description = TransportBookingDirections.Descriptions.Pickup
		};

		subShipment.SetContainerCollection(() =>
		{
			var containers = new DataObjectList<Container>();
			var container = new Container(writeManager.WriterStrategy)
			{
				ContainerType = ContainerType.New(pickup.UnitLineItem.ContainerType),
				ContainerNumber = pickup.LinkedYardUnit?.YUS_UnitID,
			};

			containers.Add(container);
			return containers;
		});

		return subShipment;
	}
}
