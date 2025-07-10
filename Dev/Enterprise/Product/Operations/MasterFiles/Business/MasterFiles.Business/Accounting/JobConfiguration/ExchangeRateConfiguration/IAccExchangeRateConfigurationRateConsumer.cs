using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IAccExchangeRateConfigurationRateConsumer
	{
		ZString JobType { get; }
		ZString Direction { get; }
		ZString TransportMode { get; }
		ZGuid LocalClientPK { get; }
		GlbCompany Company { get; }
		ZDateTime ConsolExchangeRateDate { get; }
		ZDateTime HistoricalRateFromActualArrivalDate { get; }
		ZDateTime HistoricalRateFromActualDepartureDate { get; }
		ZDateTime HistoricalRateFromEstimatedArrivalDate { get; }
		ZDateTime HistoricalRateFromEstimatedDepartureDate { get; }
		ZDateTime HistoricalRateFromEstimatedArrivalAtLoadPortDate { get; }
		ZDateTime HistoricalRateFromActualArrivalAtLoadPortDate { get; }
		ZDateTime PickupDate { get; }
		ZDateTime DeliveryDate { get; }
		ZDateTime RequiredDate { get; }
		ZDateTime FinalizedDate { get; }
		ZDateTime HouseBillIssueDate { get; }
		ZDateTime ShipmentOrBrokeragePickupDate { get; }
		ZDateTime ShipmentOrBrokerageDeliveryDate { get; }
		ZDecimal GetExchangeRateForConsolExchangeRatePreference(RefCurrency currency);
	}
}
