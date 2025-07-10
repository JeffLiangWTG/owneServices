using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class CommonShipmentEventContextReader
	{
		public CommonShipmentEventContextReader(CommonShipment shipment, IUniversalFreightHelper freightHelper)
		{
			this.shipment = Argument.NotNull(shipment, "CommonShipment shipment");
			this.freightHelper = Argument.NotNull(freightHelper, "freightHelper");
		}

		readonly CommonShipment shipment;
		readonly IUniversalFreightHelper freightHelper;

		public void AddShipmentContextValues(List<KeyValuePair<TypeWithDescription, IZType>> contextValues)
		{
			var helper = new EventContextValuesHelper(shipment.JS_TransportMode == Constants.TransportModes.Air, contextValues);

			var consol = freightHelper.GetRelatedConsolForContextValues(shipment);
			if (consol != null)
			{
				var consolEventContextReader = new CommonConsolEventContextReader(consol);
				consolEventContextReader.AddConsolContextValues(contextValues);
			}

			helper.AddHouseBillNumberAndPortCodes(shipment.JS_HouseBill, shipment.Origin, shipment.Destination);

			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.ShippersReference, shipment.JS_BookingReference);

			if (freightHelper.ShipmentHasOrders)
			{
				foreach (OrderItem orderNumber in shipment.DocsAndCartage.OrderItems)
				{
					contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.OrderNumber, orderNumber.JT_OrderReference);
				}
			}

			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.InterimReceipt, shipment.JS_InterimReceipt);

			shipment.Numbers.AddAdditionalReferences(contextValues);
		}
	}
}
