namespace Enterprise.MasterFiles.Business
{
	static class ProcessTaskCollectionValidationCaching
	{
		internal static BusinessObjectCollectionValidationCache<ProcessTask> GetValidationCache(ProcessTaskCollection processTaskCollection)
		{
			return new BusinessObjectCollectionValidationCache<ProcessTask>(processTaskCollection,
				TaskAssignmentRestrictionValidationRule.GetRule(processTaskCollection.WorkflowType));
		}
	}
}
