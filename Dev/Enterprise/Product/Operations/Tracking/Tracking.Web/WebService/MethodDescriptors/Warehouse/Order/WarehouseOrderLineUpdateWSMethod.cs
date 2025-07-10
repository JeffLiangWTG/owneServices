using System;
using System.Linq;
using System.Web;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Tracking.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.Tracking.Web.ServerServices
{
	public class WarehouseOrderLineUpdateWSMethod : WarehouseDocketLineUpdateWSMethod<WarehouseOrderLineUpdateParameters>
	{
		class OrderLineValues : DocketLineValues
		{
			public ZDecimal ShortfallQuantity;
		}

		protected override DocketLineValues GetNewDocketLineValues() => new OrderLineValues();

		protected override DocketLineValues GetDocketLineValues(WhsDocketLine line)
		{
			var values = (OrderLineValues)base.GetDocketLineValues(line);
			var orderLine = (WhsOrderLine)line;
			values.ShortfallQuantity = orderLine.WE_ShortfallQuantityCached;

			return values;
		}

		protected override void GenerateResponse(DocketLineValues initialValues, DocketLineValues updatedValues, WarehouseOrderLineUpdateParameters parameters, WhsDocketLine line, WebServiceResponse response)
		{
			base.GenerateResponse(initialValues, updatedValues, parameters, line, response);

			var orderLine = (WhsOrderLine)line;
			var initialOrderLineValues = (OrderLineValues)initialValues;
			var updatedOrderLineValues = (OrderLineValues)updatedValues;

			if (initialOrderLineValues.Product != updatedOrderLineValues.Product
				|| initialOrderLineValues.Packs != updatedOrderLineValues.Packs
				|| initialOrderLineValues.Quantity != updatedOrderLineValues.Quantity)
			{
				orderLine.ClearWE_ShortfallQuantityCached();
			}

			updatedOrderLineValues = (OrderLineValues)GetDocketLineValues(orderLine);
			AddUpdateToken(response, parameters, parameters.ShortfallControlID, initialOrderLineValues.ShortfallQuantity, updatedOrderLineValues.ShortfallQuantity);
		}

		protected override WhsDocketLine GetDocketLine(WarehouseOrderLineUpdateParameters parameters)
		{
			var session = HttpContext.Current?.Session;
			if (session != null && Guid.TryParse(parameters.LineRef, out Guid linePK))
			{
				var order = session[parameters.DocketRef] as TrackingWhsOrder;
				return order?.Lines.Where(l => ((BusinessObject)l).PK == linePK).OfType<TrackingWhsOrderLine>().FirstOrDefault()?.WhsOrderLine;
			}

			return null;
		}

		protected override string GetMethodName()
		{
			return "WarehouseOrderLineUpdate";
		}

		protected override string GetScriptFileName()
		{
			return "WarehouseOrderLineUpdateWSMethod.js";
		}
	}
}
