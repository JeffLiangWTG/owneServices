using Enterprise.DocumentEngine.Module;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.Module
{
	public class CFSCTOReports : ZReportModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.CFSCTOReports; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CFSReports; }
		}
	}
}
