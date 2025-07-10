using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class ForwardingShipmentWorkflowUserControl : ZWorkflowUserControl
	{
		protected override GetStmALogFilterStripBusinessObject GetStmALogFilterStripBusinessObject
			=> (parent) => new ForwardingShipmentLogFilterBusinessObject(parent);
	}
}
