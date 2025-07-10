namespace Enterprise.Freight.Integration
{
	public interface IContractSimulationFormConfiguration
	{
		IRatingContractSimulationFormActions FormActions { get; }
		IRatingContractSimulationFilterDefaults FilterDefaults { get; }
		IRatingContractSimulationQuantityProvider QuantityProvider { get; }
		IRatingContractSimulationNotificationProvider NotificationProvider { get; }
	}
}
