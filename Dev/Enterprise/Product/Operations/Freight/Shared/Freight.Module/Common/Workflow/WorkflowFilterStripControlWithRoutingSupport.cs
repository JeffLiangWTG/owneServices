using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Module
{
	public partial class WorkflowFilterStripControlWithRoutingSupport : WorkflowFilterStripControl
	{
		public WorkflowFilterStripControlWithRoutingSupport()
		{
			InitializeComponent();
		}

		public WorkflowFilterStripControlWithRoutingSupport(ZFilterStrip parentStrip)
			: base(parentStrip)
		{
			InitializeComponent();
		}
	}
}
