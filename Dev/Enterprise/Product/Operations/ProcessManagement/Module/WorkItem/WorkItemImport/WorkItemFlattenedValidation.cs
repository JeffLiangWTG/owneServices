namespace Enterprise.ProcessManagement.Module
{
	public class WorkItemFlattenedValidation : AutoWorkItemFlattenedValidation
	{
		public WorkItemFlattenedValidation(AutoWorkItemFlattened parent)
			: base(parent) { }

		#region Implementation

		public new WorkItemFlattened Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (WorkItemFlattened)base.Parent; }
		}

		#endregion
	}
}
