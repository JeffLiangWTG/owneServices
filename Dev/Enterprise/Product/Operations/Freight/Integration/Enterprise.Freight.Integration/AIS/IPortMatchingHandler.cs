namespace Enterprise.Freight.Integration
{
	public interface IPortMatchingHandler
	{
		bool IsUpdateRequired { get; }

		void TryMatchFirstArrivalAndLastForeignPorts();
	}
}
