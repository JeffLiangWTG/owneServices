namespace Enterprise.Customs.Business
{
	public interface IConsolidatedEntryDeclarationRemover
	{
		bool CanRemove(JobDeclarationConsolidatedEntryProvider provider, BaseJobDeclaration declaration);
		bool RemoveFromConsolidation(BaseJobDeclaration declaration, out string message);
	}
}
