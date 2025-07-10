using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class WorkflowTriggersFilterControl : ZFilterStripControl
	{
		public WorkflowTriggersFilterControl()
		{
			InitializeComponent();
		}

		public WorkflowTriggersFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
