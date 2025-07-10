using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class RepititionSelectionLookups : ZLookups
	{
		public RepititionSelectionLookups(RepititionSelection parent)
			: base(parent) { }

		#region Implementation

		protected new RepititionSelection Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (RepititionSelection)base.Parent; }
		}

		#endregion
	}
}
