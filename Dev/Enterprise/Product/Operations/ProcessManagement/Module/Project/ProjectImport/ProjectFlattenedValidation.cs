namespace Enterprise.ProcessManagement.Module
{
	public class ProjectFlattenedValidation : AutoProjectFlattenedValidation
	{
		public ProjectFlattenedValidation(AutoProjectFlattened parent)
			: base(parent) { }

		#region Implementation

		public new ProjectFlattened Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ProjectFlattened)base.Parent; }
		}

		#endregion
	}
}
