using Enterprise.Freight.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	public class JobVoyageWorkflowUserControl : ZWorkflowUserControl
	{
		protected override GetStmALogFilterStripBusinessObject GetStmALogFilterStripBusinessObject
			=> (parent) => new JobVoyageLogFilterBusinessObject(parent as JobVoyage);
	}
}
