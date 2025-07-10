namespace Enterprise.Freight.Agency.Business
{
	public class SplitBookingsHeaderValidation : AutoSplitBookingsHeaderValidation
	{
		public SplitBookingsHeaderValidation(AutoSplitBookingsHeader parent)
			: base(parent) { }

		#region Implementation

		public new SplitBookingsHeader Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (SplitBookingsHeader)base.Parent; }
		}

		#endregion
	}
}
