namespace Enterprise.Freight.Agency.Business
{
	public class BulkDetentionChildValidation : AutoBulkDetentionChildValidation
	{
		public BulkDetentionChildValidation(AutoBulkDetentionChild parent)
			: base(parent) { }

		#region Implementation

		public new BulkDetentionChild Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (BulkDetentionChild)base.Parent; }
		}

		#endregion
	}
}
