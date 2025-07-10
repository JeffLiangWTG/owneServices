using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class OrderEventContextReader
	{
		internal OrderEventContextReader(Order order)
		{
			this.order = Argument.NotNull(order, "Order order");
		}

		readonly Order order;

		internal void AddOrderContextValues(List<KeyValuePair<TypeWithDescription, IZType>> contextValues)
		{
			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.OrderNumber, order.JD_OrderNumber);

			var isAir = order.JD_TransportMode == Constants.TransportModes.Air;
			if (isAir)
			{
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.MAWBNumber, order.JD_MasterWaybill);
			}
			else
			{
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.MBOLNumber, order.JD_MasterWaybill);
			}

			var helper = new EventContextValuesHelper(isAir, contextValues);
			helper.AddHouseBillNumberAndPortCodes(order.JD_Waybill, order.GoodsAvailableAt, order.GoodsDeliveredTo);

			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.CommercialInvoiceNumber, order.JD_InvoiceNumber);
			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.ShippersReference, order.JD_BookingConfRef);
		}
	}
}
