namespace Enterprise.Freight.Integration
{
	public interface IOverrideAllocationRouteDialogProvider
	{
		bool PromptUserForConfirmingOverride(IRatingContractAllocationLine route, IAllocationRouteAssignable routeAssignable);
	}
}
