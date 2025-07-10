using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Testing.Orders
{
	class PlannedShipmentValidationTestHelper(OrderManagerMockDataBuilder builder)
	{
		public void BuildCYContainerLoadListWithPlannedShipment(
			string supplierBookingKey,
			string containerLoadListKey,
			string containerLoadListLineKey,
			string plannedShipmentKey,
			string plannedConsolKey,
			string allocatedConsolKey,
			string allocatedContainerKey)
		{
			var orderKey = $"JD_{supplierBookingKey}";
			var supplierBookingLineKey = $"JSL_{containerLoadListLineKey}";
			var orderLineKey = $"JO_{containerLoadListLineKey}";
			builder.BuildOrder(orderKey);
			builder.BuildOrderLine(orderLineKey, orderKey);
			builder.BuildSupplierBooking(supplierBookingKey, SupplierBookingStatus.Approved);
			builder.BuildContainerLoadList(containerLoadListKey, supplierBookingKey, "BKPartyOrg", ContainerLoadListHeaderStatus.Shipped);
			builder.BuildConsol(allocatedConsolKey);
			builder.BuildContainer(allocatedContainerKey, allocatedConsolKey, supplierBookingKey, containerLoadPlanKey: null);
			var bookingLine = builder.BuildSupplierBookingLine(supplierBookingLineKey, supplierBookingKey, null, orderLineKey);
			builder.BuildContainerLoadListLine(containerLoadListLineKey, containerLoadListKey, supplierBookingLineKey, allocatedContainerKey);

			if (plannedShipmentKey != null)
			{
				var shipment = builder.BuildShipment(plannedShipmentKey);
				if (plannedConsolKey != null)
				{
					shipment.Consols.Add(builder.BuildConsol(plannedConsolKey));
				}
				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_JSL_BookingLine = bookingLine.PK;
			}
		}

		readonly OrderManagerMockDataBuilder builder = builder;
	}
}
