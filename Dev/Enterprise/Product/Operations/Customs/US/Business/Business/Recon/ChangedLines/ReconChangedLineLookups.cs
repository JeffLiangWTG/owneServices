using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class ReconChangedLineLookups : ZLookups
	{
		public ReconChangedLineLookups(ReconChangedLine parent)
			: base(parent) { }

		#region Implementation

		protected new ReconChangedLine Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ReconChangedLine)base.Parent; }
		}

		#endregion
	}
}
