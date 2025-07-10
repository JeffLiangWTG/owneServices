namespace Enterprise.MasterFiles.GUI
{
	public interface IOpportunityViewControllerProvider
	{
		IOpportunityViewController CreateController(OpportunityForm opportunityView);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface IOpportunityViewController
	{
	}
}
