using System;

namespace Enterprise.MasterFiles.Integration
{
	[Flags]
	public enum OrganisationTypes
	{
		None = 0,
		Debtor = 1 << 0,
		Creditor = 1 << 1,
		Consignor = 1 << 2,
		Consignee = 1 << 3,
		TransportClient = 1 << 4,
		Carrier = 1 << 5,
		Forwarder = 1 << 6,
		Broker = 1 << 7,
		Services = 1 << 8,
		Competitor = 1 << 9,
		Sales = 1 << 10,
		WarehouseClient = 1 << 11,
		DistributionCentre = 1 << 12,
		ControllingAgent = 1 << 13,
		ControllingCustomer = 1 << 14,
	}
}
