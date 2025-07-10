using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WhsReceiveDataObjectWriter : WhsOrderAndReceiveDataObjectWriter<WhsReceive>
	{
		internal WhsReceiveDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		#region PopulateDataObject

		protected override void PopulateDataObject(WhsReceive whsReceiveBO, UniversalShipment shipmentDataObject)
		{
			base.PopulateDataObject(whsReceiveBO, shipmentDataObject);

			shipmentDataObject.SetContainerCollection(() => ProcessCollection(whsReceiveBO.Containers, new WhsDocketContainerDataObjectWriter(writeManager), CollectionContent.Complete));
			shipmentDataObject.ContainerMode = GetContainerMode(shipmentDataObject);

			var orderDataObject = shipmentDataObject.Order;

			orderDataObject.Category = whsReceiveBO.WD_ReceiveCategory;

			if (orderDataObject.SetDateCollection(() => new List<Date>()))
			{
				orderDataObject.DateCollection.Add(Date.New(DateType.BookingConfirmed, ZBool.False, whsReceiveBO.WD_BookingDate.ToZDateTime()));
				orderDataObject.DateCollection.Add(Date.New(DateType.Departure, ZBool.True, whsReceiveBO.WD_ETD.ToZDateTime()));
				orderDataObject.DateCollection.Add(Date.New(DateType.Arrival, ZBool.True, whsReceiveBO.WD_ETA.ToZDateTime()));
				orderDataObject.DateCollection.Add(Date.New(DateType.Arrival, ZBool.False, whsReceiveBO.WD_ArrivalDate.ToZDateTime()));
			}

			orderDataObject.HoldPalletIDPutaway = whsReceiveBO.WD_HoldPalletIDPutaway;
			orderDataObject.SetOrderLineCollection(() => ProcessCollection(whsReceiveBO.Lines, new WhsReceiveLineDataObjectWriter(writeManager), CollectionContent.Complete));

			var warehouse = whsReceiveBO.Warehouse;
			if (warehouse != null)
			{
				shipmentDataObject.AddOrgAddress(writeManager, warehouse.WarehouseAddress, DocAddressType.CustomsWarehouseAddress);
			}

			var inboundDockDoorLocation = whsReceiveBO.InboundDockDoor;
			if (inboundDockDoorLocation != null)
			{
				orderDataObject.StagingArea = inboundDockDoorLocation.ToLocationString();
			}
		}

		protected override ZShort GetPallets(WhsReceive whsReceiveBO) => whsReceiveBO.WD_TotalPallets;

		protected override bool PopulateOuterPacksQty(WhsReceive whsReceiveBO) => true;

		#endregion

		#region GetContainerMode

		ContainerMode GetContainerMode(UniversalShipment shipmentDataObject)
		{
			ContainerMode mode;

			if (shipmentDataObject.ContainerCollection != null)
			{
				mode = new ContainerMode() { Code = Core.Constants.ContainerModes.FCL, Description = Core.Constants.ContainerModeDescriptions.FCL };
			}
			else
			{
				mode = new ContainerMode() { Code = Core.Constants.ContainerModes.Loose, Description = Core.Constants.ContainerModeDescriptions.Loose };
			}

			return mode;
		}

		#endregion

		#region GetTopLevelDataContextType

		protected override DataContextType GetTopLevelDataContextType() => DataContextType.WarehouseReceive;

		#endregion
	}
}
