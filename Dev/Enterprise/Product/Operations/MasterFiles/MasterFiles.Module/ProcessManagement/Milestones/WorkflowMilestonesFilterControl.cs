using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class WorkflowMilestonesFilterControl : ZFilterStripControl
	{
		public WorkflowMilestonesFilterControl()
		{
			InitializeComponent();
		}

		public WorkflowMilestonesFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
