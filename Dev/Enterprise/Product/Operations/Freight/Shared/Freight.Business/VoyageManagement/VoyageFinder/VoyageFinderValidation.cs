namespace Enterprise.Freight.Business
{
	public class VoyageFinderValidation : AutoVoyageFinderValidation
	{
		public VoyageFinderValidation(AutoVoyageFinder parent)
			: base(parent) { }

		#region Implementation

		public new VoyageFinder Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (VoyageFinder)base.Parent; }
		}

		#endregion
	}
}
