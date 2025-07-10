using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class ConsolPrepareForDispatchDataObjectWriter : ConsolDataObjectWriter, IConsolPrepareForDispatchDataObjectWriter
	{
		public ConsolPrepareForDispatchDataObjectWriter(
			IDataWritingManager manager,
			ConsolPrepareForDispatchInstruction consolForPrepareDispatchInstruction
		)
			: base(manager)
		{
			consolForPrepareDispatch = consolForPrepareDispatchInstruction;
		}

		readonly ConsolPrepareForDispatchInstruction consolForPrepareDispatch;

		protected override DataObjectList<UniversalShipment> PopulateSubShipmentCollection(ForwardingConsol consol)
		{
			var selectedShipments = consolForPrepareDispatch
				.ShipmentsForSelection
				.Where(selectedShipment => selectedShipment.SelectedForDelivery && selectedShipment.IsValidToSend)
				.Select(selectedShipment => selectedShipment.Shipment);

			List<UniversalShipment> data = null;
			if (writeManager.Schema == UniversalXmlSchema.Version_2012_11_DO_NOT_USE)
			{
				data = ProcessCollection(selectedShipments, new ShipmentDataObjectWriter(writeManager, linkManager, true, true, consol));
			}
			else
			{
				data = ProcessCollection(selectedShipments, new ShipmentDataObjectWriter(writeManager, linkManager, true, false));
			}

			return data != null ? new DataObjectList<UniversalShipment>(data) : null;
		}
	}
}
