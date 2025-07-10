namespace Enterprise.MasterFiles.Business
{
	public enum TriggerApplicationType
	{
		ApplyOnce,
		AlwaysApply,
	}

	public static class TriggerApplicationTypeHelper
	{
		public static TriggerApplicationType GetTriggerApplicationType(string actionType)
		{
			switch (actionType)
			{
				case WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateAlways:
					return TriggerApplicationType.AlwaysApply;
				case WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce:
				default:
					return TriggerApplicationType.ApplyOnce;
			}
		}
	}
}
