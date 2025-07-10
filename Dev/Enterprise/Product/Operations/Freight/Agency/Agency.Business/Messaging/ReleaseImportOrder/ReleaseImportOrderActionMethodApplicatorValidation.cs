namespace Enterprise.Freight.Agency.Business
{
	public class ReleaseImportOrderActionMethodApplicatorValidation : AutoReleaseImportOrderActionMethodApplicatorValidation
	{
		public ReleaseImportOrderActionMethodApplicatorValidation(AutoReleaseImportOrderActionMethodApplicator parent)
			: base(parent) { }

		#region Implementation

		public new ReleaseImportOrderActionMethodApplicator Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ReleaseImportOrderActionMethodApplicator)base.Parent; }
		}

		#endregion
	}
}
