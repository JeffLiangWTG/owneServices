using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class OrderLineEventContextReader
	{
		internal OrderLineEventContextReader(OrderLine orderLine)
		{
			this.orderLine = Argument.NotNull(orderLine, "OrderLine orderLine");
		}
		readonly OrderLine orderLine;

		internal void AddOrderContextValues(List<KeyValuePair<TypeWithDescription, IZType>> contextValues)
		{
			if (orderLine.Order != null)
			{
				var orderReader = new OrderEventContextReader(orderLine.Order);
				orderReader.AddOrderContextValues(contextValues);
			}

			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.OrderLineNumber, orderLine.JO_LineNo);
			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.OrderLineSubLineNumber, orderLine.JO_SubLineNo);
		}
	}
}
