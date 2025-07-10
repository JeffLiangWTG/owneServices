namespace Enterprise.Freight.Forwarding.Business
{
	public interface IAllocationContainerWeightLimitDialogsProvider
	{
		bool ConfirmContainerWeightLimitOverride(string message);
		bool AllowContainerWeightLimitOverrideBySecurityRole(string message);
	}
}
