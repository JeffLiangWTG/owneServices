using Enterprise.DocumentEngine.Module;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Module
{
	public class ForwardingReportsModule : ZReportModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ForwardingReport; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ForwardingReport; }
		}
	}
}
