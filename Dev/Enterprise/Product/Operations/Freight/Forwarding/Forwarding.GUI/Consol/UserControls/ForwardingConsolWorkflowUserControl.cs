using Enterprise.Environment;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.Consol.UserControls
{
	public class ForwardingConsolWorkflowUserControl : ZWorkflowUserControl
	{
		protected override GetStmALogFilterStripBusinessObject GetStmALogFilterStripBusinessObject
			=> (parent) => Env.CurrentUser.IsSupportUser ? new ZStmALogFilterBusinessObject(parent) : new ForwardingConsolLogFilterBusinessObject(parent);
	}
}
