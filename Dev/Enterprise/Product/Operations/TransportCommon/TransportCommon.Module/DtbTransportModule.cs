using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportCommon.Module
{
	public abstract class DtbTransportModule : ZFilterGridModule
	{
		#region SupportsWorkflow

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.DtbBookingWorkflowDescriptorCode; }
		}

		#endregion
	}
}
