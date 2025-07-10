using Enterprise.DocumentEngine.Module;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Module
{
	public class TransportReports : ZReportModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.TransportReports; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.TransportReports; }
		}
	}
}
