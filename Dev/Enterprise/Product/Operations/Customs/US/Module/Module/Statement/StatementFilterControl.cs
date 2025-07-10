using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.Module
{
	/// <summary>
	/// Filter control for Statements.
	/// </summary>
	public partial class StatementFilterControl : ZFilterStripControl
	{
		public StatementFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
		}
	}
}
