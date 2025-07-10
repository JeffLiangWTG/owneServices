using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class WorkflowExceptionsFilterControl : ZFilterStripControl
	{
		public WorkflowExceptionsFilterControl()
		{
			InitializeComponent();
		}

		public WorkflowExceptionsFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
