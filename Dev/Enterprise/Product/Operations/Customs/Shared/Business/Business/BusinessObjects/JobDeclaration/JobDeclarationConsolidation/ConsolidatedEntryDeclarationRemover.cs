namespace Enterprise.Customs.Business
{
	public class ConsolidatedEntryDeclarationRemover : IConsolidatedEntryDeclarationRemover
	{
		bool IConsolidatedEntryDeclarationRemover.CanRemove(JobDeclarationConsolidatedEntryProvider provider,
			BaseJobDeclaration declaration)
			=> provider.IsConsolidated && !declaration.ActiveEntryHeaders.HasAnEntryWithEntryNumber;

		bool IConsolidatedEntryDeclarationRemover.RemoveFromConsolidation(BaseJobDeclaration declaration, out string message)
		{
			var consolidatedDeclaration = ConsolidatedDeclaration.GetConsolidatedDeclaration(declaration);
			if (consolidatedDeclaration.JobDeclarations.Count == 1)
			{
				message = Res.GetString("E4646E6F-D61F-4726-8996-A0D5B2E9C91B", "A Consolidation requires at least 1 declaration, you cannot remove the last one.");
				return false;
			}

			consolidatedDeclaration.JobDeclarations.RemoveFromRelationship(declaration);
			consolidatedDeclaration.CalculateHeaderFees();
			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();

			if (!declaration.HasMessageInitiator)
			{
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			}

			declaration.DoMerge();
			message = Res.GetString("9F3E50E7-6E79-41EE-9B9B-0727360AE6D0", "Job is no longer in Consolidation.");
			return true;
		}
	}
}
