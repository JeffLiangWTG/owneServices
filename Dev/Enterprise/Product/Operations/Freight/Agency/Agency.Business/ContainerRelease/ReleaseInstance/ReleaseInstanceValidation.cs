namespace Enterprise.Freight.Agency.Business
{
	public class ReleaseInstanceValidation : AutoReleaseInstanceValidation
	{
		public ReleaseInstanceValidation(AutoReleaseInstance parent)
			: base(parent) { }

		#region Implementation

		public new ReleaseInstance Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ReleaseInstance)base.Parent; }
		}

		#endregion
	}
}
