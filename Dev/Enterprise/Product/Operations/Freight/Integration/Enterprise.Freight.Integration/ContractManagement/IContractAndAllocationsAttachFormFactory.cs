namespace Enterprise.Freight.Integration
{
	public interface IContractAndAllocationsAttachFormFactory
	{
		// Returns IZForm
		// IZForm is not defined in an integration solution and moving it would be a large refactor, so object is used
		object CreateForm(IContractSimulationFormConfiguration formConfiguration);
	}
}
