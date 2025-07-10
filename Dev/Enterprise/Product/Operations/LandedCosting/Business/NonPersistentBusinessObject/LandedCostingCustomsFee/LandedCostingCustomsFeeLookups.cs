using CargoWise.EntityFramework;

namespace Enterprise.LandedCosting.Business
{
	public class LandedCostingCustomsFeeLookups : ZLookups
	{
		public LandedCostingCustomsFeeLookups(LandedCostingCustomsFee parent)
			: base(parent) { }

		#region Implementation

		protected new LandedCostingCustomsFee Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (LandedCostingCustomsFee)base.Parent; }
		}

		#endregion
	}
}
