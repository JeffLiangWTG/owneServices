namespace Enterprise.Freight.Integration
{
	public interface IContractPermissions
	{
		bool IsCarrierAndClientContractModulesEnabled();
		bool IsAllocationsVisible();
		bool IsTariffAndRatesVisible();
	}
}
