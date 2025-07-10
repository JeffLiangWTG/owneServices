namespace Enterprise.Freight.Agency.Business
{
	public class UpdateReturnByApplicatorValidation : AutoUpdateReturnByApplicatorValidation
	{
		public UpdateReturnByApplicatorValidation(AutoUpdateReturnByApplicator parent)
			: base(parent) { }

		#region Implementation

		public new UpdateReturnByApplicator Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (UpdateReturnByApplicator)base.Parent; }
		}

		#endregion
	}
}
