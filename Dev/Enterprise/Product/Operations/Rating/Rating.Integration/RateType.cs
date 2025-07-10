using System;

namespace Enterprise.Rating.Integration
{
	[Flags]
	public enum RateType
	{
		Forwarding = 1,
		CFS = 2,
		Warehouse = 4,
		TransportBookings = 8,
		Shipping = 16,
		ShippingImportDetention = 32,
		ShippingExportDetention = 64,
		LocalTransport = 128,
		ContainerYard = 256,
		TransitWarehouse = 512,
		Customs = 1024,
		TransitWarehouseTransportationUnit = 2048,
		ContainerYardTransportationUnit = 4096,
	}
}
