namespace Enterprise.MasterFiles.Module.Organisation.OrgImport
{
	public class OrgFlattenedValidation : AutoOrgFlattenedValidation
	{
		public OrgFlattenedValidation(AutoOrgFlattened parent)
			: base(parent) { }

		#region Implementation

		public new OrgFlattened Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (OrgFlattened)base.Parent; }
		}

		#endregion
	}
}
