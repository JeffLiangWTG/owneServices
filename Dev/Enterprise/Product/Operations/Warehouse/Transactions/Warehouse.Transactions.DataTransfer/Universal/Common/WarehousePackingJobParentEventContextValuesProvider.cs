using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WarehousePackingJobParentEventContextValuesProvider : IWarehousePackingJobParentEventContextValueProvider
	{
		public IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetAdditionalEventContextValues(IWhsOrder order)
		{
			var contextValues = new List<KeyValuePair<TypeWithDescription, IZType>>();

			var orderBO = (WhsOrder)order;
			if (orderBO != null && orderBO.Pick != null)
			{
				var pick = orderBO.Pick;
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.OrderNumber, orderBO.WD_DocketID);
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.PickNumber, pick.WP_PickNo);
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.TransportReference, order.WD_TransportReference);
			}

			return contextValues;
		}
	}
}